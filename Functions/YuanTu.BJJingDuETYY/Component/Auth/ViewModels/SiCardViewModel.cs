using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prism.Regions;
using YuanTu.Consts.Services;
using YuanTu.Core.FrameworkBase.Loadings;
using YuanTu.Consts.Gateway;
using YuanTu.Consts;
using YuanTu.Consts.Models;
using YuanTu.Consts.Enums;
using YuanTu.Core.Log;
using YuanTu.BJArea.BeiJingSiPay;
using YuanTu.Devices;
using YuanTu.Devices.CardReader;
using Microsoft.Practices.Unity;
using YuanTu.Core.Services.LightBar;
using YuanTu.Core.Reporter;
using System.Threading;
using YuanTu.Default.Tools;

namespace YuanTu.BJJingDuETYY.Component.Auth.ViewModels
{
    public class SiCardViewModel : Default.Component.Auth.ViewModels.SiCardViewModel
    {
        [Dependency]
        public ILightBarService LightBarService { get; set; }

        [Dependency]
        public ISiModel SiModel { get; set; }
        public SiCardViewModel(IIcCardReader[] icCardReaders, IRFCpuCardReader[] rfCpuCardReaders) : base(icCardReaders, rfCpuCardReaders)
        {
            _rfCpuCardReader = rfCpuCardReaders?.FirstOrDefault(p => p.DeviceId == "ACT_A6_RFIC2");
        }

        protected string cardNo, name;
        protected Action<LoadingProcesser> myAction;

        protected bool _working;
        protected bool _isA6HuaDa;

        public override void OnSet()
        {
            base.OnSet();
            BackUri = ResourceEngine.GetImageResourceUri("社保卡口");
            CardUri = ResourceEngine.GetImageResourceUri("社保卡素材");
        }

        public override void OnEntered(NavigationContext navigationContext)
        {
            base.OnEntered(navigationContext);
            LightBarService?.PowerOn(LightItem.就诊卡社保卡);

            var config = GetInstance<IConfigurationManager>();
            _isA6HuaDa = (config.GetValue("A6_HuaDa") ?? "0") == "1";

            if (_isA6HuaDa)
            {
                StartRead();
                IsShowConfirm = false;
                ContentMsg = "请在左侧插卡口插入社保卡";
            }
            else
            {
                IsShowConfirm = true;
                ContentMsg = "请在读卡器黄灯亮起后点击确定读卡";
            }
            myAction = new Action<LoadingProcesser>(Command);
        }

        protected virtual void StartRead()
        {
            Task.Run(() => StartA6());
        }
        protected virtual void StopRead()
        {
            StopA6();
        }

        protected virtual void StopA6()
        {
            _rfCpuCardReader?.UnInitialize();
            _rfCpuCardReader?.DisConnect();
        }

        protected virtual void StartA6()
        {
            try
            {
                var ret = _rfCpuCardReader.Connect();
                if (!ret.IsSuccess)
                {
                    ReportService.读卡器离线(null, ErrorSolution.读卡器离线);
                    ShowAlert(false, "友好提示", $"读卡器打开失败({ret.ResultCode})", debugInfo: ret.Message);
                    return;
                }
                _rfCpuCardReader.MoveCard(CardPos.不持卡位); //退卡

                if (!_rfCpuCardReader.Initialize().IsSuccess)
                {
                    ShowAlert(false, "友好提示", $"读卡器初始化失败({ret.ResultCode})", debugInfo: ret.Message);
                    return;
                }

                StartAdjustCardPos();
            }
            catch (Exception ex)
            {
                ShowAlert(false, "温馨提示", $"读卡失败:{ex.Message}");
                return;
            }
            finally
            {
                //DO Nothing
            }
        }


        private void StartAdjustCardPos()
        {
            _working = true;
            while (_working)
            {
                var pos = _rfCpuCardReader.GetCardPosition();
                if (pos.IsSuccess && (pos.Value == CardPos.停卡位 || pos.Value == CardPos.IC位))
                {
                    //有卡，启动判卡流程
                    _working = false;

                    if (pos.Value == CardPos.停卡位)
                    {
                        _rfCpuCardReader.MoveCard(CardPos.IC位); //移动到IC卡位
                        Thread.Sleep(200); //等卡停稳
                    }
                    View.Dispatcher.Invoke(() =>
                    {
                        StartReadMT();
                    });

                    break;
                }
                Thread.Sleep(300);
            }

        }

        //读卡器内有卡后，启动判卡流程
        public virtual void StartReadMT()
        {
            //重连读卡器
            if (!SiModel.IsOpened)
            {
                var 打开结果 = SiInterface.Handle<Req初始化读卡设备, Res初始化读卡设备>(new Req初始化读卡设备());
                if (!打开结果.IsSuccess)
                {
                    ShowAlertReadCard("医保读卡", "无法连接到医保读卡器");
                    return;
                }
            }
            var 联机读卡结果 = SiInterface.Handle<Req获取个人信息, Res获取个人信息>(new Req获取个人信息());
            if (!联机读卡结果.IsSuccess)
            {
                var 脱机读卡结果 = SiInterface.Handle<Req获取卡内个人信息, Res获取卡内个人信息>(new Req获取卡内个人信息());
                if (!脱机读卡结果.IsSuccess )
                {
                    ShowAlertReadCard("医保读卡", "读卡失败，请重试", 联机读卡结果.Value.ErrorMsg ?? "");
                    return;
                }
                else
                {
                    var 个人信息 = 脱机读卡结果.Value.output.ic;
                    SiModel.res获取个人信息 = new Res获取个人信息()
                    {
                        output = new Res获取个人信息.Output()
                        {
                            ic = new Res获取个人信息.Output.Ic()
                            {
                                card_no = 个人信息.card_no,
                                ic_no = 个人信息.ic_no,
                                id_no = 个人信息.id_no,
                                personname = 个人信息.personname,
                                sex = 个人信息.sex,
                                birthday = 个人信息.birthday,
                            }
                        },
                        state = 脱机读卡结果.Value.state,
                    };
                }
            }
            SiModel.res获取个人信息 = 联机读卡结果.Value;
            name = SiModel.res获取个人信息.output.ic.personname;
            cardNo = SiModel.res获取个人信息.output.ic.card_no;
            DoCommand(myAction);
        }
        private void ShowAlertReadCard(string title, string content,string debugInfo="")
        {
            ShowAlert(false, title, content, debugInfo:debugInfo, extend: new AlertExModel()
            {
                HideCallback = tp =>
                {
                    if (tp == AlertHideType.ButtonClick)
                    {
                        if (NavigationEngine.State == A.CK.HICard) //确定还在当前页面
                        {
                            _rfCpuCardReader.MoveCard(CardPos.不持卡位);
                            StartRead();
                        }
                    }
                }
            });
            return;
        }


        public override void Confirm()
        {
            //try
            //{
            //    #region 读卡

            //    if (!CheckSiCard())
            //    {
            //        ShowAlert(false, "医保读卡", "未检测到社保卡片，请确认朝向是否正确，是否完全插入卡槽？");
            //        return;
            //    }
            //    name = string.Empty;
            //    var sign = SiFunction.ReadCardUnion(ref idNo, ref name);
            //    if (sign != 0)
            //    {
            //        Logger.Device.Info("医保卡 读卡失败");
            //        ShowAlert(false, "医保读卡", "医保读卡失败" + SiFunction.ErrMsg);
            //        return;
            //    }

            //    #endregion 读卡

            //    DoCommand(myAction);
            //}
            //finally
            //{
            //    //Do Nothing
            //}
        }
        private void Command(LoadingProcesser ctx)
        {
            PatientModel.Req病人信息查询 = new req病人信息查询
            {
                cardNo = cardNo,
                cardType = ((int)CardModel.CardType).ToString()
            };

            var choiceModel = GetInstance<IChoiceModel>();
            if (choiceModel.Business == Business.检验结果)
            {
                PatientModel.Req病人信息查询.extend = "hisOnly";
            }
            var result = DataHandlerEx.病人信息查询(PatientModel.Req病人信息查询);
            if (result.success)
            {
                PatientModel.Res病人信息查询 = result;
                CardModel.CardNo = cardNo;
                CardModel.ExternalCardInfo = "验证";
                //if (result.data.Count > 1)
                //    Navigate(A.CK.Select);
                //else
                Next();
            }
            else  if (result.msg.Contains("身份证"))
            {
                ShowAlert(false, "病人信息查询", result.msg);
                Navigate(A.Home);
                return;
                //ShowInfoFix(PatientModel.Res病人信息查询.data.FirstOrDefault());
            }
            else 
            {
                if (result.msg.Contains("未找到"))
                {
                    var 个人信息 = SiModel.res获取个人信息.output.ic;
                    CardModel.CardNo = 个人信息.card_no;
                    CardModel.ExternalCardInfo = "建档";

                    IdCardModel.IdCardNo = 个人信息.id_no;
                    IdCardModel.Name = 个人信息.personname;
                    IdCardModel.Sex = 个人信息.sex=="1" ? Sex.男: (个人信息.sex == "2"? Sex.女 :Sex.未知 );
                    IdCardModel.Birthday = DateTime.Parse(string.Format("{0}-{1}-{2}",
                        个人信息.birthday.Substring(0, 4), 个人信息.birthday.Substring(4, 2), 个人信息.birthday.Substring(6, 2)));

                    Next();
                }
                else
                {
                    ShowAlertReadCard("医保读卡", result.msg);
                }
            }
        }

        public override void DoubleClick()
        {
            StartReadMT();
            var ret = InputTextView.ShowDialogView("输入测试医保卡号和姓名");
            if (!ret.IsSuccess)
            {
                return;
            }
            name = "医保卡激活";

            string[] input = ret.Value.Replace("\r\n", "\n").Split('\n');
            cardNo = input[0];
            if (input.Length > 1)
            {
                name = input[1];
            }

            DoCommand(myAction);
        }

        public override bool OnLeaving(NavigationContext navigationContext)
        {
            _working = false;
            StopRead();
            LightBarService?.PowerOff();

            return base.OnLeaving(navigationContext);
        }

        #region Bindings
        private string _contentMsg;
        public string ContentMsg
        {
            get { return _contentMsg; }
            set
            {
                _contentMsg = value;
                OnPropertyChanged();
            }
        }
        private bool _isShowConfirm;
        public bool IsShowConfirm
        {
            get { return _isShowConfirm; }
            set
            {
                _isShowConfirm = value;
                OnPropertyChanged();
            }
        }

        #endregion Bindings
    }
}

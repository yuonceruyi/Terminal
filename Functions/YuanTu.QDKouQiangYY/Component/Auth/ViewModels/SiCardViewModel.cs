using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prism.Regions;
using YuanTu.Consts.Services;
using YuanTu.Default.Tools;
using YuanTu.Core.FrameworkBase.Loadings;
using YuanTu.Consts.Gateway;
using YuanTu.Consts;
using YuanTu.Consts.Models;
using YuanTu.Consts.Enums;
using YuanTu.Core.Log;
using YuanTu.QDArea.QingDaoSiPay;
using YuanTu.Devices;
using YuanTu.Devices.CardReader;
using YuanTu.Core.Reporter;
using System.Threading;
using Prism.Commands;
using Microsoft.Practices.Unity;
using YuanTu.Core.Services.LightBar;

namespace YuanTu.QDKouQiangYY.Component.Auth.ViewModels
{
    public class SiCardViewModel : Default.Component.Auth.ViewModels.SiCardViewModel
    {
        [Dependency]
        public ILightBarService LightBarService { get; set; }

        public SiCardViewModel(IIcCardReader[] icCardReaders, IRFCpuCardReader[] rfCpuCardReaders) : base(icCardReaders, rfCpuCardReaders)
        {
            _rfCpuCardReader = rfCpuCardReaders?.FirstOrDefault(p => p.DeviceId == "ACT_A6_RFIC");
        }

        protected string idNo, name;
        protected Action<LoadingProcesser> myAction;

        protected bool _working;
        protected bool _isA6HuaDa;

        public override void OnSet()
        {
            base.OnSet();
            BackUri = ResourceEngine.GetImageResourceUri("插社保卡");
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
                ContentMsg = "请在左侧插卡口插入医保卡";
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
            Task.Run(() => StartA6HuaDa());
        }
        protected virtual void StopRead()
        {
            StopA6HuaDa();
        }

        protected virtual void StopA6HuaDa()
        {
            _rfCpuCardReader?.UnInitialize();
            _rfCpuCardReader?.DisConnect();
        }

        protected virtual void StartA6HuaDa()
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
            catch(Exception ex)
            {
                ShowAlert(false,"温馨提示",$"读卡失败:{ex.Message}");
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
                        StartReadA6HuaDa();
                    });

                    break;
                }
                Thread.Sleep(300);
            }

        }

        //读卡器内有卡后，启动判卡流程
        public virtual void StartReadA6HuaDa()
        {
            if (!CheckSiCard())
            {
                ShowAlert(false, "医保读卡", "未检测到社保卡片，请确认朝向是否正确，重新插入社保卡", extend: new AlertExModel()
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
            name = string.Empty;
            var sign = Function.ReadCardUnion(ref idNo, ref name);
            if (sign != 0)
            {
                Logger.Device.Info("医保卡 读卡失败");
                ShowAlert(false, "医保读卡", "医保读卡失败" + Function.ErrMsg, extend: new AlertExModel()
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
            DoCommand(myAction);
        }

        public bool CheckSiCard()
        {
            try
            {
                _icCardReader.Connect();
                var result = _icCardReader.PowerOn(SlotNo.大卡座);
                if (!result.IsSuccess)
                {
                    return false;
                }
            }
            finally
            {
                _icCardReader.DisConnect();
            }
            return true;
        }

        public override void Confirm()
        {
            try
            {
                #region 读卡

                if (!CheckSiCard())
                {
                    ShowAlert(false, "医保读卡", "未检测到社保卡片，请确认朝向是否正确，是否完全插入卡槽？");
                    return;
                }
                name = string.Empty;
                var sign = Function.ReadCardUnion(ref idNo, ref name);
                if (sign != 0)
                {
                    Logger.Device.Info("医保卡 读卡失败");
                    ShowAlert(false, "医保读卡", "医保读卡失败" + Function.ErrMsg);
                    return;
                }

                #endregion 读卡

                DoCommand(myAction);
            }
            finally
            {
                //Do Nothing
            }
        }
        private void Command(LoadingProcesser ctx)
        {
            PatientModel.Req病人信息查询 = new req病人信息查询
            {
                cardNo = idNo,
                cardType = ((int)CardModel.CardType).ToString()
            };

            var result = DataHandlerEx.病人信息查询(PatientModel.Req病人信息查询);
            if (result.success)
            {
                PatientModel.Res病人信息查询 = result;
                CardModel.CardNo = idNo;
                CardModel.ExternalCardInfo = "验证";
                //if (result.data.Count > 1)
                //    Navigate(A.CK.Select);
                //else
                Next();
            }
            else
            {
                if (result.msg.Contains("未找到"))
                {
                    CardModel.CardNo = idNo;
                    CardModel.ExternalCardInfo = "建档";

                    IdCardModel.IdCardNo = idNo;
                    IdCardModel.Name = name;
                    IdCardModel.Sex = Convert.ToInt32(idNo[16]) % 2 == 0 ? Sex.女 : Sex.男;
                    IdCardModel.Birthday = DateTime.Parse(string.Format("{0}-{1}-{2}",
                        idNo.Substring(6, 4), idNo.Substring(10, 2), idNo.Substring(12, 2)));

                    Next();
                }
                else
                {
                    ShowAlert(false, "医保读卡", result.msg, extend: new AlertExModel()
                    {
                        HideCallback = tp =>
                        {
                            if (tp == AlertHideType.ButtonClick && _isA6HuaDa)
                            {
                                if (NavigationEngine.State == A.CK.HICard) //确定还在当前页面
                                {
                                    _rfCpuCardReader.MoveCard(CardPos.不持卡位);
                                    StartRead();
                                }
                            }
                        }
                    });
                }
            }
        }

        public override void DoubleClick()
        {
            var ret = InputTextView.ShowDialogView("输入测试医保卡号和姓名");
            if (!ret.IsSuccess)
            {
                return;
            }
            name = "医保卡激活";

            string[] input = ret.Value.Replace("\r\n", "\n").Split('\n');
            idNo = input[0];
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

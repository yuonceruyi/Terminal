using System.Linq;
using YuanTu.Consts.Services;
using YuanTu.Core.Services.LightBar;
using Microsoft.Practices.Unity;
using YuanTu.Devices.CardReader;
using Prism.Regions;
using YuanTu.Core.FrameworkBase;
using Prism.Commands;
using System.Windows.Input;
using YuanTu.Consts.Enums;
using System;
using YuanTu.Core.Extension;
using YuanTu.Consts.Models;
using YuanTu.Consts.Models.InfoQuery;
using YuanTu.Core.Navigating;
using YuanTu.Consts;
using YuanTu.Consts.Models.Auth;
using YuanTu.Consts.Gateway;
using YuanTu.Consts.FrameworkBase;
using YuanTu.Default.Tools;
using YuanTu.Core.Log;

namespace YuanTu.BJJingDuETYY.Component.Auth.ViewModels
{
    public class BarScanViewModel : ViewModelBase
    {

        public override string Title => "插入就诊卡";
        [Dependency]
        public ILightBarService LightBarService { get; set; }

        [Dependency]
        public ICardModel CardModel { get; set; }
        [Dependency]
        public IPatientModel PatientModel { get; set; }
        protected string _hospitalCardNo;
        protected bool _hospitalInputFocus;

        public string HospitalCardNo
        {
            get { return _hospitalCardNo; }
            set
            {
                _hospitalCardNo = value;
                OnPropertyChanged();
            }
        }

        public bool HospitalInputFocus
        {
            get { return _hospitalInputFocus; }
            set
            {
                _hospitalInputFocus = value;
                OnPropertyChanged();
            }
        }
        private Uri _cardUri;

        public Uri CardUri
        {
            get { return _cardUri; }
            set
            {
                _cardUri = value;
                OnPropertyChanged();
            }
        }

        private Uri _backUri;

        public Uri BackUri
        {
            get { return _backUri; }
            set
            {
                _backUri = value;
                OnPropertyChanged();
            }
        }

        //确认输入的卡号
        public ICommand ConfirmHospitalCardNoCommand { get; set; }

        public BarScanViewModel(IRFCardReader[] rfCardReaders, IMagCardReader[] magCardReaders)
        {
            ConfirmHospitalCardNoCommand = new DelegateCommand(ConfirmHospitalCardNo);
        }

        protected virtual void ConfirmHospitalCardNo()
        {
            if (string.IsNullOrEmpty(HospitalCardNo))
            {
                ShowAlert(false, "扫码失败", "条码扫描失败，请重试");
                HospitalCardNo = String.Empty;
                HospitalInputFocus = true;
                return;
            }
            //误差纠正，临时措施
            Logger.Main.Info("扫码内容" +  HospitalCardNo);
            if (HospitalInputFocus)
            {
                HospitalInputFocus = false;
                if (HospitalCardNo.StartsWith("015")) { HospitalCardNo = "2" + HospitalCardNo; }//扫少了
                else if (HospitalCardNo.Length>4 && HospitalCardNo.Substring(1,4)=="2015") { HospitalCardNo = HospitalCardNo.Substring(1); }//扫多了
                else if (HospitalCardNo.StartsWith("2015")) { }//扫正确
                else //不是2015开头
                {
                    ShowAlert(false, "扫码失败", "条码扫描失败，请重试");
                    HospitalCardNo = String.Empty;
                    HospitalInputFocus = true;
                    return;
                }
                Logger.Main.Info("患者最终卡号" +  HospitalCardNo);
                CardModel.CardType = CardType.条码卡;
                OnGetInfo(HospitalCardNo);
                HospitalCardNo = String.Empty;
            }
            else
            {
                ShowAlert(false, "扫码失败", "条码扫描失败，请重试");
                HospitalCardNo = String.Empty;
                Navigate(A.Home);
            }
        }
        public override void OnSet()
        {
            base.OnSet();
            //HospitalInputFocus = true;
            BackUri = ResourceEngine.GetImageResourceUri("刷条码处");
            CardUri = ResourceEngine.GetImageResourceUri("卡_条码卡反");
        }

        protected void OnGetInfo(string cardNo, string extendInfo = null)
        {
            if (cardNo.IsNullOrWhiteSpace())
            {
                ShowAlert(false, "病人信息查询", "卡片信息为空，请确认插卡方向是否正确和卡片是否有效");
                HospitalCardNo = String.Empty;
                HospitalInputFocus = true;
                return;
            }
            CardModel.CardNo = cardNo;
            //var choiceModel = GetInstance<IChoiceModel>();
            //if (choiceModel.Business == Business.查询)
            //{
            //    var queryChoiceModel = GetInstance<IQueryChoiceModel>();
            //    if (queryChoiceModel.InfoQueryType == InfoQueryTypeEnum.检验结果)
            //    {
            //        NavigationEngine.Next(new FormContext(A.ChaKa_Context, A.CK.Info));

            //        var sm = GetInstance<IShellViewModel>();
            //        sm.Alert.Display = false;
            //        //ShowAlert(true, "", "", -1);
            //        return;
            //    }

            //}

            DoCommand(ctx =>
            {
                PatientModel.Req病人信息查询 = new req病人信息查询
                {
                    Timeout = new TimeSpan(0, 1, 0), //读取病人信息设置超时1分钟
                    cardNo = cardNo,
                    cardType = ((int)CardModel.CardType).ToString()
                };
                var choiceModel = GetInstance<IChoiceModel>();
                if (choiceModel.Business == Business.检验结果)
                {
                    PatientModel.Req病人信息查询.extend = "hisOnly";
                }
                PatientModel.Res病人信息查询 = DataHandlerEx.病人信息查询(PatientModel.Req病人信息查询);

                if (PatientModel.Res病人信息查询.success)
                {
                    if (PatientModel.Res病人信息查询.data == null || PatientModel.Res病人信息查询.data.Count == 0)
                    {
                        ShowAlert(false, "病人信息查询", "未查询到病人的信息(列表为空)",5, extend: new AlertExModel()
                        {
                            HideCallback = tp =>
                            {
                                if (tp == AlertHideType.ButtonClick)
                                {
                                    //StartRead();

                                }
                            }
                        });
                        return;
                    }
                    else
                    {
                        var shellvm = GetInstance<IShellViewModel>();
                        shellvm.Busy.IsBusy = false;
                        shellvm.Alert.Display = false;
                        Next();
                    }
                }
                else if (PatientModel.Res病人信息查询.msg.Contains("身份证"))
                {
                    ShowAlert(false, "病人信息查询",PatientModel.Res病人信息查询.msg);
                    Navigate(A.Home);
                    //ShowInfoFix(PatientModel.Res病人信息查询.data.FirstOrDefault());
                }
                else
                {
                    ShowAlert(false,
                        "病人信息查询",
                        "未查询到病人的信息",
                        debugInfo: PatientModel.Res病人信息查询.msg,
                        extend: new AlertExModel()
                        {
                            HideCallback = tp =>
                            {
                                if (tp == AlertHideType.ButtonClick)
                                {
                                    //StartRead();

                                }
                            }
                        });
                }
            });
        }
        public override void OnEntered(NavigationContext navigationContext)
        {
            base.OnEntered(navigationContext);
            HospitalInputFocus = true;
            //LightBarService?.PowerOn(LightItem.就诊卡社保卡);
        }

        public override bool OnLeaving(NavigationContext navigationContext)
        {
            //LightBarService?.PowerOff();
            return base.OnLeaving(navigationContext);
        }
        public override void DoubleClick()
        {
            var ret = InputTextView.ShowDialogView("输入测试卡号");
            if (ret.IsSuccess)
            {
                CardModel.CardType = CardType.条码卡;
                OnGetInfo(ret.Value);
            }
        }
    }
}
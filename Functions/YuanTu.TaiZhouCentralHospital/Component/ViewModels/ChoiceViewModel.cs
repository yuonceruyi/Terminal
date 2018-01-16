using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media;
using Prism.Regions;
using YuanTu.Consts;
using YuanTu.Consts.Enums;
using YuanTu.Consts.FrameworkBase;
using YuanTu.Consts.Models;
using YuanTu.Consts.Services;
using YuanTu.Core.Navigating;
using YuanTu.Devices.CardReader;
using YuanTu.Devices.PrinterCheck;

namespace YuanTu.TaiZhouCentralHospital.Component.ViewModels
{
    public class ChoiceViewModel : Default.Clinic.Component.ViewModels.ChoiceViewModel
    {
        private readonly MediaPlayer _mediaPlayer = new MediaPlayer();

        private readonly Uri _source = new Uri(Path.Combine(FrameworkConst.RootDirectory, "中心医院自助机.mp3"),
            UriKind.Absolute);

        protected IMagCardReader _magCardReader;

        public ChoiceViewModel(IMagCardReader[] magCardReaders)
        {
            _magCardReader = magCardReaders?.FirstOrDefault(p => p.DeviceId == "ACT_A6_Mag&IC");
        }

        public override void OnSet()
        {
            OnSetButton();

            if (!string.IsNullOrWhiteSpace(Startup.VideoPath))
            {
                var uri = new Uri(Startup.VideoPath, UriKind.Absolute);
                VideoUri = uri;
            }

            _mediaPlayer.MediaEnded += (s, a) => { _mediaPlayer.Position = TimeSpan.Zero; };
            _mediaPlayer.Open(_source);
            _mediaPlayer.Play();
        }

        public override void OnEntered(NavigationContext navigationContext)
        {
            if (FrameworkConst.Strategies[0] != DeviceType.Clinic)
                Task.Factory.StartNew(() =>
                {
                    //检测并退卡
                    var result = _magCardReader.Connect();
                    if (!result.IsSuccess)
                        return;
                    result = _magCardReader.Initialize();
                    if (!result.IsSuccess)
                        return;
                    var pos = _magCardReader.GetCardPosition();
                    if (pos.IsSuccess && (pos.Value != CardPos.无卡 || pos.Value != CardPos.未知))
                        _magCardReader.UnInitialize();
                    _magCardReader.DisConnect();
                });

            base.OnEntered(navigationContext);
        }

        protected override Result CheckReceiptPrinter()
        {
            //诊间屏不检测打印机状态
            if (FrameworkConst.Strategies[0] == DeviceType.Clinic)
                return Result.Success();
            var choiceModel = GetInstance<IChoiceModel>();
            switch (choiceModel.Business)
            {
                case Business.建档:
                case Business.挂号:
                case Business.预约:
                case Business.取号:
                case Business.缴费:
                case Business.充值:
                case Business.住院押金:
                    return GetInstance<IReceiptPrinterCheckService>().CheckReceiptPrinter();
            }
            return Result.Success();
        }
        protected override void Do(ChoiceButtonInfo param)
        {
            var choiceModel = GetInstance<IChoiceModel>();
            choiceModel.Business = param.ButtonBusiness;
            choiceModel.HasAuthFlow = true; //默认有插卡流程
            var engine = NavigationEngine;
            var result = CheckReceiptPrinter();
            if (!result.IsSuccess)
            {
                ShowAlert(false, "打印机检测", result.Message);
                return;
            }
            switch (param.ButtonBusiness)
            {
                case Business.建档:

                    var config = GetInstance<IConfigurationManager>();
                    if (config.GetValue("SelectCreateType") == "1")
                        engine.JumpAfterFlow(new FormContext(A.ChaKa_Context, A.CK.Select),
                            CreateJump,
                            new FormContext(A.JianDang_Context, InnerA.JD.Confirm), param.Name);
                    else
                        engine.JumpAfterFlow(new FormContext(A.ChaKa_Context, A.CK.Choice),
                            CreateJump,
                            new FormContext(A.JianDang_Context, InnerA.JD.Confirm), param.Name);
                    break;

                case Business.挂号:
                    engine.JumpAfterFlow(new FormContext(A.ChaKa_Context, A.CK.Choice),
                        RegisterJump,
                        new FormContext(A.XianChang_Context, A.XC.Wether), param.Name);
                    break;

                case Business.预约:
                    engine.JumpAfterFlow(new FormContext(A.ChaKa_Context, A.CK.Choice), AppointJump,
                        new FormContext(A.YuYue_Context, A.YY.Date), param.Name);
                    break;

                case Business.取号:
                    engine.JumpAfterFlow(new FormContext(A.ChaKa_Context, A.CK.Choice), TakeNumJump,
                        new FormContext(A.QuHao_Context, A.QH.Record), param.Name);
                    break;

                case Business.缴费:
                    engine.JumpAfterFlow(new FormContext(A.ChaKa_Context, A.CK.Choice), BillPayJump,
                        new FormContext(A.JiaoFei_Context, A.JF.BillRecord), param.Name);
                    break;

                case Business.充值:
                    engine.JumpAfterFlow(new FormContext(A.ChaKa_Context, A.CK.Choice),
                        RechargeJump,
                        new FormContext(A.ChongZhi_Context, A.CZ.RechargeWay), param.Name);
                    break;

                case Business.查询:
                    Navigate(A.QueryChoice);
                    break;

                case Business.住院押金:
                    OnInRecharge(param);
                    break;

                case Business.补打:
                    Navigate(A.PrintAgainChoice);
                    break;

                case Business.实名认证:
                    engine.JumpAfterFlow(null,
                                RealAuthJump,
                                new FormContext(A.RealAuth_Context, A.SMRZ.Card), param.Name);
                    break;

                case Business.生物信息录入:
                    engine.JumpAfterFlow(new FormContext(A.ChaKa_Context, A.CK.Choice),
                        BiometricJump,
                        new FormContext(A.Biometric_Context, A.Bio.Choice), param.Name);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
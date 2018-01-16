using Microsoft.Practices.ServiceLocation;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YuanTu.Consts;
using YuanTu.Consts.Dtos.UnionPos;
using YuanTu.Consts.Enums;
using YuanTu.Consts.FrameworkBase;
using YuanTu.Consts.Gateway;
using YuanTu.Consts.Models;
using YuanTu.Consts.Models.Auth;
using YuanTu.Consts.Models.BillPay;
using YuanTu.Consts.Models.Payment;
using YuanTu.Consts.Models.Print;
using YuanTu.Consts.Services;
using YuanTu.Core.Extension;
using YuanTu.Core.Gateway;
using YuanTu.Core.Navigating;
using YuanTu.Core.Services.ConfigService;
using YuanTu.Core.Services.PrintService;
using YuanTu.Default.Component.Tools.Models;
using YuanTu.Devices.CardReader;
using YuanTu.Devices.PrinterCheck;
using YuanTu.TongXiangHospitals.HealthInsurance;
using YuanTu.TongXiangHospitals.HealthInsurance.Model;
using YuanTu.TongXiangHospitals.HealthInsurance.Service;


namespace YuanTu.TongXiangHospitals.Component.ViewModels
{
    public class ChoiceViewModel : Default.Component.ViewModels.ChoiceViewModel
    {
        protected IMagCardReader _magCardReader;

        public ChoiceViewModel(IMagCardReader[] magCardReaders) : base()
        {
            _magCardReader = magCardReaders?.FirstOrDefault(p => p.DeviceId == "ACT_A6_Mag&IC");
        }

        public override void OnEntered(NavigationContext navigationContext)
        {
            base.OnEntered(navigationContext);
            NavigationEngine.DestinationStack.Clear();
            TimeOut = 0;
            if (!FrameworkConst.OperatorId.Contains("ABC") && !FrameworkConst.OperatorId.Contains("ICBC"))
            {
                ShowAlert(false, "温馨提示", $"终端配置错误：OperatorId={FrameworkConst.OperatorId}");
            }
            if (FrameworkConst.Local)
                return;
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
                {
                    //退卡
                    _magCardReader.UnInitialize();
                }
                _magCardReader.DisConnect();
            });

            Task.Factory.StartNew(() =>
            {
                DoCommand(lp =>
                {
                    var result = GetInstance<ISiService>().Initialize();
                    if (!result.IsSuccess)
                        ShowAlert(false, "医保初始化", "医保初始化失败:\n" + result.Message);
                });
            });
            //医保接口关闭功能取消
            //Task.Factory.StartNew(() =>
            //{
            //    var siModel = GetInstance<ISiModel>();
            //    var siService = GetInstance<ISiService>();
            //    if (siModel.InitializeSuccess)
            //    {
            //        var ret = siService.Close();
            //        if (!ret.IsSuccess)
            //        {
            //            Logger.Main.Warn($"回到主界面，医保接口关闭失败");
            //        }
            //        siModel.InitializeSuccess = false;
            //        Logger.Main.Info($"回到主界面，医保接口关闭成功");
            //    }
            //});
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
                    engine.JumpAfterFlow(
                        config.GetValue("SelectCreateType") == "1"
                            ? new FormContext(A.ChaKa_Context, A.CK.Select)
                            : new FormContext(A.ChaKa_Context, A.CK.Choice),
                        CreateJump,
                        new FormContext(A.JianDang_Context, AInner.JD.Confirm), param.Name);
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

                case Business.输液费:

                    engine.JumpAfterFlow(new FormContext(A.ChaKa_Context, A.CK.Choice), BillPay2Jump,
                        new FormContext(A.JiaoFei_Context, A.JF.Confirm), param.Name);
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        protected override Result CheckReceiptPrinter()
        {
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
                case Business.输液费:
                    return GetInstance<IReceiptPrinterCheckService>().CheckReceiptPrinter();
            }
            return Result.Success();
        }

        protected virtual Task<Result<FormContext>> BillPay2Jump()
        {
            return DoCommand(lp =>
            {
                var camera = GetInstance<ICameraService>();
                camera.SnapShot("主界面 输液费");

                lp.ChangeText("正在查询输液费信息，请稍候...");

                var SiModel = GetInstance<ISiModel>();
                var PaymentModel = GetInstance<IPaymentModel>();
                var CardModel = GetInstance<ICardModel>();
                SiModel.诊间结算 = false;

                decimal self;
                decimal insurance;
                if (CardModel.CardType == CardType.社保卡)
                {
                    var result = SiPreSettle();
                    if (!result.IsSuccess)
                    {
                        ShowAlert(false, "待缴费信息查询", "没有获得待缴费信息", debugInfo: result.Message);
                        return Result<FormContext>.Fail("");
                    }
                    var resOpRegPre = SiModel.门诊缴费预结算结果确认结果;
                    self = decimal.Parse(resOpRegPre.selfFee);
                    insurance = decimal.Parse(resOpRegPre.insurFee);
                }
                else
                {
                    var result = PreSettle();
                    if (!result.IsSuccess)
                    {
                        ShowAlert(false, "待缴费信息查询", "没有获得待缴费信息", debugInfo: result.Message);
                        return Result<FormContext>.Fail("");
                    }

                    var resOpRegPre = SiModel.门诊缴费预结算结果;
                    self = decimal.Parse(resOpRegPre.selfFee);
                    insurance = decimal.Parse(resOpRegPre.insurFee);
                }

                PaymentModel.Self = self;
                PaymentModel.Insurance = insurance;
                PaymentModel.Total = self + insurance;
                PaymentModel.NoPay = self == 0;
                PaymentModel.ConfirmAction = Confirm;

                PaymentModel.MidList = new List<PayInfoItem>()
                {
                    new PayInfoItem("个人支付：",PaymentModel.Self.In元()),
                    new PayInfoItem("医保报销：",PaymentModel.Insurance.In元()),
                    new PayInfoItem("总计金额：",PaymentModel.Total.In元(),true),
                };
                return Result<FormContext>.Success(default(FormContext));
            });
        }

        #region 输液费

        protected virtual Result PreSettle()
        {
            var SiModel = GetInstance<ISiModel>();
            var CardModel = GetInstance<ICardModel>();
            var PatientModel = GetInstance<IPatientModel>();
            //请求HIS预结算
            //ctx.ChangeText("正在进行门诊缴费预结算，请稍候...");
            var reqOpPayPre = new req缴费预结算
            {
                patientId = PatientModel.当前病人信息.patientId,
                cardNo = CardModel.CardNo,
                cardType = ((int)CardModel.CardType).ToString(),

                cash = "0",
                ybCardNo = CardModel.CardNo,
                ybTradeType = SiModel.医保个人基本信息?.医保待遇类别,
                isZj = SiModel.诊间结算 ? "1" : "0",
                cardHardInfo = SiModel.CardHardInfo,
                siPatientInfo = SiModel.SiPatientInfo,
                billNo = "000000"
            };
            var resOpPayPre = DataHandlerEx.缴费预结算(reqOpPayPre);
            if (!resOpPayPre.success)
                return Result.Fail(resOpPayPre.msg);
            SiModel.门诊缴费预结算结果 = resOpPayPre.data;
            return Result.Success();
        }

        public virtual Result SiPreSettle()
        {
            var SiModel = GetInstance<ISiModel>();
            var SiService = GetInstance<ISiService>();
            var CardModel = GetInstance<ICardModel>();
            var PatientModel = GetInstance<IPatientModel>();
            //请求HIS预结算
            //ctx.ChangeText("正在进行门诊缴费预结算，请稍候...");
            var reqOpPayPre = new req缴费预结算
            {
                patientId = PatientModel.当前病人信息.patientId,
                cardNo = CardModel.CardNo,
                cardType = ((int)CardModel.CardType).ToString(),

                cash = ("0"),
                ybCardNo = CardModel.CardNo,
                ybTradeType = SiModel.医保个人基本信息?.医保待遇类别,
                isZj = SiModel.诊间结算 ? "1" : "0",
                cardHardInfo = SiModel.CardHardInfo,
                siPatientInfo = SiModel.SiPatientInfo,
                billNo = "000000"
            };
            var resOpPayPre = DataHandlerEx.缴费预结算(reqOpPayPre);
            if (!resOpPayPre.success)
            {
                ShowAlert(false, "门诊缴费预结算", "门诊缴费预结算失败", debugInfo: resOpPayPre.msg);
                return Result.Fail(resOpPayPre.msg);
            }
            SiModel.门诊缴费预结算结果 = resOpPayPre.data;

            //医保预结算
            //ctx.ChangeText("正在进行医保预结算，请稍候...");
            string insurFeeInfo = SiModel.门诊缴费预结算结果.insurFeeInfo;
            var result = SiModel.诊间结算
                ? SiService.OpPayClinicPreSettle(insurFeeInfo)
                : SiService.OpPreSettle(insurFeeInfo);
            if (!result.IsSuccess)
            {
                ShowAlert(false, "门诊缴费预结算", "门诊缴费医保预结算失败", debugInfo: result.Message);
                return Result.Fail(result.Message);
            }

            //HIS预结算确认
            //ctx.ChangeText("正在进行门诊缴费预结算结果确认，请稍候...");
            var reqOpPayPreConfirm = new req门诊缴费预结算结果确认
            {
                patientId = PatientModel.当前病人信息.patientId,
                cardNo = CardModel.CardNo,
                cardType = ((int)CardModel.CardType).ToString(),

                cash = ("0"),
                ybCardNo = CardModel.CardNo,
                ybTradeType = SiModel.医保个人基本信息?.医保待遇类别,
                isZj = SiModel.诊间结算 ? "1" : "0",
                insurFeeInfo = SiModel.RetMessage,
                cardHardInfo = SiModel.CardHardInfo,
                siPatientInfo = SiModel.SiPatientInfo,
                billNo = "000000"
            };
            var resOpPayPreConfirm = DataHandlerEx.门诊缴费预结算结果确认(reqOpPayPreConfirm);
            if (!resOpPayPreConfirm.success)
            {
                ShowAlert(false, "门诊挂号预结算", "门诊挂号预结算结果确认失败", debugInfo: resOpPayPreConfirm.msg);
                return Result.Fail(resOpPayPreConfirm.msg);
            }
            SiModel.门诊缴费预结算结果确认结果 = resOpPayPreConfirm.data;
            return Result.Success();
        }

        protected virtual Result GetSiSettleReq()
        {
            var SiModel = GetInstance<ISiModel>();
            var SiService = GetInstance<ISiService>();
            var CardModel = GetInstance<ICardModel>();
            var PatientModel = GetInstance<IPatientModel>();
            var reqOpPayPre = new req缴费预结算
            {
                patientId = PatientModel.当前病人信息.patientId,
                cardNo = CardModel.CardNo,
                cardType = ((int)CardModel.CardType).ToString(),

                cash = ("0"),
                ybCardNo = CardModel.CardNo,
                ybTradeType = SiModel.医保个人基本信息?.医保待遇类别,
                isZj = SiModel.诊间结算 ? "1" : "0",
                cardHardInfo = SiModel.CardHardInfo,
                siPatientInfo = SiModel.SiPatientInfo,
                billNo = "000000"
            };
            var resOpPayPre = DataHandlerEx.缴费预结算(reqOpPayPre);
            if (!resOpPayPre.success)
            {
                ShowAlert(false, "门诊缴费预结算", "门诊缴费预结算失败", debugInfo: resOpPayPre.msg);
                return Result.Fail(resOpPayPre.msg);
            }
            SiModel.门诊缴费预结算结果 = resOpPayPre.data;

            //医保预结算
            string insurFeeInfo = SiModel.门诊缴费预结算结果.insurFeeInfo;
            var result = SiModel.诊间结算
                ? SiService.OpPayClinicPreSettle(insurFeeInfo)
                : SiService.OpPreSettle(insurFeeInfo);
            if (!result.IsSuccess)
            {
                ShowAlert(false, "门诊缴费预结算", "门诊缴费医保预结算失败", debugInfo: result.Message);
                return Result.Fail(result.Message);
            }

            //HIS预结算确认
            var reqOpPayPreConfirm = new req门诊缴费预结算结果确认
            {
                patientId = PatientModel.当前病人信息.patientId,
                cardNo = CardModel.CardNo,
                cardType = ((int)CardModel.CardType).ToString(),

                cash = ("0"),
                ybCardNo = CardModel.CardNo,
                ybTradeType = SiModel.医保个人基本信息?.医保待遇类别,
                isZj = SiModel.诊间结算 ? "1" : "0",
                insurFeeInfo = SiModel.RetMessage,
                cardHardInfo = SiModel.CardHardInfo,
                siPatientInfo = SiModel.SiPatientInfo,
                billNo = "000000"
            };
            var resOpPayPreConfirm = DataHandlerEx.门诊缴费预结算结果确认(reqOpPayPreConfirm);
            if (!resOpPayPreConfirm.success)
            {
                ShowAlert(false, "门诊挂号预结算", "门诊挂号预结算结果确认失败", debugInfo: resOpPayPreConfirm.msg);
                return Result.Fail(resOpPayPreConfirm.msg);
            }
            SiModel.门诊缴费预结算结果确认结果 = resOpPayPreConfirm.data;
            return Result.Success();
        }

        protected virtual Result Confirm()
        {
            return DoCommand(lp =>
            {
                var SiModel = GetInstance<ISiModel>();
                var SiService = GetInstance<ISiService>();
                var PaymentModel = GetInstance<IPaymentModel>();
                var CardModel = GetInstance<ICardModel>();
                var PatientModel = GetInstance<IPatientModel>();
                var BillPayModel = GetInstance<IBillPayModel>();
                var ExtraPaymentModel = GetInstance<IExtraPaymentModel>();
                var PrintModel = GetInstance<IPrintModel>();
                var ConfigurationManager = GetInstance<IConfigurationManager>();

                //医保结算
                if (CardModel.CardType == CardType.社保卡)
                {
                    lp.ChangeText("正在进行医保结算，请稍候...");
                    if (SiModel.诊间结算)
                    {
                        var res = GetSiSettleReq();
                        if (!res.IsSuccess)
                            return Result.Fail(res.Message);
                    }
                    string insurFeeInfo = SiModel.门诊缴费预结算结果确认结果?.insurFeeInfo;
                    var result = SiModel.诊间结算
                        ? SiService.OpPayClinicSettle(insurFeeInfo)
                        : SiService.OpSettle(insurFeeInfo);
                    if (!result.IsSuccess)
                    {
                        ShowAlert(false, "门诊缴费医保结算", "门诊缴费医保结算失败", debugInfo: result.Message);
                        return Result.Fail(result.Message);
                    }
                }
                //HIS结算
                lp.ChangeText("正在进行缴费，请稍候...");

                var patientInfo = PatientModel.当前病人信息;

                var payReq = new req缴费结算
                {
                    patientId = patientInfo.patientId,
                    cardType = ((int)CardModel.CardType).ToString(),
                    cardNo = CardModel.CardNo,
                    operId = FrameworkConst.OperatorId,
                    tradeMode = GetPayMethod(PaymentModel.PayMethod),
                    cash = PaymentModel.Self.ToString("0"),
                    accountNo = patientInfo.patientId,
                    billNo = "000000",
                    allSelf = PaymentModel.Insurance == 0 ? "1" : "0"
                };
                BillPayModel.Req缴费结算 = payReq;

                FillRechargeRequest(payReq);

                if (CardModel.CardType == CardType.社保卡)
                    FillSiRequest(payReq);
                var payRes = DataHandlerEx.缴费结算(payReq);
                BillPayModel.Res缴费结算 = payRes;
                if (payRes?.success ?? false)
                {
                    ExtraPaymentModel.Complete = true;

                    PrintModel.SetPrintInfo(true, new PrintInfo
                    {
                        TypeMsg = "缴费成功",
                        TipMsg = $"您已成功于{DateTimeCore.Now:HH:mm}分缴费",
                        PrinterName = ConfigurationManager.GetValue("Printer:Receipt"),
                        Printables = BillPayPrintables(),
                        TipImage = "提示_凭条"
                    });
                    Navigate(A.JF.Print);
                    return Result.Success();
                }
                if (SiModel.诊间结算 && CardModel.CardType == CardType.社保卡)
                {
                    if (DataHandler.UnKnowErrorCode.Any(p => p == payRes?.code))
                    {
                        //打印医保单边账凭条
                        if (PaymentModel.Insurance > 0)
                        {
                            var errorMsg = $"医保消费成功，网关返回未知结果{payRes?.code.ToString()}，打印凭条结束交易！\n请执凭条到人工咨询此交易结果！";
                            医保单边账凭证(errorMsg);
                        }
                    }
                    else if (payRes?.data?.extend != null)
                    {
                        //医保退费
                        var result = SiService.OpPayClinicSettleRefund(payRes?.data?.extend);
                        if (!result.IsSuccess)
                        {
                            //医保退费失败处理
                            ShowAlert(false, "医保门诊缴费诊间结算退费", "医保门诊缴费诊间结算退费失败", debugInfo: result.Message);
                        }
                    }
                }

                //第三方支付失败时去支付流程里面处理，不在业务里面处理
                if (NavigationEngine.State != A.Third.PosUnion)
                {
                    PrintModel.SetPrintInfo(false, new PrintInfo
                    {
                        TypeMsg = "缴费失败",
                        DebugInfo = payRes?.msg
                    });
                    Navigate(A.JF.Print);
                }

                ExtraPaymentModel.Complete = true;
                return Result.Fail(payRes?.code ?? -100, payRes?.msg);
            }).Result;
        }

        protected virtual void FillRechargeRequest(req缴费结算 req)
        {
            var extraPaymentModel = GetInstance<IExtraPaymentModel>();
            if (extraPaymentModel.CurrentPayMethod == PayMethod.银联)
            {
                var posinfo = extraPaymentModel.PaymentResult as TransResDto;
                if (posinfo != null)
                {
                    req.bankCardNo = posinfo.CardNo;
                    req.bankTime = posinfo.TransTime;
                    req.bankDate = posinfo.TransDate;
                    req.posTransNo = posinfo.Trace;
                    req.bankTransNo = posinfo.Ref;
                    req.deviceInfo = posinfo.TId;
                    req.sellerAccountNo = posinfo.MId;
                }
            }
            else if (extraPaymentModel.CurrentPayMethod == PayMethod.支付宝 ||
                     extraPaymentModel.CurrentPayMethod == PayMethod.微信支付)
            {
                var thirdpayinfo = extraPaymentModel.PaymentResult as 订单状态;
                if (thirdpayinfo != null)
                {
                    req.payAccountNo = thirdpayinfo.buyerAccount;
                    req.transNo = thirdpayinfo.outPayNo;
                    req.outTradeNo = thirdpayinfo.outTradeNo;
                    req.tradeTime = thirdpayinfo.paymentTime;
                }
            }
        }

        public virtual string GetPayMethod(PayMethod payMethod)
        {
            return payMethod == PayMethod.预缴金 ? "SMK" : payMethod.GetEnumDescription();
        }

        public virtual void FillSiRequest(req缴费结算 req)
        {
            var SiModel = GetInstance<ISiModel>();
            req.extend = SiModel.RetMessage;
            req.preYbinfo = new SiInfo
            {
                cardHardInfo = SiModel.CardHardInfo,
                siPatientInfo = SiModel.SiPatientInfo,
                transNo = SiModel.门诊缴费预结算结果确认结果?.transNo
            }.ToJsonString();
        }
        protected virtual void 医保单边账凭证(string errorMsg)
        {
            var PaymentModel = GetInstance<IPaymentModel>();
            var ExtraPaymentModel = GetInstance<IExtraPaymentModel>();
            var PrintModel = GetInstance<IPrintModel>();
            var PrintManager = GetInstance<IPrintManager>();
            var ConfigurationManager = GetInstance<IConfigurationManager>();
            var queue = PrintManager.NewQueue($"医保{ExtraPaymentModel.CurrentBusiness}单边账");
            var sb = new StringBuilder();
            sb.Append($"姓名：{ExtraPaymentModel.PatientInfo?.Name}\n");
            sb.Append($"门诊号：{ExtraPaymentModel.PatientInfo?.PatientId}\n");
            sb.Append($"当前业务：{ExtraPaymentModel.CurrentBusiness}\n");
            sb.Append($"处方单号：000000\n");
            sb.Append($"医保金额：{PaymentModel?.Insurance.In元()}\n");
            sb.Append($"异常描述：{errorMsg}\n");
            sb.Append($"交易时间：{DateTimeCore.Now:yyyy-MM-dd HH:mm:ss}\n");
            sb.Append($"柜员编号：{FrameworkConst.OperatorId}\n");
            sb.Append($"请联系导医处理，祝您早日康复！\n");
            queue.Enqueue(new PrintItemText { Text = sb.ToString() });

            PrintModel.PrintInfo = new PrintInfo
            {
                PrinterName = ConfigurationManager.GetValue("Printer:Receipt"),
                Printables = queue
            };
            PrintManager.Print();
        }

        protected static readonly BarCode.Code128 BarCode128 = new BarCode.Code128
        {
            Magnify = 1,
            Height = 80
        };

        protected virtual Queue<IPrintable> BillPayPrintables()
        {
            var SiModel = GetInstance<ISiModel>();
            var PaymentModel = GetInstance<IPaymentModel>();
            var PatientModel = GetInstance<IPatientModel>();
            var BillPayModel = GetInstance<IBillPayModel>();
            var PrintManager = GetInstance<IPrintManager>();

            var queue = PrintManager.NewQueue("门诊输液费缴费");
            var billPay = BillPayModel.Res缴费结算.data;
            var patientInfo = PatientModel.Res病人信息查询.data[PatientModel.PatientInfoIndex];
            //var record = BillRecordModel.所选缴费概要;
            var resOpRegPre = SiModel.门诊缴费预结算结果确认结果;

            var sb = new StringBuilder();
            sb.Append($"状态：缴费成功\n");
            sb.Append($"姓名：{patientInfo.name}\n");
            sb.Append($"门诊号码：{patientInfo.platformId}\n");
            sb.Append($"交易类型：自助缴门诊输液费\n");
            sb.Append($"金额总计：{PaymentModel.Total.In元()}\n");
            if (resOpRegPre != null)
            {
                sb.Append($"个人支付：{resOpRegPre?.selfFee.In元()}\n");
                sb.Append($"医保报销：{resOpRegPre?.insurFee.In元()}\n");
            }
            sb.Append($"支付方式：{PaymentModel.PayMethod}\n");
            sb.Append($"发票号：{billPay.receiptNo}\n");
            sb.Append($"发药窗口：{billPay.takeMedWin}\n");
            if (!string.IsNullOrEmpty(billPay.testCode))
            {
                sb.Append($"检验条码：{billPay.testCode}\n");
                queue.Enqueue(new PrintItemText { Text = sb.ToString() });
                sb.Clear();
                var image = BarCode128.GetCodeImage(billPay.testCode, BarCode.Code128.Encode.Code128A);
                queue.Enqueue(new PrintItemImage
                {
                    Align = ImageAlign.Left,
                    Image = image,
                    Height = image.Height / 1.5f,
                    Width = image.Width / 1.5f
                });
            }
            queue.Enqueue(new PrintItemText { Text = sb.ToString() });
            sb.Clear();
            sb.Append($"交易时间：{DateTimeCore.Now:yyyy-MM-dd HH:mm:ss}\n");
            sb.Append($"柜员编号：{FrameworkConst.OperatorId}\n");
            sb.Append($"请妥善保管好您的缴费凭证，\n");
            queue.Enqueue(new PrintItemText { Text = sb.ToString() });

            return queue;
        }

        #endregion
    }
}
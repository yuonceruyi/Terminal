using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
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
using YuanTu.Consts.Models.TakeNum;
using YuanTu.Consts.Services;
using YuanTu.Core.Extension;
using YuanTu.Core.Log;
using YuanTu.Core.Navigating;
using YuanTu.Core.Services.PrintService;
using YuanTu.Default.Component.Tools.Models;
using YuanTu.JiaShanHospital.NativeServices;
using YuanTu.JiaShanHospital.NativeServices.Dto;

namespace YuanTu.JiaShanHospital.Component.ViewModels
{
    public class ChoiceViewModel : YuanTu.Default.Component.ViewModels.ChoiceViewModel
    {
        public override void OnSetButton()
        {
            var resource = ResourceEngine;
            var config = GetInstance<IConfigurationManager>();

            LayoutRule = config.GetValue("LayoutRule");
            var bts = new List<ChoiceButtonInfo>();
            var k = Enum.GetValues(typeof(Business));
            foreach (Business buttonInfo in k)
            {
                var v = config.GetValue($"Functions:{buttonInfo}:Visabled");
                if (v != "1") continue;
                bts.Add(new ChoiceButtonInfo
                {
                    Name = config.GetValue($"Functions:{buttonInfo}:Name") ?? "未定义",
                    ButtonBusiness = buttonInfo,
                    Order = config.GetValueInt($"Functions:{buttonInfo}:Order"),
                    IsEnabled = config.GetValueInt($"Functions:{buttonInfo}:IsEnabled") == 1,
                    ImageSource = resource.GetImageResource(config.GetValue($"Functions:{buttonInfo}:ImageName"))
                });
            }
            var b = config.GetValue($"InfoQuery:{InfoQueryTypeEnum.检验结果}:Visabled");
            if (b == "1")
                bts.Add(new ChoiceButtonInfo
                {
                    Name = config.GetValue($"InfoQuery:{InfoQueryTypeEnum.检验结果}:Name") ?? "未定义",
                    ButtonBusiness = (Business)100,
                    Order = config.GetValueInt($"InfoQuery:{InfoQueryTypeEnum.检验结果}:Order"),
                    IsEnabled = config.GetValueInt($"InfoQuery:{InfoQueryTypeEnum.检验结果}:IsEnabled") == 1,
                    ImageSource = resource.GetImageResource(config.GetValue($"InfoQuery:{InfoQueryTypeEnum.检验结果}:ImageName"))
                });
            Data = bts.OrderBy(p => p.Order).ToList();
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
            if (param.ButtonBusiness == (Business)100)
            {
                engine.JumpAfterFlow(new FormContext(A.ChaKa_Context, A.CK.Card),
                    CreateJump,
                    new FormContext(A.DiagReportQuery, A.Home), param.Name);
            }
            else
            {
                switch (param.ButtonBusiness)
                {
                    case Business.建档:

                        var config = GetInstance<IConfigurationManager>();
                        if (config.GetValue("SelectCreateType") == "1")
                            engine.JumpAfterFlow(new FormContext(A.ChaKa_Context, A.CK.Select),
                                CreateJump,
                                new FormContext(A.JianDang_Context, AInner.JD.Confirm), param.Name);
                        else
                            engine.JumpAfterFlow(new FormContext(A.ChaKa_Context, A.CK.Choice),
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
                            new FormContext(A.JiaoFei_Context, A.JF.Confirm), param.Name);
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
        protected override Task<Result<FormContext>> TakeNumJump()
        {
            return DoCommand(lp =>
            {
                var camera = GetInstance<ICameraService>();
                camera.SnapShot("主界面 预约取号");
                var patientModel = GetInstance<IPatientModel>();
                var recordModel = GetInstance<IAppoRecordModel>();
                var cardModel = GetInstance<ICardModel>();
                var takeNumModel = GetInstance<ITakeNumModel>();
                lp.ChangeText("正在查询预约记录，请稍候...");
                recordModel.Req挂号预约记录查询 = new req挂号预约记录查询
                {
                    patientId = patientModel.当前病人信息?.idNo,
                    patientName = patientModel.当前病人信息?.name,
                    startDate = DateTimeCore.Now.ToString("yyyy-MM-dd"),
                    endDate = DateTimeCore.Now.AddDays(7).ToString("yyyy-MM-dd"),
                    searchType = "1",
                    appoNo = patientModel.当前病人信息?.phone,
                    cardNo = cardModel.CardNo,
                    cardType = ((int)cardModel.CardType).ToString()
                };
                recordModel.Res挂号预约记录查询 = DataHandlerEx.挂号预约记录查询(recordModel.Req挂号预约记录查询);
                if (recordModel.Res挂号预约记录查询?.success ?? false)
                {
                    if (recordModel.Res挂号预约记录查询?.data?.Count > 1)
                    {
                        return Result<FormContext>.Success(default(FormContext));
                    }
                    if (recordModel.Res挂号预约记录查询?.data?.Count == 1)
                    {
                        recordModel.所选记录 = recordModel.Res挂号预约记录查询.data.FirstOrDefault();
                        var record = recordModel.所选记录;

                        takeNumModel.List = new List<PayInfoItem>
                        {
                            new PayInfoItem("就诊日期：", record.medDate.SafeConvertToDate("yyyy-MM-dd", "yyyy年MM月dd日")),
                            new PayInfoItem("就诊科室：", record.deptName),
                            new PayInfoItem("就诊医生：", record.doctName),
                            new PayInfoItem("就诊序号：", record.appoNo),
                            new PayInfoItem("挂号金额：", record.regAmount.In元(), true)
                        };
                        return Result<FormContext>.Success(new FormContext(A.QuHao_Context, A.QH.TakeNum));
                    }
                    ShowAlert(false, "预约记录查询", "没有获得预约记录信息(列表为空)");
                    return Result<FormContext>.Fail("");
                }
                ShowAlert(false, "预约记录查询", "没有获得预约记录信息", debugInfo: recordModel.Res挂号预约记录查询?.msg);
                return Result<FormContext>.Fail("");
            });
        }

        protected override Task<Result<FormContext>> BillPayJump()
        {
            return DoCommand(lp =>
            {
                var camera = GetInstance<ICameraService>();
                camera.SnapShot("主界面 结算"); ;
                var cardModel = GetInstance<ICardModel>();
                var paymentModel = GetInstance<IPaymentModel>();
                lp.ChangeText("正在缴费预结算，,处理时间在1-2分钟请稍候...");
                var req = new PerCheckoutRequest()
                {
                    CheckoutType = "",
                    CardNo = cardModel.CardNo,
                    SelfPayTag = cardModel.CardType == CardType.就诊卡 ? 1 : 0,
                    PayFlag = PayMedhodFlag.银联,
                };
                Logger.Net.Info($"嘉善:缴费预结算入参{JsonConvert.SerializeObject(req)}");
                var res = LianZhongHisService.GetHospitalPerCheckoutInfo(req);
                Logger.Net.Info($"嘉善:缴费预结算出参{JsonConvert.SerializeObject(res)}");
                if (!res.IsSuccess)
                {
                    ShowAlert(false, "温馨提示", $"缴费预结算失败,原因:{res.Message}");
                    return Result<FormContext>.Fail("");
                }
                paymentModel.Self = decimal.Parse(res.Value?.ActualPay ?? "0") * 100;
                paymentModel.Insurance = decimal.Parse(res.Value?.HealthCarePay ?? "0") * 100;
                paymentModel.Total = decimal.Parse(res.Value?.TotalPay ?? "0") * 100;
                paymentModel.NoPay = paymentModel.Self == 0;
                paymentModel.ConfirmAction = Confirm;

                paymentModel.MidList = new List<PayInfoItem>
                {
                    new PayInfoItem("个人支付：", paymentModel.Self.In元()),
                    new PayInfoItem("医保报销：", paymentModel.Insurance.In元()),
                    new PayInfoItem("支付金额：", paymentModel.Total.In元(), true)
                };
                return Result<FormContext>.Success(default(FormContext));
            });
        }

        protected override Task<Result<FormContext>> IpRechargeJump()
        {
            return null;
        }

        private string _takeDrugWindow = string.Empty;
        protected Result Confirm()
        {
            return DoCommand(lp =>
            {
                lp.ChangeText("正在进行缴费,处理时间在1-2分钟请稍候...");
                try
                {
                    var cardModel = GetInstance<ICardModel>();
                    var paymentModel = GetInstance<IPaymentModel>();
                    var extraPaymentModel = GetInstance<IExtraPaymentModel>();
                    var printModel = GetInstance<IPrintModel>();
                    var configurationManager = GetInstance<IConfigurationManager>();
                    var req = new CheckoutRequest()
                    {
                        CheckoutType = "",
                        CardNo = cardModel.CardNo,
                        SelfPayTag = cardModel.CardType == CardType.就诊卡 ? 1 : 0,
                    };
                    req.AlipayAmount = (paymentModel.Self / 100).ToString();
                    switch (paymentModel.PayMethod)
                    {
                        case PayMethod.未知:
                            break;
                        case PayMethod.现金:
                            break;
                        case PayMethod.银联:
                            Logger.Main.Info("缴费银联支付");
                            req.PayFlag = PayMedhodFlag.银联;
                            req.Account = (extraPaymentModel.PaymentResult as TransResDto).CardNo;
                            req.AlipayTradeNo = (extraPaymentModel.PaymentResult as TransResDto).Ref;
                            Logger.Main.Info("缴费银联Model构造成功");
                            break;
                        case PayMethod.预缴金:
                            //req.PayFlag = PayMedhodFlag.院内账户;
                            break;
                        case PayMethod.社保:
                            break;
                        case PayMethod.支付宝:
                            req.PayFlag = PayMedhodFlag.支付宝;
                            req.AlipayTradeNo = (extraPaymentModel.PaymentResult as 订单状态).outTradeNo;
                            break;
                        case PayMethod.微信支付:
                            req.PayFlag = PayMedhodFlag.微信;
                            req.AlipayTradeNo = (extraPaymentModel.PaymentResult as 订单状态).outTradeNo;
                            break;
                        case PayMethod.苹果支付:
                            break;
                        case PayMethod.智慧医疗:
                            break;
                        default:
                            break;
                    }
                    var res = LianZhongHisService.ExcuteHospitalCheckout(req);
                    if (res.IsSuccess)
                    {
                        _takeDrugWindow = res.Value?.TakeDrugWindow ?? "请咨询护士台";
                        extraPaymentModel.Complete = true;
                        printModel.SetPrintInfo(true, new PrintInfo
                        {
                            TypeMsg = "缴费成功",
                            TipMsg = $"您已成功于{DateTimeCore.Now.ToString("HH:mm")}分缴费",
                            PrinterName = configurationManager.GetValue("Printer:Receipt"),
                            Printables = BillPayPrintables(res.Value.BillId),
                            TipImage = "提示_凭条"
                        });
                        lp.ChangeText("正在进行缴费交易记录上传，请稍候...");
                        UploadTradeInfo(paymentModel, res.Value.BillId);
                        Navigate(A.JF.Print);
                        return Result.Success();
                    }
                    //第三方支付失败时去支付流程里面处理，不在业务里面处理
                    if (NavigationEngine.State != A.Third.PosUnion)
                    {
                        //PrintModel.SetPrintInfo(false, "缴费失败", errorMsg: BillPayModel.Res缴费结算?.msg);
                        printModel.SetPrintInfo(false, new PrintInfo
                        {
                            TypeMsg = "缴费失败",
                            DebugInfo = res.Message
                        });
                    }
                    if (res.ResultCode == -100)
                    {
                        UploadTradeInfo(paymentModel, "123456");
                    }
                    ShowAlert(false, "友情提示", $"缴费失败:{res.Message}");
                    extraPaymentModel.Complete = true;
                    return res.ResultCode == -100 ? Result.Fail(-100, res.Message) : Result.Fail(-1, res.Message);
                }
                catch (Exception e)
                {
                    return Result.Fail(-1, $"支付异常{e}");
                }

            }).Result;
        }

        private void UploadTradeInfo(IPaymentModel paymentModel, string billId)
        {
            try
            {
                if (paymentModel.Self != 0)
                {
                    自费交易记录同步到his系统(billId);
                }
                if (paymentModel.Insurance != 0)
                {
                    paymentModel.PayMethod = PayMethod.社保;
                    医保交易记录同步到his系统(billId);
                }
            }
            catch (Exception e)
            {
                Logger.Net.Error($"嘉善:缴费交易记录同步到his系统失败;{e.Message}");
            }
        }

        protected Queue<IPrintable> BillPayPrintables(string id)
        {
            var cardModel = GetInstance<ICardModel>();
            var printManager = GetInstance<IPrintManager>();
            var paymentModel = GetInstance<IPaymentModel>();
            var patientModel = GetInstance<IPatientModel>();
            var queue = printManager.NewQueue("门诊费用缴费");
            var patientInfo = patientModel.Res病人信息查询.data[patientModel.PatientInfoIndex];
            var payMethod = paymentModel.Self == 0 ? "医保支付" : paymentModel.PayMethod.ToString();
            var sb = new StringBuilder();
            sb.Append($"状态：缴费成功\n");
            sb.Append($"姓名：{patientInfo.name}\n");
            sb.Append($"就诊卡号：{cardModel.CardNo}\n");
            sb.Append($"交易类型：自助缴费\n");
            queue.Enqueue(new PrintItemText { Text = sb.ToString() });
            sb.Clear();
            sb.Append($"金额总计：{paymentModel.Total.In元()}\n");
            sb.Append($"自费金额：{paymentModel.Self.In元()}\n");
            sb.Append($"医保金额：{paymentModel.Insurance.In元()}\n");
            sb.Append($"取药窗口：{_takeDrugWindow}\n");
            queue.Enqueue(new PrintItemText { Text = sb.ToString(), Font = new Font("微软雅黑", (CurrentStrategyType() == DeviceType.Clinic ? 10 : 14), FontStyle.Bold) });
            sb.Clear();
            sb.Append($"支付方式：{payMethod}\n");
            sb.Append($"交易时间：{DateTimeCore.Now.ToString("yyyy-MM-dd HH:mm:ss")}\n");
            sb.Append($"交易ID：{id}\n");
            sb.Append($"柜员编号：{FrameworkConst.OperatorId}\n");
            sb.Append($"该凭条不作报销凭证！\n");
            sb.Append($"如需发票，请凭此票到人工窗口换取发票！\n");
            queue.Enqueue(new PrintItemText { Text = sb.ToString() });
            return queue;
        }
        private void 自费交易记录同步到his系统(string settleId)
        {
            var cardModel = GetInstance<ICardModel>();
            var paymentModel = GetInstance<IPaymentModel>();
            var patientModel = GetInstance<IPatientModel>();
            try
            {
                Logger.Net.Info($"开始缴费交易记录同步到his系统");
                var req = new req交易记录同步到his系统
                {
                    platformId = FrameworkConst.OperatorId,
                    hisPatientId = cardModel?.CardNo,
                    cardNo = cardModel?.CardNo,
                    idNo = patientModel?.当前病人信息?.idNo,
                    patientName = patientModel?.当前病人信息?.name,
                    tradeType = "2",
                    cash = paymentModel?.Self.ToString(),
                    operId = FrameworkConst.OperatorId,
                    bankTime = DateTimeCore.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                    tradeMode = GetEnumDescription(paymentModel.PayMethod),
                    inHos = "1",
                    remarks = "缴费自费",
                    settleId = settleId
                };
                FillRechargeRequest(req);
                var res = DataHandlerEx.交易记录同步到his系统(req);
                if (res.success)
                {
                    Logger.Net.Info($"缴费交易记录同步到his系统成功");
                }
                else
                {
                    Logger.Net.Info($"缴费交易记录同步到his系统失败:{res.msg}");
                }
            }
            catch (Exception ex)
            {
                Logger.Main.Error($"缴费交易记录同步到his系统失败异常:{ex.Message}");
            }
        }
        private void 医保交易记录同步到his系统(string settleId)
        {
            var cardModel = GetInstance<ICardModel>();
            var paymentModel = GetInstance<IPaymentModel>();
            var patientModel = GetInstance<IPatientModel>();
            try
            {
                Logger.Net.Info($"开始缴费交易记录同步到his系统");
                var req = new req交易记录同步到his系统
                {
                    platformId = FrameworkConst.OperatorId,
                    hisPatientId = cardModel?.CardNo,
                    cardNo = cardModel?.CardNo,
                    idNo = patientModel?.当前病人信息?.idNo,
                    patientName = patientModel?.当前病人信息?.name,
                    tradeType = "2",
                    cash = paymentModel?.Insurance.ToString(),
                    operId = FrameworkConst.OperatorId,
                    bankTime = DateTimeCore.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                    tradeMode = GetEnumDescription(paymentModel.PayMethod),
                    inHos = "1",
                    remarks = "缴费医保",
                    settleId = settleId
                };
                var res = DataHandlerEx.交易记录同步到his系统(req);
                if (res.success)
                {
                    Logger.Net.Info("缴费交易记录同步到his系统成功");
                }
                else
                {
                    Logger.Net.Info($"缴费交易记录同步到his系统失败:{res.msg}");
                }
            }
            catch (Exception ex)
            {
                Logger.Main.Error($"缴费交易记录同步到his系统失败异常:{ex.Message}");
            }
        }
        public static string GetEnumDescription(PayMethod value)
        {
            FieldInfo fi = value.GetType().GetField(value.ToString());
            DescriptionAttribute[] attributes = (DescriptionAttribute[])fi.GetCustomAttributes(typeof(DescriptionAttribute), false);
            if (attributes != null && attributes.Length > 0)
            {
                return attributes[0].Description;
            }
            else
            {
                return value.ToString();
            }
        }
        protected void FillRechargeRequest(req交易记录同步到his系统 req)
        {
            var extraPaymentModel = GetInstance<IExtraPaymentModel>();
            if (extraPaymentModel.CurrentPayMethod == PayMethod.银联)
            {
                var posinfo = extraPaymentModel.PaymentResult as TransResDto;
                if (posinfo != null)
                {
                    req.bankCardNo = posinfo.CardNo;
                    req.bankDate = posinfo.TransDate;
                    req.posTransNo = posinfo.Trace;
                    req.bankTransNo = posinfo.Ref;
                    req.deviceInfo = posinfo.TId;
                    req.sellerAccountNo = posinfo.MId;
                }
            }
            else if (extraPaymentModel.CurrentPayMethod == PayMethod.支付宝 || extraPaymentModel.CurrentPayMethod == PayMethod.微信支付)
            {
                var thirdpayinfo = extraPaymentModel.PaymentResult as 订单状态;
                if (thirdpayinfo != null)
                {
                    req.payAccountNo = thirdpayinfo.buyerAccount;
                    req.transNo = thirdpayinfo.outPayNo;
                    //req.outTradeNo = thirdpayinfo.outTradeNo;
                    req.tradeTime = thirdpayinfo.paymentTime;
                    req.extend = thirdpayinfo.outTradeNo;
                }
            }
        }
    }
}

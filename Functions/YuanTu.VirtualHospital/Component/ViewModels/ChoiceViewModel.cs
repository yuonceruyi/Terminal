using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using YuanTu.Consts;
using YuanTu.Consts.Enums;
using YuanTu.Consts.FrameworkBase;
using YuanTu.Consts.Gateway;
using YuanTu.Consts.Models;
using YuanTu.Consts.Models.Auth;
using YuanTu.Consts.Models.Payment;
using YuanTu.Consts.Models.Print;
using YuanTu.Consts.Services;
using YuanTu.Core.Extension;
using YuanTu.Core.Navigating;
using YuanTu.Core.Systems;
using YuanTu.Devices.PrinterCheck.MsPrinterCheck;
using YuanTu.VirtualHospital.Component.Loan.Models;

namespace YuanTu.VirtualHospital.Component.ViewModels
{
    public class ChoiceViewModel : Default.Component.ViewModels.ChoiceViewModel
    {
        public override void OnSetButton()
        {
            base.OnSetButton();

            var resource = ResourceEngine;
            var config = GetInstance<IConfigurationManager>();
            var keyName = "外院卡激活";
            var re = config.GetValue($"Functions:{keyName}:Visabled");
            if (re == "1")
            {
                //var bs = (Business)(-10);

                var dt = Data.ToList();
                dt.Add(new ChoiceButtonInfo
                {
                    Name = config.GetValue($"Functions:{keyName}:Name") ?? "未定义",
                    ButtonBusiness = (Business) (-1),
                    Order = config.GetValueInt($"Functions:{keyName}:Order"),
                    IsEnabled = config.GetValueInt($"Functions:{keyName}:IsEnabled") == 1,
                    ImageSource = resource.GetImageResource(config.GetValue($"Functions:{keyName}:ImageName"))
                });
                Data = dt.OrderBy(p => p.Order).ToList();
            }

            // dt.Add();
        }

        protected override void Do(ChoiceButtonInfo param)
        {
            var result = CheckReceiptPrinter();
            if (!result.IsSuccess)
            {
                ShowAlert(false, "打印机检测", result.Message);
                return;
            }
            var choiceModel = GetInstance<IChoiceModel>();

            choiceModel.Business = param.ButtonBusiness;
            choiceModel.HasAuthFlow = true; //默认有插卡流程
            var engine = NavigationEngine;
            if ((int) param.ButtonBusiness == -1) //外院激活
            {
                engine.JumpAfterFlow(null,
                    CreateJump,
                    new FormContext(InnerA.WaiYuanCard_Contenxt, InnerA.WYC.WYCard), param.Name);
                return;
            }
            switch (param.ButtonBusiness)
            {
                case Business.建档:
                  
                    var config = GetInstance<IConfigurationManager>();
                    if (config.GetValue("SelectCreateType") == "1")
                        engine.JumpAfterFlow(new FormContext(A.ChaKa_Context, A.CK.Select),
                            CreateJump,
                            new FormContext(A.JianDang_Context, A.JD.Print), param.Name);
                    else
                        engine.JumpAfterFlow(new FormContext(A.ChaKa_Context, A.CK.Choice),
                            CreateJump,
                            new FormContext(A.JianDang_Context, A.JD.Print), param.Name);
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

                case Business.生物信息录入:
                    engine.JumpAfterFlow(new FormContext(A.ChaKa_Context, A.CK.Choice),
                        BiometricJump,
                        new FormContext(A.Biometric_Context, A.Bio.Choice), param.Name);
                    break;
               case Business.出院结算:
                   //choiceModel.HasAuthFlow = false;
                   choiceModel.AuthContext = A.ZhuYuan_Context;
                   NavigationEngine.JumpAfterFlow(new FormContext(A.ZhuYuan_Context, A.ZY.InPatientNo), ChuYuanJump,
                       new FormContext(InnerA.ChuYuan_Context, InnerA.ChuYuan.Confirm), param.Name);
                    break;

                case Business.切换终端:
                    var pid = Process.GetCurrentProcess().Id;
                    var other = Process.GetProcessesByName("Terminal")
                        .Concat(Process.GetProcessesByName("YuanTu.Test"))
                        .FirstOrDefault(proc => proc.Id != pid);
                    if (other != null)
                        WindowHelper.SetForegroundWindow(other.MainWindowHandle);
                    break;

                case Business.先诊疗后付费:
                    engine.JumpAfterFlow(new FormContext(A.ChaKa_Context, A.CK.Choice),
                        LoanJump,
                        new FormContext(InnerA.Loan_Context, InnerA.Loan.Choice), param.Name);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private Task<Result<FormContext>> LoanJump()
        {
            return DoCommand(lp =>
            {
                var CardModel = GetInstance<ICardModel>();
                var LoanModel = GetInstance<ILoanModel>();
                var req = new req查询借款权限
                {
                    cardNo = CardModel.CardNo,
                    cardType = ((int) CardModel.CardType).ToString(),
                };
                LoanModel.Req查询借款权限 = req;
                LoanModel.Valid = false;
                LoanModel.HospitalRemainingAmount = 0m;
                var res = DataHandlerEx.查询借款权限(req);
                LoanModel.Res查询借款权限 = res;
                if (res == null || !res.success || res.data == null)
                {
                    ShowAlert(false, "借款人信息查询", $"借款人信息查询失败\n{res?.msg}");
                    return Result<FormContext>.Fail("");
                }
                LoanModel.Valid = true;
                if (decimal.TryParse(res.data.hospRemainingAmt, out var amount))
                    LoanModel.HospitalRemainingAmount = amount;
                return Result<FormContext>.Success(default(FormContext));
            });
        }

        protected virtual Task<Result<FormContext>> ChuYuanJump()
        {
            var PaymentModel = GetInstance<IPaymentModel>();
            PaymentModel.Self = 10000;
            PaymentModel.Insurance = decimal.Parse("0");
            PaymentModel.Total = 10000;
            PaymentModel.NoPay = true;
            PaymentModel.ConfirmAction = ChuYuanConfirm;

            PaymentModel.LeftList = new List<PayInfoItem>
            {
                new PayInfoItem("入院日期：", DateTime.Now.ToString("yyyy年MM月dd日")),
                new PayInfoItem("入院科室：", "内科"),
                new PayInfoItem("主治医生：", "李光洙")
            };

            PaymentModel.RightList = new List<PayInfoItem>
            {
                new PayInfoItem("个人支付：", PaymentModel.Self.In元()),
                new PayInfoItem("医保报销：", PaymentModel.Insurance.In元()),
                new PayInfoItem("支付金额：", PaymentModel.Total.In元(), true)
            };
            return null;
        }

        private Result ChuYuanConfirm()
        {
           return DoCommand(lp =>
            {
                lp.ChangeText("正在办理出院，请稍候...");
                Thread.Sleep(5000);
                var PrintModel = GetInstance<IPrintModel>();
                PrintModel.SetPrintInfo(true, new PrintInfo
                {
                    TypeMsg = "缴费成功",
                    TipMsg = $"您已成功于{DateTimeCore.Now:HH:mm}分出院",
                    PrinterName = "",
                    Printables = null,
                    TipImage = "提示_凭条"
                });
                Next();
                return Result.Success();
            }).Result;
        }
    }
}
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
using YuanTu.Consts.Models.Create;
using YuanTu.Consts.Models.Print;
using YuanTu.Default.Component.Tools.Models;
using YuanTu.Devices.CardReader;

namespace YuanTu.VirtualHospital.Component.Recharge.ViewModels
{
    public class RechargeMethodViewModel: YuanTu.Default.Component.Recharge.ViewModels.RechargeMethodViewModel
    {

        protected override void OnPayButtonClick(Info i)
        {
            var choiceModel = GetInstance<IChoiceModel>();
            if (choiceModel.Business != Business.建档)
            {
                base.OnPayButtonClick(i);
                return;
            }
            
            var idCardModel = GetInstance<IIdCardModel>();
            var payMethod = (PayMethod)i.Tag;
            OpRechargeModel.RechargeMethod = payMethod;
            ExtraPaymentModel.Complete = false;
            ExtraPaymentModel.CurrentBusiness = Business.充值;
            ExtraPaymentModel.CurrentPayMethod = payMethod;
            ExtraPaymentModel.FinishFunc = OnJianDangRechargeCallback;
            //准备门诊充值所需病人信息
            ExtraPaymentModel.PatientInfo = new PatientInfo
            {
                Name = idCardModel.Name,
                PatientId = null,
                IdNo = idCardModel.IdCardNo,
                GuardianNo = null,
                CardNo = CardModel.CardNo,
                CardType = CardModel.CardType,
                Remain = 0m
            };

            ChangeNavigationContent(OpRechargeModel.RechargeMethod.ToString());
            switch (payMethod)
            {
                case PayMethod.未知:
                case PayMethod.预缴金:
                case PayMethod.社保:
                    throw new ArgumentOutOfRangeException();
                case PayMethod.现金:
                    OnCAClick();
                    break;

                case PayMethod.银联:
                case PayMethod.支付宝:
                case PayMethod.微信支付:
                case PayMethod.苹果支付:
                    Navigate(InnerA.JDCZ.InputAmount);
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        protected  Task<Result> OnJianDangRechargeCallback()
        {
            var choiceModel = GetInstance<IChoiceModel>();
            if (choiceModel.Business != Business.建档)
            {
                return base.OnRechargeCallback();

            }
            //此处执行真正的建档流程
            return DoCommand<Result>(lp =>
            {
                var createModel = GetInstance<ICreateModel>();
                var idCardModel = GetInstance<IIdCardModel>();
                //填充支付信息
                // CreateModel.Req病人建档发卡
                createModel.Req病人建档发卡.cash = ExtraPaymentModel.TotalMoney.ToString("0");
                FillRechargeRequest(createModel.Req病人建档发卡);
                lp.ChangeText("正在建档，请稍候...");
                createModel.Res病人建档发卡 = DataHandlerEx.病人建档发卡(createModel.Req病人建档发卡);
                if (createModel.Res病人建档发卡?.success ?? false)
                {
                    ExtraPaymentModel.Complete = true;
                    lp.ChangeText("正在发卡，请及时取卡。");
                    if (!FrameworkConst.DoubleClick)
                    {
                        var rfdispenser = GetInstance<IRFCardDispenser[]>().FirstOrDefault(p => p.DeviceId == "ZBR_RF");
                        rfdispenser.PrintContent(new List<ZbrPrintTextItem> {new ZbrPrintTextItem()},
                            new List<ZbrPrintCodeItem> {new ZbrPrintCodeItem()});

                    }
                    PrintModel.SetPrintInfo(true, new PrintInfo
                    {
                        TypeMsg = "建档发卡成功",
                        TipMsg = $"您已成功于{DateTimeCore.Now.ToString("HH:mm")}分建档",
                        PrinterName = ConfigurationManager.GetValue("Printer:Receipt"),
                        Printables = CreatePrintables(),
                        TipImage = "提示_凭条和发卡"
                    });
                    ChangeNavigationContent($"{idCardModel.Name}\r\n卡号{CardModel.CardNo}");
                    Navigate(InnerA.JDCZ.Print);
                    return Result.Success();
                }
                else
                {
                    ShowAlert(false, "建档发卡", "建档发卡失败", debugInfo: createModel.Res病人建档发卡?.msg);
                    return Result.Fail(createModel.Res病人建档发卡?.msg);
                }

            });
        }

        protected virtual void FillRechargeRequest(req病人建档发卡 req)
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
                    req.bankCardNo = thirdpayinfo.buyerAccount;
            }
        }

        protected virtual Queue<IPrintable> CreatePrintables()
        {
            var createModel = GetInstance<ICreateModel>();
            var req = createModel.Req病人建档发卡;
            var queue = PrintManager.NewQueue("自助发卡");

            var sb = new StringBuilder();
            sb.Append($"状态：办卡成功\n");
            sb.Append($"发卡单位：{FrameworkConst.HospitalName}\n");
            sb.Append($"姓名：{req.name}\n");
            sb.Append($"就诊卡号：{CardModel.CardNo}\n");
            sb.Append($"交易时间：{DateTimeCore.Now.ToString("yyyy-MM-dd HH:mm:ss")}\n");
            sb.Append($"柜员编号：{FrameworkConst.OperatorId}\n");
            sb.Append($"请妥善保管好您的个人信息。\n");
            sb.Append($"祝您早日康复！\n");
            queue.Enqueue(new PrintItemText { Text = sb.ToString() });
            return queue;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YuanTu.Consts;
using YuanTu.Consts.Enums;
using YuanTu.Consts.FrameworkBase;
using YuanTu.Consts.Gateway;
using YuanTu.Consts.Models;
using YuanTu.Consts.Models.Payment;
using YuanTu.Consts.Models.Print;
using YuanTu.Core.Extension;
using YuanTu.Core.FrameworkBase.Loadings;
using YuanTu.Core.Log;
using YuanTu.Core.Services.PrintService;
using YuanTu.Default.Component.Tools.Models;
using YuanTu.FuYangRMYY.HisNative.Models;
using YuanTu.FuYangRMYY.Managers;
using YuanTu.FuYangRMYY.Services;

namespace YuanTu.FuYangRMYY.Component.BillPay
{
    public class BillRecordViewModel:Default.Component.BillPay.ViewModels.BillRecordViewModel
    {
        protected override void Do()
        {
            BillRecordModel.所选缴费概要 = SelectData.Tag.As<缴费概要信息>();
            ChangeNavigationContent(SelectData.CatalogContent);

            var recordInfo = BillRecordModel.所选缴费概要;
            
            PaymentModel.Self = decimal.Parse(recordInfo.billFee);
            PaymentModel.Insurance = decimal.Parse("0");
            PaymentModel.Total = decimal.Parse(recordInfo.billFee);
            PaymentModel.NoPay = false;
            PaymentModel.ConfirmAction = Confirm;

            
            PaymentModel.LeftList = new List<PayInfoItem>
            {
                new PayInfoItem("日期：", recordInfo.billDate),
                new PayInfoItem("时间：", null),
                new PayInfoItem("科室：", recordInfo.deptName),
                new PayInfoItem("医生：", recordInfo.doctName)
            };

            PaymentModel.RightList = new List<PayInfoItem>
            {
                //new PayInfoItem("个人支付：", PaymentModel.Self.In元()),
                //new PayInfoItem("医保报销：", PaymentModel.Insurance.In元()),
                new PayInfoItem("支付金额：", PaymentModel.Total.In元(), true)
            };
            Next();
        }

        protected override Result Confirm()
        {
            return DoCommand(lp =>
            {
                var extraPaymentModel = GetInstance<IExtraPaymentModel>();

                lp.ChangeText("正在进行缴费，请稍候...");
                var useSiPay = CardModel.CardType == CardType.社保卡 && extraPaymentModel.CurrentPayMethod == PayMethod.社保;
                var patientInfo = PatientModel.当前病人信息;
                var record = BillRecordModel.所选缴费概要;
                var extendObj = new
                {
                    ExpStr = $"{(useSiPay ? $"05" : "02")}^{record.extend}",/* $"5^{record.billNo}" : $"2^{record.billNo}",*/
                    InsuFlag=useSiPay?"1":"0"
                }.ToJsonString();
                BillPayModel.Req缴费结算 = new req缴费结算
                {
                    patientId = patientInfo.patientId,
#pragma warning disable 612
                    patientName = patientInfo.name,
#pragma warning restore 612
                    cardType = ((int)CardModel.CardType).ToString(),
                    cardNo = CardModel.CardNo,
                    operId = FrameworkConst.OperatorId,
                    tradeMode = PaymentModel.PayMethod.GetEnumDescription(),
                    cash = PaymentModel.Total.ToString(CultureInfo.InvariantCulture),
                    accountNo = patientInfo.patientId,
                    billNo = record.billNo,
                    allSelf = PaymentModel.Insurance == 0 ? "1" : "0",
                    extend = extendObj
                };

                FillRechargeRequest(BillPayModel.Req缴费结算);
                BillPayModel.Res缴费结算 = DataHandlerEx.缴费结算(BillPayModel.Req缴费结算);
                if (BillPayModel.Res缴费结算?.success ?? false)
                {
                    var ret=Result.Success();
                    if (CardModel.CardType == CardType.社保卡 && extraPaymentModel.CurrentPayMethod == PayMethod.社保)
                    {
                        var resp = PayWithSi(lp);
                        if (!resp)
                        {
                            HisService.ConfirmSiBillPay(false, patientInfo, BillRecordModel.所选缴费概要,
                                BillPayModel.Res缴费结算.data);
                            ShowAlert(false, "社保交易", resp.Message);
                            return resp.Convert();
                        }

                        var pModel = BillPayModel as Models.BillPayModel;
                        pModel.社保缴费结算 = resp.Value;
                        lp.ChangeText("正在确认缴费信息");
                        var result = HisService.ConfirmSiBillPay(true, patientInfo, BillRecordModel.所选缴费概要,BillPayModel.Res缴费结算.data);
                        if (!result)
                        {
                            ShowAlert(false, "社保交易", result.Message);
                            return result.Convert();
                        }

                    }

                    if (ret)
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
                   
                }

                //第三方支付失败时去支付流程里面处理，不在业务里面处理
                if (NavigationEngine.State != A.Third.PosUnion)
                {
                    //PrintModel.SetPrintInfo(false, "缴费失败", errorMsg: BillPayModel.Res缴费结算?.msg);
                    PrintModel.SetPrintInfo(false, new PrintInfo
                    {
                        TypeMsg = "缴费失败",
                        DebugInfo = BillPayModel.Res缴费结算?.msg
                    });
                    Navigate(A.JF.Print);
                }

                ExtraPaymentModel.Complete = true;
                return Result.Fail(BillPayModel.Res缴费结算?.code ?? -100, BillPayModel.Res缴费结算?.msg);
            }).Result;
        }
        private void PasswordNotify()
        {
            var sm = GetInstance<IShellViewModel>();
            if (!sm.Busy.IsBusy)
            {
                sm.Busy.IsBusy = true;
            }
            sm.Busy.BusyContent = "请输入6位社保卡密码，并按“确认”键";
            PlaySound("请输入社保卡密码并按确认键结束");
        }
        //社保结算（没有预结算）
        private Result<社保缴费结算> PayWithSi(LoadingProcesser lp)
        {
            lp.ChangeText("正在社保扣费，请稍后...");
            SiOperatorManager.StartMonitor(PasswordNotify);
            var patientInfo = PatientModel.当前病人信息;
            var record = BillRecordModel.所选缴费概要;
            var payrest = BillPayModel.Res缴费结算;
            var card = (CardModel as Auth.Models.CardModel);
            var expStr = $"N^6^^^^^^{card.SiCardInfo.社保读卡.账户余额}!{card.SiCardInfo.社保读卡.账户余额}^";
            var rowId = payrest.data.receiptNo;

            var ret = HisNative.HisInsuranceService.OPDivide(0, "1",rowId,"1","26",expStr,"");
            SiOperatorManager.StopMonitor();
            return ret;

        }

        //TODO 门诊号换成登记号
        protected override Queue<IPrintable> BillPayPrintables()
        {
            var queue = PrintManager.NewQueue("门诊费用缴费");
            var billPay = BillPayModel.Res缴费结算.data;
            var patientInfo = PatientModel.Res病人信息查询.data[PatientModel.PatientInfoIndex];
            var record = BillRecordModel.所选缴费概要;
            var sb = new StringBuilder();
            sb.Append($"状态：缴费成功\n");
            sb.Append($"姓名：{patientInfo.name}\n");
            sb.Append($"登记号：{patientInfo.patientId}\n");
            sb.Append($"交易类型：自助缴费\n");
            sb.Append($"金额总计：{PaymentModel.Total.In元()}\n");
            sb.Append($"支付方式：{PaymentModel.PayMethod}\n");
            //sb.Append($"收据号：{billPay.receiptNo}\n");
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
            sb.Append($"收费项目明细：\n");
            queue.Enqueue(new PrintItemText { Text = sb.ToString() });
            sb.Clear();
            queue.Enqueue(new PrintItemTriText("名称", "数量", "金额"));
            if (record?.billItem != null)
            {
                foreach (var detail in record.billItem)
                    queue.Enqueue(new PrintItemTriText(detail.itemName, detail.itemQty, detail.billFee.InRMB()));
            }
            sb.Append($"交易时间：{DateTimeCore.Now:yyyy-MM-dd HH:mm:ss}\n");
            sb.Append($"机器编号：{FrameworkConst.OperatorId}\n");
            sb.Append($"请妥善保管好您的缴费凭证\n");
            queue.Enqueue(new PrintItemText { Text = sb.ToString() });

            return queue;
        }
    }
}

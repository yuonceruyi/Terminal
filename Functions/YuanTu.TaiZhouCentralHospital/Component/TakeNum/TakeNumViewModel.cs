using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.Practices.Unity;
using YuanTu.Consts;
using YuanTu.Consts.Enums;
using YuanTu.Consts.FrameworkBase;
using YuanTu.Consts.Gateway;
using YuanTu.Consts.Models;
using YuanTu.Consts.Models.Payment;
using YuanTu.Consts.Models.Print;
using YuanTu.Core.Extension;
using YuanTu.TaiZhouCentralHospital.HealthInsurance.Model;
using YuanTu.TaiZhouCentralHospital.HealthInsurance.Service;

namespace YuanTu.TaiZhouCentralHospital.Component.TakeNum
{
    public class TakeNumViewModel : Default.Component.TakeNum.ViewModels.TakeNumViewModel
    {
        [Dependency]
        public ISiService SiService { get; set; }

        [Dependency]
        public ISiModel SiModel { get; set; }

        protected override void ConfirmAction()
        {
            var record = RecordModel.所选记录;
            ChangeNavigationContent(record.doctName);

            DoCommand(lp =>
            {
                if (CardModel.CardType == CardType.社保卡)
                {
                    //todo 医保预结算
                    lp.ChangeText("正在进行医保预结算,请稍候...");
                    var result = SiPreSettle();
                    if (!result.IsSuccess)
                    {
                        ShowAlert(false, "温馨提示", result.Message);
                        return;
                    }
                    var resOpRegPre = SiModel.医保预结算结果;
                    PaymentModel.Self = decimal.Parse(resOpRegPre.合计现金支付) * 100;
                    PaymentModel.Insurance = decimal.Parse(resOpRegPre.合计报销金额) * 100;
                    PaymentModel.Total = decimal.Parse(resOpRegPre.费用总额) * 100;
                    PaymentModel.NoPay = PaymentModel.Self == 0;
                    PaymentModel.ConfirmAction = Confirm;
                }
                else
                {
                    PaymentModel.Self = decimal.Parse(record.regAmount);
                    PaymentModel.Insurance = decimal.Parse("0");
                    PaymentModel.Total = decimal.Parse(record.regAmount);
                    PaymentModel.NoPay = PaymentModel.Self == 0;
                    PaymentModel.ConfirmAction = Confirm;
                }
                PaymentModel.LeftList = new List<PayInfoItem>
                {
                    new PayInfoItem("日期：", record.medDate),
                    new PayInfoItem("时间：", record.medAmPm.SafeToAmPm()),
                    new PayInfoItem("科室：", record.deptName),
                    new PayInfoItem("医生：", record.doctName)
                };

                PaymentModel.RightList = new List<PayInfoItem>
                {
                    new PayInfoItem("个人支付：", PaymentModel.Self.In元()),
                    new PayInfoItem("医保报销：", PaymentModel.Insurance.In元()),
                    new PayInfoItem("支付金额：", PaymentModel.Total.In元(), true)
                };

                Next();
            });
        }

        public virtual Result SiPreSettle()
        {
            //todo HIS提供医保预结算入参
            var reqOpRegPre = new req预约挂号预处理
            {
                cardType = ((int) CardModel.CardType).ToString(),
                cardNo = CardModel.CardNo,
                regDate = RecordModel.所选记录.medDate,
                regType = null,
                medAmPm = RecordModel.所选记录.medAmPm,
                deptCode = RecordModel.所选记录.deptCode,
                doctCode = RecordModel.所选记录.doctCode,
                appoNo = RecordModel.所选记录.appoNo,
                patientId = PatientModel.当前病人信息.patientId,
                extend = $"{SiModel.SiPatientInfo}"
            };
            var resOpRegPre = DataHandlerEx.预约挂号预处理(reqOpRegPre);
            if (!resOpRegPre.success)
                return Result.Fail($"门诊挂号预处理失败:{resOpRegPre.msg}");
            SiModel.门诊挂号预处理结果 = resOpRegPre.data;

            //todo 医保预结算

            var result = SiService.OpPreSettle(SiModel.门诊挂号预处理结果.regFee);
            if (!result.IsSuccess)
                return Result.Fail($"门诊挂号医保预结算失败:{result.Message}");
            return Result.Success();
        }

        protected override void CancelAction()
        {
            var record = RecordModel.所选记录;
            var textblock = new TextBlock
            {
                TextWrapping = TextWrapping.Wrap,
                TextAlignment = TextAlignment.Center,
                Margin = new Thickness(0, 15, 0, 0)
            };
            textblock.Inlines.Add("\r\n您确定要取消");
            textblock.Inlines.Add(new TextBlock
            {
                Text = $"{record.medDate} {record.medAmPm.SafeToAmPm()} {record.deptName}",
                Foreground = new SolidColorBrush(Color.FromRgb(255, 0, 0))
            });
            textblock.Inlines.Add("\r\n的预约吗？\r\n\r\n\r\n\r\n");
            ShowConfirm("友好提醒", textblock, b =>
            {
                if (!b) return;
                DoCommand(lp =>
                {
                    lp.ChangeText("正在进行取消预约，请稍候...");

                    var patientInfo = PatientModel.Res病人信息查询.data[PatientModel.PatientInfoIndex];

                    CancelAppoModel.Req取消预约 = new req取消预约
                    {
                        appoNo = record.appoNo,
                        orderNo = record.orderNo,
                        patientId = patientInfo.patientId,
                        operId = FrameworkConst.OperatorId,
                        regMode = "1",
                        cardNo = CardModel.CardNo,
                        cardType = ((int) CardModel.CardType).ToString(),
#pragma warning disable 612
                        medDate = record.medDate,
                        scheduleId = record.scheduleId,
                        medAmPm = record.medAmPm,
                        regNo = record.regNo,
#pragma warning restore 612
                        extend = $"{record.deptCode}|{record.doctCode}"
                    };
                    CancelAppoModel.Res取消预约 = DataHandlerEx.取消预约(CancelAppoModel.Req取消预约);
                    if (CancelAppoModel.Res取消预约?.success ?? false)
                    {
                        ShowAlert(true, "取消预约", "您已取消预约成功");
                        RecordModel.Res挂号预约记录查询.data.Remove(RecordModel.所选记录);
                        Switch(A.QuHao_Context, RecordModel.Res挂号预约记录查询.data.Count > 0 ? A.QH.Record : A.Home);
                    }
                    else
                    {
                        ShowAlert(false, "取消预约", "取消预约失败", debugInfo: CancelAppoModel.Res取消预约?.msg);
                    }
                });
            }, 60, ConfirmExModel.Build("是", "否", true));
        }

        protected override Result Confirm()
        {
            return DoCommand(lp =>
            {
                if (CardModel.CardType == CardType.社保卡)
                {
                    lp.ChangeText("正在进行医保结算,请稍候...");
                    //todo 医保结算
                    var result = SiService.OpSettle(SiModel.门诊挂号预处理结果.treatFee);
                    if (!result.IsSuccess)
                        return Result.Fail($"门诊缴费医保结算失败:{result.Message}");
                }

                lp.ChangeText("正在进行取号，请稍候...");

                var patientInfo = PatientModel.当前病人信息;
                var record = RecordModel.所选记录;

                TakeNumModel.Req预约取号 = new req预约取号
                {
                    patientId = patientInfo.patientId,
                    cardType = record.deptCode,
                    cardNo = record.doctCode,
                    appoNo = record.appoNo,
                    orderNo = record.orderNo,
                    operId = FrameworkConst.OperatorId,
                    tradeMode = PaymentModel.PayMethod.GetEnumDescription(),
                    accountNo = patientInfo.patientId,
                    cash = PaymentModel.Total.ToString(),
#pragma warning disable 612
                    medDate = record.medDate,
                    scheduleId = record.scheduleId,
                    medAmPm = record.medAmPm,
#pragma warning restore 612
                    extend = $"{patientInfo.patientType}&{record.medAmPm}&{record.medDate}&{SiModel.医保结算结果字符串}"
                };
                FillRechargeRequest(TakeNumModel.Req预约取号);
                TakeNumModel.Res预约取号 = DataHandlerEx.预约取号(TakeNumModel.Req预约取号);
                if (TakeNumModel.Res预约取号?.success ?? false)
                {
                    ExtraPaymentModel.Complete = true;

                    PrintModel.SetPrintInfo(true, new PrintInfo
                    {
                        TypeMsg = "取号成功",
                        TipMsg = $"您已成功于{DateTimeCore.Now.ToString("HH:mm")}分取号",
                        PrinterName = ConfigurationManager.GetValue("Printer:Receipt"),
                        Printables = TakeNumPrintables(),
                        TipImage = "提示_凭条"
                    });

                    Navigate(A.QH.Print);
                    return Result.Success();
                }
                //第三方支付失败时去支付流程里面处理，不在业务里面处理
                if (NavigationEngine.State != A.Third.PosUnion)
                {
                    PrintModel.SetPrintInfo(false, new PrintInfo
                    {
                        TypeMsg = "取号失败",
                        DebugInfo = TakeNumModel.Res预约取号?.msg
                    });
                    Navigate(A.QH.Print);
                }

                ExtraPaymentModel.Complete = true;

                return Result.Fail(TakeNumModel.Res预约取号?.code ?? -100, TakeNumModel.Res预约取号?.msg);
            }).Result;
        }
    }
}
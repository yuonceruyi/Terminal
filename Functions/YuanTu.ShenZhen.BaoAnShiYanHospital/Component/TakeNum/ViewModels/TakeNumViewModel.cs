using Microsoft.Practices.Unity;
using Prism.Commands;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using YuanTu.Consts;
using YuanTu.Consts.Dtos.UnionPos;
using YuanTu.Consts.Enums;
using YuanTu.Consts.FrameworkBase;
using YuanTu.Consts.Gateway;
using YuanTu.Consts.Models;
using YuanTu.Consts.Models.Auth;
using YuanTu.Consts.Models.Payment;
using YuanTu.Consts.Models.Print;
using YuanTu.Consts.Models.TakeNum;
using YuanTu.Consts.Services;
using YuanTu.Core.Extension;
using YuanTu.Core.FrameworkBase;
using YuanTu.Core.Navigating;
using YuanTu.Default.Component.Tools.Models;
using YuanTu.ShenZhenArea.Enums;

namespace YuanTu.ShenZhen.BaoAnShiYanHospital.Component.TakeNum.ViewModels
{
    public class TakeNumViewModel : YuanTu.Default.Component.TakeNum.ViewModels.TakeNumViewModel
    {

        protected override void ConfirmAction()
        {
            var record = RecordModel.所选记录;
            ChangeNavigationContent(record.doctName);

            PaymentModel.Self = decimal.Parse(record.regAmount);
            PaymentModel.Insurance = decimal.Parse("0");
            PaymentModel.Total = decimal.Parse(record.regAmount);
            PaymentModel.NoPay = true;
            PaymentModel.PayMethod = PayMethod.现金;
            //PaymentModel.NoPay = PaymentModel.Self==0;
            PaymentModel.ConfirmAction = Confirm;

            PaymentModel.LeftList = new List<PayInfoItem>()
            {
                new PayInfoItem("日期：",record.medDate),
                new PayInfoItem("时间：",record.medAmPm.SafeToAmPm()),
                new PayInfoItem("科室：",record.deptName),
                new PayInfoItem("医生：",record.doctName),
            };

            PaymentModel.RightList = new List<PayInfoItem>()
            {
                new PayInfoItem("个人支付：",PaymentModel.Self.In元()),
                new PayInfoItem("医保报销：",PaymentModel.Insurance.In元()),
                new PayInfoItem("支付金额：",PaymentModel.Total.In元(),true),
            };

            Next();
        }

        protected override Result Confirm()
        {
            return DoCommand(lp =>
            {
                lp.ChangeText("正在进行取号，请稍候...");

                var patientInfo = PatientModel.当前病人信息;
                var record = RecordModel.所选记录;

                TakeNumModel.Req预约取号 = new req预约取号
                {
                    patientId = patientInfo.patientId,
                    cardType = ((int)CardModel.CardType).ToString(),
                    cardNo = CardModel.CardNo,
                    appoNo = RecordModel.所选记录.appoNo,
                    searchType = ((int)regMode.预约挂号).ToString(),
                    orderNo = RecordModel.所选记录.orderNo,
                    operId = FrameworkConst.OperatorId,
                    tradeMode = PaymentModel.PayMethod.GetEnumDescription(),
                    accountNo = patientInfo.patientId,
                    cash = PaymentModel.Total.ToString(),
                    medDate = record.medDate,
                    scheduleId = record.medAmPm,
                    medAmPm = record.medAmPm
                };
                FillRechargeRequest(TakeNumModel.Req预约取号);
                TakeNumModel.Res预约取号 = DataHandlerEx.预约取号(TakeNumModel.Req预约取号);
                if (TakeNumModel.Res预约取号?.success ?? false)
                {
                    ExtraPaymentModel.Complete = true;
                    //PrintModel.SetPrintInfo(true, "取号成功", $"您已成功于{DateTimeCore.Now.ToString("HH:mm")}分取号",
                    //         ConfigurationManager.GetValue("Printer:Receipt"), TakeNumPrintables());
                    //PrintModel.SetPrintImage(ResourceEngine.GetImageResourceUri("提示_凭条"));
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
                else
                {
                    //第三方支付失败时去支付流程里面处理，不在业务里面处理
                    if (NavigationEngine.State != A.Third.PosUnion)
                    {
                        //PrintModel.SetPrintInfo(false, "取号失败", errorMsg: TakeNumModel.Res预约取号?.msg);
                        PrintModel.SetPrintInfo(false, new PrintInfo
                        {
                            TypeMsg = "取号失败",
                            DebugInfo= TakeNumModel.Res预约取号?.msg
                        });
                        Navigate(A.QH.Print);
                    }

                    ExtraPaymentModel.Complete = true;

                    return Result.Fail(TakeNumModel.Res预约取号?.code??-100, TakeNumModel.Res预约取号?.msg);
                }
            }).Result;
        }

        protected override void FillRechargeRequest(req预约取号 req)
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
            else if (extraPaymentModel.CurrentPayMethod == PayMethod.支付宝 || extraPaymentModel.CurrentPayMethod == PayMethod.微信支付)
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
        protected override Queue<IPrintable> TakeNumPrintables()
        {
            var queue = PrintManager.NewQueue("取号单");
            var patientInfo = PatientModel.Res病人信息查询.data[PatientModel.PatientInfoIndex];
            var takeNum = TakeNumModel.Res预约取号.data;
            var record = RecordModel.所选记录;
            var sb = new StringBuilder();
            sb.Append($"状态：取号成功\n");
            sb.Append($"姓名：{patientInfo.name}\n");
            sb.Append($"门诊号：{patientInfo.patientId}\n");
            //sb.Append($"交易类型：预约取号\n");
            sb.Append($"科室名称：{record.deptName}\n");
            //sb.Append($"诊疗科室：{paiban.deptName}\n");
            sb.Append($"就诊医生：{record.doctName}\n");
            sb.Append($"挂号费：{record.regFee.In元()}\n");
            sb.Append($"诊疗费：{record.regAmount.In元()}\n");
            //sb.Append($"诊疗费：{record.treatFee.In元()}\n");
            //sb.Append($"挂号金额：{record.regAmount.In元()}\n");
            sb.Append($"就诊场次：{record.medAmPm.SafeToAmPm()}\n");
            //sb.Append($"就诊地址：{takeNum?.address}\n");
            //sb.Append($"挂号序号：{takeNum?.appoNo}\n");
            //sb.Append($"个人支付：{Convert.ToDouble(quhao.selfFee).In元()}\n");
            //sb.Append($"医保支付：{Convert.ToDouble(quhao.insurFee).In元()}\n");
            sb.Append($"交易时间：{DateTimeCore.Now.ToString("yyyy-MM-dd HH:mm:ss")}\n");
            sb.Append($"柜员编号：{FrameworkConst.OperatorId}\n");
            sb.Append($"祝您早日康复！\n");
            queue.Enqueue(new PrintItemText { Text = sb.ToString() });
            return queue;
        }


    }
}
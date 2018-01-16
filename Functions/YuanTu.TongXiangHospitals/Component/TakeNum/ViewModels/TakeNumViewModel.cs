using Microsoft.Practices.Unity;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using YuanTu.Consts;
using YuanTu.Consts.Enums;
using YuanTu.Consts.FrameworkBase;
using YuanTu.Consts.Gateway;
using YuanTu.Consts.Models;
using YuanTu.Consts.Models.Payment;
using YuanTu.Consts.Models.Print;
using YuanTu.Core.Extension;
using YuanTu.Core.Gateway;
using YuanTu.Core.Navigating;
using YuanTu.TongXiangHospitals.HealthInsurance;
using YuanTu.TongXiangHospitals.HealthInsurance.Model;
using YuanTu.TongXiangHospitals.HealthInsurance.Service;

namespace YuanTu.TongXiangHospitals.Component.TakeNum.ViewModels
{
    public class TakeNumViewModel : Default.Component.TakeNum.ViewModels.TakeNumViewModel
    {
        [Dependency]
        public IChoiceModel ChoiceModel { get; set; }

        [Dependency]
        public ISiService SiService { get; set; }

        [Dependency]
        public ISiModel SiModel { get; set; }

        protected override void ConfirmAction()
        {
            var record = RecordModel.所选记录;
            ChangeNavigationContent(record.doctName);
            SiModel.诊间结算 = false;

            DoCommand(ctx =>
            {
                ctx.ChangeText("正在进行门诊挂号预结算，请稍候...");
                var result = PreSettle();
                if (!result.IsSuccess)
                    return;
                //SiModel.PreSettleAction = PreSettle;
                Next();
            });
           
        }

        public virtual Result PreSettle()
        {
            var record = RecordModel.所选记录;
            if (CardModel.CardType == CardType.社保卡)
            {
                var result = SiPreSettle();
                if (!result.IsSuccess)
                {
                    return Result.Fail(result.Message);
                }
                var resOpRegPre = SiModel.门诊挂号预结算结果确认结果;
                PaymentModel.Self = decimal.Parse(resOpRegPre.selfFee);
                PaymentModel.Insurance = decimal.Parse(resOpRegPre.insurFee);
                PaymentModel.Total = decimal.Parse(record.regAmount);
                PaymentModel.NoPay = PaymentModel.Self==0;
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
                new PayInfoItem("总计金额：",PaymentModel.Total.In元(),true),
            };
            return Result.Success();
        }

        public virtual Result SiPreSettle()
        {
            //DoCommand(ctx =>
            //{
            //    //todo 请求HIS预结算
            //    ctx.ChangeText("正在进行门诊挂号预结算，请稍候...");
                var reqOpRegPre = new req门诊挂号预结算
                {
                    patientId = PatientModel.当前病人信息.patientId,
                    cardNo = CardModel.CardNo,
                    cardType = ((int)CardModel.CardType).ToString(),
                    regMode = ChoiceModel.Business == Business.挂号 ? "2" : "1",
                    regType = null,
                    medDate = RecordModel.所选记录.medDate,
                    scheduleId = RecordModel.所选记录.scheduleId,
                    deptCode = RecordModel.所选记录.deptCode,
                    doctCode = RecordModel.所选记录.doctCode,
                    medAmPm = RecordModel.所选记录.medAmPm,
                    cash = RecordModel.所选记录.regAmount,
                    ybCardNo = CardModel.CardNo,
                    ybTradeType = SiModel.医保个人基本信息?.医保待遇类别,
                    isZj = SiModel.诊间结算 ? "1" : "0",
                    cardHardInfo = SiModel.CardHardInfo,
                    siPatientInfo = SiModel.SiPatientInfo
                };
                var resOpRegPre = DataHandlerEx.门诊挂号预结算(reqOpRegPre);
                if (!resOpRegPre.success)
                {
                    ShowAlert(false, "门诊挂号预结算", "门诊挂号预结算失败", debugInfo: resOpRegPre.msg);
                    return Result.Fail( resOpRegPre.msg);
                }
                SiModel.门诊挂号预结算结果 = resOpRegPre.data;

                //todo 医保预结算
                //ctx.ChangeText("正在进行医保预结算，请稍候...");
                var result = SiModel.诊间结算 ? SiService.OpRegClinicPreSettle( SiModel.门诊挂号预结算结果.insurFeeInfo)
                                              : SiService.OpRegPreSettle( SiModel.门诊挂号预结算结果.insurFeeInfo);
                if (!result.IsSuccess)
                {
                    ShowAlert(false, "门诊挂号预结算", "门诊挂号医保预结算失败", debugInfo: result.Message);
                    return Result.Fail( result.Message);
                }

                //todo HIS预结算确认
                //ctx.ChangeText("正在进行门诊挂号预结算结果确认，请稍候...");
                var reqOpRegPreConfirm = new req门诊挂号预结算结果确认
                {
                    patientId = PatientModel.当前病人信息.patientId,
                    cardNo = CardModel.CardNo,
                    cardType = ((int)CardModel.CardType).ToString(),
                    regMode = ChoiceModel.Business == Business.挂号 ? "2" : "1",
                    regType = null,
                    medDate = RecordModel.所选记录.medDate,
                    scheduleId = RecordModel.所选记录.scheduleId,
                    deptCode = RecordModel.所选记录.deptCode,
                    doctCode = RecordModel.所选记录.doctCode,
                    medAmPm = RecordModel.所选记录.medAmPm,
                    cash = RecordModel.所选记录.regAmount,
                    ybCardNo = CardModel.CardNo,
                    ybTradeType = SiModel.医保个人基本信息?.医保待遇类别,
                    isZj = SiModel.诊间结算 ? "1" : "0",
                    insurFeeInfo = SiModel.RetMessage,
                    cardHardInfo = SiModel.CardHardInfo,
                    siPatientInfo = SiModel.SiPatientInfo
                };
                var resOpRegPreConfirm = DataHandlerEx.门诊挂号预结算结果确认(reqOpRegPreConfirm);
                if (!resOpRegPreConfirm.success)
                {
                    ShowAlert(false, "门诊挂号预结算", "门诊挂号预结算结果确认失败", debugInfo: resOpRegPreConfirm.msg);
                    return Result.Fail( resOpRegPreConfirm.msg);
                }
                SiModel.门诊挂号预结算结果确认结果 = resOpRegPreConfirm.data;
                return Result.Success();
            //});
            
        }

        protected virtual Result GetSiSettleReq()
        {
            var reqOpRegPre = new req门诊挂号预结算
            {
                patientId = PatientModel.当前病人信息.patientId,
                cardNo = CardModel.CardNo,
                cardType = ((int)CardModel.CardType).ToString(),
                regMode = ChoiceModel.Business == Business.挂号 ? "2" : "1",
                regType = null,
                medDate = RecordModel.所选记录.medDate,
                scheduleId = RecordModel.所选记录.scheduleId,
                deptCode = RecordModel.所选记录.deptCode,
                doctCode = RecordModel.所选记录.doctCode,
                medAmPm = RecordModel.所选记录.medAmPm,
                cash = RecordModel.所选记录.regAmount,
                ybCardNo = CardModel.CardNo,
                ybTradeType = SiModel.医保个人基本信息?.医保待遇类别,
                isZj = SiModel.诊间结算 ? "1" : "0",
                cardHardInfo = SiModel.CardHardInfo,
                siPatientInfo = SiModel.SiPatientInfo
            };
            var resOpRegPre = DataHandlerEx.门诊挂号预结算(reqOpRegPre);
            if (!resOpRegPre.success)
            {
                ShowAlert(false, "门诊挂号预结算", "门诊挂号预结算失败", debugInfo: resOpRegPre.msg);
                return Result.Fail(resOpRegPre.msg);
            }
            SiModel.门诊挂号预结算结果 = resOpRegPre.data;

            //todo 医保预结算
            
            var result = SiModel.诊间结算 ? SiService.OpRegClinicPreSettle( SiModel.门诊挂号预结算结果.insurFeeInfo)
                                          : SiService.OpRegPreSettle( SiModel.门诊挂号预结算结果.insurFeeInfo);
            if (!result.IsSuccess)
            {
                ShowAlert(false, "门诊挂号预结算", "门诊挂号医保预结算失败", debugInfo: result.Message);
                return Result.Fail(result.Message);
            }

            //todo HIS预结算确认
            
            var reqOpRegPreConfirm = new req门诊挂号预结算结果确认
            {
                patientId = PatientModel.当前病人信息.patientId,
                cardNo = CardModel.CardNo,
                cardType = ((int)CardModel.CardType).ToString(),
                regMode = ChoiceModel.Business == Business.挂号 ? "2" : "1",
                regType = null,
                medDate = RecordModel.所选记录.medDate,
                scheduleId = RecordModel.所选记录.scheduleId,
                deptCode = RecordModel.所选记录.deptCode,
                doctCode = RecordModel.所选记录.doctCode,
                medAmPm = RecordModel.所选记录.medAmPm,
                cash = RecordModel.所选记录.regAmount,
                ybCardNo = CardModel.CardNo,
                ybTradeType = SiModel.医保个人基本信息?.医保待遇类别,
                isZj = SiModel.诊间结算 ? "1" : "0",
                insurFeeInfo = SiModel.RetMessage,
                cardHardInfo = SiModel.CardHardInfo,
                siPatientInfo = SiModel.SiPatientInfo
            };
            var resOpRegPreConfirm = DataHandlerEx.门诊挂号预结算结果确认(reqOpRegPreConfirm);
            if (!resOpRegPreConfirm.success)
            {
                ShowAlert(false, "门诊挂号预结算", "门诊挂号预结算结果确认失败", debugInfo: resOpRegPreConfirm.msg);
                return Result.Fail(resOpRegPreConfirm.msg);
            }
            SiModel.门诊挂号预结算结果确认结果 = resOpRegPreConfirm.data;
            return Result.Success();
        }
        protected override Result Confirm()
        {
            return DoCommand(lp =>
            {
                //todo 医保结算

                if (CardModel.CardType == CardType.社保卡)
                {
                    lp.ChangeText("正在进行医保结算，请稍候...");
                    if (SiModel.诊间结算)
                    {
                        var res = GetSiSettleReq();
                        if (!res.IsSuccess)
                            return Result.Fail(res.Message);
                    }
                    var result = SiModel.诊间结算 ? SiService.OpRegClinicSettle( SiModel.门诊挂号预结算结果确认结果?.insurFeeInfo)
                                                  : SiService.OpRegSettle( SiModel.门诊挂号预结算结果确认结果?.insurFeeInfo);
                    if (!result.IsSuccess)
                    {
                        ShowAlert(false, "门诊挂号医保结算", "门诊挂号医保结算失败", debugInfo: result.Message);
                        return Result.Fail( result.Message);
                    }
                }

                lp.ChangeText("正在进行取号，请稍候...");

                var patientInfo = PatientModel.当前病人信息;
                var record = RecordModel.所选记录;

                TakeNumModel.Req预约取号 = new req预约取号
                {
                    patientId = patientInfo.patientId,
                    cardType = ((int)CardModel.CardType).ToString(),
                    cardNo = CardModel.CardNo,

                    appoNo = record.appoNo,
                    orderNo = record.orderNo,

                    operId = FrameworkConst.OperatorId,
                    tradeMode = GetPayMethod(PaymentModel.PayMethod),
                    accountNo = patientInfo.patientId,
                    cash = (Startup.TestRefund ? PaymentModel.Self + 1 : PaymentModel.Self).ToString(CultureInfo.InvariantCulture),
#pragma warning disable 612
                    medDate = record.medDate,
                    scheduleId = record.scheduleId,
                    medAmPm = record.medAmPm
#pragma warning restore 612
                };
                FillRechargeRequest(TakeNumModel.Req预约取号);

                if (CardModel.CardType == CardType.社保卡)
                    FillSiRequest(TakeNumModel.Req预约取号);

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
                if (SiModel.诊间结算 && CardModel.CardType == CardType.社保卡)
                {
                    if (DataHandler.UnKnowErrorCode.Any(p=>p==TakeNumModel.Res预约取号?.code))
                    {
                        //todo 打印医保单边账凭条
                        if (PaymentModel.Insurance > 0)
                        {
                            var errorMsg = $"医保消费成功，网关返回未知结果{TakeNumModel.Res预约取号?.code.ToString()}，打印凭条结束交易！\n请执凭条到人工咨询此交易结果！";
                            医保单边账凭证(errorMsg);
                        }
                    }
                    else if (TakeNumModel.Res预约取号?.data?.extend != null)
                    {
                        //todo 医保退费
                        var result = SiService.OpRegClinicSettleRefund( TakeNumModel.Res预约取号?.data?.extend);
                        if (!result.IsSuccess)
                        {
                            //todo 医保退费失败处理
                            ShowAlert(false, "医保门诊挂号诊间结算退费", "医保门诊挂号诊间结算退费失败", debugInfo: result.Message);
                        }
                    }
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

        public virtual string GetPayMethod(PayMethod payMethod)
        {
            return payMethod == PayMethod.预缴金 ? "SMK" : payMethod.GetEnumDescription();
        }

        public virtual void FillSiRequest(req预约取号 req)
        {
            req.extend = SiModel.RetMessage;
            req.ybInfo = new SiInfo
            {
                cardHardInfo = SiModel.CardHardInfo,
                siPatientInfo = SiModel.SiPatientInfo,
                transNo = SiModel.门诊挂号预结算结果确认结果?.transNo
            }.ToJsonString();
            //req.transNo = SiModel.门诊挂号预结算结果确认结果?.transNo;
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
                Text = $"{ record.medDate } { record.medAmPm.SafeToAmPm() } { record.deptName }",
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
                        orderNo = record.orderNo,
                        appoNo = record.appoNo,
                        patientId = patientInfo.patientId,
                        operId = FrameworkConst.OperatorId,
                        regMode = "1",
                        cardNo = CardModel.CardNo,
                        cardType = ((int)CardModel.CardType).ToString(),
#pragma warning disable 612
                        medDate = record.medDate,
                        scheduleId = record.scheduleId,
                        medAmPm = record.medAmPm,
                        regNo = record.regNo
#pragma warning restore 612
                    };
                    CancelAppoModel.Res取消预约 = DataHandlerEx.取消预约(CancelAppoModel.Req取消预约);
                    if (CancelAppoModel.Res取消预约?.success ?? false)
                    {
                        ShowAlert(true, "取消预约", "您已取消预约成功");
                        RecordModel.Res挂号预约记录查询.data.Remove(RecordModel.所选记录);
                        Navigate(RecordModel.Res挂号预约记录查询.data.Count > 0 ? A.QH.Record : A.Home);
                    }
                    else
                    {
                        ShowAlert(false, "取消预约", "取消预约失败", debugInfo: CancelAppoModel.Res取消预约?.msg);
                    }
                });
            }, 60, ConfirmExModel.Build("是", "否", true));
        }

        protected virtual void 医保单边账凭证(string errorMsg)
        {
            var queue = PrintManager.NewQueue($"医保{ExtraPaymentModel.CurrentBusiness}单边账");
            var sb = new StringBuilder();
            sb.Append($"姓名：{ExtraPaymentModel.PatientInfo?.Name}\n");
            sb.Append($"门诊号：{ExtraPaymentModel.PatientInfo?.PatientId}\n");
            sb.Append($"当前业务：{ExtraPaymentModel.CurrentBusiness}\n");
            sb.Append($"排班编号：{RecordModel.所选记录.scheduleId}\n");
            sb.Append($"医保金额：{PaymentModel?.Insurance.In元()}\n");
            sb.Append($"异常描述：{errorMsg}\n");
            sb.Append($"交易时间：{DateTimeCore.Now.ToString("yyyy-MM-dd HH:mm:ss")}\n");
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
        protected override Queue<IPrintable> TakeNumPrintables()
        {
            var queue = PrintManager.NewQueue("取号单");
            var patientInfo = PatientModel.Res病人信息查询.data[PatientModel.PatientInfoIndex];
            var takeNum = TakeNumModel.Res预约取号.data;
            var record = RecordModel.所选记录;
            var preReg = SiModel.门诊挂号预结算结果确认结果;
            var sb = new StringBuilder();
            sb.Append($"状态：取号成功\n");
            sb.Append($"姓名：{patientInfo.name}\n");
            sb.Append($"门诊号码：{patientInfo.platformId}\n");
            sb.Append($"交易类型：预约取号\n");
            sb.Append($"科室名称：{record.deptName}\n");
            sb.Append($"就诊医生：{record.doctName}\n");
            sb.Append($"挂号金额：{record.regAmount.In元()}\n");
            if (preReg != null)
            {
                sb.Append($"个人支付：{preReg?.selfFee.In元()}\n");
                sb.Append($"医保报销：{preReg?.insurFee.In元()}\n");
            }
            sb.Append($"就诊场次：{record.medAmPm.SafeToAmPm()}\n");
            sb.Append($"排队号：{takeNum?.visitNo}\n");
            sb.Append($"就诊地址：{takeNum?.address}\n");
            sb.Append($"流水号：{takeNum?.appoNo}\n");
            sb.Append($"发票号：{takeNum?.receiptNo}\n");
            sb.Append($"交易时间：{DateTimeCore.Now.ToString("yyyy-MM-dd HH:mm:ss")}\n");
            sb.Append($"柜员编号：{FrameworkConst.OperatorId}\n");
            sb.Append($"祝您早日康复！\n");
            queue.Enqueue(new PrintItemText { Text = sb.ToString() });
            return queue;
        }
    }
}
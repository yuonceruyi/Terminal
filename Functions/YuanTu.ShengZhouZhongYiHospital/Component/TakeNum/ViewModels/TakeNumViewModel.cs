using Microsoft.Practices.ServiceLocation;
using Microsoft.Practices.Unity;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Prism.Commands;
using YuanTu.Consts;
using YuanTu.Consts.Dtos.UnionPos;
using YuanTu.Consts.Enums;
using YuanTu.Consts.FrameworkBase;
using YuanTu.Consts.Gateway;
using YuanTu.Consts.Models;
using YuanTu.Consts.Models.Payment;
using YuanTu.Consts.Models.Print;
using YuanTu.Consts.Models.Register;
using YuanTu.Consts.Services;
using YuanTu.Core.Extension;
using YuanTu.Core.FrameworkBase.Loadings;
using YuanTu.Core.Log;
using YuanTu.Core.Navigating;
using YuanTu.Core.Reporter;
using YuanTu.Core.Services.PrintService;
using YuanTu.Default.Component.Tools.Models;
using YuanTu.ShengZhouZhongYiHospital.Component.TakeNum.Models;
using YuanTu.ShengZhouZhongYiHospital.Extension;
using YuanTu.ShengZhouZhongYiHospital.HisNative.Models;
using Color = System.Windows.Media.Color;
using FontStyle = System.Windows.FontStyle;

namespace YuanTu.ShengZhouZhongYiHospital.Component.TakeNum.ViewModels
{
    public class TakeNumViewModel : YuanTu.Default.Component.TakeNum.ViewModels.TakeNumViewModel
    {

        public TakeNumViewModel()
        {
            
            PreviewCommand=new DelegateCommand(PreviewCmd);
        }

        [Dependency]
        public IConfigurationManager Manager { get; set; }

        [Dependency]
        public IBusinessConfigManager BusinessConfigManager { get; set; }


        [Dependency]
        public IDeptartmentModel DepartmentModel { get; set; }

        [Dependency]
        public IScheduleModel ScheduleModel { get; set; }

        public ICommand PreviewCommand { get; set; }

        private void PreviewCmd()
        {
            if (NavigationEngine.State==A.QH.TakeNum)
            {
                if (NavigationEngine.HasPrev(NavigationEngine.Current))
                {
                    NavigationEngine.Prev();
                }
                else
                {
                    NavigationEngine.Navigate(NavigationEngine.HomeAddress);
                }
            }
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
                        orderNo = record.orderNo,
                        patientId = patientInfo.patientId,
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

        protected override void ConfirmAction()
        {
          
            DoCommand(lp =>
            {
               
                var tnModel = TakeNumModel as TakeNumModel;
                var record = RecordModel.所选记录;
                if (!JudgeCanConfirm())
                {
                    ShowAlert(false,"取号失败",$"您只能在【{record.medDate}】当天取号就诊！");
                    return;
                }

                ChangeNavigationContent(record.doctName);
                if (挂号结算And退号(lp).IsSuccess)
                {
                    var yuJieSuan = tnModel.Res挂号取号预结算;
                    PaymentModel.Self = decimal.Parse(yuJieSuan.个人现金支付金额) * 100;
                    PaymentModel.Total = decimal.Parse(yuJieSuan.总金额) * 100;
                    PaymentModel.Insurance = decimal.Parse(yuJieSuan.医保支付金额) * 100;
                    PaymentModel.NoPay = PaymentModel.Self == 0;
                    PaymentModel.ConfirmAction = Confirm;

                    PaymentModel.LeftList = new List<PayInfoItem>()
                    {
                        new PayInfoItem("日期：", record.medDate),
                        new PayInfoItem("时间：", record.medAmPm.SafeToAmPm()),
                        new PayInfoItem("科室：", record.deptName),
                    };

                    PaymentModel.RightList = new List<PayInfoItem>()
                    {
                        new PayInfoItem("医保支付：", PaymentModel.Insurance.In元()),
                        new PayInfoItem("个人支付：", PaymentModel.Self.In元()),
                        new PayInfoItem("支付金额：", PaymentModel.Self.In元(), true),
                    };
                    Next();
                }
            });
        }

        protected override bool CanConfirm()
        {
            return true;
          
        }

        protected virtual bool JudgeCanConfirm()
        {
            var record = RecordModel.所选记录;
            var realTime = record.medDate + " " + record.medTime;
            DateTime medTime;
            if (DateTime.TryParseExact(realTime, "yyyy-MM-dd HH:mm", null, DateTimeStyles.AllowWhiteSpaces, out medTime))
            {
                return DateTimeCore.Today == medTime.Date && DateTimeCore.Now < medTime;
            }
            else if (DateTime.TryParseExact(record.medDate, "yyyy-MM-dd", null, DateTimeStyles.AllowWhiteSpaces, out medTime))
            {
                return DateTimeCore.Today == medTime.Date;
            }
            return true;
        }

        protected override Result Confirm()
        {



            return DoCommand(lp =>
            {
                lp.ChangeText("正在进行取号结算，请稍候...");
                var extraPaymentModel = GetInstance<IExtraPaymentModel>();
                var tranNo = string.Empty;
                var tnModel = TakeNumModel as TakeNumModel;
                int ZHIFULX = 1;
                if (PaymentModel.PayMethod == PayMethod.银联)
                {
                    tranNo = (extraPaymentModel.PaymentResult as TransResDto).Ref;
                    ZHIFULX = 1;
                }
                else if (PaymentModel.PayMethod == PayMethod.支付宝)
                {
                    tranNo = (extraPaymentModel.PaymentResult as 订单状态).outTradeNo.Substring(4);
                    ZHIFULX = 2;
                }
                else if (PaymentModel.PayMethod == PayMethod.微信支付)
                {
                    tranNo = (extraPaymentModel.PaymentResult as 订单状态).outTradeNo.Substring(4);
                    ZHIFULX = 3;
                }
                else
                {
                    tranNo = BusinessConfigManager.GetFlowId("取号");
                    ZHIFULX = 1;
                }

                tnModel.Req挂号取号结算 = new Req挂号取号结算()
                {
                    患者唯一标识 = PatientModel.当前病人信息.patientId,
                    姓名 = PatientModel.当前病人信息.name,
                    排班表主键 = RecordModel.所选记录.scheduleId,
                    科室编号 = RecordModel.所选记录.deptCode,
                    医生工号 = "", //ScheduleModel.所选排班.doctCode,
                    挂号类型 = "1",
                    挂号时间 = DateTimeCore.Now.ToString("yyyy-MM-dd"),
                    医保类型 = "", //PatientModel.当前病人信息.accountNo,
                    预约标记 = "1",
                    预约记录主键 = RecordModel.所选记录.orderNo,
                    挂号费 = "0.00",
                    诊疗费 = PaymentModel.Total.In元(),
                    工本费 = "0",
                    就诊序号 = "0",
                    挂号序号 = "0",
                    现金结算单流水号 = tranNo, //tranNo.BackNotNullOrEmpty(BusinessConfigManager.GetFlowId("取号结算流水")),
                    现金结算金额 = "0",
                    总金额 = RecordModel.所选记录?.treatFee,
                    程序名 = "0",
                    操作科室 = "0",
                    终端编号 = FrameworkConst.OperatorId,
                    值班类别 = RecordModel.所选记录?.medAmPm,
                    支付类别 = ZHIFULX.ToString()
                };
                tnModel.Res挂号取号结算 = HisHandleEx.执行挂号取号结算(tnModel.Req挂号取号结算);
                if (tnModel.Res挂号取号结算.IsSuccess)
                {
                    PrintModel.SetPrintInfo(true, new PrintInfo
                    {
                        TypeMsg = "取号成功",
                        TipMsg = $"您已成功于{DateTimeCore.Now.ToString("HH:mm")}分取号",
                        PrinterName = ConfigurationManager.GetValue("Printer:Receipt"),
                        Printables = TakeNumPrintables(tnModel.Res挂号取号结算),
                        TipImage = "提示_凭条"
                    });
                    ExtraPaymentModel.Complete = true;
                    try
                    {
                        lp.ChangeText("正在交易数据上传,请稍后...");
                        上传数据到HIS();
                        if (PaymentModel.Self != 0)
                        {
                            自费交易记录同步到his系统();
                        }
                        if (PaymentModel.Insurance != 0)
                        {
                            医保交易记录同步到his系统();
                        }
                    }
                    catch (Exception e)
                    {
                        Logger.Net.Error($"数据统一上传失败:{e.Message}");
                    }

                    Navigate(A.QH.Print);
                    return Result.Success();
                }
                ReportService.HIS请求失败($"嵊州取号结算失败,HIS服务返回:{tnModel.Res挂号取号结算.原始报文}", null);
                ExtraPaymentModel.Complete = true;
                ShowAlert(false, "温馨提示", $"取号结算失败:{tnModel.Res挂号取号结算.Message}");
                return Result.Fail(tnModel.Res挂号取号结算?.RetCode ?? -100, tnModel.Res挂号取号结算.Message);

            }).Result;
        }

        private Result 取号预结算(LoadingProcesser lp)
        {
            var tnModel = TakeNumModel as TakeNumModel;



            lp.ChangeText("正在进行取号预结算，请稍候...");

            tnModel.Req挂号取号预结算 = new Req挂号取号预结算()
            {
                患者唯一标识 = PatientModel.当前病人信息.patientId,
                姓名 = PatientModel.当前病人信息.name,
                排班表主键 = RecordModel.所选记录.scheduleId,
                科室编号 = RecordModel.所选记录.deptCode,
                医生工号 = "", //ScheduleModel.所选排班.doctCode,
                挂号类型 = " ",
                挂号时间 = DateTimeCore.Now.ToString("yyyy-MM-dd"),
                医保类型 = "", //PatientModel.当前病人信息.accountNo,
                预约标记 = "1",
                预约记录主键 = RecordModel.所选记录.orderNo,
                挂号费 = "0.00",
                诊疗费 = "0.00",
                工本费 = "0",
                就诊序号 = "0",
                挂号序号 = "0",
                现金结算单流水号 = "",
                现金结算金额 = "0",
                总金额 = RecordModel.所选记录?.treatFee,
                程序名 = "0",
                操作科室 = "0",
                终端编号 = FrameworkConst.OperatorId,
                值班类别 = RecordModel.所选记录?.medAmPm,
                支付类别 = "1"
            };
            tnModel.Res挂号取号预结算 = HisHandleEx.执行挂号取号预结算(tnModel.Req挂号取号预结算);
            if (tnModel.Res挂号取号预结算.IsSuccess)
            {
                return Result.Success();
            }
            ShowAlert(false, "温馨提示", $"取号预结算失败:{tnModel.Res挂号取号预结算.Message}");
            return Result.Fail(tnModel.Res挂号取号预结算.Message);



        }

        private Result 挂号结算And退号(LoadingProcesser lp)
        {

            var ys = 取号预结算(lp);
            if (!ys.IsSuccess)
            {
                return ys;
            }
            var tnModel = TakeNumModel as TakeNumModel;
            if (tnModel.Res挂号取号预结算.医保类型 != "78") //农保（78）存在该问题
            {
                return ys;
            }

            var req = new Req挂号取号结算
            {
                患者唯一标识 = PatientModel.当前病人信息.patientId,
                姓名 = PatientModel.当前病人信息.name,
                排班表主键 = RecordModel.所选记录.scheduleId,
                科室编号 = RecordModel.所选记录.deptCode,
                医生工号 = "", //ScheduleModel.所选排班.doctCode,
                挂号类型 = "1",
                挂号时间 = DateTimeCore.Now.ToString("yyyy-MM-dd"),
                医保类型 = "", //PatientModel.当前病人信息.accountNo,
                预约标记 = "1",
                预约记录主键 = RecordModel.所选记录.orderNo,
                挂号费 = "0.00",
                诊疗费 = PaymentModel.Total.In元(),
                工本费 = "0",
                就诊序号 = "0",
                挂号序号 = "0",
                现金结算单流水号 = "SB0001",
                现金结算金额 = "0",
                总金额 = RecordModel.所选记录?.treatFee,
                程序名 = "0",
                操作科室 = "0",
                终端编号 = FrameworkConst.OperatorId,
                值班类别 = RecordModel.所选记录?.medAmPm,
                支付类别 = "1"
            };

            var ret = HisHandleEx.执行挂号取号结算(req);
            if (!ret.IsSuccess)
            {
                ShowAlert(false, "计算费用失败", ret.Message);
                return Result.Fail(ret.Message);
            }

            //回滚

            var reqRefund = new Req挂号取号回滚()
            {
                患者唯一标识 = PatientModel.当前病人信息.patientId,
                姓名 = PatientModel.当前病人信息.name,
                原挂号记录序号 = ret.挂号序号,
                预约标识 = "1",
                预约记录序号 = RecordModel.所选记录.orderNo
            };

            int i = 3;
            Res挂号取号回滚 resRefund = null;
            while (i-- > 0)
            {
                Logger.Net.Info($"嵊州取号预结算第{i}次回滚");
                resRefund = HisHandleEx.执行挂号取号回滚(reqRefund);
                if (resRefund.IsSuccess)
                {
                    tnModel.Res挂号取号预结算.总金额 = ret.总金额;
                    tnModel.Res挂号取号预结算.个人现金支付金额 = ret.个人现金支付金额;
                    tnModel.Res挂号取号预结算.医保支付金额 = ret.医保支付金额;
                    return Result.Success();
                }

            }
            ShowAlert(false, "费用计算时代扣失败", resRefund.Message);
            PrintRefundFailed(resRefund.Message);
            return Result.Fail(resRefund.Message);
        }


        private void PrintRefundFailed(string reason)
        {
            try
            {
                Logger.Main.Info($"[取号单边账]正在打印取号单边账凭条,原因:{reason}");
                var queue = PrintManager.NewQueue("取号单边账");
                var patientInfo = PatientModel.当前病人信息;
                var sb = new StringBuilder();
                sb.Append($"姓名：{patientInfo.name}\n");
                sb.Append($"就诊卡号：{patientInfo.patientId}\n");
                sb.Append($"就诊序号：{RecordModel.所选记录?.appoNo}\n");
                sb.Append($"科室名称：{RecordModel.所选记录?.deptName}\n");
                sb.Append($"科室编号：{RecordModel.所选记录?.deptCode}\n");
                sb.Append($"交易时间：{DateTimeCore.Now:yyyy-MM-dd HH:mm:ss}\n");
                sb.Append($"柜员编号：{FrameworkConst.OperatorId}\n");
                sb.Append($"请执凭条与医护工作人员联系\n");
                sb.Append($"{reason}\n");
                sb.Append($"祝您早日康复！\n");
                queue.Enqueue(new PrintItemText() { Text = sb.ToString() });

                var printName = ConfigurationManager.GetValue("Printer:Receipt");

                PrintManager.QuickPrint(printName, queue);
            }
            catch (Exception ex)
            {

                Logger.Main.Error($"[单边账凭条打印失败]{ex.Message} {ex.StackTrace}");
            }


        }

        protected Queue<IPrintable> TakeNumPrintables(Res挂号取号结算 res)
        {
            var queue = PrintManager.NewQueue("(挂号凭条)");
            var patientInfo = PatientModel.Res病人信息查询.data[PatientModel.PatientInfoIndex];
            var record = RecordModel?.所选记录;
            var extraPaymentModel = GetInstance<IExtraPaymentModel>();
            var tranNo = string.Empty;
            if (PaymentModel.PayMethod == PayMethod.银联)
            {
                tranNo = (extraPaymentModel.PaymentResult as TransResDto).Ref;
            }
            else if (PaymentModel.PayMethod == PayMethod.支付宝 || PaymentModel.PayMethod == PayMethod.微信支付)
            {
                tranNo = (extraPaymentModel.PaymentResult as 订单状态).outTradeNo;
            }
            var time = record.medAmPm == "1" ? "上午" : "下午";
            var sb = new StringBuilder();
            sb.Append($"------------------------------------\n");
            sb.Append("【当日有效，隔日作废】\n");
            sb.Append($"终端流水：{tranNo}\n");
            sb.Append($"就诊时间：{DateTimeCore.Now.ToString("yyyy-MM-dd")} {time}\n");
            sb.Append($"就诊序号：{res?.全员挂号序号}\n");
            sb.Append($"门诊序号：{res?.挂号序号}\n");
            sb.Append($"姓    名：{patientInfo?.name}\n");
            sb.Append($"就诊卡号：{patientInfo?.patientId}\n");
            sb.Append($"------------------------------------\n");
            sb.Append($"科    室：{RecordModel?.所选记录?.deptName}  {res?.就诊地点}\n");
            if (!string.IsNullOrEmpty(RecordModel?.所选记录?.doctName))
            {
                sb.Append($"医    生：{RecordModel?.所选记录?.doctName}\n");
            }
            else
            {
                sb.Append($"医    生：无\n");
            }
            sb.Append($"就 诊 号：{res?.就诊序号}\n");
            var cardtype = CardModel?.CardType == CardType.社保卡 ? CardType.社保卡.ToString() : "自费卡";
            sb.Append($"卡 类 型：{cardtype}\n");
            sb.Append($"挂 号 费：{PaymentModel.Total.In元()}\n");
            sb.Append($"个人支付：{PaymentModel.Self.In元()}\n");
            sb.Append($"医保支付：{PaymentModel.Insurance.In元()}\n");
            sb.Append($"其他支付：0.00元\n");
            sb.Append($"------------------------------------\n");
            sb.Append($"打印时间：{DateTimeCore.Now.ToString("yyyy-MM-dd   HH:mm:ss")}\n");
            sb.Append($"机器号：{FrameworkConst.OperatorId}\n");
            sb.Append($"凭本条就诊！\n");
            
            sb.Append($"请在当日就诊，隔日作废！\n");
            sb.Append($"如需要发票请携带就诊卡和此凭条到导引台打印\n");
            queue.Enqueue(new PrintItemText { Text = sb.ToString(), Font = new Font("微软雅黑", 9, System.Drawing.FontStyle.Regular) });
            sb.Clear();
            if (!string.IsNullOrEmpty(patientInfo.patientId))
            {
                var image = BarCode128.GetCodeImage(patientInfo.patientId, BarCode.Code128.Encode.Code128A);
                queue.Enqueue(new PrintItemImage
                {
                    Align = ImageAlign.Center,
                    Image = image,
                    Height = image.Height / 2f,
                    Width = image.Width / 2f
                });
            }
            sb.Append(patientInfo?.patientId);
            queue.Enqueue(new PrintItemText { Text = sb.ToString(), StringFormat = PrintConfig.Center, Font = new Font("微软雅黑", 9, System.Drawing.FontStyle.Regular) });
            sb.Clear();
            sb.AppendLine(".");
            sb.AppendLine(".");
            queue.Enqueue(new PrintItemText { Text = sb.ToString(), Font = new Font("微软雅黑", 9, System.Drawing.FontStyle.Regular) });
            return queue;
        }

        protected static readonly BarCode.Code128 BarCode128 = new BarCode.Code128
        {
            Magnify = 1,
            Height = 80
        };
        private void 上传数据到HIS()
        {
            try
            {
                var medApm = DateTimeCore.Now.JudgeAmPm();
                var req = new req预约挂号记录同步到his系统
                {
                    regMode = "2",
                    cardType = CardModel.CardType.ToString(),
                    idNo = PatientModel.当前病人信息.idNo,
                    patientName = PatientModel.当前病人信息.name,
                    phone = PatientModel.当前病人信息.phone,
                    medAmPm = medApm.ToString(),
                    medDate = DateTimeCore.Now.ToString("yyyy-MM-dd"),
                    medTime = DateTimeCore.Now.ToString("HH:mm:ss"),
                    deptCode = RecordModel.所选记录.deptCode,
                    deptName = RecordModel.所选记录.deptName,
                    doctCode = "无",
                    cash = PaymentModel.Total.ToString("0")
                };
                FillRechargeRequest(req);
                var res = DataHandlerEx.预约挂号记录同步到his系统(req);
                if (res.success)
                {
                    Logger.Net.Info($"预约挂号记录同步到his系统成功");
                }
                else
                {
                    Logger.Net.Info($"预约挂号记录同步到his系统失败:{res.msg}");
                }
            }
            catch (Exception ex)
            {
                Logger.Main.Error($"预约挂号记录同步到his系统失败异常:{ex.Message}");
                return;
            }
        }
        private void 自费交易记录同步到his系统()
        {
            try
            {
                var req = new req交易记录同步到his系统
                {
                    platformId = FrameworkConst.OperatorId,
                    hisPatientId = CardModel?.CardNo,
                    cardNo = CardModel?.CardNo,
                    idNo = PatientModel?.当前病人信息?.idNo,
                    patientName = PatientModel?.当前病人信息?.name,
                    tradeType = "2",
                    cash = PaymentModel?.Self.ToString("0"),
                    operId = FrameworkConst.OperatorId,
                    bankTime = DateTimeCore.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                    tradeMode = PaymentModel.PayMethod.GetEnumDescription(),
                    inHos = "1",
                    remarks = "挂号",
                };
                FillRechargeRequest(req);
                var res = DataHandlerEx.交易记录同步到his系统(req);
                if (res.success)
                {
                    Logger.Net.Info($"交易记录同步到his系统成功");
                }
                else
                {
                    Logger.Net.Info($"交易记录同步到his系统失败:{res.msg}");
                }
            }
            catch (Exception ex)
            {
                Logger.Main.Error($"交易记录同步到his系统失败异常:{ex.Message}");
                return;
            }
        }
        private void 医保交易记录同步到his系统()
        {
            try
            {
                var req = new req交易记录同步到his系统
                {
                    platformId = FrameworkConst.OperatorId,
                    hisPatientId = CardModel?.CardNo,
                    cardNo = CardModel?.CardNo,
                    idNo = PatientModel?.当前病人信息?.idNo,
                    patientName = PatientModel?.当前病人信息?.name,
                    tradeType = "2",
                    cash = PaymentModel?.Insurance.ToString("0"),
                    operId = FrameworkConst.OperatorId,
                    bankTime = DateTimeCore.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                    tradeMode = "医保支付",
                    inHos = "1",
                    remarks = "挂号",
                    payAccountNo = "医保账户",
                    tradeTime = DateTimeCore.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                    transNo = BusinessConfigManager.GetFlowId("医保交易记录同步到his系统")
                };
                var res = DataHandlerEx.交易记录同步到his系统(req);
            }
            catch (Exception ex)
            {
                Logger.Main.Error($"交易记录同步到his系统失败异常:{ex.Message}");
                return;
            }
        }
        protected virtual void FillRechargeRequest(req预约挂号记录同步到his系统 req)
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
                    //req.outTradeNo = thirdpayinfo.outTradeNo;
                    req.tradeTime = thirdpayinfo.paymentTime;
                }
            }
        }
        protected virtual void FillRechargeRequest(req交易记录同步到his系统 req)
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
                    //req.outTradeNo = thirdpayinfo.outTradeNo;
                    req.tradeTime = thirdpayinfo.paymentTime;
                }
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
    }
}

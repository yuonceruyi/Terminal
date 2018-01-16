using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prism.Commands;
using Prism.Regions;
using YuanTu.Consts;
using YuanTu.Consts.Dtos.UnionPos;
using YuanTu.Consts.Enums;
using YuanTu.Consts.FrameworkBase;
using YuanTu.Consts.Gateway;
using YuanTu.Consts.Models.Print;
using YuanTu.Consts.Services;
using YuanTu.Consts.Sounds;
using YuanTu.Core.Extension;
using YuanTu.Core.Navigating;
using System.Drawing;

namespace YuanTu.QDQLYY.Component.Register.ViewModels
{
    public class ScheduleViewModel: YuanTu.QDKouQiangYY.Component.Register.ViewModels.ScheduleViewModel
    {
        //public Font HeaderFont2 = new Font("微软雅黑", 14, FontStyle.Bold);        

        protected override Queue<IPrintable> RegisterPrintables()
        {
            var queue = PrintManager.NewQueue("挂号单");
            var register = RegisterModel.Res预约挂号.data;
            var department = DepartmentModel.所选科室;
            var schedule = ScheduleModel.所选排班;
            var patientInfo = PatientModel.Res病人信息查询.data[PatientModel.PatientInfoIndex];
            var reMain = 0m;

            var sb = new StringBuilder();
            sb.Append($"------------------------------------\n");
            sb.Append($"状态：挂号成功\n");

            #region 个人主要信息
            sb.Append($"姓名：{patientInfo.name}\n");
            sb.Append($"门诊号：{patientInfo.patientId}\n");
            sb.Append($"就诊卡号：{patientInfo.cardNo}\n");
            queue.Enqueue(new PrintItemText { Text = sb.ToString() });
            #endregion

            #region  大粗字体,诊疗信息
            sb = new StringBuilder();
            sb.Append($"挂号科室：{department.deptName}\n");
            sb.Append($"挂号序号：{register?.appoNo}号\n");
            sb.Append($"就诊医生：{schedule.doctName}\n");
            sb.Append($"就诊地址：{register?.address}\n");
            sb.Append($"就诊时间：{schedule.medDate} {schedule.medAmPm.SafeToAmPm()}\n");
            queue.Enqueue(new PrintItemText { Text = sb.ToString(), Font = PrintConfig.HeaderFont2 });
            #endregion

            #region 交易信息
            sb = new StringBuilder();
            sb.Append($"交易类型：现场挂号\n");
            sb.Append($"交易金额：{schedule.regAmount.In元()}\n");
            switch (PaymentModel.PayMethod)
            {
                case PayMethod.预缴金:
                    reMain = Convert.ToDecimal(patientInfo.accBalance ?? "0") - Convert.ToDecimal(schedule.regAmount ?? "0");
                    sb.Append($"缴费方式：预缴金\n");
                    sb.Append($"缴费后余额：{reMain.ToString().In元()}\n");
                    sb.Append($"交易流水号：{register.transNo}\n");
                    sb.Append($"交易时间：{DateTimeCore.Now.ToString("yyyy-MM-dd HH:mm:ss")}\n");
                    break;
                case PayMethod.银联:
                    var posinfo = ExtraPaymentModel.PaymentResult as TransResDto;

                    sb.Append($"缴费方式：银行卡\n");
                    if (!string.IsNullOrEmpty(posinfo?.Receipt))
                    {
                        sb.Append($"{posinfo?.Receipt}\n");
                    }
                    else
                    {
                        sb.Append($"银行卡号：{posinfo.CardNo}\n");
                        sb.Append($"授权号：{posinfo.Auth}\n");
                        sb.Append($"终端编号：{posinfo.TId}\n");
                        sb.Append($"终端流水：{posinfo.Trace}\n");
                        sb.Append($"批次号：{posinfo.Batch}\n");
                        sb.Append($"检索参考号：{posinfo.Ref}\n");
                        sb.Append($"交易日期：{posinfo.TransDate}\n");
                        sb.Append($"交易时间：{posinfo.TransTime}\n");
                    }
                    break;
                case PayMethod.社保:
                    var ybinfo = ExtraPaymentModel.PaymentResult as TransResDto;

                    reMain = Convert.ToDecimal(ExtraPaymentModel.ThridRemain); //余额

                    sb.Append($"缴费方式：医保卡\n");
                    sb.Append($"缴费后余额：{reMain.In元()}\n");
                    sb.Append($"卡号：{ybinfo.CardNo}\n");
                    queue.Enqueue(new PrintItemText { Text = sb.ToString() });
                    sb = new StringBuilder();

                    var str1 = string.Empty;
                    var str2 = string.Empty;
                    if (!string.IsNullOrEmpty(ybinfo.Ref))
                    {
                        if (ybinfo.Ref.Length > 12)
                        {
                            str1 = ybinfo.Ref.Substring(0, 12);
                            str2 = ybinfo.Ref.Substring(12, ybinfo.Ref.Length - 12);
                        }
                        else
                        {
                            str1 = ybinfo.Ref;
                        }
                    }

                    sb.Append($"医院支付流水号：{str1}\n");
                    if (!string.IsNullOrEmpty(str2))
                    {
                        sb.Append($"{str2}\n")
                            ;
                    }
                    queue.Enqueue(new PrintItemText { Text = sb.ToString() });
                    sb = new StringBuilder();
                    str1 = string.Empty;
                    str2 = string.Empty;
                    if (!string.IsNullOrEmpty(ybinfo.Trace))
                    {
                        if (ybinfo.Trace.Length > 15)
                        {
                            str1 = ybinfo.Trace.Substring(0, 15);
                            str2 = ybinfo.Trace.Substring(15, ybinfo.Trace.Length - 15);
                        }
                        else
                        {
                            str1 = ybinfo.Trace;
                        }
                    }
                    sb.Append($"交易流水号：{str1}\n");
                    if (!string.IsNullOrEmpty(str2))
                    {
                        sb.Append($"{str2}\n")
                            ;
                    }
                    queue.Enqueue(new PrintItemText { Text = sb.ToString() });
                    sb = new StringBuilder();
                    sb.Append($"批次号：{ybinfo.TId}\n");
                    sb.Append($"交易时间：{ybinfo.TransDate + ybinfo.TransTime}\n");
                    break;
                case PayMethod.支付宝:
                case PayMethod.微信支付:
                    var thirdpayinfo = ExtraPaymentModel.PaymentResult as 订单状态;

                    sb.Append($"缴费方式：{PaymentModel.PayMethod.ToString()}\n");
                    sb.Append($"支付账号：{thirdpayinfo.buyerAccount}\n");
                    str1 = string.Empty;
                    str2 = string.Empty;
                    if (!string.IsNullOrEmpty(thirdpayinfo?.outTradeNo))
                    {
                        if (thirdpayinfo?.outTradeNo.Length > 15)
                        {
                            str1 = thirdpayinfo.outTradeNo.Substring(0, 15);
                            str2 = thirdpayinfo.outTradeNo.Substring(15, thirdpayinfo.outTradeNo.Length - 15);
                        }
                        else
                        {
                            str1 = thirdpayinfo?.outTradeNo;
                        }
                    }
                    sb.Append($"交易流水号：{str1}\n");
                    if (!string.IsNullOrEmpty(str2))
                    {
                        sb.Append($"{str2}\n");
                    }
                    sb.Append($"交易时间：{thirdpayinfo.paymentTime}\n");
                    break;
                default:
                    break;
            }
            #endregion

            sb.Append($"柜员编号：{FrameworkConst.OperatorId}\n");
            queue.Enqueue(new PrintItemText { Text = sb.ToString() });

            #region 温馨提示
            guahaoSpicalPrint(queue);
            #endregion

            sb = new StringBuilder();
            sb.Append($"祝您早日康复！\n");
            queue.Enqueue(new PrintItemText { Text = sb.ToString() });
            return queue;
        }

        protected override Queue<IPrintable> AppointPrintables()
        {
            var queue = PrintManager.NewQueue("预约单");
            var register = RegisterModel.Res预约挂号.data;
            var department = DepartmentModel.所选科室;
            var schedule = ScheduleModel.所选排班;
            var sourceModel = SourceModel.所选号源;

            var patientInfo = PatientModel.Res病人信息查询.data[PatientModel.PatientInfoIndex];
            var reMain = 0m;

            var sb = new StringBuilder();
            sb.Append($"------------------------------------\n");
            sb.Append($"状态：预约成功\n");

            #region 个人主要信息
            sb.Append($"姓名：{patientInfo.name}\n");
            sb.Append($"门诊号：{patientInfo.patientId}\n");
            sb.Append($"就诊卡号：{patientInfo.cardNo}\n");
            queue.Enqueue(new PrintItemText { Text = sb.ToString() });
            #endregion

            #region  大粗字体,诊疗信息
            sb = new StringBuilder();
            sb.Append($"预约科室：{department.deptName}\n");
            sb.Append($"预约医生：{schedule.doctName}\n");
            sb.Append($"就诊地址：{register.address}\n");
            sb.Append($"就诊时间：{schedule.medDate} {schedule.medAmPm.SafeToAmPm()} {sourceModel?.medBegtime}\n");
            queue.Enqueue(new PrintItemText { Text = sb.ToString(), Font = PrintConfig.HeaderFont2 });
            #endregion

            #region 交易信息
            sb = new StringBuilder();
            sb.Append($"交易类型：预约挂号\n");

            if (RegisterExtendModel.isPayNow == "1")//已支付
            {
                sb.Append($"支付状态：预约时支付\n");
                sb.Append($"交易金额：{schedule.regAmount.In元()}\n");

                //todo
                reMain = Convert.ToDecimal(patientInfo.accBalance ?? "0") - Convert.ToDecimal(schedule.regAmount ?? "0");
                sb.Append($"缴费方式：预缴金\n");
                sb.Append($"缴费后余额：{reMain.ToString().In元()}\n");
                sb.Append($"交易流水号：{register.transNo}\n");
                sb.Append($"交易时间：{DateTimeCore.Now.ToString("yyyy-MM-dd HH:mm:ss")}\n");
            }
            else
            {
                sb.Append($"支付状态：取号时支付\n");
            }
            sb.Append($"交易时间：{DateTimeCore.Now.ToString("yyyy-MM-dd HH:mm:ss")}\n");
            queue.Enqueue(new PrintItemText { Text = sb.ToString() });
            #endregion

            sb = new StringBuilder();
            sb.Append($"柜员编号：{FrameworkConst.OperatorId}\n");
            sb.Append($"------------------------------------\n");
            sb.Append($"温馨提示：\n");
            sb.Append($"请于就诊当天提前半小时取号\n");
            sb.Append($"如需取消，请于就诊时间2小时前取消预约。\n");
            sb.Append($"------------------------------------\n");

            sb.Append($"祝您早日康复！\n");
            queue.Enqueue(new PrintItemText { Text = sb.ToString() });
            return queue;
        }
        protected virtual void guahaoSpicalPrint(Queue<IPrintable> queue)
        {
            var sb = new StringBuilder();
            var schedule = ScheduleModel.所选排班;
            sb.Append($"------------------------------------\n");
            sb.Append($"温馨提示：\n");
            sb.Append($"1、请妥善保管好您的凭条。\n");
            sb.Append($"2、此号当日有效，如退号请在三日内办理。\n");
            sb.Append($"3、如需打印发票，儿童到一楼门诊大厅,\n");
            sb.Append($"妇产到负一楼门诊大厅。\n");

            if (Convert.ToDecimal(schedule.regAmount) > 100)
            {
                sb.Append($"4、当日消费凭条可作为当日出门凭证使用。\n");
            }
            sb.Append($"------------------------------------\n");
            queue.Enqueue(new PrintItemText { Text = sb.ToString() });
        }
    }
}

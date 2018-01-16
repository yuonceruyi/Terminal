using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Practices.Unity;
using Prism.Regions;
using YuanTu.Consts;
using YuanTu.Consts.Enums;
using YuanTu.Consts.Gateway;
using YuanTu.Consts.Models.Auth;
using YuanTu.Consts.Models.Payment;
using YuanTu.Core.Extension;
using YuanTu.Core.FrameworkBase;
using YuanTu.Default.Component.Tools.Models;
using YuanTu.Consts.Models.Recharge;
using YuanTu.Core.Gateway;
using YuanTu.Core.Log;
using YuanTu.Core.Navigating;
using YuanTu.Consts.Services;
using System.Threading;
using System.Windows.Data;
using YuanTu.Consts.FrameworkBase;
using YuanTu.Consts.Models.Print;
using YuanTu.Core.DB;
using YuanTu.Core.FrameworkBase.Loadings;
using YuanTu.Core.Services.UploadService;

namespace YuanTu.ShenZhen.BaoAnCenterHospital.Component.Tools.ViewModels
{
    public class ScanQrCodeViewModel : YuanTu.Default.Component.Tools.ViewModels.ScanQrCodeViewModel
    {
      
        protected override void 打印退费失败凭证(string 退费原因,string 失败原因)
        {
            var queue = PrintManager.NewQueue($"{ExtraPaymentModel.CurrentPayMethod}订单取消失败");

            var sb = new StringBuilder();
      
            sb.Append($"姓名：{ExtraPaymentModel.PatientInfo?.Name}\n");
            sb.Append($"登记号：{ExtraPaymentModel.PatientInfo?.PatientId}\n");
            sb.Append($"当前业务：{ExtraPaymentModel.CurrentBusiness}\n");
            sb.Append($"订单号：{订单扫码?.outTradeNo}\n");
            sb.Append($"交易金额：{ExtraPaymentModel?.TotalMoney.In元()}\n");
            sb.Append($"支付账户：{订单状态?.buyerAccount}\n");
            sb.Append($"退费原因：{退费原因}\n");
            sb.Append($"失败原因：{失败原因}\n");
            sb.Append($"交易时间：{DateTimeCore.Now.ToString("yyyy-MM-dd HH:mm:ss")}\n");
            sb.Append($"柜员编号：{FrameworkConst.OperatorId}\n");
            sb.Append($"请联系导诊处理退费事宜，祝您早日康复！\n");
            queue.Enqueue(new PrintItemText { Text = sb.ToString() });
            PrintManager.QuickPrint(ConfigurationManager.GetValue("Printer:Receipt"), queue);
        }
        protected override void 打印网关未知异常凭证(string 异常描述)
        {
            var queue = PrintManager.NewQueue($"{ExtraPaymentModel.CurrentPayMethod}单边账");
            var sb = new StringBuilder();
            sb.Append($"姓名：{ExtraPaymentModel.PatientInfo?.Name}\n");
            sb.Append($"登记号：{ExtraPaymentModel.PatientInfo?.PatientId}\n");
            sb.Append($"当前业务：{ExtraPaymentModel.CurrentBusiness}\n");
            sb.Append($"订单号：{订单扫码?.outTradeNo}\n");
            sb.Append($"交易金额：{ExtraPaymentModel?.TotalMoney.In元()}\n");
            sb.Append($"支付账户：{订单状态?.buyerAccount}\n");
            sb.Append($"异常描述：{异常描述}\n");
            sb.Append($"交易时间：{DateTimeCore.Now.ToString("yyyy-MM-dd HH:mm:ss")}\n");
            sb.Append($"柜员编号：{FrameworkConst.OperatorId}\n");
            sb.Append($"请联系导诊处理，祝您早日康复！\n");
            queue.Enqueue(new PrintItemText { Text = sb.ToString() });
            PrintManager.QuickPrint(ConfigurationManager.GetValue("Printer:Receipt"), queue);
        }
    }
}

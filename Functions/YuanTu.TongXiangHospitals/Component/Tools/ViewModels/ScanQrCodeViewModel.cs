using System.Text;
using Prism.Regions;
using YuanTu.Consts;
using YuanTu.Consts.Enums;
using YuanTu.Consts.Models.Print;
using YuanTu.Core.Extension;

namespace YuanTu.TongXiangHospitals.Component.Tools.ViewModels
{
    public class ScanQrCodeViewModel : Default.Component.Tools.ViewModels.ScanQrCodeViewModel
    {
        public override void OnEntered(NavigationContext navigationContext)
        {
            OptDic[Business.输液费] = OptDic[Business.缴费];
            base.OnEntered(navigationContext);
        }

        protected override void 打印网关未知异常凭证(string 异常描述)
        {
            var queue = PrintManager.NewQueue($"{ExtraPaymentModel.CurrentPayMethod}单边账");
            var sb = new StringBuilder();
            sb.Append($"姓名：{ExtraPaymentModel.PatientInfo?.Name}\n");
            sb.Append($"卡号：{ExtraPaymentModel.PatientInfo?.CardNo}\n");
            sb.Append($"当前业务：{ExtraPaymentModel.CurrentBusiness}\n");
            sb.Append($"订单号：{订单扫码?.outTradeNo}\n");
            sb.Append($"交易金额：{ExtraPaymentModel?.TotalMoney.In元()}\n");
            sb.Append($"支付账户：{订单状态?.buyerAccount}\n");
            sb.Append($"异常描述：{异常描述}\n");
            sb.Append($"交易时间：{DateTimeCore.Now.ToString("yyyy-MM-dd HH:mm:ss")}\n");
            sb.Append($"柜员编号：{FrameworkConst.OperatorId}\n");
            sb.Append($"请联系导诊处理，祝您早日康复！\n");
            queue.Enqueue(new PrintItemText {Text = sb.ToString()});
            PrintManager.QuickPrint(ConfigurationManager.GetValue("Printer:Receipt"), queue);
        }
    }
}
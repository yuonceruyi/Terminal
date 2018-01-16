using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using YuanTu.Consts;
using YuanTu.Consts.Enums;
using YuanTu.Consts.Models;
using YuanTu.Core.Extension;
using YuanTu.Core.Log;

namespace YuanTu.ZheJiangZhongLiuHospital.Component.ViewModels
{
    public class ConfirmViewModel : YuanTu.Default.Component.ViewModels.ConfirmViewModel
    {
        protected override void HandlePaymethod(Info i, PayMethod payMethod)
        {
            switch (payMethod)
            {
                case PayMethod.银联:
                    ExtraPaymentModel.FinishFunc = () => Task.Run(PaymentModel.ConfirmAction);
                    GoToUnion();
                    break;

                case PayMethod.支付宝:
                    ExtraPaymentModel.FinishFunc = () => Task.Run(PaymentModel.ConfirmAction);
                    Navigate(A.Third.ScanQrCode);
                    break;

                case PayMethod.微信支付:
                    ExtraPaymentModel.FinishFunc = () => Task.Run(PaymentModel.ConfirmAction);
                    Navigate(A.Third.ScanQrCode);
                    break;
            }
        }

        protected override void NoPayConfirm()
        {
            if ((ChoiceModel.Business == Business.挂号 || ChoiceModel.Business == Business.取号) && CardModel.CardType != CardType.社保卡)
            {
                var patientModel = PatientModel.当前病人信息;
                if (decimal.Parse(patientModel.accBalance) < PaymentModel.Self)
                {
                    var textblock = new TextBlock
                    {
                        TextWrapping = TextWrapping.Wrap,
                        TextAlignment = TextAlignment.Center,
                        Margin = new Thickness(0, 15, 0, 0)
                    };


                    ExtraPaymentModel.TotalMoney = PaymentModel.Self - decimal.Parse(patientModel.accBalance);
                    textblock.Inlines.Add("应支付金额：");
                    textblock.Inlines.Add(new TextBlock { Text = $"{PaymentModel.Self.In元()}", Foreground = new SolidColorBrush(Color.FromRgb(245, 162, 81)) });
                    textblock.Inlines.Add("\r\n账户余额：");
                    textblock.Inlines.Add(new TextBlock { Text = $"{patientModel.accBalance.In元()}", Foreground = new SolidColorBrush(Color.FromRgb(245, 162, 81)) });
                    textblock.Inlines.Add("\r\n差额：");
                    textblock.Inlines.Add(new TextBlock { Text = $"{ExtraPaymentModel.TotalMoney.In元()}", Foreground = new SolidColorBrush(Color.FromRgb(245, 162, 81)) });

                    ShowConfirm("账户余额不足", textblock, cp =>
                    {
                        if (cp)
                            StackNavigate(A.ChongZhi_Context, A.CZ.RechargeWay);
                    }, 30, ConfirmExModel.Build("去充值", "暂不", true));
                    return;
                }
            }


            PaymentModel.ConfirmAction?.BeginInvoke(cp =>
            {
                var rest = PaymentModel.ConfirmAction?.EndInvoke(cp);
                if (rest?.IsSuccess ?? false)
                    ChangeNavigationContent("");
            }, null);
        }
    }
}

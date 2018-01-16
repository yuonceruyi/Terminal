using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using Prism.Commands;
using Prism.Regions;
using YuanTu.Consts.Enums;
using YuanTu.Consts.Models;
using YuanTu.Consts.Services;

namespace YuanTu.VirtualHospital.Tablet.Component.Cashier.ViewModels
{
    class AmountViewModel : YuanTu.Default.Tablet.Component.Cashier.ViewModels.AmountViewModel
    {
        public override void OnEntered(NavigationContext navigationContext)
        {
            base.OnEntered(navigationContext);
            PayOut.Add(new InfoPay()
            {
                Title = "刷脸支付",
                ConfirmCommand = new DelegateCommand<Info>(OnButtonClick),
                IconUri = ResourceEngine.GetImageResourceUri("支付_社保卡"),
                Tag = null, // 特殊处理
                Color = Color.FromRgb(50, 50, 50),
                IsEnabled = true,
            });
        }

        protected override void Confirm(Info i)
        {
            // 刷脸支付
            if (i.Tag == null)
            {
                Cashier.PayMethod = PayMethod.预缴金;
                Navigate(InnerA.SY.FaceRec);
                return;
            }
            base.Confirm(i);
        }
    }
}

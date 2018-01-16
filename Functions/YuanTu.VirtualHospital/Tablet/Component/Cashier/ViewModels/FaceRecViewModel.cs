using Microsoft.Practices.Unity;
using YuanTu.Core.FrameworkBase.Loadings;
using YuanTu.Default.Tablet.Component.Cashier.Models;
using YuanTu.VirtualHospital.Component.Auth.ViewModels;

namespace YuanTu.VirtualHospital.Tablet.Component.Cashier.ViewModels
{
    public class FaceRecViewModel : FaceRecViewModelBase
    {
        public override string Title => "刷脸支付";

        protected override void Act(LoadingProcesser lp, string imageData)
        {
            Cashier.GotCardBareFunc(imageData, "22").GetAwaiter().GetResult();
        }

        [Dependency]
        public ICashierModel Cashier { get; set; }
    }
}
using Microsoft.Practices.Unity;
using Prism.Regions;
using YuanTu.Core.FrameworkBase;
using YuanTu.Default.Tablet.Component.Cashier.Models;

namespace YuanTu.Default.Tablet.Component.Cashier.ViewModels
{
    internal class SelectViewModel : ViewModelBase
    {
        public override string Title => "";

        [Dependency]
        public ICashierModel Cashier { get; set; }

        public override void OnEntered(NavigationContext navigationContext)
        {
        }
    }
}
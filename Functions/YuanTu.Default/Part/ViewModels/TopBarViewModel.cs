using Microsoft.Practices.Unity;
using YuanTu.Core.FrameworkBase;
using YuanTu.Core.Models;

namespace YuanTu.Default.Part.ViewModels
{
    public class TopBarViewModel : ViewModelBase
    {
        public override string Title => "页首";

        [Dependency]
        public ITopBottomModel Info { get; set; }
    }
}
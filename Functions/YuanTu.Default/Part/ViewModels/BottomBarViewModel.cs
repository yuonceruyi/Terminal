using Microsoft.Practices.Unity;
using YuanTu.Core.FrameworkBase;
using YuanTu.Core.Models;

namespace YuanTu.Default.Part.ViewModels
{
    public class BottomBarViewModel : ViewModelBase
    {
        public override string Title => "底部任务栏";

        [Dependency]
        public ITopBottomModel Info { get; set; }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prism.Events;
using YuanTu.Core.Navigating;

namespace YuanTu.BJJingDuETYY.Part.ViewModels
{
    public class NavigateBarViewModel : Default.Part.ViewModels.NavigateBarViewModel
    {
        public NavigateBarViewModel(IEventAggregator eventAggregator) : base(eventAggregator)
        {
        }
        protected override void NavigateClick(NavigationItem obj)
        {
        }
    }
}

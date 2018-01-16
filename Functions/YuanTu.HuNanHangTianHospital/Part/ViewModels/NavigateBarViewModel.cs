using Prism.Commands;
using Prism.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YuanTu.Consts;
using YuanTu.Consts.Enums;
using YuanTu.Consts.EventModels;
using YuanTu.Core.Navigating;
using YuanTu.Consts.Models;

namespace YuanTu.HuNanHangTianHospital.Part.ViewModels
{
    public class NavigateBarViewModel:YuanTu.Default.Part.ViewModels.NavigateBarViewModel
    {
        public NavigateBarViewModel(IEventAggregator eventAggregator) : base(eventAggregator)
        {
        }
        protected override void NavigateClick(NavigationItem obj)
        {
        }
    }
}

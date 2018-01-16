using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prism.Regions;

namespace YuanTu.YanTaiYDYY.Component.TakeNum.ViewModels
{
    public class TakeNumViewModel:YuanTu.Default.Component.TakeNum.ViewModels.TakeNumViewModel
    {
        protected override bool CanConfirm()
        {
            return false;
        }
    }
}

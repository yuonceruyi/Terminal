using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YuanTu.YiWuFuBao.Component.ViewModels
{
    public class PrintViewModel: YuanTu.YiWuArea.Component.ViewModels.PrintViewModel
    {
        protected override void ExitCard()
        {
            if (!ConstInner.IsOldMachine)
            {
                base.ExitCard();
            }
        }
    }
}

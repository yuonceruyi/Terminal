using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YuanTu.YiWuFuBao.Clinic
{
    public class MainWindowViewModel: YuanTu.Default.MainWindowViewModel
    {

        public override bool ShowNavigating
        {
            get { return false; }
            set
            {
                OnPropertyChanged();
                OnPropertyChanged(nameof(ShowBack));
            }
        }

        public override bool ShowBack => true;
    }
}

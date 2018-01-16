using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Practices.Unity;
using Prism.Commands;
using YuanTu.Consts;
using YuanTu.Core.FrameworkBase;
using YuanTu.Core.Models;

namespace YuanTu.YuHangZYY
{
    public class MainWindowViewModel:YuanTu.Default.MainWindowViewModel
    {
        [Dependency]
        public ITopBottomModel TopBottomModel { get; set; }

      
    }
}

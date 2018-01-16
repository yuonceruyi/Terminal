using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YuanTu.Consts;

namespace YuanTu.TongXiangHospitals.Component.ViewModels
{
    public class PrintViewModel:YuanTu.Default.Component.ViewModels.PrintViewModel
    {
        private bool _doCommand;
        protected override void Confirm()
        {
            if (_doCommand)
                return;
            _doCommand = !_doCommand;
            Navigate(A.Home);
        }
    }
}

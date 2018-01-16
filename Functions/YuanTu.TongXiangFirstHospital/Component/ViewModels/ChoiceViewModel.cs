using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YuanTu.Devices.CardReader;

namespace YuanTu.TongXiangFirstHospital.Component.ViewModels
{
    class ChoiceViewModel:TongXiangHospitals.Component.ViewModels.ChoiceViewModel
    {
        public ChoiceViewModel(IMagCardReader[] magCardReaders) : base(magCardReaders)
        {
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YuanTu.Devices.CardReader;

namespace YuanTu.TongXiangFirstHospital.Component.Auth.ViewModels
{
    public class SiCardViewModel:YuanTu.TongXiangHospitals.Component.Auth.ViewModels.SiCardViewModel
    {
        public SiCardViewModel(IIcCardReader[] icCardReaders, IRFCpuCardReader[] rfCpuCardReaders) : base(icCardReaders, rfCpuCardReaders)
        {
        }
    }
}

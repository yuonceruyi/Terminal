using YuanTu.Devices.CardReader;

namespace YuanTu.TongXiangZhouQuanHospital.Component.Auth.ViewModels
{
    public class SiCardViewModel:YuanTu.TongXiangHospitals.Component.Auth.ViewModels.SiCardViewModel
    {
        public SiCardViewModel(IIcCardReader[] icCardReaders, IRFCpuCardReader[] rfCpuCardReaders) : base(icCardReaders, rfCpuCardReaders)
        {
        }
    }
}

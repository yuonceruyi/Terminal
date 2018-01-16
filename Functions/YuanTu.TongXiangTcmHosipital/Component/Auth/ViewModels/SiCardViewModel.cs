using YuanTu.Devices.CardReader;

namespace YuanTu.TongXiangTcmHosipital.Component.Auth.ViewModels
{
    public class SiCardViewModel:TongXiangHospitals.Component.Auth.ViewModels.SiCardViewModel
    {
        public SiCardViewModel(IIcCardReader[] icCardReaders, IRFCpuCardReader[] rfCpuCardReaders) : base(icCardReaders, rfCpuCardReaders)
        {
        }
    }
}

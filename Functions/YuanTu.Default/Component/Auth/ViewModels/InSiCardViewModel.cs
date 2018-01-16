using YuanTu.Devices.CardReader;

namespace YuanTu.Default.Component.Auth.ViewModels
{
    public class InSiCardViewModel : SiCardViewModel
    {
        public InSiCardViewModel(IIcCardReader[] icCardReaders, IRFCpuCardReader[] rfCpuCardReaders) : base(icCardReaders, rfCpuCardReaders)
        {

        }
    }
}
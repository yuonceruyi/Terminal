using YuanTu.Devices.CardReader;

namespace YuanTu.VirtualHospital.Tablet.Component.Auth.ViewModels
{
    public class CardViewModel : Default.Tablet.Component.Auth.ViewModels.CardViewModel
    {
        public CardViewModel(IRFCardReader[] rfCardReaders, IMagCardReader[] magCardReaders)
            : base(rfCardReaders, magCardReaders)
        {
        }
    }
}
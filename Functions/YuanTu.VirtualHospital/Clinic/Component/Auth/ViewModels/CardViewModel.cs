using YuanTu.Devices.CardReader;

namespace YuanTu.VirtualHospital.Clinic.Component.Auth.ViewModels
{
    public class CardViewModel : Default.Clinic.Component.Auth.ViewModels.CardViewModel
    {
        public CardViewModel(IRFCardReader[] rfCardReaders, IMagCardReader[] magCardReaders)
            : base(rfCardReaders, magCardReaders)
        {
        }
    }
}
using YuanTu.Devices.CardReader;

namespace YuanTu.QDFuErYY.Clinic.Component.Auth.ViewModels
{
    public class CardViewModel : QDKouQiangYY.Clinic.Component.Auth.ViewModels.CardViewModel
    {
        public CardViewModel(IRFCardReader[] rfCardReaders, IMagCardReader[] magCardReaders)
            : base(rfCardReaders, magCardReaders)
        {

        }
    }
}
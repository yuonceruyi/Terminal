using YuanTu.Devices.CardReader;

namespace YuanTu.QDQLYY.Component.Auth.ViewModels
{
    internal class CardViewModel : QDKouQiangYY.Component.Auth.ViewModels.CardViewModel
    {
        public CardViewModel(IRFCardReader[] rfCardReaders, IMagCardReader[] magCardReaders)
            : base(rfCardReaders, magCardReaders)
        {
        }
    }
}
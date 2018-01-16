using YuanTu.Consts.Services;
using YuanTu.Devices.CardReader;

namespace YuanTu.BJJingDuETYY.Clinic.Component.Auth.ViewModels
{
    public class CardViewModel : Default.Clinic.Component.Auth.ViewModels.CardViewModel
    {
        public CardViewModel(IRFCardReader[] rfCardReaders, IMagCardReader[] magCardReaders)
            : base(rfCardReaders, magCardReaders)
        {

        }
        public override void OnSet()
        {
            base.OnSet();
            BackUri = ResourceEngine.GetImageResourceUri("插就诊卡");
        }
    }
}
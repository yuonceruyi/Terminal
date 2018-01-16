using System.Threading.Tasks;
using YuanTu.Consts.Services;
using YuanTu.Devices.CardReader;

namespace YuanTu.TongXiangHospitals.Component.Auth.ViewModels
{
    public class CardViewModel : Default.Component.Auth.ViewModels.CardViewModel
    {
        public CardViewModel(IRFCardReader[] rfCardReaders, IMagCardReader[] magCardReaders)
            : base(rfCardReaders, magCardReaders)
        {
        }

        public override void OnSet()
        {
            BackUri = ResourceEngine.GetImageResourceUri("插就诊卡");
        }

        protected override void StartRead()
        {
            Task.Run(() => StartMag());
        }

        protected override void StopRead()
        {
            StopMag();
        }
    }
}
using System.Linq;
using YuanTu.Devices.CardReader;

namespace YuanTu.FuYangRMYY.Component.Auth.ViewModels
{
    public class IDCardViewModel : Default.Component.Auth.ViewModels.IDCardViewModel
    {
        public IDCardViewModel(IIdCardReader[] idCardReaders) : base(idCardReaders)
        {
            _idCardReader = idCardReaders.FirstOrDefault(p => p.DeviceId == "DK_dk");
        }
    }
}
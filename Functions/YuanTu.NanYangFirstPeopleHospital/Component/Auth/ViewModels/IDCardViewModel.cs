using System.Linq;
using Prism.Regions;
using YuanTu.Devices.CardReader;

namespace YuanTu.NanYangFirstPeopleHospital.Component.Auth.ViewModels
{
    public class IDCardViewModel : Default.Component.Auth.ViewModels.IDCardViewModel
    {
        public IDCardViewModel(IIdCardReader[] idCardReaders) : base(idCardReaders)
        {
            //_idCardReader = idCardReaders.FirstOrDefault(p => p.DeviceId == "HuaDa_HUADA");
        }
        public override void OnEntered(NavigationContext navigationContext)
        {
            base.OnEntered(navigationContext);
            CardModel.CardType = Consts.Enums.CardType.身份证;
        }
    }
}
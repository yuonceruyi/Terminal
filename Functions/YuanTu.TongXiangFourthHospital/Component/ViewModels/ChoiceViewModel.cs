using YuanTu.Devices.CardReader;

namespace YuanTu.TongXiangFourthHospital.Component.ViewModels
{
    public class ChoiceViewModel:TongXiangHospitals.Component.ViewModels.ChoiceViewModel
    {
        public ChoiceViewModel(IMagCardReader[] magCardReaders) : base(magCardReaders)
        {
        }
    }
}

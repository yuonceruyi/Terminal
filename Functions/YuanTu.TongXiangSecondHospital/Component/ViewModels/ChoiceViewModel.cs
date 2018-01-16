using YuanTu.Devices.CardReader;

namespace YuanTu.TongXiangSecondHospital.Component.ViewModels
{
    class ChoiceViewModel:TongXiangHospitals.Component.ViewModels.ChoiceViewModel
    {
        public ChoiceViewModel(IMagCardReader[] magCardReaders) : base(magCardReaders)
        {
        }
    }
}

using YuanTu.Devices.CardReader;

namespace YuanTu.TongXiangPiFangHospital.Component.ViewModels
{
    class ChoiceViewModel:TongXiangHospitals.Component.ViewModels.ChoiceViewModel
    {
        public ChoiceViewModel(IMagCardReader[] magCardReaders) : base(magCardReaders)
        {
        }
    }
}

using YuanTu.Devices.CardReader;

namespace YuanTu.TongXiangShiMenHospital.Component.ViewModels
{
    public class ChoiceViewModel:TongXiangHospitals.Component.ViewModels.ChoiceViewModel
    {
        public ChoiceViewModel(IMagCardReader[] magCardReaders) : base(magCardReaders)
        {
        }
    }
}

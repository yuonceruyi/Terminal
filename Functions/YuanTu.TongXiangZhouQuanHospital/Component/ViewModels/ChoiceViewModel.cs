using YuanTu.Devices.CardReader;

namespace YuanTu.TongXiangZhouQuanHospital.Component.ViewModels
{
    public class ChoiceViewModel:TongXiangHospitals.Component.ViewModels.ChoiceViewModel
    {
        public ChoiceViewModel(IMagCardReader[] magCardReaders) : base(magCardReaders)
        {
        }
    }
}

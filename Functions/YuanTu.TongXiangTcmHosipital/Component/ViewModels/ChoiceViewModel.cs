using YuanTu.Devices.CardReader;

namespace YuanTu.TongXiangTcmHosipital.Component.ViewModels
{
    public class ChoiceViewModel:TongXiangHospitals.Component.ViewModels.ChoiceViewModel
    {
        public ChoiceViewModel(IMagCardReader[] magCardReaders) : base(magCardReaders)
        {
        }
    }
}

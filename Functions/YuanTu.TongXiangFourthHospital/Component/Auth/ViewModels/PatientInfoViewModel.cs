using YuanTu.Devices.CardReader;

namespace YuanTu.TongXiangFourthHospital.Component.Auth.ViewModels
{
    public  class PatientInfoViewModel:TongXiangHospitals.Component.Auth.ViewModels.PatientInfoViewModel
    {
        public PatientInfoViewModel(IMagCardDispenser[] magCardDispenser, IMagCardReader[] magCardReaders) : base(magCardDispenser, magCardReaders)
        {
        }
    }
}

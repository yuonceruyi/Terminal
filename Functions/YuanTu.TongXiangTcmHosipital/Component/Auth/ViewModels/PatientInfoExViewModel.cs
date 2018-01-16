using YuanTu.Devices.CardReader;

namespace YuanTu.TongXiangTcmHosipital.Component.Auth.ViewModels
{
    public class PatientInfoExViewModel:TongXiangHospitals.Component.Auth.ViewModels.PatientInfoExViewModel
    {
        public PatientInfoExViewModel(IMagCardDispenser[] magCardDispenser) : base(magCardDispenser)
        {
        }
    }
}

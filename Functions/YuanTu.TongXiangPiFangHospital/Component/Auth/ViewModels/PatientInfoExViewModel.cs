using YuanTu.Devices.CardReader;

namespace YuanTu.TongXiangPiFangHospital.Component.Auth.ViewModels
{
    public class PatientInfoExViewModel:TongXiangHospitals.Component.Auth.ViewModels.PatientInfoExViewModel
    {
        public PatientInfoExViewModel(IMagCardDispenser[] magCardDispenser) : base(magCardDispenser)
        {
        }
    }
}

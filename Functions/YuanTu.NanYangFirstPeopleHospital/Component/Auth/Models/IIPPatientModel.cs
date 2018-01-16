using YuanTu.Consts.FrameworkBase;

namespace YuanTu.NanYangFirstPeopleHospital.Component.Auth.Models
{
    public  interface IIpPatientModel:IModel
    {
        string IpPatientNo { get; set; }
    }

    public class IpPatientModel :ModelBase,IIpPatientModel
    {
        public string IpPatientNo { get; set; }
    }
}

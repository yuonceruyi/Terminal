using YuanTu.Consts.FrameworkBase;

namespace YuanTu.PanYu.House
{
    public interface IPatientModel : IModel
    {
        string Name { get; set; }
        string Gender { get; set; }
        string IDNo { get; set; }
        string Birthday { get; set; }
        string Address { get; set; }
        string Phone { get; set; }
        string CardNo { get; set; }
        decimal CertType { get; set; }

        string Nation { get; set; }
    }

    public class PatientModel : ModelBase, IPatientModel
    {
        public string Name { get; set; }
        public string Gender { get; set; }
        public string IDNo { get; set; }
        public string Birthday { get; set; }
        public string Address { get; set; }
        public string Phone { get; set; }
        public string CardNo { get; set; }
        public decimal CertType { get; set; }
        public string Nation { get; set; }
    }
}
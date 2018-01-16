using System.Collections.Generic;
using YuanTu.Consts.FrameworkBase;
using YuanTu.Consts.UserCenter.Entities;

namespace YuanTu.Consts.Models.Auth
{
    public interface IQrCodeModel : IModel
    {
        string Uuid { get; set; }

        List<PatientVO> Patients { get; set; }

        PatientVO Patient { get; set; }
    }

    public class QrCodeModel : ModelBase, IQrCodeModel
    {
        public string Uuid { get; set; }
        public List<PatientVO> Patients { get; set; }
        public PatientVO Patient { get; set; }
    }
}
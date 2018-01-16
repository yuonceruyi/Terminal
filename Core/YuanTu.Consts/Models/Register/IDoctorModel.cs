using YuanTu.Consts.FrameworkBase;
using YuanTu.Consts.Gateway;

namespace YuanTu.Consts.Models.Register
{
    public interface IDoctorModel : IModel
    {
        req医生信息查询 医生信息查询 { get; set; }
        res医生信息查询 Res医生信息查询 { get; set; }
        医生信息 所选医生 { get; set; }
        req查询所有医生信息 查询所有医生信息 { get; set; }
        res查询所有医生信息 Res查询所有医生信息 { get; set; }
        医生介绍 医生介绍 { get; set; }
    }

    public class DefaultDoctorModel : ModelBase, IDoctorModel
    {
        public req医生信息查询 医生信息查询 { get; set; }
        public res医生信息查询 Res医生信息查询 { get; set; }
        public 医生信息 所选医生 { get; set; }
        public req查询所有医生信息 查询所有医生信息 { get; set; }
        public res查询所有医生信息 Res查询所有医生信息 { get; set; }
        public 医生介绍 医生介绍 { get; set; }
    }
}
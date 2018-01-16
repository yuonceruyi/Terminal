using System.Collections.Generic;
using YuanTu.Consts.FrameworkBase;
using YuanTu.Consts.Gateway;
using YuanTu.QDQLYY.Current.Models;

namespace YuanTu.QDQLYY.Current.Models
{

    public interface IDeptDocModel : IModel
    {
        res科室信息查询 Res科室信息查询 { get; set; }
        req科室信息查询 Req科室信息查询 { get; set; }
        res医生信息查询 Res医生信息查询 { get; set; }
        req医生信息查询 Req医生信息查询 { get; set; }
        科室信息 所选科室信息 { get; set; }
        医生信息 所选医生信息 { get; set; }

    }

    public class DeptDocModel : ModelBase, IDeptDocModel
    {
        public res科室信息查询 Res科室信息查询 { get; set; }
        public req科室信息查询 Req科室信息查询 { get; set; }
        public res医生信息查询 Res医生信息查询 { get; set; }
        public req医生信息查询 Req医生信息查询 { get; set; }
        public 科室信息 所选科室信息 { get; set; }
        public 医生信息 所选医生信息 { get; set; }
    }
}


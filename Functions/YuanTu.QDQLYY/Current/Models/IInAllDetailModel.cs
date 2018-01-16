using System.Collections.Generic;
using YuanTu.Consts.FrameworkBase;
using YuanTu.Consts.Gateway;
using YuanTu.QDQLYY.Current.Models;

namespace YuanTu.QDQLYY.Current.Models
{

    public interface IInAllDetailModel : IModel
    {
        res出院结算明细次数 Res出院结算明细次数 { get; set; }
        req出院结算明细次数 Req出院结算明细次数 { get; set; }
        res出院结算明细查询 Res出院结算明细查询 { get; set; }
        req出院结算明细查询 Req出院结算明细查询 { get; set; }
        res出院结算明细打印 Res出院结算明细打印 { get; set; }
        req出院结算明细打印 Req出院结算明细打印 { get; set; }
        bool CanPrint { get; set; }
}

    public class InAllDetailModel : ModelBase, IInAllDetailModel
    {
        public res出院结算明细次数 Res出院结算明细次数 { get; set; }
        public req出院结算明细次数 Req出院结算明细次数 { get; set; }
        public res出院结算明细查询 Res出院结算明细查询 { get; set; }
        public req出院结算明细查询 Req出院结算明细查询 { get; set; }
        public res出院结算明细打印 Res出院结算明细打印 { get; set; }
        public req出院结算明细打印 Req出院结算明细打印 { get; set; }
        public bool CanPrint { get; set; }
    }
}


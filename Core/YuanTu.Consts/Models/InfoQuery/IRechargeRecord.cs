using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YuanTu.Consts.FrameworkBase;
using YuanTu.Consts.Gateway;

namespace YuanTu.Consts.Models.InfoQuery
{   
    public interface IRechargeRecordModel : IModel
    {
        req查询预缴金充值记录 Req查询预缴金充值记录 { get; set; }
        res查询预缴金充值记录 Res查询预缴金充值记录 { get; set; }
    }

    public class RechargeRecordModel : ModelBase, IRechargeRecordModel
    {
        public req查询预缴金充值记录 Req查询预缴金充值记录 { get; set; }
        public res查询预缴金充值记录 Res查询预缴金充值记录 { get; set; }
    }
}

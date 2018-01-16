using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YuanTu.Consts.FrameworkBase;
using YuanTu.Consts.Gateway;

namespace YuanTu.Default.Component.PrintAgain.ViewModels
{
    public interface ISettlementRecordModel : IModel
    {
        req获取已结算记录 Req获取已结算记录 { get; set; }
        res获取已结算记录 Res获取已结算记录 { get; set; }
        已缴费概要信息 所选已缴费概要 { get; set; }
    }

    public class SettlementRecordModel : ModelBase, ISettlementRecordModel
    {
        public req获取已结算记录 Req获取已结算记录 { get; set; }
        public res获取已结算记录 Res获取已结算记录 { get; set; }
        public 已缴费概要信息 所选已缴费概要 { get; set; }
    }
}

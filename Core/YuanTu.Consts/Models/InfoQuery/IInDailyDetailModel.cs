using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YuanTu.Consts.FrameworkBase;
using YuanTu.Consts.Gateway;

namespace YuanTu.Consts.Models.InfoQuery
{
    public interface IInDailyDetailModel : IModel
    {
        req住院患者费用明细查询 Req住院患者费用明细查询 { get; set; }
        res住院患者费用明细查询 Res住院患者费用明细查询 { get; set; }
        DateTime StartDate { get; set; }
        DateTime EndDate { get; set; }
    }

    public class InDailyDetailModel : ModelBase, IInDailyDetailModel
    {
        public req住院患者费用明细查询 Req住院患者费用明细查询 { get; set; }
        public res住院患者费用明细查询 Res住院患者费用明细查询 { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}

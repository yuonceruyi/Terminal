using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YuanTu.Consts.FrameworkBase;
using YuanTu.Consts.Gateway;

namespace YuanTu.Consts.Models.DischargeSettlement
{
    /// <summary>
    /// 自助出院Model
    /// </summary>
    public interface IDischargeSettlementModel:IModel
    {
        req自助出院预结算 Req自助出院预结算 { get; set; }
        res自助出院预结算 Res自助出院预结算 { get; set; }
        req自助出院结算 Req自助出院结算 { get; set; }
        res自助出院结算 Res自助出院结算 { get; set; }
    }

    public class DischargeSettlementModel : ModelBase, IDischargeSettlementModel
    {
        public req自助出院预结算 Req自助出院预结算 { get; set; }
        public res自助出院预结算 Res自助出院预结算 { get; set; }
        public req自助出院结算 Req自助出院结算 { get; set; }
        public res自助出院结算 Res自助出院结算 { get; set; }
    }
}

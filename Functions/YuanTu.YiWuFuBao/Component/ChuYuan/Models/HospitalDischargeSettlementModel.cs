using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YuanTu.Consts.Models.DischargeSettlement;
using YuanTu.Core.FrameworkBase.Loadings;
using YuanTu.YiWuArea.Insurance.Models;
using YuanTu.YiWuFuBao.Dtos;

namespace YuanTu.YiWuFuBao.Component.ChuYuan.Models
{
    public class HospitalDischargeSettlementModel: DischargeSettlementModel
    {
      //  public ZHUYUANRYXX_OUT ZyPatientSubInfoDto { get; set; }

        public Req住院预结算 Req住院预结算 { get; set; }
        public Res住院预结算 Res住院预结算 { get; set; }

        public Req出院结算 Req出院结算 { get; set; }
        public Res出院结算 Res出院结算 { get; set; }
        public Res交易确认 Res交易确认 { get; set; }
        public Action<LoadingProcesser> YiBaoCardInfoCallback { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YuanTu.Consts.FrameworkBase;
using YuanTu.XiaoShanArea.CYHIS.DLL;
using YuanTu.XiaoShanArea.CYHIS.WebService;

namespace YuanTu.XiaoShanHealthStation.Component.BillPay.Models
{
    public interface IBillModel:IModel
    {
        MENZHENFYMX_OUT 门诊费用明细Out { get; set; }
        缴费预结算_OUT 缴费预结算Out { get; set; }
        缴费结算_OUT 缴费结算Out { get; set; }
    }

    public class BillModel : ModelBase, IBillModel
    {
       
        public MENZHENFYMX_OUT 门诊费用明细Out { get; set; }
        public 缴费预结算_OUT 缴费预结算Out { get; set; }
        public 缴费结算_OUT 缴费结算Out { get; set; }
    }
}

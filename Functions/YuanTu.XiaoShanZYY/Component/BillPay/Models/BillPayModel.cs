using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YuanTu.Consts.FrameworkBase;
using YuanTu.XiaoShanZYY.HIS;

namespace YuanTu.XiaoShanZYY.Component.BillPay.Models
{
    public interface IBillPayModel : IModel
    {
        List<MENZHENFYXX> FEIYONGMX { get; set; }
        List<JIBINGXX> JIBINGMX { get; set; }
        decimal Sum { get; set; }
        Res预结算 Res预结算 { get; set; }
        Res结算 Res结算 { get; set; }
        string 取药地址 { get; set; }
    }
    class BillPayModel:ModelBase, IBillPayModel
    {
        public List<MENZHENFYXX> FEIYONGMX { get; set; }
        public List<JIBINGXX> JIBINGMX { get; set; }
        public decimal Sum { get; set; }
        public Res预结算 Res预结算 { get; set; }
        public Res结算 Res结算 { get; set; }
        public string 取药地址 { get; set; }
    }
}

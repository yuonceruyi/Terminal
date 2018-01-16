using System.Collections.Generic;
using YuanTu.Consts.FrameworkBase;
using YuanTu.ZheJiangHospital.HIS;

namespace YuanTu.ZheJiangHospital.Component.BillPay.Models
{
    public interface IBillPayModel : IModel
    {
        List<CFYJ> Records { get; set; }
        decimal Total { get; set; }
    }

    public class BillPayModel : ModelBase, IBillPayModel
    {
        public List<CFYJ> Records { get; set; }
        public decimal Total { get; set; }
    }
}
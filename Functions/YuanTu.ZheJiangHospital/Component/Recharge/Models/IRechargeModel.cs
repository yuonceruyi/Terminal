using Prism.Mvvm;
using YuanTu.Consts.FrameworkBase;
using YuanTu.ZheJiangHospital.ICBC;

namespace YuanTu.ZheJiangHospital.Component.Recharge.Models
{
    public interface IRechargeModel : IModel
    {
        bool Success { get; set; }
        decimal Balance { get; set; }
        decimal NewBalance { get; set; }

        int[] Counts { get; set; }
        Req充值 Req充值 { get; set; }
        Res充值 Res充值 { get; set; }
        //IcbcRequest Req充值 { get; set; }
        //IcbcResponse Res充值 { get; set; }
    }

    internal class RechargeModel : BindableBase, IRechargeModel
    {
        public bool Success { get; set; }
        public decimal Balance { get; set; }
        public decimal NewBalance { get; set; }
        public int[] Counts { get; set; }

        public Req充值 Req充值 { get; set; }

        public Res充值 Res充值 { get; set; }
        //public IcbcRequest Req充值 { get; set; }
        //public IcbcResponse Res充值 { get; set; }
    }
}
using Prism.Mvvm;
using YuanTu.Consts.FrameworkBase;
using YuanTu.ZheJiangHospital.ICBC;

namespace YuanTu.ZheJiangHospital.Component.Recharge.Models
{
    public interface IAccountModel:IModel
    {
        bool HasAccount { get; set; }
        string AccountNo { get; set; }
        string AccountId { get; set; }

        decimal Balance { get; set; }

        Req查询虚拟账户余额 Req查询虚拟账户余额 { get; set; }
        Res查询虚拟账户余额 Res查询虚拟账户余额 { get; set; }

        //IcbcRequest Req查询虚拟账户余额 { get; set; }
        //IcbcResponse Res查询虚拟账户余额 { get; set; }
    }

    public class AccountModel : BindableBase, IAccountModel
    {
        public bool HasAccount { get; set; }
        public string AccountNo { get; set; }
        public string AccountId { get; set; }
        public decimal Balance { get; set; }

        public Req查询虚拟账户余额 Req查询虚拟账户余额 { get; set; }

        public Res查询虚拟账户余额 Res查询虚拟账户余额 { get; set; }
        //public IcbcRequest Req查询虚拟账户余额 { get; set; }
        //public IcbcResponse Res查询虚拟账户余额 { get; set; }
    }
}

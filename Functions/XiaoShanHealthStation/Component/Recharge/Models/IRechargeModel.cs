using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YuanTu.Consts.FrameworkBase;
using YuanTu.XiaoShanArea.CitizenCard;

namespace YuanTu.XiaoShanHealthStation.Component.Recharge.Models
{
    public interface IRechargeModel : IModel
    {
        Req市民卡账户充值 Req市民卡账户充值 { get; set; }
        Res市民卡账户充值 Res市民卡账户充值 { get; set; }
        Res密码回显 Res密码回显 { get; set; }
        string DCardNo { get; set; }
        string Password { get; set; }
        string OldPassword { get; set; }
        string NewPassword { get; set; }
}

    public class RechargeModel:ModelBase,IRechargeModel
    {
     
        public Req市民卡账户充值 Req市民卡账户充值 { get; set; }
        public Res市民卡账户充值 Res市民卡账户充值 { get; set; }
        public string DCardNo { get; set; }
        public string Password { get; set; }
        public Res密码回显 Res密码回显 { get; set; }
        public string OldPassword { get; set; }
        public string NewPassword { get; set; }
    }
}

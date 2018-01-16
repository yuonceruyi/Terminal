using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YuanTu.Consts.Enums;
using YuanTu.Consts.FrameworkBase;
using YuanTu.Core.Log;
using YuanTu.XiaoShanZYY.CitizenCard;
using YuanTu.XiaoShanZYY.HIS;

namespace YuanTu.XiaoShanZYY.Component.Auth.Models
{

    public interface IAuthModel : IModel
    {
        Res读接触卡号 Res读接触卡号 { get; set; }
        Res读非接触卡号 Res读非接触卡号 { get; set; }
        CardType RealCardType { get; set; }
        Res病人信息查询建档 人员信息 { get; set; }
        PatientInfo Info { get; set; }
    }
    public class AuthModel : ModelBase, IAuthModel
    {
        public Res读接触卡号 Res读接触卡号 { get; set; }
        public Res读非接触卡号 Res读非接触卡号 { get; set; }
        public CardType RealCardType { get; set; }
        public Res病人信息查询建档 人员信息 { get; set; }

        public PatientInfo Info { get; set; }
    }
    public class PatientInfo
    {
        public string CardNo { get; set; }
        public string CardType { get; set; }
        public string PatientType { get; set; }

        public string Name { get; set; }
        public string IdNo { get; set; }
        public decimal Remain { get; set; }

        public bool HaveSMK { get; set; }
        public bool NoAccount { get; set; } //市民卡账户
        public bool NoSmartHealth { get; set; } //智慧医疗账户
    }
}

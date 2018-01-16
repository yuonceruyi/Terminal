using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YuanTu.Consts.FrameworkBase;
using YuanTu.XiaoShanArea.CitizenCard;
using YuanTu.XiaoShanArea.Consts.Enums;
using YuanTu.XiaoShanArea.CYHIS.DLL;

namespace YuanTu.XiaoShanHealthStation.Component.Auth.Models
{
    public interface IChaKaModel:IModel
    {
        Res读接触非接卡号 Res读接触非接卡号 { get; set; }
        bool IsCitizenCard { get; set; }
        Res可扣查询_SMK Res可扣查询Smk { get; set; }
        Res可扣查询_JKK Res可扣查询Jkk { get; set; }
        string CardNo { get; set; }
        decimal Remain { get; set; }
        string PatientType { get; set; }
        查询建档_OUT 查询建档Out { get; set; }

        bool HasAccount { get; set; }
        bool HasSmartHealth { get; set; }
        string CardType { get; set; }
    }

    public class ChaKaModel : ModelBase, IChaKaModel
    {
        public Res读接触非接卡号 Res读接触非接卡号 { get; set; }
        public bool IsCitizenCard { get; set; }
        public Res可扣查询_SMK Res可扣查询Smk { get; set; }
        public Res可扣查询_JKK Res可扣查询Jkk { get; set; }
        public string CardNo { get; set; }
        public decimal Remain { get; set; }
        public string PatientType { get; set; }
        public 查询建档_OUT 查询建档Out { get; set; }
        public bool HasAccount { get; set; }
        public bool HasSmartHealth { get; set; }
        public string CardType { get; set; }
    }
}

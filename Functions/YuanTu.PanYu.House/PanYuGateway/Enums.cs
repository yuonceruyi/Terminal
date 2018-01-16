using System.Xml.Serialization;

namespace YuanTu.PanYu.House.PanYuGateway
{
    public enum AuthMode
    {
        验证,
        补卡,
        补本,
    }

    public enum CardType
    {
        [XmlEnum("0")]
        NoCard = 0,

        [XmlEnum("1")]
        IDCard = 1,

        [XmlEnum("2")]
        MagCard = 2,

        [XmlEnum("3")]
        HICard = 3,
    }

    public enum sex
    {
        [XmlEnum("1")]
        男 = 1,

        [XmlEnum("2")]
        女 = 2,
    }

    public enum regtype
    {
        [XmlEnum("1")]
        预约 = 1,

        [XmlEnum("2")]
        挂号 = 2,
    }

    public enum regclass
    {
        [XmlEnum("1")]
        普通 = 1,

        [XmlEnum("2")]
        专家 = 2,

        [XmlEnum("3")]
        名医 = 3,

        [XmlEnum("4")]
        急诊 = 4,
    }

    public enum streak
    {
        [XmlEnum("")]
        全天 = 0,

        [XmlEnum("1")]
        上午 = 1,

        [XmlEnum("2")]
        下午 = 2,
    }

    public enum preway
    {
        [XmlEnum("1")]
        院内 = 1,

        [XmlEnum("2")]
        省厅 = 2,
    }

    public enum regstatus
    {
        [XmlEnum("0")]
        已预约 = 0,

        [XmlEnum("1")]
        已挂号 = 1,

        [XmlEnum("2")]
        已退 = 2
    }

    public enum cardstatus
    {
        [XmlEnum("1")]
        未开户 = 1,

        [XmlEnum("2")]
        已开户 = 2,
    }

    public enum ghbzbz
    {
        [XmlEnum("0")]
        普通 = 0,

        [XmlEnum("1")]
        规定病种 = 1,
    }

    public enum regMode
    {
        [XmlEnum("1")]
        预约 = 1,

        [XmlEnum("2")]
        挂号 = 2,

      
    }

    public enum regType
    {
        [XmlEnum("1")]
        普通 = 1,

        [XmlEnum("2")]
        专家 = 2,

        [XmlEnum("3")]
        名医 = 3,

        [XmlEnum("4")]
        急诊 = 4,

        [XmlEnum("5")]
        免费 = 5
    }

    public enum searchType
    {
        [XmlEnum("1")]
        挂号 = 1,

        [XmlEnum("2")]
        预约 = 2,
    }

    public enum status
    {
        [XmlEnum("1")]
        查询可取号信息 = 1,

        [XmlEnum("2")]
        查询可退号信息 = 2,
    }

    public enum visitFrom
    {
        [XmlEnum("1")]
        门诊 = 1,

        [XmlEnum("2")]
        住院 = 2,
    }

    public enum rechargeMode
    {
        [XmlEnum("1")]
        门诊充值 = 1,

        [XmlEnum("2")]
        住院充值 = 2,
    }

    public enum certType
    {
        [XmlEnum("0")]
        本人身份证 = 0,

        [XmlEnum("1")]
        监护人身份证 = 1,

        [XmlEnum("2")]
        其他 = 2,
    }

    public enum txnChnl
    {
        [XmlEnum("1")]
        柜面 = 1,

        [XmlEnum("2")]
        自助终端 = 2,

        [XmlEnum("3")]
        HIS = 3,

        [XmlEnum("4")]
        老卡系统 = 4,

        [XmlEnum("5")]
        广电BOSS系统 = 5,
    }

    public enum txnCode
    {
        [XmlEnum("1002")]
        民生卡工本费 = 1002,

        [XmlEnum("1001")]
        民生卡开卡 = 1001,

        [XmlEnum("1013")]
        民生卡终端签到 = 1013,

        [XmlEnum("2001")]
        民生卡余额查询 = 2001,

        [XmlEnum("2002")]
        民生卡交易明细查询 = 2002,

        [XmlEnum("2003")]
        民生卡充值 = 2003,

        [XmlEnum("2004")]
        民生卡充值冲正 = 2004,

        [XmlEnum("2008")]
        民生卡消费 = 2008,

        [XmlEnum("2009")]
        民生卡消费冲正 = 2009,

        [XmlEnum("2010")]
        银联卡消费登记 = 2010,

        [XmlEnum("2011")]
        民生卡退费 = 2011,

        [XmlEnum("1014")]
        民生卡客户信息更新 = 1014,

        [XmlEnum("1011")]
        民生卡重置密码 = 1011,

        [XmlEnum("1009")]
        民生卡卡片信息查询 = 1009,

        [XmlEnum("1010")]
        民生卡密码修改 = 1010,

        [XmlEnum("1018")]
        CPU卡密码设置 = 1018,
    }

    public enum chargeType
    {
        [XmlEnum("0")]
        现金 = 0,

        [XmlEnum("1")]
        银联卡 = 1,

        [XmlEnum("2")]
        支付宝 = 2,

        [XmlEnum("3")]
       微信 = 3,

       [XmlEnum("4")]
        余额查询 = 4,

        [XmlEnum("5")]
        重置密码 = 5,

        [XmlEnum("6")]
        密码修改 = 6,

        [XmlEnum("7")]
        民生卡 = 7,

        [XmlEnum("8")]
        CPU卡密码重置 = 8,

        [XmlEnum("9")]
        停车收费=9,

      
    }

    public enum payFlag
    {
        [XmlEnum("0")]
        非诊间 = 0,

        [XmlEnum("1")]
        诊间支付 = 1,
    }

    public enum regInType
    {
        当天挂号,快速挂号
    }

    public enum queryType
    {
        检查报告, 影像报告, 门诊费用, 药品信息, 住院清单,民生卡交易明细,接种清单
    }

    public enum payMode
    {
        银联卡缴费,民生卡缴费,医保银联卡缴费,公费
    }
}
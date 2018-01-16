using System.Collections.Generic;
using YuanTu.Consts.FrameworkBase;
using YuanTu.ShenZhenArea.Enums;
using YuanTu.ShenZhenArea.Gateway;

namespace YuanTu.ShenZhenArea.Models
{

    public interface IYBModel:IModel
    {
        bool IsYBPat { get; set; }
        string 医保卡号 { get; set; }
        string 医疗证号 { get; set; }
        string 医保密码 { get; set; }
        Cblx 参保类型 { get;  }
      
        double 总额 { get;  }
        double 记账金额 { get;  }
        double 现金金额 { get;  }
        医保门诊挂号登记 医保门诊挂号登记 { get; set; }
        门诊登记 门诊登记 { get; set; }
        List<门诊费用> 门诊费用列表 { get; set; }
        res医保个人基本信息查询 Res医保个人基本信息 { get; set; }
        医保个人基本信息 医保个人基本信息 { get;  }
        res医保门诊挂号 Res医保门诊挂号 { get; set; }
        List<医保门诊挂号结果> 医保门诊挂号结果 { get;}
        res医保门诊登记 Res医保门诊登记 { get; set; }
        res医保门诊费用 Res医保门诊费用 { get; set; }
        医保门诊费用结果 医保门诊费用结果 { get;  }
        res医保门诊退费 Res医保门诊退费 { get; set; }
        医保门诊退费结果 医保门诊退费结果 { get;  }
        res医保门诊支付确认 Res医保门诊支付确认 { get; set; }

        List<门诊结算结果> 门诊结算结果 { get;  }
        List<门诊支付> 门诊支付 { get;}
        门诊支付结果 门诊支付结果 { get;  }

        double 账户支付额 { get; set; }
        double 自费 { get; set; }
        double 比例自付 { get; set; }
        double 记账前 { get; set; }
        double 记账后 { get; set; }

        string HIS结算所需医保信息 { get; set; }
        string 就诊记录ID { get; set; }
        string 科室名称 { get; set; }
        string 科室编码 { get; set; }
        医保扩展信息 医保扩展信息 { get; set; }
        /// <summary>
        /// 门诊流水号
        /// </summary>
        string mzlsh { get; set; }
        /// <summary>
        /// 单据号
        /// </summary>
        string djh { get; set; }
        /// <summary>
        /// 单据号2
        /// </summary>
        string djh2 { get; set; }


        string HIS挂号所需医保信息 { get; set; }

    }
}

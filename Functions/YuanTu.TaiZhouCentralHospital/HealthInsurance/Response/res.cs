 using System;
using System.Collections.Generic;
using System.Text;
namespace YuanTu.TaiZhouCentralHospital.HealthInsurance.Response
{

    public class Res获取参保人信息结果:ResBase
    {
        public string 个人基本信息 { get; set; }
        public string 医疗身份验证结果 { get; set; }
        public string 工伤身份验证结果 { get; set; }
        public string 生育身份验证结果 { get; set; }
        public string 预留位 { get; set; }
       

    }

    public class Res交易确认结果:ResBase
    {
        public string 交易流水号 { get; set; }
       

    }

    public class Res交易结果查询结果:ResBase
    {
        public string 用户交易是否成功 { get; set; }
        public string 交易时间 { get; set; }
        public string 交易结算流水号 { get; set; }
        public string 交易处于阶段 { get; set; }
        public string 该用户交易出口参数 { get; set; }
        public string 基金分段信息结构体 { get; set; }
        public string 费用汇总信息结构体 { get; set; }
       

    }

    public class Res医保预结算结果:ResBase
    {
        public string 超限提示标记 { get; set; }
        public string 规定病种标志 { get; set; }
        public string 结算时间 { get; set; }
        public string 计算结果信息 { get; set; }
        public string 基金分段信息结构体 { get; set; }
        public string 超限明细列表 { get; set; }
        public string 自负比例不对列表 { get; set; }
       

    }

    public class Res医保结算结果:ResBase
    {
        public string 超限提示标记 { get; set; }
        public string 规定病种标志 { get; set; }
        public string 结算时间 { get; set; }
        public string 结算流水号 { get; set; }
        public string 计算结果信息 { get; set; }
        public string 基金分段信息结构体 { get; set; }
        public string 超限明细列表 { get; set; }
        public string 自负比例不对列表 { get; set; }
       

    }


}
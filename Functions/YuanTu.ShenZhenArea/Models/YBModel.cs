using System;
using System.Collections.Generic;
using System.ComponentModel;
using YuanTu.Consts.FrameworkBase;
using YuanTu.ShenZhenArea.Enums;
using YuanTu.ShenZhenArea.Gateway;

namespace YuanTu.ShenZhenArea.Models
{
    public class YBModel : ModelBase, IYBModel
    {
        public bool IsYBPat { get; set; }
        public string 医保卡号 { get; set; }
        public string 医疗证号 { get; set; }

        public string 医保密码 { get; set; }

        public Cblx 参保类型
        {
            get { return (Cblx)(Convert.ToInt32(医保个人基本信息?.CBLX ?? "0")); }
        }

        public double 总额
        {
            get { return Convert.ToDouble(医保门诊费用结果?.theMZZFJG.JE); }
        }

        public double 记账金额
        {
            get { return Convert.ToDouble(医保门诊费用结果?.theMZZFJG.JZHJ); }
        }

        public double 现金金额
        {
            get { return Convert.ToDouble(医保门诊费用结果?.theMZZFJG.XJHJ); }
        }

        public 医保门诊挂号登记 医保门诊挂号登记 { get; set; }

        public 门诊登记 门诊登记 { get; set; }

        public List<门诊费用> 门诊费用列表 { get; set; }

        public res医保个人基本信息查询 Res医保个人基本信息 { get; set; }

        public 医保个人基本信息 医保个人基本信息
        {
            get { return Res医保个人基本信息?.data; }
        }



        public res医保门诊挂号 Res医保门诊挂号 { get; set; }

        public List<医保门诊挂号结果> 医保门诊挂号结果
        {
            get { return Res医保门诊挂号?.data; }
        }

        public res医保门诊登记 Res医保门诊登记 { get; set; }
        public res医保门诊费用 Res医保门诊费用 { get; set; }

        public 医保门诊费用结果 医保门诊费用结果
        {
            get { return Res医保门诊费用?.data; }
        }

        public res医保门诊退费 Res医保门诊退费 { get; set; }

        public 医保门诊退费结果 医保门诊退费结果
        {
            get { return Res医保门诊退费?.data; }
        }

        public res医保门诊支付确认 Res医保门诊支付确认 { get; set; }

        public List<门诊结算结果> 门诊结算结果
        {
            get { return Res医保门诊费用?.data.theMZJS; }
        }

        public List<门诊支付> 门诊支付
        {
            get { return Res医保门诊费用?.data.theMZZF; }
        }

        public 门诊支付结果 门诊支付结果
        {
            get { return Res医保门诊费用?.data.theMZZFJG; }
        }

        public double 账户支付额 { get; set; }
        public double 自费 { get; set; }
        public double 比例自付 { get; set; }
        public double 记账前 { get; set; }
        public double 记账后 { get; set; }
        public string HIS结算所需医保信息 { get; set; }
        public string 就诊记录ID { get; set; }
        public string 科室名称 { get; set; }
        public string 科室编码 { get; set; }
        public 医保扩展信息 医保扩展信息 { get; set; }

        public string mzlsh { get; set; }

        public string djh { get; set; }

        public string djh2{ get; set; }
        public string HIS挂号所需医保信息 { get; set ; }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
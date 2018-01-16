 using System;
using System.Collections.Generic;
using System.Text;
namespace YuanTu.JiaShanHospital.HealthInsurance.Response
{

    public class 个人基本信息
    {
        public string 卡号 { get; set; }
        public string 个人社保编号 { get; set; }
        public string 姓名 { get; set; }
        public string 性别 { get; set; }
        public string 民族 { get; set; }
        public string 出生日期 { get; set; }
        public string 公民身份号 { get; set; }
        public string 单位性质 { get; set; }
        public string 单位名称 { get; set; }
        public string 地区编码 { get; set; }
        public string 地区名称 { get; set; }
        public string 医保待遇类别 { get; set; }
        public string 荣誉类别 { get; set; }
        public string 低保类别 { get; set; }
        public string 优抚级别 { get; set; }
        public string 特殊病标志 { get; set; }
        public string 特殊病编码 { get; set; }
        public string 当年帐户余额 { get; set; }
        public string 历年帐户余额 { get; set; }
        public string 当年住院医保累计 { get; set; }
        public string 当年门诊医保累计 { get; set; }
        public string 当年规定病医保累计 { get; set; }
        public string 当年累计列入统筹基数 { get; set; }
        public string 当年统筹支付医保费用累计 { get; set; }
        public string 当年统筹基金支付累计 { get; set; }
        public string 当年补充保险支付累计 { get; set; }
        public string 当年公务员补助支付累计 { get; set; }
        public string 当年专项基金支付累计 { get; set; }
        public string 当年住院次数 { get; set; }
        public string 当年规定病次数 { get; set; }
        public string 工伤认定部位 { get; set; }
        public string 医疗小险种 { get; set; }
        public override string ToString()
	    {
	      var sb=new StringBuilder();
		  sb.Append($"卡号:{卡号}\n");
		  sb.Append($"个人社保编号:{个人社保编号}\n");
		  sb.Append($"姓名:{姓名}\n");
		  sb.Append($"性别:{性别}\n");
		  sb.Append($"民族:{民族}\n");
		  sb.Append($"出生日期:{出生日期}\n");
		  sb.Append($"公民身份号:{公民身份号}\n");
		  sb.Append($"单位性质:{单位性质}\n");
		  sb.Append($"单位名称:{单位名称}\n");
		  sb.Append($"地区编码:{地区编码}\n");
		  sb.Append($"地区名称:{地区名称}\n");
		  sb.Append($"医保待遇类别:{医保待遇类别}\n");
		  sb.Append($"荣誉类别:{荣誉类别}\n");
		  sb.Append($"低保类别:{低保类别}\n");
		  sb.Append($"优抚级别:{优抚级别}\n");
		  sb.Append($"特殊病标志:{特殊病标志}\n");
		  sb.Append($"特殊病编码:{特殊病编码}\n");
		  sb.Append($"当年帐户余额:{当年帐户余额}\n");
		  sb.Append($"历年帐户余额:{历年帐户余额}\n");
		  sb.Append($"当年住院医保累计:{当年住院医保累计}\n");
		  sb.Append($"当年门诊医保累计:{当年门诊医保累计}\n");
		  sb.Append($"当年规定病医保累计:{当年规定病医保累计}\n");
		  sb.Append($"当年累计列入统筹基数:{当年累计列入统筹基数}\n");
		  sb.Append($"当年统筹支付医保费用累计:{当年统筹支付医保费用累计}\n");
		  sb.Append($"当年统筹基金支付累计:{当年统筹基金支付累计}\n");
		  sb.Append($"当年补充保险支付累计:{当年补充保险支付累计}\n");
		  sb.Append($"当年公务员补助支付累计:{当年公务员补助支付累计}\n");
		  sb.Append($"当年专项基金支付累计:{当年专项基金支付累计}\n");
		  sb.Append($"当年住院次数:{当年住院次数}\n");
		  sb.Append($"当年规定病次数:{当年规定病次数}\n");
		  sb.Append($"工伤认定部位:{工伤认定部位}\n");
		  sb.Append($"医疗小险种:{医疗小险种}\n");
          return sb.ToString();
        }

    }


}
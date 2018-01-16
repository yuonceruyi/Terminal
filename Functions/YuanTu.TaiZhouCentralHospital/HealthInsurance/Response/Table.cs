 using System;
using System.Collections.Generic;
using System.Text;
namespace YuanTu.TaiZhouCentralHospital.HealthInsurance.Response
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

    public class 计算结果信息
    {
        public string 费用总额 { get; set; }
        public string 自费总额_丙类 { get; set; }
        public string 自理总额_乙类 { get; set; }
        public string 医保费用 { get; set; }
        public string 转院自费 { get; set; }
        public string 起付线 { get; set; }
        public string 个人自费现金支付 { get; set; }
        public string 个人自负现金支付 { get; set; }
        public string 合计现金支付 { get; set; }
        public string 合计报销金额 { get; set; }
        public string 历年帐户支付 { get; set; }
        public string 当年帐户支付 { get; set; }
        public string 统筹基金支付 { get; set; }
        public string 大病补助支付 { get; set; }
        public string 公务员补助支付 { get; set; }
        public string 离休基金支付 { get; set; }
        public string  二乙基金支付 { get; set; }
        public string 劳模医疗补助支付 { get; set; }
        public string 大额自付补助 { get; set; }
        public string 个人自负 { get; set; }
        public string 工伤基金支付 { get; set; }
        public string 生育基金支付 { get; set; }
        public string 结算后当年个帐余额 { get; set; }
        public string 结算后历年个帐余额 { get; set; }
        public string 门诊统筹基金支付 { get; set; }
        public string 门诊统筹医保费用 { get; set; }
        public string 门诊统筹起付线 { get; set; }
        public string 民政补助 { get; set; }
        public string 优抚救助 { get; set; }
        public override string ToString()
	    {
	      var sb=new StringBuilder();
		  sb.Append($"费用总额:{费用总额}\n");
		  sb.Append($"自费总额_丙类:{自费总额_丙类}\n");
		  sb.Append($"自理总额_乙类:{自理总额_乙类}\n");
		  sb.Append($"医保费用:{医保费用}\n");
		  sb.Append($"转院自费:{转院自费}\n");
		  sb.Append($"起付线:{起付线}\n");
		  sb.Append($"个人自费现金支付:{个人自费现金支付}\n");
		  sb.Append($"个人自负现金支付:{个人自负现金支付}\n");
		  sb.Append($"合计现金支付:{合计现金支付}\n");
		  sb.Append($"合计报销金额:{合计报销金额}\n");
		  sb.Append($"历年帐户支付:{历年帐户支付}\n");
		  sb.Append($"当年帐户支付:{当年帐户支付}\n");
		  sb.Append($"统筹基金支付:{统筹基金支付}\n");
		  sb.Append($"大病补助支付:{大病补助支付}\n");
		  sb.Append($"公务员补助支付:{公务员补助支付}\n");
		  sb.Append($"离休基金支付:{离休基金支付}\n");
		  sb.Append($" 二乙基金支付:{ 二乙基金支付}\n");
		  sb.Append($"劳模医疗补助支付:{劳模医疗补助支付}\n");
		  sb.Append($"大额自付补助:{大额自付补助}\n");
		  sb.Append($"个人自负:{个人自负}\n");
		  sb.Append($"工伤基金支付:{工伤基金支付}\n");
		  sb.Append($"生育基金支付:{生育基金支付}\n");
		  sb.Append($"结算后当年个帐余额:{结算后当年个帐余额}\n");
		  sb.Append($"结算后历年个帐余额:{结算后历年个帐余额}\n");
		  sb.Append($"门诊统筹基金支付:{门诊统筹基金支付}\n");
		  sb.Append($"门诊统筹医保费用:{门诊统筹医保费用}\n");
		  sb.Append($"门诊统筹起付线:{门诊统筹起付线}\n");
		  sb.Append($"民政补助:{民政补助}\n");
		  sb.Append($"优抚救助:{优抚救助}\n");
          return sb.ToString();
        }

    }

    public class 基金分段结构体参数
    {
        public string 分段编码 { get; set; }
        public string 分段名称 { get; set; }
        public string 进入额度 { get; set; }
        public string 分段基金支付金额 { get; set; }
        public string 报销比例 { get; set; }
        public string 分段自负金额 { get; set; }
        public string 分段自费金额 { get; set; }
        public override string ToString()
	    {
	      var sb=new StringBuilder();
		  sb.Append($"分段编码:{分段编码}\n");
		  sb.Append($"分段名称:{分段名称}\n");
		  sb.Append($"进入额度:{进入额度}\n");
		  sb.Append($"分段基金支付金额:{分段基金支付金额}\n");
		  sb.Append($"报销比例:{报销比例}\n");
		  sb.Append($"分段自负金额:{分段自负金额}\n");
		  sb.Append($"分段自费金额:{分段自费金额}\n");
          return sb.ToString();
        }

    }

    public class 超限列表结构体参数
    {
        public string 药品诊疗类型 { get; set; }
        public string 医院项目编码 { get; set; }
        public string 超限数量 { get; set; }
        public string 超限自费金额 { get; set; }
        public string 合计自费金额  { get; set; }
        public string 超限原因代码 { get; set; }
        public string 超限原因说明 { get; set; }
        public override string ToString()
	    {
	      var sb=new StringBuilder();
		  sb.Append($"药品诊疗类型:{药品诊疗类型}\n");
		  sb.Append($"医院项目编码:{医院项目编码}\n");
		  sb.Append($"超限数量:{超限数量}\n");
		  sb.Append($"超限自费金额:{超限自费金额}\n");
		  sb.Append($"合计自费金额 :{合计自费金额 }\n");
		  sb.Append($"超限原因代码:{超限原因代码}\n");
		  sb.Append($"超限原因说明:{超限原因说明}\n");
          return sb.ToString();
        }

    }

    public class 自负比例不对列表
    {
        public string 单据号码 { get; set; }
        public string 项目医院编号 { get; set; }
        public string 医院自负比例 { get; set; }
        public string 中心自负比例 { get; set; }
        public override string ToString()
	    {
	      var sb=new StringBuilder();
		  sb.Append($"单据号码:{单据号码}\n");
		  sb.Append($"项目医院编号:{项目医院编号}\n");
		  sb.Append($"医院自负比例:{医院自负比例}\n");
		  sb.Append($"中心自负比例:{中心自负比例}\n");
          return sb.ToString();
        }

    }


}
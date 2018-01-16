using System.Collections.Generic;
namespace YuanTu.ShenZhenArea.Gateway
{
    
    public partial class 医保个人基本信息查询
    {

		public string yljgbm { get; set; }

		public string ylzh { get; set; }

		public string mm { get; set; }

		public string czybm { get; set; }

		public string czyxm { get; set; }
    }
    
    public partial class 医保个人基本信息
    {

		public string YLZH { get; set; }

		public string DNH { get; set; }

		public string JHR1DNH { get; set; }

		public string JHR2DNH { get; set; }

		public string SFZH { get; set; }

		public string JHR1SFZH { get; set; }

		public string JHR2SFZH { get; set; }

		public string XM { get; set; }

		public string JHR1XM { get; set; }

		public string JHR2XM { get; set; }

		public string XB { get; set; }

		public string CSSJ { get; set; }

		public string NL { get; set; }

		public string TSRQ { get; set; }

		public string DWBM { get; set; }

		public string DWMC { get; set; }

		public string JYZT { get; set; }

		public string CBLX { get; set; }

		public string JFLY { get; set; }

		public string GLSD { get; set; }

		public string YLFLAG { get; set; }

		public string TCFLAG { get; set; }

		public string LXFLAG { get; set; }

		public string SYFLAG { get; set; }

		public string GSFLAG { get; set; }

		public string SEFLAG { get; set; }

		public string ACCOUNT { get; set; }

		public string NY { get; set; }

		public string LXCBYS { get; set; }

		public string JBZGXE { get; set; }

		public string JBYYJE { get; set; }

		public string JBKYYE { get; set; }

		public string BCZGXE { get; set; }

		public string BCYYJE { get; set; }

		public string BCKYYE { get; set; }

		public string BZZFXE { get; set; }

		public string BZZFYYJE { get; set; }

		public string BZZFKYYE { get; set; }

		public string BDSK { get; set; }

		public string BDJSYY { get; set; }

		public string NDBGXE { get; set; }

		public string NDBGYY { get; set; }

		public string NDBGKY { get; set; }
    }
    
    public partial class 病人信息
    {

		public string patientId { get; set; }

		public string platformId { get; set; }

		public string name { get; set; }

		public string sex { get; set; }

		public string birthday { get; set; }

		public string idNo { get; set; }

		public string cardNo { get; set; }

		public string guardianNo { get; set; }

		public string address { get; set; }

		public string phone { get; set; }

		public string patientType { get; set; }

		public string accountNo { get; set; }

		public string accBalance { get; set; }
    }
    
    public partial class 医保个人绑定信息查询
    {

		public string yljgbm { get; set; }

		public string ylzh { get; set; }

		public string mm { get; set; }

		public string czybm { get; set; }

		public string czyxm { get; set; }
    }
    
    public partial class 医保个人绑定信息
    {

		public string YLZH { get; set; }

		public string DNH { get; set; }

		public string XM { get; set; }

		public string BDSK { get; set; }

		public string BDJSYY { get; set; }

		public string NDBGXE { get; set; }

		public string NDBGYY { get; set; }

		public string NDBGKY { get; set; }
    }
    
    public partial class 医保门诊挂号
    {

		public string yljgbm { get; set; }

		public string ylzh { get; set; }

		public string mm { get; set; }

		public string czybm { get; set; }

		public string czyxm { get; set; }

		public 医保门诊挂号登记 mzghdj { get; set; }
    }
    
    public partial class 医保门诊挂号登记
    {

		public string bRLX { get; set; }

		public string mZLSH { get; set; }

		public string kSBM { get; set; }

		public string kSMC_str { get; set; }

		public string gHLB { get; set; }

		public string gHF { get; set; }

		public string zJZJ { get; set; }

		public string gHHJ { get; set; }

		public string sSJ { get; set; }

		public string zSJ { get; set; }
    }
    
    public partial class 医保门诊挂号结果
    {

		public string ZFXM { get; set; }

		public string JE { get; set; }
    }
    
    public partial class 医保门诊登记
    {

		public string yljgbm { get; set; }

		public string czybm { get; set; }

		public string czyxm { get; set; }

		public 门诊登记 mzdj { get; set; }
    }
    
    public partial class 门诊登记
    {

		public string mZLSH { get; set; }

		public string mZLB { get; set; }

		public string tBLB { get; set; }

		public string tJLB { get; set; }

		public string zD { get; set; }

		public string zDSM_str { get; set; }

		public string cFZS { get; set; }

		public string ySBM { get; set; }

		public string ySXM_str { get; set; }

		public string ySDH { get; set; }
    }
    
    public partial class 医保门诊费用
    {

		public string yljgbm { get; set; }

		public string czybm { get; set; }

		public string czyxm { get; set; }

		public string mzlsh { get; set; }

		public string djh { get; set; }

		public List<门诊费用> mzfydetail { get; set; }
    }
    
    public partial class 门诊费用
    {

		public string nO { get; set; }

		public string yLJGNBBM { get; set; }

		public string tYBM { get; set; }

		public string jSXM { get; set; }

		public string mC_str { get; set; }

		public string gG_str { get; set; }

		public string dW_str { get; set; }

		public string dJ { get; set; }

		public string sL { get; set; }

		public string hJJE { get; set; }
    }
    
    public partial class 医保门诊费用结果
    {

		public List<门诊结算结果> theMZJS { get; set; }

		public List<门诊支付> theMZZF { get; set; }

		public 门诊支付结果 theMZZFJG { get; set; }
    }
    
    public partial class 门诊结算结果
    {

		public string JSXM { get; set; }

		public string JE { get; set; }
    }
    
    public partial class 门诊支付
    {

		public string ZFXM { get; set; }

		public string JE { get; set; }
    }
    
    public partial class 门诊支付结果
    {

		public string YLJGBM { get; set; }

		public string YLJGMC { get; set; }

		public string MZLSH { get; set; }

		public string DJH { get; set; }

		public string YLZH { get; set; }

		public string DNH { get; set; }

		public string XM { get; set; }

		public string JE { get; set; }

		public string XJHJ { get; set; }

		public string JZHJ { get; set; }
    }
    
    public partial class 医保门诊退费
    {

		public string yljgbm { get; set; }

		public string czybm { get; set; }

		public string czyxm { get; set; }

		public string mzlsh { get; set; }

		public string djh { get; set; }

		public string djh2 { get; set; }

		public List<门诊费用> theMZFY { get; set; }
    }
    
    public partial class 医保门诊退费结果
    {

		public List<门诊结算结果> theMZJS { get; set; }

		public List<门诊支付> theMZZF { get; set; }

		public 门诊支付结果 theMZZFJG { get; set; }
    }
    
    public partial class 医保门诊支付确认
    {

		public string yljgbm { get; set; }

		public string czybm { get; set; }

		public string czyxm { get; set; }

		public string mzlsh { get; set; }

		public string djh { get; set; }

		public string ssj { get; set; }

		public string zsj { get; set; }
    }
    
    public partial class 挂号扩展信息
    {

		public 医保挂号信息 gh { get; set; }
    }
    
    public partial class 结算扩展信息
    {

		public string regId { get; set; }

		public 医保挂号信息 gh { get; set; }

		public 医保结算信息 js { get; set; }
    }
    
    public partial class 医保挂号信息
    {

		public string INADMAdmDr { get; set; }

		public string INADMInsuId { get; set; }

		public string INADMCardNo  { get; set; }

		public string INADMPatType { get; set; }

		public string INADMCompany { get; set; }

		public string INADMAccount { get; set; }

		public string INADMAdmSeriNo { get; set; }

		public string INADMActiveFlag { get; set; }

		public string INADMAdmDate { get; set; }

		public string INADMAdmTime { get; set; }

		public string INADMAdmType { get; set; }

		public string INADMDeptDesc { get; set; }

		public string INADMInsuType { get; set; }

		public string INADMUserDr { get; set; }

		public string INADMXString1 { get; set; }

		public string INADMXString5 { get; set; }
    }
    
    public partial class 医保结算信息
    {

		public string INPAY_bcbxf0 { get; set; }

		public string INPAY_djlsh0 { get; set; }

		public string INPAY_grzfe0 { get; set; }

		public string INPAY_id0000 { get; set; }

		public string INPAY_jjzfe0 { get; set; }

		public string INPAY_sUserDr { get; set; }

		public string INPAY_ming0 { get; set; }

		public string INPAY_zhzfe0 { get; set; }

		public string INPAY_zyksmc { get; set; }

		public string INPAY_zylsh0 { get; set; }

		public string INPAY_InsuPay3 { get; set; }

		public string INPAY_InsuPay4 { get; set; }

		public string INPAY_Zstr08 { get; set; }

		public string INPAY_Zstr09 { get; set; }

		public string theMZJS { get; set; }

		public string theMZZF { get; set; }
    }
    
    public partial class 医保扩展信息
    {

		public string ybinfoexp { get; set; }

		public string ybinfo { get; set; }

		public string regId { get; set; }

		public string deptCode { get; set; }

		public string deptName { get; set; }
    }
    
    public partial class 结算记录
    {

		public string tradeTime { get; set; }

		public string receiptNo { get; set; }

		public string billFee { get; set; }

		public string itemName { get; set; }

		public string itemSpecs { get; set; }

		public string billType { get; set; }

		public string itemLiquid { get; set; }

		public string itemUnits { get; set; }

		public string itemQty { get; set; }

		public string itemPrice { get; set; }

		public string cost { get; set; }
    }
}
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YuanTu.Consts.FrameworkBase;
using YuanTu.ShenZhenArea.Enums;
using YuanTu.ShenZhenArea.Gateway;
using YuanTu.ShenZhenArea.Insurance;
using YuanTu.Consts.Gateway;
using YuanTu.Consts.Enums;
using YuanTu.Consts;

namespace YuanTu.ShenZhen.BaoAnCenterHospital.Services
{
    public class YBService : YuanTu.ShenZhenArea.Services.YBService
    {

        public override 门诊登记 门诊登记()
        {
            string[] djInfo = YB.医保扩展信息.ybinfoexp?.Split('^');
            //1   门诊类别	普通
            //2   门诊类别 特病
            //3   门诊类别 特检
            //4   门诊类别 病种
            //5   门诊类别 健康体检
            //6   门诊类别 预防接种
            //7   门诊类别 家庭通道

            if (JiaoFei.所选缴费概要 == null)  //所选缴费信息为空时 为挂号
            {
                return new 门诊登记
                {
                    mZLSH = YB.mzlsh,
                    mZLB = "1",
                    tBLB = "",
                    tJLB = "",
                    zD = djInfo.Length > 1 ? djInfo[1] : "",
                    zDSM_str = djInfo.Length > 2 ? djInfo[2] : "",
                    cFZS = "",
                    ySBM = ScheduleModel.所选排班.doctCode,
                    ySXM_str = ScheduleModel.所选排班.doctName,
                    ySDH = ""
                };
            }
            string mzlb = "1";
            if (JiaoFei.所选缴费概要.billType == "特检")
            {
                mzlb = "3";
            }

            return new 门诊登记
            {
                mZLSH = YB.mzlsh,
                mZLB = mzlb,        //"1",   
                tBLB = "",
                tJLB = djInfo.Length > 5 ? djInfo[5] : "",
                zD = djInfo.Length > 1 ? djInfo[1] : "",
                zDSM_str = djInfo.Length > 2 ? djInfo[2] : "",
                cFZS = "",
                ySBM = djInfo.Length > 3 ? djInfo[3] : "",
                ySXM_str = djInfo.Length > 4 ? djInfo[4] : "",
                ySDH = ""
            };
        }


        /// <summary>
        /// 挂号的时候不做0费用挂号
        /// </summary>
        /// <returns></returns>
        public override 医保门诊挂号登记 医保门诊挂号登记()
        {
            YB.mzlsh = GetChnlSsn();
            var kSMC_str = YB?.科室名称 ?? (ChoiceModel.Business == Business.挂号 ? DepartmentModel.所选科室.deptName : RecordModel.所选记录.deptName);
            if (ChoiceModel.Business == Business.挂号)
            {
                YB.djh = "#";
                //科室名称太长  医保挂号会失败
                kSMC_str = "";
                string RegFee = "";
                //extend 的格式如下：
                /*
                  {"AdmDoc":"普通门诊号","deptName":"","AdmDate":"2017-11-28","AdmTime":"上午","ybinfoexp":"1^^^01679^郑芳","ybinfo":"1^挂号费^707597^^173^0^1^0^110100001^Az!2^普通门诊诊查费^700003^^173^3^1^3^110200001^C","regId":"829||742","ZFUploadData":"1^挂号费^707597^^173^0^1^0^110100001^Az^1^0^110100001^Az!2^普通门诊诊查费^700003^^173^14^1^14^110200001^C","deptCode":"","RegFee":"3.00","TransactionId":""}
                 */
                Dictionary<string, string> list = JsonConvert.DeserializeObject<Dictionary<string, string>>(RegisterModel.Res挂号锁号?.data?.extend);
                if (list.ContainsKey("RegFee"))
                    RegFee = list["RegFee"];
                return new 医保门诊挂号登记
                {
                    bRLX = ((int)获取病人类型(YB.参保类型)).ToString(),
                    mZLSH = YB.mzlsh,
                    kSBM = YB?.科室编码 ?? (ChoiceModel.Business == Business.挂号 ? DepartmentModel.所选科室.deptCode : RecordModel.所选记录.deptCode),
                    kSMC_str = kSMC_str,
                    gHLB = "1",
                    gHF = "0",
                    zJZJ = RegFee,
                    gHHJ = RegFee,
                    sSJ = RegFee,
                    zSJ = "0"
                };
            }
            else
            {
                //科室名称太长  医保挂号会失败
                kSMC_str = "";
                return new 医保门诊挂号登记
                {
                    bRLX = ((int)获取病人类型(YB.参保类型)).ToString(),
                    mZLSH = YB.mzlsh,
                    kSBM = YB?.科室编码 ?? (ChoiceModel.Business == Business.挂号 ? DepartmentModel.所选科室.deptCode : RecordModel.所选记录.deptCode),
                    kSMC_str = kSMC_str,
                    gHLB = "1",
                    gHF = "0",
                    zJZJ = "0",
                    gHHJ = "0",
                    sSJ = "0",
                    zSJ = "0"
                };
            }
        }





        public override void 处理医保挂号结算信息()
        {
            /*
            9   自费
            6   劳务工医保
            1   住院医保
            7   少儿医保
            5   工伤医保
            1   综合医保
            */
            var INADMXString1_Value = "9";
            switch (获取病人类型(YB.参保类型))
            {
                case Brlx.劳务工医保:
                    INADMXString1_Value = "6";
                    break;
                case Brlx.医疗保险:
                    INADMXString1_Value = "1";
                    break;
                case Brlx.家属统筹医疗:
                    INADMXString1_Value = "";
                    break;
                case Brlx.少儿医保:
                    INADMXString1_Value = "7";
                    break;
                case Brlx.工伤医疗:
                    INADMXString1_Value = "5";
                    break;
                case Brlx.生育医疗:
                    INADMXString1_Value = "2";
                    break;
                case Brlx.离休医疗:
                    INADMXString1_Value = "3";
                    break;
                case Brlx.自费:
                    INADMXString1_Value = "9";
                    break;
                default:
                    break;
            }
            var yb = new 结算扩展信息();
            if ((ChoiceModel.Business == Business.挂号 && YB.账户支付额 > 0) || YB.记账金额 > 0)
            {
                var admDate = DateTimeCore.Now.ToString("yyyy-MM-dd");
                var admTime = DateTimeCore.Now.ToString("HH:mm:ss");
                if (JiaoFei.所选缴费概要 != null)
                {
                    if (JiaoFei.所选缴费概要.billDate.Length > 9)
                        admDate = JiaoFei.所选缴费概要.billDate.Substring(0, 10);
                    if (JiaoFei.所选缴费概要.billDate.Length > 19)
                        admTime = JiaoFei.所选缴费概要.billDate.Substring(11, 8);
                }

                yb.gh = new 医保挂号信息
                {
                    INADMAdmDr = YB.就诊记录ID,
                    INADMInsuId = YB.医保个人基本信息.DNH,
                    INADMCardNo = YB.医保个人基本信息.YLZH ?? "",
                    INADMPatType = YB.医保个人基本信息.JYZT,
                    INADMCompany = YB.医保个人基本信息.DWMC,
                    INADMAccount = YB.医保个人基本信息.ACCOUNT,
                    INADMAdmSeriNo = YB.mzlsh,
                    INADMActiveFlag = "A",
                    INADMAdmDate = admDate,
                    INADMAdmTime = admTime,
                    INADMAdmType = "1",
                    INADMDeptDesc = YB.科室名称,
                    INADMInsuType = "SZOP",
                    INADMUserDr = "",
                    INADMXString1 = INADMXString1_Value,
                    INADMXString5 = YB.医疗证号
                };
                if (ChoiceModel.Business != Business.挂号)
                    处理医保费用();

                yb.js = new ShenZhenArea.Gateway.医保结算信息
                {
                    INPAY_bcbxf0 = ChoiceModel.Business == Business.挂号 ? (YB.自费 + YB.账户支付额).ToString() : YB.门诊支付结果.JE,
                    INPAY_djlsh0 = YB.mzlsh,
                    INPAY_grzfe0 = ChoiceModel.Business == Business.挂号 ? YB.自费.ToString() : YB.门诊支付结果.XJHJ,
                    INPAY_id0000 = YB.医保个人基本信息.YLZH,
                    INPAY_jjzfe0 = ChoiceModel.Business == Business.挂号 ? YB.账户支付额.ToString() : YB.门诊支付结果.JZHJ,
                    INPAY_sUserDr = FrameworkConst.OperatorId,
                    INPAY_ming0 = YB.医保个人基本信息.XM,
                    INPAY_zhzfe0 = YB.账户支付额.ToString("F2"),
                    INPAY_zyksmc = YB.科室名称,
                    INPAY_zylsh0 = YB.djh,
                    INPAY_InsuPay3 = YB.自费.ToString("F2"),
                    INPAY_InsuPay4 = YB.比例自付.ToString("F2"),
                    INPAY_Zstr08 = YB.记账前.ToString("F2"),
                    INPAY_Zstr09 = YB.记账后.ToString("F2"),
                    theMZJS = "",
                    theMZZF = "",
                };

                if (ChoiceModel.Business == Business.挂号)
                {
                    yb.js.theMZJS = "";

                    yb.js.theMZZF = string.Join("", YB?.Res医保门诊挂号?.data.Select(d => "|" + d.ZFXM + ":" + d.JE).ToList());
                }
                else
                {
                    yb.js.theMZJS = string.Join("", YB?.Res医保门诊费用?.data.theMZJS.Select(d => "|" + d.JSXM + ":" + d.JE).ToList());
                    yb.js.theMZZF = string.Join("", YB?.Res医保门诊费用?.data.theMZZF.Select(d => "|" + d.ZFXM + ":" + d.JE).ToList());
                }
            }
            else
            {
                yb.gh = null;
                yb.js = null;
            }
            yb.regId = YB.就诊记录ID;
            YB.HIS结算所需医保信息 = JsonConvert.SerializeObject(yb);
        }


        public override void 处理医保费用()
        {
            YB.账户支付额 = 0;
            YB.自费 = 0;
            YB.比例自付 = 0;
            YB.记账前 = 0;
            YB.记账后 = 0;


            if (ChoiceModel.Business == Business.挂号)
            {
                foreach (var mzghItem in YB?.Res医保门诊挂号?.data)
                {
                    if (mzghItem.ZFXM == "0201")
                        YB.账户支付额 += Convert.ToDouble(mzghItem.JE);
                    if ((mzghItem.ZFXM == "0101") || (mzghItem.ZFXM == "0111") || (mzghItem.ZFXM == "0104"))
                        YB.自费 += Convert.ToDouble(mzghItem.JE);
                    if ((mzghItem.ZFXM == "0102") || (mzghItem.ZFXM == "0108") || (mzghItem.ZFXM == "0109") || (mzghItem.ZFXM == "0110") || (mzghItem.ZFXM == "0103") || (mzghItem.ZFXM == "0112"))
                        YB.比例自付 += Convert.ToDouble(mzghItem.JE);
                    if (mzghItem.ZFXM == "0303")
                        YB.记账前 += Convert.ToDouble(mzghItem.JE);
                    if (mzghItem.ZFXM == "0306")
                        YB.记账后 += Convert.ToDouble(mzghItem.JE);
                }
            }
            else
            {
                base.处理医保费用();
            }
        }





        public override void 获取挂号信息()
        {
            var info = RegisterModel.Res挂号锁号?.data?.extend;
            YB.医保扩展信息 = JsonConvert.DeserializeObject<医保扩展信息>(info);
            YB.就诊记录ID = YB.医保扩展信息.regId;
            YB.科室编码 = YB.医保扩展信息.deptCode ?? (ChoiceModel.Business == Business.挂号 ? DepartmentModel.所选科室.deptName : RecordModel.所选记录.deptName);
            YB.科室名称 = YB.医保扩展信息.deptName ?? (ChoiceModel.Business == Business.挂号 ? DepartmentModel.所选科室.deptName : RecordModel.所选记录.deptName);
        }



    }
}

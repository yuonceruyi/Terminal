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

namespace YuanTu.ShenZhen.BaoAnChineseMedicineHospital.Services
{
    public class YBService : YuanTu.ShenZhenArea.Services.YBService
    {
        ///// <summary>
        ///// 
        ///// </summary>
        ///// <param name="cblx">病人医保参保类型</param>
        ///// <returns></returns>
        //public override Brlx 获取病人类型(Cblx cblx)
        //{
        //    if (JiaoFei.所选缴费概要.billType.Contains("生育医保"))  //生育医保(门诊)
        //        return Brlx.生育医疗;
        //    switch (cblx)
        //    {
        //        case Cblx.不参加:
        //            return Brlx.自费;
        //        case Cblx.基本医疗保险一档:
        //        case Cblx.基本医疗保险二档:
        //        case Cblx.基本医疗保险三档:
        //            return Brlx.医疗保险;
        //        case Cblx.特殊:
        //            return Brlx.离休医疗;
        //        case Cblx.医疗保险二档少儿:
        //            return Brlx.少儿医保;
        //        case Cblx.统筹保险:
        //            return Brlx.家属统筹医疗;
        //        default:
        //            return Brlx.自费;
        //    }
        //}


        public override 门诊登记 门诊登记()
        {
            var djInfo = YB.医保扩展信息.ybinfoexp.Split('^');
            //1   门诊类别	普通
            //2   门诊类别 特病
            //3   门诊类别 特检
            //4   门诊类别 病种
            //5   门诊类别 健康体检
            //6   门诊类别 预防接种
            //7   门诊类别 家庭通道
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
                tJLB = djInfo[5],
                zD = djInfo[1],
                zDSM_str = djInfo[2],
                cFZS = "",
                ySBM = djInfo[3],
                ySXM_str = djInfo[4],
                ySDH = ""
            };
        }

        public override 医保门诊挂号登记 医保门诊挂号登记()
        {
            //获取就诊信息();
            YB.mzlsh = GetChnlSsn();
            var kSMC_str = YB?.科室名称 ?? (ChoiceModel.Business == Business.挂号 ? DepartmentModel.所选科室.deptName : RecordModel.所选记录.deptName);
            //PFXBKMZ-皮肤性病科门诊
            //科室名称太长  医保挂号会失败
            kSMC_str = "";
            kSMC_str = kSMC_str.Contains("-") ? kSMC_str.Split('-')[1] : kSMC_str;
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




        public override void 处理医保挂号信息()
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
            var yb = new 挂号扩展信息();

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
                INADMAdmDate = DateTimeCore.Now.ToString("yyyy-MM-dd"),
                INADMAdmTime = DateTimeCore.Now.ToString("HH:mm:ss"),
                INADMAdmType = "1",
                INADMDeptDesc = YB?.科室名称 ?? (ChoiceModel.Business == Business.挂号 ? DepartmentModel.所选科室.deptName : RecordModel.所选记录.deptName),
                INADMInsuType = "SZOP",
                INADMUserDr = "",
                INADMXString1 = INADMXString1_Value,
                INADMXString5 = YB.医疗证号
            };

            YB.HIS挂号所需医保信息 = JsonConvert.SerializeObject(yb);
        }


    }
}

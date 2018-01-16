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

namespace YuanTu.ShenZhen.BaoAnPeopleHospital.Services
{
    public class YBService : YuanTu.ShenZhenArea.Services.YBService
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="cblx">病人医保参保类型</param>
        /// <returns></returns>
        public override Brlx 获取病人类型(Cblx cblx)
        {
            if (JiaoFei.所选缴费概要.billType.Contains("生育医保"))  //生育医保(门诊)
                return Brlx.生育医疗;
            switch (cblx)
            {
                case Cblx.不参加:
                    return Brlx.自费;
                case Cblx.基本医疗保险一档:
                case Cblx.基本医疗保险二档:
                case Cblx.基本医疗保险三档:
                    return Brlx.医疗保险;
                case Cblx.特殊:
                    return Brlx.离休医疗;
                case Cblx.医疗保险二档少儿:
                    return Brlx.少儿医保;
                case Cblx.统筹保险:
                    return Brlx.家属统筹医疗;
                default:
                    return Brlx.自费;
            }
        }


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
            var ksmc = YB.科室名称;
            ksmc = ksmc.Contains("-") ? ksmc.Split('-')[1] : ksmc;//PFXBKMZ-皮肤性病科门诊
            //科室名称太长  医保挂号会失败
            ksmc = "";
            YB.mzlsh = GetChnlSsn();
            return new 医保门诊挂号登记
            {
                bRLX = ((int)获取病人类型(YB.参保类型)).ToString(),
                mZLSH = YB.mzlsh,
                kSBM = YB.科室编码,
                kSMC_str = ksmc,
                gHLB = "1",
                gHF = "0",
                zJZJ = "0",
                gHHJ = "0",
                sSJ = "0",
                zSJ = "0"
            };
        }
    }
}

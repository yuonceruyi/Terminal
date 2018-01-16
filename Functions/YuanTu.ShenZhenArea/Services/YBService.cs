using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using YuanTu.ShenZhenArea.Gateway;
using YuanTu.Consts.FrameworkBase;
using YuanTu.ShenZhenArea.Models;
using YuanTu.Consts.Models.BillPay;
using YuanTu.Consts;
using YuanTu.ShenZhenArea.Insurance;
using Microsoft.Practices.Unity;
using Microsoft.Practices.ServiceLocation;
using YuanTu.Consts.Services;
using System.Linq;
using YuanTu.ShenZhenArea.Enums;
using YuanTu.Consts.Gateway;
using YuanTu.Consts.Models.TakeNum;
using YuanTu.Consts.Models;
using YuanTu.Consts.Models.Register;

namespace YuanTu.ShenZhenArea.Services
{

    public interface IYBService : IService
    {
        Result 医保个人基本信息查询();

        Result 医保门诊挂号();
        Result 医保门诊登记();

        Result 医保门诊费用();
        Result 医保门诊退费();
        Result 医保门诊支付确认(bool isFeeBack);

        医保门诊挂号登记 医保门诊挂号登记();
        门诊登记 门诊登记();
        List<门诊费用> 门诊费用列表();

        /// <summary>
        /// 病人类型
        /// </summary>
        /// <param name="cblx"></param>
        /// <returns></returns>
        Brlx 获取病人类型(Cblx cblx);

        /// <summary>
        ///01	自费
        ///02	劳务工医保
        ///03	住院医保
        ///04	少儿医保
        ///05	工伤医保
        ///06	家属统筹(市)
        ///07	一级保健
        ///08	离休医保
        ///10	综合医保
        ///11	新安街道办
        ///12	优抚对象
        ///13	宝安区统筹
        ///17	综合医保(优抚对象)
        /// </summary>
        /// <param name="cblx"></param>
        /// <returns></returns>
        string 获取HIS需要的患者类型(Cblx cblx);

        void 处理医保挂号结算信息();

        void 处理医保费用();

        void 获取就诊信息();
        void 获取挂号信息();

        /// <summary>
        /// 处理医保挂号信息
        /// </summary>
        void 处理医保挂号信息();

    }

    public class YBService : ServiceBase, IYBService
    {
        public virtual Result<object> Do(string action, params object[] objects)
        {
            throw new NotImplementedException();
        }


        /// <summary>
        /// 医保Model
        /// </summary>
        [Dependency]
        public IYBModel YB { get; set; }

        /// <summary>
        /// 缴费时能用到的
        /// </summary>
        [Dependency]
        public IBillRecordModel JiaoFei { get; set; }

        /// <summary>
        /// 挂号时能用到
        /// </summary>

        [Dependency]
        public IRegisterModel RegisterModel { get; set; }


        [Dependency]
        public IScheduleModel ScheduleModel { get; set; }

        /// <summary>
        /// 取号时要用的预约记录
        /// </summary>
        [Dependency]
        public IAppoRecordModel RecordModel { get; set; }

        [Dependency]
        public IChoiceModel ChoiceModel { get; set; }


        [Dependency]
        public IDeptartmentModel DepartmentModel { get; set; }

        

        public string ServiceName
        {
            get;
        }

        public virtual Result 医保个人基本信息查询()
        {
            var 医保个人基本信息查询 = new 医保个人基本信息查询
            {
                yljgbm = YBBase.yljgbm,
                ylzh = YB?.医疗证号,
                mm = YB.医保密码,
                czybm = YBBase.czybm,
                czyxm = YBBase.czyxm
            };
            var req = new req医保个人基本信息查询
            {
                data = JsonConvert.SerializeObject(医保个人基本信息查询)
            };
            var res = SZDataHandler.医保个人基本信息查询(req, YBBase.Uri);
            YB.Res医保个人基本信息 = res;
            if (!(res?.success ?? false))
                return Result.Fail("获取医保个人基本信息失败\n" + res?.msg);
            return res.data == null ? Result.Fail("获取医保个人基本信息失败") : Result.Success();
        }





        public virtual Result 医保门诊挂号()
        {
            var 医保门诊挂号 = new 医保门诊挂号
            {
                yljgbm = YBBase.yljgbm,
                ylzh = YB.医疗证号,
                mm = YB.医保密码 ?? "",
                czybm = YBBase.czybm,
                czyxm = YBBase.czyxm,
                mzghdj = 医保门诊挂号登记()
            };
            var req = new req医保门诊挂号
            {
                data = JsonConvert.SerializeObject(医保门诊挂号)
            };
            var res = SZDataHandler.医保门诊挂号(req, YBBase.Uri);
            YB.Res医保门诊挂号 = res;
            if (res?.success ?? false)
            {
                return res.data.Count == 0 ? Result.Fail("医保门诊挂号失败") : Result.Success();
            }
            //挂号门诊流水号重复时，直接进行后面的登记
            return res?.msg != null && res.msg.Contains("(21030002)") ? Result.Success() : Result.Fail("医保门诊挂号失败\n" + res?.msg);
        }



        public virtual Result 医保门诊登记()
        {
            var 医保门诊登记 = new 医保门诊登记
            {
                yljgbm = YBBase.yljgbm,
                czybm = YBBase.czybm,
                czyxm = YBBase.czyxm,
                mzdj = 门诊登记()
            };
            var req = new req医保门诊登记
            {
                data = JsonConvert.SerializeObject(医保门诊登记)
            };
            var res = SZDataHandler.医保门诊登记(req, YBBase.Uri);
            YB.Res医保门诊登记 = res;
            if (res?.success ?? false)
            {
                return Result.Success();
            }
            return Result.Fail("医保门诊登记失败\n" + res?.msg);
        }

        public virtual Result 医保门诊费用()
        {
            YB.djh = GetChnlSsn();
            var 医保门诊费用 = new 医保门诊费用
            {
                yljgbm = YBBase.yljgbm,
                czybm = YBBase.czybm,
                czyxm = YBBase.czyxm,
                mzlsh = YB.mzlsh,
                djh = YB.djh,
                mzfydetail = 门诊费用列表()
            };
            var req = new req医保门诊费用
            {
                data = JsonConvert.SerializeObject(医保门诊费用)
            };
            var res = SZDataHandler.医保门诊费用(req, YBBase.Uri);
            YB.Res医保门诊费用 = res;
            if (!(res?.success ?? false))
                return Result.Fail("医保门诊费用预结算失败\n" + res?.msg);
            return res.data == null ? Result.Fail("医保门诊费用预结算失败") : Result.Success();
        }

        public virtual Result 医保门诊退费()
        {
            YB.djh2 = GetChnlSsn();
            var 医保门诊退费 = new 医保门诊退费
            {
                yljgbm = YBBase.yljgbm,
                czybm = YBBase.czybm,
                czyxm = YBBase.czyxm,
                mzlsh = YB.mzlsh,
                djh = YB.djh,
                djh2 = YB.djh2,
                theMZFY = YB.门诊费用列表
            };
            var req = new req医保门诊退费
            {
                data = JsonConvert.SerializeObject(医保门诊退费)
            };
            var res = SZDataHandler.医保门诊退费(req, YBBase.Uri);
            YB.Res医保门诊退费 = res;
            if (!(res?.success ?? false))
                return Result.Fail("医保门诊退费失败\n" + res?.msg);
            return res.data == null ? Result.Fail("医保门诊退费失败") : Result.Success();
        }

        public virtual Result 医保门诊支付确认(bool isFeeBack)
        {
            var 医保门诊支付确认 = new 医保门诊支付确认
            {
                yljgbm = YBBase.yljgbm,
                czybm = YBBase.czybm,
                czyxm = YBBase.czyxm,
                mzlsh = YB.mzlsh,
                djh = isFeeBack ? YB.djh2 : YB.djh,
                ssj = YB.记账金额.ToString(),//实收金额
                zsj = "0"//找回赎金
            };
            var req = new req医保门诊支付确认
            {
                data = JsonConvert.SerializeObject(医保门诊支付确认)
            };
            var res = SZDataHandler.医保门诊支付确认(req, YBBase.Uri);
            YB.Res医保门诊支付确认 = res;
            if (res?.success ?? false)
            {
                return Result.Success();
            }
            return Result.Fail("医保门诊支付确认失败\n" + res?.msg);
        }
        public virtual 医保门诊挂号登记 医保门诊挂号登记()
        {
            //获取就诊信息();
            YB.mzlsh = GetChnlSsn();
            return new 医保门诊挂号登记
            {
                bRLX = ((int)获取病人类型(YB.参保类型)).ToString(),
                mZLSH = YB.mzlsh,
                kSBM = YB.科室编码,
                kSMC_str = YB.科室名称, 
                gHLB = "1",
                gHF = "0",
                zJZJ = "0",
                gHHJ = "0",
                sSJ = "0",
                zSJ = "0"
            };
        }

        public virtual 门诊登记 门诊登记()
        {
            var djInfo = YB.医保扩展信息.ybinfoexp.Split('^');
            return new 门诊登记
            {
                mZLSH = YB.mzlsh,
                mZLB = "1",
                tBLB = "",
                tJLB = "",
                zD = djInfo[1],
                zDSM_str = djInfo[2],
                cFZS = "",
                ySBM = djInfo[3],
                ySXM_str = djInfo[4],
                ySDH = ""
            };
        }


        public virtual List<门诊费用> 门诊费用列表()
        {
            var fyInfo = YB.医保扩展信息.ybinfo;
            var MzfyList = new List<门诊费用>();
            if (fyInfo.Contains("!"))
            {
                var fyinfoArray = fyInfo.Split('!');
                foreach (var s in fyinfoArray)
                {
                    var fy = s.Split('^');
                    MzfyList.Add(new 门诊费用
                    {
                        nO = fy[0],
                        mC_str = fy[1],
                        yLJGNBBM = fy[2],
                        gG_str = fy[3],
                        dW_str = fy[4],
                        dJ = fy[5],
                        sL = fy[6],
                        hJJE = fy[7],
                        tYBM = fy[8],
                        jSXM = fy[9]
                    });
                }
            }
            else
            {
                var fy = fyInfo.Split('^');
                MzfyList.Add(new 门诊费用
                {
                    nO = fy[0],
                    mC_str = fy[1],
                    yLJGNBBM = fy[2],
                    gG_str = fy[3],
                    dW_str = fy[4],
                    dJ = fy[5],
                    sL = fy[6],
                    hJJE = fy[7],
                    tYBM = fy[8],
                    jSXM = fy[9]
                });
            }
            YB.门诊费用列表 = MzfyList;
            return MzfyList;
        }
        public virtual void 获取就诊信息()
        {
            var info = JiaoFei.所选缴费概要.extendBalanceInfo;
            YB.医保扩展信息 = JsonConvert.DeserializeObject<医保扩展信息>(info);
            YB.就诊记录ID = YB.医保扩展信息.regId;
            YB.科室编码 = YB.医保扩展信息.deptCode;
            YB.科室名称 = YB.医保扩展信息.deptName;
        }

        public virtual void 获取挂号信息()
        {
            var info = RegisterModel.Res挂号锁号?.data?.extend;
            YB.医保扩展信息 = JsonConvert.DeserializeObject<医保扩展信息>(info);
            YB.就诊记录ID = YB.医保扩展信息.regId;
            YB.科室编码 = YB.医保扩展信息.deptCode;
            YB.科室名称 = YB.医保扩展信息.deptName;
        }





        /// <summary>
        /// 
        /// </summary>
        /// <param name="cblx">病人医保参保类型</param>
        /// <returns></returns>
        public virtual Brlx 获取病人类型(Cblx cblx)
        {
            if (JiaoFei?.所选缴费概要?.billType == "生育通道")  //生育通道
                return Brlx.生育医疗;

            //if (JiaoFei.所选缴费概要.billType == "特检")  //特检
            //    return Brlx.医疗保险;
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


        /// <summary>
        /// 01  自费
        /// 02  劳务工医保
        /// 03  住院医保
        /// 04  少儿医保
        /// 05  工伤医保
        /// 06  家属统筹(市)
        /// 07  一级保健
        /// 08  离休医保
        /// 10  综合医保
        /// 11  新安街道办
        /// 12  优抚对象
        /// 13  宝安区统筹
        /// 17  综合医保(优抚对象)
        /// </summary>
        /// <param name="cblx"></param>
        /// <returns></returns>
        public virtual string 获取HIS需要的患者类型(Cblx cblx)
        {
            var CBLX = Convert.ToInt32(cblx);
            switch (cblx)
            {
                //综合医保
                case Cblx.不参加:
                    return "01";
                case Cblx.基本医疗保险一档:
                    return "10";
                case Cblx.基本医疗保险二档:
                case Cblx.基本医疗保险三档:
                    return "03";
                case Cblx.特殊:
                    return "01";
                case Cblx.医疗保险二档少儿:
                    return "04";
                case Cblx.统筹保险:
                    return "13";
                default:
                    return "01";
            }
        }


        public virtual void 处理医保挂号结算信息()
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
            if (YB.记账金额 > 0)
            {
                var admDate = JiaoFei.所选缴费概要.billDate.Substring(0, 10);
                var admTime = JiaoFei.所选缴费概要.billDate.Substring(11, 8);
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
                处理医保费用();
                yb.js = new Gateway.医保结算信息
                {
                    INPAY_bcbxf0 = YB.门诊支付结果.JE,
                    INPAY_djlsh0 = YB.mzlsh,
                    INPAY_grzfe0 = YB.门诊支付结果.XJHJ,
                    INPAY_id0000 = YB.医保个人基本信息.YLZH,
                    INPAY_jjzfe0 = YB.门诊支付结果.JZHJ,
                    INPAY_sUserDr = FrameworkConst.OperatorId,
                    INPAY_ming0 = YB.医保个人基本信息.XM,
                    INPAY_zhzfe0 = YB.账户支付额.ToString("F2"),
                    INPAY_zyksmc = YB.科室名称,
                    INPAY_zylsh0 = YB.djh,
                    INPAY_InsuPay3 = YB.自费.ToString("F2"),
                    INPAY_InsuPay4 = YB.比例自付.ToString("F2"),
                    INPAY_Zstr08 = YB.记账前.ToString("F2"),
                    INPAY_Zstr09 = YB.记账后.ToString("F2"),
                    theMZJS = string.Join("", YB?.Res医保门诊费用?.data.theMZJS.Select(d => "|" + d.JSXM + ":" + d.JE).ToList()),
                    theMZZF = string.Join("", YB?.Res医保门诊费用?.data.theMZZF.Select(d => "|" + d.ZFXM + ":" + d.JE).ToList()),
                };
            }
            else
            {
                yb.gh = null;
                yb.js = null;
            }
            yb.regId = YB.就诊记录ID;
            YB.HIS结算所需医保信息 = JsonConvert.SerializeObject(yb);
        }

        public virtual void 处理医保费用()
        {
            YB.账户支付额 = 0;
            YB.自费 = 0;
            YB.比例自付 = 0;
            YB.记账前 = 0;
            YB.记账后 = 0;

            foreach (var mzjsDetail in YB.门诊结算结果)
            {
                if (mzjsDetail.JSXM == "0201")
                    YB.账户支付额 += Convert.ToDouble(mzjsDetail.JE);
                if ((mzjsDetail.JSXM == "0101") || (mzjsDetail.JSXM == "0111") || (mzjsDetail.JSXM == "0104"))
                    YB.自费 += Convert.ToDouble(mzjsDetail.JE);
                if ((mzjsDetail.JSXM == "0102") || (mzjsDetail.JSXM == "0108") || (mzjsDetail.JSXM == "0109") || (mzjsDetail.JSXM == "0110") || (mzjsDetail.JSXM == "0103") || (mzjsDetail.JSXM == "0112"))
                    YB.比例自付 += Convert.ToDouble(mzjsDetail.JE);
                if (mzjsDetail.JSXM == "0303")
                    YB.记账前 += Convert.ToDouble(mzjsDetail.JE);
                if (mzjsDetail.JSXM == "0306")
                    YB.记账后 += Convert.ToDouble(mzjsDetail.JE);
            }
            foreach (var mzzfDetail in YB.门诊支付)
            {
                if (mzzfDetail.ZFXM == "0201")
                    YB.账户支付额 += Convert.ToDouble(mzzfDetail.JE);
                if ((mzzfDetail.ZFXM == "0101") || (mzzfDetail.ZFXM == "0111") || (mzzfDetail.ZFXM == "0104"))
                    YB.自费 += Convert.ToDouble(mzzfDetail.JE);
                if ((mzzfDetail.ZFXM == "0102") || (mzzfDetail.ZFXM == "0108") || (mzzfDetail.ZFXM == "0109") || (mzzfDetail.ZFXM == "0110") || (mzzfDetail.ZFXM == "0103") || (mzzfDetail.ZFXM == "0112"))
                    YB.比例自付 += Convert.ToDouble(mzzfDetail.JE);
                if (mzzfDetail.ZFXM == "0303")
                    YB.记账前 += Convert.ToDouble(mzzfDetail.JE);
                if (mzzfDetail.ZFXM == "0306")
                    YB.记账后 += Convert.ToDouble(mzzfDetail.JE);
            }
        }
        
        public virtual void 处理医保挂号信息()
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
                INADMDeptDesc = YB.科室名称,
                INADMInsuType = "SZOP",
                INADMUserDr = "",
                INADMXString1 = INADMXString1_Value,
                INADMXString5 = YB.医疗证号
            };

            YB.HIS挂号所需医保信息 = JsonConvert.SerializeObject(yb);
        }

    }
}
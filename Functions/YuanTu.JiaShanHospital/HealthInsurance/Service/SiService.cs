using Microsoft.Practices.Unity;
using System;
using System.Text;
using System.Threading;
using YuanTu.Consts;
using YuanTu.Consts.FrameworkBase;
using YuanTu.Core.Log;
using YuanTu.JiaShanHospital.HealthInsurance.Model;
using YuanTu.JiaShanHospital.HealthInsurance.Request;
using YuanTu.JiaShanHospital.HealthInsurance.Response;
using System.Collections.Generic;
using System.Diagnostics;
using YuanTu.Core.Extension;

namespace YuanTu.JiaShanHospital.HealthInsurance.Service
{
    public class SiService :  ISiService
    {
        public string ServiceName => "医保接口服务";
        private const string Prefix = "医保系统";
        private long _count;
        public long Count => Interlocked.Increment(ref _count);

        [Dependency]
        public ISiModel SiModel { get; set; }

        public Result Initialize()
        {
            if(Environment.CurrentDirectory!=FrameworkConst.RootDirectory)
                Environment.CurrentDirectory = FrameworkConst.RootDirectory;

            if (SiConfig.InitializeSuccess && SiConfig.InitializeDate == DateTimeCore.Now.Date)
                return Result.Success();
            var sb = new StringBuilder(4096);
            Logger.Net.Debug($"[{Prefix}] [{Count}] f_UserBargaingInit，开始初始化医保接口");
            var watch = Stopwatch.StartNew();
            SiModel.Ret = UnSafeMethods.f_UserBargaingInit("$$$$", sb, SiConfig.HospitalCode);
            watch.Stop();
            var time = watch.ElapsedMilliseconds;
            SiModel.RetMessage = sb.ToString();
            if (SiModel.Ret >= 0)
                SiConfig.InitializeDate = DateTimeCore.Now.Date;
            SiConfig.InitializeSuccess = SiModel.Ret >= 0;
            Logger.Net.Debug($"[{Prefix}] [{Count}] [耗时：{time}] f_UserBargaingInit，初始化医保接口结果{SiConfig.InitializeSuccess}");
            return SiConfig.InitializeSuccess ? Result.Success() : Result.Fail($"医保初始化失败:{sb}");
        }

        public Result OperatorSignIn(int transNo, string req)
        {
            var sb = new StringBuilder(4096);
            SiModel.Ret = UnSafeMethods.f_UserBargaingApply(transNo, SiConfig.TransSeq, req, sb,SiConfig.HospitalCode);
            SiModel.RetMessage = sb.ToString();
            return SiModel.Ret >= 0 ? Result.Success() : Result.Fail("医保操作员签到失败");
        }

        public Result Close()
        {
            try
            {
                if (!SiConfig.InitializeSuccess)
                    return Result.Success();
                if (Environment.CurrentDirectory != FrameworkConst.RootDirectory)
                    Environment.CurrentDirectory = FrameworkConst.RootDirectory;
                var sb = new StringBuilder(4096);
                Logger.Net.Debug($"[{Prefix}] [{Count}] f_UserBargaingClose，开始关闭医保接口");
                SiModel.Ret = UnSafeMethods.f_UserBargaingClose("$$$$", sb, SiConfig.HospitalCode);
                SiModel.RetMessage = sb.ToString();
                if (SiModel.Ret >= 0)
                    SiConfig.InitializeSuccess = false;
                Logger.Net.Debug($"[{Prefix}] [{Count}] f_UserBargaingClose，关闭医保接口结果{!SiConfig.InitializeSuccess}");
                return SiModel.Ret >= 0 ? Result.Success() : Result.Fail( $"医保关闭失败:{sb}");
            }
            catch (Exception ex)
            {
                Logger.Net.Error($"[{Prefix}] [{Count}] 医保关闭失败 {ex.Message} {ex.StackTrace}");
                return  Result.Fail( "医保关闭失败");
            }
        }

        public Result OpRegClinicPreSettle(string req)
        {
           return SiApply(91, req);
        }

        public Result OpRegClinicSettle(string req)
        {
           return SiApply(92, req);
        }

        public Result OpPayClinicPreSettle(string req)
        {
            
            return SiApply(91, req);
        }

        public Result OpPayClinicSettle(string req)
        {
            return SiApply(92, req);
        }

        public Result OpRegClinicSettleRefund(string req)
        {
            return SiApply(93, req);
        }

        public Result OpPayClinicSettleRefund( string req)
        {
           
            return SiApply(93, req);
        }

        public Result GetSiPatientInfo( Req获取参保人信息 reqSiPatientInfo)
        {
            try
            {
                var req = $"$${reqSiPatientInfo.MakeInput("~")}$$";
              
                var result= SiApply(22, req);
                if (!result.IsSuccess)
                    return result;
                #region 解析信息

                var msgArray = SiModel.RetMessage.Trim().Trim('$').Split('~');
                SiModel.获取参保人信息结果 = new Res获取参保人信息结果();
                SiModel.获取参保人信息结果 = SiModel.获取参保人信息结果.Decode(msgArray);

                var patientInfo = SiModel.获取参保人信息结果.个人基本信息串.Split(new[] { "%%" }, StringSplitOptions.None);
                SiModel.医保个人基本信息 = new 个人基本信息();
                SiModel.医保个人基本信息 = SiModel.医保个人基本信息.Decode(patientInfo);

                Logger.Net.Debug($"[{Prefix}] [{Count}] [医保个人信息]\n{SiModel.医保个人基本信息}");

                #endregion 解析信息

                return Result.Success();
            }
            catch (Exception ex)
            {
                Logger.Net.Error($"{ex.Message} {ex.StackTrace}");
                return Result.Fail( $"医保获取参保人信息失败:{ex.Message}");
            }
        }

        public Result OpRegPreSettle(string req)
        {
            return SiApply(29, req);
        }

        public Result OpRegSettle(string req)
        {
            return SiApply(30, req);
        }

        public Result OpPreSettle(string req)
        {
          
            return SiApply(29, req);
        }

        public Result OpSettle(string req)
        {
            return SiApply(30, req);
        }

        public Result OpRegRefund(string req)
        {
            return SiApply(31,req);
        }

        public Result OpRefund(string req)
        {
            return SiApply(31,req);
        }

        public Result QueryTradeResult(Req交易结果查询 reqQueryTradeResult)
        {
            var req = $"$${reqQueryTradeResult.MakeInput("~")}$$";
            return SiApply(43,req);
        }

        public Result TradeConfirm(Req交易确认 reqTradeConfirm)
        {
            var req = $"$${reqTradeConfirm.MakeInput("~")}$$";
            var result = SiApply(49,req);
            if (!result.IsSuccess)
                return result;
            //解析
            var msgArray = SiModel.RetMessage.Trim().Trim('$').Split('~');
            SiModel.交易确认结果 = new Res交易确认结果();
            SiModel.交易确认结果 = SiModel.交易确认结果.Decode(msgArray);
            return Result.Success();
        }

        private Result SiApply(int transNo,string req)
        {
            if (Environment.CurrentDirectory != FrameworkConst.RootDirectory)
                Environment.CurrentDirectory = FrameworkConst.RootDirectory;
            var index = Count;
            var sb = new StringBuilder(4096);
            Logger.Net.Debug($"[{Prefix}] [{index}] [req{SiTransName[transNo]}]\n{req}");
            var watch = Stopwatch.StartNew();
            SiModel.Ret = UnSafeMethods.f_UserBargaingApply(transNo, SiConfig.TransSeq, req, sb, SiConfig.HospitalCode);
            watch.Stop();
            var time = watch.ElapsedMilliseconds;
            Logger.Net.Debug($"[{Prefix}] [{index}] [res{SiTransName[transNo]}] [耗时：{time}]\n{sb}");
            SiModel.RetMessage = sb.ToString();
            var errorMsg = string.Empty;
            if (SiModel.Ret < 0)
            {
               errorMsg= GetErrorMsg(SiModel.RetMessage); 
            }
            return SiModel.Ret >= 0 ? Result.Success() : Result.Fail(errorMsg);
        }

        private string GetErrorMsg(string orgMsg)
        {
            try
            {
                //取错误信息
                var msgArray = orgMsg.Trim().Trim('$').Split('~');
                return msgArray[1].Contains("%%") ? msgArray[1].Split(new[] { "%%" }, StringSplitOptions.None)[0] : msgArray[1];
            }
            catch
            {
                return null;
            }
        }
        public static Dictionary<int, string> SiTransName = new Dictionary<int, string>
        {
            {22,"获取参保人信息"},
            {29,"门诊/挂号预结算"},
            {30,"门诊/挂号结算"},
            {91,"诊间预结算"},
            {92,"诊间结算"},
            {49,"交易确认"},
            {43,"交易结果查询"},
            {31,"挂号退号/门诊退费" },
            {93,"诊间结算退费"},
        };
        public string FakerSiServer(int transNo)
        {
            switch (transNo)
            {
                case 22:
                    return
                        "$$0~~~~~330483D156000005000C4B7BF1AC651C%%0026800%%姜磊芬%%2%%01%%19730302%%330205197303024829%%自收自支事业单位%%桐乡市第一人民医院%%330483%%桐乡市%%11%%0%%0%%0%%1%%%%910.23%%2006.67%%0%%1119.81%%0%%0%%0%%0%%0%%0%%0%%0%%%%20~000000011000000~~~$$";

                case 29:
                    return
                        "$$0~~~~~~0~20161116 143801~10%%0%%0%%10%%0%%0%%0%%0%%0%%10%%0%%2%%8%%0%%0%%0%%0%%0%%0%%0%%0%%0%%0%%0%%0%%0%%908.23%%2006.67~~~92956834$$";

                case 30:
                    return null;

                default:
                    return null;
            }
        }

       
    }
}
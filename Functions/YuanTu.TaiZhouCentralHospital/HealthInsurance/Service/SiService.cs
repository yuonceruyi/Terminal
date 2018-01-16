using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using Microsoft.Practices.Unity;
using YuanTu.Consts;
using YuanTu.Consts.FrameworkBase;
using YuanTu.Core.Log;
using YuanTu.TaiZhouCentralHospital.HealthInsurance.Model;
using YuanTu.TaiZhouCentralHospital.HealthInsurance.Request;
using YuanTu.TaiZhouCentralHospital.HealthInsurance.Response;

namespace YuanTu.TaiZhouCentralHospital.HealthInsurance.Service
{
    public class SiService : ISiService
    {
        private const string Prefix = "医保系统";

        public static Dictionary<int, string> SiTransName = new Dictionary<int, string>
        {
            {22, "获取参保人信息"},
            {29, "门诊/挂号预结算"},
            {30, "门诊/挂号结算"},
            {49, "交易确认"},
            {43, "交易结果查询"},
            {31, "挂号退号/门诊退费"}
        };

        private long _count;
        public long Count => Interlocked.Increment(ref _count);

        [Dependency]
        public ISiModel SiModel { get; set; }

        public string ServiceName => "医保接口服务";

        public Result Initialize()
        {
           
            if (SiConfig.InitializeSuccess && SiConfig.InitializeDate == DateTimeCore.Now.Date)
                return Result.Success();
            var sb = new StringBuilder(4096);
            Logger.Net.Info($"[{Prefix}] [{Count}] f_UserBargaingInit，开始初始化医保接口");
            var watch = Stopwatch.StartNew();
            SiModel.Ret = UnSafeMethods.f_UserBargaingInit("$$$$", sb, SiConfig.HospitalCode);
            watch.Stop();
            var time = watch.ElapsedMilliseconds;
            SiModel.RetMessage = sb.ToString();
            if (SiModel.Ret >= 0)
                SiConfig.InitializeDate = DateTimeCore.Now.Date;
            SiConfig.InitializeSuccess = SiModel.Ret >= 0;
            Logger.Net.Info(
                $"[{Prefix}] [{Count}] [耗时：{time}] f_UserBargaingInit，初始化医保接口结果{SiConfig.InitializeSuccess}");
            return SiConfig.InitializeSuccess ? Result.Success() : Result.Fail($"医保初始化失败:{sb}");
        }

        public Result OperatorSignIn(int transNo, string req)
        {
            var sb = new StringBuilder(4096);
            SiModel.Ret = UnSafeMethods.f_UserBargaingApply(transNo, SiConfig.TransSeq, req, sb, SiConfig.HospitalCode);
            SiModel.RetMessage = sb.ToString();
            return SiModel.Ret >= 0 ? Result.Success() : Result.Fail("医保操作员签到失败");
        }

        public Result Close()
        {
            try
            {
                if (!SiConfig.InitializeSuccess)
                    return Result.Success();
                var sb = new StringBuilder(4096);
                Logger.Net.Info($"[{Prefix}] [{Count}] f_UserBargaingClose，开始关闭医保接口");
                SiModel.Ret = UnSafeMethods.f_UserBargaingClose("$$$$", sb, SiConfig.HospitalCode);
                SiModel.RetMessage = sb.ToString();
                if (SiModel.Ret >= 0)
                    SiConfig.InitializeSuccess = false;
                Logger.Net.Info($"[{Prefix}] [{Count}] f_UserBargaingClose，关闭医保接口结果{!SiConfig.InitializeSuccess}");
                return SiModel.Ret >= 0 ? Result.Success() : Result.Fail($"医保关闭失败:{sb}");
            }
            catch (Exception ex)
            {
                Logger.Net.Error($"[{Prefix}] [{Count}] 医保关闭失败 {ex.Message} {ex.StackTrace}");
                return Result.Fail("医保关闭失败");
            }
        }

        public Result GetSiPatientInfo(Req获取参保人信息 reqSiPatientInfo)
        {
            try
            {
                var req = $"$${reqSiPatientInfo.MakeInput("~")}$$";

                var result = SiApply(22, req);
                if (!result.IsSuccess)
                    return result;

                #region 解析信息

                var msgArray = SiModel.RetMessage.Trim().Trim('$').Split('~');
                SiModel.获取参保人信息结果 = new Res获取参保人信息结果();
                SiModel.获取参保人信息结果 = SiModel.获取参保人信息结果.Decode(msgArray);

                var patientInfo = SiModel.获取参保人信息结果.个人基本信息.Split(new[] {"%%"}, StringSplitOptions.None);
                SiModel.医保个人基本信息 = new 个人基本信息();
                SiModel.医保个人基本信息 = SiModel.医保个人基本信息.Decode(patientInfo);

                Logger.Net.Info($"[{Prefix}] [{Count}] [医保个人信息]\n{SiModel.医保个人基本信息}");

                #endregion 解析信息

                return Result.Success();
            }
            catch (Exception ex)
            {
                Logger.Net.Error($"{ex.Message} {ex.StackTrace}");
                return Result.Fail($"医保获取参保人信息失败:{ex.Message}");
            }
        }

        public Result OpPreSettle(string req)
        {
            try
            {
                var result = SiApply(29, req);
                if (!result.IsSuccess)
                    return result;

                #region 解析信息

                SiModel.医保预结算结果字符串 = SiModel.RetMessage;
                var msgArray = SiModel.RetMessage.Trim().Trim('$').Split('~');
                var 医保预结算返回 = new Res医保预结算结果();
                医保预结算返回 = 医保预结算返回.Decode(msgArray);

                var 计算结果信息 = 医保预结算返回.计算结果信息.Split(new[] {"%%"}, StringSplitOptions.None);
                SiModel.医保预结算结果 = new 计算结果信息();
                SiModel.医保预结算结果 = SiModel.医保预结算结果.Decode(计算结果信息);

                #endregion 解析信息

                return Result.Success();
            }
            catch (Exception ex)
            {
                Logger.Net.Error($"{ex.Message} {ex.StackTrace}");
                return Result.Fail($"医保预结算失败:{ex.Message}");
            }
        }

        public Result OpSettle(string req)
        {
            try
            {
                var result = SiApply(30, req);
                if (!result.IsSuccess)
                    return result;

                #region 解析信息
                SiModel.医保结算结果字符串 = SiModel.RetMessage;
                var msgArray = SiModel.RetMessage.Trim().Trim('$').Split('~');
                var 医保结算返回 = new Res医保结算结果();
                医保结算返回 = 医保结算返回.Decode(msgArray);

                SiModel.医保结算流水号 = 医保结算返回.结算流水号;

                #endregion 解析信息

                return Result.Success();
            }
            catch (Exception ex)
            {
                Logger.Net.Error($"{ex.Message} {ex.StackTrace}");
                return Result.Fail($"医保结算失败:{ex.Message}");
            }
        }

        public Result OpRefund(Req医保退费 req)
        {
            try
            {
                var reqString = $"$${req.MakeInput("~")}$$";
                return SiApply(31, reqString);
            }
            catch (Exception ex)
            {
                Logger.Net.Error($"{ex.Message} {ex.StackTrace}");
                return Result.Fail($"医保退费失败:{ex.Message}");
            }
        }

        public Result QueryTradeResult(Req交易结果查询 reqQueryTradeResult)
        {
            var req = $"$${reqQueryTradeResult.MakeInput("~")}$$";
            return SiApply(43, req);
        }

        public Result TradeConfirm(Req交易确认 reqTradeConfirm)
        {
            try
            {
                var req = $"$${reqTradeConfirm.MakeInput("~")}$$";
                return SiApply(49, req);
            }
            catch (Exception ex)
            {
                Logger.Net.Error($"{ex.Message} {ex.StackTrace}");
                return Result.Fail($"交易确认失败:{ex.Message}");
            }
        }

        private Result SiApply(int transNo, string req)

        {
            if (FrameworkConst.FakeServer)
            {
                SiModel.RetMessage = FakerSiServer(transNo);
                if (SiModel.Ret < 0 || SiModel.RetMessage.Trim('$').Split('~')[0] != "0")
                    return Result.Fail(GetErrorMsg(SiModel.RetMessage));
                return Result.Success();
            }
                
            //TODO 交易初始化
            var result = Initialize();
            if (!result.IsSuccess)
                return result;

            var index = Count;
            var sb = new StringBuilder(4096);
            Logger.Net.Info($"[{Prefix}] [{index}] [req{SiTransName[transNo]}]\n{req}");
            var watch = Stopwatch.StartNew();
            SiModel.Ret = UnSafeMethods.f_UserBargaingApply(transNo, SiConfig.TransSeq, req, sb, SiConfig.HospitalCode);
            watch.Stop();
            var time = watch.ElapsedMilliseconds;
            Logger.Net.Info($"[{Prefix}] [{index}] [res{SiTransName[transNo]}] [耗时：{time}] [返回值：{SiModel.Ret}]\n{sb}");
            SiModel.RetMessage = sb.ToString();

            if (SiModel.Ret < 0||sb.ToString().Trim('$').Split('~')[0]!="0")
                return Result.Fail(GetErrorMsg(SiModel.RetMessage));

            return Result.Success();
        }

        private string GetErrorMsg(string orgMsg)
        {
            try
            {
                //取错误信息
                var msgArray = orgMsg.Trim().Trim('$').Split('~');
                return msgArray[1].Contains("%%")
                    ? msgArray[1].Split(new[] {"%%"}, StringSplitOptions.None)[0]
                    : msgArray[1];
            }
            catch
            {
                return null;
            }
        }
        public string FakerSiServer(int transNo)
        {
            switch (transNo)
            {
                case 22:
                    return
                        "$$-400~找不到该卡号(打开端口失败,请检查配置文件是否正确设置!)的有关信息%%SIM_TRANSPACK.F_OrafGetPsseno%%ORA-01403: no data found~~~$$";

                case 29:
                    return
                        "$$0~~~~~~0~20161116 143801~10%%0%%0%%10%%0%%0%%0%%0%%0%%10%%0%%2%%8%%0%%0%%0%%0%%0%%0%%0%%0%%0%%0%%0%%0%%0%%908.23%%2006.67~~~92956834$$";

                case 30:
                    return "$$0~~~~~~0~20161116 143801~10%%0%%0%%10%%0%%0%%0%%0%%0%%10%%0%%2%%8%%0%%0%%0%%0%%0%%0%%0%%0%%0%%0%%0%%0%%0%%908.23%%2006.67~~~92956834$$";

                default:
                    return null;
            }
        }
    }
}
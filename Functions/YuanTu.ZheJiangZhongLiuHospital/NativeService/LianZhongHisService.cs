using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using YuanTu.Consts.FrameworkBase;
using YuanTu.Core.Log;

namespace YuanTu.ZheJiangZhongLiuHospital.NativeService
{
    /// <summary>
    /// 联众HIS的本地化调用接口
    /// </summary>
    public static class LianZhongHisService
    {
        public static string HisExePath = "";

        [DllImport("MediInfo.RunExe.dll")]
        private static extern int RunExe(string sExePath, string sExeIn, [MarshalAs(UnmanagedType.VBByRefStr)] ref string sExeOut, int iTimeOut);

        /// 执行挂号结算
        /// 00、成功
        /// 01、取消结算
        /// -1、失败
        /// CZ|4.00  差额四元
        public static Result<string> ExcuteHospitalRegister(string cardNo)
        {
            var res = InternalRunExeInvoke(cardNo);
            if (!res.IsSuccess)
            {
                return Result<string>.Fail(res.Message);
            }
            if (res.Value.Contains("00"))
            {
                return Result<string>.Success("挂号成功");
            }
            if (res.Value.Contains("01"))
            {
                return Result<string>.Fail($"异常,取消结算");
            }
            if(res.Value.Contains("CZ"))
            {
                var count = res.Value.Split('|')[1];
                return Result<string>.Fail($"余额不足,相差{count}元");
            }
            return Result<string>.Fail($"HIS异常返回:{res.Value}");
        }
        
        /// 执行缴费结算
        public static Result<string> ExcuteHospitalCheckout(string cardNo)
        {
            var res = InternalRunExeInvoke(cardNo);
            if (!res.IsSuccess)
            {
                return Result<string>.Fail(res.Message);
            }
            if (res.Value.Contains("00"))
            {
                return Result<string>.Success(res.Value);
            }
            if (res.Value.Contains("-1"))
            {
                return Result<string>.Fail($"{res.Value.Split('|')[1]}");
            }
            if (res.Value.Contains("CZ"))
            {
                var count = res.Value.Split('|')[1];
                return Result<string>.Fail($"余额不足,相差{count}元");
            }
            return Result<string>.Fail($"HIS异常返回:{res.Value}");
        }
        
        /// 执行取号结算
        public static Result<string> ExcuteHospitalGetTicketCheckout(string cardNo)
        {
            var res = InternalRunExeInvoke(cardNo);
            if (!res.IsSuccess)
            {
                return Result<string>.Fail(res.Message);
            }
            if (res.Value.Contains("00"))
            {
                return Result<string>.Success("取号成功");
            }
            if (res.Value.Contains("-1"))
            {
                return Result<string>.Fail($"{res.Value.Split('|')[1]}");
            }
            if (res.Value.Contains("CZ"))
            {
                var count = res.Value.Split('|')[1];
                return Result<string>.Fail($"余额不足,相差{count}元");
            }
            return Result<string>.Fail($"HIS异常返回:{res.Value}");
        }
        
        private static readonly object LockObj = new object();
        private static Result<string> InternalRunExeInvoke(string input)
        {

            lock (LockObj)
            {
                try
                {
                    string[] outPut = { "".PadRight(1024) };
                    var watch = Stopwatch.StartNew();
                    Logger.Net.Info($"input:{input}");
                    var res = Task.Factory.StartNew(() =>
                    {
                        try
                        {
                            RunExe(HisExePath, input, ref outPut[0], 0);
                        }
                        catch (Exception e)
                        {
                            Logger.Net.Info($"异常{e}");
                        }
                    }).Wait(1000 * 60);//60秒
                    watch.Stop();
                    outPut[0] = outPut[0].Trim().Replace("\u0000", "").Replace("\\u0000", "").Trim();
                    Logger.Net.Info($"HisExePath:{HisExePath},recv:{outPut[0]}");
                    if (res && !string.IsNullOrWhiteSpace(outPut[0]))
                    {
                        return Result<string>.Success(outPut[0]);
                    }
                    Logger.Net.Error($"调用本地HIS服务异常，{res}，{outPut[0]}");
                    return Result<string>.Fail("调用本地HIS服务异常，请稍后再试");
                }
                catch (Exception ex)
                {
                    Logger.Net.Error($"调用本地HIS服务异常 {ex.Message} {ex.StackTrace}");
                    return Result<string>.Fail($"调用本地HIS服务异常 {ex.Message}");
                }

            }

        }
    }
}

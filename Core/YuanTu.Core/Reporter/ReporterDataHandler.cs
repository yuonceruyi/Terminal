using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using YuanTu.Consts;
using YuanTu.Consts.Gateway;
using YuanTu.Consts.Gateway.Base;
using YuanTu.Core.Extension;
using YuanTu.Core.Log;

namespace YuanTu.Core.Reporter
{
    public partial class ReporterDataHandler
    {
        private const string Prefix = "监控平台";
        private static long _count;

        public static TimeSpan TimeOut { get; set; } = new TimeSpan(0, 3, 0);

        public static string FakeDirectory { get; set; } = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "FakeServer");

        public static bool Disabled { get; set; }

        public static res系统签到 系统签到(req系统签到 req)
        {
            return Query<res系统签到, req系统签到>(req);
        }

        public static res信息上报 信息上报(req信息上报 req)
        {
            return Query<res信息上报, req信息上报>(req);
        }
        public static res拍照录像上传 拍照录像上传(req拍照录像上传 req)
        {
            return Query<res拍照录像上传, req拍照录像上传>(req);
        }

        public static res清钱箱上报 清钱箱上报(req清钱箱上报 req)
        {
            return Query<res清钱箱上报, req清钱箱上报>(req);
        }

        public static TRes Query<TRes, TReq>(TReq req)
            where TRes : GatewayResponse, new()
            where TReq : GatewayRequest
        {
            if (Disabled)
                return new TRes() { success = true };
            var i = Interlocked.Increment(ref _count);
            var content = new Wrapper { Query = req.GetParams() }.Content();
            var name = req.serviceName;
            Logger.Maintenance.Info($"[{Prefix}] [{name}] [{i}] 发送内容:{content.ToJsonString()}");
            if (FrameworkConst.FakeServer)
            {
                string res;
                var file = Path.Combine(FakeDirectory, FrameworkConst.HospitalId, name + ".json");
                if (!File.Exists(file))
                {
                    file = Path.Combine(FakeDirectory, name + ".json");
                    if (!File.Exists(file))
                        return new TRes
                        {
                            success = false,
                            msg = file + " 未找到"
                        };
                }
                using (var sr = new StreamReader(file))
                    res = sr.ReadToEnd();

                Logger.Maintenance.Info($"[{Prefix}] [{name}] [{i}] 耗时:{0}毫秒 接收内容:{res}");
                return res.ToJsonObject<TRes>();
            }
            using (var client = new HttpClient(new HttpClientHandler { UseProxy = false }))
            {
                client.Timeout = TimeOut;
                try
                {
                    var watch = Stopwatch.StartNew();
                    var formContent = new FormUrlEncodedContent(content);
                    // Log?.Invoke($"[{i}] [{name}] SendOrigin: {formContent.ReadAsStringAsync().Result}");
                    var task = client.PostAsync(FrameworkConst.MonitorUrl, formContent);
                    var response = task.Result;
                    var text = response.Content.ReadAsStringAsync().Result;
                    watch.Stop();
                    var time = watch.ElapsedMilliseconds;
                    Logger.Maintenance.Info($"[{Prefix}] [{name}] [{i}] 耗时:{time}毫秒 接收内容:{text}");
                    if (response.StatusCode != HttpStatusCode.OK)
                        return new TRes
                        {
                            success = false,
                            msg = $"{(int)response.StatusCode}:{response.StatusCode}"
                        };
                    return text.ToJsonObject<TRes>();
                }
                catch (AggregateException ex)
                {
                    var mainException = string.Empty;
                    Logger.Maintenance.Error($"[{Prefix}] [{name}] [{i}] 异常内容:{PrintAggregateException(ex, out mainException)}");
                    return new TRes
                    {
                        success = false,
                        code = long.MinValue,
                        msg = mainException
                    };
                }
                catch (Exception ex)
                {
                    Logger.Maintenance.Error($"[{Prefix}] [{name}] [{i}] 异常内容:{ex.Message}\r\n{ex.StackTrace}");
                    return new TRes
                    {
                        success = false,
                        code = long.MinValue,
                        msg = ex.Message
                    };
                }
            }
        }

        public static string PrintAggregateException(AggregateException ex, out string mainException)
        {
            mainException = string.Empty;
            var sb = new StringBuilder();
            foreach (var inner in ex.InnerExceptions)
            {
                mainException = inner.Message;
                sb.AppendLine($"{inner.Message}\n{inner.StackTrace}");
                var n = 1;
                var pointer = inner.InnerException;
                while (pointer != null)
                {
                    mainException = pointer.Message;
                    sb.AppendLine($"{"".PadLeft(n, '\t')}{pointer.Message}\n{"".PadLeft(n, '\t')}{pointer.StackTrace}");
                    pointer = pointer.InnerException;
                    n++;
                }
            }
            return sb.ToString();
        }
    }
}
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Web;
using YuanTu.Consts;
using YuanTu.Consts.Gateway.Base;
using YuanTu.Core.Extension;
using YuanTu.Core.Log;
using YuanTu.Core.Reporter;

namespace YuanTu.YiWuArea.Gateway
{
    public partial class DataHandler : IDataHandler
    {
        private const string Prefix = "义乌客制化前置网关";
        private long _count;

        public static long[] UnKnowErrorCode = { -2, -4, -100 };
        // public TimeSpan DefaultTimeOut { get; set; } = new TimeSpan(0, 3, 0);

        public string FakeDirectory { get; set; } = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "FakeServer");


        public TRes Query<TRes, TReq>(TReq req)
            where TRes : GatewayResponse, new()
            where TReq : GatewayRequest
        {
            var i = Interlocked.Increment(ref _count);
            var content = new Wrapper { Query = req.GetParams() }.Content();
            var name = req.serviceName;
            var val = content.ToJsonString();
            Logger.Net.Info($"[{Prefix}] [{name}] [{i}] 发送内容:{val}");
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

                Logger.Net.Info($"[{Prefix}] [{name}] [{i}] 耗时:{0}毫秒 接收内容:{res}");

                Thread.Sleep(500);
                return res.ToJsonObject<TRes>();
            }
            using (var client = new HttpClient())
            {
                client.Timeout = req.Timeout;
                try
                {
                    var watch = Stopwatch.StartNew();
                    HttpContent formContent = null;
                    if (val.Length < 1024*10)
                    {
                        formContent = new FormUrlEncodedContent(content);

                    }
                    else
                    {
                        var kvcontent = string.Join("&",content.Select(p => $"{p.Key}={HttpUtility.UrlEncode(p.Value)}"));
                        formContent = new StringContent(kvcontent, Encoding.UTF8, "application/x-www-form-urlencoded");
                    }
                    var task = client.PostAsync(FrameworkConst.GatewayUrl, formContent);
                    var response = task.Result;
                    var text = response.Content.ReadAsStringAsync().Result;
                    watch.Stop();
                    var time = watch.ElapsedMilliseconds;
                    Logger.Net.Info($"[{Prefix}] [{name}] [{i}] 耗时:{time}毫秒 接收内容:{text}");
                    if (response.StatusCode != HttpStatusCode.OK)
                    {
                        return new TRes
                        {
                            code = -100,
                            success = false,
                            msg = $"{(int)response.StatusCode}:{response.StatusCode}"
                        };
                    }
                    if (text.IsNullOrWhiteSpace())
                    {
                        ReportService.网关返回异常($"接口[{name}]网关返回null", null);
                        return new TRes
                        {
                            code = -100,
                            success = false,
                            msg = $"接口[{name}]网关返回内容异常！"
                        };
                    }
                    var res = text.ToJsonObject<TRes>();
                    if ((!res.success) && string.IsNullOrWhiteSpace(res.msg))
                    {
                        ReportService.网关返回异常($"接口[{name}]网关返回false，但msg中没有错误内容", null);
                    }
                    return res;
                }
                catch (AggregateException ex)
                {
                    string mainException;
                    Logger.Net.Error($"[{Prefix}] [{name}] [{i}] 异常内容:{PrintAggregateException(ex, out mainException)}");
                    ReportService.网关返回异常($"接口[{name}]网关异常，异常内容:{PrintAggregateException(ex, out mainException)}", null);
                    return new TRes
                    {
                        success = false,
                        code = -100,
                        msg = mainException
                    };
                }
                catch (Exception ex)
                {
                    Logger.Net.Error($"[{Prefix}] [{name}] [{i}] 异常内容:{ex.Message}\r\n{ex.StackTrace}");
                    ReportService.网关返回异常($"接口[{name}]网关异常，异常内容:{ex.Message}\r\n{ex.StackTrace}", null);
                    return new TRes
                    {
                        success = false,
                        code = -100,
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
using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using YuanTu.Consts;
using YuanTu.Core.Extension;
using YuanTu.Core.Log;
using YuanTu.Default.House.HealthManager.Base;

namespace YuanTu.Default.House.HealthManager
{
    public class DataHandler
    {
        private const string Prefix = "健康服务";
        private static readonly string _houseUrl = FrameworkConst.UserCenterUrl;
        private static long _count;

        public static long[] UnKnowErrorCode = { -2, -4, -100 };
        public static TimeSpan TimeOut { get; set; } = new TimeSpan(0, 3, 0);

        public static string FakeDirectory { get; set; } = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "FakeServer");

        public static TRes Query<TRes, TReq>(TReq req)
            where TRes : ResponseBase, new()
            where TReq : RequestBase
        {
            var i = Interlocked.Increment(ref _count);
            var content = new Wrapper { Query = req.GetParams() }.Content();
            var name = req.serviceName;
            var url = $"{_houseUrl}{req.service}";
            Logger.Net.Info($"[{Prefix}] [{name}] [{i}] 发送内容:{content.ToJsonString()}");
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
                return res.ToJsonObject<TRes>();
            }
            using (var client = new HttpClient())
            {
                client.Timeout = TimeOut;
                try
                {
                    var watch = Stopwatch.StartNew();
                    var formContent = new FormUrlEncodedContent(content);
                    // Log?.Invoke($"[{i}] [{name}] SendOrigin: {formContent.ReadAsStringAsync().Result}");
                    var task = client.PostAsync(url, formContent);
                    var response = task.Result;
                    var text = response.Content.ReadAsStringAsync().Result;
                    watch.Stop();
                    var time = watch.ElapsedMilliseconds;
                    Logger.Net.Info($"[{Prefix}] [{name}] [{i}] 耗时:{time}毫秒 接收内容:{text}");
                    if (response.StatusCode != HttpStatusCode.OK)
                    {
                        return new TRes
                        {
                            resultCode = -100,
                            success = false,
                            msg = $"{(int)response.StatusCode}:{response.StatusCode}"
                        };
                    }
                    if (text.IsNullOrWhiteSpace())
                    {
                        return new TRes
                        {
                            resultCode = -100,
                            success = false,
                            msg = $"接口[{name}]健康服务返回内容异常！"
                        };
                    }
                    var res = text.ToJsonObject<TRes>();
                    return res;
                }
                catch (AggregateException ex)
                {
                    string mainException;
                    Logger.Net.Error($"[{Prefix}] [{name}] [{i}] 异常内容:{PrintAggregateException(ex, out mainException)}");
                    return new TRes
                    {
                        success = false,
                        resultCode = -100,
                        msg = mainException
                    };
                }
                catch (Exception ex)
                {
                    Logger.Net.Error($"[{Prefix}] [{name}] [{i}] 异常内容:{ex.Message}\r\n{ex.StackTrace}");
                    return new TRes
                    {
                        success = false,
                        resultCode = -100,
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
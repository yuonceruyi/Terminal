using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using YuanTu.Consts;
using YuanTu.Core.Extension;
using YuanTu.Core.Log;
using YuanTu.PanYu.House.PanYuGateway.Base;

namespace YuanTu.PanYu.House.PanYuGateway
{
    public partial class DataHandler
    {
        public static long[] UnKnowErrorCode = { -2, -4, -100 };
        private static long _count;
        public static Uri Uri { get; set; }
        public static string HospitalId { get; set; }
        public static string HospCode { get; set; }
        public static string terminalNo { get; set; } 
        public static string OperId { get; set; }

        public static TimeSpan TimeOut { get; set; } = new TimeSpan(0, 3, 0);
        public static bool Local => FrameworkConst.FakeServer;
        public static string FakeDirectory { get; set; } = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "FakeServer");
        public static event Action<string> Log = Logger.Net.Debug;

        public static TRes Query<TRes, TReq>(TReq req)
            where TRes : res, new()
            where TReq : req
        {
            var i = Interlocked.Increment(ref _count);
            var content = new wrapper {query = req.GetParams()}.Content();
            var name = req.serviceName;
            Log?.Invoke($"[{i}] [{name}] Send: {content.ToJsonString()}");

            if (Local /*&& !name.Contains("订单")*/)
            {
                string res;
                var file = Path.Combine(FakeDirectory, HospitalId, name + ".json");
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
                Log?.Invoke($"[{i}] [{name}] Recv: {res}");
                return res.ToJsonObject<TRes>();
            }
            using (var client = new HttpClient())
            {
                client.Timeout = TimeOut;
                var task = client.PostAsync(Uri, new FormUrlEncodedContent(content));
                try
                {
                    var response = task.Result;
                    var text = response.Content.ReadAsStringAsync().Result;
                    Log?.Invoke($"[{i}] [{name}] Recv: {text}");

                    if (response.StatusCode != HttpStatusCode.OK)
                        return new TRes
                        {
                            code = -100,
                            success = false,
                            msg = $"{(int)response.StatusCode}:{response.StatusCode}"
                        };
                    if (string.IsNullOrWhiteSpace(text))
                        return new TRes
                        {
                            code = -100,
                            success = false,
                            msg = $"服务端返回无法解析的内容"
                        };
                    return text.ToJsonObject<TRes>();
                }
                catch (AggregateException ex)
                {
                    var mainException = string.Empty;
                    Log?.Invoke($"[{i}] [{name}] Exception: {PrintAggregateException(ex, out mainException)}");
                    return new TRes
                    {
                        code = -100,
                        success = false,
                        msg = mainException
                    };
                }
                catch (Exception ex)
                {
                    Log?.Invoke($"[{i}] [{name}] Exception: {ex.Message}\n{ex.StackTrace}");
                    return new TRes
                    {
                        code = -100,
                        success = false,
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
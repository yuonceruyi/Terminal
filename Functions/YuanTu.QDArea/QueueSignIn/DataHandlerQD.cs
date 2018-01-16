using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using YuanTu.Consts;
using YuanTu.Consts.FrameworkBase;
using YuanTu.Core.Log;

namespace YuanTu.QDArea.QueueSignIn
{
    class DataHandlerQD
    {
        protected long _count;
        //private static readonly HttpClient Client = new HttpClient();
        public Uri Uri { get; set; }

        public virtual Result<TRes> Query<TRes, TReq>(TReq req)
        where TRes : IRes, new()
        where TReq : IReq
        {
            if (FrameworkConst.FakeServer)
                return QueryLocal<TRes, TReq>(req);

            var i = Interlocked.Increment(ref _count);
            Logger.Net.Info($"[{i}] [{req.serviceUrl}] Send: {JsonConvert.SerializeObject(req)}");
            try
            {
                var Client = new HttpClient();
                var watch = Stopwatch.StartNew();
                var formContent = new FormUrlEncodedContent(req.GetParams());
                var task = Client.PostAsync(new Uri(Uri, req.serviceUrl), formContent);
                //var url = new Uri(Uri, req.serviceUrl);               
                //url = new Uri(url, createUrl(req.GetParams()));                
                //var response = await Client.GetAsync(url);
                var response = task.Result;
                var text = response.Content.ReadAsStringAsync().Result;
                watch.Stop();
                var time = watch.ElapsedMilliseconds;
                Logger.Net.Info($"[{i}] [{req.serviceUrl}] Elapsed:{time}ms Recv: {text}");
                if (!response.IsSuccessStatusCode)
                    return Result<TRes>.Fail($"服务器返回状态:{(int)response.StatusCode} {response.StatusCode}");
                var res = JsonConvert.DeserializeObject<TRes>(text);
                return Result<TRes>.Success(res);
            }
            catch (AggregateException ex)
            {
                string mainException;
                Logger.Net.Warn($"[{i}] [{req.serviceUrl}] Exception: {PrintAggregateException(ex, out mainException)}");
                return Result<TRes>.Fail(mainException, ex);
            }
            catch (Exception ex)
            {
                Logger.Net.Warn($"[{i}] [{req.serviceUrl}] Exception: {ex.Message}\n{ex.StackTrace}");
                return Result<TRes>.Fail(ex.Message, ex);
            }
        }
        public Result<TRes> QueryLocal<TRes, TReq>(TReq req)
            where TRes : IRes, new()
            where TReq : IReq
        {
            var i = Interlocked.Increment(ref _count);
            Logger.Net.Info($"[{i}] [{req.serviceUrl}] Send: {JsonConvert.SerializeObject(req)}");

            var name = typeof(TReq).Name.Substring(3);
            string fileName = $"FakeServer/{name}.json";
            if (!File.Exists(fileName))
            {
                var error = $"{fileName} not found";
                Logger.Net.Info($"[{i}] [{req.serviceUrl}] Elapsed:-1ms Recv: {error}");
                return Result<TRes>.Fail(error);
            }

            try
            {
                using (var sr = new StreamReader(fileName, Encoding.UTF8))
                {
                    var text = sr.ReadToEnd();
                    var res = JsonConvert.DeserializeObject<TRes>(text);
                    Logger.Net.Info($"[{i}] [{req.serviceUrl}] Elapsed:-1ms Recv: {text}");
                    return Result<TRes>.Success(res);
                }
            }
            catch (Exception ex)
            {
                return Result<TRes>.Fail($"{fileName} {ex.Message}", ex);
            }
        }

        public string PrintAggregateException(AggregateException ex, out string mainException)
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

        /// <summary>
        /// 创建url中的表单数据
        /// </summary>
        /// <returns></returns>
        public static string createUrl(Dictionary<string,string> dic)
        {
            string tStr = "?";
            if (dic == null|| dic.Count==0)
            {
                return tStr;
            }

            foreach (var key in dic.Keys)
            {
                
                    tStr += string.Format("{0}={1}&", key, dic[key]);           
            }
            tStr = tStr.TrimEnd('&');
            return tStr;
        }
    }
}

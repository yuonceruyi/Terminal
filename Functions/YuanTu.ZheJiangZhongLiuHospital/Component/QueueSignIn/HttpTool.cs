using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using YuanTu.Core.Extension;
using YuanTu.Core.Log;

namespace YuanTu.ZheJiangZhongLiuHospital.Component.QueueSignIn
{
    public class HttpTool
    {
        private static readonly HttpClient Client = new HttpClient();
        public static T Post<T>(string url, Dictionary<string, string> kv) where T : class
        {
            try
            {
                var tsk = Client.PostAsync(url, new FormUrlEncodedContent(kv));
                var rest = tsk.Result.Content.ReadAsStringAsync().Result;
                Logger.Net.Info($"[排队叫号]Url:{url} 参数:{kv.ToJsonString()} 返回:{rest}");
                return rest.ToJsonObject<T>();
            }
            catch (Exception ex)
            {
                Logger.Net.Error($"[排队叫号]异常 Url:{url} 参数:{kv.ToJsonString()} 错误原因:{ex.Message}");
                return default(T);
            }
        }
    }
}

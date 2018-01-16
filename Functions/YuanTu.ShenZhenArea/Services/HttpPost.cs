using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;

namespace YuanTu.ShenZhenArea.Services
{
    public  class HttpPost
    {
        private static long _count;
       
        public static event Action<string> Log;

        public static string PostData(string req, string uri)
        {
            var i = Interlocked.Increment(ref _count);
            try
            {
                Log?.Invoke($"[{i}] [{uri}] Send: {req}");
                var paramData = "name=" + System.Web.HttpUtility.UrlEncode(req, System.Text.Encoding.UTF8); ;
                var byteArray = Encoding.UTF8.GetBytes(paramData);
                var webReq = (HttpWebRequest)WebRequest.Create(new Uri(uri));
                webReq.Method = "POST";
                //webReq.ContentType = "application/json; charset=utf-8";
                webReq.ContentType = "application/x-www-form-urlencoded; charset=utf-8";
                webReq.ContentLength = byteArray.Length;
                using (var newStream = webReq.GetRequestStream())
                    newStream.Write(byteArray, 0, byteArray.Length);//写入参数
                var response = (HttpWebResponse)webReq.GetResponse();
                string ret;
                // ReSharper disable once AssignNullToNotNullAttribute
                using (var sr = new StreamReader(response.GetResponseStream(), Encoding.UTF8))
                    ret = sr.ReadToEnd();
                response.Close();
                Log?.Invoke($"[{i}] [{uri}] Recv: {ret}");
                return ret;
            }
            catch (Exception ex)
            {
                Log?.Invoke($"[{i}] [{uri}] Exception: {ex.Message}\n{ex.StackTrace}");
                return null;
            }
        }
    }
}

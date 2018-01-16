using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using YuanTu.Consts;
using YuanTu.Consts.FrameworkBase;
using YuanTu.Core.DB;
using YuanTu.Core.Extension;
using YuanTu.Core.Log;
using YuanTu.YiWuArea.Models;

namespace YuanTu.YiWuArea.Commons
{
    public class Tools
    {
        public static Result<T> YiWuMiddlwHttpMethod<T>(string url, HttpMethodEnum httpMethod, string content)
        {
            var method = httpMethod.ToString().ToUpper();
            try
            {
                var watch = Stopwatch.StartNew();
                var client = new HttpClient();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                var request = new HttpRequestMessage(new HttpMethod(method), url);
                request.Headers.Add("token", "3861757c-5c2c-4873-b97b-87ae97b1eaae");//授权
                if (content != null)
                {
                    request.Content = new StringContent(content, Encoding.UTF8, "application/json");
                }
                var rest = client.SendAsync(request).Result;
                var retContent = rest.Content.ReadAsStringAsync().Result;
                watch.Stop();
                Logger.Net.Info($"[HttpMethod] {url} 协议:{method} 发送请求:{request} 发送内容:{content} 接收内容:{retContent} 耗时:{watch.ElapsedMilliseconds}ms");
                //return Result<string>.Success(retContent);
                return retContent.ToJsonObject<Result<T>>();
            }
            catch (Exception ex)
            {
                DBManager.Insert("Data\\Middleware.db",new YiWuAreaMiddlewareMessageModel
                {
                    FullUrl = url,
                    HttpMethod = method,
                    Content = content,
                    FailedMessage = ex.Message
                });

                return Result<T>.Fail(-200,ex.Message);
            }

        }

        public static Result<T> XmlDescribe<T>(string input) where T:class 
        {
            try
            {
                var serializer = new XmlSerializer(typeof(T));
                using (var sr=new StringReader(input))
                {
                    var t = serializer.Deserialize(sr) as T;
                    return Result<T>.Success(t);
                }

            }
            catch (Exception ex)
            {
                Logger.Main.Error($"[Xml反序列化异常]，入参:{input},转换类型:{typeof(T)},异常消息:{ex}");
                return Result<T>.Fail(ex.Message);
            }
        }
    }

    public enum HttpMethodEnum
    {
        Get,
        Post,
        Put,
        Delete
    }
}

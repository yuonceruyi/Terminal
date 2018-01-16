using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using YuanTu.Consts;
using YuanTu.Consts.UserCenter;
using YuanTu.Core.Extension;
using YuanTu.Core.Log;
using YuanTu.Core.Reporter;
using YuanTu.Core.Systems;

namespace YuanTu.Core.UserCenter
{
    public partial class DataHandler : IDataHandler
    {
        private const string Prefix = "用户管理平台";

        private readonly string[] _noTokenService =
            {
               "生成登录二维码",
               "根据uuid获取绑定就诊人信息",
               "取消扫码",
               "获取deviceSecret",
               "获取token"
            };

        private long _count;

        public TimeSpan DefaultTimeOut { get; set; } = new TimeSpan(0, 3, 0);

        public string FakeDirectory { get; set; } = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "FakeServer");

        public TRes Query<TRes, TReq>(TReq req, string url)
            where TRes : UserCenterResponse, new()
            where TReq : UserCenterRequest
        {
            var i = Interlocked.Increment(ref _count);
            var content = req.GetParams();
            var name = req.ServiceName;
            url = url == null ? $"{FrameworkConst.UserCenterUrl}{req.UrlPath}" : $"{url}{req.UrlPath}";

            Logger.Net.Info($"[{Prefix}] [{name}] [{i}] 发送内容:{content.ToJsonString()}");
            if (FrameworkConst.FakeServer)
            {
                string res;
                var file = Path.Combine(FakeDirectory, FrameworkConst.HospitalId, $"{name}.json");
                if (!File.Exists(file))
                {
                    file = Path.Combine(FakeDirectory, $"{name}.json");
                    if (!File.Exists(file))
                        return new TRes
                        {
                            success = false,
                            msg = $"{file} 未找到"
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
                client.Timeout = DefaultTimeOut;
                try
                {
                    var watch = Stopwatch.StartNew();
                    var formContent = new FormUrlEncodedContent(content);
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
                            success = false,
                            msg = $"{(int)response.StatusCode}:{response.StatusCode}"
                        };
                    }
                    if (text.IsNullOrWhiteSpace())
                    {
                        ReportService.网关返回异常($"接口[{name}]平台返回null", null);
                        return new TRes
                        {
                            success = false,
                            msg = $"接口[{name}]平台返回内容异常！"
                        };
                    }
                    var res = text.ToJsonObject<TRes>();
                    if ((!res.success) && string.IsNullOrWhiteSpace(res.msg))
                    {
                        ReportService.网关返回异常($"接口[{name}]平台返回false，但msg中没有错误内容", null);
                    }
                    if (_noTokenService.Contains(req.ServiceName)) return res;
                    if (res.resultCode == "403" || req.token == null)
                    {
                        //token过期或者为空,重新获取
                        #region
                        var reqOrg = req;
                        var req获取deviceSecret = new req获取deviceSecret
                        {
                            deviceMac = NetworkManager.MAC
                        };
                        var res获取deviceSecret = DataHandlerEx.获取deviceSecret(req获取deviceSecret, FrameworkConst.DeviceUrl);

                        if (!res获取deviceSecret.success)
                        {
                            return new TRes
                            {
                                success = false,
                                msg = $"{res获取deviceSecret.msg}"
                            };
                        }
                        var req获取token = new req获取token
                        {
                            deviceSecret = res获取deviceSecret.data
                        };
                        var res获取token = DataHandlerEx.获取token(req获取token, FrameworkConst.DeviceUrl);
                        if (!res获取token.success)
                        {
                            return new TRes
                            {
                                success = false,
                                msg = $"{res获取token.msg}"
                            };
                        }
                        FrameworkConst.Token = res获取token.data;
                        reqOrg.token = res获取token.data;
                        i = Interlocked.Increment(ref _count);
                        content = reqOrg.GetParams();
                        name = reqOrg.ServiceName;
                        url = $"{FrameworkConst.UserCenterUrl}{req.UrlPath}";

                        Logger.Net.Info($"[{Prefix}] [{name}] [{i}] 发送内容:{content.ToJsonString()}");

                        using (var clientOrg = new HttpClient())
                        {
                            clientOrg.Timeout = DefaultTimeOut;
                            try
                            {
                                watch = Stopwatch.StartNew();
                                formContent = new FormUrlEncodedContent(content);
                                task = client.PostAsync(url, formContent);
                                response = task.Result;
                                text = response.Content.ReadAsStringAsync().Result;
                                watch.Stop();
                                time = watch.ElapsedMilliseconds;
                                Logger.Net.Info($"[{Prefix}] [{name}] [{i}] 耗时:{time}毫秒 接收内容:{text}");
                                if (response.StatusCode != HttpStatusCode.OK)
                                {
                                    return new TRes
                                    {
                                        success = false,
                                        msg = $"{(int)response.StatusCode}:{response.StatusCode}"
                                    };
                                }
                                if (text.IsNullOrWhiteSpace())
                                {
                                    ReportService.网关返回异常($"接口[{name}]平台返回null", null);
                                    return new TRes
                                    {
                                        success = false,
                                        msg = $"接口[{name}]平台返回内容异常！"
                                    };
                                }
                                res = text.ToJsonObject<TRes>();
                                if ((!res.success) && string.IsNullOrWhiteSpace(res.msg))
                                {
                                    ReportService.网关返回异常($"接口[{name}]平台返回false，但msg中没有错误内容", null);
                                }

                                return res;
                            }
                            catch (AggregateException ex)
                            {
                                string mainException;
                                Logger.Net.Error($"[{Prefix}] [{name}] [{i}] 异常内容:{PrintAggregateException(ex, out mainException)}");
                                ReportService.网关返回异常($"接口[{name}]平台异常，异常内容:{PrintAggregateException(ex, out mainException)}", null);
                                return new TRes
                                {
                                    success = false,
                                    msg = mainException
                                };
                            }
                            catch (Exception ex)
                            {
                                Logger.Net.Error($"[{Prefix}] [{name}] [{i}] 异常内容:{ex.Message}\r\n{ex.StackTrace}");
                                ReportService.网关返回异常($"接口[{name}]平台异常，异常内容:{ex.Message}\r\n{ex.StackTrace}", null);
                                return new TRes
                                {
                                    success = false,
                                    msg = ex.Message
                                };
                            }
                        }
                        #endregion
                    }
                    return res;
                }
                catch (AggregateException ex)
                {
                    string mainException;
                    Logger.Net.Error($"[{Prefix}] [{name}] [{i}] 异常内容:{PrintAggregateException(ex, out mainException)}");
                    ReportService.网关返回异常($"接口[{name}]平台异常，异常内容:{PrintAggregateException(ex, out mainException)}", null);
                    return new TRes
                    {
                        success = false,
                        msg = mainException
                    };
                }
                catch (Exception ex)
                {
                    Logger.Net.Error($"[{Prefix}] [{name}] [{i}] 异常内容:{ex.Message}\r\n{ex.StackTrace}");
                    ReportService.网关返回异常($"接口[{name}]平台异常，异常内容:{ex.Message}\r\n{ex.StackTrace}", null);
                    return new TRes
                    {
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
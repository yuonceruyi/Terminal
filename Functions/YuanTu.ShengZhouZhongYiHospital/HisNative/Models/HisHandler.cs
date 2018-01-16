using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Practices.ServiceLocation;
using YuanTu.Consts;
using YuanTu.Consts.FrameworkBase;
using YuanTu.Consts.Services;
using YuanTu.Core.Extension;
using YuanTu.Core.Log;
using YuanTu.Core.Reporter;

namespace YuanTu.ShengZhouZhongYiHospital.HisNative.Models
{
    public static partial class HisHandleEx
    {
        private const string Name = "本地HIS服务";
      
        private static Encoding _encoding = Encoding.GetEncoding("GBK");
        private static int Index = 0;
        public static string FakeDirectory { get; set; } = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "FakeServer");

        public static TRes Handler<TReq, TRes>(TReq req, string serviceName, int timeoutSeconds) where TReq : HisReq
            where TRes : HisRes, new()
        {
            if (ConstInner.LocalHis)
            {
                var name = req.服务编号;
                string res;
                var file = Path.Combine(FakeDirectory, FrameworkConst.HospitalId, $"{name}.txt");
                if (!File.Exists(file))
                {
                    file = Path.Combine(FakeDirectory, $"{name}.txt");
                    if (!File.Exists(file))
                        return new TRes
                        {
                            RetCode = -1,
                            Message = "未找到" + name + "的Faker文件"
                        };
                }
                using (var sr = new StreamReader(file,Encoding.UTF8))
                {
                    res = sr.ReadToEnd();
                }

                //Logger.Net.Info($"[{Name}] [{name}] [{i}] 耗时:{0}毫秒 接收内容:{res}");

                Thread.Sleep(500);
                var resV = new TRes();
                resV.Deserialize(res);
                return resV;
            }

            var manager = ServiceLocator.Current.GetInstance<IConfigurationManager>();
            var ip = manager.GetValue("LocalSocketIP");
            var port = manager.GetValueInt("LocalSocketPort");
            var i = Interlocked.Increment(ref Index);
            using (var _client = new TcpClient())
            {
                _client.ReceiveTimeout = timeoutSeconds*1000;
                //_client.Connect(ip, port);
                try
                {
                    _client.ConnectAsync(ip, port).Wait(1000);
                }
                catch (Exception)
                {
                    
                   
                }
               
                if (!_client.Connected)
                {
                    Logger.Net.Info($"[{Name}] 连接异常，IP:{ip} 端口:{port}");
                    ReportService.HIS请求超时($"[{Name}] 连接异常，IP:{ip} 端口:{port}", $"联系医护人员检测HIS本地服务是否开启！");
                    return new TRes() {RetCode = -1, Message = "医院本地服务连接失败，请联系医护工作人员处理！"};
                }
                var ns = _client.GetStream();
                try
                {
                    var send = req.Serialize();
                    Logger.Net.Info($"[{Name}][{serviceName}][{i}]入参:{send}");
                    var rec = string.Empty;
                    var watch = Stopwatch.StartNew();
                    var ret = Task.Run(() =>
                    {
                        var sendBts = _encoding.GetBytes(send);
                        ns.Write(sendBts, 0, sendBts.Length);
                        var buff = new byte[4096];
                        var len = ns.Read(buff, 0, buff.Length);
                        rec = _encoding.GetString(buff, 0, len);
                    }).Wait(timeoutSeconds*1000);
                    watch.Stop();
                    Logger.Net.Info($"[{Name}][{serviceName}][{i}]出参:{rec} 耗时:{watch.ElapsedMilliseconds}ms");
                    if (ret)
                    {
                        var res = new TRes();
                        res.Deserialize(rec);
                        return res;
                    }
                    ReportService.HIS请求超时($"[{Name}]][{serviceName}] 入参:{send}", $"联系医护人员检测HIS本地服务是否正常工作！");
                    return new TRes() {RetCode = -2, Message = "医院本地服务接收超时，请联系医护工作人员处理！"};
                }
                catch (Exception ex)
                {
                    Logger.Net.Info($"[{Name}][{serviceName}][{i}]发生异常：{ex.Message} {ex.StackTrace}");
                    ReportService.HIS请求超时($"[{Name}]][{serviceName}] 异常:{ex.Message}", $"联系远图工程师处理！");
                    return new TRes() {RetCode = -2, Message = "医院本地服务接收失败，请联系医护工作人员处理！"};
                }
            }
        }
    }
}

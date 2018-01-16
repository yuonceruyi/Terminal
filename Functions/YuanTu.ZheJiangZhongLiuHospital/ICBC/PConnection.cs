using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Xml;
using System.Xml.Serialization;
using YuanTu.Consts;
using YuanTu.Consts.FrameworkBase;
using YuanTu.Core.Log;

namespace YuanTu.ZheJiangZhongLiuHospital.ICBC
{
    public static class PConnection
    {
        private static readonly Encoding Encoding = Encoding.GetEncoding("GBK");
        private static readonly XmlWriterSettings Settings;
        private static readonly XmlSerializerNamespaces Ns;
        private static long _count;
        public static long Count => _count++;

        static PConnection()
        {
            Settings = new XmlWriterSettings
            {
                Encoding = Encoding,
                Indent = false,
                OmitXmlDeclaration = false
            };
            Ns = new XmlSerializerNamespaces();
            Ns.Add("", "");
        }

        public static IPEndPoint IcbcipEndPoint { get; set; }

        public static string HisCode { get; set; }


        public static string Serialize(object o)
        {
            var xs = new XmlSerializer(o.GetType());
            using (var stream = new MemoryStream())
            {
                using (var writer = XmlWriter.Create(stream, Settings))
                {
                    xs.Serialize(writer, o, Ns);
                }
                return Encoding.GetString(stream.ToArray());
            }
        }

        private static T Deserialize<T>(string text)
            where T : class, IRes
        {
            return new XmlSerializer(typeof(T)).Deserialize(new StringReader(text)) as T;
        }

        private const string EndFlag = "</TransInfo>";

        static string ScoketRequestWithEndFlag(string s, int timeout = 30)
        {
            var client = new TcpClient();
            var timer = new Timer(state => client.Close(), null, 30000, 0);
            client.Connect(IcbcipEndPoint);
            timer.Dispose();
            client.ReceiveTimeout = timeout * 1000;
            using (var ns = client.GetStream())
            using (var sw = new StreamWriter(ns, Encoding))
            using (var sr = new StreamReader(ns, Encoding))
            {
                var textLen = Encoding.GetByteCount(s);
                sw.Write($"100000{textLen:D4}");
                sw.Write(s);
                sw.Flush();
                var sb = new StringBuilder();
                while (true)
                {
                    var buffer = new char[256];
                    var c = sr.Read(buffer, 0, 256);
                    sb.Append(buffer, 0, c);
                    if (sb.ToString().Contains(EndFlag))
                        break;
                }
                return sb.ToString(10, sb.Length - 10);
            }
        }

        public static Result<T> Handle<T>(IReq req) where T : class, IRes
        {
            var n = $"[工行服务] [{req.ClassName}] [{Count}]";
            var sw = Stopwatch.StartNew();
            try
            {
                var send = Serialize(req);

                Logger.Net.Info($"{n} 发送内容: {send}");
                string recv = string.Empty;
                recv = FrameworkConst.FakeServer ? "<?xml version='1.0' encoding='GBK' ?><TransInfo><ResultFlag>00000</ResultFlag><ResultMark>#</ResultMark><Remain>3110</Remain><ComLimit>500000</ComLimit></TransInfo>" 
                    : ScoketRequestWithEndFlag(send);
                //if (req.ClassName == "充值")
                //{
                //    recv =
                //        "<?xml version='1.0' encoding='GBK' ?><TransInfo><ResultFlag>10002</ResultFlag><ResultMark>报文验证失败:null</ResultMark></TransInfo>";
                //}
                var res = Deserialize<T>(recv);

                Logger.Net.Info($"{n} 耗时:{sw.ElapsedMilliseconds}毫秒 Recv: {recv}");

                if (Convert.ToInt32(res.ResultFlag) != 0)
                    return Result<T>.Fail(res.ResultMark);

                return Result<T>.Success(res);
            }
            catch (SocketException ex)
            {
                Logger.Net.Error($"{n} 耗时:{sw.ElapsedMilliseconds}毫秒 网络通信出错 {typeof(T).Name}\n{ex.Message}\n{ex.StackTrace}");
                return Result<T>.Fail($"网络通信出错:{ex}");
            }
            catch (Exception ex)
            {
                Logger.Net.Error($"{n} 耗时:{sw.ElapsedMilliseconds}毫秒 出错 {typeof(T).Name}\n{ex.Message}\n{ex.StackTrace}");
                return Result<T>.Fail($"出错:{ex}");
            }
        }
    }
}
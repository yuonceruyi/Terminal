using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Xml;
using System.Xml.Serialization;
using YuanTu.Consts.FrameworkBase;
using YuanTu.Core.Log;

namespace YuanTu.ZheJiangHospital.ICBC
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

        public static IPEndPoint ICBC { get; set; }

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

        private static string SocketRequest(string s)
        {
            var textLen = Encoding.GetByteCount(s);
            s = "100000" + textLen.ToString("D4") + s;

            var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            var timer = new Timer(state => socket.Close(), null, 30000, 0);
            socket.Connect(ICBC);
            var bytes = Encoding.GetBytes(s);
            socket.Send(bytes);

            var len = 65536;
            var data = new byte[len];
            len = socket.Receive(data);
            while (socket.Available > 0)
            {
                var n = socket.Available;
                var newdata = new byte[len + n];
                Array.Copy(data, newdata, len);
                data = newdata;
                var count = socket.Receive(data, len, n, SocketFlags.None);
                len += count;
            }
            timer.Dispose();

            var r = Encoding.GetString(data).Trim('\n');
            r = r.Substring(10);
            return r;
        }

        private const string EndFlag = "</TransInfo>";

        static string ScoketRequestWithEndFlag(string s, int timeout = 30)
        {
            var client = new TcpClient();
            var timer = new Timer(state => client.Close(), null, 30000, 0);
            client.Connect(ICBC);
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

                //var recv = SocketRequest(send);
                var recv = ScoketRequestWithEndFlag(send);

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
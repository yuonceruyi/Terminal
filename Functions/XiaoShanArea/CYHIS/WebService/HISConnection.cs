using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using YuanTu.Consts;
using YuanTu.Core.Log;

namespace YuanTu.YuHangArea.CYHIS.WebService
{
	public static class HisConnection
	{
		private static readonly Encoding encoding = new UTF8Encoding(false);
		private static readonly XmlWriterSettings settings;
		private static readonly XmlSerializerNamespaces ns;
 
		static HisConnection()
		{
			settings = new XmlWriterSettings
			{
				Encoding = encoding,
				Indent = false,
				OmitXmlDeclaration = false
			};
			ns = new XmlSerializerNamespaces();
			ns.Add("", "");
		}

		public static string Serilize(object reqItem)
		{
			var serializer = new XmlSerializer(reqItem.GetType());
			string text;
			using (var ms = new MemoryStream())
			{
				using (var textWriter = new StreamWriter(ms, encoding))
				{
					using (var xmlWriter = XmlWriter.Create(textWriter, settings))
					{
						serializer.Serialize(xmlWriter, reqItem, ns);
					}
					text = encoding.GetString(ms.GetBuffer()).Trim('\0');
				}
			}
			return text;
		}

		public static T Handle<T>(IReqBase req)
			where T : class, IResBase
		{
			try
			{
				var send = Serilize(req);
				Logger.Net.Info("Send:" + req.GetType().Name + " " + send);
			    string recv;
                if (FrameworkConst.FakeServer)
                    recv = SocketRequest(send);
                else
                {
                    string strRecv = "";     
                    //var ret = Data.service.runservice("HIS1.Biz." + req.GetType().Name.Split('_')[0], send, ref strRecv);
			          recv = strRecv;
			    }
			    Logger.Net.Info("Recv:" + req.GetType().Name + " " + recv);

			    if (string.IsNullOrEmpty(recv))
					throw new Exception("HIS返回数据为空");

				var res = new XmlSerializer(typeof (T))
					.Deserialize(new StringReader(recv)) as T;
				if (res == null)
					return null;
				Logger.Net.Info(res.ToString());
				return res;
			}
			catch (SocketException ex)
			{
				Logger.Net.Error("网络通信出错" + typeof (T).Name + "\n" + ex.Message + "\n" + ex.StackTrace);
				return null;
			}
			catch (Exception ex)
			{
				Logger.Net.Error("出错" + typeof (T).Name + "\n" + ex.Message + "\n" + ex.StackTrace);
				return null;
			}
		}
        #region Test

        public static IPAddress Address=IPAddress.Loopback;
		public static Encoding Encoding = Encoding.UTF8;
		public static int Port=8888;

		public static string SocketRequest(string message)
		{
			var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
			socket.Connect(Address, Port);
			var bytes = Encoding.GetBytes(message);
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
			var ret = Encoding.GetString(data).Trim();
			return ret;
		}
		#endregion Test
	}
}
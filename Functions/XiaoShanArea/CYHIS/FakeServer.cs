using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Xml;

namespace YuanTu.YuHangArea.CYHIS
{
	public class FakeServer
	{
		private readonly string dir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "FakeServer");
		private Socket listener;
		private event EventHandler log;

		private void Log(string text)
		{
			if (log == null)
				return;
			log(this, new LogEventArgs(text));
		}

		public void Start(int port = 8888)
		{
			listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
			listener.Bind(new IPEndPoint(IPAddress.Any, port));
			listener.Listen(16);
			Log("开始监听端口" + port);
			while (true)
			{
				var client = listener.Accept();
				var t = new Thread(Accept) { IsBackground = true };
				t.Start(client);
			}
		}

		private void Accept(object o)
		{
			var client = o as Socket;
			if (client == null)
				return;
			Log(client.RemoteEndPoint + "==================\n");
			var encoding = Encoding.UTF8;
			try
			{
				Mode mode;
				var data = new byte[4];
				var count = client.Receive(data);
				var text = encoding.GetString(data).Trim();
				if (text.Contains("<"))
				{
					mode = Mode.WebService;
					text = Receive(client, encoding, "", data);
				}
				else
				{
					mode = Mode.RunExe;
					text = Receive(client, encoding, "", data);
				}
				Log(client.RemoteEndPoint + " Recv:\n" + text);

				text = Respond(text, mode).Trim();
        
				data = Encoding.UTF8.GetBytes(text);
				count = client.Send(data);
				while (count < data.Length)
				{
					count += client.Send(data, count, data.Length - count, SocketFlags.None);
				}
				Log(client.RemoteEndPoint + " Send:\n" + text);
			}
			catch (Exception e)
			{
				Log(e.Message + "\n" + e.StackTrace);
			}
			finally
			{
				client.Close();
			}
		}

		private string Receive(Socket client, Encoding encoding, string endFlag, byte[] head)
		{
			var buffer = new byte[4096];
			var count = client.Receive(buffer);
			var tmp = new byte[head.Length + count];
			Array.Copy(head, 0, tmp, 0, head.Length);
			Array.Copy(buffer, 0, tmp, head.Length, count);
			var data = tmp;
			var text = encoding.GetString(data).Trim();
			if (String.IsNullOrEmpty(endFlag))
				return text;
			while (!text.EndsWith(endFlag))
			{
				count = client.Receive(buffer);
				tmp = new byte[data.Length + count];
				Array.Copy(data, 0, tmp, 0, data.Length);
				Array.Copy(buffer, 0, tmp, data.Length, count);
				data = tmp;
				text += encoding.GetString(data).Trim();
			}
			return text;
		}

		private string Respond(string text, Mode mode)
		{
			string file;
			string tradeCode;
			string[] list;
			switch (mode)
			{
				case Mode.WebService:
					var doc = new XmlDocument();
					doc.LoadXml(text);
					var header = doc.ChildNodes[1];
					file = Path.Combine(dir, header.Name.Split('_')[0] + ".txt");
					if (!File.Exists(file))
						file = Path.Combine(dir, "HISError.txt");
					break;

				case Mode.RunExe:
					list = text.Split('|');
					tradeCode = list[0];

					file = Path.Combine(dir, tradeCode + ".txt");
					if (!File.Exists(file))
						file = Path.Combine(dir, "DLLError.txt");
					break;
				default:
					file = Path.Combine(dir, "Error.txt");
					break;
			}
			using (var streamReader = new StreamReader(file, Encoding.GetEncoding("GBK")))
				text = streamReader.ReadToEnd();
			return text;
		}

		public class LogEventArgs : EventArgs
		{
			public string info;

			public LogEventArgs(string text)
			{
				info = text;
			}
		}

		private enum Mode
		{
			WebService,
			RunExe,
		}
	}
}
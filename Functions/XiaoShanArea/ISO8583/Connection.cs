using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using YuanTu.Consts.FrameworkBase;
using YuanTu.Core.Log;

namespace YuanTu.YuHangArea.ISO8583
{
    public class Connection
    {
        public IPAddress Address { get; set; }

        public int Port { get; set; }

        public int TimeOut { get; set; } = 30000;//3秒

        public static LoggerWrapper Log { get; set; } = Logger.POS;

        public Result<byte[]> Handle(byte[] send)
        {
            try
            {
                using (var client = new TcpClient
                {
                    SendTimeout = TimeOut,
                    ReceiveTimeout = TimeOut
                }){
                    //var rest = Task.Factory.StartNew(() =>
                    //{
                    //    try
                    //    {
                    //        client.Connect(Address, Port);
                    //        return true;
                    //    }
                    //    catch (Exception ex)
                    //    {
                    //        Log.Error($"IP:{Address} Port:{Port} "+ex.Message);
                    //        Thread.Sleep(5000);
                    //        return false;
                    //    }
                    //}).Wait(5000);
                    
                    //if (!rest)
                    //{
                    //    var message = "服务器连接失败";
                    //    Log.Error(message);
                    //    return Result<byte[]>.Fail(message);
                    //}
                    try
                    {
                        client.Connect(Address, Port);
                    }
                    catch (Exception ex)
                    {
                        var message = "服务器连接失败";
                        Log.Error($"服务器连接失败 IP:{Address} Port:{Port} " + ex.Message);
                        return Result<byte[]>.Fail(message);
                    }
                    Log.Debug("Send\n" + Print(send));
                    var socket = client.Client;
                    socket.Send(send);

                    var lengthBytes = new byte[2];
                    var n = 0;
                    while (n < 2)
                        n += socket.Receive(lengthBytes, n, 2 - n, SocketFlags.None);

                    Log.Debug("Pre Recv:" + BitConverter.ToString(lengthBytes, 0) + "\r\n");
                    var len = int.Parse(BitConverter.ToString(lengthBytes, 0).Replace("-", ""), System.Globalization.NumberStyles.HexNumber);
                    Log.Debug("Rec Len:" + len + "\r\n");
                    var recv = new byte[len + 2];
                    Array.Copy(lengthBytes, recv, 2);
                    n = 0;
                    while (n < len)
                        n += socket.Receive(recv, 2 + n, len - n, SocketFlags.None);
                    Log.Debug("Recv\n" + Print(recv));
                    client.Close();
                    return Result<byte[]>.Success(recv);
                }
            }
            catch (SocketException ex)
            {
                Log.Warn($"{ex.Message}\n{ex.ErrorCode}:{ex.SocketErrorCode}\n{ex.StackTrace}");
                return Result<byte[]>.Fail(ex.Message, ex);
            }
            catch (Exception ex)
            {
                Log.Warn($"{ex.Message}\n{ex.StackTrace}");
                return Result<byte[]>.Fail(ex.Message, ex);
            }
        }

        public static string Print(byte[] data)
        {
            var sb = new StringBuilder();
            var lineCount = data.Length / 16 + (data.Length % 16 > 0 ? 1 : 0);
            for (var i = 0; i < lineCount; i++)
                PrintLine(data, i, sb);
            return sb.ToString();
        }

        public static void PrintLine(byte[] data, int index, StringBuilder sb)
        {
            var start = index * 0x10;
            sb.Append($" {start:X4}:");
            for (var i = 0; i < 16; i++)
            {
                if (i == 8)
                    sb.Append(" ");
                if (start + i < data.Length)
                    sb.Append($" {data[start + i]:X2}");
                else
                    sb.Append("   ");
            }
            sb.Append(" ");
            for (var i = 0; i < 16; i++)
            {
                if (i == 8)
                    sb.Append(" ");
                if (start + i < data.Length)
                {
                    var b = data[start + i];
                    if (b > 128)
                    {
                        sb.Append('.');
                    }
                    else
                    {
                        var c = Convert.ToChar(b);
                        sb.Append(char.IsControl(c) ? '.' : c);
                    }
                }
                else
                    sb.Append(" ");
            }
            sb.Append($" :{start + 15:X4}\n");
        }
    }
}
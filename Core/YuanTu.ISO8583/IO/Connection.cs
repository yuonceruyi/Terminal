using System;
using System.Globalization;
using System.Net.Sockets;
using System.Text;
using log4net;
using YuanTu.Consts.FrameworkBase;
using YuanTu.ISO8583.Enums;
using YuanTu.ISO8583.Interfaces;
using YuanTu.ISO8583.Util;

namespace YuanTu.ISO8583.IO
{
    public class Connection : IConnection
    {
        public int TimeOut { get; set; } = 30000;

        public ILog Log { get; set; } = POSLogger.Net;

        public IBuildConfig BuildConfig { get; set; }

        public IConfig Config { get; set; }

        public Result<byte[]> Handle(byte[] send)
        {
            try
            {
                using (var client = new TcpClient
                {
                    SendTimeout = TimeOut,
                    ReceiveTimeout = TimeOut
                })
                {
                    if (!client.ConnectAsync(Config.Address, Config.Port).Wait(5000))
                    {
                        var message = "服务器连接失败";
                        Log.Error(message);
                        return Result<byte[]>.Fail(message);
                    }
                    Log.Debug("Send\n" + Print(send));
                    var socket = client.Client;
                    socket.Send(send);

                    var lenlen = BuildConfig.LengthBytesLength;
                    var lengthBytes = new byte[lenlen];
                    var n = 0;
                    while (n < lenlen)
                        n += socket.Receive(lengthBytes, n, lenlen - n, SocketFlags.None);
                    Log.Debug("Recvlength\n" + n);
                    var len = 0;
                    switch (BuildConfig.LengthBytesFormat)
                    {
                        case Format.BCD:
                            len = int.Parse(lengthBytes.Bytes2Hex());
                            break;
                        case Format.ASCII:
                            len = Convert.ToInt32(Encoding.Default.GetString(lengthBytes));
                            break;
                        case Format.Binary:
                            len = int.Parse(lengthBytes.Bytes2Hex(), NumberStyles.HexNumber);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                    var recv = new byte[len + lenlen];
                    Array.Copy(lengthBytes, recv, lenlen);
                    n = 0;
                    while (n < len)
                        n += socket.Receive(recv, lenlen + n, len - n, SocketFlags.None);
                    Log.Debug("Recv\n" + Print(recv));
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
            var lineCount = data.Length/16 + (data.Length%16 > 0 ? 1 : 0);
            for (var i = 0; i < lineCount; i++)
                PrintLine(data, i, sb);
            return sb.ToString();
        }

        public static void PrintLine(byte[] data, int index, StringBuilder sb)
        {
            var start = index*0x10;
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
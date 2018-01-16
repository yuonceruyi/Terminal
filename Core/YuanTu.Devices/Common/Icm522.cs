using System;
using System.IO.Ports;
using YuanTu.Consts.FrameworkBase;
using YuanTu.Core.Log;

namespace YuanTu.Devices.Common
{
    public class Icm522
    {
        private const string DeviceName = "ICM522";
        private static readonly SerialPort SerialPort = new SerialPort();

        //public static int Port { get; set; } = 3;
        //public static int Baud { get; set; } = 19200;
        public static Result Open(int comPort, int baud)
        {
            try
            {
                SerialPort.PortName = "COM" + comPort;
                SerialPort.BaudRate = baud;
                SerialPort.Open();
                return Result.Success();
            }
            catch (Exception ex)
            {
                Logger.Device.Error($"[{DeviceName}]连接异常，异常内容:{ex.Message}");
                return Result.Fail("射频是写模块连接失败");
            }
        }

        public static void Close()
        {
            SerialPort.Close();
        }

        public static Res Send(Req req)
        {
            SerialPort.DiscardInBuffer();

            var send = req.GetBytes();
            SerialPort.Write(send, 0, send.Length);
            var buffer = new byte[32];
            Console.WriteLine(SerialPort.BytesToRead);
            buffer[0] = (byte) SerialPort.ReadByte();
            if (0xFE != buffer[0])
            {
                SerialPort.DiscardInBuffer();
                return new Res {Success = false, ErrorMsg = $"硬件返回的状态不正确，状态值为:0X{buffer[0]:X}"};
            }
            buffer[1] = (byte) SerialPort.ReadByte();
            for (var i = 0; i < buffer[1]; i++)
                buffer[i + 2] = (byte) SerialPort.ReadByte();
            var res = Res.Parse(buffer);
            if (!res.Success)
                return res;
            if (res.Cmd != req.Cmd)
                return new Res {Success = false, ErrorMsg = $"请求状态码0X{req.Cmd:X2}与相应状态码0x{res.Cmd:X2}不匹配！"};
            return res;
        }

        public static bool SetLow()
        {
            var req = new Req
            {
                Cmd = 0x01
            };
            var res = Send(req);
            return res.Success;
        }

        public static bool SetOn(bool ant, bool auto)
        {
            var data = (byte) (ant ? 1 : 0);
            if (auto)
                data += 2;
            var req = new Req
            {
                Cmd = 0x02,
                Data = new[] {data}
            };
            var res = Send(req);
            return res.Success;
        }

        public static bool SetScan()
        {
            var req = new Req
            {
                Cmd = 0x0C
            };
            var res = Send(req);
            return res.Success;
        }

        public static Result<byte[]> Find()
        {
            var req = new Req
            {
                Cmd = 0x03
            };
            var res = Send(req);
            if (res.Success)
                return Result<byte[]>.Success(res.Data);
            Logger.Device.Error($"[{DeviceName}]寻卡失败，错误内容：{res.ErrorMsg}");
            return Result<byte[]>.Fail(res.ErrorMsg);
        }

        public static byte[] Read(byte sector, byte chunk, byte key, byte[] keyValue)
        {
            var date = GetData(sector, chunk, key, keyValue, null);

            var req = new Req
            {
                Data = date,
                Cmd = 0x04
            };
            var res = Send(req);

            if (res.Success)
                return res.Data;
            return null;
        }

        public static bool Write(byte sector, byte chunk, byte key, byte[] keyValue, byte[] data)
        {
            var date = GetData(sector, chunk, key, keyValue, data);

            var req = new Req
            {
                Data = date,
                Cmd = 0x05
            };
            var res = Send(req);

            if (res.Success)
                return true;
            return false;
        }

        public static byte[] GetData(byte sector, byte chunk, byte key, byte[] keyValue, byte[] data)
        {
            var start = 2;
            var len = data?.Length ?? 0;
            len = 1 + 1 + keyValue.Length + len;

            chunk = (byte) (chunk + sector * 4);
            var newData = new byte[len];
            newData[0] = key;
            newData[1] = chunk;

            Array.Copy(keyValue, 0, newData, start, keyValue.Length);

            start = start + keyValue.Length;
            if (data != null)
                Array.Copy(data, 0, newData, start, data.Length);
            return newData;
        }

        public static byte Check(byte[] data, int start, int len)
        {
            var check = data[start];
            for (var i = 1; i < len; i++)
                check = (byte) (check ^ data[start + i]);
            return check;
        }

        public class Req
        {
            public byte Cmd { get; set; }
            public byte[] Data { get; set; }

            public byte[] GetBytes()
            {
                var n = 0;
                if (Data != null)
                    n = Data.Length;

                var len = 2 + 1 + 1 + n + 1;
                var bytes = new byte[len];

                bytes[2] = (byte) (1 + 1 + n);
                bytes[3] = Cmd;

                if (n > 0)
                    Array.Copy(Data, 0, bytes, 4, n);

                bytes[len - 1] = Check(bytes, 2, 1 + 1 + n);

                return bytes;
            }
        }

        public class Res
        {
            public bool Success { get; set; }
            public byte Cmd { get; set; }
            public byte[] Data { get; set; }
            public string ErrorMsg { get; set; }

            public static Res Parse(byte[] bytes)
            {
                int len = bytes[1];
                var check = Check(bytes, 1, len);
                if (bytes[1 + len] != check)
                    return new Res
                    {
                        Success = false,
                        ErrorMsg = $"数据校验失败，当前数据:{BitConverter.ToString(bytes)}，当前校验值:{bytes[len + 1]}，计算校验值:{check}"
                    };
                var res = new Res
                {
                    Success = true,
                    Cmd = bytes[2]
                };
                if (len > 2)
                {
                    var data = new byte[len - 2];
                    Array.Copy(bytes, 3, data, 0, len - 2);
                    res.Data = data;
                }
                return res;
            }
        }
    }
}
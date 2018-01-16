using Microsoft.Practices.Unity;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YuanTu.Consts.FrameworkBase;
using YuanTu.Consts.Services;
using YuanTu.Core.Log;
using YuanTu.Core.Reporter;

namespace YuanTu.Default.House.Device.SpO2
{
    public interface ISpO2Service : IDeviceService
    {
        bool MeasureFinished { get; set; }
    }

    internal class SpO2Service : ISpO2Service
    {
        private Action<SpO2Data> _action;

        private 血氧 spo2;

        [Dependency]
        public ISpO2Model SpO2Model { get; set; }

        [Dependency]
        public IConfigurationManager ConfigurationManager { get; set; }

        public int Port { get; set; }
        public List<SpO2Data> Data { get; private set; }
        public bool MeasureFinished { get; set; }

        public Result StartMeasure()
        {
            try
            {
                Port = ConfigurationManager.GetValueInt("SpO2:Port");
                MeasureFinished = false;
                Data = new List<SpO2Data>();

                spo2 = new 血氧(Port);
                spo2.DataReceived += res =>
                {
                    var q = res as SpO2Data;
                    if (q.SpO2 == 0)
                        return;
                    Data.Add(q);
                    _action?.Invoke(q);
                    if (Data.Count < 10)
                        return;
                    spo2.Stop();

                    SpO2Model.SpO2 = Convert.ToDecimal(Data.Average(d => d.SpO2));
                    SpO2Model.PR = Convert.ToDecimal(Data.Average(d => d.PR));
                    SpO2Model.PI = Convert.ToDecimal(Data.Average(d => d.PI));
                    SpO2Model.参考结果 = GetReferenceResult(SpO2Model.SpO2).Value;
                    MeasureFinished = true;
                };
                spo2.Run();

                return Result.Success();
            }
            catch (Exception ex)
            {
                ReportService.健康小屋血氧仪离线("连接设备失败", "检查设备连接");
                Logger.Device.Error($"{ex.Message} {ex.StackTrace}");
                return Result.Fail($"测量失败，原因：{ex.Message}");
            }
        }

        public Result<string> GetReferenceResult(decimal 血氧饱和度)
        {
            if (血氧饱和度 > 94)
                return Result<string>.Success("正常");
            if (血氧饱和度 < 70)
                return Result<string>.Success("测量无效");
            return Result<string>.Success("低氧");
        }

        public string ServiceName { get; }

        public Result StopMeasure()
        {
            spo2.Stop();
            return Result.Success();
        }

        public void SetUpdate(Action<SpO2Data> action)
        {
            _action = action;
        }
    }

    internal class 血氧
    {
        private readonly SerialPort _port;

        public 血氧(int port)
        {
            _port = new SerialPort($"COM{port}")
            {
                BaudRate = 38400,
                DataBits = 8,
                Parity = Parity.None,
                StopBits = StopBits.One
            };
        }

        public bool Running { get; private set; }

        public void SendCmd(Req cmd)
        {
            if (!_port.IsOpen)
                return;
            var req = cmd.GetBytes();
            _port.Write(req, 0, req.Length);
        }

        public event Action<IRes> DataReceived;

        public void Run()
        {
            Task.Run(() =>
            {
                if (_port.IsOpen)
                    return;
                _port.Open();
                Running = true;
                while (Running)
                    try
                    {
                        var res = Res.Read(_port.BaseStream);
                        DataReceived?.Invoke(res.Parse());
                    }
                    catch
                    {
                        //
                    }
            });

            //Task.Run(() =>
            //{
            //    while (Running)
            //    {
            //        Thread.Sleep(10);
            //    }
            //    _port.Close();
            //});
        }

        public void Stop()
        {
            Running = false;
            if (_port.IsOpen)
                _port.Close();
        }
    }

    #region Req

    internal abstract class Req
    {
        protected abstract byte Token { get; }

        protected abstract byte Type { get; }

        protected abstract byte[] Data { get; }

        public byte[] GetBytes()
        {
            var length = Data.Length;
            var bytes = new byte[6 + length];
            bytes[0] = 0xAA;
            bytes[1] = 0x55;
            bytes[2] = Token;
            bytes[3] = (byte)(length + 2);
            bytes[4] = Type;
            Array.Copy(Data, 0, bytes, 5, length);
            bytes[5 + length] = CRC8.CRC(bytes, 0, 5 + length);
            return bytes;
        }
    }

    internal class ReqQueryId : Req
    {
        protected override byte Token => 0xFF;
        protected override byte Type => 0x01;
        protected override byte[] Data => new byte[0];

        public override string ToString()
        {
            return "QueryId";
        }
    }

    internal class ReqQueryVer : Req
    {
        protected override byte Token => 0x51;
        protected override byte Type => 0x01;
        protected override byte[] Data => new byte[0];

        public override string ToString()
        {
            return "QueryVer";
        }
    }

    internal class ReqQueryStatus : Req
    {
        protected override byte Token => 0x51;
        protected override byte Type => 0x02;
        protected override byte[] Data => new byte[0];

        public override string ToString()
        {
            return "QueryStatus";
        }
    }

    internal class ReqSetMode : Req
    {
        public ReqSetMode(byte mode = 0x00)
        {
            Mode = mode;
        }

        protected override byte Token => 0x50;
        protected override byte Type => 0x01;
        protected override byte[] Data => new[] { Mode };

        public byte Mode { get; set; }

        public override string ToString()
        {
            return $"SetMode:{Mode:X2}";
        }
    }

    internal class ReqSetSend : Req
    {
        public ReqSetSend(byte mode = 0x01)
        {
            Mode = mode;
        }

        protected override byte Token => 0x50;
        protected override byte Type => 0x02;
        protected override byte[] Data => new[] { Mode };

        public byte Mode { get; set; }

        public override string ToString()
        {
            return $"SetSend:{Mode:X2}";
        }
    }

    internal class ReqSetSleep : Req
    {
        public ReqSetSleep(bool sleep = true)
        {
            Sleep = sleep;
        }

        protected override byte Token => 0x50;
        protected override byte Type => 0x03;
        protected override byte[] Data => Sleep ? new byte[0] : new byte[10];

        public bool Sleep { get; set; }

        public override string ToString()
        {
            return $"SetSleep:{Sleep}";
        }
    }

    #endregion Req

    #region Res

    internal class Res
    {
        public byte Token { get; private set; }

        public byte Type { get; private set; }

        public byte[] Data { get; private set; }

        public bool Check { get; private set; }

        public static Res Read(Stream s)
        {
            if (s.ReadByte() != 0xAA || s.ReadByte() != 0x55)
                return null;
            var res = new Res();
            res.Token = (byte)s.ReadByte();
            var len = s.ReadByte();
            var data = new byte[len + 4];
            data[0] = 0xAA;
            data[1] = 0x55;
            data[2] = res.Token;
            data[3] = (byte)len;

            var n = 0;
            while (n < len)
                n += s.Read(data, 4 + n, len - n);
            res.Type = data[4];
            res.Data = new byte[len - 2];
            if (len > 2)
                Array.Copy(data, 5, res.Data, 0, len - 2);
            res.Check = data[len + 3] == CRC8.CRC(data, 0, len + 3);
            Logger.Device.Debug($"[血氧] {BitConverter.ToString(data)}");
            return res;
        }

        public IRes Parse()
        {
            switch (Token * 0x100 + Type)
            {
                case 0x5001:
                    switch (Data[0])
                    {
                        case 0x00:
                            return new ResSetMode
                            {
                                Mode = "成人"
                            };

                        case 0x01:
                            return new ResSetMode
                            {
                                Mode = "儿童"
                            };

                        case 0x02:
                            return new ResSetMode
                            {
                                Mode = "动物"
                            };
                    }
                    break;

                case 0x5002:
                    return new ResSetSend();

                case 0x5003:
                    return new ResSetSleep();

                case 0x5101:
                    return new ResQueryVersion { Version = BitConverter.ToString(Data) };

                case 0x5102:
                    var n = Data[0];
                    string mode;
                    switch (n >> 6)
                    {
                        case 0:
                            mode = "成人";
                            break;

                        case 1:
                            mode = "儿童";
                            break;

                        case 2:
                            mode = "动物";
                            break;

                        case 3:
                            mode = "预留";
                            break;

                        default:
                            mode = string.Empty;
                            break;
                    }
                    return new ResQueryStatus
                    {
                        Mode = mode,
                        ActiveSend = (n & 0x20) > 0,
                        ProbeConnected = (n & 0x10) > 0,
                        ProbeOff = (n & 0x8) > 0,
                        CheckProbe = (n & 0x4) > 0
                    };

                case 0x5201:
                    return new ResUniformedData
                    {
                        Data = Data.Select(UniformedData.Parse).ToArray()
                    };

                case 0x5202:
                    break;

                case 0x5301:
                    return new SpO2Data
                    {
                        SpO2 = Data[0],
                        PR = BitConverter.ToInt16(Data, 1),
                        PI = Data[3],
                        State = Data[4]
                    };

                case 0xFF01:
                    return new ResQueryId
                    {
                        Id = Encoding.Default.GetString(Data)
                    };
            }
            return new ResUnknown
            {
                Res = this
            };
        }

        public override string ToString()
        {
            return $"{Token:X2} {Type:X2} {BitConverter.ToString(Data)}";
        }
    }

    internal interface IRes
    {
    }

    internal class ResUnknown : IRes
    {
        public Res Res { get; set; }
    }

    internal class ResQueryId : IRes
    {
        public string Id { get; set; }
    }

    internal class ResQueryVersion : IRes
    {
        public string Version { get; set; }
    }

    internal class ResSetMode : IRes
    {
        public string Mode { get; set; }
    }

    internal class SpO2Data : IRes
    {
        public byte SpO2 { get; set; }

        public short PR { get; set; }

        public byte PI { get; set; }
        public byte State { get; set; }
    }

    internal class ResQueryStatus : IRes
    {
        public string Mode { get; set; }

        public bool ActiveSend { get; set; }

        public bool ProbeConnected { get; set; }

        public bool ProbeOff { get; set; }

        public bool CheckProbe { get; set; }
    }

    internal class ResUniformedData : IRes
    {
        public UniformedData[] Data { get; set; }
    }

    internal class UniformedData
    {
        public bool Pulse { get; set; }

        public byte Value { get; set; }

        public override string ToString()
        {
            return $"{(Pulse ? 1 : 0)}+{Value}";
        }

        public static UniformedData Parse(byte b)
        {
            return new UniformedData
            {
                Pulse = b >= 128,
                Value = (byte)(b % 128)
            };
        }
    }

    internal class ResSetSleep : IRes
    {
    }

    internal class ResSetSend : IRes
    {
    }

    #endregion Res

    internal class CRC8
    {
        /// <summary>
        ///     CRC8位校验表
        /// </summary>
        private static readonly byte[] CRC8Table =
        {
            0, 94, 188, 226, 97, 63, 221, 131, 194, 156, 126, 32, 163, 253, 31, 65,
            157, 195, 33, 127, 252, 162, 64, 30, 95, 1, 227, 189, 62, 96, 130, 220,
            35, 125, 159, 193, 66, 28, 254, 160, 225, 191, 93, 3, 128, 222, 60, 98,
            190, 224, 2, 92, 223, 129, 99, 61, 124, 34, 192, 158, 29, 67, 161, 255,
            70, 24, 250, 164, 39, 121, 155, 197, 132, 218, 56, 102, 229, 187, 89, 7,
            219, 133, 103, 57, 186, 228, 6, 88, 25, 71, 165, 251, 120, 38, 196, 154,
            101, 59, 217, 135, 4, 90, 184, 230, 167, 249, 27, 69, 198, 152, 122, 36,
            248, 166, 68, 26, 153, 199, 37, 123, 58, 100, 134, 216, 91, 5, 231, 185,
            140, 210, 48, 110, 237, 179, 81, 15, 78, 16, 242, 172, 47, 113, 147, 205,
            17, 79, 173, 243, 112, 46, 204, 146, 211, 141, 111, 49, 178, 236, 14, 80,
            175, 241, 19, 77, 206, 144, 114, 44, 109, 51, 209, 143, 12, 82, 176, 238,
            50, 108, 142, 208, 83, 13, 239, 177, 240, 174, 76, 18, 145, 207, 45, 115,
            202, 148, 118, 40, 171, 245, 23, 73, 8, 86, 180, 234, 105, 55, 213, 139,
            87, 9, 235, 181, 54, 104, 138, 212, 149, 203, 41, 119, 244, 170, 72, 22,
            233, 183, 85, 11, 136, 214, 52, 106, 43, 117, 151, 201, 74, 20, 246, 168,
            116, 42, 200, 150, 21, 75, 169, 247, 182, 232, 10, 84, 215, 137, 107, 53
        };

        public static byte CRC(byte[] buffer)
        {
            return CRC(buffer, 0, buffer.Length);
        }

        public static byte CRC(byte[] buffer, int off, int len)
        {
            byte crc = 0;
            if (buffer == null)
                throw new ArgumentNullException(nameof(buffer));
            if (off < 0 || len < 0 || off + len > buffer.Length)
                throw new ArgumentOutOfRangeException();

            for (var i = off; i < len; i++)
                crc = CRC8Table[crc ^ buffer[i]];
            return crc;
        }
    }
}
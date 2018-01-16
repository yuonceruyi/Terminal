using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using YuanTu.Consts.Enums;
using YuanTu.Consts.FrameworkBase;
using YuanTu.Consts.Services;
using YuanTu.Core.Extension;
using YuanTu.Core.Log;

namespace YuanTu.Devices.CardReader
{
    public interface IMagCardReader : IDevice
    {
        /// <summary>
        ///     获取卡在设备上的位置
        /// </summary>
        /// <returns></returns>
        Result<CardPos> GetCardPosition();

        /// <summary>
        /// 移动卡位置
        /// </summary>
        /// <param name="pos"></param>
        /// <returns></returns>
        Result MoveCard(CardPos pos);

        /// <summary>
        ///     读取磁条卡内容
        /// </summary>
        /// <param name="road">读取的磁道</param>
        /// <param name="type">解码类型</param>
        /// <returns></returns>
        Result<Dictionary<TrackRoad, string>> ReadTrackInfos(TrackRoad road, ReadType type);

        /// <summary>
        ///     读取指定磁道的信息
        /// </summary>
        /// <param name="track">磁道 1 2 3 4</param>
        /// <returns></returns>
        Result<string> ReadTrackInfos(int track);
        /// <summary>
        /// 手动设置handle 用作一个读卡器同时读两种卡时 共用一个handle
        /// </summary>
        /// <returns></returns>
        Result SetHandle(int handle);
        /// <summary>
        /// 手动获取handle 用作一个读卡器同时读两种卡时 共用一个handle
        /// </summary>
        /// <returns></returns>
        Result<int> GetHandle();
    }

    public class A6MagCardReader : IMagCardReader, IIcCardReader
    {
        private readonly IConfigurationManager _configurationManager;
        private int _baud;
        private DeviceStatus _currentStatus = DeviceStatus.UnInitialized;
        private int _handle;
        private bool _isConnectd;
        private int _port;

        public A6MagCardReader(IConfigurationManager configurationManager)
        {
            _configurationManager = configurationManager;
        }

        public string DeviceName => "ACT_A6";
        public string DeviceId => DeviceName + "_Mag&IC";

        public Result<DeviceStatus> GetDeviceStatus()
        {
            return Result<DeviceStatus>.Success(_currentStatus);
        }

        public Result Connect()
        {
            if (_isConnectd)
            {
                return Result.Success();
            }
            _port = _configurationManager.GetValueInt("Act_A6:Port");
            _baud = _configurationManager.GetValueInt("Act_A6:Baud");
            var ret = UnSafeMethods.A6_Connect(_port, _baud, ref _handle);
            if (ret != 0)
            {
                Logger.Device.Error($"[读卡器{DeviceId}]连接异常，返回码:{ret} 端口:{_port} 波特率:{_baud}");
                return Result.Fail("读卡器连接失败");
            }
            _isConnectd = true;
            _currentStatus = DeviceStatus.Connected;
            Logger.Device.Info($"[读卡器{DeviceId}]连接成功，端口:{_port} 波特率:{_baud}");
            return Result.Success();
        }

        public Result Initialize()
        {
            if (!_isConnectd)
            {
                return Result.Fail("读卡器未连接");
            }
            //1.初始化读卡器
            var bufferLen = 1024;
            var buffer = new byte[bufferLen];
            var ret = UnSafeMethods.A6_Initialize(_handle, 0x30, buffer, ref bufferLen);
            if (ret != 0)
            {
                Logger.Device.Error($"[读卡器{DeviceName}]初始化异常，返回码:{ret} 端口:{_port} 波特率:{_baud} 句柄:{_handle}");
                return Result.Fail("读卡器初始化失败");
            }
            //2.设置允许允许前门进卡，
            ret = UnSafeMethods.A6_SetCardIn(_handle, 0x33, 0x31);
            if (ret != 0)
            {
                Logger.Device.Error($"[读卡器{DeviceName}]进卡设置失败，返回码:{ret} 端口:{_port} 波特率:{_baud} 句柄:{_handle}");
                return Result.Fail("读卡器初始化失败");
            }

            UnSafeMethods.A6_LedOn(_handle);
            Logger.Device.Info($"[读卡器{DeviceId}]初始化成功");
            return Result.Success();
        }

        public Result UnInitialize()
        {
            if (!_isConnectd)
            {
                Logger.Device.Error($"[读卡器{DeviceName}]_isConnectd={_isConnectd}，不能反初始化");
                return Result.Fail("读卡器未连接");
            }
            UnSafeMethods.A6_LedOff(_handle);
            //强制退卡
            var ret = UnSafeMethods.A6_MoveCard(_handle, 0x30);
            if (ret != 0)
            {
                Logger.Device.Error($"[读卡器{DeviceName}]退卡失败，返回码:{ret}");
                return Result.Fail("退卡失败！");
            }
            Logger.Device.Info($"[读卡器{DeviceName}]退卡成功，返回码:{ret}");
            //设置不允许进卡
            ret = UnSafeMethods.A6_SetCardIn(_handle, 0x31, 0x31);
            if (ret != 0)
            {
                Logger.Device.Error($"[读卡器{DeviceName}]禁止入卡失败，返回码:{ret}");
                return Result.Fail("禁卡失败！");
            }
            Logger.Device.Info($"[读卡器{DeviceName}]禁止入卡成功，返回码:{ret}");
            return Result.Success();
        }

        public Result DisConnect()
        {
            if (!_isConnectd || _handle == 0)
            {
                _isConnectd = false;
                _currentStatus = DeviceStatus.Disconnect;
                return Result.Success();
            }
            //设置不允许进卡
            var ret = UnSafeMethods.A6_SetCardIn(_handle, 0x31, 0x31);
            if (ret != 0)
            {
                Logger.Device.Error($"[读卡器{DeviceName}]禁止入卡失败，返回码:{ret}");
                return Result.Fail("禁卡失败！");
            }
            Logger.Device.Info($"[读卡器{DeviceName}]禁止入卡成功，返回码:{ret}");
            UnSafeMethods.A6_LedOff(_handle);
            ret = UnSafeMethods.A6_Disconnect(_handle);
            if (ret != 0)
            {
                _currentStatus = _currentStatus | DeviceStatus.Error;
                Logger.Device.Error($"[读卡器{DeviceName}]连接断开异常，返回码:{ret} 端口:{_port} 波特率:{_baud} 句柄:{_handle}");
                return Result.Fail("读卡器断开失败");
            }
            Logger.Device.Info($"[读卡器{DeviceName}]连接断开成功，返回码:{ret} 端口:{_port} 波特率:{_baud} 句柄:{_handle}");
            _isConnectd = false;
            _currentStatus = DeviceStatus.Disconnect;
            return Result.Success();
        }

        public Result PowerOn(SlotNo slotNo)
        {
            var ret = UnSafeMethods.A6_IccPowerOn(_handle);
            if (ret != 0)
            {
                Logger.Device.Error($"[读卡器{DeviceName}]上电异常，返回码:{ret}");
                return Result.Fail("IC卡上电失败，请确认插入的方向！");
            }
            Logger.Device.Info($"[读卡器{DeviceName}]上电成功");
            return Result.Success();
        }

        public Result<byte[]> CPUCodeReset()
        {
            var data = new byte[256];
            var len = 256;
            var ret = UnSafeMethods.A6_CpuColdReset(_handle, data, ref len);
            if (ret != 0)
            {
                Logger.Device.Error($"[读卡器{DeviceName}]IC卡冷复位失败，返回码:{ret}");
                return Result<byte[]>.Fail( "IC卡冷复位失败！");
            }
            var apdu = new byte[len];
            Array.Copy(data, apdu, len);
            return Result<byte[]>.Success(apdu);
        }

        public Result<byte[]> CPUChipIO(bool t0, byte[] input)
        {
            var recv = new byte[1024];
            var recvLen = 1024;
            var protocol = t0 ? ICCardProtocol.T0 : ICCardProtocol.T1;
            var ret = UnSafeMethods.A6_CpuTransmit(_handle, (byte)protocol, input, input.Length, recv, ref recvLen);
            if (ret != 0)
            {
                Logger.Device.Error($"[读卡器{DeviceName}]IC卡通信失败，返回码:{ret}");
                return Result<byte[]>.Fail( "IC卡通信失败！");
            }
            var data = new byte[recvLen];
            Array.Copy(recv,data,recvLen);
            return Result<byte[]>.Success(data);
        }

        public Result<CardPos> GetCardPosition()
        {
            var status = new byte[3];
            var ret = UnSafeMethods.A6_GetCRCondition(_handle, status);
            if (ret != 0)
            {
                Logger.Device.Error($"[读卡器{DeviceName}]卡位置检测异常，返回码:{ret}");
                return Result<CardPos>.Fail( "磁条读卡失败，请确认插入的方向！");
            }
            Logger.Device.Info($"[读卡器{DeviceName}]卡位置检测，返回值:{status[0]}");
            return Result<CardPos>.Success((CardPos) status[0]);
        }

        public Result MoveCard(CardPos pos)
        {
            CardMovePos p;
            switch (pos)
            {
                case CardPos.不持卡位:
                    p = CardMovePos.前端不持卡;
                    break;
                case CardPos.持卡位:
                    p = CardMovePos.前端持卡;
                    break;
                case CardPos.停卡位:
                    p = CardMovePos.非接;
                    break;
                case CardPos.IC位:
                    p = CardMovePos.IC;
                    break;
                case CardPos.后端持卡位:
                    p = CardMovePos.后端持卡;
                    break;
                case CardPos.后端不持卡位:
                    p = CardMovePos.后端不持卡;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(pos), pos, null);
            }
            var ret = UnSafeMethods.A6_MoveCard(_handle, (int)p);
            if (ret != 0)
            {
                Logger.Device.Error($"[读卡器{DeviceName}]移动卡位置异常，返回码:{ret}");
                return Result.Fail( "移动卡失败！");
            }
            return Result.Success();
        }

        public Result<Dictionary<TrackRoad, string>> ReadTrackInfos(TrackRoad road, ReadType type)
        {
            _currentStatus = _currentStatus | DeviceStatus.Busy;
            var t = (uint) road;
            var trackInfo = new TrackInfo();
            var ret = UnSafeMethods.A6_ReadTracks(_handle, (byte) type, t, ref trackInfo);
            if (ret != 0)
            {
                Logger.Device.Error($"[读卡器{DeviceName}]读卡异常，返回码:{ret}");
                _currentStatus = ~DeviceStatus.Busy & _currentStatus;
                return Result<Dictionary<TrackRoad, string>>.Fail( "磁条读卡失败，请确认插入的方向！");
            }
            var tps = road.GetEnums();
            var dict = new Dictionary<TrackRoad, string>();
            foreach (var trackRoad in tps)
            {
                var roadIndex = 0;
                var valus = Enum.GetValues(typeof(TrackRoad));
                for (var i = 0; i < valus.Length; i++)
                {
                    if (trackRoad == (TrackRoad) valus.GetValue(i))
                    {
                        roadIndex = i;
                        break;
                    }
                }
                switch (type)
                {
                    case ReadType.ASCII:
                        dict[trackRoad] = Encoding.ASCII.GetString(trackInfo.Contents, roadIndex*512,
                            trackInfo.Lengths[roadIndex]);
                        break;

                    case ReadType.BINARY:
                        dict[trackRoad] = BitConverter.ToString(trackInfo.Contents, roadIndex*512,
                            trackInfo.Lengths[roadIndex]);
                        break;

                    default:
                        throw new ArgumentOutOfRangeException(nameof(type), type, null);
                }
            }
            _currentStatus = ~DeviceStatus.Busy & _currentStatus;
            var kvs = dict.Select(p => $"{p.Key}:{p.Value}");
            Logger.Device.Info($"[读卡器{DeviceId}]成功获取卡片磁条信息:{string.Join(";", kvs)}");
            return Result<Dictionary<TrackRoad, string>>.Success(dict);
        }

       

        public Result<string> ReadTrackInfos(int track)
        {
            TrackRoad tr;
            switch (track)
            {
                case 1:tr= TrackRoad.Trace1;
                    break;
                case 2:tr = TrackRoad.Trace2;
                    break;
                case 3:tr = TrackRoad.Trace3;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(track), track, null);
            }
            var res = ReadTrackInfos(tr, ReadType.ASCII);
            if (!res.IsSuccess)
                return Result<string>.Fail(res.ResultCode, res.Message, res.Exception);

            switch (track)
            {
                case 1:
                    return Result<string>.Success(res.Value[TrackRoad.Trace1]);
                case 2:
                    return Result<string>.Success(res.Value[TrackRoad.Trace2]);
                case 3:
                    return Result<string>.Success(res.Value[TrackRoad.Trace2]);
                default:
                    throw new ArgumentOutOfRangeException(nameof(track), track, null);
            }
        }

        public Result SetHandle(int handle)
        {
            throw new NotImplementedException();
        }

        public Result<int> GetHandle()
        {
            throw new NotImplementedException();
        }
    }

    public class HuaDaMagCardReader : IMagCardReader
    {
        private readonly string _deviceName = "USB1";

        private int _handle;
        private int _ret;
        public string DeviceName => "HuaDa";
        public string DeviceId => DeviceName + "_Mag";

        public Result<DeviceStatus> GetDeviceStatus()
        {
            throw new NotImplementedException();
        }

        public Result Connect()
        {
            _handle = UnSafeMethods.ICC_Reader_Open(_deviceName);
            if (_handle > 0) return Result.Success();
            Logger.Device.Error($"[华大读卡器{DeviceId}]连接异常，接口返回值:{_handle}");
            return Result.Fail("华大读卡器连接异常");
        }

        public Result Initialize()
        {
            _ret = UnSafeMethods.ICC_PosBeep(_handle, 30);
            if (_ret >= 0) return Result.Success();
            Logger.Device.Error($"[华大读卡器{DeviceId}]初始化（蜂鸣）异常，接口返回值:{_ret}");
            return Result.Fail("华大读卡器初始化（蜂鸣）异常");
        }

        public Result UnInitialize()
        {
            return Result.Success();
        }

        public Result DisConnect()
        {
            _ret = UnSafeMethods.ICC_Reader_Close(_handle);
            if (_ret == 0) return Result.Success();
            Logger.Device.Error($"[华大读卡器{DeviceId}]断开连接异常，接口返回值:{_ret}");
            return Result.Fail("华大读卡器断开连接异常");
        }

        public Result<CardPos> GetCardPosition()
        {
            _ret = UnSafeMethods.PICC_Reader_Request(_handle);
            return _ret == 0
                ? Result<CardPos>.Success(CardPos.停卡位)
                : Result<CardPos>.Fail("华大读卡器请求卡片失败");
        }

        public Result MoveCard(CardPos pos)
        {
            return Result.Success();
        }

        public Result<Dictionary<TrackRoad, string>> ReadTrackInfos(TrackRoad road, ReadType type)
        {
            throw new NotImplementedException();
        }

        public Result<string> ReadTrackInfos(int track)
        {
            byte rlen = 0;
            var data = new StringBuilder(256);
            _ret = UnSafeMethods.Rcard(_handle, 3, track, ref rlen, data);
            if (_ret == 0)
            {
                Logger.Device.Info($"[华大读卡器{DeviceId}]成功获取卡片磁条信息:{data}");
                return Result<string>.Success(data.ToString(0,rlen));
            }
            Logger.Device.Error($"[华大读卡器{DeviceName}]读卡异常，返回码:{_ret}");
            return Result<string>.Fail("磁条读卡失败，请确认插入的方向！");
        }

        public Result SetHandle(int handle)
        {
            throw new NotImplementedException();
        }

        public Result<int> GetHandle()
        {
            throw new NotImplementedException();
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YuanTu.Consts.Enums;
using YuanTu.Consts.FrameworkBase;
using YuanTu.Consts.Models.Print;
using YuanTu.Consts.Services;
using YuanTu.Core.Log;
using YuanTu.Devices;
using YuanTu.Devices.CardReader;

namespace YuanTu.ShengZhouZhongYiHospital.CardDispenser
{
    public class ZbrCardDispenser : IRFCpuCardDispenser
    {
        private static IntPtr _hdc;
        private IntPtr _handle;
        private int _prnType;
        private bool _isConnectd;
        private int _err;
        private int _icdev;
        private readonly IConfigurationManager _configurationManager;
        private string _deviceName;

        public string DeviceName { get; } = "ZBR";
        public string DeviceId => DeviceName + "_CPU";

        public ZbrCardDispenser(IConfigurationManager configurationManager)
        {
            _configurationManager = configurationManager;
            _deviceName = _configurationManager.GetValue("Printer:ZBRPrinter");
        }

        public Result<DeviceStatus> GetDeviceStatus()
        {
            throw new NotImplementedException();
        }

        public Result Connect()
        {
            if (_isConnectd)
            {
                return Result.Success();
            }
           

            int err;
            var ret = UnSafeMethods.ZBRGetHandle(out _handle,
                Encoding.GetEncoding("GB2312").GetBytes(_deviceName).ToArray(), out _prnType, out err);
            if (ret == 1)
            {
                _isConnectd = true;
                return Result.Success();
            }
            Logger.Device.Error($"[发卡器{DeviceId}]连接异常，打印机名称:{_deviceName}");
            return Result.Fail("发卡器连接失败");
        }

        public Result Initialize()
        {
            if (!_isConnectd)
            {
                return Result.Fail("发卡器未连接");
            }
            return Result.Success();
        }

        public Result UnInitialize()
        {
            return Result.Success();
        }

        public Result DisConnect()
        {
            if (!_isConnectd)
            {
                return Result.Fail("发卡器未连接");
            }
            _isConnectd = false;
            int err;
            UnSafeMethods.ZBRCloseHandle(_handle, out err);
            return Result.Success();
        }

        public Result<byte[]> EnterCard()
        {
            var ret = UnSafeMethods.ZBRPRNStartSmartCard(_handle, _prnType, 1, out _err);
            if (ret == 1)
            {
                Logger.Device.Info($"移动卡片成功: _handle{_handle} _prnType{_prnType}");
                return GetCardId();
            }
            return Result<byte[]>.Fail($"移动卡片失败 错误代码：{_err}");
        }

        public Result<byte[]> GetCardId()
        {
            _icdev = DllHandler.IC_InitComm(100);
            if (_icdev > 0)
            {
                DllHandler.IC_DevBeep(_icdev, 10);
                var res = DllHandler.IC_Status(_icdev);
                if (res == 0)
                {
                    res = DllHandler.IC_InitType(_icdev, 4);
                    if (res == 0)
                    {
                        var cardData = new byte[70];
                        res = DllHandler.IC_ReadWithProtection(_icdev, 100, cardData.Length, cardData);
                        if (res == 0)
                        {
                            return Result<byte[]>.Success(cardData);
                        }
                        Logger.Device.Info($"发卡机德卡读数据失败");
                    }
                    else
                    {
                        Logger.Device.Info($"发卡机德卡IC_InitType失败");
                    }
                }
                else
                {
                    Logger.Device.Info($"发卡机德卡上电失败");
                }
            }
            else
            {
                Logger.Device.Info($"发卡机德卡连接失败");
            }
            return Result<byte[]>.Fail("获取卡号失败");
        }

        public Result VerifyPwd()
        {
            var ret = DllHandler.IC_CheckPass_4442hex(_icdev, "ffffff");
            return ret == 0 ? Result.Success() : Result.Fail("密码验证失败");
        }

        public Result Write( int i, byte[] bytes)
        {
            var ret = DllHandler.IC_WriteWithProtection(_icdev, i, bytes.Length, bytes);
            if (ret != 0)
            {
                Logger.Device.Info($"写入{Encoding.GetEncoding("GB2312").GetString(bytes)}异常");
                return Result.Fail($"发卡机写入异常");
            }
            return Result.Success();
        }

        public Result<byte[]> CpuTransmit(byte[] order)
        {
            throw new NotImplementedException();
        }

        public Result PrintContent(List<ZbrPrintTextItem> printInfo = null, List<ZbrPrintCodeItem> printCode = null)
        {
            var res = UnSafeMethods.ZBRPRNEndSmartCard(_handle, _prnType, 1, 0, out _err);
            if (res != 1)
            {
                Logger.Device.Info($"发卡机 ZBRPRNEndSmartCard失败,错误码{_err} _handle{_handle} _prnType{_prnType}");
                return Result.Fail($"发卡机 ZBRPRNEndSmartCard失败,错误码{_err}");
            }
            res = UnSafeMethods.ZBRGDIInitGraphics(Encoding.GetEncoding("GB2312").GetBytes(_deviceName).ToArray(),
                out _hdc, out _err);
            if (res != 1)
            {
                Logger.Device.Info($"发卡机 初始化打印模块失败,错误码{_err}");
                return Result.Fail($"发卡机 初始化打印模块失败,错误码{_err}");
            }
            var name = Encoding.Default.GetBytes(printInfo.First().Text);
            var font = Encoding.Default.GetBytes(printInfo.First().Font);
            res = UnSafeMethods.ZBRGDIDrawText(printInfo.First().X, printInfo.First().Y, name, font,
                printInfo.First().FontSize, 1, 0, out _err);
            if (res != 1)
            {
                Logger.Device.Info($"发卡机 打印内容写入失败,错误码{_err}");
                return Result.Fail($"发卡机 打印内容写入失败,错误码{_err}");
            }
            res = UnSafeMethods.ZBRPRNMovePrintReady(_handle, _prnType, out _err);
            if (res != 1)
            {
                Logger.Device.Info($"发卡机 PrintReady失败,错误码{_err}");
                return Result.Fail($"发卡机 PrintReady失败,错误码{_err}");
            }
            res = UnSafeMethods.ZBRGDIPrintGraphics(_hdc, out _err);
            if (res != 1)
            {
                Logger.Device.Info($"发卡机 打印失败,错误码{_err}");
                return Result.Fail($"发卡机 打印失败,错误码{_err}");
            }
            res = UnSafeMethods.ZBRGDICloseGraphics(_hdc, out _err);
            DisConnect();
            return Result.Success();
        }

        public Result EndSmartCard()
        {
            DllHandler.IC_Down(_icdev);
            DllHandler.IC_ExitComm(_icdev);
            var res = UnSafeMethods.ZBRPRNEndSmartCard(_handle, _prnType, 1, 0, out _err);
            if (res == 1)
            {
                return Result.Success();
            }
            Logger.Device.Info($"发卡机：结束IC读取失败，返回码:{res},错误:{_err}");
            return Result.Fail("结束IC读取失败");
        }

        public Result MoveCardOut()
        {
            return Result.Success();
        }
    }
}

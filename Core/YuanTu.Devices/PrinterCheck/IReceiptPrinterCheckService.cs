using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Microsoft.Practices.ServiceLocation;
using System.Text;
using YuanTu.Consts;
using YuanTu.Consts.FrameworkBase;
using YuanTu.Consts.Services;
using YuanTu.Core.Log;
using YuanTu.Devices.PrinterCheck.CePrinterCheck;
using YuanTu.Devices.PrinterCheck.MsPrinterCheck;
using YuanTu.Devices.PrinterCheck;

namespace YuanTu.Devices.PrinterCheck
{
    public interface IReceiptPrinterCheckService : IService
    {
        /// <summary>
        ///     检测打印机
        /// </summary>
        /// <returns></returns>
        Result CheckReceiptPrinter();
        Result OtherCheckPrinter();
    }

    public class ReceiptPrinterCheckService : IReceiptPrinterCheckService
    {
        public string ServiceName { get; } = "热敏打印机检测服务";
        private const string MsPrinterKey = "MS";
        private const string CePrinterKey = "K80";

        public virtual Result CheckReceiptPrinter()
        {
            Logger.Device.Info("开始监控热敏打印机状态");
            if (FrameworkConst.Local)
                return Result.Success();
            var configurationManager = ServiceLocator.Current.GetInstance<IConfigurationManager>();
            var printName = configurationManager.GetValue("Printer:Receipt");
            Logger.Device.Info($"热敏打印机名称{printName}");
            if (printName.Contains(MsPrinterKey))
                return MsCheckPrinter();
            if (printName.Contains(CePrinterKey))
                return CeCheckPrinter();
            return OtherCheckPrinter();
        }

        public virtual Result MsCheckPrinter()
        {
            if (FrameworkConst.Local)
                return Result.Success();
            MsPrinter.PrinterSatusMsg = null;
            var configurationManager = ServiceLocator.Current.GetInstance<IConfigurationManager>();
            var printName = configurationManager.GetValue("Printer:Receipt");
            var sName = new StringBuilder(printName);
            MsPrinter.SetPrintNameUSB(sName);
            MsPrinter.SetPrintModel(4);
            var iStatus = MsPrinter.GetPrintStatusUSB();
            MsPrinter.PrinterSatusMsg = $"凭条打印机状态：{MsPrinter.GetStatusMsg(iStatus)}";
            Logger.Device.Info($"打印机Ms状态:{MsPrinter.GetStatusMsg(iStatus)}");
            if (iStatus != 0)
            {
                //上传状态
                switch (iStatus)
                {
                    case 1:
                    case 100:
                        Core.Reporter.ReportService.凭条打印机离线(MsPrinter.PrinterSatusMsg, null);
                        break;

                    case 10:
                        Core.Reporter.ReportService.凭条打印机纸将尽(MsPrinter.PrinterSatusMsg, null);
                        break;

                    case 7:
                        Core.Reporter.ReportService.凭条打印机纸尽(MsPrinter.PrinterSatusMsg, null);
                        break;

                    default:
                        Core.Reporter.ReportService.凭条打印机其他异常(MsPrinter.PrinterSatusMsg, null);
                        break;
                }
            }
            if (iStatus != 0 && iStatus != 10)
                return Result.Fail(MsPrinter.PrinterSatusMsg);
            return Result.Success();
        }

        public virtual Result CeCheckPrinter()
        {
            if (FrameworkConst.Local)
                return Result.Success();
            var result = CePrinter.CallInitUsb();
            if (!result.IsSuccess)
            {
                return Result.Fail($"{result.Message}");
            }
            var configurationManager = ServiceLocator.Current.GetInstance<IConfigurationManager>();
            var printName = configurationManager.GetValue("Printer:Receipt");
            var res = CePrinter.CallInterfaceNumUsb(printName);
            if (!res.IsSuccess)
            {
                return Result.Fail($"{res.Message}");
            }
            CePrinter._prnDevNum = res.Value;
            var recv = CePrinter.CallPrnGetFullModelUsb(CePrinter._prnDevNum, ref CePrinter._prnModel);
            if (!recv.IsSuccess)
            {
                return Result.Fail($"{recv.Message}");
            }
            CePrinter.CallSetPrinterModelUsb(CePrinter._prnDevNum, CePrinter._prnModel);

            // Get status from printer
            result = CePrinter.CePrnGetStsUsb(CePrinter._prnDevNum);
            if (!result.IsSuccess)
            {
                return Result.Fail($"{result.Message}");
            }
            // Decode printer status to visual information
            var resultStatus = CePrinter.DecodePrintStatus(result.Value);
            if (!resultStatus.IsSuccess)
            {
                return Result.Fail($"{resultStatus.Message}");
            }

            var strStatus = "打印机状态：";
            var printerStatus = resultStatus.Value;
            Logger.Device.Info($"打印机状态:PaperEnd:{printerStatus.PaperEnd}\n" +
                               $"NearpaperEnd:{printerStatus.NearpaperEnd}\n" +
                               $"TicketOut:{printerStatus.TicketOut}\n" +
                               $"PaperJam:{printerStatus.PaperJam}\n" +
                               $"CoverOpen:{printerStatus.CoverOpen}");
            if (printerStatus.PaperEnd)
            {
                strStatus += "纸尽\n";
                Core.Reporter.ReportService.凭条打印机纸尽("纸尽", null);
            }
            else if (printerStatus.NearpaperEnd)
            {
                strStatus += "纸将尽\n";
                Core.Reporter.ReportService.凭条打印机纸将尽("纸将尽", null);
            }
            if (printerStatus.TicketOut)
            {
                strStatus += "出纸口有纸\n";
                Core.Reporter.ReportService.凭条打印机出纸口有纸("出纸口有纸", null);
            }
            if (printerStatus.PaperJam)
            {
                strStatus += "卡纸\n";
                Core.Reporter.ReportService.凭条打印机卡纸("卡纸", null);
            }
            if (printerStatus.CoverOpen)
            {
                strStatus += "胶辊开启\n";
                Core.Reporter.ReportService.凭条打印机胶辊开启("胶辊开启", null);
            }
            //NearpaperEnd 由于打印机检测纸将尽不准确,只上报监控平台,不阻止病人继续操作
            if (!printerStatus.PaperEnd /*&& !printerStatus.NearpaperEnd*/ && !printerStatus.TicketOut && !printerStatus.PaperJam &&
                !printerStatus.CoverOpen)
            {
                return Result.Success();
            }
            return Result.Fail(strStatus);
        }

        public virtual Result WinApiCheckPrinter()
        {
            try
            {
                if (FrameworkConst.Local)
                    return Result.Success();
                var configurationManager = ServiceLocator.Current.GetInstance<IConfigurationManager>();
                var printerName = configurationManager.GetValue("Printer:Receipt");
                IntPtr printer;
                if (!WindowsDriverPrinter.OpenPrinter(printerName, out printer, IntPtr.Zero))
                {
                    var error = Marshal.GetLastWin32Error();
                    if (error == 1801)
                        return Result.Fail($"不存在名为[{printerName}]的打印机");
                    return Result.Fail($"查询打印机状态出错[{printerName}][{error}]");
                }

                uint bytesNeeded = 0;
                WindowsDriverPrinter.GetPrinter(printer, 2, IntPtr.Zero, 0, ref bytesNeeded);
                var dataPtr = Marshal.AllocHGlobal((int)bytesNeeded);
                try
                {
                    if (!WindowsDriverPrinter.GetPrinter(printer, 2, dataPtr, bytesNeeded, ref bytesNeeded))
                    {
                        var error = Marshal.GetLastWin32Error();
                        return Result.Fail($"查询打印机状态出错[{printerName}][{error}]");
                    }
                    var info = Marshal.PtrToStructure<WindowsDriverPrinter.PRINTER_INFO_2>(dataPtr);
                    switch (info.Status)
                    {
                        case 0:
                            return Result.Success();

                        case (int)WindowsDriverPrinter.PrinterStatus.PRINTER_STATUS_PRINTING:
                            return Result.Success();

                        case (int)WindowsDriverPrinter.PrinterStatus.PRINTER_STATUS_PAPER_OUT:
                            return Result.Fail("打印机缺纸");

                        case (int)WindowsDriverPrinter.PrinterStatus.PRINTER_STATUS_DOOR_OPEN:
                            return Result.Fail("机盖未关");

                        case (int)WindowsDriverPrinter.PrinterStatus.PRINTER_STATUS_NOT_AVAILABLE:
                            return Result.Fail("打印机不可用");

                        case (int)WindowsDriverPrinter.PrinterStatus.PRINTER_STATUS_OFFLINE:
                            return Result.Fail("打印机离线");

                        case (int)WindowsDriverPrinter.PrinterStatus.PRINTER_STATUS_PAPER_JAM:
                            return Result.Fail("打印机卡纸");

                        case (int)WindowsDriverPrinter.PrinterStatus.PRINTER_STATUS_USER_INTERVENTION:
                            return Result.Fail("打印机纸将尽");

                        default:
                            return Result.Fail("未知状态");
                    }
                }
                finally
                {
                    Marshal.FreeHGlobal(dataPtr);
                }
            }
            catch (Exception ex)
            {
                return Result.Fail($"查询打印机状态异常{ex.Message}", ex);
            }
        }

        public virtual Result OtherCheckPrinter()
        {
            return Result.Success();
        }
    }
}
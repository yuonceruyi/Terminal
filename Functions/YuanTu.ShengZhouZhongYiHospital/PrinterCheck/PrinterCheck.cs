using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Practices.ServiceLocation;
using YuanTu.Consts;
using YuanTu.Consts.FrameworkBase;
using YuanTu.Consts.Services;
using YuanTu.Core.Log;
using YuanTu.Devices.PrinterCheck;
using YuanTu.Devices.PrinterCheck.CePrinterCheck;

namespace YuanTu.ShengZhouZhongYiHospital.PrinterCheck
{
    public class PrinterCheck:YuanTu.Devices.PrinterCheck.ReceiptPrinterCheckService
    {
        public override Result CeCheckPrinter()
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
    }
}

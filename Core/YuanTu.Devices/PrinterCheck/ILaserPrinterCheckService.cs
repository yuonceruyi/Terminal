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
using YuanTu.Devices.PrinterCheck.BrotherPrinterCheck;

namespace YuanTu.Devices.PrinterCheck
{
    public interface ILaserPrinterCheckService : IService
    {
        /// <summary>
        ///     检测激光打印机
        /// </summary>
        /// <returns></returns>
        Result CheckLaserPrinter();
    }

    public class LaserPrinterCheckService : ILaserPrinterCheckService
    {
        public string ServiceName { get; } = "激光打印机检测服务";
        private const string BrotherPrinterKey = "Brother";

        public Result CheckLaserPrinter()
        {
            Logger.Device.Info("开始监控激光打印机状态");
            if (FrameworkConst.Local)
                return Result.Success();
            var configurationManager = ServiceLocator.Current.GetInstance<IConfigurationManager>();
            var printName = configurationManager.GetValue("Printer:Laser");
            Logger.Device.Info($"激光打印机名称{printName}");
            if (printName.Contains(BrotherPrinterKey))
                return BrotherPrinter(printName);
            return Result.Success();
        }

        public virtual Result BrotherPrinter(string name)
        {
            var brother = new BrotherPrinter(name);
            return brother.GetPrinterStatus();
        }
    }
}

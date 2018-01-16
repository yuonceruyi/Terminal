using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Practices.Unity;
using YuanTu.Consts.FrameworkBase;
using YuanTu.Consts.Services;
using YuanTu.Core.Log;

namespace YuanTu.Core.Services.LightBar
{
    public enum LightItem : byte
    {
        热敏打印机 = 0x01,
        银行卡 = 0x02,
        就诊卡社保卡 = 0x04,
        发卡机 = 0x08,
        激光打印机 = 0x10,
        病历本打印机 = 0x20,
        发票打印机 = 0x40,
        备用 = 0x80
    }

    public interface ILightBarService:IService
    {
        void PowerOn(byte lightitem);
        void PowerOn(LightItem lightitem);
        void PowerOff();
    }

    public class LightBarService : ILightBarService
    {
        public string ServiceName => "默认基础灯条";
        [Dependency]
        public IConfigurationManager Manager { get; set; }
        private byte lightItem = 0x00;

        public LightBarService()
        {
            (new Thread(_ =>
            {
                var serialport=new SerialPort();
                while (true)
                {
                    try
                    {
                        if (lightItem == 0x00)
                        {
                            Thread.Sleep(100);
                            continue;
                        }
                        var enable = Manager.GetValueInt("LightBar:Enable", 0) == 1;
                        Logger.Device.Info($"[灯条]是否允许:{enable}");
                        if (!enable)
                        {
                            Thread.Sleep(60 * 1000);
                            continue;
                        }
                        serialport.PortName = "COM" + Manager.GetValueInt("LightBar:Port", 3);
                        serialport.BaudRate = 9600;
                        Logger.Device.Info($"[灯条]串口:{serialport.PortName}");
                        try
                        {
                            serialport.Open();
                        }
                        catch (Exception ex)
                        {
                            Logger.Device.Error($"[灯条]串口:{ex.Message}");
                            Thread.Sleep(60 * 1000);
                            continue;
                        }
                        var workitemstr = Manager.GetValue("LightBar:WorkItem");
                        var workitem = workitemstr.StartsWith("0x")
                            ? int.Parse(workitemstr.Replace("0x", ""), NumberStyles.HexNumber)
                            : int.Parse(workitemstr);

                        var current = (byte)(lightItem & workitem);
                        var cmd = new byte[] { 0x03, 0xaa, current };
                        var closecmd = new byte[] { 0x03, 0xaa, 0x00 };
                        Logger.Device.Info($"[灯条]指令:{current}");
                        while (lightItem != 0x00)
                        {
                            serialport.Write(cmd, 0, cmd.Length);
                            Thread.Sleep(100);
                            serialport.Write(closecmd, 0, closecmd.Length);
                            Thread.Sleep(100);

                        }
                        serialport.Write(closecmd, 0, closecmd.Length);
                        serialport.Close();
                    }
                    catch (Exception ex)
                    {
                        Logger.Device.Error($"[灯条]异常:{ex}");

                    }
                }

            }) {IsBackground = true}).Start();
        }

        public void PowerOn(byte lightitem)
        {
            lightItem = lightitem;
        }

        public void PowerOn(LightItem lightitem)
        {
            lightItem = (byte)lightitem;
        }

        public void PowerOff()
        {
            lightItem = 0x00;
        }
    }
}

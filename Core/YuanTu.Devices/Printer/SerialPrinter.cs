using System;
using System.Diagnostics;
using System.IO.Ports;
using System.Windows.Forms;

namespace YuanTu.Devices.Printer
{
    public class SerialPrinter : PrintableBase
    {

        private class SerialPrinterContext : PrintableContext
        {
            public SerialPrinterContext()
            {
                base[SerialPort] = "COM1";
                base[BaudRate] = 38400;
            }
        }
        SerialPort serial = new SerialPort();

        #region Overrides of PrintableBase

        protected override Attempt<int> InternalConnect()
        {
            try
            {
                serial.PortName = (string)this.Context[PrintableContext.SerialPort];
                serial.BaudRate = (int)this.Context[PrintableContext.BaudRate]; ;
                serial.Open();

                return Attempt<int>.Succeed();
            }
            catch (Exception ex)
            {
                Debugger.Break();
                return Attempt<int>.Fail(ex.Message);
            }

        }

        protected override Attempt<int> InternalDisconnect()
        {
            serial.Close();
            return new Attempt<int>();
        }

        public override void InternalSendCommands(byte[] cmds)
        {
            if (serial.IsOpen)
            {
                serial.Write(cmds, 0, cmds.Length);
            }
            else
            {
                MessageBox.Show("端口没打开");
            }
        }

        public override PrintableContext GetContext()
        {
            return new SerialPrinterContext();
        }

        #endregion
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YuanTu.Devices.Printer
{
    /// <summary>
    /// 打印机的上下文
    /// </summary>
    public class PrintableContext : Dictionary<string, object>
    {
        // ReSharper disable InconsistentNaming
        public const string Ip = "Ip";
        public const string Port = "Port";
        public const string ParallelPort = "ParallelPort";
        public const string SerialPort = "SerialPort";
        public const string BaudRate = "BaudRate";
        public const string UsbPath = "UsbPath";
        public const string Defination = "Defination";
        public const int MaxBufferSize = 8192000;
        public const string PrinterName = "PrinterName";
        // ReSharper restore InconsistentNaming

        public PrintableContext()
        {
            WritePos = 0;
            Buffer = new List<byte>(1024);
            InternalBuffer = new List<byte[]>();
        }

        public List<byte> Buffer;
        public List<byte[]> InternalBuffer;
        public int WritePos = 0;

        public PrintableContext Append(byte[] cmd)
        {
            if (Buffer.Count + cmd.Length > MaxBufferSize)
            {
                InternalBuffer.Add(Buffer.ToArray());
                Buffer.Clear();
            }
            Buffer.AddRange(cmd);
            return this;
        }

        public PrintableContext Append(byte cmd)
        {
            if (Buffer.Count > MaxBufferSize - 1)
            {
                InternalBuffer.Add(Buffer.ToArray());
                Buffer.Clear();
            }
            Buffer.Add(cmd);
            return this;
        }

        //public PrintDefination PrintDefination {
        //    get {
        //        object result;
        //        if (this.TryGetValue(Defination, value: out result)) {
        //            return (PrintDefination)result;
        //        }
        //        return PrintDefination.Empty;
        //    }
        //}
    }
}

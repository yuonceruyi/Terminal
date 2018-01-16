using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YuanTu.Devices.Printer
{
    /// <summary>
    /// 对对象进行打印输出
    /// </summary>
    public interface IPrintableFormatter
    {
        /// <summary>
        /// 打印对象类型
        /// </summary>
        bool CanHandle(object printable);

        void Format(IPrintable printer, object printable);
    }

    public class NullPrintableFormatter : IPrintableFormatter
    {
        private static readonly IPrintableFormatter _instance = new NullPrintableFormatter();

        public static IPrintableFormatter Instance
        {
            get { return _instance; }
        }

        #region Implementation of IPrintableFormatter

        public bool CanHandle(object printable)
        {
            return false;
        }

        public void Format(IPrintable printer, object printable)
        {
            printer.Text(printable.ToString()).Print();
        }

        #endregion
    }
}

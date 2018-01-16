using System;
using System.Collections.Generic;
using YuanTu.Consts.FrameworkBase;

namespace YuanTu.Consts.Models.Print
{
    public interface IPrintModel : IModel
    {
        bool Success { get; set; }

        PrintInfo PrintInfo { get; set; }

        bool NeedPrint { get; set; }

        void SetPrintInfo(bool success, PrintInfo printInfo);
    }

    public class PrintModel : ModelBase, IPrintModel
    {
        public bool Success { get; set; }
        public PrintInfo PrintInfo { get; set; }

        public bool NeedPrint { get; set; }

        public void SetPrintInfo(bool success, PrintInfo printInfo)
        {
            Success = success;
            PrintInfo = printInfo;
            NeedPrint = printInfo.Printables != null || printInfo.PrintablesList != null;
        }
    }

    public class PrintInfo
    {
        /// <summary>
        ///     业务类型描述
        /// </summary>
        public string TypeMsg { get; set; }

        /// <summary>
        ///     具体提示信息
        /// </summary>
        public string TipMsg { get; set; }

        /// <summary>
        ///     调试信息，该信息仅在Debug模式下出现
        /// </summary>
        public string DebugInfo { get; set; }

        /// <summary>
        ///     要发送到的打印机的名称
        /// </summary>
        public string PrinterName { get; set; }

        /// <summary>
        ///     打印内容 
        /// </summary>
        public Queue<IPrintable> Printables { get; set; }

        /// <summary>
        /// 支持多条打印内容
        /// </summary>
        public List<Queue<IPrintable>> PrintablesList { get; set; }

        public string TipImage { get; set; }
    }
}
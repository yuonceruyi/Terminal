using System.Net;
using YuanTu.Consts;
using YuanTu.Core.Systems.Ini;
using YuanTu.ISO8583.Interfaces;

namespace YuanTu.ISO8583
{
    public class Config : IConfig
    {
        protected static readonly IniFile iniFile = new IniFile("POS.ini", true);

        public Config() : this("")
        {
        }

        public Config(string index)
        {
            IniTransSeq = new IniInteger(iniFile, "POS", "TransSeq" + index);
            IniLogonDate = new IniString(iniFile, "POS", "LogonDate" + index);
            IniBatchNo = new IniInteger(iniFile, "POS", "BatchNo" + index);
        }

        protected IniInteger IniBatchNo { get; }
        protected IniString IniLogonDate { get; }
        protected IniInteger IniTransSeq { get; }

        public IPAddress Address { get; set; }
        public int Port { get; set; }

        public string TPDU { get; set; }

        public string Head { get; set; }

        /// <summary>
        ///     …Ãªß∫≈
        /// </summary>
        public string MerchantId { get; set; }

        /// <summary>
        ///     ÷’∂À∫≈
        /// </summary>
        public string TerminalId { get; set; }

        /// <summary>
        /// </summary>
        public string AcqInst { get; set; }

        public string Field_2F01 { get; set; }

        public string Field_48 { get; set; }

        public int MainKeyIndex { get; set; }

        public int TransSeq
        {
            get { return IniTransSeq.Value; }
            set { IniTransSeq.Value = value; }
        }

        public bool IsLogon
        {
            get { return IniLogonDate.Value == DateTimeCore.Today.ToString("yyMMdd"); }
            set { IniLogonDate.Value = value ? DateTimeCore.Today.ToString("yyMMdd") : ""; }
        }

        public int BatchNo
        {
            get { return IniBatchNo.Value; }
            set { IniBatchNo.Value = value; }
        }
    }
}
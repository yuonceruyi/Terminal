using System.Collections.Generic;

namespace YuanTu.ShengZhouHospital.ISO8583
{
    public class Config
    {
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
        /// 
        /// </summary>
        public string AcqInst { get; set; }

        public string Field_2F01 { get; set; }

        public int MainKeyIndex { get; set; }

        public static Config DefaultConfig
        {
            get { return Configs[""]; }
            set { Configs[""] = value; }
        }

        public static Dictionary<string,Config> Configs { get; set; }  = new Dictionary<string, Config>();
    }
}
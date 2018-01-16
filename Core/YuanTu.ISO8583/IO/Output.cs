using System;
using Newtonsoft.Json;
using YuanTu.ISO8583.Data;
using YuanTu.ISO8583.Interfaces;

namespace YuanTu.ISO8583.IO
{
    public class Output
    {
        public IConfig Config { get; set; }
        public string Ret { get; set; }

        public string Message { get; set; }

        public DateTime TransTime { get; set; }

        [JsonIgnore]
        public Message MessageBody { get; set; }

        public bool Notify { get; set; }

        public int Amount { get; set; }

        public string BankNo { get; set; }

        public string CenterSeq { get; set; }

        public string ClearDate { get; set; }

        public string MerchantID { get; set; }

        public string TerminalID { get; set; }

        public int TransSeq { get; set; }

        public int BatchNo => Config.BatchNo;

        #region IC

        public bool ICCard { get; set; }

        public string TC { get; set; }

        public string TVR { get; set; }

        public string CSN { get; set; }

        public string AID { get; set; }

        public string ATC { get; set; }

        public string TSI { get; set; }

        public string APP_LABEL { get; set; }

        public string UNPR_NUM { get; set; }

        public string AIP { get; set; }

        public string CVMR { get; set; }

        public string IAD { get; set; }

        public string Term_Capa { get; set; }

        #endregion IC
    }
}
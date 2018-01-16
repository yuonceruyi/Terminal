using System;
using Newtonsoft.Json;
using YuanTu.ShengZhouZhongYiHospital.ISO8583.Data;

namespace YuanTu.ShengZhouZhongYiHospital.ISO8583
{
    public class Output
    {
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
        /// <summary>
        /// Åú´ÎºÅ
        /// </summary>
        public string LotId { get; set; }
        /// <summary>
        /// ÊÚÈ¨Âë
        /// </summary>
        public string AuthorizeNo { get; set; }
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
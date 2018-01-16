using System;

namespace YuanTu.ShengZhouHospital.ISO8583
{
    public class Input
    {
        public string BankNo { get; set; }

        public int Amount { get; set; }

        public string CenterSeq { get; set; }

        public string ClearDate { get; set; }

        public byte[] PIN { get; set; }

        public string Track2 { get; set; }

        public string Track3 { get; set; }

        public int TransSeq { get; set; }

        public DateTime Now { get; set; }

        public int CardSNum { get; set; }
    }
}
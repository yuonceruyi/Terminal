using System;
using YuanTu.Consts;

namespace YuanTu.ISO8583.IO
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

        public DateTime Now { get; set; } = DateTimeCore.Now;

        public int CardSNum { get; set; }
    }
}
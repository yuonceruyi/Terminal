using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace YuanTu.ZheJiangHospital.ICBC
{
    class PDll
    {
        [return: MarshalAs(UnmanagedType.BStr)]
        [DllImport("hzmispos.dll", EntryPoint = "YAddPIncType")]
        static extern string YAddPIncType(string chanel, string printFlag, string accountno, string accountid,
            string trademode, string cash,
            string operid, string deviceinfo,
            string tradeserial, string track2, string track3,
            string rsv1, string rsv2);

        [return: MarshalAs(UnmanagedType.BStr)]
        [DllImport("hzmispos.dll", EntryPoint = "YQueryAccNew")]
        static extern string YQueryAccNew(string chanel, string accountno, string accountid,
            string operid, string deviceinfo,
            string tradeserial,
            string rsv1, string rsv2);

        public static IcbcResponse Recharge(IcbcRequest req)
        {
            var s = YAddPIncType(req.Chanel, req.PrintFlag, req.AccountNo, req.AccountId,
                req.TradeMode, req.Cash,
                req.OperId, req.DeviceInfo,
                req.TradeSerial, req.Track2, req.Track3,
                req.Rsv1, req.Rsv2);
            return IcbcResponse.ParseRecharge(s);
        }

        public static IcbcResponse Query(IcbcRequest req)
        {
            var s = YQueryAccNew(req.Chanel, req.AccountNo, req.AccountId,
                req.OperId, req.DeviceInfo,
                req.TradeSerial,
                req.Rsv1, req.Rsv2);

            return IcbcResponse.ParseQuery(s);
        }
    }

    public class IcbcRequest
    {
        public string Chanel { get; set; }
        public string PrintFlag { get; set; }
        public string AccountNo { get; set; }
        public string AccountId { get; set; }
        public string TradeMode { get; set; }
        public string Cash { get; set; }
        public string OperId { get; set; }
        public string DeviceInfo { get; set; }
        public string TradeSerial { get; set; }
        public string Track2 { get; set; }
        public string Track3 { get; set; }
        public string Rsv1 { get; set; }
        public string Rsv2 { get; set; }
    }

    public class IcbcResponse
    {
        public bool Success { get; set; }
        public string ResultMark { get; set; }

        public string BankSerial { get; set; }

        public decimal Balance { get; set; }

        public decimal Amount { get; set; }

        public string BankWorkDt { get; set; }

        public static IcbcResponse ParseQuery(string s)
        {
            var r = new IcbcResponse();
            r.ResultMark = s.Substring(0, 5);
            r.BankSerial = s.Substring(5, 20);
            var f = s[25] == '1';
            var balance = decimal.Parse(s.Substring(26, 12));
            if (f)
                r.Balance = -balance;
            else
                r.Balance = balance;
            r.Success = r.ResultMark == "00000";
            return r;
        }

        public static IcbcResponse ParseRecharge(string s)
        {
            var r = new IcbcResponse();
            r.ResultMark = s.Substring(0, 5);
            r.BankSerial = s.Substring(5, 20);
            r.Amount = decimal.Parse(s.Substring(25, 12));
            r.BankWorkDt = s.Substring(37, 8);
            var f = s[45] == '1';
            var balance = decimal.Parse(s.Substring(46, 12));
            if (f)
                r.Balance = -balance;
            else
                r.Balance = balance;
            r.Success = r.ResultMark == "00000";
            return r;
        }
    }
}

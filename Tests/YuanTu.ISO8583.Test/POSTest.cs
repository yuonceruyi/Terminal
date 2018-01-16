using System;
using System.Net;
using YuanTu.Consts;
using YuanTu.ISO8583.IO;

namespace YuanTu.ISO8583.Test
{
    public class POSTest
    {
        public static void Run()
        {
            var pos = new POS
            {
                //Connection = new Connection
                //{
                //    Address = IPAddress.Parse("59.41.103.97"),
                //    Port = 18178
                //}
            };
            pos.CalcMacFunc = pos.CalcMAC;

            var logonRes = pos.DoLogon();
            if (!logonRes.IsSuccess)
                return;
            var bytes = logonRes.Value;
            pos.CheckKeys(bytes);

            var bankNo = "6225880209237727";
            var input = new Input
            {
                BankNo = bankNo,
                Amount = 32000,
                Now = DateTimeCore.Now,
                PIN = pos.GetPin(bankNo, "111111")
            };

            var res = pos.DoSale(input);
            if (!res.IsSuccess)
                return;

            res = pos.DoReverse(input);
            if (!res.IsSuccess)
                return;
        }
    }
}
using System;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using YuanTu.Consts;
using YuanTu.ISO8583.CPUCard;
using YuanTu.ISO8583.IO;
using YuanTu.ISO8583.Peripherals;
using YuanTu.ISO8583.Util;
using YuanTu.ISO8583.浙江;

namespace YuanTu.ISO8583.Test
{
    public class Program
    {
        public static bool Local { get; set; }

        public static void Main(string[] args)
        {
            //var pos = new 深圳.POS();
            //var sound = pos.CheckKeys("8E908BA24C362C1BA4B5E53E49041C1958673148EAE812007EBFB99F0000000000000000283231C1".Hex2Bytes());
            //Console.WriteLine(sound);
            //var pin1 = pos.GetPin("6225000000000014", "123456").Bytes2Hex();
            //Console.WriteLine(pin1);
            //var pin2 = pos.GetPin("0000000000000000", "123456").Bytes2Hex();
            //Console.WriteLine(pin2);
            //var pin3 = pos.GetPin("6225000000000014", "888888").Bytes2Hex();
            //Console.WriteLine(pin3);
            //var pin4 = pos.GetPin("0000000000000000", "888888").Bytes2Hex();
            //Console.WriteLine(pin4);
            //Console.ReadLine();
            //return;
            Local = args.Any();
            try
            {
                CPUDecoder.TagNames = Manager.LoadTagsDevice();

                var config = new Config("")
                {
                    Address = IPAddress.Parse("219.133.37.82"),
                    Port = 9900,
                    TPDU = "6000100000",
                    //Head = "603102000000",
                    TerminalId = "127133qB",
                    MerchantId = "103330180628515",
                    Field_2F01= "086123456789012316",
                    AcqInst = "81785800",
                };

                if (Local)
                {
                    new FakeServer(IPAddress.Loopback, 9900).Start();
                    config.Address = IPAddress.Loopback;
                    //config.Address = Dns.GetHostAddresses("0.tcp.ngrok.io")[0];
                    //config.Address = IPAddress.Parse("7.195.189.87");
                    //config.Port = 9900;
                }
                else
                {
                    //new WrapperFacade().Init(7, 9600);
                }

                var manager = new Loader().Initialize(config);

                manager.POS.CalcMacFunc = manager.POS.CalcMAC;

                var res = manager.Initialize();
                var log = manager.DoLogon();
                Console.WriteLine($"initialize: {res.IsSuccess}");

                #region Mag

                //var input = new Input
                //{
                //    Amount = 1,
                //    BankNo = "6228480402564890018",
                //    Track2 = "6228480402564890018=0000000000",
                //    Now = DateTime.Now
                //};
                //var logonRes = manager.DoLogon();

                //var valid = manager.POS.CheckKeys(logonRes.Value);

                //Console.WriteLine($"Keys valid:{valid}\n");

                ////input.PIN = manager.POS.GetPin(input.BankNo, "111111");
                //input.PIN = manager.POS.GetPin("0000000000000000", "111111");

                //var saleRes = manager.DoSale(input);

                //var reverseRes = manager.DoReverse();

                #endregion Mag

                #region IC

                //var input = new Input
                //{
                //    Amount = 1,
                //    //BankNo = "6228480402564890018",
                //    //Track2 = "6228480402564890018=0000000000",
                //    Now = DateTime.Now
                //};
                //manager.Wrapper.EnterCard(true);
                //manager.ReadCard(input);
                //var logonRes = manager.DoLogon();

                //var valid = manager.POS.CheckKeys(logonRes.Value);

                //Console.WriteLine($"Keys valid:{valid}\n");

                //var readRes = manager.ReadCard(input);

                //if (!readRes.IsSuccess)
                //{
                //    Console.WriteLine(readRes.Message);
                //    return;
                //}

                ////input.PIN = manager.POS.GetPin(input.BankNo, "123456");
                //input.PIN = manager.POS.GetPin("000000000000000", "123456");

                //var saleRes = manager.DoSale(input);

                //Console.WriteLine("DoSale Done");

                ////var reverseRes = manager.DoReverse();

                #endregion IC

                #region Test

                var s = "0000000000000000";
                var bytes = Encoding.GetEncoding("GBK").GetBytes(s);
                //bytes = "0000000000000000".Hex2Bytes();
                var ss = bytes.Bytes2Hex();
                Console.WriteLine(ss);

                var left = new Cryptography
                {
                    CipherMode = CipherMode.ECB,
                    PaddingMode = PaddingMode.Zeros,
                    Key = "0E4FD287867EFC34".Hex2Bytes(),
                    IV = new byte[8]
                };
                var right = new Cryptography
                {
                    CipherMode = CipherMode.ECB,
                    PaddingMode = PaddingMode.Zeros,
                    Key = "7DA7F788D8DC31A0".Hex2Bytes(),
                    IV = new byte[8]
                };

                // 8 字节对齐
                var g = bytes.Length/8;
                if (bytes.Length%8 != 0)
                {
                    g++;
                    var newData = new byte[g*8];
                    Array.Copy(bytes, newData, bytes.Length);
                    bytes = newData;
                }
                // 求异或
                var buffer = new byte[8];
                var dx = new byte[8];
                for (var i = 0; i < g; i++)
                {
                    Array.Copy(bytes, i*8, buffer, 0, 8);
                    Console.WriteLine($"data  {i:D2}:{buffer.Bytes2Hex()}");
                    for (var j = 0; j < 8; j++)
                        buffer[j] = (byte) (dx[j] ^ bytes[i*8 + j]);
                    Console.WriteLine($"buffer{i:D2}:{buffer.Bytes2Hex()}");

                    dx = left.DESEncrypt(buffer);
                    Console.WriteLine($"dx    {i:D2}:{dx.Bytes2Hex()}");
                    Console.WriteLine();
                }
                Console.WriteLine($"dx:{dx.Bytes2Hex()}");
                var dd = right.DESDecrypt(dx);
                Console.WriteLine($"dd:{dd.Bytes2Hex()}");

                var ddd = left.DESEncrypt(dd);

                Console.WriteLine($"mac:{ddd.Bytes2Hex()}");

                #endregion Test
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

            //EnDeCode.Run();

            //ICDecode();
            //Config.DefaultConfig.Field_2F01 = "086";
            //FullIC();
            //FullMag("6225880209237727", "111111");
            //FullMag("6225880209237727", "111112");
            //FullMag("6225000000000030", "123456");

            //Logger.Main.Info("What?");

            //FullDown();

            //POSTest.Run();

            //RSA.Main0();

            Console.ReadLine();
        }

        public static void ICDecode()
        {
            //string hex = Console.ReadLine();
            var hex =
                "9F260857E370AB5200A9189F2701809F101307150103A02000010A010000000000093707999F3704A59B756A9F360200839505080004E0009A031603219C01009F0206";
            var cpuDecoder = new CPUDecoder();
            var cpuEncoder = new CPUEncoder();
            while (!string.IsNullOrEmpty(hex))
            {
                cpuDecoder.Decode(hex.Hex2Bytes());
                cpuEncoder.Tlvs = cpuDecoder.Tlvs;
                var bytes = cpuEncoder.Encode();
                Console.WriteLine(bytes.Bytes2Hex());
                hex = Console.ReadLine();
            }
        }

        //public static void FullIC()
        //{
        //    var pos = new POS();
        //    pos.CalcMacFunc = pos.CalcMAC;
        //    var logonRes = pos.DoLogon();
        //    if (!logonRes.IsSuccess)
        //        return;

        //    var bytes = logonRes.Value;
        //    pos.CheckKeys(bytes);

        //    if (!File.Exists("PubKey.json"))
        //    {
        //        var downRes = pos.DownloadPubKey();
        //        if (!downRes.IsSuccess)
        //        {
        //            Console.WriteLine("DownloadPubKey Failed!");
        //            return;
        //        }
        //    }
        //    var listPubKeys = TLVStorage.Load("PubKey.json");
        //    ICPOS.PublicKeys = TLVStorage.LoadTLVDics(listPubKeys);
        //    Console.WriteLine("DownloadPubKey Done");

        //    if (!File.Exists("Params.json"))
        //    {
        //        var downRes = pos.DownloadParams();
        //        if (!downRes.IsSuccess)
        //        {
        //            Console.WriteLine("DownloadParams Failed!");
        //            return;
        //        }
        //    }
        //    var listParams = TLVStorage.Load("Params.json");
        //    ICPOS.Params = TLVStorage.LoadTLVDics(listParams);
        //    Console.WriteLine("DownloadParams Done");

        //    var input = new Input
        //    {
        //        Amount = 32000,
        //        Now = DateTimeCore.Now
        //    };
        //    var icPos = new ICPOS();

        //    if (!icPos.Init())
        //        return;

        //    var readRes = icPos.ReadCard(input);
        //    if (!readRes.IsSuccess)
        //        return;

        //    var list = icPos.FirstHalf();
        //    if (list == null)
        //        return;

        //    input.PIN = pos.GetPin(input.BankNo, "123456");

        //    var res = pos.DoSaleIC(input, list);
        //    if (!res.IsSuccess)
        //    {
        //        res = pos.DoReverseIC(input, list);
        //        Console.WriteLine("DoReverseIC " + (res.IsSuccess ? "Done" : "Failed"));
        //    }
        //    else
        //    {
        //        Console.WriteLine("DoSaleIC Done");
        //        var output = res.Value;

        //        if (!icPos.SecondHalf(output))
        //        {
        //            res = pos.DoReverseIC();
        //            Console.WriteLine("DoReverseIC " + (res.IsSuccess ? "Done" : "Failed"));
        //        }
        //        else
        //        {
        //            if (output.Notify)
        //            {
        //                res = pos.DoNotifyIC(input, icPos.MakeNotifyList());
        //                Console.WriteLine("DoNotifyIC " + (res.IsSuccess ? "Done" : "Failed"));
        //            }
        //            {
        //                res = pos.DoUploadIC(input, icPos.MakeUploadList());
        //                Console.WriteLine("DoUploadIC " + (res.IsSuccess ? "Done" : "Failed"));
        //            }
        //        }
        //    }
        //    icPos.Uninit();
        //}

        //public static void FullMag(string bankNo, string pass)
        //{
        //    var pos = new POS();
        //    pos.CalcMacFunc = pos.CalcMAC;

        //    var logonRes = pos.DoLogon();
        //    if (!logonRes.IsSuccess)
        //    {
        //        Console.WriteLine("DoLogon Failed:" + logonRes.Message);
        //        return;
        //    }
        //    Console.WriteLine("DoLogon Done");
        //    var bytes = logonRes.Value;
        //    pos.CheckKeys(bytes);

        //    var input = new Input
        //    {
        //        BankNo = bankNo,
        //        Amount = 32000,
        //        Now = DateTimeCore.Now,
        //        PIN = pos.GetPin(bankNo, pass),
        //        Track2 = "603367100132734910=230412007933286083",
        //        Track3 = ""
        //    };

        //    var res = pos.DoSale(input);
        //    if (!res.IsSuccess)
        //    {
        //        Console.WriteLine("DoSale Failed:" + res.Message);
        //        return;
        //    }
        //    Console.WriteLine("DoSale Done");

        //    res = pos.DoReverse();
        //    if (!res.IsSuccess)
        //    {
        //        Console.WriteLine("DoReverse Failed:" + res.Message);
        //        return;
        //    }
        //    Console.WriteLine("DoReverse Done");
        //}

        //public static void FullDown()
        //{
        //    var wrapper = new WrapperFacade();
        //    var ret = wrapper.Init(3, 9600);
        //    if (!ret)
        //        ret = wrapper.Init(4, 9600);
        //    Console.WriteLine($"Init = {ret}");
        //    if (!ret)
        //        return;
        //    ret = wrapper.EnterCard(true);
        //    var pos = CardPos.无卡;
        //    while ((pos != CardPos.停卡位) && (pos != CardPos.IC位))
        //    {
        //        Thread.Sleep(500);
        //        ret = wrapper.CheckCard(out pos);
        //        Console.WriteLine($"CheckCard = {pos}");
        //    }
        //    if (pos == CardPos.停卡位)
        //    {
        //        ret = wrapper.MoveCard();
        //        Console.WriteLine($"MoveCard = {ret}");
        //    }
        //    byte[] apdu;
        //    ret = wrapper.CPUCodeReset(out apdu);
        //    Console.WriteLine($"CPUCodeReset = {ret} {apdu?.Bytes2Hex()}");

        //    var keyCard = new KeyCard(wrapper);
        //    var snRes = keyCard.GetSN();
        //    if (!snRes.IsSuccess)
        //    {
        //        Console.WriteLine("Get SN Failed:" + snRes.Message);
        //        return;
        //    }
        //    var sn = snRes.Value.Bytes2Hex();
        //    Console.WriteLine("SN:" + sn);

        //    var posClient = new POS();
        //    posClient.CalcMacFunc = posClient.CalcMAC;

        //    var downRes = posClient.DoDownloadMasterKey(sn);
        //    if (!downRes.IsSuccess)
        //    {
        //        Console.WriteLine("DoDownloadMasterKey Failed:" + downRes.Message);
        //        return;
        //    }
        //    Console.WriteLine("PASSWORD:");
        //    var pass = Console.ReadLine();

        //    var varifyRes = keyCard.VerifyPassword(pass);
        //    if (!varifyRes.IsSuccess)
        //    {
        //        Console.WriteLine("VerifyPassword Failed:" + varifyRes.Message);
        //        return;
        //    }
        //    var full = downRes.Value;
        //    var key = full.Substring(0, full.Length - 4);
        //    var chk = full.Substring(full.Length - 4);
        //    var decryptRes = keyCard.Decrypt(key.Hex2Bytes(), chk.Hex2Bytes());
        //    if (!decryptRes.IsSuccess)
        //    {
        //        Console.WriteLine("Decrypt Failed:" + decryptRes.Message);
        //        return;
        //    }
        //    Console.WriteLine("Done Masterkey:" + decryptRes.Value.Bytes2Hex());
        //}
    }
}
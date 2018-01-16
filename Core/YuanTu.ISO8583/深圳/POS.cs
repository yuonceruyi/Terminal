using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using YuanTu.Consts;
using YuanTu.Consts.FrameworkBase;
using YuanTu.ISO8583.CPUCard;
using YuanTu.ISO8583.Data;
using YuanTu.ISO8583.IO;
using YuanTu.ISO8583.Util;

namespace YuanTu.ISO8583.深圳
{
    public class POS : ISO8583.POS
    {
        public override Result<byte[]> DoLogon()
        {
            var encoder = ContainerWrapper.CreateEncoder();
            encoder.MessageType = "0800";
            encoder.Values = new Dictionary<int, string>
            {
                [11] = Config.TransSeq++.ToFixedString(6),
                [41] = Config.TerminalId,
                [42] = Config.MerchantId,
                [60] = "00000001003",
                [63] = "007"
            };

            var send = encoder.Encode();
            var decoder = ContainerWrapper.CreateDecoder();
            decoder.Decode(send);
            var res = Connection.Handle(send);
            if (!res.IsSuccess)
                return Result<byte[]>.Fail(res.Message);

            var message = decoder.Decode(res.Value);
            if (message[39].Text != "00")
                return Result<byte[]>.Fail(message[39].Text + StatusCode.Get(message[39].Text));

            Config.BatchNo = Convert.ToInt32(message[60].Text.Substring(2, 6));
            //if (!CheckKeys(message[48].Text.Hex2Bytes()))
            //    return Result<byte[]>.Fail("密钥验证失败");
            return Result<byte[]>.Success(message[62].Text.Hex2Bytes());
        }

        /// <summary>
        ///     消费
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public override Result<Output> DoSale(Input input)
        {
            var encoder = ContainerWrapper.CreateEncoder();
            encoder.MessageType = "0200";
            encoder.CalcMacFunc = CalcMacFunc;
            input.TransSeq = Config.TransSeq;
            encoder.Values = new Dictionary<int, string>
            {
                [2] = input.BankNo,
                [3] = "000000", //
                [4] = input.Amount.ToString("D12"),
                [11] = input.TransSeq.ToFixedString(6),
                [22] = "021",
                [25] = "00",
                [26] = "06",
                [41] = Config.TerminalId,
                [42] = Config.MerchantId,
                [49] = "156",
                [52] = input.PIN.Bytes2Hex(),
                [53] = "1600000000000000",
                [60] = "22" + Config.BatchNo.ToString("D6"),
                [64] = "0000000000000000"
            };


            if (!string.IsNullOrEmpty(input.Track2))
                encoder.Values[35] = input.Track2;
            if (!string.IsNullOrEmpty(input.Track3))
                encoder.Values[36] = input.Track3;

            lastInput = input;

            var send = encoder.Encode();
            var decoder = ContainerWrapper.CreateDecoder();
            decoder.Decode(send);

            var res = Connection.Handle(send);
            Config.TransSeq++;
            if (!res.IsSuccess)
                return Result<Output>.Fail(res.Message);

            var message = decoder.Decode(res.Value);
            if (message[39].Text != "00")
                return Result<Output>.Fail(message[39].Text + StatusCode.Get(message[39].Text));

            return Result<Output>.Success(new Output
            {
                Ret = "00",
                TransSeq = input.TransSeq,
                TransTime = input.Now,
                Amount = input.Amount,
                TerminalID = Config.TerminalId,
                MerchantID = Config.MerchantId,
                BankNo = input.BankNo,
                CenterSeq = message[37].Text,
                ClearDate = message[15].Text,
                //AID = message[38].Text,  //授权码取不到
            });
        }

        /// <summary>
        ///     冲正
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public override Result<Output> DoReverse(Input input)
        {
            var now = DateTimeCore.Now;
            var encoder = ContainerWrapper.CreateEncoder();
            encoder.MessageType = "0400";
            encoder.CalcMacFunc = CalcMacFunc;
            encoder.Values = new Dictionary<int, string>
            {
                [2] = input.BankNo,
                [3] = "000000",
                [4] = input.Amount.ToString("D12"),
                [11] = input.TransSeq.ToFixedString(6),
                [22] = "021",
                [25] = "00",
                [39] = "96",
                [41] = Config.TerminalId,
                [42] = Config.MerchantId,
                [49] = "156",
                [60] = "22" + Config.BatchNo.ToString("D6"),
                //[61] = Config.BatchNo.ToString("D6") + input.TransSeq.ToString("D6") + input.ClearDate + "00000000000000000",
                [64] = "0000000000000000"
            };

            var send = encoder.Encode();
            var decoder = ContainerWrapper.CreateDecoder();
            decoder.Decode(send);
            var res = Connection.Handle(send);

            if (!res.IsSuccess)
                return Result<Output>.Fail(res.Message);

            var message = decoder.Decode(res.Value);
            if (message[39].Text != "00")
                return Result<Output>.Fail(message[39].Text + StatusCode.Get(message[39].Text));

            return Result<Output>.Success(new Output
            {
                Ret = "00",
                TransSeq = input.TransSeq,
                TransTime = now,
                Amount = input.Amount,
                TerminalID = Config.TerminalId,
                MerchantID = Config.MerchantId,
                BankNo = input.BankNo,
                CenterSeq = message[37].Text,
                ClearDate = message[15].Text
            });
        }

        /// <summary>
        ///     IC卡消费
        /// </summary>
        /// <param name="input"></param>
        /// <param name="list"></param>
        /// <returns></returns>
        public override Result<Output> DoSaleIC(Input input, List<CPUTlv> list)
        {
            lastInput = input;
            lastList = list;
            var encoder = ContainerWrapper.CreateEncoder();
            encoder.MessageType = "0200";
            encoder.CalcMacFunc = CalcMacFunc;
            input.TransSeq = Config.TransSeq;
            encoder.Values = new Dictionary<int, string>
            {
                [2] = input.BankNo,
                [3] = "000000",
                [4] = input.Amount.ToString("D12"),
                [11] = input.TransSeq.ToFixedString(6),
                [22] = "051",
                [23] = input.CardSNum.ToString("D4"),
                [25] = "00",
                [26] = "06",
                [41] = Config.TerminalId,
                [42] = Config.MerchantId,
                [49] = "156",
                [52] = input.PIN.Bytes2Hex(),
                [53] = "1600000000000000",
                [55] = "",
                [60] = "22" + Config.BatchNo.ToString("D6") + "00050",
                [64] = "0000000000000000"
            };
            encoder.ICPackages = list;

            if (!string.IsNullOrEmpty(input.Track2))
                encoder.Values[35] = input.Track2;
            if (!string.IsNullOrEmpty(input.Track3))
                encoder.Values[36] = input.Track3;

            var send = encoder.Encode();
            var decoder = ContainerWrapper.CreateDecoder();
            decoder.Decode(send);
            var res = Connection.Handle(send);
            Config.TransSeq++;
            if (!res.IsSuccess)
                return Result<Output>.Fail(res.Message);

            var message = decoder.Decode(res.Value);
            if (message[39].Text != "00")
                return Result<Output>.Fail(message[39].Text + StatusCode.Get(message[39].Text));

            input.CenterSeq = message[37].Text;
            return Result<Output>.Success(new Output
            {
                Ret = "00",
                TransSeq = input.TransSeq,
                TransTime = input.Now,
                Amount = input.Amount,
                TerminalID = Config.TerminalId,
                MerchantID = Config.MerchantId,
                BankNo = input.BankNo,
                CenterSeq = message[37].Text,
                ClearDate = message[15].Text,
                MessageBody = message,
                //AID = message[38].Text,//授权码取不到
            });
        }

        /// <summary>
        ///     IC卡冲正
        /// </summary>
        /// <param name="input"></param>
        /// <param name="list"></param>
        /// <returns></returns>
        public override Result<Output> DoReverseIC(Input input, List<CPUTlv> list)
        {
            var encoder = ContainerWrapper.CreateEncoder();
            encoder.MessageType = "0400";
            encoder.CalcMacFunc = CalcMacFunc;
            encoder.Values = new Dictionary<int, string>
            {
                [2] = input.BankNo,
                [3] = "000000",
                [4] = input.Amount.ToString("D12"),
                [11] = input.TransSeq.ToFixedString(6),
                [22] = "051",
                [23] = input.CardSNum.ToString("D4"),
                [25] = "00",
                [26] = "06",
                [39] = "96",
                [41] = Config.TerminalId,
                [42] = Config.MerchantId,
                [49] = "156",
                //[52] = input.PIN.Bytes2Hex(),
                [53] = "1600000000000000",
                [55] = "",
                [60] = "22" + Config.BatchNo.ToString("D6") + "00050",
                [64] = "0000000000000000"
            };
            encoder.ICPackages = list;

            var send = encoder.Encode();
            var decoder = ContainerWrapper.CreateDecoder();
            decoder.Decode(send);
            var res = Connection.Handle(send);
            if (!res.IsSuccess)
                return Result<Output>.Fail(res.Message);

            var message = decoder.Decode(res.Value);
            if (message[39].Text != "00")
                return Result<Output>.Fail(message[39].Text + StatusCode.Get(message[39].Text));

            return Result<Output>.Success(new Output
            {
                Ret = "00",
                TransSeq = input.TransSeq,
                TransTime = input.Now,
                Amount = input.Amount,
                TerminalID = Config.TerminalId,
                MerchantID = Config.MerchantId,
                BankNo = input.BankNo,
                CenterSeq = message[37].Text,
                ClearDate = message[15].Text,
                MessageBody = message,
            });
        }

        //public override Result<Output> DoNotifyIC(Input input, List<CPUTlv> list)
        //{
        //    var encoder = ContainerWrapper.CreateEncoder();
        //    encoder.MessageType = "0620";
        //    encoder.CalcMacFunc = CalcMacFunc;
        //    encoder.Values = new Dictionary<int, string>
        //    {
        //        [2] = input.BankNo,
        //        [3] = "000000",
        //        [4] = input.Amount.ToString("D12"),
        //        [11] = Config.TransSeq.ToString("D6"),
        //        [22] = "051",
        //        [23] = input.CardSNum.ToString("D4"),
        //        [25] = "00",
        //        [37] = input.CenterSeq,
        //        [41] = Config.TerminalId,
        //        [42] = Config.MerchantId,
        //        [49] = "156",
        //        [55] = "",
        //        [60] = "22" + Config.BatchNo.ToString("D6") + "95160",
        //        [64] = "0000000000000000"
        //    };
        //    encoder.ICPackages = list;

        //    var send = encoder.Encode();
        //    var decoder = ContainerWrapper.CreateDecoder();
        //    decoder.Decode(send);
        //    var res = Connection.Handle(send);
        //    if (!res.IsSuccess)
        //        return Result<Output>.Fail(res.Message);

        //    var message = decoder.Decode(res.Value);
        //    if (message[39].Text != "00")
        //        return Result<Output>.Fail(message[39].Text + StatusCode.Get(message[39].Text));

        //    return Result<Output>.Success(new Output
        //    {
        //        Ret = "00",
        //        TransSeq = Config.TransSeq++,
        //        TransTime = input.Now,
        //        Amount = input.Amount,
        //        TerminalID = Config.TerminalId,
        //        MerchantID = Config.MerchantId,
        //        BankNo = input.BankNo,
        //        MessageBody = message
        //    });
        //}

        //public override Result<Output> DoUploadIC(Input input, List<CPUTlv> list)
        //{
        //    var encoder = ContainerWrapper.CreateEncoder();
        //    encoder.MessageType = "0320";
        //    encoder.CalcMacFunc = CalcMacFunc;
        //    encoder.Values = new Dictionary<int, string>
        //    {
        //        [2] = input.BankNo,
        //        [4] = input.Amount.ToString("D12"),
        //        [11] = Config.TransSeq.ToString("D6"),
        //        [12] = input.Now.ToString("HHmmss"),
        //        [13] = input.Now.ToString("MMdd"),
        //        [22] = "051",
        //        [23] = input.CardSNum.ToString("D4"),
        //        [41] = Config.TerminalId,
        //        [42] = Config.MerchantId,
        //        [49] = "156",
        //        [55] = "",
        //        [60] = "000006100000000",
        //        [63] = "610100000000000000156",
        //    };
        //    encoder.ICPackages = list;

        //    var send = encoder.Encode();
        //    var decoder = ContainerWrapper.CreateDecoder();
        //    decoder.Decode(send);
        //    var res = Connection.Handle(send);
        //    if (!res.IsSuccess)
        //        return Result<Output>.Fail(res.Message);

        //    var message = decoder.Decode(res.Value);
        //    if (message[39].Text != "00")
        //        return Result<Output>.Fail(message[39].Text + StatusCode.Get(message[39].Text));

        //    return Result<Output>.Success(new Output
        //    {
        //        Ret = "00",
        //        TransSeq = Config.TransSeq++,
        //        TransTime = input.Now,
        //        Amount = input.Amount,
        //        TerminalID = Config.TerminalId,
        //        MerchantID = Config.MerchantId,
        //        BankNo = input.BankNo,
        //        CenterSeq = message[37].Text,
        //        MessageBody = message
        //    });
        //}

        public override bool CheckKeys(byte[] data)
        {
            var list = new byte[4][];

            /*
            对于双倍长密钥算法，前20个字节为PIN的工作密钥的密文，后20个字节为MAC的工作密钥的密文。
            （其中，“PIN工作密钥”前16个字节是密文，后4个字节是checkvalue；前16个字节解出明文后，
            对8个数值0做双倍长密钥算法，取结果的前四位与checkvalue 的值比较应该是一致的；
            “MAC工作密钥”前8个字节是密文，再8个字节是二进制零，后4个字节是checkvalue；前8个字节解出明文后，
            对8个数值0做单倍长密钥算法，取结果的前四位与checkvalue 的值比较应该是一致的）
            */
            list[0] = new byte[16];
            Array.Copy(data, 0, list[0], 0, 16);
            list[1] = new byte[4];
            Array.Copy(data, 16, list[1], 0, 4);
            list[2] = new byte[8];
            Array.Copy(data, 20, list[2], 0, 8);
            list[3] = new byte[4];
            Array.Copy(data, 36, list[3], 0, 4);

            var masterKey = "54304AD05FC35BEA48F835A2DA4E6142".Hex2Bytes();

            var cryptography = new Cryptography
            {
                CipherMode = CipherMode.ECB,
                PaddingMode = PaddingMode.Zeros,
                Key = masterKey,
                IV = new byte[8]
            };

            var pinKey = cryptography.TripleDESDecrypt(list[0]);
            var macKey = cryptography.TripleDESDecrypt(list[2]);

            Console.WriteLine(masterKey.Bytes2Hex());
            Console.WriteLine(list[0].Bytes2Hex());
            Console.WriteLine(list[1].Bytes2Hex());
            Console.WriteLine(list[2].Bytes2Hex());
            Console.WriteLine(list[3].Bytes2Hex());
            Console.WriteLine(pinKey.Bytes2Hex());
            Console.WriteLine(macKey.Bytes2Hex());

            cryptography.Key = pinKey;
            var pinChk = cryptography.TripleDESEncrypt(new byte[16]);
            if (!pinChk.Bytes2Hex().StartsWith(list[1].Bytes2Hex()))
                return false;
            cryptography.Key = macKey;
            var macChk = cryptography.DESEncrypt(new byte[8]);
            if (!macChk.Bytes2Hex().StartsWith(list[3].Bytes2Hex()))
                return false;
            PINKey = pinKey;
            MACKey = macKey;
            return true;
        }

        //public override byte[] GetPin(string cardNo, string pass)
        //{
        //    var pin = ("06" + pass + "FFFFFFFF").Hex2Bytes();
        //    var c = new Cryptography
        //    {
        //        CipherMode = CipherMode.ECB,
        //        PaddingMode = PaddingMode.Zeros,
        //        IV = new byte[8],
        //        Key = PINKey
        //    };
        //    var pinRes = c.TripleDESEncrypt(pin);
        //    return pinRes;
        //}

        public override byte[] CalcMAC(byte[] data)
        {
            // 截掉前13字节长度 后8字节MAC占位
            var len = data.Length - 13 - 8;
            var tmpData = new byte[len];
            Array.Copy(data, 13, tmpData, 0, len);
            var cryptography = new Cryptography
            {
                CipherMode = CipherMode.ECB,
                PaddingMode = PaddingMode.Zeros,
                Key = MACKey,
                IV = new byte[8]
            };
            return cryptography.MAC_DES_ECB(tmpData);
        }

        public override Result<Message> DownloadPubKey(string param)
        {
            var encoder = ContainerWrapper.CreateEncoder();
            encoder.MessageType = "0820";
            encoder.Values = new Dictionary<int, string>
            {
                [41] = Config.TerminalId,
                [42] = Config.MerchantId,
                [60] = "00" + Config.BatchNo.ToString("D6") + "372",
                [62] = param
            };

            var send = encoder.Encode();
            var decoder = ContainerWrapper.CreateDecoder();
            decoder.Decode(send);
            var res = Connection.Handle(send);
            if (!res.IsSuccess)
                return Result<Message>.Fail(res.Message);

            var message = decoder.Decode(res.Value);
            if (message[39].Text != "00")
                return Result<Message>.Fail(message[39].Text);

            return Result<Message>.Success(message);
        }

        public override Result<Message> DownloadPubKeySingle(string param)
        {
            var encoder = ContainerWrapper.CreateEncoder();
            encoder.MessageType = "0800";
            encoder.Values = new Dictionary<int, string>
            {
                [41] = Config.TerminalId,
                [42] = Config.MerchantId,
                [60] = "00" + Config.BatchNo.ToString("D6") + "370",
                [62] = param
            };

            var send = encoder.Encode();
            var decoder = ContainerWrapper.CreateDecoder();
            decoder.Decode(send);
            var res = Connection.Handle(send);
            if (!res.IsSuccess)
                return Result<Message>.Fail(res.Message);

            var message = decoder.Decode(res.Value);
            if (message[39].Text != "00")
                return Result<Message>.Fail(message[39].Text);

            return Result<Message>.Success(message);
        }

        public override Result<Message> DownloadParams(string param)
        {
            var encoder = ContainerWrapper.CreateEncoder();
            encoder.MessageType = "0820";
            encoder.Values = new Dictionary<int, string>
            {
                [41] = Config.TerminalId,
                [42] = Config.MerchantId,
                [60] = "00" + Config.BatchNo.ToString("D6") + "382",
                [62] = param
            };

            var send = encoder.Encode();
            var decoder = ContainerWrapper.CreateDecoder();
            decoder.Decode(send);
            var res = Connection.Handle(send);
            if (!res.IsSuccess)
                return Result<Message>.Fail(res.Message);

            var message = decoder.Decode(res.Value);
            if (message[39].Text != "00")
                return Result<Message>.Fail(message[39].Text);

            return Result<Message>.Success(message);
        }

        public override Result<Message> DownloadParamsSingle(string param)
        {
            var encoder = ContainerWrapper.CreateEncoder();
            encoder.MessageType = "0800";
            encoder.Values = new Dictionary<int, string>
            {
                [41] = Config.TerminalId,
                [42] = Config.MerchantId,
                [60] = "00" + Config.BatchNo.ToString("D6") + "380",
                [62] = param
            };

            var send = encoder.Encode();
            var decoder = ContainerWrapper.CreateDecoder();
            decoder.Decode(send);
            var res = Connection.Handle(send);
            if (!res.IsSuccess)
                return Result<Message>.Fail(res.Message);

            var message = decoder.Decode(res.Value);
            if (message[39].Text != "00")
                return Result<Message>.Fail(message[39].Text);

            return Result<Message>.Success(message);
        }
    }
}
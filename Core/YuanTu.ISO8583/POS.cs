using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using YuanTu.Consts;
using YuanTu.Consts.FrameworkBase;
using YuanTu.ISO8583.CPUCard;
using YuanTu.ISO8583.Data;
using YuanTu.ISO8583.Enums;
using YuanTu.ISO8583.Interfaces;
using YuanTu.ISO8583.IO;
using YuanTu.ISO8583.Util;

namespace YuanTu.ISO8583
{
    public class POS : IPOS
    {
        protected Input lastInput;
        protected List<CPUTlv> lastList;

        public IContainerWrapper ContainerWrapper { get; set; }

        public IConfig Config { get; set; }

        public IConnection Connection { get; set; }

        public Func<byte[], byte[]> CalcMacFunc { get; set; }

        public virtual Result<byte[]> DoLogon()
        {
            var now = DateTimeCore.Now;
            var encoder = ContainerWrapper.CreateEncoder();
            encoder.MessageType = "0800";
            encoder.Values = new Dictionary<int, string>
            {
                [1] = "",
                [3] = "000000",
                [7] = now.ToString("MMddHHmmss"),
                [11] = Config.TransSeq++.ToFixedString(6),
                [32] = Config.AcqInst,
                [41] = Config.TerminalId,
                [42] = Config.MerchantId,
                [53] = "2600000000000000"
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

            //Config.BatchNo = 0;//批次号默认0

            //if (!CheckKeys(message[48].Text.Hex2Bytes()))
            //    return Result<byte[]>.Fail("密钥验证失败");
            return Result<byte[]>.Success(message[48].Text.Hex2Bytes());
        }

        public virtual Result<Output> DoSale(Input input)
        {
            var encoder = ContainerWrapper.CreateEncoder();
            encoder.MessageType = "0200";
            encoder.CalcMacFunc = CalcMacFunc;
            encoder.Values = new Dictionary<int, string>
            {
                [1] = "",
                [2] = input.BankNo,
                [3] = "430350",
                [4] = input.Amount.ToString("D12"),
                [7] = input.Now.ToString("MMddHHmmss"),
                [11] = Config.TransSeq.ToFixedString(6),
                [12] = input.Now.ToString("HHmmss"),
                [13] = input.Now.ToString("MMdd"),
                [18] = "4900",
                [22] = "021",
                [25] = "00",
                [26] = "12",
                [32] = Config.AcqInst,
                [33] = Config.AcqInst,
                [41] = Config.TerminalId,
                [42] = Config.MerchantId,
                [48] = "",
                [49] = "156",
                [52] = input.PIN.Bytes2Hex(),
                [53] = "2600000000000000",
                [128] = "0000000000000000"
            };
            encoder.Packages = MakePackages();
            input.TransSeq = Config.TransSeq;

            if (!string.IsNullOrEmpty(input.Track2))
                encoder.Values[35] = input.Track2;
            if (!string.IsNullOrEmpty(input.Track3))
                encoder.Values[36] = input.Track3;

            lastInput = input;

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
                TransSeq = Config.TransSeq++,
                TransTime = input.Now,
                Amount = input.Amount,
                TerminalID = Config.TerminalId,
                MerchantID = Config.MerchantId,
                BankNo = input.BankNo,
                CenterSeq = message[37].Text,
                ClearDate = message[15].Text
            });
        }

        public virtual Result<Output> DoReverse()
        {
            return DoReverse(lastInput);
        }

        public virtual Result<Output> DoReverse(Input input)
        {
            var now = DateTimeCore.Now;
            var acq = Config.AcqInst.PadLeft(11, '0');
            var encoder = ContainerWrapper.CreateEncoder();
            encoder.MessageType = "0420";
            encoder.CalcMacFunc = CalcMacFunc;
            encoder.Values = new Dictionary<int, string>
            {
                [1] = "",
                [2] = input.BankNo,
                [3] = "430350",
                [4] = input.Amount.ToString("D12"),
                [7] = now.ToString("MMddHHmmss"),
                [11] = Config.TransSeq.ToFixedString(6),
                [12] = now.ToString("HHmmss"),
                [13] = now.ToString("MMdd"),
                [18] = "4900",
                [22] = "021",
                [25] = "00",
                [32] = Config.AcqInst,
                [33] = Config.AcqInst,
                [41] = Config.TerminalId,
                [42] = Config.MerchantId,
                [48] = "",
                [49] = "156",
                [90] = "0200" + input.TransSeq.ToString("D6") + input.Now.ToString("MMddHHmmss") + acq + acq,
                [128] = "0000000000000000"
            };
            encoder.Packages = MakePackages();

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
                TransSeq = Config.TransSeq++,
                TransTime = now,
                Amount = input.Amount,
                TerminalID = Config.TerminalId,
                MerchantID = Config.MerchantId,
                BankNo = input.BankNo,
                CenterSeq = message[37].Text,
                ClearDate = message[15].Text
            });
        }

        public virtual Result<Output> DoSaleIC(Input input, List<CPUTlv> list)
        {
            lastInput = input;
            lastList = list;
            var encoder = ContainerWrapper.CreateEncoder();
            encoder.MessageType = "0200";
            encoder.CalcMacFunc = CalcMacFunc;
            encoder.Values = new Dictionary<int, string>
            {
                [1] = "",
                [2] = input.BankNo,
                [3] = "430350",
                [4] = input.Amount.ToString("D12"),
                [7] = input.Now.ToString("MMddHHmmss"),
                [11] = Config.TransSeq.ToFixedString(6),
                [12] = input.Now.ToString("HHmmss"),
                [13] = input.Now.ToString("MMdd"),
                [18] = "8901",
                [22] = "051",
                [23] = input.CardSNum.ToString("D3"),
                [25] = "00",
                [26] = "06",
                [32] = Config.AcqInst,
                [33] = Config.AcqInst,
                [41] = Config.TerminalId,
                [42] = Config.MerchantId,
                [48] = "",
                [49] = "156",
                [52] = input.PIN.Bytes2Hex(),
                [53] = "2600000000000000",
                [55] = "",
                [60] = "2200000000050",
                [128] = "0000000000000000"
            };
            encoder.Packages = MakePackages();
            encoder.ICPackages = list;

            if (!string.IsNullOrEmpty(input.Track2))
                encoder.Values[35] = input.Track2;
            if (!string.IsNullOrEmpty(input.Track3))
                encoder.Values[36] = input.Track3;

            var send = encoder.Encode();
            var decoder = ContainerWrapper.CreateDecoder();
            decoder.Decode(send);
            var res = Connection.Handle(send);
            if (!res.IsSuccess)
                return Result<Output>.Fail(res.Message);

            var message = decoder.Decode(res.Value);
            if (message[39].Text != "00")
                return Result<Output>.Fail(message[39].Text + StatusCode.Get(message[39].Text));

            input.TransSeq = Config.TransSeq;
            input.CenterSeq = message[37].Text;
            return Result<Output>.Success(new Output
            {
                Ret = "00",
                TransSeq = Config.TransSeq++,
                TransTime = input.Now,
                Amount = input.Amount,
                TerminalID = Config.TerminalId,
                MerchantID = Config.MerchantId,
                BankNo = input.BankNo,
                CenterSeq = message[37].Text,
                ClearDate = message[15].Text,
                MessageBody = message
            });
        }

        public virtual Result<Output> DoReverseIC(Input input, List<CPUTlv> list)
        {
            var now = DateTimeCore.Now;
            var acq = Config.AcqInst.PadLeft(11, '0');
            var encoder = ContainerWrapper.CreateEncoder();
            encoder.MessageType = "0420";
            encoder.CalcMacFunc = CalcMacFunc;
            encoder.Values = new Dictionary<int, string>
            {
                [1] = "",
                [2] = input.BankNo,
                [3] = "430350",
                [4] = input.Amount.ToString("D12"),
                [7] = now.ToString("MMddHHmmss"),
                [11] = input.TransSeq.ToFixedString(6),
                [12] = now.ToString("HHmmss"),
                [13] = now.ToString("MMdd"),
                [18] = "8901",
                [22] = "051",
                [25] = "00",
                [32] = Config.AcqInst,
                [33] = Config.AcqInst,
                [41] = Config.TerminalId,
                [42] = Config.MerchantId,
                [48] = "",
                [49] = "156",
                [55] = "",
                [60] = "2200000000050",
                [90] = "0200" + input.TransSeq.ToString("D6") + input.Now.ToString("MMddHHmmss") + acq + acq,
                [128] = "0000000000000000"
            };
            encoder.Packages = MakePackages();
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
                TransSeq = Config.TransSeq++,
                TransTime = input.Now,
                Amount = input.Amount,
                TerminalID = Config.TerminalId,
                MerchantID = Config.MerchantId,
                BankNo = input.BankNo,
                CenterSeq = message[37].Text,
                ClearDate = message[15].Text,
                MessageBody = message
            });
        }

        public virtual Result<Output> DoReverseIC()
        {
            return DoReverseIC(lastInput, lastList);
        }

        public virtual Result<Output> DoNotifyIC(Input input, List<CPUTlv> list)
        {
            var acq = Config.AcqInst.PadLeft(11, '0');
            var encoder = ContainerWrapper.CreateEncoder();
            encoder.MessageType = "0620";
            encoder.CalcMacFunc = CalcMacFunc;
            encoder.Values = new Dictionary<int, string>
            {
                [1] = "",
                [2] = input.BankNo,
                [3] = "951000",
                [4] = input.Amount.ToString("D12"),
                [11] = Config.TransSeq.ToFixedString(6),
                [22] = "051",
                [23] = input.CardSNum.ToString("D3"),
                [32] = Config.AcqInst,
                [37] = input.CenterSeq,
                [41] = Config.TerminalId,
                [42] = Config.MerchantId,
                [49] = "156",
                [55] = "",
                [60] = "0000000195150",
                [61] = "000001" + input.TransSeq.ToString("D6") + input.Now.ToString("MMdd"),
                [90] = "0200" + input.TransSeq.ToString("D6") + input.Now.ToString("MMddHHmmss") + acq + acq,
                [128] = "0000000000000000"
            };
            encoder.Packages = MakePackages();
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
                TransSeq = Config.TransSeq++,
                TransTime = input.Now,
                Amount = input.Amount,
                TerminalID = Config.TerminalId,
                MerchantID = Config.MerchantId,
                BankNo = input.BankNo,
                MessageBody = message
            });
        }

        public virtual Result<Output> DoUploadIC(Input input, List<CPUTlv> list)
        {
            var encoder = ContainerWrapper.CreateEncoder();
            encoder.MessageType = "0320";
            encoder.CalcMacFunc = CalcMacFunc;
            encoder.Values = new Dictionary<int, string>
            {
                [1] = "",
                [2] = input.BankNo,
                [3] = "203000",
                [4] = input.Amount.ToString("D12"),
                [7] = input.Now.ToString("MMddHHmmss"),
                [11] = Config.TransSeq.ToFixedString(6),
                [12] = input.Now.ToString("HHmmss"),
                [13] = input.Now.ToString("MMdd"),
                [22] = "051",
                [23] = input.CardSNum.ToString("D3"),
                [32] = Config.AcqInst,
                [41] = Config.TerminalId,
                [42] = Config.MerchantId,
                [55] = "",
                [60] = "0000000120350",
                [62] = Encoding.Default.GetBytes("610000" + input.Amount.ToString("D12") + "156").Bytes2Hex()
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
                TransSeq = Config.TransSeq++,
                TransTime = input.Now,
                Amount = input.Amount,
                TerminalID = Config.TerminalId,
                MerchantID = Config.MerchantId,
                BankNo = input.BankNo,
                CenterSeq = message[37].Text,
                MessageBody = message
            });
        }

        public virtual Result DownloadPubKey()
        {
            var i = 0;
            var all = new List<Tuple<int, TlvPackageData>>();
            while (true)
            {
                var s = Encoding.Default.GetBytes((100 + i).ToString()).Bytes2Hex();
                var res = DownloadPubKey(s);
                if (!res.IsSuccess)
                    return Result.Fail(res.Message);
                var list = res.Value.PackageList;
                all.AddRange(list);
                if (!res.Value.HasMore)
                    break;
                i += list.Count(one => one.Item1 == 0x9F22);
            }
            var storageList = new List<TLVStorage>();
            for (var j = 0; j < all.Count; j += 3)
            {
                var aid = CPUTlv.Convert(all[j].Item2);
                var pki = CPUTlv.Convert(all[j + 1].Item2);
                var bytes = new CPUEncoder
                {
                    Tlvs = new List<CPUTlv> {aid, pki}
                }.Encode();
                var res = DownloadPubKeySingle(bytes.Bytes2Hex());
                if (res.IsSuccess)
                {
                    var f = res.Value[62];
                    storageList.Add(new TLVStorage
                    {
                        Name = aid.Value.Bytes2Hex() + "|" + pki.Value.Bytes2Hex(),
                        Value = f.Text.Substring(2)
                    });
                }
            }
            TLVStorage.Save("PubKey.json", storageList);
            return Result.Success();
        }

        public virtual Result DownloadParams()
        {
            var i = 0;
            var all = new List<Tuple<int, TlvPackageData>>();
            while (true)
            {
                var s = Encoding.Default.GetBytes((100 + i).ToString()).Bytes2Hex();
                var res = DownloadParams(s);
                if (!res.IsSuccess)
                    return Result.Fail(res.Message);
                var list = res.Value.PackageList;
                all.AddRange(list);
                if (!res.Value.HasMore)
                    break;
                i += list.Count(one => one.Item1 == 0x9F06);
            }
            var storageList = new List<TLVStorage>();
            foreach (var tuple in all)
            {
                var aid = CPUTlv.Convert(tuple.Item2);
                var bytes = new CPUEncoder
                {
                    Tlvs = new List<CPUTlv> {aid}
                }.Encode();
                var res = DownloadParamsSingle(bytes.Bytes2Hex());
                if (res.IsSuccess)
                {
                    var f = res.Value[62];
                    storageList.Add(new TLVStorage
                    {
                        Name = aid.Value.Bytes2Hex(),
                        Value = f.Text.Substring(2)
                    });
                }
            }
            TLVStorage.Save("Params.json", storageList);
            return Result.Success();
        }

        public virtual Result<string> DoDownloadMasterKey(string sn)
        {
            var encoder = ContainerWrapper.CreateEncoder();
            encoder.MessageType = "0800";
            encoder.Values = new Dictionary<int, string>
            {
                [1] = "",
                [3] = "960827",
                [7] = DateTimeCore.Now.ToString("MMddHHmmss"),
                [11] = Config.TransSeq++.ToFixedString(6),
                [32] = Config.AcqInst,
                [41] = Config.TerminalId,
                [42] = Config.MerchantId,
                [48] = sn,
                [53] = "2000000000000000"
            };

            encoder.BuildConfig.Fields[48].Format = Format.Binary;

            var send = encoder.Encode();
            var decoder = ContainerWrapper.CreateDecoder();
            decoder.Decode(send);

            encoder.BuildConfig.Fields[48].Format = Format.ASCII;

            var res = Connection.Handle(send);
            if (!res.IsSuccess)
                return Result<string>.Fail(res.Message);

            var message = decoder.Decode(res.Value);
            if (message[39].Text != "00")
                return Result<string>.Fail(message[39].Text);

            return Result<string>.Success(message[48].Text);
        }

        protected List<TlvPackageData> MakePackages()
        {
            return new List<TlvPackageData>
            {
                new TlvPackageData
                {
                    Tag = 0x1F21,
                    Length = 3,
                    Value = "350" //和第3域后3位一样
                },
                new TlvPackageData
                {
                    Tag = 0x2F01,
                    Length = Config.Field_2F01.Length,
                    Value = Config.Field_2F01
                },
                new TlvPackageData
                {
                    Tag = 0x2F05,
                    Length = 3,
                    Value = "123" //自定义对账数据
                }
            };
        }

        public virtual Result<Message> DownloadPubKey(string param)
        {
            var encoder = ContainerWrapper.CreateEncoder();
            encoder.MessageType = "0820";
            encoder.Values = new Dictionary<int, string>
            {
                [1] = "",
                [3] = "372000",
                [41] = Config.TerminalId,
                [42] = Config.MerchantId,
                [60] = "00000001372",
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

        public virtual Result<Message> DownloadPubKeySingle(string param)
        {
            var encoder = ContainerWrapper.CreateEncoder();
            encoder.MessageType = "0800";
            encoder.Values = new Dictionary<int, string>
            {
                [1] = "",
                [3] = "370000",
                [41] = Config.TerminalId,
                [42] = Config.MerchantId,
                [60] = "00000001370",
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

        public virtual Result<Message> DownloadParams(string param)
        {
            var encoder = ContainerWrapper.CreateEncoder();
            encoder.MessageType = "0820";
            encoder.Values = new Dictionary<int, string>
            {
                [1] = "",
                [3] = "382000",
                [41] = Config.TerminalId,
                [42] = Config.MerchantId,
                [60] = "00000001382",
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

        public virtual Result<Message> DownloadParamsSingle(string param)
        {
            var encoder = ContainerWrapper.CreateEncoder();
            encoder.MessageType = "0800";
            encoder.Values = new Dictionary<int, string>
            {
                [1] = "",
                [3] = "380000",
                [41] = Config.TerminalId,
                [42] = Config.MerchantId,
                [60] = "00000001380",
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

        #region Tools

        protected byte[] MasterKey = "1234567890ABCDEF1234567890ABCDEF".Hex2Bytes();
        protected byte[] MACKey;
        protected byte[] PINKey;

        public string GetPan(string cardNo)
        {
            // 取等号前
            var main = cardNo.Split('=')[0];

            // 右数第二位开始左取12位
            return "0000" + main.Substring(main.Length - 1 - 12, 12);
        }

        public virtual byte[] GetPin(string cardNo, string pass)
        {
            var pan = GetPan(cardNo).Hex2Bytes();
            var pin = ("06" + pass + "FFFFFFFF").Hex2Bytes();
            var xorRes = new byte[8];
            for (var i = 0; i < 8; i++)
                xorRes[i] = (byte) (pan[i] ^ pin[i]);
            var c = new Cryptography
            {
                CipherMode = CipherMode.ECB,
                PaddingMode = PaddingMode.Zeros,
                IV = new byte[8],
                Key = PINKey
            };
            var pinRes = c.TripleDESEncryptOk(xorRes);
            return pinRes;
        }

        public virtual bool CheckKeys(byte[] data)
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
            list[1] = new byte[2];
            Array.Copy(data, 16, list[1], 0, 2);
            list[2] = new byte[16];
            Array.Copy(data, 18, list[2], 0, 16);
            list[3] = new byte[2];
            Array.Copy(data, 34, list[3], 0, 2);

            var masterKey = MasterKey;

            var cryptography = new Cryptography
            {
                CipherMode = CipherMode.ECB,
                PaddingMode = PaddingMode.Zeros,
                Key = masterKey,
                IV = new byte[8]
            };

            var pinKey = cryptography.TripleDESDecryptOk(list[0]);
            var macKey = cryptography.TripleDESDecryptOk(list[2]);

            Console.WriteLine(masterKey.Bytes2Hex());
            Console.WriteLine(list[0].Bytes2Hex());
            Console.WriteLine(list[1].Bytes2Hex());
            Console.WriteLine(list[2].Bytes2Hex());
            Console.WriteLine(list[3].Bytes2Hex());
            Console.WriteLine(pinKey.Bytes2Hex());
            Console.WriteLine(macKey.Bytes2Hex());

            cryptography.Key = pinKey;
            var pinChk = cryptography.TripleDESEncryptOk(new byte[16]);
            if (!pinChk.Bytes2Hex().StartsWith(list[1].Bytes2Hex()))
                return false;
            cryptography.Key = macKey;
            var macChk = cryptography.TripleDESEncryptOk(new byte[16]);
            if (!macChk.Bytes2Hex().StartsWith(list[3].Bytes2Hex()))
                return false;
            PINKey = pinKey;
            MACKey = macKey;
            return true;
        }

        public virtual byte[] CalcMAC(byte[] data)
        {
            // 截掉前4字节长度 后8字节MAC占位
            var len = data.Length - 4 - 8;
            var tmpData = new byte[len];
            Array.Copy(data, 4, tmpData, 0, len);
            var cryptography = new Cryptography
            {
                CipherMode = CipherMode.ECB,
                PaddingMode = PaddingMode.Zeros,
                Key = MACKey,
                IV = new byte[8]
            };
            return cryptography.MAC_TDES_ECB(tmpData);
        }

        #endregion Tools
    }
}
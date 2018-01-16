using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using YuanTu.Consts;
using YuanTu.Consts.FrameworkBase;
using YuanTu.Core.Log;
using YuanTu.JiaShanHospital.ISO8583.Commons;
using YuanTu.JiaShanHospital.ISO8583.CPUCard;
using YuanTu.JiaShanHospital.ISO8583.Data;
using YuanTu.JiaShanHospital.ISO8583.Enums;


namespace YuanTu.JiaShanHospital.ISO8583
{
    public class POS
    {
        private readonly IniFile iniFile;
        private readonly IniString iniLogonDate;
        private readonly IniString initLotId;
        private readonly IniString initLogonData;
        private readonly IniInteger iniTransSeq;

        public Input lastInput;
        private List<CPUTlv> lastList;

        public POS(string index = "")
        {
            iniFile = new IniFile("POS.ini", true);
            iniTransSeq = new IniInteger(iniFile, "POS", "TransSeq" + index);
            iniLogonDate = new IniString(iniFile, "POS", "LogonDate" + index);
            initLotId = new IniString(iniFile, "POS", "LotId" + index);
            initLogonData = new IniString(iniFile, "POS", "LogonData" + index);
            Config = Config.Configs[index];
        }

        public Connection Connection { get; set; }

        public int TransSeq
        {
            get { return iniTransSeq.Value; }
            set
            {
                if (value>=1000000)
                {
                    value = 1;
                }
                iniTransSeq.Value = value;
            }
        }

        public string LotId//批次号，从签到中获取 62域
        {
            get { return initLotId.Value; }
            set { initLotId.Value = value; }
        } 
        public bool IsLogon
        {
            get { return iniLogonDate.Value == DateTimeCore.Today.ToString("yyMMdd"); }
            set { iniLogonDate.Value = value ? DateTimeCore.Today.ToString("yyMMdd") : ""; }
        }


        public Func<byte[], byte[]> CalcMacFunc { get; set; }
        public Func<byte[], byte[]> MacKeyEncrptyFunc { get; set; }


        public Config Config { get; set; }

        public List<TlvPackageData> MakePackages()
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

        //将25域改为14试试
        public Result<byte[]> DoLogon()
        {
            if (IsLogon)
            {
                var rest = initLogonData.Value.Hex2Bytes();
                if (!CheckKeys(rest))
                    return Result<byte[]>.Fail("密钥验证失败");
                return Result<byte[]>.Success(rest);
            }
            var now = DateTimeCore.Now;
            var encoder = new Encoder
            {
                MessageType = "0800",
                Values = new Dictionary<int, string>
                {
                    [3] = "940000",
                    [25] = "14",
                    [41] = Config.TerminalId,
                    [42] = Config.MerchantId,
                    [60] = "A00100",
                }
            };
            var send = encoder.Encode();
            var decoder = new Decoder();
            decoder.Decode(send);
            var res = Connection.Handle(send);
            if (!res.IsSuccess)
                return Result<byte[]>.Fail(res.Message);

            var message = decoder.Decode(res.Value);
            if (message[39].Text != "00")
                return Result<byte[]>.Fail(message[39].Text + StatusCode.Get(message[39].Text));
            if (!CheckKeys(message[61].Text.Hex2Bytes()))
                return Result<byte[]>.Fail("密钥验证失败");
            LotId = message[62].Text;
            initLogonData.Value = message[61].Text;
            TransSeq = int.Parse(message[11].Text)+1/*+10000*/;
            //var ret = SafeInfoUpload();
            //if (!ret.IsSuccess)
            //{
            //    return Result<byte[]>.Fail(ret.Message);
            //}
            IsLogon = true;
            return Result<byte[]>.Success(message[61].Text.Hex2Bytes());
        }

        public Result SafeInfoUpload()
        {
            var serialNo = "yt550abc".PadRight(50,' ');
            var comName = "yuantu".PadRight(60, ' '); ;
            var productType = "yt550".PadRight(20, ' '); ;
            var version = "20170427".PadRight(8, ' '); ;

            var area58 = serialNo + comName + productType + version;
            var now = DateTimeCore.Now;
            var encoder = new Encoder
            {
                MessageType = "0800",
                CalcMacFunc = CalcMacFunc,
                Values = new Dictionary<int, string>
                {
                    [3] = "960000",
                    [11]= TransSeq.ToString("D6"),
                    [12] = now.ToString("HHmmss"),
                    [13] = now.ToString("MMdd"),
                    [25] = "14",
                    [41] = Config.TerminalId,
                    [42] = Config.MerchantId,
                    [58]= area58,
                    [60] = "A00100",
                    [62] = LotId,
                    [64] = "0000000000000000",
                }
            };
            var send = encoder.Encode();
            var decoder = new Decoder();
            decoder.Decode(send);
            var res = Connection.Handle(send);
            TransSeq++;
            if (!res.IsSuccess)
                return Result.Fail(res.Message);

            var message = decoder.Decode(res.Value);
            if (message[39].Text != "00")
                return Result.Fail(message[39].Text + StatusCode.Get(message[39].Text));

            return Result.Success();
        } 

        public Result<Output> DoSale(Input input)
        {
            var now = DateTimeCore.Now;
            var trace2 = input.Track2 ?? "";
            var trace3 = input.Track3 ?? "";
            var bankNo = input.BankNo;
            var expire = "";
            var cvd2 = "";
            //var data58 = BuildSafeData();
            var data60 = BuildSensitiveData(ref bankNo,ref expire, ref cvd2,ref trace2, ref trace3);
            var encoder = new Encoder
            {
                MessageType = "0200",
                CalcMacFunc = CalcMacFunc,
                Values = new Dictionary<int, string>
                {
                    [2] = bankNo,
                    [3] = "00A000",
                    [4] = input.Amount.ToString("D12"),
                    [11] = TransSeq.ToString("D6"),
                    [12] = now.ToString("HHmmss"),
                    [13] = now.ToString("MMdd"),
                    [22] = "0901",
                    [25] = "14",
                    [35] = trace2,
                    [36] = trace3,
                    [41] = Config.TerminalId,
                    [42] = Config.MerchantId,
                    [49] = "156",
                    [52] = input.PIN.Bytes2Hex(),
                    //[58] = "",
                    [60] = data60,
                    [62] = LotId,
                    [64] = "0000000000000000",
                },
                Packages = MakePackages(),
                //Packages_58 = data58,
            };
            if (string.IsNullOrWhiteSpace(input.Track3))
            {
                encoder.Values.Remove(36);
            }
            input.TransSeq = TransSeq;
            //if (!string.IsNullOrEmpty(input.Track2))
            //    encoder.Values[35] = input.Track2;
            //if (!string.IsNullOrEmpty(input.Track3))
            //    encoder.Values[36] = input.Track3;

           

            var send = encoder.Encode();
            var decoder = new Decoder();
           var msg= decoder.Decode(send);
            
            var res = Connection.Handle(send);
            var old = TransSeq++;
            if (!res.IsSuccess)
                return Result<Output>.Fail(res.Message);
            
            var message = decoder.Decode(res.Value);
            if (message[39].Text != "00")
                return Result<Output>.Fail(message[39].Text + StatusCode.Get(message[39].Text));
            input.CenterSeq = message.Fields.ContainsKey(37) ? message[37]?.Text : "";
            input.TransSeq = old;
            lastInput = input;

            return Result<Output>.Success(new Output
            {
                Ret = "00",
                TransSeq = old,
                TransTime = input.Now,
                Amount = input.Amount,
                TerminalID = Config.TerminalId,
                MerchantID = Config.MerchantId,
                BankNo = input.BankNo,
                CenterSeq = message.Fields.ContainsKey(37) ? message[37]?.Text:"",
                LotId = LotId,
                AuthorizeNo = message.Fields.ContainsKey(38)? message[38]?.Text:"",
                Message = res.Value.Bytes2Hex(),
                //ClearDate = message[15].Text
            });
        }

        public Result<Output> DoReverse()
        {
            return DoReverse(lastInput);
        }

        public Result<Output> DoReverse(Input input)
        {
            var now = DateTimeCore.Now;
           // var acq = Config.AcqInst.PadLeft(11, '0');
            var trace2 = input.Track2;
            var trace3 = input.Track3;
            var bankNo = input.BankNo;
            var expire = "";
            var cvd2 = "";
            var data60 = BuildSensitiveData(ref bankNo, ref expire, ref cvd2, ref trace2, ref trace3);
            var encoder = new Encoder
            {
                MessageType = "0200",
                CalcMacFunc = CalcMacFunc,
                Values = new Dictionary<int, string>
                {

                    [2] = bankNo,
                    [3] = "02A000",
                    [4] = input.Amount.ToString("D12"),
                    [11] = TransSeq.ToString("D6"),
                    //[12] = now.ToString("HHmmss"),
                    //[13] = now.ToString("MMdd"),
                    [22] = "0901",
                    [23] = input.CardSNum.ToString("D4"),//new
                    [25] = "14",
                    [35] = trace2,
                    [36] = trace3,
                    [37] = input.CenterSeq,
                    [41] = Config.TerminalId,
                    [42] = Config.MerchantId,
                    //[48] = input.TransSeq.ToString("D12"),
                    [49] = "156",
                    [60] = data60,
                    [62] = LotId,
                    [64] = "0000000000000000"
                },
                //Packages = new List<TlvPackageData>
                //{
                //    new TlvPackageData
                //    {
                //        Tag = 0x1F21,
                //        Length = 3,
                //        Value = "350" //和第3域后3位一样
                //    },
                //    new TlvPackageData
                //    {
                //        Tag = 0x2F01,
                //        Length = Config.Field_2F01.Length,
                //        Value = Config.Field_2F01
                //    },
                //    new TlvPackageData
                //    {
                //        Tag = 0x2F05,
                //        Length = 3,
                //        Value = "123" //自定义对账数据
                //    }
                //}
            };
            if (string.IsNullOrWhiteSpace(input.Track3))
            {
                encoder.Values.Remove(36);
            }
            var send = encoder.Encode();
            var decoder = new Decoder();
            decoder.Decode(send);
            var res = Connection.Handle(send);
            if (!res.IsSuccess)
                return Result<Output>.Fail(res.Message);
            var old = TransSeq++;
            var message = decoder.Decode(res.Value);
            if (message[39].Text != "00")
                return Result<Output>.Fail(message[39].Text + StatusCode.Get(message[39].Text));

            return Result<Output>.Success(new Output
            {
                Ret = "00",
                TransSeq = old,
                TransTime = now,
                Amount = input.Amount,
                TerminalID = Config.TerminalId,
                MerchantID = Config.MerchantId,
                BankNo = input.BankNo,
                //CenterSeq = message[37].Text,
                //ClearDate = message[15].Text
            });
        }

        public Result<Output> DoRefund()
        {
            return DoRefund(lastInput);
        }
        public Result<Output> DoRefund(Input input)
        {
            var now = DateTimeCore.Now;
            var trace2 = input.Track2 ?? "";//.Replace('=','D');
            var trace3 = input.Track3 ?? "";//.Replace('=', 'D');
            var bankNo = input.BankNo;
            var expire = "";
            var cvd2 = "";
            var data60 = BuildSensitiveData(ref bankNo, ref expire, ref cvd2, ref trace2, ref trace3);
            var encoder = new Encoder
            {
                MessageType = "0420",
                CalcMacFunc = CalcMacFunc,
                Values = new Dictionary<int, string>
                {

                    [2] = bankNo,
                    [3] = "00A000",
                    [4] = input.Amount.ToString("D12"),
                    [11] = TransSeq.ToString("D6"),
                    [22] = "0901",
                   // [23] = input.CardSNum.ToString("D4"),//new
                    [25] = "14",
                    [41] = Config.TerminalId,
                    [42] = Config.MerchantId,
                    //[48]=LotId+input.CenterSeq,
                    //[48] = input.TransSeq.ToString("D6"),
                    [48] = LotId.Substring(6) + input.TransSeq.ToString("D6"),
                    [49] = "156",
                    [60] = data60,
                    [62] = LotId,
                    [64] = "0000000000000000"
                },
            };
            Logger.Net.Info($"Encode1");
            var send = encoder.Encode();
            Logger.Net.Info($"Encode2");
            var decoder = new Decoder();
            decoder.Decode(send);
            var res = Connection.Handle(send);
            if (!res.IsSuccess)
                return Result<Output>.Fail(res.Message);
            var old = TransSeq++;
            var message = decoder.Decode(res.Value);
            if (message[39].Text != "00")
                return Result<Output>.Fail(message[39].Text + StatusCode.Get(message[39].Text));

            return Result<Output>.Success(new Output
            {
                Ret = "00",
                TransSeq = old,
                TransTime = now,
                Amount = input.Amount,
                TerminalID = Config.TerminalId,
                MerchantID = Config.MerchantId,
                BankNo = input.BankNo,
                //CenterSeq = message[37].Text,
                //ClearDate = message[15].Text
            });
        }

        public Result<Output> DoSaleIC(Input input, List<CPUTlv> list)
        {
            lastInput = input;
            lastList = list;
            var trace2 = input.Track2??"";//.Replace('=','D');
            var trace3 = input.Track3??"";//.Replace('=', 'D');
            var bankNo = input.BankNo;
            var expire = "";
            var cvd2 = "";
            var data60 = BuildSensitiveData(ref bankNo, ref expire, ref cvd2, ref trace2, ref trace3);
            var encoder = new Encoder
            {
                MessageType = "0200",
                CalcMacFunc = CalcMacFunc,
                Values = new Dictionary<int, string>
                {
                    [2] = bankNo,
                    [3] = "00A000",
                    [4] = input.Amount.ToString("D12"),
                    [11] = TransSeq.ToString("D6"),
                    [22] = "0051",
                    [23] = input.CardSNum.ToString("D4"),
                    [25] = "14",
                    [35] = trace2,
                    [36] = trace3,
                    [41] = Config.TerminalId,
                    [42] = Config.MerchantId,
                    [52] = input.PIN.Bytes2Hex(),
                    [55] = "",
                    [60] = data60,
                    [62] = LotId,
                    [64] = "0000000000000000",
                },
                Packages = MakePackages(),
                ICPackages = list
            };
            if (string.IsNullOrWhiteSpace(input.Track3))
            {
                encoder.Values.Remove(36);
            }
            input.TransSeq = TransSeq;
            //if (!string.IsNullOrEmpty(input.Track2))
            //    encoder.Values[35] = input.Track2;
            //if (!string.IsNullOrEmpty(input.Track3))
            //    encoder.Values[36] = input.Track3;

            var send = encoder.Encode();
            var decoder = new Decoder();
            decoder.Decode(send);
            var res = Connection.Handle(send);
            if (!res.IsSuccess)
                return Result<Output>.Fail(res.Message);

            var message = decoder.Decode(res.Value);
           var old= TransSeq++;
            if (message[39].Text != "00")
                return Result<Output>.Fail(message[39].Text + StatusCode.Get(message[39].Text));
            
            //input.TransSeq = TransSeq;
            input.CenterSeq = message[37].Text;
            lastInput = input;
            return Result<Output>.Success(new Output
            {
                Ret = "00",
                TransSeq = old,
                TransTime = input.Now,
                Amount = input.Amount,
                TerminalID = Config.TerminalId,
                MerchantID = Config.MerchantId,
                BankNo = input.BankNo,
                CenterSeq = message[37]?.Text,
               // ClearDate = message[15].Text,
                MessageBody = message,
                LotId = LotId,
                AuthorizeNo = message[38]?.Text,
                Message = res.Value.Bytes2Hex(),
            });
        }

        public Result<Output> DoReverseIC(Input input, List<CPUTlv> list)
        {
            var now = DateTimeCore.Now;
            // var acq = Config.AcqInst.PadLeft(11, '0');
            var trace2 = input.Track2 ?? "";//.Replace('=','D');
            var trace3 = input.Track3 ?? "";//.Replace('=', 'D');
            var bankNo = input.BankNo;
            var expire = "";
            var cvd2 = "";
            var data60 = BuildSensitiveData(ref bankNo, ref expire, ref cvd2, ref trace2, ref trace3);
            var encoder = new Encoder
            {
                MessageType = "0200",
                CalcMacFunc = CalcMacFunc,
                Values = new Dictionary<int, string>
                {
                    [2] = bankNo,
                    [3] = "02A000",
                    [4] = input.Amount.ToString("D12"),
                    [11] = TransSeq.ToString("D6"),
                    [12] = now.ToString("HHmmss"),
                    [13] = now.ToString("MMdd"),
                    [22] = "0051",
                    [23] = input.CardSNum.ToString("D4"),
                    [25] = "14",
                    [35] = trace2,
                    [36] = trace3,
                    [37] = input.CenterSeq,
                    [41] = Config.TerminalId,
                    [42] = Config.MerchantId,
                    //[52] = input.PIN.Bytes2Hex(),
                    [55] = "",
                    [60] = data60,
                    [62] = LotId,
                    [64] = "0000000000000000",
                },
                Packages = MakePackages(),
                ICPackages = list
            };

            var send = encoder.Encode();
            var decoder = new Decoder();
            decoder.Decode(send);
            var res = Connection.Handle(send);
            if (!res.IsSuccess)
                return Result<Output>.Fail(res.Message);
            var old = TransSeq++;
            var message = decoder.Decode(res.Value);
            if (message[39].Text != "00")
                return Result<Output>.Fail(message[39].Text + StatusCode.Get(message[39].Text));

            return Result<Output>.Success(new Output
            {
                Ret = "00",
                TransSeq = old,
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

        public Result<Output> DoReverseIC()
        {
            return DoReverseIC(lastInput, lastList);
        }

        public Result<Output> DoRefundIC()
        {
            return DoRefundIC(lastInput, lastList);
        }
        public Result<Output> DoRefundIC(Input input, List<CPUTlv> list)
        {
            var now = DateTimeCore.Now;
            var trace2 = input.Track2 ?? "";//.Replace('=','D');
            var trace3 = input.Track3 ?? "";//.Replace('=', 'D');
            var bankNo = input.BankNo;
            var expire = "";
            var cvd2 = "";
            var data60 = BuildSensitiveData(ref bankNo, ref expire, ref cvd2, ref trace2, ref trace3);
           
            var encoder = new Encoder
            {
                MessageType = "0420",
                CalcMacFunc = CalcMacFunc,
                Values = new Dictionary<int, string>
                {
                    [2] = bankNo,
                    [3] = "00A000",
                    [4] = input.Amount.ToString("D12"),
                    [11] = TransSeq.ToString("D6"),
                    [22] = "0051",
                    [23] = input.CardSNum.ToString("D4"),//new
                    [25] = "14",
                    [41] = Config.TerminalId,
                    [42] = Config.MerchantId,
                    //[48] = LotId + input.CenterSeq,
                    //[48] = input.TransSeq.ToString("D6"),
                    [48] = LotId.Substring(6) + input.TransSeq.ToString("D6"),
                    [55] = "",
                    [49] = "156",
                    [60] = data60,
                    [62] = LotId,
                    [64] = "0000000000000000"
                },
                Packages = MakePackages(),
                ICPackages = list
            };
            
            var send = encoder.Encode();
            var decoder = new Decoder();
            decoder.Decode(send);
            var res = Connection.Handle(send);
            if (!res.IsSuccess)
                return Result<Output>.Fail(res.Message);
            var old = TransSeq++;
            var message = decoder.Decode(res.Value);
            if (message[39].Text != "00")
                return Result<Output>.Fail(message[39].Text + StatusCode.Get(message[39].Text));

            return Result<Output>.Success(new Output
            {
                Ret = "00",
                TransSeq = old,
                TransTime = now,
                Amount = input.Amount,
                TerminalID = Config.TerminalId,
                MerchantID = Config.MerchantId,
                BankNo = input.BankNo,
                //CenterSeq = message[37].Text,
                //ClearDate = message[15].Text
            });
        }

        public Result<Output> DoNotifyIC(Input input, List<CPUTlv> list)
        {
            var acq = Config.AcqInst.PadLeft(11, '0');
            var encoder = new Encoder
            {
                MessageType = "0620",
                CalcMacFunc = CalcMacFunc,
                Values = new Dictionary<int, string>
                {
                    [2] = input.BankNo,
                    [3] = "951000",
                    [4] = input.Amount.ToString("D12"),
                    [11] = TransSeq.ToString("D6"),
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
                },
                Packages = MakePackages(),
                ICPackages = list
            };

            var send = encoder.Encode();
            var decoder = new Decoder();
            decoder.Decode(send);
            var res = Connection.Handle(send);
            if (!res.IsSuccess)
                return Result<Output>.Fail(res.Message);
            var old = TransSeq++;
            var message = decoder.Decode(res.Value);
            if (message[39].Text != "00")
                return Result<Output>.Fail(message[39].Text + StatusCode.Get(message[39].Text));

            return Result<Output>.Success(new Output
            {
                Ret = "00",
                TransSeq = old,
                TransTime = input.Now,
                Amount = input.Amount,
                TerminalID = Config.TerminalId,
                MerchantID = Config.MerchantId,
                BankNo = input.BankNo,
                MessageBody = message
            });
        }

        public Result<Output> DoUploadIC(Input input,Output output, List<CPUTlv> list)
        {
            return Result<Output>.Success(null);
            var encoder = new Encoder
            {
                MessageType = "0320",
                CalcMacFunc = CalcMacFunc,
                Values = new Dictionary<int, string>
                {
                    [2] = input.BankNo,
                    [3] = "970000",
                    [11] = TransSeq.ToString("D6"),
                    [12] = input.Now.ToString("HHmmss"),
                    [13] = input.Now.ToString("MMdd"),
                    [23] = input.CardSNum.ToString("D3"),
                    [25] = "50",
                    [37] = output.CenterSeq,
                    [41] = Config.TerminalId,
                    [42] = Config.MerchantId,
                    [55] = "",
                    [60] = "2200000000050",
                    [64]= "0000000000000000",
                },
                ICPackages = list
            };

            var send = encoder.Encode();
            var decoder = new Decoder();
            decoder.Decode(send);
            var res = Connection.Handle(send);
            if (!res.IsSuccess)
                return Result<Output>.Fail(res.Message);
            if (send.SequenceEqual(res.Value))//出入包完全相同，原包打回
            {
                return Result<Output>.Fail("【DoUploadIC】原包打回");
            }
            var message = decoder.Decode(res.Value);
            if (message[39].Text != "00")
                return Result<Output>.Fail(message[39].Text + StatusCode.Get(message[39].Text));

            return Result<Output>.Success(new Output
            {
                Ret = "00",
                TransSeq = TransSeq++,
                TransTime = input.Now,
                Amount = input.Amount,
                TerminalID = Config.TerminalId,
                MerchantID = Config.MerchantId,
                BankNo = input.BankNo,
                CenterSeq = message[37].Text,
                MessageBody = message
            });
        }
		
		  //下载IC卡终端参数
        public Result<Message> DownloadICParams()
        {
            var now=DateTimeCore.Now;
            var encoder = new Encoder
            {
                CalcMacFunc = CalcMacFunc,
                MessageType = "0800",
                Values = new Dictionary<int, string>
                {
                    [2] = "00000000000000000000",
                    [3] = "920000",
                    [11] = TransSeq++.ToString("D6"),
                    [12] = now.ToString("HHmmss"),
                    [13] = now.ToString("MMdd"),
                    [25] = "14",
                    [41] = Config.TerminalId,
                    [42] = Config.MerchantId,
                    [60] = "00",
                    [64] = "0000000000000000"
                }
            };
            var send = encoder.Encode();
            var decoder = new Decoder();
            decoder.Decode(send);
            var res = Connection.Handle(send);
            if (!res.IsSuccess)
                return Result<Message>.Fail(null);

            var message = decoder.Decode(res.Value);
            if (message[39].Text != "00")
                return Result<Message>.Fail(message[39].Text);
           
            return Result<Message>.Success(message);
        }

        public Result<Message> DownloadICPublicKey(Message icparam,string savepath)
        {
            var count = int.Parse(icparam[63].Text.Substring(0, 2));
            var storageList = new List<TLVStorage>();
            for (int i = 0; i < count; i++)
            {
                var s = (i + 1).ToString("D2");//(new byte[] {(byte)(i+1)}).Bytes2Hex();
                var encoder = new Encoder
                {
                    CalcMacFunc = CalcMacFunc,
                    MessageType = "0800",
                    Values = new Dictionary<int, string>
                    {
                        [2] = "00000000000000000000",
                        [3] = "930000",
                        [11] = TransSeq++.ToString("D6"),
                        [25] = "14",
                        [41] = Config.TerminalId,
                        [42] = Config.MerchantId,
                        [60] = "00",
                        [63] = s,
                        [64] = "0000000000000000"
                    }
                };
                var send = encoder.Encode();
                var decoder = new Decoder();
                decoder.Decode(send);
                var res = Connection.Handle(send);
                if (!res.IsSuccess)
                    return Result<Message>.Fail(res.Message);

                var message = decoder.Decode(res.Value);
                if (message[39].Text != "00")
                    return Result<Message>.Fail(message[39].Text);
                var section63 = message[63].Text;
                var index = 2;
                var rid = section63.Substring(index, 10);
                index += 10;
                var 认证中心公钥索引 = section63.Substring(index, 2);
                index += 2;
                var 认证中心哈希算法标识 = section63.Substring(index, 2);
                index += 2;
                var 认证中心公钥算法标识 = section63.Substring(index, 2);
                index += 2;
                var 认证中心公钥模Length = section63.Substring(index, 2);
                index += 2;
                var realLen = int.Parse(认证中心公钥模Length, NumberStyles.HexNumber) * 2;
                var 认证中心公钥模 = section63.Substring(index, realLen);
                index += realLen;
                var 认证中心公钥指数 = section63.Substring(index, 6);
                index += 6;
                var 证中心公钥校验数 = section63.Substring(index, 40);
                index += 40;
                var 认证中心公钥失效日期 = "3230" + section63.Substring(index, 6);//ASCII
                var lst = new[]
                {
                    new CPUTlv()
                    {
                        Tag = 0x9f06,
                        Value = rid.Hex2Bytes(),
                    },
                     new CPUTlv()
                    {
                        Tag = 0x9f22,
                        Value = 认证中心公钥索引.Hex2Bytes(),
                    }, new CPUTlv()
                    {
                        Tag = 0xdf05,
                        Value = 认证中心公钥失效日期.Hex2Bytes(),
                    }, new CPUTlv()
                    {
                        Tag = 0xdf06,
                        Value = 认证中心哈希算法标识.Hex2Bytes(),
                    }, new CPUTlv()
                    {
                        Tag = 0xdf07,
                        Value = 认证中心公钥算法标识.Hex2Bytes(),
                    }, new CPUTlv()
                    {
                        Tag = 0xdf02,
                        Value = 认证中心公钥模.Hex2Bytes(),
                    }, new CPUTlv()
                    {
                        Tag = 0xdf04,
                        Value = 认证中心公钥指数.Hex2Bytes(),
                    }, new CPUTlv()
                    {
                        Tag = 0xdf03,
                        Value = 证中心公钥校验数.Hex2Bytes(),
                    },

                };
                var text = new CPUEncoder() { Tlvs = lst.ToList() }.Encode().Bytes2Hex();
                storageList.Add(new TLVStorage()
                {
                    Name = rid+"|"+ 认证中心公钥索引,
                    Value = text
                });
            }
            TLVStorage.Save(savepath??"PubKey.json", storageList);
            return Result<Message>.Success(null);
        }

        public Result<string> DownloadAid(Message icparam,string savepath)
        {
            var t = icparam[63].Text;
            var count = int.Parse(t.Substring(t.Length-2),NumberStyles.HexNumber);
            var storageList = new List<TLVStorage>();
            for (int i = 0; i < count; i++)
            {
                var s = (i + 1).ToString("D2"); // (new byte[] { (byte)(i + 1) }).Bytes2Hex();
                var encoder = new Encoder
                {
                    CalcMacFunc = CalcMacFunc,
                    MessageType = "0800",
                    Values = new Dictionary<int, string>
                    {
                        [2] = "00000000000000000000",
                        [3] = "990000",
                        [11] = TransSeq++.ToString("D6"),
                        [25] = "50",
                        [41] = Config.TerminalId,
                        [42] = Config.MerchantId,
                        [60] = "00",
                        [63] = s,
                        [64] = "0000000000000000"
                    }
                };
                var send = encoder.Encode();
                var decoder = new Decoder();
                decoder.Decode(send);
                var res = Connection.Handle(send);
                if (!res.IsSuccess)
                    return Result<string>.Fail(null);

                var message = decoder.Decode(res.Value);
                if (message[39].Text != "00")
                    return Result<string>.Fail(message[39].Text);
                var section63 = message[63].Text;
                var index = 2;
                var aid = section63.Substring(index, 32).Replace("FF","^").TrimEnd('^');
                index += 32;
                var 应用版本号= section63.Substring(index, 4);
                index += 4;
                var AID匹配方式 = section63.Substring(index, 2);
                index += 2;
                var 终端行为代码缺省 = section63.Substring(index, 10);
                index += 10;
                var 终端行为代码拒绝= section63.Substring(index, 10);
                index += 10;
                var 终端行为代码联机 = section63.Substring(index, 10);
                index += 10;
                var 终端最低限额 = section63.Substring(index, 12);
                index += 12;
                var 随机交易选择指示符 = section63.Substring(index, 2);
                index += 2;
                var 偏置随机选择阈值 = section63.Substring(index, 12);
                index += 12;
                var 偏置随机选择最大目标百分数 = section63.Substring(index, 2);
                index += 2;
                var 随机选择目标百分数 = section63.Substring(index, 2);
                index += 2;
                var 缺省DDOL = section63.Substring(index, 6);
                index += 6;
                var 终端联机PIN支持能力 = section63.Substring(index, 2);
                index += 2;
                var lst = new[]
                {
                    new CPUTlv()
                    {
                        Tag = 0x9f06,
                        Value = aid.Hex2Bytes(),
                    }, new CPUTlv()
                    {
                        Tag = 0xdf01,
                        Value =AID匹配方式.Hex2Bytes(),
                    }, new CPUTlv()
                    {
                        Tag = 0x9f08,
                        Value = 应用版本号.Hex2Bytes(),
                    }, new CPUTlv()
                    {
                        Tag = 0xdf11,
                        Value = 终端行为代码缺省.Hex2Bytes(),
                    }, new CPUTlv()
                    {
                        Tag = 0xdf12,
                        Value = 终端行为代码联机.Hex2Bytes(),
                    }, new CPUTlv()
                    {
                        Tag = 0xdf13,
                        Value = 终端行为代码拒绝.Hex2Bytes(),
                    }, new CPUTlv()
                    {
                        Tag = 0x9f1b,
                        Value = 终端最低限额.Hex2Bytes(),
                    }, new CPUTlv()
                    {
                        Tag = 0xdf15,
                        Value = 偏置随机选择阈值.Hex2Bytes(),
                    }, new CPUTlv()
                    {
                        Tag = 0xdf16,
                        Value = 偏置随机选择最大目标百分数.Hex2Bytes(),
                    }, new CPUTlv()
                    {
                        Tag = 0xdf17,
                        Value = 随机选择目标百分数.Hex2Bytes(),
                    }, new CPUTlv()
                    {
                        Tag = 0xdf14,
                        Value = 缺省DDOL.Hex2Bytes(),
                    }, new CPUTlv()
                    {
                        Tag = 0xdf18,
                        Value = 终端联机PIN支持能力.Hex2Bytes(),
                    }, new CPUTlv()
                    {
                        Tag = 0x9f7b,
                        Value = 终端最低限额.Hex2Bytes(),
                    },
                     new CPUTlv()
                    {
                        Tag = 0xdf19,
                        Value = "000000100000".Hex2Bytes(),
                    },new CPUTlv()
                    {
                        Tag = 0xdf20,
                        Value = "000000100000".Hex2Bytes(),
                    },new CPUTlv()
                    {
                        Tag = 0xdf21,
                        Value = "000000100000".Hex2Bytes(),
                    },


                };
                var text=new CPUEncoder() {Tlvs = lst.ToList()}.Encode().Bytes2Hex();
                storageList.Add(new TLVStorage()
                {
                    Name = aid,
                    Value = text
                });

            }
            TLVStorage.Save(savepath??"Params.json", storageList);

            return Result<string>.Success("");
        }



        #region Tools

        public static byte[] MasterKey = "0123456789ABCDEF0123456789ABCDEF".Hex2Bytes();
       // public static byte[] MasterKey = "5BAE32133D54B3C8B0202F3BFD235DBF".Hex2Bytes();
        private byte[] MACKey= "98400E58312AD53D".Hex2Bytes();
        private byte[] PINKey= "B5F4B69BFE5E8C75DF64F88F152F0DC8".Hex2Bytes();

        public string GetPan(string cardNo)
        {
            // 取等号前
            var main = cardNo.Split('=')[0];

            // 右数第二位开始左取12位
            return "0000" + main.Substring(main.Length - 1 - 12, 12);
        }

        public byte[] GetPin(string cardNo, string pass)
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

        public bool CheckKeys(byte[] data)
        {
          
            var list = new byte[4][];
            /*浙江省农行 该区域长度：33,1-33位位有效内容 1-16 PinKey 17-33 MacKey*/
            /*
            对于双倍长密钥算法，前20个字节为PIN的工作密钥的密文，后20个字节为MAC的工作密钥的密文。
            （其中，“PIN工作密钥”前16个字节是密文，后4个字节是checkvalue；前16个字节解出明文后，
            对8个数值0做双倍长密钥算法，取结果的前四位与checkvalue 的值比较应该是一致的；
            “MAC工作密钥”前8个字节是密文，再8个字节是二进制零，后4个字节是checkvalue；前8个字节解出明文后，
            对8个数值0做单倍长密钥算法，取结果的前四位与checkvalue 的值比较应该是一致的）
            */
            list[0] = new byte[16];
            Array.Copy(data, 1, list[0], 0, 16);
            list[1] = new byte[2];
            // Array.Copy(data, 16, list[1], 0, 2);
            list[2] = new byte[16];
            Array.Copy(data, 17, list[2], 0, 16);
            list[3] = new byte[2];
            // Array.Copy(data, 34, list[3], 0, 2);

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
            PINKey = pinKey;//.Take(pinKey.Length/2).ToArray();
            MACKey = macKey.Take(macKey.Length / 2).ToArray();
            Logger.POS.Info($"主密钥：{masterKey.Bytes2Hex()} PINKey:{PINKey.Bytes2Hex()} MACKey:{MACKey.Bytes2Hex()}");
            return true;
            
        }


        public byte[] CalcMAC(byte[] data)
        {
            var cryptography = new Cryptography
            {
                CipherMode = CipherMode.ECB,
                PaddingMode = PaddingMode.Zeros,
                Key = MACKey,
                IV = new byte[8]
            };
            return cryptography.MAC_ECB_DES(data);
        }
        public byte[] EncryptByWorkKey(byte[] data)
        {
            var cryptography = new Cryptography
            {
                CipherMode = CipherMode.ECB,
                PaddingMode = PaddingMode.Zeros,
                Key = MACKey,
                IV = new byte[8]
            };
            return cryptography.DESEncrypt(data);
        }

        private string BuildSensitiveData(ref string cardNo,ref string expir,ref string cvd,ref string _35,ref string _36)
        {
            try
            {
                var inner35 = _35;
                var inner36 = _36;
                if (_35.Length > 32)
                {
                    inner35 = _35.Substring(0, 32);
                    _35 = "".PadRight(32, '0') + _35.Substring(32);
                }
                if (_36.Length > 32)
                {
                    inner36 = _36.Substring(0, 32);
                    _36 = "".PadRight(32, '0') + _36.Substring(32);
                }
                inner35 = inner35.Replace('=', 'D');
                inner36 = inner36.Replace('=', 'D');
                var arr = new[] { cardNo, expir, cvd, inner35, inner36 };

                cardNo = "".PadRight(cardNo?.Length ?? 0, '0');
                expir = "".PadRight(expir?.Length ?? 0, '0');
                cvd = "".PadRight(cvd?.Length ?? 0, '0');

                var innerFunc = (Func<string, string>)(p =>
                {
                    return string.IsNullOrEmpty(p) ? "" : p + "F";
                });
                var oristr = string.Join("", arr.Select(p => innerFunc(p)));
                var bitmap = new byte[2];
                for (int i = 0; i < arr.Length; i++)
                {
                    bitmap[i / 8] |= (byte)(string.IsNullOrEmpty(arr[i]) ? 0 : (1 << (7 - i % 8)));
                }


                oristr = oristr.PadRight((oristr.Length + 1) / 2 * 2, 'F');
                //var c = new Cryptography
                //{
                //    CipherMode = CipherMode.ECB,
                //    PaddingMode = PaddingMode.Zeros,
                //    IV = new byte[8],
                //    Key = MACKey
                //};
                var hexBts = oristr.Hex2Bytes();
                hexBts = hexBts.Concat(new byte[(hexBts.Length + 7) / 8 * 8 - hexBts.Length]).ToArray();
                var lst = new List<byte>();

                for (int i = 0; i < hexBts.Length; i += 8)
                {
                    lst.AddRange(MacKeyEncrptyFunc(hexBts.Skip(i).Take(8).ToArray()));
                }
                return "A00200" + bitmap.Bytes2Hex() + lst.ToArray().Bytes2Hex();
            }
            catch (Exception ex)
            {
                Logger.POS.Error($"BuildSensitiveData异常:{ex.Message} {ex.StackTrace}");
                return null;
            }

            
           
        }

        private List<TlvPackageData> BuildSafeData()
        {
           
           
            var lst=new List<TlvPackageData>();
            //终端入网认证编号（Tag 03，ans5）
            lst.Add(new TlvPackageData()
            {
                Tag = 0x3,
                Length = 5,
                Value = "P3104"
            });
            //设备类型（Tag 04，ans2）
            lst.Add(new TlvPackageData()
            {
                Tag = 0x4,
                Length = 2,
                Value = "02"
            });
            //终端序列号（Tag 05，ans..50）
            lst.Add(new TlvPackageData()
            {
                Tag = 0x5,
                Length = 16,
                Value = "000002023A297884"
            });
            ////加密随机因子（Tag 06，ans..10）
            //lst.Add(new TlvPackageData()
            //{
            //    Tag = 0x6,
            //    Length = 10,
            //    Value = "312".PadRight(10, ' ')
            //});
            ////序列号密文（Tag 07，b64）
            //lst.Add(new TlvPackageData()
            //{
            //    Tag = 0x7,
            //    Length = 64,
            //    Value = "".PadRight(64, '0')
            //});
            //应用程序版本号（Tag 08，ans8）；
            lst.Add(new TlvPackageData()
            {
                Tag = 0x8,
                Length = 8,
                Value = "170410".PadRight(8, ' ')
            });
            //var sbuilder=new StringBuilder();
            //foreach (var data in lst)
            //{
            //    SingleTlv(sbuilder, data);
            //}
            //return sbuilder.ToString();
            return lst;

        }
        public  void SingleTlv(StringBuilder sb, TlvPackageData data)
        {
            var f = BuildConfig.DefaultConfig.Packages[data.Tag];
            sb.Append(data.Tag.ToString("X2"));
            if (!f.OmitLength)
                sb.Append(data.Length.ToString("X2"));
            switch (f.Format)
            {
                case Format.BCD:
                    // BCD 奇数长度左补0
                    if (data.Length % 2 == 1)
                        sb.Append("0");
                    sb.Append(data.Value);
                    break;

                case Format.ASCII:
                    sb.Append(Encoding.Default.GetBytes(data.Value).Bytes2Hex());
                    break;

                case Format.Binary:
                    sb.Append(data.Value);
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }


        #endregion Tools
    }
}
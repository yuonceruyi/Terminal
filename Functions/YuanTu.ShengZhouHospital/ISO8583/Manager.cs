using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using YuanTu.Consts;
using YuanTu.Consts.FrameworkBase;
using YuanTu.ShengZhouHospital.ISO8583.CPUCard;
using YuanTu.ShengZhouHospital.ISO8583.Data;
using YuanTu.ShengZhouHospital.ISO8583.Enums;
using YuanTu.ShengZhouHospital.ISO8583.External;


namespace YuanTu.ShengZhouHospital.ISO8583
{
    public class Manager
    {
        private POS pos;
        private ICPOS icPos;
        public bool icMode { get; private set; }
        public readonly ICardDevice Wrapper;
        private List<CPUTlv> firstHalfList;

        public static IPAddress Address { get; set; }
        public static int Port { get; set; }

        public bool IsLogon
        {
            get { return pos.IsLogon; }
            set { pos.IsLogon = value; }
        }

        public int MainKeyIndex => pos.Config.MainKeyIndex;

        public POS POS => pos;

        public Func<byte[],byte[]> CalcMacFunc
        {
            get { return pos.CalcMacFunc; }
            set
            {
                pos.CalcMacFunc = value;
            }
        }

        public Func<byte[], byte[]> MacKeyEncrptyFunc
        {
            get { return pos.MacKeyEncrptyFunc; }
            set
            {
                pos.MacKeyEncrptyFunc = value;
            }

        }

        public Manager(string index = "")
        {

            pos = new POS(index)
            {
                Connection = new Connection()
                {
                    Address = Address,
                    Port = Port,
                },
            };
            Wrapper = new ACT_A6_Wrapper();
            icPos = new ICPOS(Wrapper);
        }

        /// <summary>
        /// 格式读取
        /// </summary>
        /// <returns></returns>
        public static Result LoadConfig()
        {
            #region Fields

            var fields = new List<Field>
            {
                new Field
                {
                    Id = 1,
                    Name = "Bitmap",
                    Length = 64,
                    Format = Format.Binary,
                    VarType = VarType.Fixed
                },
                new Field
                {
                    Id = 2,
                    Name = "PAN",
                    Length = 19,
                    Format = Format.BCD,
                    VarType = VarType.LLVar
                },
                new Field
                {
                    Id = 3,
                    Name = "ProcCode",
                    Length = 6,
                    Format = Format.BCD,
                    VarType = VarType.Fixed
                },
                new Field
                {
                    Id = 4,
                    Name = "TranAmt",
                    Length = 12,
                    Format = Format.BCD,
                    VarType = VarType.Fixed
                },
                new Field
                {
                    Id = 7,
                    Name = "TranDtTm",
                    Length = 10,
                    Format = Format.ASCII,
                    VarType = VarType.Fixed
                },
                new Field
                {
                    Id = 11,
                    Name = "AcqSsn",
                    Length = 6,
                    Format = Format.BCD,
                    VarType = VarType.Fixed
                },
                new Field
                {
                    Id = 12,
                    Name = "LTime",
                    Length = 6,
                    Format = Format.BCD,
                    VarType = VarType.Fixed
                },
                new Field
                {
                    Id = 13,
                    Name = "LDate",
                    Length = 4,
                    Format = Format.BCD,
                    VarType = VarType.Fixed
                },
                new Field
                {
                    Id = 14,
                    Name = "卡有效期",
                    Length = 4,
                    Format = Format.BCD,
                    VarType = VarType.Fixed
                },
                new Field
                {
                    Id = 15,
                    Name = "SettDate",
                    Length = 4,
                    Format = Format.ASCII,
                    VarType = VarType.Fixed
                },
                new Field
                {
                    Id = 18,
                    Name = "MercType",
                    Length = 4,
                    Format = Format.ASCII,
                    VarType = VarType.Fixed
                },
                new Field
                {
                    Id = 22,
                    Name = "EntrMode",
                    Length = 3,
                    Format = Format.BCD,
                    VarType = VarType.Fixed
                },
                new Field
                {
                    Id = 23,
                    Name = "CardSNum",
                    Length = 3,
                    Format = Format.BCD,
                    VarType = VarType.Fixed
                },
                new Field
                {
                    Id = 25,
                    Name = "CondMode",
                    Length = 2,
                    Format = Format.BCD,
                    VarType = VarType.Fixed
                },
                new Field
                {
                    Id = 26,
                    Name = "PinCode",
                    Length = 2,
                    Format = Format.ASCII,
                    VarType = VarType.Fixed
                },
                new Field
                {
                    Id = 32,
                    Name = "AcqInst",
                    Length = 11,
                    Format = Format.ASCII,
                    VarType = VarType.LLVar
                },
                new Field
                {
                    Id = 33,
                    Name = "ForwInst",
                    Length = 11,
                    Format = Format.ASCII,
                    VarType = VarType.LLVar
                },
                new Field
                {
                    Id = 35,
                    Name = "Trck2Dat",
                    Length = 37,
                    Format = Format.BCD,
                    VarType = VarType.LLVar
                },
                new Field
                {
                    Id = 36,
                    Name = "Trck3Dat",
                    Length = 104,
                    Format = Format.BCD,
                    VarType = VarType.LLLVar
                },
                new Field
                {
                    Id = 37,
                    Name = "IndexNum",
                    Length = 12,
                    Format = Format.ASCII,
                    VarType = VarType.Fixed
                },
				new Field
                {
                    Id = 38,
                    Name = "授权码",
                    Length = 6,
                    Format = Format.ASCII,
                    VarType = VarType.Fixed
                },
                new Field
                {
                    Id = 39,
                    Name = "RespCode",
                    Length = 2,
                    Format = Format.ASCII,
                    VarType = VarType.Fixed
                },
                new Field
                {
                    Id = 41,
                    Name = "TermCode",
                    Length = 8,
                    Format = Format.ASCII,
                    VarType = VarType.Fixed
                },
                new Field
                {
                    Id = 42,
                    Name = "MercCode",
                    Length = 15,
                    Format = Format.ASCII,
                    VarType = VarType.Fixed
                },
                new Field
                {
                    Id = 43,
                    Name = "MercNmAd",
                    Length = 40,
                    Format = Format.ASCII,
                    VarType = VarType.Fixed
                },
                new Field
                {
                    Id = 44,
                    Name = "AddiData_ABC",
                    Length = 600,
                    Format = Format.ASCII,
                    VarType = VarType.LLVar
                },
                new Field
                {
                    Id = 48,
                    Name = "AddiData",
                    Length = 600,
                    Format = Format.ASCII,
                    VarType = VarType.LLLVar
                },
                new Field
                {
                    Id = 49,
                    Name = "TranCurr",
                    Length = 3,
                    Format = Format.ASCII,
                    VarType = VarType.Fixed
                },
                new Field
                {
                    Id = 52,
                    Name = "PinData",
                    Length = 64,
                    Format = Format.Binary,
                    VarType = VarType.Fixed
                },
                new Field
                {
                    Id = 53,
                    Name = "CtrlInfo",
                    Length = 16,
                    Format = Format.ASCII,
                    VarType = VarType.Fixed
                },
                new Field
                {
                    Id = 55,
                    Name = "ICCData",
                    Length = 40,
                    Format = Format.Hex,
                    VarType = VarType.LLLVar
                },
                new Field
                {
                    Id = 60,
                    Name = "操作员号",
                    Length = 1,
                    Format = Format.Hex,
                    VarType = VarType.LLLVar
                },
                new Field
                {
                    Id = 61,
                    Name = "IdentNum",
                    Length = 80,
                    Format = Format.Hex,
                    VarType = VarType.LLLVar
                },
                new Field
                {
                    Id = 62,
                    Name = "BCSCData",
                    Length = 80,
                    Format = Format.ASCII,
                    VarType = VarType.LLLVar
                }, new Field
                {
                    Id = 63,
                    Name = "终端工作参数",
                    Length = 80,
                    Format = Format.Hex,
                    VarType = VarType.LLLVar
                },
                 new Field
                {
                    Id = 64,
                    Name = "信息验证码",
                    Length = 64,
                    Format = Format.Binary,
                    VarType = VarType.Fixed
                },
                new Field
                {
                    Id = 90,
                    Name = "OrigData",
                    Length = 42,
                    Format = Format.ASCII,
                    VarType = VarType.Fixed
                },
                new Field
                {
                    Id = 100,
                    Name = "DestInst",
                    Length = 11,
                    Format = Format.ASCII,
                    VarType = VarType.LLVar
                },
                new Field
                {
                    Id = 128,
                    Name = "MAC",
                    Length = 64,
                    Format = Format.Binary,
                    VarType = VarType.Fixed
                }
            };

            #endregion Fields

            #region Packages

            var packages = new List<TlvPackage>
            {
                new TlvPackage
                {
                    Tag = 0x1F21,
                    Length = 3,
                    Format = Format.BCD
                },
                new TlvPackage
                {
                    Tag = 0x2F01,
                    Length = 30,
                    Format = Format.ASCII
                },
                new TlvPackage
                {
                    Tag = 0x2F05,
                    Length = 60,
                    Format = Format.ASCII
                },
                new TlvPackage
                {
                    Tag = 0x2F14,
                    Length = 60,
                    Format = Format.ASCII
                }
            };

            #endregion Packages

            #region BuildConfig

            BuildConfig.DefaultConfig = new BuildConfig
            {
                BitmapLength = 64,
                MACFieldId = 64,
                VarFormat = VarFormat.BCD,
                LengthFormat = VarFormat.BCD,
                MTIFormat = VarFormat.BCD,
                OmitHead = true,
                OmitTPDU = false,
                Fields = fields.ToDictionary(one => one.Id),
                Packages = packages.ToDictionary(one => one.Tag)
            };

            #endregion BuildConfig

            #region Config

            //Config.DefaultConfig = new Config
            //{
            //    TerminalId = "20292029",
            //    MerchantId = "123456789012316",
            //    AcqInst = "81785800",
            //    Field_2F01 = "086123456789012316"
            //};

            #endregion Config

            CPUDecoder.TagNames = LoadTagsDevice();

            ICCard.TagNames = CPUDecoder.TagNames;

            return Result.Success();
        }

        public static Dictionary<int, string> LoadTagsDevice()
        {
            var dic = new Dictionary<int, string>();
            using (var sr = new StreamReader(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "CurrentResource", FrameworkConst.HospitalId, "Tags.csv"), Encoding.Default))
            {
                sr.ReadLine();
                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    var list = line.Split(',');
                    var tag = list[1];
                    var tagBytes = tag.Hex2Bytes();
                    int tagValue = tagBytes[0];
                    if (tagBytes.Length > 1)
                        tagValue = tagValue*0x100 + tagBytes[1];
                    dic[tagValue] = list[0];
                }
            }
            return dic;
        }

        /// <summary>
        /// CA 参数 下载
        /// </summary>
        /// <returns></returns>
        public Result Initialize()
        {
            var pubKey = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "CurrentResource",
                FrameworkConst.HospitalId, "PubKey.json");
            var Params= Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "CurrentResource",
                FrameworkConst.HospitalId, "Params.json");
            if (!(File.Exists(pubKey) && File.Exists(Params)))
            {
                var downres = pos.DownloadICParams();
                if (!downres.IsSuccess)
                {
                    return Result.Fail("IC卡终端参数下载失败");
                }
                var pubparam = pos.DownloadICPublicKey(downres.Value);
                if (!pubparam.IsSuccess)
                {
                    return Result.Fail("IC卡终端公钥下载失败");
                }
                var aidparam = pos.DownloadAid(downres.Value);
                if (!aidparam.IsSuccess)
                {
                    return Result.Fail("IC卡终端AID下载失败");
                }
            }
            var listPubKeys = TLVStorage.Load(pubKey);
            ICPOS.PublicKeys = TLVStorage.LoadTLVDics(listPubKeys);
            var listParams = TLVStorage.Load(Params);
            ICPOS.Params = TLVStorage.LoadTLVDics(listParams);
           
            return Result.Success();
        }

        public Result ReadCard(Input input)
        {
            CardPos pos;
            var ret = Wrapper.CheckCard(out pos);

            if (pos != CardPos.停卡位 && pos != CardPos.IC位)
                return Result.Fail("未检测到卡");

            if (pos == CardPos.停卡位)
            {
                ret = Wrapper.MoveCard();
                if(!ret)
                    return Result.Fail("移动卡失败");
            }
           byte[] apdu;
            string track2, track3;
            icMode = Wrapper.CPUCodeReset(out apdu);
            //#if DEBUG
            //            track2 = "6228480328843248274=24092204427970000";
            //            track3 = "996228480328843248274=156156000000000000000000000021414142409==000000000000=000000000000=008565000000000";
            //#endif

            if (!icMode)//不是IC卡，开始读磁条
            {
                ret = Wrapper.ReadTracks(out track2, out track3);
                if (!ret)
                    return Result.Fail("读磁轨失败");
                input.Track2 = track2;
                input.Track3 = track3;
                int n = track2.IndexOf('=');
                if (n < 0)
                    return Result.Fail("读磁轨失败或该卡不是银行卡，请重试或更换卡");
                input.BankNo = track2.Substring(0, n);
                return Result.Success();
            }

            ret = icPos.ReadCard(input);

            if (!ret)
                return Result.Fail("读IC卡信息失败");

            return Result.Success();
        }

        public Result<byte[]> DoLogon()
        {
            return pos.DoLogon();
        }

        public Result<Output> DoSale(Input input)
        {
            input.Now=DateTimeCore.Now;
            if (!icMode)
                return pos.DoSale(input);
            
            firstHalfList = icPos.FirstHalf();
            if(firstHalfList == null)
                return Result<Output>.Fail("IC卡处理失败");

            var doRes = pos.DoSaleIC(input, firstHalfList);
            if (!doRes.IsSuccess)
            {
                icPos.OnlineFail();
                return Result<Output>.Fail(doRes.Message);
            }
            var output = doRes.Value;
            if (!icPos.SecondHalf(output))
            {
                var reRes = pos.DoReverseIC();
                if (reRes.IsSuccess)
                    return Result<Output>.Fail("IC卡确认失败 冲正成功");
                return Result<Output>.Fail("IC卡确认失败 冲正失败");
            }
            if (output.Notify)
            {
                var noRes = pos.DoNotifyIC(input, icPos.MakeNotifyList());
                Console.WriteLine("DoNotifyIC "+(noRes.IsSuccess ? "Done":"Failed"));
            }
            {
                var upRes = pos.DoUploadIC(input,output, icPos.MakeUploadList());
                Console.WriteLine("DoUploadIC "+(upRes.IsSuccess ? "Done":"Failed"));
            }
            return Result<Output>.Success(output);
        }

        public Result<Output> DoReverse()
        {
            if (icMode)
                return pos.DoReverseIC();

            return pos.DoReverse();
        }

        public Result<string> DoDownloadMasterKey(string sn)
        {
            //return pos.DoDownloadMasterKey(sn);
            return Result<string>.Fail("");
        }
    }
}

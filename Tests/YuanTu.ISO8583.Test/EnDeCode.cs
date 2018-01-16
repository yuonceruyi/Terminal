using System;
using System.Collections.Generic;
using System.Net;
using YuanTu.ISO8583.Data;
using YuanTu.ISO8583.Enums;
using YuanTu.ISO8583.IO;
using YuanTu.ISO8583.Util;

namespace YuanTu.ISO8583.Test
{
    public class EnDeCode
    {
        public static void Run()
        {
            //var encoder = new Encoder();
            //var decoder = new Decoder();

            //var c = new Connection
            //{
            //    Address = IPAddress.Parse("59.41.103.97"),
            //    Port = 18178
            //};
            //encoder.CalcMacFunc = bytes => new byte[8];
            //DoSale(encoder);
            //DoSaleRes(encoder);
            //DoReverse(encoder);
            //DoReverseRes(encoder);
            //DoLogon(encoder);
            //DoLogonRes(encoder);
            //DoDownKey(encoder);
            //DoDownKeyRes(encoder);
            //DoICUpload(encoder);
            //DoICUploadRes(encoder);
            //DoScriptNotify(encoder);
            //DoDownPublicKey(encoder);
            //var send = encoder.Encode();
            //var send ="3031343230383130A220000102C1080000000000100000003936303832373132313731353135333030303035303130383831373835383030303032303239323032393132333435363738393031323331363033364135373933383543414639394546354436423145344346374439323935433631463437383230303030303030303030303030303030383831373835383030"
            //        .Hex2Bytes();
            //decoder.Decode(send);
            //Console.WriteLine(Connection.Print(send));

            //var res = c.Handle(send);
            //if (res.IsSuccess)
            //{
            //    var message = decoder.Decode(res.Value);
            //}
        }

        #region Examples

        public static void DoSale(Encoder encoder)
        {
            encoder.MessageType = "0200";
            encoder.Values = new Dictionary<int, string>
            {
                [1] = "",
                [2] = "6225880209237727",
                [3] = "430350",
                [4] = "000000001642",
                [7] = "0103113740",
                [11] = "000462",
                [12] = "113740",
                [13] = "0103",
                [18] = "4900",
                [22] = "021",
                [25] = "00",
                [26] = "12",
                [32] = "81785800",
                [33] = "81785800",
                [35] = "6225880209237727=49121200543100336958",
                [36] =
                    "996225880209237727=1561560500050000001015336958214000049120=0209237727=000000000=02000000020000000000000",
                [41] = "20292020",
                [42] = "123456789012316",
                [48] = "",
                [49] = "156",
                [52] = "A6DBFC984AD79E5B",
                [53] = "2600000000000000",
                [128] = "3132384130413932"
            };
            encoder.Packages = new List<TlvPackageData>
            {
                new TlvPackageData
                {
                    Tag = 0x1F21,
                    Length = 3,
                    Value = "350"
                },
                new TlvPackageData
                {
                    Tag = 0x2F01,
                    Length = 3,
                    Value = "004"
                },
                new TlvPackageData
                {
                    Tag = 0x2F05,
                    Length = 0x3A,
                    Value = "350|004|          1004025811201205   1          EC11111716"
                }
            };
        }

        public static void DoSaleRes(Encoder encoder)
        {
            encoder.MessageType = "0210";
            encoder.Values = new Dictionary<int, string>
            {
                [1] = "",
                [2] = "6225000000000030",
                [3] = "430350",
                [4] = "000000032000",
                [7] = "0611142558",
                [11] = "000563",
                [12] = "142606",
                [13] = "0611",
                [15] = "0615",
                [18] = "8901",
                [23] = "003",
                [25] = "00",
                [32] = "48025800",
                [33] = "81785800",
                [37] = "733225733225",
                [39] = "00",
                [41] = "20292078",
                [42] = "123456789012316",
                [43] = "普通一机一密测试商户(ZHANG)",
                [48] = "",
                [49] = "156",
                [55] = "9F360217CC72168609841800000474D6C3EE860984240000043A3829D4910A8A4240F5EDE8D39D3030",
                [100] = "00012900",
                [128] = "3845343541464241"
            };
            encoder.Packages = new List<TlvPackageData>
            {
                new TlvPackageData
                {
                    Tag = 0x1F21,
                    Length = 3,
                    Value = "350"
                },
                new TlvPackageData
                {
                    Tag = 0x2F14,
                    Length = 0x3F,
                    Value = "普通一机一密测试商户(ZHANG)             12345678901231647692056"
                }
            };
        }

        public static void DoReverse(Encoder encoder)
        {
            encoder.MessageType = "0420";
            encoder.Values = new Dictionary<int, string>
            {
                [1] = "",
                [2] = "6225000000000063",
                [3] = "430350",
                [4] = "000000000001",
                [7] = "1031114139",
                [11] = "000004",
                [12] = "114139",
                [13] = "1031",
                [18] = "8901",
                [22] = "051",
                [23] = "006",
                [25] = "00",
                [32] = "81785800",
                [33] = "81785800",
                [41] = "20292026",
                [42] = "123456789012316",
                [48] = "",
                [49] = "156",
                [55] =
                    "9F3303604000950580080408009F3704000E0E039F1E0820202020202020209F100807000103A08000019F2608A521E5EBDABB654F9F3602004382025C009F1A0201569A03141031",
                [60] = "2200000100051",
                [90] = "020000000210311140150008178580000081785800",
                [128] = "3138393235374535"
            };
            encoder.Packages = new List<TlvPackageData>
            {
                new TlvPackageData
                {
                    Tag = 0x1F21,
                    Length = 3,
                    Value = "350"
                },
                new TlvPackageData
                {
                    Tag = 0x2F01,
                    Length = 3,
                    Value = "024"
                },
                new TlvPackageData
                {
                    Tag = 0x2F05,
                    Length = 0x0C,
                    Value = "201410271545"
                }
            };
        }

        public static void DoReverseRes(Encoder encoder)
        {
            encoder.MessageType = "0430";
            encoder.Values = new Dictionary<int, string>
            {
                [1] = "",
                [2] = "6225000000000063",
                [3] = "430350",
                [4] = "000000000001",
                [7] = "1031114139",
                [11] = "000004",
                [12] = "114139",
                [13] = "1031",
                [15] = "1031",
                [18] = "8901",
                [23] = "006",
                [25] = "00",
                [32] = "81785800",
                [33] = "81785800",
                [37] = "329551329551",
                [39] = "00",
                [41] = "20292026",
                [42] = "123456789012316",
                [43] = "非税测试商户",
                [48] = "",
                [49] = "156",
                [55] =
                    "9F3303604000950580080408009F3704000E0E039F1E0820202020202020209F100807000103A08000019F2608A521E5EBDABB654F9F3602004382025C009F1A0201569A03141031",
                [60] = "4013",
                [100] = "00012900",
                [128] = "3138393235374535"
            };
            encoder.Packages = new List<TlvPackageData>
            {
                new TlvPackageData
                {
                    Tag = 0x1F21,
                    Length = 3,
                    Value = "350"
                }
            };
        }

        public static void DoLogon(Encoder encoder)
        {
            encoder.MessageType = "0800";
            encoder.Values = new Dictionary<int, string>
            {
                [1] = "",
                [3] = "000000",
                [7] = "0212145642",
                [11] = "000339",
                [32] = "81785800",
                [41] = "20292029",
                [42] = "123456789012316",
                [53] = "2600000000000000"
            };
        }

        public static void DoLogonRes(Encoder encoder)
        {
            encoder.MessageType = "0810";
            encoder.Values = new Dictionary<int, string>
            {
                [1] = "",
                [3] = "000000",
                [7] = "0212144534",
                [11] = "000339",
                [32] = "81785800",
                [39] = "00",
                [41] = "20292020",
                [42] = "123456789012316",
                [48] = "57C987B3D6A3492EE9E5F24A5285D8B457345E4E7EB1531651AD68C01EA150A944867522",
                [53] = "2600000000000000",
                [100] = "81785800"
            };
        }

        public static void DoDownKey(Encoder encoder)
        {
            encoder.BuildConfig.Fields[48] = new Field
            {
                Id = 48,
                Name = "AddiData",
                Length = 600,
                Format = Format.Binary,
                VarType = VarType.LLLVar
            };
            encoder.MessageType = "0800";
            encoder.Values = new Dictionary<int, string>
            {
                [1] = "",
                [3] = "960827",
                [7] = "1203114708",
                [11] = "114708",
                [32] = "81785800",
                [41] = "20292081",
                [42] = "123456789012316",
                [48] = "B5BEA6E1A070940A",
                [53] = "2000000000000000"
            };
        }

        public static void DoDownKeyRes(Encoder encoder)
        {
            encoder.MessageType = "0810";
            encoder.Values = new Dictionary<int, string>
            {
                [1] = "",
                [3] = "960827",
                [7] = "1208113536",
                [11] = "000501",
                [32] = "81785800",
                [39] = "00",
                [41] = "20292029",
                [42] = "123456789012316",
                [48] = "A579385CAF99EF5D6B1E4CF7D9295C61F478",
                [53] = "2000000000000000",
                [100] = "81785800"
            };
        }

        public static void DoICUpload(Encoder encoder)
        {
            encoder.MessageType = "0320";
            encoder.Values = new Dictionary<int, string>
            {
                [1] = "",
                [2] = "6225000000000030",
                [3] = "203000",
                [4] = "000000032000",
                [7] = "0616161805",
                [11] = "773620",
                [12] = "161805",
                [13] = "0616",
                [22] = "021",
                [23] = "003",
                [32] = "80925800",
                [41] = "20292041",
                [42] = "123456789012316",
                [55] =
                    "9F2608C5849F848B7AA8A19F2701809F100807000103A00800019F3704020530379F36020077950500000008009A031506169C01009F02060000000000005F2A02015682027D009F1A0201569F03060000000000009F33036048209F34033F00009F3501149F1E0841424344313233348408A0000003330101039F090200209F410400000360",
                [60] = "0077362120351",
                [62] = "0000000000032000000"
            };
        }

        public static void DoICUploadRes(Encoder encoder)
        {
            encoder.MessageType = "0330";
            encoder.Values = new Dictionary<int, string>
            {
                [1] = "",
                [2] = "6225000000000030",
                [3] = "203000",
                [4] = "000000032000",
                [11] = "773620",
                [12] = "162114",
                [13] = "0616",
                [32] = "80925800",
                [37] = "001122334455",
                [39] = "00",
                [41] = "20292041",
                [42] = "123456789012316",
                [60] = "0077362120351",
                [128] = "3042353244453642"
            };
        }

        public static void DoScriptNotify(Encoder encoder)
        {
            encoder.MessageType = "0620";
            encoder.Values = new Dictionary<int, string>
            {
                [1] = "",
                [2] = "6225000000000022",
                [3] = "951000",
                [4] = "000000008900",
                [11] = "002314",
                [22] = "051",
                [23] = "001",
                [32] = "81785800",
                [37] = "360099360099",
                [41] = "20292029",
                [42] = "123456789012316",
                [49] = "156",
                [55] =
                    "9F3303E0E1C8950500000498009F370401E3A9429F1E08922A4028A735408C9F100807000103600002019F260843F4F9CB2EE1D4749F360201AB82027D009F1A0201569A03140915",
                [60] = "0000000000050",
                [61] = "0000000023110915",
                [90] = "020000231109151610370008178580000081785800",
                [128] = "3846353336373244"
            };
            //encoder.ICPackages
        }

        public static void DoDownPublicKey(Encoder encoder)
        {
            encoder.MessageType = "0820";
            encoder.Values = new Dictionary<int, string>
            {
                [1] = "",
                [3] = "372000",
                [41] = "20292029",
                [42] = "123456789012316",
                [60] = "00000001372",
                [62] = "313030"
            };
        }

        public static void DoDownPublicKey2(Encoder encoder)
        {
            encoder.MessageType = "0820";
            encoder.Values = new Dictionary<int, string>
            {
                [1] = "",
                [3] = "382000",
                [41] = "20292029",
                [42] = "123456789012316",
                [60] = "00000001382",
                [62] = "313030"
            };
        }

        public static void DoDownPublicKey3(Encoder encoder)
        {
            encoder.MessageType = "0800";
            encoder.Values = new Dictionary<int, string>
            {
                [1] = "",
                [3] = "372000",
                [41] = "20292029",
                [42] = "123456789012316",
                [60] = "00000001372",
                [62] = "313030"
            };
        }

        #endregion Examples
    }
}
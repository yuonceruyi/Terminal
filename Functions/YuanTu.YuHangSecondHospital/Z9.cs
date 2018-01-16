using System;
using System.Linq;
using System.Text;
using YuanTu.Consts.FrameworkBase;
using YuanTu.Core.Log;
using YuanTu.ISO8583.Util;
using YuanTu.Core.Extension;

namespace YuanTu.YuHangSecondHospital
{
    public class Z9
    {
        public static int Port { get; set; } = 100;

        private static int _handle = 0;

        public static Result Open()
        {
            var handle = UnSafeMethods.IC_InitComm(Port);
            if (handle <= 0)
                return Result.Fail(handle.ToString());
            _handle = handle;
            return Result.Success();
        }

        public static Result Close()
        {
            var ret = UnSafeMethods.IC_ExitComm(_handle);
            if (ret != 0) return Result.Fail(ret.ToString());
            _handle = 0;
            return Result.Success();
        }

        public static Result<byte[]> Reset()
        {
            byte len = 255;
            var data = new byte[len];
            var ret = UnSafeMethods.IC_CpuReset(_handle, ref len, data);
            if (ret == 0)
            {
                return Result<byte[]>.Success(data.Take(len).ToArray());
            }
            return Result<byte[]>.Fail("");
        }

        public static Result<HiInfo> ReadData()
        {
            var info = new HiInfo();

            var selectResult = CpuApdu("00a404000f7378312e73682ec9e7bbe1b1a3d5cf", false);
            if (!selectResult.IsSuccess)
                goto Error;

            var selectResult2 = CpuApdu("00a4000002EF05", false);
            if (!selectResult2.IsSuccess)
                goto Error;

            var cardIdResult = CpuApdu("00b2010010", false);
            if (!cardIdResult.IsSuccess)
                goto Error;
            var cardId = cardIdResult.Value;

            var cardTypeResult = CpuApdu("00b2020001", true);
            if (!cardTypeResult.IsSuccess)
                goto Error;
            var cardType = cardTypeResult.Value;

            var versionResult = CpuApdu("00b2030004", true);
            if (!versionResult.IsSuccess)
                goto Error;
            var version = versionResult.Value;

            var issuerResult = CpuApdu("00b204000c", false);
            if (!issuerResult.IsSuccess)
                goto Error;
            var issuer = issuerResult.Value;

            var selectResult3 = CpuApdu("00b2050004", false);
            if (!selectResult3.IsSuccess)
                goto Error;

            var validDateResult = CpuApdu("00b2060004", false);
            if (!validDateResult.IsSuccess)
                goto Error;
            var validDate = validDateResult.Value;

            var cardNoResult = CpuApdu("00b2070009", true);
            if (!cardNoResult.IsSuccess)
                goto Error;
            var cardNo = cardNoResult.Value;

            var issueDateResult = CpuApdu("00a4000002ef06", true);
            if (!issueDateResult.IsSuccess)
                goto Error;
            var issueDate = issueDateResult.Value;

            var idNoResult = CpuApdu("00b2080018", true);
            if (!idNoResult.IsSuccess)
                goto Error;
            var idNo = idNoResult.Value;

            var nameResult = CpuApdu("00b209001e", true);
            if (!nameResult.IsSuccess)
                goto Error;
            var name = nameResult.Value;

            var genderResult = CpuApdu("00b20a0001", true);
            if (!genderResult.IsSuccess)
                goto Error;
            var gender = genderResult.Value;

            var nationResult = CpuApdu("00b20b0001", false);
            if (!nationResult.IsSuccess)
                goto Error;
            var nation = nationResult.Value;

            var birthPlaceResult = CpuApdu("00b20c0003", false);
            if (!birthPlaceResult.IsSuccess)
                goto Error;
            var birthPlace = birthPlaceResult.Value;

            var birthdayResult = CpuApdu("00b20d0004", false);
            if (!birthdayResult.IsSuccess)
                goto Error;
            var birthday = birthdayResult.Value;

            info.卡识别码 = cardId;
            info.统筹区码 = cardId.Substring(0, 6);
            info.卡类别 = cardType;
            info.规范版本 = version;
            info.发卡机构 = issuer;
            info.发卡日期 = issueDate;
            info.卡有效期 = validDate;
            info.卡号 = cardNo;
            info.身份证号 = idNo;
            info.姓名 = name.Trim(' ', '\0');
            if (gender == "1")
                info.性别 = "男";
            else if (gender == "2")
                info.性别 = "女";
            else
                info.性别 = "未知";
            info.民族 = nation;
            info.出生地 = birthPlace;
            info.出生日期 = birthday;
            Logger.Device.Info($"[社保卡个人信息]\n{info.ToJsonString()}");
            return Result<HiInfo>.Success(info);

            Error:
            return Result<HiInfo>.Fail("读卡出错，请重新插卡");
        }

        private static Result<string> CpuApdu(string cmdData, bool ascii)
        {
            var result = CpuApduClean(cmdData);
            if (!result.IsSuccess)
                return Result<string>.Fail(result.Message);
            var res = result.Value;
            var resHex = res.Bytes2Hex();
            if (!resHex.EndsWith("9000"))
            {
                if (res.Length == 2 && res[0] == 0x6C)
                {
                    var newCmdData = $"{cmdData.Substring(0, cmdData.Length - 2)}{res[1]:X2}";
                    return CpuApdu(newCmdData, ascii);
                }
                if (res.Length == 2 && res[0] == 0x61)
                {
                    var newCmdData = $"00C00000{res[1]:X2}";
                    var newResult = CpuApduClean(newCmdData);
                    if (!newResult.IsSuccess)
                        return Result<string>.Fail(newResult.Message);
                    res = newResult.Value;
                    resHex = res.Bytes2Hex();
                    if (!resHex.EndsWith("9000"))
                        return Result<string>.Fail(resHex);
                }
                else
                {
                    return Result<string>.Fail(resHex);
                }
            }
            if (ascii)
            {
                if (res.Length == 2)
                    return Result<string>.Success("");
                var value = Encoding.Default.GetString(res, 2, res[1]);
                Console.WriteLine($"Result: [{res.Bytes2Hex()}] {value}");
                return Result<string>.Success(value);
            }
            else
            {
                if (res.Length == 2)
                    return Result<string>.Success("9000");
                var value = res.Bytes2Hex(2, res[1]);
                Console.WriteLine($"Result: {value}");
                return Result<string>.Success(value);
            }
        }

        private static Result<byte[]> CpuApduClean(string cmdData)
        {
            Console.WriteLine($"CpuApdu <= {cmdData}");
            var reqData = cmdData.Hex2Bytes();
            byte len = 255;
            var resData = new byte[len];
            var ret = UnSafeMethods.IC_CpuApdu(_handle, (byte) reqData.Length, reqData, ref len, resData);
            if (ret != 0)
            {
                Console.WriteLine($"CpuApdu => Fail [{ret}]");
                return Result<byte[]>.Fail(ret.ToString());
            }
            var array = resData.Take(len).ToArray();
            Console.WriteLine($"CpuApdu => [{len}] {array.Bytes2Hex()}");
            return Result<byte[]>.Success(array);
        }

    }

    public class HiInfo
        {
            public string 出生地 { get; set; }

            public string 出生日期 { get; set; }

            public string 发卡机构 { get; set; }

            public string 发卡日期 { get; set; }

            public string 规范版本 { get; set; }

            public string 卡号 { get; set; }

            public string 卡类别 { get; set; }

            public string 卡识别码 { get; set; }

            public string 卡有效期 { get; set; }

            public string 民族 { get; set; }
            public string 身份证号 { get; set; }
            public string 统筹区码 { get; set; }
            public string 性别 { get; set; }
            public string 姓名 { get; set; }
            public string 非接卡序列号 { get; set; }
            public string 非接卡序列号Hex { get; set; }

            public string OldNumber { get; set; }
        
    }
}
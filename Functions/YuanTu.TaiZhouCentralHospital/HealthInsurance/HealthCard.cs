using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using YuanTu.Consts.FrameworkBase;
using YuanTu.Core.Extension;
using YuanTu.Core.Log;

namespace YuanTu.TaiZhouCentralHospital.HealthInsurance
{
    public static class HealthCard
    {
        private const string DllPathSmkJy = "SMK_JY.dll";

        private static readonly Dictionary<int, string> ErrCode = new Dictionary<int, string>
        {
            {0, "成功"},
            {-200001, "打开读卡器端口失败"},
            {-200003, "读卡器端口无效"},
            {-200010, "寻卡失败"},
            {-200011, "寻卡成功，获取卡序"},
            {-200015, "卡类型无效"},
            {-200021, "CPU卡写卡号与输入"},
            {-200031, "CPU（PSAM）卡上电复"},
            {-200032, "CPU（PSAM）卡读卡失"},
            {-200051, "M1卡扇区密钥无效"},
            {-200053, "M1卡读块数据失败"},
            {-200054, "M1卡写块数据失败"},
            {-200055, "M1卡第一次处理密钥"},
            {-200056, "M1卡第二次处理密钥"},
            {-200057, "M1卡读认证失败"},
            {-200058, "M1卡写认证失败"},
            {-200059, "M1卡写卡参数无效"},
            {-200060, "M1卡写卡号与输入卡"},
            {-200061, "M1卡写块数据失败"},
            {-200091, "PSAM卡设置卡座失败"},
            {-200092, "PSAM卡上电复位失败"},
            {-200093, "PSAM卡设置参数失败"},
            {-200094, "PSAM卡获取公共信息"},
            {-200095, "PSAM卡获取终端机编"},
            {-200096, "PSAM卡获取应用公共"},
            {-21, "IC通讯失败"},
            {-22, "读卡器函数返回失败"},
            {-230001, "读卡器返回数据不足两字节"},
            {-5, "读卡器通讯成功但返回失败"},
            {-290001, "读卡器动态库文件不存在或无效"},
            {-290002, "函数输入参数无效"}
        };

        [DllImport(DllPathSmkJy, EntryPoint = "ReadCitizenTreatmentCardInfo", CharSet = CharSet.Ansi)]
        public static extern int ReadCitizenTreatmentCardInfo(StringBuilder outData);

        [DllImport(DllPathSmkJy, EntryPoint = "WriteTreatmentCardInfo", CharSet = CharSet.Ansi)]
        public static extern int WriteTreatmentCardInfo(StringBuilder inData);

        public static Result<OutData> ReadCitizenCard()
        {
            try
            {
                var sb = new StringBuilder(1024);
                var res = ReadCitizenTreatmentCardInfo(sb);
                Logger.Device.Info($"读取健康卡,Dll路径:{DllPathSmkJy},返回值:{res},返回值定义:{GetErrorMsg(res)},返回数据:{sb}");
                if (res != 0)
                    return Result<OutData>.Fail($"读取健康卡失败,返回值:{res},返回值定义:{GetErrorMsg(res)}");
                //解析
                var outdata = Decode(sb.ToString());
                return outdata == null
                    ? Result<OutData>.Fail($"读取健康卡失败,返回值:{res},返回值定义:{GetErrorMsg(res)},返回数据:{sb}")
                    : Result<OutData>.Success(outdata);
            }
            catch (Exception ex)
            {
                Logger.Device.Error($"读取健康卡异常,Dll路径:{DllPathSmkJy},异常内容:{ex.Message}");
                return Result<OutData>.Fail($"读取健康卡异常,异常内容:{ex.Message}");
            }
        }

        public static Result WriteCitizenCard(InData inData)
        {
            try
            {
                var input = inData.MakeInput("|");
                var res = WriteTreatmentCardInfo(new StringBuilder(input));
                Logger.Device.Info($"写健康卡,Dll路径:{DllPathSmkJy},写入数据:{input},返回值:{res},返回值定义:{GetErrorMsg(res)}");
                return res != 0 ? Result.Fail($"写健康卡失败,写入数据:{input},返回值:{res},返回值定义:{GetErrorMsg(res)}") : Result.Success();
            }
            catch (Exception ex)
            {
                Logger.Device.Info($"写健康卡异常,Dll路径:{DllPathSmkJy},异常内容:{ex.Message}");
                return Result.Fail($"写健康卡异常,异常内容:{ex.Message}");
            }
        }

        public static OutData Decode(string orgInfo)
        {
            if (orgInfo.IsNullOrWhiteSpace())
                return null;
            if (!orgInfo.Contains('|'))
                return null;
            var arrInfo = orgInfo.SafeToSplit('|');
            var outData = new OutData().Decode(arrInfo);
            //var outData = new OutData
            //{
            //    物理卡号 = arrInfo[0],
            //    市民卡健康卡卡号 = arrInfo[1],
            //    启用标志 = arrInfo[2],
            //    姓名 = arrInfo[3],
            //    出生日期 = arrInfo[4],
            //    性别 = arrInfo[5],
            //    证件类型 = arrInfo[6],
            //    联系电话 = arrInfo[7],
            //    证件号码 = arrInfo[8],
            //    地址 = arrInfo[9],
            //    社保卡卡号 = arrInfo[10]
            //};
            if (outData.性别.IsNullOrWhiteSpace())
                if (outData.证件号码.Length >= 17)
                {
                    var sex = int.Parse(outData.证件号码.SafeSubstring(16, 1));
                    outData.性别 = sex % 2 == 0 ? "2" : "1";
                }
            if (outData.出生日期.IsNullOrWhiteSpace() || outData.出生日期.EndsWith("00"))
                if (outData.证件号码.Length >= 14)
                    outData.出生日期 = outData.证件号码.SafeSubstring(6, 8);
            return outData;
        }

        public static string GetErrorMsg(int resultCode)
        {
            return ErrCode.FirstOrDefault(p => resultCode.ToString().StartsWith(p.Key.ToString())).Value;
        }
    }

    public class OutData
    {
        public string 物理卡号 { get; set; }
        public string 市民卡健康卡卡号 { get; set; }
        public string 启用标志 { get; set; }
        public string 姓名 { get; set; }
        public string 出生日期 { get; set; }
        public string 性别 { get; set; }
        public string 证件类型 { get; set; }
        public string 联系电话 { get; set; }
        public string 证件号码 { get; set; }
        public string 地址 { get; set; }
        public string 社保卡卡号 { get; set; }
    }

    public class InData
    {
        public string 健康卡卡号 { get; set; }
        public string 姓名 { get; set; }
        public string 出生日期 { get; set; }
        public string 性别 { get; set; }
        public string 证件类型 { get; set; }
        public string 联系电话 { get; set; }
        public string 证件号码 { get; set; }
        public string 地址 { get; set; }
    }
}
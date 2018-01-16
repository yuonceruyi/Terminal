using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using YuanTu.Consts.FrameworkBase;
using YuanTu.Core.Extension;

namespace YuanTu.YiWuArea.Insurance.Models.Base
{
    /// <summary>
    /// 病人IC卡信息
    /// </summary>
    public class PatientIcData
    {
        private PatientIcData() { }
        /// <summary>
        /// 原始IC报文
        /// </summary>
        public string OringinContent { get; set; }
        [StringIndex(1,20)]
        public string 医疗证号 { get; set; }
        [StringIndex(21, 20)]
        public string 个人社保编号 { get; set; }
        [StringIndex(41, 3)]
        public string 险种 { get; set; }
        [StringIndex(44, 4)]
        public string 医保待遇 { get; set; }
        [StringIndex(48, 1)]
        public string 特殊病标志 { get; set; }
        [StringIndex(49, 18, StringType.GB11643)]
        public string 公民身份号 { get; set; }
        [StringIndex(67, 40)]
        public string 姓名 { get; set; }
        [StringIndex(107, 3, StringType.GB2261)]
        public string 性别 { get; set; }
        [StringIndex(110, 3, StringType.GB3304)]
        public string 民族 { get; set; }
        [StringIndex(113, 10, StringType.日期)]
        public string 出生日期 { get; set; }
        [StringIndex(123, 3)]
        public string 单位性质 { get; set; }
        [StringIndex(126, 40)]
        public string 单位名称 { get; set; }
        [StringIndex(166, 12, StringType.金额)]
        public string 当年帐户余额 { get; set; }
        [StringIndex(178, 12, StringType.金额)]
        public string 历年帐户余额 { get; set; }
        [StringIndex(190, 12, StringType.金额)]
        public string 当年住院医保累计 { get; set; }
        [StringIndex(202, 12, StringType.金额)]
        public string 当年门诊医保累计 { get; set; }
        [StringIndex(214, 12, StringType.金额)]
        public string 当年基本特殊病医保累计 { get; set; }
        [StringIndex(226, 12, StringType.金额)]
        public string 当年补充特殊病医保累计 { get; set; }
        [StringIndex(238, 12, StringType.金额)]
        public string 当年统筹支付累计 { get; set; }
        [StringIndex(250, 12, StringType.金额)]
        public string 当年大额支付累计 { get; set; }
        [StringIndex(262, 12, StringType.金额)]
        public string 当年补助支付累计 { get; set; }
        [StringIndex(274, 3,StringType.数字)]
        public string 当年住院次数 { get; set; }

        private static readonly List<PropertyInfo>CachePropertyInfos=new List<PropertyInfo>(); 
        public static Result<PatientIcData> Deserialize(string originstr)
        {
            var encoding = Encoding.GetEncoding("GBK");
            var bts = encoding.GetBytes(originstr);
            if (bts.Length < 276)
            {
                return Result<PatientIcData>.Fail("社保IC卡参数长度异常！");
            }
            var data=new PatientIcData()
            {
                OringinContent = originstr
            };
            if (!CachePropertyInfos.Any())
            {
                var members = data.GetType()
                    .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                    .Where(p => p.GetCustomAttribute<StringIndexAttribute>() != null)
                    .ToArray();
                CachePropertyInfos.AddRange(members);
            }

            foreach (var memberInfo in CachePropertyInfos)
            {
                var att = memberInfo.GetCustomAttribute<StringIndexAttribute>();
                var val = encoding.GetString(bts.Skip(att.Start-1).Take(att.Length).ToArray());//originstr.Substring(att.Start-1, att.Length);
                memberInfo.SetValue(data, DataFormatConvert(att.StringType,val));

            }
            return Result<PatientIcData>.Success(data);
        }

        private static string DataFormatConvert(StringType sType, string str)
        {
            switch (sType)
            {
                case StringType.字符:
                    break;
                case StringType.GB11643:
                    break;
                case StringType.GB2261:
                    break;
                case StringType.GB3304:
                    break;
                case StringType.日期:
                    return DateTime.ParseExact(str, "yyyy.MM.dd", null).ToString("yyyy-MM-dd");
                case StringType.金额:
                    return decimal.Parse(str.TrimStart(' ', '0').BackNotNullOrEmpty("0")).ToString("0.00");
                case StringType.数字:
                    return int.Parse(str.TrimStart(' ', '0').BackNotNullOrEmpty("0")).ToString();
                default:
                    break;
            }
            return str?.TrimEnd();
        }
    }


    internal class StringIndexAttribute : Attribute
    {
        internal StringIndexAttribute(int start, int len, StringType sType= StringType.字符)
        {
            Start = start;
            Length = len;
            StringType = sType;
        }
        /// <summary>
        /// 起始的位值（从1开始）
        /// </summary>
        public int Start { get; set; }
        public int Length { get; set; }
        public StringType StringType { get; set; }
       
    }

    internal enum StringType
    {
        字符,
        GB11643,
        GB2261,
        GB3304,
        日期,
        金额,
        数字
    }
}

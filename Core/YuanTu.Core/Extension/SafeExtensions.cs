using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YuanTu.Consts.Enums;

namespace YuanTu.Core.Extension
{
    public static class SafeExtensions
    {
        public static string SafeToAmPm(this int ampm)
        {
            switch (ampm)
            {
                case 0: return "全天";
                case 1: return "上午";
                case 2: return "下午";
                case 3: return "昼夜";
                default: return ampm.ToString();
            }
        }

        public static string SafeToAmPm(this string ampm)
        {
            int v = 0;
            if (int.TryParse(ampm, out v))
            {
                switch (v)
                {
                    case 0: return "全天";
                    case 1: return "上午";
                    case 2: return "下午";
                    case 3: return "昼夜";
                    default: return ampm;
                }
            }
            return ampm;
        }

        public static AmPmSession SafeToAmPmEnum(this string ampm)
        {
            AmPmSession ampmenum;
            Enum.TryParse(ampm, out ampmenum);
            return ampmenum;
        }

        public static Sex SafeToSex(this string sex, string[] man = null, string[] woman = null)
        {
            man = man ?? new[] {"男", "1"};
            woman = woman ?? new[] {"女", "2"};
            if (man.Contains(sex))
            {
                return Sex.男;
            }
            else if (woman.Contains(sex))
            {
                return Sex.女;
            }
            return Sex.未知;
        }

        public static string[] SafeToSplit(this string orgStr, char splitChar, int arrLen = 0)
        {
            if (orgStr == null) return new string[arrLen];
            var origin = orgStr.Split(splitChar);
            if (orgStr.Length < arrLen)
            {
                var newArr = new string[arrLen];
                Array.Copy(origin, newArr, origin.Length);

                return newArr;
            }
            return origin;
            // return !orgStr.Contains(splitChar) ? new string []{ orgStr } : orgStr.Split(splitChar);
        }

        public static string SafeSubstring(this string orgStr, int startIndex, int len)
        {
            if (orgStr == null) return null;
            return orgStr.Length >= (len + startIndex) ? orgStr.Substring(startIndex, len) : orgStr;
        }
    }

}

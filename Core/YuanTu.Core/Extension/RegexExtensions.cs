using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YuanTu.Core.Extension
{
    public static  class RegexExtensions
    {
        /// <summary>
        /// 验证电话号
        /// </summary>
        /// <param name="strTelephone"></param>
        /// <returns></returns>
        public static bool IsTelephone(this string strTelephone)
        {
            return System.Text.RegularExpressions.Regex.IsMatch(strTelephone, @"^(\d{3,4}-)?\d{6,8}$");
        }

        /// <summary>
        /// 验证手机号
        /// </summary>
        /// <param name="strHandset">手机号</param>
        /// <returns></returns>
        public static bool IsHandset(this string strHandset)
        {
            return System.Text.RegularExpressions.Regex.IsMatch(strHandset, @"^1[3-8]\d{9}$");
            //                                                                
        }

        /// <summary>
        /// 验证身份证号
        /// </summary>
        /// <param name="strIdcard"></param>
        /// <returns></returns>
        public static bool IsIDcard(this string strIdcard)
        {
            return System.Text.RegularExpressions.Regex.IsMatch(strIdcard, @"(^\d{18}$)|(^\d{15}$)");
        }

        /// <summary>
        /// 验证数字
        /// </summary>
        /// <param name="str_number"></param>
        /// <returns></returns>
        public static bool IsNumber(this string strNumber)

        {
            return System.Text.RegularExpressions.Regex.IsMatch(strNumber, @"^[0-9]*$");
        }

        /// <summary>
        /// 验证邮编
        /// </summary>
        /// <param name="strPostalcode"></param>
        /// <returns></returns>

        public static bool IsPostalcode(this string strPostalcode)

        {
            return System.Text.RegularExpressions.Regex.IsMatch(strPostalcode, @"^\d{6}$");
        }

        /// <summary>
        /// 验证邮箱
        /// </summary>
        /// <param name="strEmail"></param>
        /// <returns></returns>
        public static bool IsEmail(this string strEmail)
        {
            return System.Text.RegularExpressions.Regex.IsMatch(strEmail, @"\\w{1,}@\\w{1,}\\.\\w{1,}");
        }

        /// <summary>
        /// 验证汉字
        /// </summary>
        /// <param name="strHanzi"></param>
        /// <returns></returns>
        public static bool IsHanzi(this string strHanzi)
        {
            return System.Text.RegularExpressions.Regex.IsMatch(strHanzi, @"^[\u4E00-\u9FA5]+$");
        }

        /// <summary>
        /// 验证6位数字(6位数字密码)
        /// </summary>
        /// <param name="str_number"></param>
        /// <returns></returns>
        public static bool Is6Number(this string strNumber)

        {
            return System.Text.RegularExpressions.Regex.IsMatch(strNumber, @"^\d{6}$");
        }
    }
}

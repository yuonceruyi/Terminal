using System;
using System.Linq;
using System.Text;

namespace YuanTu.Core.Extension
{
    public static class StringExtensions
    {
        public static string FormatWith(this string str, params object[] parameters)
        {
            return string.Format(str, parameters);
        }

        public static bool IsNullOrWhiteSpace(this string str)
        {
            return string.IsNullOrWhiteSpace(str);
        }

        /*123456*/
        public static string Mask(this string str, int start, int len,char maskChr='*')
        {
            if (str==null|| str.Length <= start)
            {
                return str;
            }
            var part1 = str.Substring(0, start);
            var part2 = "".PadRight(Math.Min(str.Length - start, len), maskChr);
            var part3 = (str.Length - start - len > 0) ? str.Substring(start + len) : "";
            return part1 + part2 + part3;
        }
        public static string BackNotNullOrEmpty(this string origin, params string[] parms)
        {
            if (!string.IsNullOrWhiteSpace(origin))
            {
                return origin;
            }
            foreach (var s in parms)
            {
                if (!string.IsNullOrWhiteSpace(s))
                {
                    return s;
                }
            }
            return null;
        }


        public static string SureLength(this string origin, int length, bool trimEnd, bool paddingEnd, char paddingchar)
        {
            var len = length - (origin?.Length ?? 0);
            if (len == 0)
            {
                return origin;
            }
            else if (len > 0) //补位
            {
                var padstr = "".PadRight(len, paddingchar);
                if (paddingEnd)
                {
                    return origin + padstr;
                }
                else
                {
                    return padstr + origin;
                }
            }
            else if (len < 0)
            {
                if (trimEnd)
                {
                    return origin.Substring(0, length);
                }
                else
                {
                    return new string(origin.Skip(-len).ToArray());
                }
            }
            return origin;
        }

        public static string 右侧补全中文空格(this string str, int length = 4)
        {
            if (str.Length > length)
            {
                return str.Substring(0, length);
            }
            else
            {
                while (str.Length < length)
                {
                    str += "　";
                }
                return str;
            }
        }



        public static string TrimWithEncoding(this string str, int btnLength, Encoding encoding = default(Encoding))
        {
            if (encoding == default(Encoding))
            {
                encoding = Encoding.UTF8;
            }
            var tmpStr = str ?? "";
            try
            {
               
                var currentLen = encoding.GetByteCount(tmpStr);
                if (currentLen > btnLength)//需要删掉
                {
                    while (true)
                    {
                        if (string.IsNullOrWhiteSpace(tmpStr))
                        {
                            return tmpStr;
                        }
                        tmpStr = tmpStr.Remove(tmpStr.Length - 1, 1);
                        currentLen = encoding.GetByteCount(tmpStr);
                        if (currentLen <= btnLength)
                        {
                            return tmpStr;
                        }
                    }
                }
                else if (currentLen < btnLength)//需要追加
                {
                    while (true)
                    {
                        tmpStr = tmpStr + " ";
                        currentLen = encoding.GetByteCount(tmpStr);
                        if (currentLen >= btnLength)
                        {
                            return tmpStr;
                        }
                    }

                }
            }
            catch (Exception)
            {
                
               
            }
           
            return tmpStr;
        }

        public static string ByteToString(this byte[] byteData)
        {
            var text = "";
            for (var i = 0; i < byteData.Length; i++)
            {
                text += string.Format("{0:X2}", byteData[i]);
            }
            return text;
        }

        public static byte[] StringToByte(this string stringData)
        {
            int l = (stringData.Length + 1) / 2;
            byte[] r = new byte[l];
            for (int i = 0; i < l; i++)
            {
                string sHex = stringData.Substring(i * 2, 2);
                r[i] = Convert.ToByte(sHex, 16);
            }
            return r;
        }

        /// <summary>
        /// 取字节数
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static int StringLength(this string str)
        {
            int len = 0;
            byte[] b;

            for (int i = 0; i < str.Length; i++)
            {
                b = Encoding.Default.GetBytes(str.Substring(i, 1));
                if (b.Length > 1)
                    len += 2;
                else
                    len++;
            }
            return len;
        }
    }
}

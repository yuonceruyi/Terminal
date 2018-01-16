using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YuanTu.QDArea.Card
{
    public enum EnumM1Valid
    {
        未启用 = 01,
        启用 = 02,
        停用 = 03,
        异常 = 04
    }
    public enum EnumM1IdType
    {
        保留 = 00,
        身份证 = 01,
        监护人身份证 = 02
    }   
    public class M1
    {   
        internal static byte[] KeyValue = new byte[6] { 0x7E, 0x8F, 0xE8, 0x5C, 0x71, 0x42 };
        internal static byte Key = 0x00;

        /// <summary>
        /// BCD码转为10进制串(阿拉伯数据) 
        /// </summary>
        /// <param name="bytes">BCD码 </param>
        /// <returns>10进制串 </returns>
        internal static string bcd2Str(byte[] bytes)
        {
            StringBuilder temp = new StringBuilder(bytes.Length * 2);

            for (int i = 0; i < bytes.Length; i++)
            {
                temp.Append((byte)((bytes[i] & 0xf0) >> 4));
                temp.Append((byte)(bytes[i] & 0x0f));
            }
            return temp.ToString().Substring(0, 1).Equals("0") ? temp.ToString().Substring(1) : temp.ToString();
        }

        /// <summary>
        /// 10进制串转为BCD码 
        /// </summary>
        /// <param name="asc">10进制串 </param>
        /// <returns>BCD码 </returns>
        internal static byte[] str2Bcd(string asc)
        {
            int len = asc.Length;
            int mod = len % 2;

            if (mod != 0)
            {
                asc = "0" + asc;
                len = asc.Length;
            }

            byte[] abt = new byte[len];
            if (len >= 2)
            {
                len = len / 2;
            }

            byte[] bbt = new byte[len];
            abt = System.Text.Encoding.ASCII.GetBytes(asc);
            int j, k;

            for (int p = 0; p < asc.Length / 2; p++)
            {
                if ((abt[2 * p] >= '0') && (abt[2 * p] <= '9'))
                {
                    j = abt[2 * p] - '0';
                }
                else if ((abt[2 * p] >= 'a') && (abt[2 * p] <= 'z'))
                {
                    j = abt[2 * p] - 'a' + 0x0a;
                }
                else
                {
                    j = abt[2 * p] - 'A' + 0x0a;
                }

                if ((abt[2 * p + 1] >= '0') && (abt[2 * p + 1] <= '9'))
                {
                    k = abt[2 * p + 1] - '0';
                }
                else if ((abt[2 * p + 1] >= 'a') && (abt[2 * p + 1] <= 'z'))
                {
                    k = abt[2 * p + 1] - 'a' + 0x0a;
                }
                else
                {
                    k = abt[2 * p + 1] - 'A' + 0x0a;
                }

                int a = (j << 4) + k;
                byte b = (byte)a;
                bbt[p] = b;
            }
            return bbt;
        }
        /// <summary>
        /// 校验位
        /// </summary>
        /// <param name="b"></param>
        /// <returns></returns>
        internal static byte Checkcode(byte[] b)
        {
            int ret = 0;
            for (int i = 0; i < b.Length; i++)
            {
                //高四位 
                byte b1 = (byte)((b[i] >> 4) & 0xF);
                //低四位  
                byte b2 = (byte)(b[i] & 0xF);
                ret += b1 + b2;
            }
            byte[] bRet = str2Bcd(ret.ToString());
            int len = bRet.Length - 1;
            return bRet[len];
        }
    }
}

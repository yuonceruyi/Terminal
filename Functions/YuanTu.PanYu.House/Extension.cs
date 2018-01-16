using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YuanTu.PanYu.House
{
    public static  class Extension
    {
        public static string Byte2String(this byte[] data)
        {
            int g = data.Length / 8;
            if (data.Length % 8 != 0)
            {
                g++;
                var newData = new byte[g * 8];
                Array.Copy(data, newData, data.Length);
                data = newData;
            }
            var buffer = new byte[8];
            Array.Copy(data, 0, buffer, 0, 8);
            for (int i = 1; i < g; i++)
                for (int j = 0; j < 8; j++)
                    buffer[j] = (byte)(buffer[j] ^ data[i * 8 + j]);

            var hex = Bytes2Hex(buffer);
            var array = Array.ConvertAll(hex.ToCharArray(), Convert.ToByte);
            return Bytes2Hex(array);
        }

        public  static byte[] String2Hex(this string data, int needLen)
        {
            var bdata = new byte[needLen];
            for (int n = 0; n < needLen; n++)
            {
                bdata[n] = 0;
            }

            for (int n = 0; n < data.Length; n++)
            {
                var tmp = new byte[1];
                tmp = Encoding.Default.GetBytes(data.Substring(n, 1));

                bdata[n] = tmp[0];
            }

            return bdata;
        }

        public static string Bytes2Hex(byte[] data)
        {
            var sb = new StringBuilder();
            foreach (byte t in data)
                sb.Append(t.ToString("X2"));
            return sb.ToString();
        }

        public  static byte Char2Byte(this char c)
        {
            if (c >= '0' && c <= '9')
                return Convert.ToByte(c - '0');
            if (c >= 'a' && c <= 'f')
                return Convert.ToByte(c - 'a' + 10);
            if (c >= 'A' && c <= 'F')
                return Convert.ToByte(c - 'A' + 10);
            return Convert.ToByte(-1);
        }
        public static string Bytes2String(this byte[] buff, int len)
        {
            var text = "";
            for (var i = 0; i < len; i++)
            {
                text += string.Format("{0:X2}", buff[i]);
            }
            return text;
        }
    }
}

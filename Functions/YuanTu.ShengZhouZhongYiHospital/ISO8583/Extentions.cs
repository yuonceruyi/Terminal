using System;
using System.Text;

namespace YuanTu.ShengZhouZhongYiHospital.ISO8583
{
    public static class Extentions
    {
        public static int Char2Hex(this char c)
        {
            if (c >= '0' && c <= '9')
                return c - '0';
            if (c >= 'A' && c <= 'F')
                return c - 'A' + 10;
            if (c >= 'a' && c <= 'f')
                return c - 'a' + 10;
            if (c == '=')
                return 0x0D;
            throw new ArgumentOutOfRangeException(nameof(c), $"无法转换:{c}");
        }

        public static byte[] Hex2Bytes(this string text)
        {
            var len = text.Length/2;
            var data = new byte[len];
            for (var i = 0; i < len; i++)
                data[i] = (byte) (text[i*2].Char2Hex()*0x10 + text[i*2 + 1].Char2Hex());
            return data;
        }

        public static string Bytes2Hex(this byte[] data)
        {
            var sb = new StringBuilder();
            foreach (var t in data)
                sb.Append(t.ToString("X2"));
            return sb.ToString();
        }

        public static int BCD2Int(this byte b)
        {
            return b/16*10 + (b & 15);
        }
    }
}
using System;
using System.Linq;
using System.Text;

namespace YuanTu.NingXiaHospital.CardReader.BankCard.Util
{
    public static class Extentions
    {
        public static int Char2Hex(this char c)
        {
            if ((c >= '0') && (c <= '9'))
                return c - '0';
            if ((c >= 'A') && (c <= 'F'))
                return c - 'A' + 10;
            if ((c >= 'a') && (c <= 'f'))
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

        public static string Bytes2Hex(this byte[] data, int startIndex, int count)
        {
            if (startIndex < 0 || startIndex >= data.Length)
                throw new ArgumentOutOfRangeException(nameof(startIndex));

            if (count <= 0 || startIndex + count >= data.Length)
                throw new ArgumentOutOfRangeException(nameof(count));

            var sb = new StringBuilder();
            foreach (var t in data.Skip(startIndex).Take(count))
                sb.Append(t.ToString("X2"));
            return sb.ToString();
        }

        public static int BCD2Int(this byte b)
        {
            return b/16*10 + (b & 15);
        }

        /// <summary>
        ///     取i的长度为n的字符串 前补0 过长取后n位
        /// </summary>
        /// <param name="i">值</param>
        /// <param name="n">长度</param>
        /// <returns></returns>
        public static string ToFixedString(this int i, int n)
        {
            var full = i.ToString($"D{n}");
            if (full.Length > n)
                return full.Substring(full.Length - n);
            return full;
        }
    }
}
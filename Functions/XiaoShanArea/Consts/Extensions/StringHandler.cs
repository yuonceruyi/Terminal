using System.Text;

namespace YuanTu.YuHangArea.Consts.Extensions
{
    public static class StringHandler
    {
        private static readonly Encoding Encoding = Encoding.GetEncoding("GB2312");
        private static readonly Encoding Encoding2 = Encoding.GetEncoding("GBK");

        public enum PadFlag
        {
            Space = 1,
            Zero = 2,
        }

        public enum PadDirection
        {
            Left = 1,
            Right = 2,
        }
        public enum EncodingFlag
        {
            GB2312 = 1,
            GBK = 2,
        }
        public static string FillPadChar(this string s, int len, EncodingFlag coding = EncodingFlag.GB2312,
            PadFlag flag = PadFlag.Space,
            PadDirection direction = PadDirection.Right)
        {
            int slen = coding == EncodingFlag.GB2312 ? Encoding.GetByteCount(s) : Encoding2.GetByteCount(s);

            if (slen > len)
            {
                int i;
                int count = 0;
                var array = s.ToCharArray();
                for (i = 0; i < s.Length; i++)
                {
                    int clen = Encoding.GetByteCount(array, i, 1);
                    count += clen;
                    if (count > len)
                        break;
                }
                return s.Substring(0, i);
            }

            var sb = new StringBuilder(len);
            char c = flag == PadFlag.Space ? ' ' : '0';

            if (direction == PadDirection.Left)
                sb.Append(c, len - slen);

            sb.Append(s);

            if (direction == PadDirection.Right)
                sb.Append(c, len - slen);

            return sb.ToString();
        }
    }
}

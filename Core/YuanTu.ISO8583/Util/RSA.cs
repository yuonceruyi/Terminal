using System.Globalization;
using System.Numerics;
using System.Security.Cryptography;

namespace YuanTu.ISO8583.Util
{
    internal class RSA
    {
        public static string Decode(string data, string ex, string key)
        {
            var iData = BigInteger.Parse("00" + data, NumberStyles.HexNumber);
            var iEx = BigInteger.Parse("00" + ex, NumberStyles.HexNumber);
            var iKey = BigInteger.Parse("00" + key, NumberStyles.HexNumber);

            var res = BigInteger.ModPow(iData, iEx, iKey);

            return res.ToString("X");
        }

        public static string Hash(string data)
        {
            using (var provider = new SHA1CryptoServiceProvider())
            {
                return provider.ComputeHash(data.Hex2Bytes()).Bytes2Hex();
            }
        }
    }
}
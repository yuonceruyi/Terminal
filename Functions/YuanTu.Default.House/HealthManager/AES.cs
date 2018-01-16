using System;
using System.Security.Cryptography;
using System.Text;

namespace YuanTu.Default.House.HealthManager
{
    public class AES
    {
        private static readonly string key = "d8aab995-0329-4f";

        public static string Sign(string before)
        {
            var keyArray = Encoding.UTF8.GetBytes(key);
            var toEncryptArray = Encoding.UTF8.GetBytes(before);
            var rDel = new RijndaelManaged
            {
                Key = keyArray,
                Mode = CipherMode.ECB,
                Padding = PaddingMode.PKCS7
            };
            var cTransform = rDel.CreateEncryptor();
            var resultArray = cTransform.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);
            return BitConverter.ToString(resultArray).Replace("-", "").ToLower();
        }
    }
}
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;

namespace YuanTu.YuHangArea.ISO8583
{
    public class Cryptography
    {
        public Cryptography()
        {
            IV = new byte[8];
            Key = new byte[8];
            CipherMode = CipherMode.CBC;
            PaddingMode = PaddingMode.None;
        }

        /// <summary>
        ///     DES加密所采用的算法
        /// </summary>
        public CipherMode CipherMode { get; set; }

        /// <summary>
        ///     指定在消息数据块比加密操作所需的全部字节数短时应用的填充类型
        /// </summary>
        public PaddingMode PaddingMode { get; set; }

        /// <summary>
        ///     密钥
        /// </summary>
        public byte[] Key { get; set; }

        /// <summary>
        ///     初始化向量
        /// </summary>
        public byte[] IV { get; set; }

        /// <summary>
        ///     ＤＥＳ加密
        /// </summary>
        /// <param name="bySource">要加密的源数据</param>
        /// </param>
        public byte[] DESEncrypt(byte[] bySource)
        {
            try
            {
                var MyServiceProvider = new DESCryptoServiceProvider();
                //计算des加密所采用的算法
                MyServiceProvider.Mode = CipherMode;
                //计算填充类型
                MyServiceProvider.Padding = PaddingMode;
                var MyTransform = MyServiceProvider.CreateEncryptor(Key, IV);
                //CryptoStream对象的作用是将数据流连接到加密转换的流
                var ms = new MemoryStream();
                var MyCryptoStream = new CryptoStream(ms, MyTransform, CryptoStreamMode.Write);
                //将字节数组中的数据写入到加密流中
                MyCryptoStream.Write(bySource, 0, bySource.Length);
                MyCryptoStream.FlushFinalBlock();
                MyCryptoStream.Close();
                var byEncRet = ms.ToArray();
                ms.Close();
                return byEncRet;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception caught: {0}", ex.Message);
                return new byte[8];
            }
        }

        /// <summary>
        ///     DES解密
        /// </summary>
        /// <param name="bySource">要解密的数据</param>
        public byte[] DESDecrypt(byte[] bySource)
        {
            try
            {
                var MyServiceProvider = new DESCryptoServiceProvider();
                //计算des加密所采用的算法
                MyServiceProvider.Mode = CipherMode;
                //计算填充类型
                MyServiceProvider.Padding = PaddingMode;
                var MyTransform = MyServiceProvider.CreateDecryptor(Key, IV);
                //CryptoStream对象的作用是将数据流连接到加密转换的流
                var ms = new MemoryStream();
                var MyCryptoStream = new CryptoStream(ms, MyTransform, CryptoStreamMode.Write);
                //将字节数组中的数据写入到加密流中
                MyCryptoStream.Write(bySource, 0, bySource.Length);
                MyCryptoStream.FlushFinalBlock();
                MyCryptoStream.Close();
                var byEncRet = ms.ToArray();
                ms.Close();
                return byEncRet;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception caught: {0}", ex.Message);
                return new byte[8];
            }
        }

        /// <summary>
        ///     ＭＡＣ计算所要采用的ＣＢＣ　ＤＥＳ算法实现
        /// </summary>
        public byte[] HCDES(byte[] Key, byte[] Data)
        {
            try
            {
                //创建一个DES算法的加密类
                var MyServiceProvider = new DESCryptoServiceProvider();
                MyServiceProvider.Mode = CipherMode.CBC;
                MyServiceProvider.Padding = PaddingMode.None;
                //从DES算法的加密类对象的CreateEncryptor方法,创建一个加密转换接口对象
                //第一个参数的含义是：对称算法的机密密钥(长度为64位,也就是8个字节)
                // 可以人工输入,也可以随机生成方法是：MyServiceProvider.GenerateKey();
                //第二个参数的含义是：对称算法的初始化向量(长度为64位,也就是8个字节)
                // 可以人工输入,也可以随机生成方法是：MyServiceProvider.GenerateIV()
                //
                var MyTransform = MyServiceProvider.CreateEncryptor(Key, new byte[8]);
                //CryptoStream对象的作用是将数据流连接到加密转换的流
                var ms = new MemoryStream();
                var MyCryptoStream = new CryptoStream(ms, MyTransform, CryptoStreamMode.Write);
                //将字节数组中的数据写入到加密流中

                MyCryptoStream.Write(Data, 0, Data.Length);
                //关闭加密流对象
                var bEncRet = new byte[8];
                // Array.Copy(ms.GetBuffer(), bEncRet, ms.Length);
                bEncRet = ms.ToArray(); // MyCryptoStream关闭之前ms.Length 为8， 关闭之后为16
                MyCryptoStream.FlushFinalBlock();
                MyCryptoStream.Close();
                var bTmp = ms.ToArray();
                ms.Close();

                // return bEncRet;
                return bTmp; //
            }
            catch (Exception ex)
            {
                Console.WriteLine("HCDES Exception Caught, Exception = {0}", ex.Message);
                return new byte[8];
            }
        }

        /// <summary>
        ///     MAC计算 CBC
        /// </summary>
        public byte[] MAC_CBC(byte[] MacData)
        {
            try
            {
                var iGroup = 0;
                var bKey = Key;
                var bIV = IV;
                var bTmpBuf1 = new byte[8];
                var bTmpBuf2 = new byte[8];
                // init
                Array.Copy(bIV, bTmpBuf1, 8);
                if (MacData.Length%8 == 0)
                    iGroup = MacData.Length/8;
                else
                    iGroup = MacData.Length/8 + 1;
                var i = 0;
                var j = 0;
                for (i = 0; i < iGroup; i++)
                {
                    Array.Copy(MacData, 8*i, bTmpBuf2, 0, 8);
                    for (j = 0; j < 8; j++)
                        bTmpBuf1[j] = (byte) (bTmpBuf1[j] ^ bTmpBuf2[j]);
                    bTmpBuf2 = HCDES(bKey, bTmpBuf1);
                    Array.Copy(bTmpBuf2, bTmpBuf1, 8);
                }
                return bTmpBuf2;
            }
            catch (Exception ex)
            {
                Console.WriteLine("MAC_CBC() Exception caught, exception = {0}", ex.Message);
                return new byte[8];
            }
        }

        /// <summary>
        ///     MAC计算 ECB
        /// </summary>
        public byte[] MAC_ECB(byte[] data)
        {
            try
            {
                // 8 字节对齐
                var g = data.Length/8;
                if (data.Length%8 != 0)
                {
                    g++;
                    var newData = new byte[g*8];
                    Array.Copy(data, newData, data.Length);
                    data = newData;
                }
                // 求异或
                var buffer = new byte[8];
                Array.Copy(data, 0, buffer, 0, 8);
                for (var i = 1; i < g; i++)
                    for (var j = 0; j < 8; j++)
                        buffer[j] = (byte) (buffer[j] ^ data[i*8 + j]);

                // 转Hex后分高低部分
                var hex = buffer.Bytes2Hex();
                var array = Array.ConvertAll(hex.ToCharArray(), Convert.ToByte);
                var hBytes = new byte[8];
                var lBytes = new byte[8];
                Array.Copy(array, 0, hBytes, 0, 8);
                Array.Copy(array, 8, lBytes, 0, 8);

                // 高位加密 与低位异或 再加密
                var des = TripleDESEncryptOk(hBytes);
                for (var i = 0; i < 8; i++)
                {
                    lBytes[i] = (byte) (lBytes[i] ^ des[i]);
                }
                des = TripleDESEncryptOk(lBytes);

                // 取Hex前8字符
                var desHex = des.Bytes2Hex();
                return Encoding.Default.GetBytes(desHex.Substring(0, 8));

                //var desArray = Array.ConvertAll(desHex.ToCharArray(0, 8), Convert.ToByte);

                //return desArray.Bytes2Hex();
            }
            catch (Exception ex)
            {
                Console.WriteLine("MAC_ECB() Exception caught, exception = {0}", ex.Message);
                return new byte[8];
            }
        }

        public byte[] MAC_ECB_DES(byte[] data)
        {
            try
            {
                // 8 字节对齐
                var g = data.Length / 8;
                if (data.Length % 8 != 0)
                {
                    g++;
                    var newData = new byte[g * 8];
                    Array.Copy(data, newData, data.Length);
                    data = newData;
                }
                // 求异或
                var buffer = new byte[8];
                Array.Copy(data, 0, buffer, 0, 8);
                for (var i = 1; i < g; i++)
                    for (var j = 0; j < 8; j++)
                        buffer[j] = (byte)(buffer[j] ^ data[i * 8 + j]);
                return DESEncrypt(buffer);
            }
            catch (Exception ex)
            {
                Console.WriteLine("MAC_ECB() Exception caught, exception = {0}", ex.Message);
                return new byte[8];
            }
        }

        /// <summary>
        ///     ３ＤＥＳ加密
        /// </summary>
        public byte[] TripleDESEncrypt(byte[] bySource)
        {
            try
            {
                var MyServiceProvider = new TripleDESCryptoServiceProvider();
                //计算des加密所采用的算法
                MyServiceProvider.Mode = CipherMode;
                //计算填充类型
                MyServiceProvider.Padding = PaddingMode;
                /* MyServiceProvider.GenerateKey();
                MyServiceProvider.GenerateIV();
                byte[] key = MyServiceProvider.Key;
                byte[] iv = MyServiceProvider.IV;
                MyServiceProvider.IV = this.mbyIV;
                MyServiceProvider.Key = this.mbyKey;
                ICryptoTransform MyTransform = MyServiceProvider.CreateEncryptor();*/
                //TripleDESCryptoServiceProvider
                //支持从 128 位到 192 位（以 64 位递增）的密钥长度
                //IV需要8个字节
                //设置KEY时要注意的是可能引发CryptographicException异常，主要是因为所设置的KEY为WeakKey
                var MyTransform = MyServiceProvider.CreateEncryptor(Key, IV);
                //CryptoStream对象的作用是将数据流连接到加密转换的流
                var ms = new MemoryStream();
                var MyCryptoStream = new CryptoStream(ms, MyTransform, CryptoStreamMode.Write);
                //将字节数组中的数据写入到加密流中
                MyCryptoStream.Write(bySource, 0, bySource.Length);
                MyCryptoStream.FlushFinalBlock();
                MyCryptoStream.Close();
                var byEncRet = ms.ToArray();
                ms.Close();
                return byEncRet;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception caught: {0}", ex.Message);
                return new byte[8];
            }
        }

        /// <summary>
        ///     3ＤＥＳ解密
        /// </summary>
        public byte[] TripleDESDecrypt(byte[] bySource)
        {
            try
            {
                //var MyServiceProvider = new TripleDESCryptoServiceProvider();
                ////计算des加密所采用的算法
                //MyServiceProvider.Mode = mCipherMode;
                ////计算填充类型
                //MyServiceProvider.Padding = mPaddingMode;
                //支持从 128 位到 192 位（以 64 位递增）的密钥长度
                //IV需要8个字节
                //设置KEY时要注意的是可能引发CryptographicException异常，主要是因为所设置的KEY为WeakKey
                var MyTransform = ReflectorCreate(Key, CipherMode, IV);
                //MyServiceProvider.CreateDecryptor(mbyKey, mbyIV);
                //CryptoStream对象的作用是将数据流连接到加密转换的流
                var ms = new MemoryStream();
                var MyCryptoStream = new CryptoStream(ms, MyTransform, CryptoStreamMode.Write);
                //将字节数组中的数据写入到加密流中
                MyCryptoStream.Write(bySource, 0, bySource.Length);
                MyCryptoStream.FlushFinalBlock();
                MyCryptoStream.Close();
                var byEncRet = ms.ToArray();
                ms.Close();
                return byEncRet;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception caught: {0}", ex.Message);
                return new byte[8];
            }
        }

        private ICryptoTransform ReflectorCreate(byte[] rgbKey, CipherMode mode, byte[] rgbIV, int feedbackSize = 8)
        {
            var type = typeof (TripleDESCryptoServiceProvider);
            var myServiceProvider = new TripleDESCryptoServiceProvider();
            var methods = type.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance);
            var med = methods.FirstOrDefault(p => p.Name == "_NewEncryptor");
            return
                (ICryptoTransform)
                    med?.Invoke(myServiceProvider, new object[] {rgbKey, CipherMode, IV, feedbackSize, 1});
        }

        public byte[] CalcDesMac(byte[] data)
        {
            var des = new TripleDESCryptoServiceProvider();
            //des.Key = Key;
            var fi = des.GetType().GetField("KeyValue", BindingFlags.Instance | BindingFlags.NonPublic);
            fi.SetValue(des, Key);
            fi = des.GetType().GetField("KeySizeValue", BindingFlags.Instance | BindingFlags.NonPublic);
            fi.SetValue(des, Key.Length*8);
            des.IV = new byte[8];
            des.Padding = PaddingMode;
            des.Mode = CipherMode;
            var ms = new MemoryStream();
            using (var cs = new CryptoStream(ms, des.CreateDecryptor(), CryptoStreamMode.Write))
            {
                cs.Write(data, 0, data.Length);
            }
            var encryption = ms.ToArray();
            var mac = new byte[8];
            Array.Copy(encryption, encryption.Length - 8, mac, 0, 8);
            //PrintByteArray(encryption);
            return mac;
        }

        public byte[] TripleDESDecryptOk(byte[] str)
        {
            var des = new TripleDESCryptoServiceProvider();
            des.Padding = PaddingMode;
            des.Mode = CipherMode;
            var t = Type.GetType("System.Security.Cryptography.CryptoAPITransformMode");
            var obj =
                t.GetField("Decrypt",
                    BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic |
                    BindingFlags.DeclaredOnly).GetValue(t);

            var mi = des.GetType().GetMethod("_NewEncryptor", BindingFlags.Instance | BindingFlags.NonPublic);
            var desCrypt = (ICryptoTransform) mi.Invoke(des, new[] {Key, CipherMode.ECB, null, 0, obj});

            var result = desCrypt.TransformFinalBlock(str, 0, str.Length);
            return result;
        }

        public byte[] TripleDESEncryptOk(byte[] str)
        {
            var des = new TripleDESCryptoServiceProvider
            {
                Padding = PaddingMode,
                Mode = CipherMode
            };
            var t = Type.GetType("System.Security.Cryptography.CryptoAPITransformMode");
            var obj =
                t.GetField("Encrypt",
                    BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic |
                    BindingFlags.DeclaredOnly).GetValue(t);

            var mi = des.GetType().GetMethod("_NewEncryptor", BindingFlags.Instance | BindingFlags.NonPublic);
            var desCrypt = (ICryptoTransform) mi.Invoke(des, new[] {Key, CipherMode.ECB, IV, 0, obj });
           // System.Security.Cryptography.CryptoAPITransformMode
            var result = desCrypt.TransformFinalBlock(str, 0, str.Length);
            return result;
        }
    }
}
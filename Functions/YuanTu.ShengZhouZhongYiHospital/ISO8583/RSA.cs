﻿using System;
using System.Globalization;
using System.Security.Cryptography;
using System.Numerics;
namespace YuanTu.ShengZhouZhongYiHospital.ISO8583
{
    internal class RSA
    {
        public static void Main0()
        {
            var sha1 = Hash("046225000000000030FFFF123000008301018001C407A9F815015CCAD363F478005A4AD213F8A102D5A4DB730A101E73E2FD981946E117FB8F90D0C10DEAFB1BA99439F11E537687F396FB5ADB74DF2C1D89268BEBFD20CBBEBBA969BF2847183D28F30023C25FBC0ED641CC717606A26E189DFB57E4AFACF09CA9E9F00C718636660BC41ECB1C8C176B0F155D64671178F0D41B035F25030510315F24033010315A0862250000000000305F3401039F0702FF008E100000000000000000420341031E031F009F0D05D8689CF8009F0E0500100000009F0F05D8609CA8005F280201567800");
            Console.WriteLine(sha1);

            var data ="89C3C2AD81D6750FE6CA95E42404E83F0C913E09B022A9E83B213BC0A74F4411C77F21D2B1E333ECB39AD58D17C85F3210BF48F4108CAAB518B2D640D10AA2773121CEE83775074D709A9152DA71DBD352CA63A139D475D3301756E028C6DA4E4F3151F11E870837642617C2873364BF50509F6B837A1217965089A685C372D977502F291BB4DC9701B5E5A654D44EC0";
            var key ="C2ABE763CD75D57DDCD34CF632AA27F5E95A5204562C2D39E9460774C761B86573E9D4C1B5AC4DADA9F42F9217712B73D5A66E29EA8E0274085FF633CB8EBBFAFB13F8BC826384E1522FAB4FC4545818CB6F416585845E7E64B721A34BE48FAEF0B078DCBADEBE5FFA22A747FFABC8ECF62FE4B096949FAE88A331792873163BECD90D75D8F1570F47ED40F78690B7FB";
            var ex = "03";
            
            var s = Decode(data, ex, key);

            Console.WriteLine(s);
        }

        public static string Decode(string data,string ex,string key)
        {
            var iData = BigInteger.Parse("00"+data, NumberStyles.HexNumber);
            var iEx = BigInteger.Parse("00" + ex, NumberStyles.HexNumber);
            var iKey = BigInteger.Parse("00"+key, NumberStyles.HexNumber);

            var res = BigInteger.ModPow(iData, iEx, iKey);

            return res.ToString("X");
        }

        public static string Hash(string data)
        {
            using (var provider = new SHA1CryptoServiceProvider())
                return provider.ComputeHash(data.Hex2Bytes()).Bytes2Hex();
        }
    }
    
}
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace YuanTu.PanYu.House.PanYuGateway
{
    internal class RSA
    {
        private static RSACryptoServiceProvider _csp;
        private static RSACryptoServiceProvider Csp => _csp ?? (_csp = Initialize());

        public static string Sign(string data)
        {
            return Convert.ToBase64String(Csp.SignData(Encoding.UTF8.GetBytes(data), CryptoConfig.MapNameToOID("SHA1")));
        }

        private static RSACryptoServiceProvider Initialize()
        {
            var privateKey =
                "MIICWwIBAAKBgQCVrC7H3klwPvqX4BTc0ZtIyru3LEzceF5Zpo474ZpPbyxP41aP4bGNyXBmhZj0IeGU4swMhgwrMwS7oQqozgQ7MP8KQpEiaHdvi+enrL+ZH730FiPcMH6Hu3CI2+64PfasFKWg2WF+mFffIo39USGzenMHvAmtufi9y2iVt3QayQIDAQABAoGAcN34JRaWkMOdW5xKfz69eAYRyo7oozVOCRMVnUiCgjZW1yObqkLLDx4B2f4TGr7WCt2AfE01rfn6LrfBfwxkmUsSG1C2p+39P5lYLz2Masn2E5HnbJ970lgZE7EnLD5LoyhTlE6z4OomWjaoS29beA91RZAEjDlgvqbyJvFuEPUCQQDHYu7ln7VslZJ7oe1gF1946VWRPOS+GgxndPk3Iq7KlE7c8zz27teHhRPR81xvYFg5LAHA332mOY1h9hW7yhlbAkEAwCuhw4xgXggdWr2uynhphew8uJ0wxjJ5nnv0Cn8Qwy649zPEcXS2Bn4+nwq5Cwj7rfJUnJ0tUQwDZ5VfRgxxqwJAHE8erjmbz7v43VbjViZbZtZyULm9nIQkSLgh/kMNYDPocpSSjljg/xvU8ZVFBYc/X3axFQpmU6iOO19uPIh4SwJALRGOjJf9edAZYlCeD6oRxXDUBNAIwlLQJxUW9Oc7/SqWApPHfHxMvFUbRp1zLF1c+TyeD2TlXd6dZcuoXntIpQJAae4jE5wFP/+IqGMWaqlYMdOfdM503C/u/foJL7GxRBz/n9LvGo0k1MKJp2sPvetRt2h/WNPNkvvPgyr5FifpMA==";
            var bytes = Convert.FromBase64String(privateKey);
            return DecodeRSAPrivateKey(bytes);
        }

        private static RSACryptoServiceProvider DecodeRSAPrivateKey(byte[] privkey)
        {
            byte[] MODULUS, E, D, P, Q, DP, DQ, IQ;

            // --------- Set up stream to decode the asn.1 encoded RSA private key ------
            var mem = new MemoryStream(privkey);
            var binr = new BinaryReader(mem); //wrap Memory Stream with BinaryReader for easy reading
            byte bt = 0;
            ushort twobytes = 0;
            var elems = 0;
            try
            {
                twobytes = binr.ReadUInt16();
                if (twobytes == 0x8130) //data read as little endian order (actual data order for Sequence is 30 81)
                    binr.ReadByte(); //advance 1 byte
                else if (twobytes == 0x8230)
                    binr.ReadInt16(); //advance 2 bytes
                else
                    return null;

                twobytes = binr.ReadUInt16();
                if (twobytes != 0x0102) //version number
                    return null;
                bt = binr.ReadByte();
                if (bt != 0x00)
                    return null;

                //------ all private key components are Integer sequences ----
                elems = GetIntegerSize(binr);
                MODULUS = binr.ReadBytes(elems);

                elems = GetIntegerSize(binr);
                E = binr.ReadBytes(elems);

                elems = GetIntegerSize(binr);
                D = binr.ReadBytes(elems);

                elems = GetIntegerSize(binr);
                P = binr.ReadBytes(elems);

                elems = GetIntegerSize(binr);
                Q = binr.ReadBytes(elems);

                elems = GetIntegerSize(binr);
                DP = binr.ReadBytes(elems);

                elems = GetIntegerSize(binr);
                DQ = binr.ReadBytes(elems);

                elems = GetIntegerSize(binr);
                IQ = binr.ReadBytes(elems);

                // ------- create RSACryptoServiceProvider instance and initialize with public key -----
                var cspParameters = new CspParameters {Flags = CspProviderFlags.UseMachineKeyStore};
                var rsa = new RSACryptoServiceProvider(1024, cspParameters);
                var rsAparams = new RSAParameters
                {
                    Modulus = MODULUS,
                    Exponent = E,
                    D = D,
                    P = P,
                    Q = Q,
                    DP = DP,
                    DQ = DQ,
                    InverseQ = IQ
                };
                rsa.ImportParameters(rsAparams);
                return rsa;
            }
            catch (Exception)
            {
                return null;
            }
            finally
            {
                binr.Close();
            }
        }

        private static int GetIntegerSize(BinaryReader binr)
        {
            byte bt = 0;
            byte lowbyte = 0x00;
            byte highbyte = 0x00;
            var count = 0;
            bt = binr.ReadByte();
            if (bt != 0x02) //expect integer
                return 0;
            bt = binr.ReadByte();

            if (bt == 0x81)
                count = binr.ReadByte(); // data size in next byte
            else if (bt == 0x82)
            {
                highbyte = binr.ReadByte(); // data size in next 2 bytes
                lowbyte = binr.ReadByte();
                byte[] modint = {lowbyte, highbyte, 0x00, 0x00};
                count = BitConverter.ToInt32(modint, 0);
            }
            else
            {
                count = bt; // we already have the data size
            }

            while (binr.ReadByte() == 0x00)
            {
                //remove high order zeros in data
                count -= 1;
            }
            binr.BaseStream.Seek(-1, SeekOrigin.Current); //last ReadByte wasn't a removed zero, so back up a byte
            return count;
        }
    }
}
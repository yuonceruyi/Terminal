using System;
using System.Text;
using YuanTu.Core.Log;

namespace YuanTu.PanYu.House.CardReader
{
    public  class IACT_A6_V2:ACT_A6_V2
    {
        public enum ICardType
        {
            M1,
            CPU,
            Unknow
        }
        public static ICardType cardType { get; set; }
        public static ICardType DetectIccType()
        {
        
            var pbType = new byte[1];
            ret = A6_DetectIccType(handle, pbType);
            var res = pbType.Bytes2String(pbType.Length);
            if (res == "10")
                cardType = ICardType.M1;
            else if (res == "13")
                cardType = ICardType.CPU;
            else
                cardType = ICardType.Unknow;
            if (ret != 0)
            {
                var log = "检测卡类型失败";
                cardType = ICardType.Unknow;
                Logger.Device.Debug(log);
                throw new Exception(log);
            }
            return cardType;
        }

        public static string ReadCPUCardNo_SZ()
        {
            string text;
            SelectTypeACpu();
            if (ret != 0)
                return "";
            text = Transmit("00A40000023F00");
            if (ret != 0 || !text.EndsWith("9000"))
                return "";
            text = Transmit("00A40000023F02");
            if (ret != 0 || !text.EndsWith("9000"))
                return "";
            text = Transmit("00A4000002ADF1");
            if (ret != 0 || !text.EndsWith("9000"))
                return "";
            text = Transmit("00A40000020015");
            if (ret != 0 || !text.EndsWith("9000"))
                return "";
            text = Transmit("00B0950000");
            if (ret != 0 || !text.EndsWith("9000"))
                return "";
            Logger.Device.Debug($"深圳CPU卡应用基本文件信息：{text}");
            return text;
        }
        public static string ReadCPUCardNo_SH()
        {
            var CardNo = "";
            string text;
            SelectTypeACpu();
            if (ret != 0)
                return "";
            text = Transmit("00A404000B4755414E474449414E4D46");
            if (ret != 0 || !text.EndsWith("9000"))
                return "";
            text = Transmit("00A404000C4755414E474449414E414446");
            if (ret != 0 || !text.EndsWith("9000"))
                return "";
            text = Transmit("00B0950818");
            if (ret != 0 || !text.EndsWith("9000"))
                return "";
            Logger.Device.Debug($"上海CPU卡应用基本文件信息：{text}");
            CardNo = text.Substring(12, 19);
            return CardNo;
        }

        public static string ReadCPUPatInfo_SZ()
        {
            string text = "";
            SelectTypeACpu();
            if (ret != 0)
                return "";
            text = Transmit("00A40000023F00");
            if (ret != 0 || !text.EndsWith("9000"))
                return "";
            text = Transmit("00A40000023F02");
            if (ret != 0 || !text.EndsWith("9000"))
                return "";
            text = Transmit("00A4000002ADF1");
            if (ret != 0 || !text.EndsWith("9000"))
                return "";
            text = Transmit("00A40000020016");
            if (ret != 0 || !text.EndsWith("9000"))
                return "";
            text = Transmit("00B0960000");
            if (ret != 0 || !text.EndsWith("9000"))
                return "";
            return text;
        }
        public static string ReadCPUPatInfo_SH()
        {
            string text = "";
            SelectTypeACpu();
            if (ret != 0)
                return "";
            text = Transmit("00A404000B4755414E474449414E4D46");
            if (ret != 0 || !text.EndsWith("9000"))
                return "";
            text = Transmit("00A404000C4755414E474449414E414446");
            if (ret != 0 || !text.EndsWith("9000"))
                return "";
            text = Transmit("805C000204");
            if (ret != 0 || !text.EndsWith("9000"))
                return "";
            text = Transmit("00B096004F");
            if (ret != 0 || !text.EndsWith("9000"))
                return "";
          
            return text;
        }


        public static void GetCPUPatInfo(string orgcode, out string name, out string sex, out string Idtype, out string Idno)
        {
            var _name = orgcode.Substring(0, 60);
            var _nameArray = new byte[30];
            int j = 0;
            for (var i = 0; i < _name.Length; i += 2)
            {
                _nameArray[j] = Convert.ToByte(_name[i].ToString() + _name[i + 1].ToString(), 16);
                j++;
            }
            name = Encoding.GetEncoding("GBK").GetString(_nameArray).Trim('\0');
            var _idtype = orgcode.Substring(60, 4);
            Idtype = _idtype == "0000" ? "身份证" : "其他";

            var _idno = orgcode.Substring(72, 64);
            var IDNum = "";
            for (var i = 0; i < _idno.Length; i++)
            {
                if (i % 2 != 0)
                    IDNum += _idno[i];
            }
            Idno = IDNum.Substring(0, 18);
            var _sex = orgcode.Substring(136, 2);
            sex = _sex == "31" ? "男" : "女";
        }
      
        private static byte[] Buffer { get; set; }
        public static string KeyA_str => KeyA.Bytes2String( KeyA.Length);
        public static byte[] KeyA => GetKeyA();

        public static byte[] GetKeyA()
        {
            byte[] result = new byte[2];

            result[0] = (byte)(Buffer[0] ^ Buffer[2]);
            result[1] = (byte)(Buffer[1] ^ Buffer[3]);
            var keyA = new byte[6];
            for (var i = 0; i < Buffer.Length; i++)
                keyA[i] = Buffer[i];
            for (var i = 0; i < result.Length; i++)
                keyA[4 + i] = result[i];
            return keyA;
        }

        public static string GetSeqNo()
        {
            var uid = new byte[5];
            var l = uid.Length;
            ret = A6_SxxGetUID(handle, uid, ref l);
            if (ret != 0)
            {
                Logger.Device.Debug("读卡片序列号失败：" + ret);
                throw new Exception("读卡片序列号失败：" + ret);
            }
            Buffer = new byte[4];
            for (var i = 0; i < 4; i++)
                Buffer[i] = uid[i + 1];
            return Buffer.Bytes2String(Buffer.Length);
        }

        public static string ReadCardNo()
        {
            ret = A6_SxxVerifyPassword(handle, 1, true, KeyA);
            var keyA = KeyA.Bytes2String(KeyA.Length);
            if (ret != 0)
            {
                Logger.Device.Debug("1扇区验证密码失败：" + ret);
                throw new Exception("1扇区验证密码失败：" + ret);
            }
            var cardNo = new byte[16];
            ret = A6_SxxReadBlock(handle, 1, 0, cardNo);
            if (ret != 0)
            {
                Logger.Device.Debug("读卡号和密码失败：" + ret);
                throw new Exception("读卡号和密码失败：" + ret);
            }
            return cardNo.Bytes2String( cardNo.Length).Substring(0, 19);
        }

        public static void ReadPatInfo(out string Name, out string Sex, out string IdType, out string IdNo)
        {
            //ret = A6_SxxVerifyPassword(handle, 1, true, KeyA);
            //if (ret != 0)
            //{
            //    Logger.Device.Debug("1扇区验证密码失败：" + ret);
            //    throw new Exception("1扇区验证密码失败：" + ret);
            //}
            //var chunk0 = new byte[16];
            //ret = A6_SxxReadBlock(handle, 1, 2, chunk0);
            //if (ret != 0)
            //{
            //    Logger.Device.Debug("读城市和档案号失败：" + ret);
            //    throw new Exception("读城市和档案号失败：" + ret);
            //}
            //var Chunk0 = Bytes2String(chunk0, 16);
            //Logger.Device.Debug(Chunk0);

            ret = A6_SxxVerifyPassword(handle, 2, true, KeyA);
            if (ret != 0)
            {
                Logger.Device.Debug("2扇区验证密码失败：" + ret);
                throw new Exception("2扇区验证密码失败：" + ret);
            }
            var chunk1 = new byte[16];
            ret = A6_SxxReadBlock(handle, 2, 0, chunk1);
            if (ret != 0)
            {
                Logger.Device.Debug("读姓名和性别失败：" + ret);
                throw new Exception("读姓名和性别失败：" + ret);
            }
            var Chunk1 = chunk1.Bytes2String( 16);
            Logger.Device.Debug(Chunk1);

            var chunk2 = new byte[16];
            ret = A6_SxxReadBlock(handle, 2, 1, chunk2);
            if (ret != 0)
            {
                Logger.Device.Debug("读证件类型和证件号失败：" + ret);
                throw new Exception("读证件类型和证件号失败：" + ret);
            }
            var Chunk2 = chunk2.Bytes2String(16);
            Logger.Device.Debug(Chunk2);
            var chunk3 = new byte[16];
            ret = A6_SxxReadBlock(handle, 2, 2, chunk3);
            if (ret != 0)
            {
                Logger.Device.Debug("读证件号失败：" + ret);
                throw new Exception("读证件号失败：" + ret);
            }
            var Chunk3 = chunk3.Bytes2String( 16);
            Logger.Device.Debug(Chunk3);
            Name = GetName(Chunk1);
            Sex = GetSex(Chunk1);
            IdType = GetIdType(Chunk2);
            IdNo = GetIdNo(Chunk2,Chunk3);
        }

        private static string GetName(string Chunk1)
        {
            var name = Chunk1.Substring(0, 28);
            var Name = "";
            var _nameArray = new byte[14];
            int j = 0;
            for (var i = 0; i < name.Length; i += 2)
            {
                _nameArray[j] = Convert.ToByte(name[i].ToString() + name[i + 1].ToString(), 16);
                j++;
            }
            Name = Encoding.GetEncoding("GBK").GetString(_nameArray).Trim('\0');
            var _name = Convert.ToChar(Name.Substring(0, 1));
            Name = _name + Name.Split(_name)[1];
            return Name;
        }

        private static string GetSex(string Chunk1)
        {
            var sex = Chunk1.Substring(28, 2);
            return sex == "01" ? "男" : "女";
        }

        private static string GetIdType(string Chunk2)
        {
            var idtype = Chunk2.Substring(0, 2);
            var IDtype = "";
            if (idtype == "00")
                IDtype = "保留";
            if (idtype == "01")
                IDtype = "身份证";
            if (idtype == "02")
                IDtype = "监护人身份证";
            return IDtype;
        }

        private static string GetIdNo(string Chunk2, string Chunk3)
        {
            var idno = Chunk2.Substring(2, 28) + Chunk3.Substring(0, 30);
            var IDNum = "";
            for (var i = 0; i < idno.Length; i++)
            {
                if (i % 2 != 0)
                    IDNum += idno[i];
            }
            return IDNum.Substring(0, 18);
        }
    }
}

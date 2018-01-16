using System;
using System.Text;
using YuanTu.Core.Log;
using YuanTu.PanYu.House.CardReader.HuaDa;

namespace YuanTu.PanYu.House.CardReader
{
    public  class HuaDaReaderHelper
    {
     

        private static byte[] Buffer { get; set; }


        public static byte[] GetKeyA(byte[] buffer)
        {
            Buffer = new byte[4];
            for (var i = 0; i < 4; i++)
                Buffer[i] = buffer[i];
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

        public static string GetCardNo(byte[] data)
        {
            return Bytes2String(data, data.Length).Substring(0, 19);
        }

        public static string Bytes2String(byte[] buff, int len)
        {
            var text = "";
            for (var i = 0; i < len; i++)
            {
                text += string.Format("{0:X2}", buff[i]);
            }
            return text;
        }

        public static string GetName(byte[] chunk1)
        {
            var Chunk1= Bytes2String(chunk1, 16);
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

        public static string GetSex(byte[] chunk1)
        {
            var Chunk1 = Bytes2String(chunk1, 16);
            var sex = Chunk1.Substring(28, 2);
            return sex == "01" ? "男" : "女";
        }

        public static string GetIdType(byte[] chunk2)
        {
            var Chunk2 = Bytes2String(chunk2, 16);
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

        public static string GetIdNo(byte[] chunk2, byte[] chunk3)
        {
            var Chunk2 = Bytes2String(chunk2,16);
            var Chunk3= Bytes2String(chunk3, 16);
            var idno = Chunk2.Substring(2, 28) + Chunk3.Substring(0, 30);
            var IDNum = "";
            for (var i = 0; i < idno.Length; i++)
            {
                if (i % 2 != 0)
                    IDNum += idno[i];
            }
            return IDNum.Substring(0, 18);
        }



        public static string ReadCPUCardNo_SZ()
        {
            bool ret;
            string res;
     
            ret = CPU.PICCReaderApplication ("00A40000023F00",out res);
            if (!ret || !res.Contains("9000"))
                return "";
            ret = CPU.PICCReaderApplication("00A40000023F02", out res);
            if (!ret || !res.Contains("9000"))
                return "";
            ret = CPU.PICCReaderApplication("00A4000002ADF1", out res);
            if (!ret || !res.Contains("9000"))
                return "";
            ret = CPU.PICCReaderApplication("00A40000020015", out res);
            if (!ret || !res.Contains("9000"))
                return "";
            ret = CPU.PICCReaderApplication("00B0950000", out res);
            if (!ret || !res.Contains("9000"))
                return "";
         
            Logger.Device.Debug($"深圳CPU卡应用基本文件信息：{res}");
            return res;
        }

        public static string ReadCPUCardNo_SH()
        {
            var CardNo = "";
            bool ret;
            string res;
          
            ret = CPU.PICCReaderApplication("00A404000B4755414E474449414E4D46",out res);
            if (!ret || !res.Contains("9000"))
                return "";
            ret = CPU.PICCReaderApplication("00A404000C4755414E474449414E414446",out res);
            if (!ret|| !res.Contains("9000"))
                return "";
            ret = CPU.PICCReaderApplication("00B0950818",out res);
            if (!ret || !res.Contains("9000"))
                return "";
            Logger.Device.Debug($"上海CPU卡应用基本文件信息：{res}");
            CardNo = res.Substring(12, 19);
            return CardNo;
        }
        public static string ReadCPUPatInfo_SH()
        {
            bool ret;
            string res;
            ret = CPU.PICCReaderApplication("00A404000B4755414E474449414E4D46",out res);
            if (!ret || !res.Contains("9000"))
                return "";
            ret = CPU.PICCReaderApplication("00A404000C4755414E474449414E414446",out res);
            if (!ret || !res.Contains("9000"))
                return "";
            ret = CPU.PICCReaderApplication("805C000204",out res);
            if (!ret || !res.Contains("9000"))
                return "";
            ret = CPU.PICCReaderApplication("00B096004F",out res);
            if (!ret || !res.Contains("9000"))
                return "";

            return res;
        }

        public static string ReadCPUPatInfo_SZ()
        {
            bool ret;
            string res;

            ret = CPU.PICCReaderApplication("00A40000023F00",out res);
            if (!ret|| !res.Contains("9000"))
                return "";
            ret = CPU.PICCReaderApplication("00A40000023F02",out res);
            if (!ret || !res.Contains("9000"))
                return "";
            ret = CPU.PICCReaderApplication("00A4000002ADF1",out res);
            if (!ret || !res.Contains("9000"))
                return "";
            ret = CPU.PICCReaderApplication("00A40000020016",out res);
            if (!ret || !res.Contains("9000"))
                return "";
            ret = CPU.PICCReaderApplication("00B0960000",out res);
            if (!ret || !res.Contains("9000"))
                return "";
            return res;
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

    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using YuanTu.Consts.FrameworkBase;
using YuanTu.Devices.CardReader;

namespace YuanTu.QDArea.Card
{
    /// <summary>
    /// M1卡读写
    /// </summary>
    public class M1DispenserRW : M1
    {
        private static IRFCardDispenser rfCardDispenser;

        public static IRFCardDispenser RfCardDispenser
        {
            get
            {
                return rfCardDispenser;
            }

            set
            {
                rfCardDispenser = value;
            }
        }

        /// <summary>
        /// //第4扇区 块0 平台卡号	院区代码	启用标志
        /// </summary>        
        public static Result WriteCommChunk0(string CardNo, string HosNo, EnumM1Valid eValid)
        {
            string valid = eValid.GetHashCode().ToString();
            byte[] chunk0 = new byte[15];
            byte[] bCardNo = str2Bcd(CardNo);//平台卡号
            byte[] bHosNo = str2Bcd(HosNo);
            byte[] bValid = str2Bcd(valid);
            chunk0 = bCardNo.Concat(bHosNo).Concat(bValid).ToArray();
            byte[] data = new byte[16];
            Array.Copy(chunk0, data, chunk0.Length);
            data[15] = Checkcode(chunk0);

            return rfCardDispenser.WirteBlock(4, 0, false, KeyValue, data);          
        }
        /// <summary>
        /// //第4扇区 块0 平台卡号	院区代码	启用标志
        /// </summary>        
        public static void ReadCommChunk0(out string CardNo, out string HosNo, out string valid)
        {
            CardNo = string.Empty;
            HosNo = string.Empty;
            valid = string.Empty;
            byte[] data;
            Result<byte[]> result = rfCardDispenser.ReadBlock(4, 0, false, KeyValue);            
            if (!result.IsSuccess)
            {
                return;
            }
            data = result.Value;

            byte[] chunk0 = new byte[15];
            byte[] bCardNo = new byte[9];
            byte[] bHosNo = new byte[5];
            byte[] bValid = new byte[1];
            Array.Copy(data, chunk0, 15);
            if (Checkcode(chunk0) == data[15])
            {
                Array.Copy(chunk0, bCardNo, 9);
                CardNo = bcd2Str(bCardNo);

                Array.Copy(chunk0, 9, bHosNo, 0, 5);
                HosNo = bcd2Str(bHosNo);

                bValid[0] = chunk0[14];
                valid = bcd2Str(bValid);
            }
        }

        /// <summary>
        /// 第4扇区 块1	发行日期	有效日期	启用日期		
        /// </summary>        
        public static Result WriteCommChunk1(string IssueDate, string validDate, string BeginDate)
        {
            byte[] chunk0 = new byte[15];
            byte[] bIssueDate = str2Bcd(IssueDate);//平台卡号
            byte[] bValidDate = str2Bcd(validDate);
            byte[] bBeginDate = str2Bcd(BeginDate);
            chunk0 = bIssueDate.Concat(bValidDate).Concat(bBeginDate).ToArray();
            byte[] data = new byte[16];
            Array.Copy(chunk0, data, chunk0.Length);
            data[15] = Checkcode(chunk0);
            //第4扇区 块1	发行日期	有效日期	启用日期	
            return rfCardDispenser.WirteBlock(4, 1, false, KeyValue, data);         
        }

        /// <summary>
        /// 第4扇区 块1	发行日期	有效日期	启用日期		
        /// </summary>        
        public static void ReadCommChunk1(out string IssueDate, out string validDate, out string BeginDate)
        {
            IssueDate = string.Empty;
            validDate = string.Empty;
            BeginDate = string.Empty;
            byte[] data;
            Result<byte[]> result = rfCardDispenser.ReadBlock(4, 1, false, KeyValue);
            if (!result.IsSuccess)
            {
                return;
            }
            data = result.Value;
        
            byte[] chunk0 = new byte[15];
            byte[] bIssueDate = new byte[4];
            byte[] bvalidDate = new byte[4];
            byte[] bBeginDate = new byte[4];
            Array.Copy(data, chunk0, 15);
            if (Checkcode(chunk0) == data[15])
            {
                Array.Copy(chunk0, bIssueDate, 4);
                IssueDate = bcd2Str(bIssueDate);

                Array.Copy(chunk0, 4, bvalidDate, 0, 4);
                validDate = bcd2Str(bvalidDate);

                Array.Copy(chunk0, 8, bBeginDate, 0, 4);
                BeginDate = bcd2Str(bBeginDate);
            }
        }

        /// <summary>
        /// 第4扇区 块2	平台患者流水号		
        /// </summary>        
        public static Result WriteCommChunk2(string patientId)
        {
            byte[] chunk0 = new byte[15];
            byte[] bPatientId = str2Bcd(patientId);//平台患者流水号            

            byte[] data = new byte[16];
            int len = bPatientId.Length;
            Array.Copy(bPatientId, 0, chunk0, 15 - len, len);
            Array.Copy(chunk0, data, chunk0.Length);
            data[15] = Checkcode(chunk0);
            //第4扇区 块2	平台患者流水号	  
            return rfCardDispenser.WirteBlock(4, 2, false, KeyValue, data);            
        }
        /// <summary>
        /// 第4扇区 块2	平台患者流水号		
        /// </summary>        
        public static void ReadCommChunk2(out string patientId)
        {
            patientId = string.Empty;
            byte[] data ;
            Result<byte[]> result = rfCardDispenser.ReadBlock(4, 2, false, KeyValue);
            if (!result.IsSuccess)
            {
                return;
            }
            data = result.Value;

            byte[] chunk0 = new byte[15];

            Array.Copy(data, chunk0, 15);
            if (Checkcode(chunk0) == data[15])
            {
                patientId = bcd2Str(chunk0);
            }
            patientId = patientId.TrimStart('0');
        }
        /// <summary>
        /// //第5扇区 块0 姓名、性别
        /// </summary>
        /// <param name="name"></param>
        /// <param name="sex"></param>
        public static Result WritePatientChunk0(string name, string sex)
        {
            byte[] chunk0 = new byte[15];
            byte[] patientName = System.Text.Encoding.GetEncoding("GB2312").GetBytes(name);//姓名
            Array.Copy(patientName, chunk0, patientName.Length);
            if (sex == "男")//性别
            {
                chunk0[14] = 1;
            }
            else
            {
                chunk0[14] = 2;
            }
            byte[] data = new byte[16];
            Array.Copy(chunk0, data, chunk0.Length);
            data[15] = Checkcode(chunk0);
            //第5扇区 块0 姓名、性别   
            return rfCardDispenser.WirteBlock(5, 0, false, KeyValue, data);         
        }
        /// <summary>
        /// //第5扇区 块0 姓名、性别
        /// </summary>
        /// <param name="name"></param>
        /// <param name="sex"></param>
        public static void ReadPatientChunk0(out string name, out string sex)
        {
            name = string.Empty;
            sex = string.Empty;            
            byte[] data;
            Result<byte[]> result = rfCardDispenser.ReadBlock(5, 0, false, KeyValue);
            if (!result.IsSuccess)
            {
                return;
            }
            data = result.Value;
    
            byte[] chunk0 = new byte[15];
            Array.Copy(data, chunk0, 15);
            if (Checkcode(chunk0) == data[15])
            {
                name = Encoding.GetEncoding("GB2312").GetString(chunk0);
                int end = name.IndexOf('\0');
                name = name.Substring(0, end);
                if (chunk0[14] == 1)
                {
                    sex = "男";
                }
                else
                {
                    sex = "女";
                }
            }
        }
        /// <summary>
        /// //第5扇区 块1和2 证件类型、证件号、联系方式
        /// </summary>       
        public static Result WritePatientChunk1(EnumM1IdType enumM1IdType, string idNo, string phone)
        {
            string idType = enumM1IdType.GetHashCode().ToString();
            byte[] chunk1 = new byte[16];
            byte[] chunk2 = new byte[15];
            byte[] chunk = new byte[31];
            byte[] data2 = new byte[16];
            chunk1[0] = 0;
            chunk1[1] = str2Bcd(idType)[0];//证件类型
            byte[] bIDNo = Encoding.ASCII.GetBytes(idNo);//证件号
            Array.Copy(bIDNo, 0, chunk1, 2, 14);

            Array.Copy(bIDNo, 14, chunk2, 0, 4);
            byte[] bPhone = Encoding.ASCII.GetBytes(phone);//联系方式
            Array.Copy(bPhone, 0, chunk2, 4, 11);

            chunk = chunk1.Concat(chunk2).ToArray();
            Array.Copy(chunk2, data2, chunk2.Length);
            data2[15] = Checkcode(chunk);
            //第5扇区 块1
            Result result = rfCardDispenser.WirteBlock(5, 1, false, KeyValue, chunk1);

            if(!result.IsSuccess)
            {
                return result;
            }
            //块2
            return rfCardDispenser.WirteBlock(5, 2, false, KeyValue, data2);            
        }

        /// <summary>
        /// //第5扇区 块1和2 证件类型、证件号、联系方式
        /// </summary>       
        public static void ReadPatientChunk1(out string idType, out string idNo, out string phone)
        {
            idType = string.Empty;
            idNo = string.Empty;
            phone = string.Empty;
            byte[] data2, chunk1;
            Result<byte[]> result = rfCardDispenser.ReadBlock(5, 1, false, KeyValue);
            if (!result.IsSuccess)
            {
                return;
            }
            chunk1 = result.Value;
            result = rfCardDispenser.ReadBlock(5, 2, false, KeyValue);
            if (!result.IsSuccess)
            {
                return;
            }
            data2 = result.Value;                     
            byte[] chunk2 = new byte[15];
            byte[] chunk = new byte[31];
            Array.Copy(data2, chunk2, 15);
            chunk = chunk1.Concat(chunk2).ToArray();
            if (Checkcode(chunk) == data2[15])
            {
                idType = chunk1[1].ToString();
                idNo = Encoding.ASCII.GetString(chunk1, 2, 14) + Encoding.ASCII.GetString(chunk2, 0, 4);
                phone = Encoding.ASCII.GetString(chunk2, 4, 11);
            }
        }
    }
}

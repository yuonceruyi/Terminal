using System;
using System.IO;
using System.Text;
using System.Windows.Forms;
using YuanTu.Consts;
using YuanTu.Core.Systems;
using static YuanTu.QDArea.QingDaoSiPay.Common.ExternFun;

namespace YuanTu.QDArea.QingDaoSiPay.Common
{
    /// <summary>
    /// [功能描述: 医保接口基础类]<br></br>
    /// [创 建 者: guocl]<br></br>
    /// [创建时间: 2015-11-13]<br></br>
    /// </summary>
    public class Comm
    {
        #region 常量
        private const string SUCCESS = "00000";
        private const int BACKCODE_START = 25;
        private const int BACKCODE_LEN = 5;
        private const int MESBODY_START = 226;
        private const int MESBODY_LEN = 3766;
        private const int EXCHANGE_START = MESBODY_START + MESBODY_LEN;
        private const int EXCHANGE_LEN = 100;
        //特殊门诊大病病种{C46E6721-3AE8-43c0-BAED-2625AAE7BDBC}
        //恶性肿瘤放化疗、白血病
        public const string DISEASE_DBSP = "DB022|DB024";
        #region Send Message header
        private const string BEGINCODE = "SSSS";
        private static string RECEIVETIME = string.Empty.PadLeft(16, '\x20');
        private static string BACKCODE = string.Empty.PadLeft(5, '\x20');
        private static string VERIFYCODE = string.Empty.PadLeft(64, '\x20');
        private static string VERSIONCODE = string.Empty.PadLeft(7, '\x20');//版本号暂时为空，如果医保要求必填需修改
        private static string RESERVECODE = string.Empty.PadLeft(100, '\x20');//系统预留
        #endregion

        #region Send Message ending
        private static string EXCHANGECODE = string.Empty.PadLeft(100, '\x20');
        private const string ENDCODE = "ZZZZ";
        #endregion

        /// <summary>
        /// 日期时间格式
        /// </summary>
        public const string DATE_TIME_FORMAT = "yyyyMMdd/hhmmss/";
        /// <summary>
        /// 日期格式
        /// </summary>
        public const string DATE_FORMAT = "yyyyMMdd";

        #endregion

        #region 变量
        

        #endregion

        #region 方法
        /// <summary>
        /// 获取消息头
        /// </summary>
        /// <param name="messageType">信息类型；(NOTNULL)</param>
        /// <returns>消息头</returns>
        public static string getSendHeader(string messageType)
        {
            if (Comm.GetLength(messageType) != 5)
            {
                MessageBox.Show("消息类型码错误！");
            }             
            string header = BEGINCODE;
            header = header + RECEIVETIME + messageType + BACKCODE;

            string siCode = SiSet.SiHosCode;
            string terminalCode = SiSet.terminalCode;
            string operatorCode = SiSet.operatorCode;           
            operatorCode = rightPad(operatorCode, 10);

            header = header + rightPad(siCode, 5) + rightPad(terminalCode, 10) + VERIFYCODE + operatorCode;
            header = header + VERSIONCODE + RESERVECODE;
            return header;
        }
        /// <summary>
        /// 获取消息尾
        /// </summary>
        /// <returns>消息尾</returns>
        public static string getSendEnding()
        {
            string ending = EXCHANGECODE + ENDCODE;
            return ending;
        }

        /// <summary>
        /// 调用医保接口
        /// </summary>
        /// <returns>消息尾</returns>
        public static bool SendMessage(string messageType, string sendMessage, ref string receiveMessage, ref string exchangeMessage)
        {
            string pDatainput = getSendHeader(messageType) + rightPad(sendMessage, MESBODY_LEN) + getSendEnding();
            //pDatainput = "SSSS                X0001     100111234567890                                                                12345678901234567                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                              ZZZZ";
            StringBuilder pDataOutput = new StringBuilder(4096);
            string sDataOutput;

            //调用医保接口
            writeLog(pDatainput);
            HookStop();
            HookStart();
            ulong sign = ExternFun.SendPos(pDatainput, pDataOutput);
            HookStop();
            sDataOutput = pDataOutput.ToString();
            writeLog(sDataOutput);

            int len = GetLength(sDataOutput);//字符串总长度
            string backCode = GetSubString(sDataOutput, BACKCODE_START, BACKCODE_LEN);
            //通过返回码判断是否成功
            if (backCode.Equals(SUCCESS))
            {
                receiveMessage = GetSubString(sDataOutput, MESBODY_START, MESBODY_LEN);
                exchangeMessage = GetSubString(sDataOutput, EXCHANGE_START, EXCHANGE_LEN).Trim();
            }
            else
            {
                if (len >= EXCHANGE_START + EXCHANGE_LEN)
                {
                    //LocalManager localManager = new LocalManager();
                    //string errMessage = localManager.getErrMessage(backCode); //提取错误信息

                    exchangeMessage = " " + backCode + " " + GetSubString(sDataOutput, EXCHANGE_START, EXCHANGE_LEN).Trim();
                }
                return false;
            }
            return true;
        }
        /// <summary>
        /// 调用医保接口
        /// </summary>
        /// <returns>消息尾</returns>
        public static bool SendMessage(string messageType, string sendMessage, ref string receiveMessage, ref string exchangeMessage, ref string refbackCode)
        {
            string pDatainput = getSendHeader(messageType) + rightPad(sendMessage, MESBODY_LEN) + getSendEnding();
            //pDatainput = "SSSS                X0001     100111234567890                                                                12345678901234567                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                              ZZZZ";
            StringBuilder pDataOutput = new StringBuilder(4096);
            string sDataOutput;

            //调用医保接口
            writeLog(pDatainput);
            HookStart();
            switch (messageType)
            {
                case "":
                case "CX002":
                case "CX005":
                    ulong sign = ExternFun.SendRcv(pDatainput, pDataOutput);
                    break;
                default:
                    sign = ExternFun.SendPos(pDatainput, pDataOutput);
                    break;
            }
            HookStop();
            sDataOutput = pDataOutput.ToString();
            writeLog(sDataOutput);

            int len = GetLength(sDataOutput);//字符串总长度
            string backCode = GetSubString(sDataOutput, BACKCODE_START, BACKCODE_LEN);
            refbackCode = backCode;
            //通过返回码判断是否成功
            if (backCode.Equals(SUCCESS))
            {
                receiveMessage = GetSubString(sDataOutput, MESBODY_START, MESBODY_LEN);
                exchangeMessage = GetSubString(sDataOutput, EXCHANGE_START, EXCHANGE_LEN).Trim();
            }
            else
            {
                if (len >= EXCHANGE_START + EXCHANGE_LEN)
                {
                    //LocalManager localManager = new LocalManager();
                    //string errMessage = localManager.getErrMessage(backCode); //提取错误信息

                    exchangeMessage = " " + backCode + " " + GetSubString(sDataOutput, EXCHANGE_START, EXCHANGE_LEN).Trim();
                }
                return false;
            }
            return true;
        }
        /// <summary>
        /// 特殊业务用直接调用医保接口
        /// </summary>
        /// <returns>消息尾</returns>
        public bool SendMessage(string sendMessage, ref string receiveMessage, ref string exchangeMessage)
        {
            string pDatainput = sendMessage;

            StringBuilder pDataOutput = new StringBuilder(4096);
            string sDataOutput;

            //调用医保接口
            writeLog("特殊业务" + pDatainput);
            ulong sign = ExternFun.SendRcv(pDatainput, pDataOutput);
            sDataOutput = pDataOutput.ToString();
            writeLog("特殊业务" + sDataOutput);

            int len = GetLength(sDataOutput);//字符串总长度
            string backCode = GetSubString(sDataOutput, BACKCODE_START, BACKCODE_LEN);
            //通过返回码判断是否成功
            if (backCode.Equals(SUCCESS))
            {
                receiveMessage = GetSubString(sDataOutput, MESBODY_START, MESBODY_LEN);
                exchangeMessage = GetSubString(sDataOutput, EXCHANGE_START, EXCHANGE_LEN).Trim();
            }
            else
            {
                if (len >= EXCHANGE_START + EXCHANGE_LEN)
                {
                    
                    string errMessage = SiSet.SiMessage[backCode]; //提取错误信息

                    if (string.IsNullOrEmpty(errMessage))//本地无提示信息从消息交换体取
                    {
                        receiveMessage = GetSubString(sDataOutput, EXCHANGE_START, EXCHANGE_LEN).Trim();
                        exchangeMessage = GetSubString(sDataOutput, EXCHANGE_START, EXCHANGE_LEN).Trim();
                    }
                    else
                    {
                        receiveMessage = errMessage;
                        exchangeMessage = errMessage;
                    }
                }
                return false;
            }
            return true;
        }

        #region 医保性别转换
        /// <summary>
        /// 医保性别转换
        /// </summary>
        /// <param name="SexCode"></param>
        /// <returns></returns>
        public string ConvertSex(string SexCode)
        {
            switch (SexCode)
            {
                case "2":
                    {
                        return "F";
                    }
                case "1":
                    {
                        return "M";
                    }
                case "9":
                    {
                        return "O";
                    }
                case "M":
                    {
                        return "1";
                    }
                case "F":
                    {
                        return "2";
                    }
                case "O":
                    {
                        return "9";
                    }
                case "A":
                    {
                        return "9";
                    }
                case "U":
                    {
                        return "9";
                    }
                default:
                    return "O";                   
            }
        }
        #endregion

        #region 性别代码含义
        /// <summary>
        /// 性别代码含义
        /// </summary>
        /// <param name="SexCode"></param>
        /// <returns></returns>
        public string ConvertSexCode(string SexCode)
        {
            switch (SexCode)
            {
                case "1":
                    {
                        return "男";
                    }
                case "2":
                    {
                        return "女";
                    }
            }
            return string.Empty;
        }
        #endregion

        #region 定点类型含义
        /// <summary>
        /// 定点类型含义
        /// </summary>
        /// <param name="DDLX"></param>
        /// <returns></returns>
        public string ConvertDDLX(string DDLX)
        {
            switch (DDLX)
            {
                case "1":
                    {
                        return "离休定点";
                    }
            }
            return string.Empty;
        }
        #endregion

        #region 缴费类别代码含义
        /// <summary>
        /// 缴费类别含义
        /// </summary>
        /// <param name="PayCode"></param>
        /// <returns></returns>
        public string ConvertPayCode(string PayCode)
        {
            switch (PayCode)
            {
                case "1":
                    {
                        return "正常交";
                    }
                case "2":
                    {
                        return "未缴费";
                    }
                case "3":
                    {
                        return "已退收";
                    }
            }
            return string.Empty;
        }
        #endregion

        #region 签约类型代码含义
        /// <summary>
        /// 签约类型代码含义
        /// </summary>
        /// <param name="signType"></param>
        /// <returns></returns>
        public string ConvertSignType(string signType)
        {
            switch (signType)
            {
                case "1":
                    {
                        return "家庭医生";
                    }
                case "2":
                    {
                        return "社区门诊";
                    }
                case "3":
                    {
                        return "农民工";
                    }
            }
            return string.Empty;
        }
        #endregion

        #region 日期转换
        /// <summary>
        /// 时间日期格式B
        /// </summary>
        /// <param name=""></param>
        /// <returns></returns>
        public DateTime toDate(string strDate)
        {
            return DateTime.ParseExact(strDate, DATE_FORMAT, null);
        }
        /// <summary>
        /// 时间日期格式A
        /// </summary>
        /// <param name=""></param>
        /// <returns></returns>
        public DateTime toDateTime(string strDate)
        {
            return DateTime.ParseExact(strDate, DATE_TIME_FORMAT, null);
        }
        /// <summary>
        /// 时间日期格式B
        /// </summary>
        /// <param name=""></param>
        /// <returns></returns>
        public static string dateToString(DateTime date)
        {
            return date.ToString(DATE_FORMAT);
        }
        /// <summary>
        /// 时间日期格式A
        /// </summary>
        /// <param name=""></param>
        /// <returns></returns>
        public string dateTimeToString(DateTime date)
        {
            string strDate = date.ToString(DATE_TIME_FORMAT);
            strDate = strDate.Replace('-', '/');
            return strDate;
        }
        /// <summary>
        /// 时间日期格式B
        /// </summary>
        /// <param name=""></param>
        /// <returns></returns>
        public string strDateToUI(string strDate)
        {
            if (string.IsNullOrEmpty(strDate) || strDate.Length != 8)
            {
                strDate = string.Empty;
            }
            else
            {
                strDate = strDate.Substring(0, 4) + "年" + strDate.Substring(4, 2) + "月" + strDate.Substring(6, 2) + "日";
            }
            return strDate;
        }
        /// <summary>
        /// 时间日期格式A
        /// </summary>
        /// <param name=""></param>
        /// <returns></returns>
        public string strDateTimeToUI(string strDate)
        {
            //TO DO
            return null;
        }
        #endregion

        #region 字符串处理
        /// <summary>
        /// 获取字符串中指定位置开始的指定长度的字符串，支持汉字英文混合 汉字为2字节计数
        /// </summary>
        /// <param name="strSub">输入中英混合字符串</param>
        /// <param name="start">开始截取的起始位置</param>
        /// <param name="length">要截取的字符串长度</param>
        /// <returns></returns>
        public static string GetSubString(string strSub, int start, int length)
        {
            string temp = strSub;
            int j = 0, k = 0, p = 0;
            length = length < 0 ? 0 : length;//兼容异常情况guocl
            if (GetLength(temp) < start)//兼容异常情况guocl
            {
                return string.Empty;
            }

            CharEnumerator ce = temp.GetEnumerator();
            while (ce.MoveNext())
            {
                // j += (ce.Current > 0 && ce.Current < 255) ? 1 : 2;
                j += GetLength(ce.Current.ToString());//兼容异常情况guocl                      

                if (j <= start)
                {
                    p++;
                }
                else
                {
                    if (j <= length + start)
                    {
                        k++;
                    }
                    else
                    {
                        break;
                    }
                }
            }
            temp = temp.Substring(p, k);

            return temp;
        }

        /// <summary>           
        /// 得到字符串的长度，一个汉字算2个字符           
        /// </summary>           
        /// <param name="str">字符串</param>           
        /// <returns>返回字符串长度</returns>           
        public static int GetLength(string str)
        {
            if (string.IsNullOrEmpty(str))
            {
                return 0;
            }
            if (str.Length == 0)
            {
                return 0;
            }
            int minus = SubstringCount(str, "?");   //guocl乱码偏差处理         

            ASCIIEncoding ascii = new ASCIIEncoding();
            int tempLen = 0;
            byte[] s = ascii.GetBytes(str);
            for (int i = 0; i < s.Length; i++)
            {
                if ((int)s[i] == 63)
                {
                    tempLen += 2;
                }
                else
                {
                    tempLen += 1;
                }
            }
            return tempLen - minus;
        }
        /// <summary>        
        /// 计算字符串中子串出现的次数        
        /// </summary>        
        /// <param name="str">字符串</param>        
        /// <param name="substring">子串</param>       
        /// <returns>出现的次数</returns>        
        static int SubstringCount(string str, string substring)
        {
            if (string.IsNullOrEmpty(str))
            {
                return 0;
            }

            if (str.Contains(substring))
            {
                string strReplaced = str.Replace(substring, "");
                return (str.Length - strReplaced.Length) / substring.Length;
            }
            return 0;
        }
        /// <summary>
        /// 左填充、数字类型串左边填充0
        /// </summary>
        /// <param name="strOrigin">原始字符串</param>
        /// <param name="length">目标字符串的长度</param> 
        /// <returns></returns>
        public static string leftPad(String strOrigin, int length)
        {
            int originLen = GetLength(strOrigin);
            string firstChar = GetSubString(strOrigin, 0, 1);
            if (firstChar.Equals("-"))//处理负数
            {
                strOrigin = GetSubString(strOrigin, 1, originLen - 1);
                strOrigin = firstChar + string.Empty.PadLeft(length - originLen, '0') + strOrigin;
            }
            else
            {
                strOrigin = string.Empty.PadLeft(length - originLen, '0') + strOrigin;
            }
            return strOrigin;
        }

        /// <summary>
        /// 右填充、非数字型串右边填充空格
        /// </summary>
        /// <param name="strOrigin">要统计的字符串</param>
        /// <param name="length">目标字符串的长度</param> 
        /// <returns></returns>
        public static string rightPad(String strOrigin, int length)
        {
            // int minus = SubstringCount(strOrigin, "?");//乱码偏差处理放到GetLength中
            int originLen = GetLength(strOrigin);// -minus;
            strOrigin = strOrigin + string.Empty.PadRight(length - originLen, '\x20');
            return strOrigin;
        }

        /// <summary>
        /// 取得字串在字符串中的位置
        /// </summary>
        /// <param name="str"></param>
        /// <param name="strSub"></param>
        /// <param name="start"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public static int GetSubInString(string str, string strSub, int start, int count)
        {
            int pos = 0, posTmp = 0, lenSub = strSub.Length, lenTmp = 0, lenStar = 0;
            if (count <= 0)
            {
                return -1;
            }
            for (int i = 0; i < count; i++)
            {
                posTmp = str.IndexOf(strSub, start);
                if (posTmp < 0)
                {
                    return -1;
                }
                lenTmp = str.Length - posTmp - lenSub;
                lenStar = posTmp + lenSub;

                try
                {
                    str = str.Substring(lenStar, lenTmp);
                }
                catch 
                {
                    return -1;
                }
                start = 0;
                pos = pos + posTmp;
            }
            return pos + count - 1;
        }
        #endregion

        #region 记录接口调用日志
        /// <summary>
        /// 记录接口调用日志
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public static void writeLog(String message)
        {
            string dirName = AppDomain.CurrentDomain.BaseDirectory + "\\SiLog";
            string fileName = dirName + "\\" + DateTimeCore.Now.ToString("yyyy-MM-dd") + ".log";
            FileStream file;

            message = DateTimeCore.Now + "\t" + message;
            try
            {
                if (!Directory.Exists(dirName)) //判断目录是否存在
                {
                    Directory.CreateDirectory(dirName);//生成目录
                }

                if (!File.Exists(fileName)) //判断文件是否存在
                {
                    file = File.Create(fileName);
                    file.Close();
                }

                file = File.Open(fileName, FileMode.Append);
                using (StreamWriter writer = new StreamWriter(file, Encoding.Default))
                {
                    writer.WriteLine(message);
                    writer.Close();
                };
                file.Close();
            }
            catch
            {
                //MessageBox.Show("日志 文件操作失败！");
            }
        }
        #endregion


        //public void MedicalBalanceTypeToUI(string medicalTypeCode, string balanceTypeCode, ref string medicalTypeName, ref string balanceTypeName)
        //{
        //    EnumMedicalType enumMedicalType = (EnumMedicalType)(Convert.ToInt16(medicalTypeCode));
        //    medicalTypeName = enumMedicalType.ToString();
        //    //结算类别
        //    switch (enumMedicalType)
        //    {
        //        case EnumMedicalType.门诊医保:
        //            balanceTypeName = ((EnumBalanceTypeMY)(Convert.ToInt16(balanceTypeCode))).ToString();
        //            break;
        //        case EnumMedicalType.医保住院:
        //            balanceTypeName = ((EnumBalanceTypeZY)(Convert.ToInt16(balanceTypeCode, 16))).ToString();
        //            break;
        //        case EnumMedicalType.门诊工伤:
        //        case EnumMedicalType.工伤住院:
        //            balanceTypeName = ((EnumBalanceTypeIndustrialinjury)(Convert.ToInt16(balanceTypeCode))).ToString();
        //            break;
        //        case EnumMedicalType.门诊生育:
        //        case EnumMedicalType.生育住院:
        //            balanceTypeName = string.Empty;
        //            break;
        //    }
        //}

        /// <summary>
        /// 判断是否为数字字符串
        /// </summary>
        /// <param name="message"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public bool isNumberic(string message, ref decimal result)
        {
            //判断是否为数字字符串
            //是的话则将其转换为数字并将其设为out类型的输出值、返回true, 否则为false
            result = -1;   //result 定义为out 用来输出值
            try
            {
                //当数字字符串的为是少于4时，以下三种都可以转换，任选一种
                //如果位数超过4的话，请选用Convert.ToInt32() 和int.Parse()

                //result = int.Parse(message);
                //result = Convert.ToInt16(message);
                result = Convert.ToDecimal(message);
                return true;
            }
            catch
            {
                return false;
            }
        }
        /// <summary>
        /// 判断是否为日期格式
        /// </summary>
        /// <param name="message"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public bool isDate(string message)
        {
            //判断是否为日期格式
            try
            {
                Convert.ToDateTime(message);
                return true;
            }
            catch
            {
                return false;
            }
        }
        public string InDays(DateTime dt1, DateTime dt2)
        {
            TimeSpan d = dt2.Date - dt1.Date;
            int days = d.Days;
            if (days == 0)
            {
                days = 1;
            }
            return days.ToString();
        }
        /// <summary>
        /// 通过编码获取七大类费用名字
        /// </summary>
        /// <returns></returns>
        public string getFeeCodeName(string feeCode)
        {
            string feeCodeName = string.Empty;
            if (feeCode == "14")
            {
                feeCodeName = "药品";
            }
            else if (feeCode == "21")
            {
                feeCodeName = "检查检验";
            }
            else if (feeCode == "22")
            {
                feeCodeName = "治疗";
            }
            else if (feeCode == "23")
            {
                feeCodeName = "手术麻醉";
            }
            else if (feeCode == "24")
            {
                feeCodeName = "一次性材料费";
            }
            else if (feeCode == "27")
            {
                feeCodeName = "其他材料";
            }
            else if (feeCode == "26")
            {
                feeCodeName = "服务设施";
            }
            return feeCodeName;
        }
        #endregion

        #region 钩子
        static int hSiHook = 0;

        static HookProc siHookProc;

        public static void HookStart()
        {
            if (hSiHook == 0)
            {
                // 创建HookProc实例
                siHookProc = new HookProc(SiHookProc);
                // 设置线程钩子
                hSiHook = SetWindowsHookEx((int)HookType.WH_CBT, siHookProc, IntPtr.Zero, ExternFun.GetCurrentThreadId());
                // 如果设置钩子失败
                if (hSiHook == 0)
                {
                    HookStop();
                    //throw new Exception("SetWindowsHookEx failed.");
                }
            }
        }
        // 卸载钩子
        public static void HookStop()
        {
            bool sign = true;
            if (hSiHook != 0)
            {
                sign = UnhookWindowsHookEx(hSiHook);
                hSiHook = 0;
            }
            if (!sign)
            {
                //throw new Exception("SetWindowsHookEx failed.");
            }
        }
        private static int SiHookProc(int nCode, Int32 wParam, IntPtr lParam)
        {
            var winPtr = WindowHelper.FindWindow(null,"Terminal");
            if (winPtr != IntPtr.Zero)
            {
                WindowHelper.PostMessage(winPtr, (uint)(WindowHelper.WindowMessage.CLOSE), IntPtr.Zero, IntPtr.Zero);
            }                        
            return CallNextHookEx(hSiHook, nCode, wParam, lParam);
        }
        #endregion 钩子
    }
}

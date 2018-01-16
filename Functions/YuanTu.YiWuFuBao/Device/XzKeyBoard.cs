using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using YuanTu.Consts.FrameworkBase;
using YuanTu.Core.Log;

namespace YuanTu.YiWuFuBao.Device
{
    public class XzKeyBoard
    {
        private const string Prefix = "旭子键盘";
        private const string DevicePath = "External\\YiWuXz\\XZ_POS_Pay.dll";
        [DllImport(DevicePath, EntryPoint = "SIP_CRT_Reset", CharSet = CharSet.Ansi,
            CallingConvention = CallingConvention.Winapi)]
        private static extern int SIT_CRT_Reset(int nMode);

        [DllImport(DevicePath, EntryPoint = "SIT_CRT_Entry", CharSet = CharSet.Ansi,
            CallingConvention = CallingConvention.Winapi)]
        private static extern int SIT_CRT_Entry();

        [DllImport(DevicePath, EntryPoint = "SIT_CRT_DisEntry", CharSet = CharSet.Ansi,
            CallingConvention = CallingConvention.Winapi)]
        private static extern int SIT_CRT_DisEntry();

        [DllImport(DevicePath, EntryPoint = "SIT_CRT_GetStatus", CharSet = CharSet.Ansi,
            CallingConvention = CallingConvention.Winapi)]
        private static extern int SIT_CRT_GetStatus();

        [DllImport(DevicePath, EntryPoint = "SIT_CRT_Eject", CharSet = CharSet.Ansi,
            CallingConvention = CallingConvention.Winapi)]
        private static extern int SIT_CRT_Eject();

        [DllImport(DevicePath, EntryPoint = "SIT_CRT_Capture", CharSet = CharSet.Ansi,
            CallingConvention = CallingConvention.Winapi)]
        private static extern int SIT_CRT_Capture();

        [DllImport(DevicePath, EntryPoint = "SIT_CRT_MovePosition", CharSet = CharSet.Ansi,
            CallingConvention = CallingConvention.Winapi)]
        private static extern int SIT_CRT_MovePosition(char position);

        [DllImport(DevicePath, EntryPoint = "SIT_CRT_ReadTrack", CharSet = CharSet.Ansi,
            CallingConvention = CallingConvention.Winapi)]
        private static extern int SIT_CRT_ReadTrack();

        [DllImport(DevicePath, EntryPoint = "SIT_Trans_init", CharSet = CharSet.Ansi,
            CallingConvention = CallingConvention.Winapi)]
        private static extern int SIT_Trans_init();

        [DllImport(DevicePath, EntryPoint = "SIT_CRT_OpenPort", CharSet = CharSet.Ansi,
            CallingConvention = CallingConvention.Winapi)]
        private static extern int SIT_CRT_OpenPort(Int16 nPort, int nBaudrate);

        [DllImport(DevicePath, EntryPoint = "XZ_POS_Trans", CharSet = CharSet.Ansi,
            CallingConvention = CallingConvention.Winapi)]
        private static extern int XZ_POS_Trans(int nTransCode,
            string pMerchantCode,
            string pOperNo,
            string pAmount,
            string pBatchNo,
            string pSequenceNo,
            string pPosReferenceId,
            string pAuthorizationId,
            string pTransDate,
            StringBuilder pReturnInfo);

        [DllImport(DevicePath, EntryPoint = "SIT_Trans_UnInit", CharSet = CharSet.Ansi,
            CallingConvention = CallingConvention.Winapi)]
        public static extern int SIT_Trans_UnInit();
        [DllImport(DevicePath, EntryPoint = "SIT_EPP_ClosePort", CharSet = CharSet.Ansi,
            CallingConvention = CallingConvention.Winapi)]
        private static extern int SIT_EPP_ClosePort();//关闭键盘端口
        [DllImport(DevicePath, EntryPoint = "SIT_CRT_ClosePort", CharSet = CharSet.Ansi,
            CallingConvention = CallingConvention.Winapi)]
        private static extern int SIT_CRT_ClosePort();//关闭读卡器端口

        //int WINAPI SIT_CRT_ReadCardNumber(int *nCardLen,char *ucCardNumber);
        [DllImport(DevicePath, EntryPoint = "SIT_CRT_ReadCardNumber", CharSet = CharSet.Ansi,
            CallingConvention = CallingConvention.Winapi)]
        private static extern int SIT_CRT_ReadCardNumber(ref int cardLen, StringBuilder cardNumber);//读卡号

        [DllImport(DevicePath, EntryPoint = "SIT_EPP_UseEppPlainTextMode", CharSet = CharSet.Ansi,
             CallingConvention = CallingConvention.Winapi)]
        private static extern int SIT_EPP_UseEppPlainTextMode();

        [DllImport(DevicePath, EntryPoint = "SIT_EPP_ScanKeyPress", CharSet = CharSet.Ansi,
            CallingConvention = CallingConvention.Winapi)]
        private static extern int SIT_EPP_ScanKeyPress(out char c);

        [DllImport(DevicePath, EntryPoint = "SIT_EPP_GetPin", CharSet = CharSet.Ansi,
            CallingConvention = CallingConvention.Winapi)]
        private static extern int SIT_EPP_GetPin(int ucPinMinLen, int ucPinMaxLen, int autoReturnFlag);

        [DllImport(DevicePath, EntryPoint = "SIT_EPP_CloseEppPlainTextMode", CharSet = CharSet.Ansi,
            CallingConvention = CallingConvention.Winapi)]
        private static extern int SIT_EPP_CloseEppPlainTextMode();

        [DllImport(DevicePath, EntryPoint = "SIT_EPP_GetPinBlock", CharSet = CharSet.Ansi,
            CallingConvention = CallingConvention.Cdecl)]
        private static extern int SIT_EPP_GetPinBlock(int nMasterKeyId,
            int nWorkKeyId,
            int nPinBlockFomat,
            string cCardNumber,
            StringBuilder pucPinblockResult);


        public static bool Initialize()
        {
            return SIT_Trans_init() == 0;
        }
        private static Dictionary<int,string>_havingCardDic=new Dictionary<int, string>()
        {
            [1]="停卡位",
            [5]="IC卡位"
        }; 
        public static bool HaveCard()
        {
            var ret = SIT_CRT_GetStatus();
            if (_havingCardDic.ContainsKey(ret))
            {
                Logger.Device.Info($"[{Prefix}]获取到卡片，位置:{_havingCardDic[ret]}");
                return true;
            }
            return false;//停卡位，IC卡位
        }

        public enum ResetMode
        {
            Capture,
            Eject,
            Keep,
        }

        public static bool Reset(ResetMode mode = ResetMode.Eject)
        {
            var ret = SIT_CRT_Reset((int)mode);
            Logger.Device.Info($"[{Prefix}]初始化 Mode:{mode} 结果:{ret}");
            return ret == 0;
        }

        public static bool Eject()
        {
            var ret = SIT_CRT_Eject();
            Logger.Device.Info($"[{Prefix}]退卡 结果:{ret}");
            return ret == 0;
        }

        public static bool Read(out int ret)
        {
            ret = SIT_CRT_ReadTrack();
            Logger.Device.Info($"[{Prefix}]读卡 结果:{ret}");
            return ret >= 0;
        }

        public static bool GetPin()
        {
            return SIT_EPP_GetPin(6, 6, 0x01) == 0;
        }

        public static bool GetPinBlock(out string returnInfo)
        {
            var sb = NewSb();
            var ret = SIT_EPP_GetPinBlock(0, 0, 0x00, "000000000000", sb);
            returnInfo = sb.ToString();
            Logger.Device.Info($"[{Prefix}]获取最终密码 结果:{returnInfo}");
            return ret == 0;
        }

        public static void Scan(out char c)
        {
            SIT_EPP_ScanKeyPress(out c);
        }

        public static bool SetEnterCard(bool allow)
        {
            var ret = allow ? SIT_CRT_Entry() : SIT_CRT_DisEntry();
            Logger.Device.Info($"[{Prefix}]设置是否允许进卡 是否允许:{allow} 结果:{ret}");
            return ret == 0;
        }

        public static bool MoveCard()
        {
            var ret = SIT_CRT_MovePosition(Convert.ToChar(0x01));
            Logger.Device.Info($"[{Prefix}]移动卡位置 结果:{ret}");
            return ret == 0;
        }

        private static StringBuilder NewSb()
        {
            var sb = new StringBuilder(4096);
            sb.Append(' ', 1024);
            sb.Clear();
            return sb;
        }

        public static Result<Output> DoLogon()
        {
            var pReturnInfo = NewSb();
            var ret = XZ_POS_Trans(6, null, null, null, null, null, null, null, null, pReturnInfo);
            Logger.Device.Info($"[{Prefix}]签到 结果:{ret} 内容:{pReturnInfo}");
            var output = Output.Decipher(pReturnInfo.ToString(), ret);
            return new Result<Output>(ret == 0, ret, output?.错误信息, output);
        }

        public static int DoSale(int amount, out Output output)
        {
            var pReturnInfo = NewSb();
            var money = (amount / 100.0).ToString("F2");

            var ret = XZ_POS_Trans(1, null, null, money, null, null, null, null, null, pReturnInfo);
            Logger.Device.Info($"[{Prefix}]交易 结果:{ret} 内容:{pReturnInfo}");
            output = Output.Decipher(pReturnInfo.ToString(), ret);
            try
            {
                if (output.交易金额 != money)
                {
                    Logger.POS.Error($"[旭子键盘]银联金额不一致，回传：{output.交易金额 } 实际金额：{money}");
                    output.交易金额 = money;
                }

            }
            catch (Exception ex)
            {
                Logger.POS.Error($"[旭子键盘]交易操作发生异常,{ex.Message} {ex.StackTrace}");
            }
            return ret;
        }

        //public static bool DoSale2(int amount, out string output)
        //{
        //    var pReturnInfo = newSB();
        //    var ret = XZ_POS_Trans(1, null, null, (amount / 100.0).ToString("F2"), null, null, null, null, null, pReturnInfo);
        //    output = pReturnInfo.ToString();
        //    return ret == 0;
        //}

        public static bool DoQuery(out Output output)
        {
            var pReturnInfo = NewSb();
            var ret = XZ_POS_Trans(5, null, null, null, null, null, null, null, null, pReturnInfo);
            Logger.Device.Info($"[{Prefix}]查询 结果:{ret} 内容:{pReturnInfo}");
            output = Output.Decipher(pReturnInfo.ToString(), ret);
            return ret == 0;
        }

        public static bool DoRefund(int amount, string transSeq, string centerSeq, out Output output)
        {
            output = null;
            var success = false;
            try
            {
                var errcode = 0;
                var res = Read(out errcode);
                Logger.Device.Info($"[{Prefix}]撤销前读卡 结果:{res} 返回码:{errcode}");
                var pReturnInfo = NewSb();
                var ret = XZ_POS_Trans(2, null, null, (amount/100.0).ToString("F2"), null, transSeq, centerSeq, null,
                    null,
                    pReturnInfo);
                output = Output.Decipher(pReturnInfo.ToString(), ret);
                Logger.Device.Info($"[{Prefix}]撤销 结果:{ret} 内容:{pReturnInfo}");
                success = ret == 0;
                return success;
            }
            catch (Exception ex)
            {
                Logger.POS.Error($"[旭子键盘]撤销操作发生异常,{ex.Message} {ex.StackTrace}");
                return false;
            }
            finally
            {
                Logger.POS.Error(
                    $"[旭子键盘]执行POS消费撤销，结果:{success},总额:{amount},流水号:{transSeq} 中心流水号:{centerSeq} 输出:{output}");
            }

        }

        public class Output
        {
            public static readonly Dictionary<int, string> ErrorDictionary = new Dictionary<int, string>
            {
                {0, "承兑或交易成功"},
                {1, "查发卡行"},
                {2, "查发卡行的特殊条件"},
                {3, "无效商户"},
                {4, "没收卡"},
                {5, "不予承兑"},
                {6, "出错"},
                {7, "特殊条件下没收卡"},
                {9, "请求正在处理中"},
                {12, "无效交易"},
                {13, "无效金额"},
                {14, "无效卡号"},
                {15, "无此发卡行"},
                {19, "重新送入交易"},
                {20, "无效应答"},
                {21, "不做任何处理"},
                {22, "怀疑操作有误"},
                {23, "不可接受的交易费"},
                {25, "未能找到文件上记录"},
                {30, "格式错误"},
                {31, "银联不支持的银行"},
                {33, "过期的卡"},
                {34, "有作弊嫌疑"},
                {35, "受卡方与安全保密部门联系"},
                {36, "受限制的卡"},
                {37, "受卡方呼受理方安全保密部门(没收卡)"},
                {38, "超过允许的PIN试输入"},
                {39, "无此信用卡帐户"},
                {40, "请求的功能尚不支持"},
                {41, "丢失卡"},
                {42, "无此帐户"},
                {43, "被窃卡"},
                {44, "无此投资帐户"},
                {51, "无足够的存款"},
                {52, "无此支票账户"},
                {53, "无此储蓄卡账户"},
                {54, "过期的卡"},
                {55, "密码错误"},
                {56, "无此卡记录"},
                {57, "不允许持卡人进行的交易"},
                {58, "不允许终端进行的交易"},
                {59, "有作弊嫌疑"},
                {60, "受卡方与安全保密部门联系"},
                {61, "超出取款金额限制"},
                {62, "受限制的卡"},
                {63, "违反安全保密规定"},
                {64, "原始金额不正确"},
                {65, "超出取款次数限制"},
                {66, "受卡方呼受理方安全保密部门"},
                {67, "捕捉（没收卡）"},
                {68, "收到的回答太迟"},
                {75, "允许的输入PIN次数超限"},
                {77, "需要向网络中心签到"},
                {79, "脱机交易对帐不平"},
                {90, "日期切换正在处理"},
                {91, "发卡行或银联不能操作"},
                {92, "金融机构或中间网络设施找不到或无法达到"},
                {93, "交易违法、不能完成"},
                {94, "重复交易"},
                {95, "调节控制错"},
                {96, "系统异常、失效"},
                {97, "POS终端号找不到"},
                {98, "银联收不到发卡行应答"},
                {99, "PIN格式错"},
                {160, "MAC校验错"}
            };

            public string AID{get;set;}
            public string ATC{get;set;}
            public string TC{get;set;}
            public string Text{get;set;}
            public string TSI{get;set;}
            public string TVR{get;set;}
            public bool 成功{get;set;}
            public string 错误信息{get;set;}
            public string 发卡银行{get;set;}
            public string 交易金额{get;set;}
            public string 交易类型{get;set;}
            public string 交易时间{get;set;}
            public string 卡号{get;set;}
            public string 卡有效期{get;set;}
            public string 流水号{get;set;}
            public string 批次号{get;set;}
            public string 商户代码{get;set;}
            public string 收单银行{get;set;}
            public string 首选名称{get;set;}
            public string 授权号{get;set;}
            public string 输入方法{get;set;}
            public string 系统参考号{get;set;}
            public string 应用标签{get;set;}
            public string 终端号{get;set;}

            public static Output Decipher(string text, int ret)
            {
                Logger.POS.Error($"[{Prefix}]解析银联参数，返回值:{ret} 解析内容:{text}");

                if (text.IndexOf('|') < 0 || ret != 0)
                {
                    return new Output
                    {
                        Text = text,
                        成功 = false,
                        错误信息 = ret + " " + Parse(text)
                    };
                }
                var list = text.Split('|');
                Output output;
                if (list.Length < 20)
                    output = new Output
                    {
                        Text = text,
                        成功 = true,
                        交易金额 = list[0].Substring(8),
                        批次号 = list[1].Substring(3),
                        流水号 = list[2].Substring(3),
                        商户代码 = list[3].Substring(4),
                        终端号 = list[4].Substring(3),
                        收单银行 = list[5].Substring(4),
                        发卡银行 = list[6].Substring(4),
                        卡号 = list[7].Substring(3),
                        交易类型 = list[8].Substring(4),
                        系统参考号 = list[9].Substring(5),
                        交易时间 = list[10].Substring(4),
                        授权号 = list[11].Substring(3),
                        卡有效期 = list[12].Substring(4),
                        输入方法 = list[13].Substring(4)
                    };
                else
                    output = new Output
                    {
                        Text = text,
                        成功 = true,
                        交易金额 = list[0].Substring(8),
                        批次号 = list[1].Substring(3),
                        流水号 = list[2].Substring(3),
                        商户代码 = list[3].Substring(4),
                        终端号 = list[4].Substring(3),
                        收单银行 = list[5].Substring(4),
                        发卡银行 = list[6].Substring(4),
                        卡号 = list[7].Substring(3),
                        交易类型 = list[8].Substring(4),
                        系统参考号 = list[9].Substring(5),
                        交易时间 = list[10].Substring(4),
                        TC = list[11].Substring(2),
                        AID = list[12].Substring(3),
                        TVR = list[13].Substring(3),
                        TSI = list[14].Substring(3),
                        ATC = list[15].Substring(3),
                        应用标签 = list[16].Substring(4),
                        首选名称 = list[17].Substring(4),
                        授权号 = list[18].Substring(3),
                        卡有效期 = list[19].Substring(4),
                        输入方法 = list[20].Substring(4)
                    };
                return output;
            }

            public override string ToString()
            {
                var text = new StringBuilder(4096);
                if (!成功)
                {
                    text.Append("错误信息    :" + 错误信息 + "\n");
                    return text.ToString();
                }
                text.Append("交易金额    :" + 交易金额 + "\n");
                text.Append("批次号      :" + 批次号 + "\n");
                text.Append("流水号      :" + 流水号 + "\n");
                text.Append("商户代码    :" + 商户代码 + "\n");
                text.Append("终端号      :" + 终端号 + "\n");
                text.Append("收单银行    :" + 收单银行 + "\n");
                text.Append("发卡银行    :" + 发卡银行 + "\n");
                text.Append("卡号        :" + 卡号 + "\n");
                text.Append("交易类型    :" + 交易类型 + "\n");
                text.Append("系统参考号  :" + 系统参考号 + "\n");
                text.Append("交易时间    :" + 交易时间 + "\n");
                text.Append("TC          :" + (TC ?? "") + "\n");
                text.Append("AID         :" + (AID ?? "") + "\n");
                text.Append("TVR         :" + (TVR ?? "") + "\n");
                text.Append("TSI         :" + (TSI ?? "") + "\n");
                text.Append("ATC         :" + (ATC ?? "") + "\n");
                text.Append("应用标签    :" + (应用标签 ?? "") + "\n");
                text.Append("首选名称    :" + (首选名称 ?? "") + "\n");
                text.Append("授权号      :" + 授权号 + "\n");
                text.Append("有效期      :" + 卡有效期 + "\n");
                text.Append("输入方法    :" + 输入方法 + "\n");
                return text.ToString();
            }

            public static string Parse(string error)
            {
                try
                {
                    return ErrorDictionary[Convert.ToInt32(error.Substring(0, 2))];
                }
                catch (Exception)
                {
                    if (error.StartsWith("A0"))
                        return ErrorDictionary[160];
                    return error;
                }
            }
        }
    }
}

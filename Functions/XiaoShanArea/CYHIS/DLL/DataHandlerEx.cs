using System;
using System.Runtime.InteropServices;
using YuanTu.Consts;
using YuanTu.Core.Log;

namespace YuanTu.YuHangArea.CYHIS.DLL
{
    public static partial class DataHandler
    {
        public static string ExeName;
        private const string DllPath = "MediInfo.RunExe.dll";
        [DllImport(DllPath)]
        private static extern int RunExe(string sExePath, string sExeIn,
            [MarshalAs(UnmanagedType.VBByRefStr)] ref string sExeOut, int iTimeOut);

        public static int RunExe(string input, out string output, int operation)
        {
            output = "".PadRight(1024);
            var exePath = FrameworkConst.RootDirectory + @"\zzjs\" + ExeName;
            Logger.Net.Info("RunExe " + ExeName + " IN:" + input);
            if (FrameworkConst.FakeServer)
            {
                switch (operation)
                {
                    case 1: //"查询建档":
                        //output = "00|测试|B14345317|1|339005199111265130|1991.11.26|84|532.65|318.34|44.13|15868848044";
                        output = "00|祝远红|04120148|2|332501197808090023|1978.08.09|88|0.00|0.00|161.72|";
                      //  output = "-1|SYSTEM|业务处理失败：市民卡余额不足!";
                        break;
                    case 2: //"挂号取号":
                            //output =
                            //      "00|白冬丽|70000011|2015.08.10|0.00|10.00|10.00|0.00|0.00|10.00|康复医学|||上午6号|99000004387|";
                             output = "00|白冬丽|70000011|2015.08.06|0.00|10.00|10.00|0.00|0.00|10.00|创伤骨科副高||胡建鑫|11号|4337|";
                       // output = "- 1 | SYSTEM | 业务处理失败：市民卡余额不足!";
                        break;
                    case 3: //"缴费预结算":
                             output = "00|用户姓名:刘孝|单据总金额:19.00|医保报销金额:0|应付金额:19.00";
                        //output = "-1|SYSTEM|业务处理失败：市民卡余额不足!";

                        break;
                    case 4: //"缴费结算":
                        //output = "00|朱校平|杭州市-<市民卡医疗支付>结算成功！|201602310000001|45.11|0.00|45.11|0.00|0.00|155.89|2016.01.13 10:54:14|如需发票请到收费窗口打印|";
                        //output= "00|用户姓名:测试病人1|杭州市-<市民卡医疗支付>结算成功！|电脑号:99000123737|单据总金额:4.48|医保报销金额:0|应付金额:4.48|结算日期:2015-6-11 16:53:53|如需发票请到门诊打印|";
                        //output="00|用户姓名:白冬丽|杭州市-<市民卡医疗支付>结算成功！|电脑号:99000004619|单据总金额:0.07|医保报销金额:0.07|应付金额:0|医保本年账户余额:600.85|医保历年账户余额:318.46|智慧医疗账户余额:2.01|结算日期:2015/8/22 14:50:55|如需发票请到门诊打印|取药地址:";
                        output="00|用户姓名:测试病人1|杭州市-<市民卡医疗支付>结算成功！|电脑号:99000123737|单据总金额:4.48|医保报销金额:0|应付金额:4.48|医保本年账户余额:0|医保历年账户余额:0|智慧医疗账户余额:60.52|结算日期:2015-6-11 16:53:53|如需发票请到门诊打印|取药地址: 西药房0";
                        break;
                }

            }
            else
            {
                var i = RunExe(exePath, input.PadRight(1024), ref output, 0);
                Logger.Net.Info("RunExe " + ExeName + " OUT:" + i + " " + output);
                if (i != 0)
                    return i;
                if (input==output)
                {
                    return -4;
                }
                if (!output.Contains("|"))
                    return -2;
                int j;
                string ret = output.Substring(0, output.IndexOf('|'));
                if (int.TryParse(ret, out j))
                {
                    if (j != 0)
                        throw new Exception(output.Substring(output.IndexOf('|')));
                    return j;
                }
                return -3;
               }
                return 0;
        }
	}
}
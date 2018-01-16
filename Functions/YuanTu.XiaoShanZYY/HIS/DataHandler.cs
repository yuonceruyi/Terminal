using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using YuanTu.Consts;
using YuanTu.Consts.FrameworkBase;
using YuanTu.Core.Log;
using YuanTu.XiaoShanZYY.HIS;

namespace YuanTu.XiaoShanZYY.HIS
{
    class RunExeHelper
    {
        public static string ExePath { get; set; }

        [DllImport("MediInfo.RunExe.dll")]
        private static extern int RunExe(string sExePath, string sExeIn,
            [MarshalAs(UnmanagedType.VBByRefStr)] ref string sExeOut, int iTimeOut);

        public static Result<string> RunExe(string input, int operation)
        {
            Logger.Net.Info($"RunExe {ExePath} IN:{input}");

            int i;
            var output = String.Empty.PadRight(1024);
            if (Startup.HISDll)
            {
                i = 0;
                output = RunExeMock(operation);
            }
            else
            {
                i = RunExe(ExePath, input.PadRight(1024), ref output, 90 * 1000);
                output = output.Trim('\0', ' ');
            }

            Logger.Net.Info($"RunExe {ExePath} OUT:{i} {output}");
            if (i != 0)
                return Result<string>.Fail($"{i}");

            if (output == input)
                return Result<string>.Fail(-4, $"-4: 返回数据与入参相同");

            var p = output.IndexOf('|');
            if (p < 0)
                return Result<string>.Fail(-2, $"-2: 返回数据缺少分隔符:{output}");

            string ret = output.Substring(0, p);
            if (!int.TryParse(ret, out int j))
                return Result<string>.Fail(-3, $"-3: 返回数据:{output}");

            if (j != 0)
                return Result<string>.Fail(j, $"{j}: {output.Substring(p + 1)}");
            return Result<string>.Success(output);
        }

        private static string RunExeMock(int operation)
        {
            string output = String.Empty;
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
                    output = "00|沈小英|70008287|2017.08.17|0.00|10.00|0.00|1.00|0.00|0.00|内科|门诊三楼内科|住院部|下午|99000540165||9.00";
                    //output = "-1|SYSTEM|业务处理失败：市民卡余额不足!";
                    break;
                case 3: //"缴费预结算":
                    output = "00|用户姓名：刘孝|单据总金额：19.00|医保报销金额：0|市民卡余额：0|应付金额：0.01";
                    //output = "-1|SYSTEM|业务处理失败：市民卡余额不足!";

                    break;
                case 4: //"缴费结算":
                    output = "00|朱校平|杭州市-<市民卡医疗支付>结算成功！|201602310000001|45.11|0.00|45.11|0.00|0.00|155.89|2016.01.13 10:54:14|如需发票请到收费窗口打印|";
                    //"00|用户姓名:测试病人1|杭州市-<市民卡医疗支付>结算成功！|电脑号:99000123737|单据总金额:4.48|医保报销金额:0|应付金额:4.48|结算日期:2015-6-11 16:53:53|如需发票请到门诊打印|";
                    //"00|用户姓名:白冬丽|杭州市-<市民卡医疗支付>结算成功！|电脑号:99000004619|单据总金额:0.07|医保报销金额:0.07|应付金额:0|医保本年账户余额:600.85|医保历年账户余额:318.46|智慧医疗账户余额:2.01|结算日期:2015/8/22 14:50:55|如需发票请到门诊打印|取药地址:";
                    //"00|用户姓名:测试病人1|杭州市-<市民卡医疗支付>结算成功！|电脑号:99000123737|单据总金额:4.48|医保报销金额:0|应付金额:4.48|医保本年账户余额:0|医保历年账户余额:0|智慧医疗账户余额:60.52|结算日期:2015-6-11 16:53:53|如需发票请到门诊打印|取药地址: 西药房0";
                    output = "-1|SYSTEM|业务处理失败：市民卡余额不足!";
                    output = "00|用户姓名：沈小英|杭州市-<市民卡医疗支付>结算成功！|电脑号：99000542754|单据总金额：16.00|医保报销金额：16.00|应付金额：0|医保本年账户余额：0|医保历年账户余额：0|智慧医疗账户余额：0|结算日期：2017/8/17 09:40:02|如需发票请到门诊打印|取药地址：";
                    break;
            }

            return output;
        }


        public static Result<T> RunExe<T>(ReqDll req, int operation)
            where T : DllRes, new()
        {
            var result = RunExe(req.ToString(), operation);
            if (!result.IsSuccess)
                return result.Convert().Convert<T>();
            var res = new T();
            res.Parse(result.Value);
            return Result<T>.Success(res);
        }
    }
}

namespace YuanTu.XiaoShanZYY
{
    public partial class DataHandler
    {
        public static Result<Res医院排班信息> 医院排班信息(Req医院排班信息 req)
        {
            return HISConnection.Handle<Res医院排班信息>(req);
        }
        public static Result<Res挂号医生信息> 挂号医生信息(Req挂号医生信息 req)
        {
            return HISConnection.Handle<Res挂号医生信息>(req);
        }
        public static Result<Res挂号号源信息> 挂号号源信息(Req挂号号源信息 req)
        {
            return HISConnection.Handle<Res挂号号源信息>(req);
        }
        public static Result<Res门诊费用明细> 门诊费用明细(Req门诊费用明细 req)
        {
            return HISConnection.Handle<Res门诊费用明细>(req);
        }
        public static Result<Res预约挂号处理> 预约挂号处理(Req预约挂号处理 req)
        {
            return HISConnection.Handle<Res预约挂号处理>(req);
        }
        public static Result<Res预约退号处理> 预约退号处理(Req预约退号处理 req)
        {
            return HISConnection.Handle<Res预约退号处理>(req);
        }
        public static Result<Res档案信息处理> 档案信息处理(Req档案信息处理 req)
        {
            return HISConnection.Handle<Res档案信息处理>(req);
        }
        public static Result<Res缴费结算查询> 缴费结算查询(Req缴费结算查询 req)
        {
            return HISConnection.Handle<Res缴费结算查询>(req);
        }
        public static Result<Res补打查询> 补打查询(Req补打查询 req)
        {
            return HISConnection.Handle<Res补打查询>(req);
        }
        public static Result<Res病人信息查询建档> 病人信息查询建档(ReqDll req)
        {
            return RunExeHelper.RunExe<Res病人信息查询建档>(req, 1);
        }
        public static Result<Res挂号取号> 挂号取号(ReqDll req)
        {
            return RunExeHelper.RunExe<Res挂号取号>(req, 2);
        }
        public static Result<Res预结算> 预结算(ReqDll req)
        {
            return RunExeHelper.RunExe<Res预结算>(req, 3);
        }
        public static Result<Res结算> 结算(ReqDll req)
        {
            return RunExeHelper.RunExe<Res结算>(req, 4);
        }
        public static Result<Res计算LIS试管费> 计算LIS试管费(ReqDll req)
        {
            return RunExeHelper.RunExe<Res计算LIS试管费>(req, 5);
        }
    }
}

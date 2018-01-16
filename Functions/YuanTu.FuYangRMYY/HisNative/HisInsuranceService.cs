using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using YuanTu.Consts;
using YuanTu.Consts.FrameworkBase;
using YuanTu.Core.Log;
using YuanTu.FuYangRMYY.HisNative.Models;
using YuanTu.FuYangRMYY.HisNative.Models.Base;

namespace YuanTu.FuYangRMYY.HisNative
{
    public static class HisInsuranceService
    {
        private static readonly string Spliter = $"OOO:YbLittle:OOO";
        
        public static  Result<社保挂号结算> InsuOPReg(int handle, string userId, string paddingRowid, string admSource,
            string admReasonId, string expString)
        {
            var resp= StartRun("OPReg", string.Join(Spliter, new[] {handle.ToString(), userId, paddingRowid , admSource , admReasonId , expString }));
            return InsuranceResponseBase<社保挂号结算>.Build(resp);
        }

        public static Result<社保读卡> ReadCard(int handle=0, string cardNo="", string userId="1", string cardType="1")
        {
            var resp= StartRun("ReadCard", string.Join(Spliter, handle.ToString(), cardNo, userId, cardType));
            return InsuranceResponseBase<社保读卡>.Build(resp);
        }

        public static Result<社保挂号冲销> OPRegDestroy(int handle, string userId, string rowId, string admSource,
            string admReasonId, string expString)
        {
            var resp = StartRun("OPRegDestroy", string.Join(Spliter, new[] { handle.ToString(), userId, rowId, admSource, admReasonId, expString }));
            return InsuranceResponseBase<社保挂号冲销>.Build(resp);
        }

        public static Result<社保缴费结算> OPDivide(int handle, string userId, string rowId, string admSource, string admReasonId,
            string expString, string cppFlag)
        {
            //0^医保结算表rowid^(个人支付+起付标准)^发票rowid+Chr(2)+支付方式1^基金支付+Chr(2)+支付方式2^医保账户支付
            //0^326714^0^2239368 6^0 31^10
            var resp = StartRun("OPDivide", string.Join(Spliter, new[] { handle.ToString(), userId, rowId, admSource, admReasonId, expString, cppFlag }));
            var ret= InsuranceResponseBase<社保缴费结算>.Build(resp);
            Logger.Main.Info($"[医保交易]缴费，原始信息:{BitConverter.ToString(Encoding.UTF8.GetBytes(ret.Value?.OriginStr??""))}");
            if (ret)
            {
                var arr = Regex.Split(ret.Value.OriginStr, "[\u0001-\u0009]"); //ret.Value.OriginStr.Split('\u0003');
                if (arr.Length>=3)
                {
                    var col1 = arr[0].Split('^');
                    ret.Value.医保结算表RowId = col1[1];
                    ret.Value.个人支付_起付标准 = col1[2];
                    ret.Value.发票rowId = col1[3];

                    var col2 = arr[1].Split('^');
                    ret.Value.基金支付 = col2[1];

                    var col3 = arr[1].Split('^');
                    ret.Value.医保账户支付 = col3[1];

                }
            }
            return ret;
        }

        private static string StartRun(string tradeName,string input)
        {
            Logger.Net.Info($"[医保交易][{tradeName}]传入：{input.Replace(Spliter," ")}");
            var watch = Stopwatch.StartNew();
            var path = @"C:\DHCInsurance\DLL\"; //Path.Combine(FrameworkConst.RootDirectory, "External", "LittleProgram");//
            var info = new ProcessStartInfo()
            {
                FileName = Path.Combine(path, "YbLittle.exe") ,
                WorkingDirectory = path,
                UseShellExecute = false,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
                Arguments = $"{tradeName} \"{input}\""
            };
            var da = Process.Start(info);
            var outputStr = da.StandardOutput.ReadToEnd().Trim();
            watch.Stop();
            var times = watch.ElapsedMilliseconds;
            Logger.Net.Info($"[医保交易][{tradeName}]返回：{outputStr} 耗时:{times}");
            return outputStr;
        }

    }
}

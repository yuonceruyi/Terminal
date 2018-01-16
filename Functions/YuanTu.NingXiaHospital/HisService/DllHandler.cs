using System;
using System.Diagnostics;
using System.IO;
using Newtonsoft.Json;
using YuanTu.Consts;
using YuanTu.Consts.FrameworkBase;
using YuanTu.Core.Extension;
using YuanTu.Core.Log;
using YuanTu.NingXiaHospital.HisService.Request;
using YuanTu.NingXiaHospital.HisService.Response;

namespace YuanTu.NingXiaHospital.HisService
{
    public class DllHandler
    {
        public static Result<ResQueryPrescription> QueryPrescription(string req)
        {
            try
            {
                var result = DoHisFunction(req, "M1");
                if (result)
                {
                    var resModel = JsonConvert.DeserializeObject<ResQueryPrescription>(result.Value);
                    return Result<ResQueryPrescription>.Success(resModel);
                }
                return Result<ResQueryPrescription>.Fail(result.Message);
            }
            catch (Exception e)
            {
                Logger.Net.Info($"[处方查询]异常：{e.Message}");
                return Result<ResQueryPrescription>.Fail(e.Message);
            }
        }

        public static Result<ResBill> Bill(string req)
        {
            try
            {
                var result = DoHisFunction(req, "M2");
                if (result)
                {
                    var resModel = JsonConvert.DeserializeObject<ResBill>(result.Value);
                    return Result<ResBill>.Success(resModel);
                }
                return Result<ResBill>.Fail(result.Message);
            }
            catch (Exception e)
            {
                Logger.Net.Info($"[缴费结算]异常：{e.Message}");
                return Result<ResBill>.Fail(e.Message);
            }
        }

        public static Result<ResBillResult> BillNotice(string req)
        {
            try
            {
                var result = DoHisFunction(req, "M3");
                if (result)
                {
                    var resModel = JsonConvert.DeserializeObject<ResBillResult>(result.Value);
                    return Result<ResBillResult>.Success(resModel);
                }
                return Result<ResBillResult>.Fail(result.Message);
            }
            catch (Exception e)
            {
                Logger.Net.Info($"[结算通知]异常：{e.Message}");
                return Result<ResBillResult>.Fail(e.Message);
            }
        }

        public static Result<ResPatientSign> PatientSign(string req)
        {
            try
            {
                var result = DoHisFunction(req, "M4");
                if (result)
                {
                    var resModel = JsonConvert.DeserializeObject<ResPatientSign>(result.Value);
                    return Result<ResPatientSign>.Success(resModel);
                }
                return Result<ResPatientSign>.Fail(result.Message);
            }
            catch (Exception e)
            {
                Logger.Net.Info($"[病人信息签约]异常：{e.Message}");
                return Result<ResPatientSign>.Fail(e.Message);
            }
        }

        public static Result<ResPatientSignInfoQuery> QueryPatientSignInfo(string klx, string kh)
        {
            try
            {
                var req = new
                {
                    request = new Request.Request
                    {
                        head = new ReqHeadPatientSignInfoQuery
                        {
                            klx = klx,
                            kh = kh
                        }
                    }
                };
                var result = DoHisFunction(req.ToJsonString(), "M5");
                if (result)
                {
                    var resModel = JsonConvert.DeserializeObject<ResPatientSignInfoQuery>(result.Value);
                    return Result<ResPatientSignInfoQuery>.Success(resModel);
                }
                return Result<ResPatientSignInfoQuery>.Fail(result.Message);
            }
            catch (Exception e)
            {
                Logger.Net.Info($"[病人信息签约查询]异常：{e.Message}");
                return Result<ResPatientSignInfoQuery>.Fail(e.Message);
            }
        }

        public static Result<string> DoHisFunction(string req, string type)
        {
            try
            {
                Logger.Net.Info($"{GetMethodName(type)} 通信构造参数:{req}");
                var workingDirectory = Path.Combine(FrameworkConst.RootDirectory, "External", "NingXiaHis");
                var p = new Process
                {
                    StartInfo =
                    {
                        WorkingDirectory = workingDirectory,
                        FileName = Path.Combine(workingDirectory, "NingxiaTransConsole.exe"),
                        UseShellExecute = false,
                        RedirectStandardInput = true,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        CreateNoWindow = true,
                        Arguments = $"{type} " + $"{req.Replace("\"", "^").Replace(" ", "")}"
                    }
                };
                p.Start();
                p.WaitForExit();
                Logger.Net.Info($"{GetMethodName(type)} 通信转换参数:{p.StartInfo.Arguments}");
                var outputStr = p.StandardOutput.ReadToEnd();
                Logger.Net.Info($"{GetMethodName(type)} 通信返回信息:{outputStr}");
                if (!string.IsNullOrEmpty(outputStr) && outputStr.Contains("TransConsoleRes="))
                {
                    var temp = outputStr.Replace("TransConsoleRes={\"response\":", "").Trim();
                    temp = temp.Substring(0, temp.Length - 1);
                    return Result<string>.Success(temp);
                }
                Logger.Net.Info($"{GetMethodName(type)} 通信异常，返回异常，请重新操作");
                return Result<string>.Fail($"{GetMethodName(type)} 通信异常，返回异常，请重新操作");
            }
            catch (Exception ex)
            {
                Logger.Net.Error($"[宁夏]通信异常:{ex.Message}");
                return Result<string>.Fail(ex.Message);
            }
        }

        private static string GetMethodName(string type)
        {
            switch (type)
            {
                case "M1": return "处方查询";
                case "M2": return "缴费结算";
                case "M3": return "结算通知";
                case "M4": return "卡签约";
                case "M5": return "签约查询";
                default: return "异常入参";
            }
        }
    }
}
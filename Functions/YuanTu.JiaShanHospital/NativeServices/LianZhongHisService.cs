﻿
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using YuanTu.JiaShanHospital.NativeServices.Dto;
using YuanTu.Consts;
using YuanTu.Core.Log;
using YuanTu.Consts.FrameworkBase;

namespace YuanTu.JiaShanHospital.NativeServices
{
    /// <summary>
    /// 联众HIS的本地化调用接口
    /// </summary>
    public static class LianZhongHisService
    {
        private static string Prefix = "联众本地EXE";
        public static string HisExePath = "";

        [DllImport("MediInfo.RunExe.dll")]
        private static extern int RunExe(string sExePath, string sExeIn,
            [MarshalAs(UnmanagedType.VBByRefStr)] ref string sExeOut, int iTimeOut);

        /// <summary>
        /// 为用户建档
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public static Result<string> ExcuteHospitalAddArchive(AddArchiveRequest request)
        {
            //"808080808#8###0100#姓名|男|1989-01-01|33333333|333333|333333#123123" 
            //"808080809#8###0200#|||||#123123"医保卡建档示例
            if (request == null) throw new ArgumentNullException(nameof(request));
            //var tmp = string.Format(inStr, request.CardNo, request.BussinessType, request.ArchiveType, request.Phone,request.HealthCareCardContent, request.TimeSpan);
            var tmp = $"{request.CardNo}#{request.BussinessType}###{request.ArchiveType}#{request.Name}|{request.Sex}|{request.Birthday}|{request.IdNumber}|{request.HomeAddress}|{request.Phone}#{request.TimeSpan}";

            var outPut = InternalRunExeInvoke("用户建档", tmp);
            if (!outPut.IsSuccess)
            {
                return outPut.ResultCode == -100 ? Result<string>.Fail(-100, outPut.Message) : Result<string>.Fail(outPut.Message);
            }
            var outArr = outPut.Value.Split(new[] { '#', '|' });
            if (outArr.Length <= 1)
            {
                return Result<string>.Fail("His 返回参数不正确");
            }
            if (outPut.Value.StartsWith("-1"))
            {
                return Result<string>.Fail(outArr.Last());
            }
            return Result<string>.Success(outPut.Value);
        }

        /// <summary>
        ///  获取挂号预结算    
        /// </summary>
        /// <param name="request">预结算请求核心数据</param>
        /// <returns>返回挂号预结算相关信息</returns>
        public static Result<PerRegisterPay> GetHospitalPerRegisterInfo(PerRegisterPayRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            var val = (int)request.PayFlag;
            const string inStr = "{0},{1}#{2}#{3}#{4}#{5}|{6}#{7}#{8}";
            var tmp = string.Format(inStr, request.CardNo, request.SelfPayTag, request.BussinessType, request.LoginName, request.Password,
                request.PaiBanId,
                (int)request.TimeFlag, (val + "" + (val == 3 || val == 4 ? "||" : "")), request.TimeSpan);
            var outPut = InternalRunExeInvoke("挂号预结算", tmp);
            if (!outPut.IsSuccess)
            {
                return outPut.ResultCode == -100 ? Result<PerRegisterPay>.Fail(-100, outPut.Message) : Result<PerRegisterPay>.Fail(outPut.Message);
            }
            var outArr = outPut.Value.Split(new[] { '#', '|' });

            if (outArr.Length <= 1)
            {
                return Result<PerRegisterPay>.Fail("His 返回参数不正确");
            }
            if (outPut.Value.StartsWith("-1"))
            {
                return Result<PerRegisterPay>.Fail(outArr.Last(), null);
            }
            return Result<PerRegisterPay>.Success(new PerRegisterPay()
            {
                PatientName = outArr[1],
                TotoalPay = outArr[2],
                ActualPay = outArr[3],
                DiscountPay = outArr[4],
                HealthCarePay = outArr[5],
                HospitalBalance = outArr[6],
                CitizenCardBalance = outArr[7],
                PreRegisterId = outArr[8],
            });

        }

        /// <summary>
        /// 执行挂号结算
        /// </summary>
        /// <param name="request">返回结算结果，包含病人基础信息</param>
        /// <returns></returns>
        public static Result<RegisterPay> ExcuteHospitalRegister(RegisterPayRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            const string inStr = "{0},{1}#{2}#{3}#{4}#{5}|{6}#{7}|{8}#{9}";
            var tradeInfo = $"{request.Account},{request.AlipayTradeNo}";
            var val = (int)request.PayFlag;
            var tmp = string.Format(inStr,
                request.CardNo,
                request.SelfPayTag,
                request.BussinessType,
                request.AlipayAmount,
                "",
                request.PaiBanId,
                (int)request.TimeFlag,
                val,
                tradeInfo,
                request.TimeSpan);
            var outPut = InternalRunExeInvoke("挂号结算", tmp);
            if (!outPut.IsSuccess)
            {
                return outPut.ResultCode == -100 ? Result<RegisterPay>.Fail(-100, outPut.Message) : Result<RegisterPay>.Fail(outPut.Message);
            }
            var outArr = outPut.Value.Split(new[] { '#', '|' });
            if (outArr.Length <= 1)
            {
                return Result<RegisterPay>.Fail("His 返回参数不正确");
            }
            if (outPut.Value.StartsWith("-1"))
            {
                return Result<RegisterPay>.Fail(outArr.Last());
            }
            return Result<RegisterPay>.Success(new RegisterPay()
            {
                //00#高杰#10.00|0|0|10.00|0|0|1003822502|98|请到一楼（西）儿科门诊候诊区候诊
                PatientName = outArr[1],
                TotoalPay = outArr[2],
                ActualPay = outArr[3],
                DiscountPay = outArr[4],
                HealthCarePay = outArr[5],
                HospitalBalance = outArr[6],
                CitizenCardBalance = outArr[7],
                PreRegisterId = outArr[8],
                RegisterNo = outArr[9],
                VisitingLocation = outArr[10]
            });

        }

        /// <summary>
        /// 获取缴费预结算信息
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public static Result<PerCheckout> GetHospitalPerCheckoutInfo(PerCheckoutRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            const string inStr = "{0},{1}#{2}#{3}#{4}##{5}#{6}#{7}";
            var val = (int)request.PayFlag;
            var tmp = string.Format(inStr, request.CardNo, request.SelfPayTag, request.BussinessType, request.LoginName, request.Password,
                (val + "" + (val == 3 || val == 4 ? $"|{ request.AlipayTradeNo}|{request.AlipayAmount}" : "")), request.CheckoutType, request.TimeSpan);
            var outPut = InternalRunExeInvoke("缴费预结算", tmp);
            if (!outPut.IsSuccess)
            {
                return outPut.ResultCode == -100 ? Result<PerCheckout>.Fail(-100, outPut.Message) : Result<PerCheckout>.Fail(outPut.Message);
            }
            var outArr = outPut.Value.Split(new[] { '#', '|' });
            if (outArr.Length <= 1)
            {
                return Result<PerCheckout>.Fail("His 返回参数不正确");
            }
            if (outPut.Value.StartsWith("-1"))
            {
                return Result<PerCheckout>.Fail(outArr.Last());
            }
            var aSpit = outPut.Value.Split('#');
            var bSpit = aSpit[2].Split('|');
            var cSpit = aSpit[3].Split('|');
            return Result<PerCheckout>.Success(new PerCheckout
            {
                PatientName = aSpit[1],
                TotalPay = bSpit[0],
                HealthCarePay = bSpit[1],
                DiscountPay = bSpit[2],
                ActualPay = bSpit[3],
                RoundingPay = bSpit[4],
                HospitalBalance = bSpit[5],
                CitienCardBalance = bSpit[6],
                DoctorTechIds = cSpit,
                PrescriptionId = aSpit[4]
            });

        }

        /// <summary>
        /// 执行缴费结算
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public static Result<Checkout> ExcuteHospitalCheckout(CheckoutRequest request)
        {
            if (request == null) { throw new ArgumentNullException(nameof(request)); }
            const string inStr = "{0},{1}#{2}#{3}#{4}##{5}|{6}#{7}#{8}";
            var val = (int)request.PayFlag;
            var tradeInfo = $"{request.Account},{request.AlipayTradeNo}";
            var tmp = string.Format(inStr,
                request?.CardNo,
                request.SelfPayTag,
                request.BussinessType,
                request?.AlipayAmount,
                "",
                val,
                tradeInfo,
                request?.CheckoutType ?? "",
                request?.TimeSpan ?? "");
            var outPut = InternalRunExeInvoke("缴费结算", tmp);
            if (!outPut.IsSuccess)
            {
                return outPut.ResultCode == -100 ? Result<Checkout>.Fail(-100, outPut.Message) : Result<Checkout>.Fail(outPut.Message);
            }
            var outArr = outPut.Value.Split(new[] { '#', '|' });
            if (outArr.Length <= 1)
            {
                return Result<Checkout>.Fail("His 返回参数不正确");
            }
            if (outPut.Value.StartsWith("-1"))
            {
                return Result<Checkout>.Fail(outArr.Last());
            }
            var aSpit = outPut.Value.Split('#');
            var bSpit = aSpit[2].Split('|');
            var cSpit = aSpit[3].Split('|');
            return Result<Checkout>.Success(new Checkout
            {
                PatientName = aSpit[1],
                TotalPay = bSpit[0],
                HealthCarePay = bSpit[1],
                DiscountPay = bSpit[2],
                ActualPay = bSpit[3],
                RoundingPay = bSpit[4],
                HospitalBalance = bSpit[5],
                CitienCardBalance = bSpit[6],
                DoctorTechIds = cSpit,
                PrescriptionId = aSpit[4],
                TakeDrugWindow = aSpit[5],
                BillId = aSpit[6],
            });
        }

        /// <summary>
        /// 获取预约取号预结算信息
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public static Result<PerGetTicketCheckout> GetHospitalPerGetTicketCheckoutInfo(
            PerGetTicketCheckoutRequest request)
        {
            //就诊卡号#业务类型#用户名#密码#预约ID|预约时间|上下午标志|挂号序号|挂号类别#结算方式#时间戳
            //00#姓名#合计费用|自负金额|优惠金额|医保金额|舍入金额|账户余额|市民卡余额|挂号ID|挂号序号|
            if (request == null) throw new ArgumentNullException(nameof(request));
            const string inStr = "{0},{1}#{2}#{3}#{4}#{5}|{6}|{7}|{8}|{9}#{10}#{11}";
            var val = (int)request.PayFlag;
            var tmp = string.Format(inStr,
                request.CardNo,
                request.SelfPayTag,
                request.BussinessType,
                request.LoginName,
                request.Password,
                request.AppointmentId,
                request.AppointmentTime.ToString("yyyy-MM-dd"),
                (int)request.DayTimeFlag,
                request.RegisterOrder,
                (int)request.RegisterType,
                (val + "" + (val == 3 || val == 4 ? $"|{ request.AlipayTradeNo}|{request.AlipayAmount}" : "")),
                request.TimeSpan);
            var outPut = InternalRunExeInvoke("取号预结算", tmp);
            if (!outPut.IsSuccess)
            {
                return outPut.ResultCode == -100 ? Result<PerGetTicketCheckout>.Fail(-100, outPut.Message) : Result<PerGetTicketCheckout>.Fail(outPut.Message);
            }
            var outArr = outPut.Value.Split(new[] { '#', '|' });
            if (outArr.Length <= 1)
            {
                return Result<PerGetTicketCheckout>.Fail("His 返回参数不正确");
            }
            if (outPut.Value.StartsWith("-1"))
            {
                return Result<PerGetTicketCheckout>.Fail(outArr.Last());
            }
            if (outArr.Length < 11)
            {
                return Result<PerGetTicketCheckout>.Fail("His 返回参数不正确(长度不够)");
            }
            //00#姓名#合计费用|自负金额|优惠金额|医保金额|账户余额|市民卡余额|挂号ID|挂号序号|
            return Result<PerGetTicketCheckout>.Success(new GetTicketCheckout()
            {
                PatientName = outArr[1],
                TotalPay = outArr[2],
                ActualPay = outArr[3],
                DiscountPay = outArr[4],
                HealthCarePay = outArr[5],
                HospitalBalance = outArr[6],
                CitienCardBalance = outArr[7],
                RegisterId = outArr[8],
                RegisterOrder = outArr[9]
            });
        }
        /// <summary>
        /// 执行预约取号结算
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public static Result<GetTicketCheckout> ExcuteHospitalGetTicketCheckout(GetTicketCheckoutRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            const string inStr = "{0},{1}#{2}#{3}#{4}#{5}|{6}|{7}|{8}|{9}#{10}|{11}#{12}";
            var val = (int)request.PayFlag;
            var tradeInfo = $"{request.Account},{request.AlipayTradeNo}";
            var tmp = string.Format(inStr,
                request.CardNo,
                request.SelfPayTag,
                request.BussinessType,
                request.AlipayAmount,
                "",
                request.AppointmentId,
                request.AppointmentTime.ToString("yyyy-MM-dd"),
                (int)request.DayTimeFlag,
                request.RegisterOrder,
                (int)request.RegisterType,
                val,
                tradeInfo,
                request.TimeSpan);
            var outPut = InternalRunExeInvoke("取号结算", tmp);
            if (!outPut.IsSuccess)
            {
                return outPut.ResultCode == -100 ? Result<GetTicketCheckout>.Fail(-100, outPut.Message) : Result<GetTicketCheckout>.Fail(outPut.Message);
            }
            var outArr = outPut.Value.Split(new[] { '#', '|' });
            if (outArr.Length <= 1)
            {
                return Result<GetTicketCheckout>.Fail("His 返回参数不正确");
            }
            if (outPut.Value.StartsWith("-1"))
            {
                return Result<GetTicketCheckout>.Fail(outArr.Last());
            }
            if (outArr.Length < 11)
            {
                return Result<GetTicketCheckout>.Fail("His 返回参数不正确(长度不够)");
            }
            return Result<GetTicketCheckout>.Success(new GetTicketCheckout()
            {
                PatientName = outArr[1],
                TotalPay = outArr[2],
                ActualPay = outArr[3],
                DiscountPay = outArr[4],
                HealthCarePay = outArr[5],
                HospitalBalance = outArr[6],
                CitienCardBalance = outArr[7],
                RegisterId = outArr[8],
                RegisterOrder = outArr[9],
                VisitingLocation = outArr[10]
            });
        }

        //  private static readonly AutoResetEvent ResetEvent = new AutoResetEvent(false);
        private static readonly object LockObj = new object();
        private static Result<string> InternalRunExeInvoke(string method, string input)
        {
            lock (LockObj)
            {

                const int timeout = 60 * 3; //2017年7月28日 9:36 石庆庆和医院领导商量 超时不退费并且超时时间变成3分钟

                string output = "".PadLeft(1024, ' ');
                var watch = Stopwatch.StartNew();

                Logger.Net.Info($"[{Prefix}][{method}]路径:{HisExePath},入参:{input}");
                int exeRes = 0;
                var res = Task.Factory.StartNew(() =>
                {
                    exeRes = RunExe(HisExePath, input, ref output, timeout + 5);
                }).Wait(1000 * timeout);

                watch.Stop();
                var timeelsp = watch.ElapsedMilliseconds;
                var outPut = output.Trim().Replace("\u0000", "").Replace("\\u0000", "").Trim();
                Logger.Net.Info($"[{Prefix}][{method}]出参:{outPut} 耗时:{timeelsp}ms 是否超时:{(res ? "否" : "是")}");
                if (res && !string.IsNullOrWhiteSpace(outPut) && exeRes != -9999)
                {
                    return Result<string>.Success(outPut);
                }
                else
                {
                    string tmp = null;
                    var sbuilder = new StringBuilder("HIS异常，强杀HIS进程：");

                    try
                    {
                        var processes = Process.GetProcesses();
                        var exename = Path.GetFileNameWithoutExtension(HisExePath);
                        sbuilder.AppendLine(
                            $"Exe:{exename}\r\n进程列表:{string.Join(",", processes.Select(p => p.ProcessName))}");
                        foreach (var process in processes)
                        {
                            if (exename.Equals(process.ProcessName, StringComparison.OrdinalIgnoreCase))
                            {
                                tmp = process.ProcessName;
                                sbuilder.AppendLine($"杀死进程,Id:{process.Id} Name:{process.ProcessName}");
                                process.Kill();
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Net.Error($"[{Prefix}][{method}]杀死His进程时发生异常,可能有用信息:{tmp}，原因：" + ex);
                    }
                    finally
                    {
                        Logger.Net.Info($"[{Prefix}][{method}]{sbuilder}");
                    }
                    return Result<string>.Fail(-100, "医院HIS服务通讯超时");
                }
            }
        }
    }
}

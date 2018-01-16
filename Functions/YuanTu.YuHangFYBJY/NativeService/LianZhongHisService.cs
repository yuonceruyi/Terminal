using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using YuanTu.Consts.FrameworkBase;
using YuanTu.Core.Log;
using YuanTu.YuHangFYBJY.NativeService.Dto;

namespace YuanTu.YuHangFYBJY.NativeService
{
    /// <summary>
    /// 联众HIS的本地化调用接口
    /// </summary>
    public static class LianZhongHisService
    {
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
            //W17927532#8###0200|11111111111|河南省三门峡市湖滨区崖底乡刘家渠１１０号    |411202198609153013#20111225#1450361607881
            //W33638247#8###0200|13777358591|江西省上饶市横峰县葛源镇石桥村下前山０４９号|362325198404202930#330110D15600000500767D5942182ABC#1000000000
            if (request == null) throw new ArgumentNullException(nameof(request));
            //var tmp = string.Format(inStr, request.CardNo, request.BussinessType, request.ArchiveType, request.Phone,request.HealthCareCardContent, request.TimeSpan);
            var tmp = $"{request.CardNo}#{request.BussinessType}###0200|{request.Phone}|{request.HomeAddress}|{request.IdNumber}#{request.HealthCareCardContent}#{request.TimeSpan}";

            var outPut = InternalRunExeInvoke(tmp);
            if (!outPut.IsSuccess)
            {
                return outPut;
            }
            var outArr = outPut.Value.Split(new[] { '#', '|' });
            if (outArr.Length <= 1)
            {
                return Result<string>.Fail("His 返回参数不正确", null);
            }
            if (outPut.Value.StartsWith("-"))
            {
                return Result<string>.Fail(outArr.Last(), null);
            }
            return Result<string>.Success("建档成功");
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
            const string inStr = "{0}#{1}#{2}#{3}#{4}|{5}#{6}#{7}";
            var tmp = string.Format(inStr, request.CardNo, request.BussinessType, request.LoginName, request.Password,
                request.PaiBanId,
                (int)request.TimeFlag, (val + "" + (val == 3 || val == 4 ? "||" : "")), request.TimeSpan);
            var outPut = InternalRunExeInvoke(tmp);
            if (!outPut.IsSuccess)
            {
                return Result<PerRegisterPay>.Fail(outPut.Message, null);
            }
            var outArr = outPut.Value.Split(new[] { '#', '|' });

            if (outArr.Length <= 1)
            {
                return Result<PerRegisterPay>.Fail("His 返回参数不正确");
            }
            if (outPut.Value.StartsWith("-"))
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
            const string inStr = "{0}#{1}#{2}#{3}#{4}|{5}#{6}#{7}";
            var val = (int)request.PayFlag;
            var tmp = string.Format(inStr, request.CardNo, request.BussinessType, request.LoginName, request.Password,
                request.PaiBanId,
                (int)request.TimeFlag, (val + "" + (val == 3 || val == 4 ? $"|{ request.AlipayTradeNo}|{request.AlipayAmount}" : "")), request.TimeSpan);
            var outPut = InternalRunExeInvoke(tmp);
            if (!outPut.IsSuccess)
            {
                return Result<RegisterPay>.Fail(outPut.Message);
            }
            var outArr = outPut.Value.Split(new[] { '#', '|' });
            if (outArr.Length <= 1)
            {
                return Result<RegisterPay>.Fail("His 返回参数不正确");
            }
            if (outPut.Value.StartsWith("-"))//负数结尾都是错
            {
                return Result<RegisterPay>.Fail(outArr.Last());
            }
            return Result<RegisterPay>.Success(new RegisterPay()
            {
                //00#鲁香瑛#10.00|0.80|1|8.20|0|0|1004723479|51|门诊一楼^请咨询护士台
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
            const string inStr = "{0}#{1}#{2}#{3}##{4}#{5}#{6}";
            var val = (int)request.PayFlag;
            var tmp = string.Format(inStr, request.CardNo, request.BussinessType, request.LoginName, request.Password,
                (val + "" + (val == 3 || val == 4 ? $"|{ request.AlipayTradeNo}|{request.AlipayAmount}" : "")), request.CheckoutType, request.TimeSpan);
            var outPut = InternalRunExeInvoke(tmp);
            if (!outPut.IsSuccess)
            {
                return Result<PerCheckout>.Fail(outPut.Message);
            }
            var outArr = outPut.Value.Split(new[] { '#', '|' });
            if (outArr.Length <= 1)
            {
                return Result<PerCheckout>.Fail("His 返回参数不正确");
            }
            if (outPut.Value.StartsWith("-"))
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
                CitizenCardBalance = bSpit[6],
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

            if (request == null) throw new ArgumentNullException(nameof(request));
            const string inStr = "{0}#{1}#{2}#{3}##{4}#{5}#{6}";
            var val = (int)request.PayFlag;
            var tmp = string.Format(inStr, request.CardNo, request.BussinessType, request.LoginName, request.Password,
                  (val + "" + (val == 3 || val == 4 ? $"|{ request.AlipayTradeNo}|{request.AlipayAmount}" : "")), request.CheckoutType, request.TimeSpan);
            var outPut = InternalRunExeInvoke(tmp);
            if (!outPut.IsSuccess)
            {
                return Result<Checkout>.Fail(outPut.Message);
            }
            var outArr = outPut.Value.Split(new[] { '#', '|' });
            if (outArr.Length <= 1)
            {
                return Result<Checkout>.Fail("His 返回参数不正确");
            }
            if (outPut.Value.StartsWith("-"))
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
                CitizenCardBalance = bSpit[6],
                DoctorTechIds = cSpit,
                PrescriptionId = aSpit[4]
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
            const string inStr = "{0}#{1}#{2}#{3}#{4}|{5}|{6}|{7}|{8}#{9}#{10}";
            var val = (int)request.PayFlag;
            var tmp = string.Format(inStr,
                request.CardNo,
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
            var outPut = InternalRunExeInvoke(tmp);
            if (!outPut.IsSuccess)
            {
                return Result<PerGetTicketCheckout>.Fail(outPut.Message);
            }
            var outArr = outPut.Value.Split(new[] { '#', '|' });
            if (outArr.Length <= 1)
            {
                return Result<PerGetTicketCheckout>.Fail("His 返回参数不正确");
            }
            if (outPut.Value.StartsWith("-"))
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
                CitizenCardBalance = outArr[7],
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
            //就诊卡号#业务类型#用户名#密码#预约ID|预约时间|上下午标志|挂号序号|挂号类别#结算方式#时间戳
            //00#姓名#合计费用|自负金额|优惠金额|医保金额|舍入金额|账户余额|市民卡余额|挂号ID|挂号序号|就诊地点
            if (request == null) throw new ArgumentNullException(nameof(request));
            const string inStr = "{0}#{1}#{2}#{3}#{4}|{5}|{6}|{7}|{8}#{9}#{10}";
            var val = (int)request.PayFlag;
            var tmp = string.Format(inStr,
                request.CardNo,
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
            var outPut = InternalRunExeInvoke(tmp);
            if (!outPut.IsSuccess)
            {
                return Result<GetTicketCheckout>.Fail(outPut.Message);
            }
            var outArr = outPut.Value.Split(new[] { '#', '|' });
            if (outArr.Length <= 1)
            {
                return Result<GetTicketCheckout>.Fail("His 返回参数不正确");
            }
            if (outPut.Value.StartsWith("-"))
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
                CitizenCardBalance = outArr[7],
                RegisterId = outArr[8],
                RegisterOrder = outArr[9],
                VisitingLocation = outArr[10]
            });
        }

        //  private static readonly AutoResetEvent ResetEvent = new AutoResetEvent(false);
        private static readonly object LockObj = new object();
        private static Result<string> InternalRunExeInvoke(string input)
        {

            lock (LockObj)
            {
                try
                {
                    string[] outPut = { "".PadRight(1024) };
                    long timeelsp = 0;
                    var watch = Stopwatch.StartNew();
                    Logger.Net.Info($"HISExePath:{HisExePath},input:{input}");
                    var res = Task.Factory.StartNew(() =>
                    {
                        RunExe(HisExePath, input, ref outPut[0], 60);
                    }).Wait(1000 * 60);//60秒
                    watch.Stop();
                    timeelsp = watch.ElapsedMilliseconds;
                    outPut[0] = outPut[0].Trim().Replace("\u0000", "").Replace("\\u0000", "").Trim();
                    Logger.Net.Info($"recv:{outPut[0]}");
                    if (res && !string.IsNullOrWhiteSpace(outPut[0]))
                    {
                        return Result<string>.Success(outPut[0]);
                    }
                    else
                    {
                        string tmp = null;
                        var sbuilder = new StringBuilder("进程杀死记录");

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
                            Logger.Net.Error($"杀死His进程时发生异常,可能有用信息:{tmp}，原因：" + ex);
                        }
                        finally
                        {
                            Logger.Net.Info(sbuilder.ToString());
                        }
                        Logger.Net.Error($"调用本地HIS服务异常，{res}，{outPut[0]}");
                        return Result<string>.Fail("调用本地HIS服务异常，请稍后再试");
                    }
                }
                catch (Exception ex)
                {
                    Logger.Net.Error($"调用本地HIS服务异常 {ex.Message} {ex.StackTrace}");
                    return Result<string>.Fail($"调用本地HIS服务异常 {ex.Message}");
                }
               
            }

        }
    }
}

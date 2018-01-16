using System;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using YuanTu.Consts;
using YuanTu.Consts.Gateway;
using YuanTu.Core.Extension;
using YuanTu.Core.Log;
using YuanTu.Core.Systems;

namespace YuanTu.Core.Reporter
{
    public static class ReportService
    {
        /// <summary>
        ///     执行系统签到
        /// </summary>
        /// <returns></returns>
        public static bool SignIn()
        {
            var res = ReporterDataHandler.系统签到(new req系统签到
            {
                deviceType = FrameworkConst.DeviceType,
                deviceVersion = FrameworkConst.DeviceType,
                deviceNo = FrameworkConst.OperatorId,
                deviceIp = NetworkManager.IP,
                deviceMac = NetworkManager.MAC,
                method = "heart"
            });
            if (res?.success ?? false)
            {
                //var mint = res.startTime;
                //if (mint > 0)
                //    DateTimeCore.Now =
                //        new DateTime(1970, 1, 1, 8, 0, 0, DateTimeKind.Local).AddMilliseconds(mint);
                var dtime = res.data?.currentDate;
                DateTimeCore.Now = DateTime.ParseExact(dtime, "yyyy-MM-dd HH:mm:ss", null, DateTimeStyles.None);
            }
            return false;
        }

        /// <summary>
        ///     执行系统签到
        /// </summary>
        /// <returns></returns>
        public static Task<bool> SignInAsync()
        {
            new Thread(a =>
                {
                    while (true)
                        try
                        {
                            SignIn();
                        }
                        catch (Exception ex)
                        {
                            Logger.Main.Error($"[监控平台]签到时发生异常，异常内容：{ex.Message}");
                        }
                        finally
                        {
                            var rnd = new Random().NextDouble() * 10;
                            Thread.Sleep(TimeSpan.FromMinutes(25 + rnd));
                        }
                })
            { IsBackground = true, Priority = ThreadPriority.Lowest }.Start();
            return new Task<bool>(() => true);
            // return Task.Factory.StartNew(SignIn);
        }

        public static bool 信息上报(ExceptionModel exception)
        {
            var res = ReporterDataHandler.信息上报(new req信息上报
            {
                errorCode = exception.ErrorCode.GetEnumDescription(),
                errorLevel = (exception.ErrorCode.GetEnumAttribute<ExceptionLevelAttribute>()?.Level ?? 0).ToString(),
                errorMsg = exception.ErrorCode.ToString(),
                errorDetail = exception.ErrorDetail,
                errorSolution = exception.ErrorSolution,
                method = "reportError",
                deviceNo = FrameworkConst.OperatorId,
                deviceIp = NetworkManager.IP,
                deviceMac = NetworkManager.MAC,
            });
            return res.success;
        }

        public static bool 清钱箱上报(decimal totalMoney)
        {
            var ret = ReporterDataHandler.清钱箱上报(new req清钱箱上报() { cash = totalMoney.ToString("0") });
            return ret.success;
        }

        public static Task<bool> ReportExceptionAsync(ExceptionModel exception)
        {
            return FrameworkConst.Local
                ? Task.FromResult(true)
                : Task.Factory.StartNew(p => 信息上报((ExceptionModel)p), exception);
        }

        public static Task<bool> 内存耗尽(string errorDetail, string errorSolution)
        {
            return ReportExceptionAsync(new ExceptionModel(ErrorCode.内存耗尽, errorDetail, errorSolution));
        }

        public static Task<bool> 软件自动更新异常(string errorDetail, string errorSolution)
        {
            return ReportExceptionAsync(new ExceptionModel(ErrorCode.软件自动更新异常, errorDetail, errorSolution));
        }

        public static Task<bool> 软件初始化异常(string errorDetail, string errorSolution)
        {
            return ReportExceptionAsync(new ExceptionModel(ErrorCode.软件初始化异常, errorDetail, errorSolution));
        }
        public static Task<bool> 软件系统异常(string errorDetail, string errorSolution)
        {
            return ReportExceptionAsync(new ExceptionModel(ErrorCode.软件系统异常, errorDetail, errorSolution));
        }

        public static Task<bool> 读卡器离线(string errorDetail, string errorSolution)
        {
            return ReportExceptionAsync(new ExceptionModel(ErrorCode.读卡器离线, errorDetail, errorSolution));
        }

        public static Task<bool> 读卡器被占用(string errorDetail, string errorSolution)
        {
            return ReportExceptionAsync(new ExceptionModel(ErrorCode.读卡器被占用, errorDetail, errorSolution));
        }

        public static Task<bool> 钱箱离线(string errorDetail, string errorSolution)
        {
            return ReportExceptionAsync(new ExceptionModel(ErrorCode.钱箱离线, errorDetail, errorSolution));
        }

        public static Task<bool> 钱箱卡币(string errorDetail, string errorSolution)
        {
            return ReportExceptionAsync(new ExceptionModel(ErrorCode.钱箱卡币, errorDetail, errorSolution));
        }

        public static Task<bool> 钱箱已满(string errorDetail, string errorSolution)
        {
            return ReportExceptionAsync(new ExceptionModel(ErrorCode.钱箱已满, errorDetail, errorSolution));
        }

        public static Task<bool> 凭条打印机离线(string errorDetail, string errorSolution)
        {
            return ReportExceptionAsync(new ExceptionModel(ErrorCode.凭条打印机离线, errorDetail, errorSolution));
        }

        public static Task<bool> 凭条打印机纸将尽(string errorDetail, string errorSolution)
        {
            return ReportExceptionAsync(new ExceptionModel(ErrorCode.凭条打印机纸将尽, errorDetail, errorSolution));
        }
        public static Task<bool> 凭条打印机纸尽(string errorDetail, string errorSolution)
        {
            return ReportExceptionAsync(new ExceptionModel(ErrorCode.凭条打印机纸尽, errorDetail, errorSolution));
        }
        public static Task<bool> 凭条打印机其他异常(string errorDetail, string errorSolution)
        {
            return ReportExceptionAsync(new ExceptionModel(ErrorCode.凭条打印机其他异常, errorDetail, errorSolution));
        }
        public static Task<bool> 凭条打印机出纸口有纸(string errorDetail, string errorSolution)
        {
            return ReportExceptionAsync(new ExceptionModel(ErrorCode.凭条打印机出纸口有纸, errorDetail, errorSolution));
        }
        public static Task<bool> 凭条打印机卡纸(string errorDetail, string errorSolution)
        {
            return ReportExceptionAsync(new ExceptionModel(ErrorCode.凭条打印机卡纸, errorDetail, errorSolution));
        }
        public static Task<bool> 凭条打印机胶辊开启(string errorDetail, string errorSolution)
        {
            return ReportExceptionAsync(new ExceptionModel(ErrorCode.凭条打印机胶辊开启, errorDetail, errorSolution));
        }
        public static Task<bool> 发卡器离线(string errorDetail, string errorSolution)
        {
            return ReportExceptionAsync(new ExceptionModel(ErrorCode.发卡器离线, errorDetail, errorSolution));
        }

        public static Task<bool> 发卡器被占用(string errorDetail, string errorSolution)
        {
            return ReportExceptionAsync(new ExceptionModel(ErrorCode.发卡器被占用, errorDetail, errorSolution));
        }

        public static Task<bool> 卡已耗尽(string errorDetail, string errorSolution)
        {
            return ReportExceptionAsync(new ExceptionModel(ErrorCode.卡已耗尽, errorDetail, errorSolution));
        }

        public static Task<bool> 卡剩余5张(string errorDetail, string errorSolution)
        {
            return ReportExceptionAsync(new ExceptionModel(ErrorCode.卡剩余5张, errorDetail, errorSolution));
        }

        public static Task<bool> 卡剩余10张(string errorDetail, string errorSolution)
        {
            return ReportExceptionAsync(new ExceptionModel(ErrorCode.卡剩余10张, errorDetail, errorSolution));
        }

        public static Task<bool> 卡剩余20张(string errorDetail, string errorSolution)
        {
            return ReportExceptionAsync(new ExceptionModel(ErrorCode.卡剩余20张, errorDetail, errorSolution));
        }

        public static Task<bool> 身份证读卡器离线(string errorDetail, string errorSolution)
        {
            return ReportExceptionAsync(new ExceptionModel(ErrorCode.身份证读卡器离线, errorDetail, errorSolution));
        }

        public static Task<bool> 金属键盘离线(string errorDetail, string errorSolution)
        {
            return ReportExceptionAsync(new ExceptionModel(ErrorCode.金属键盘离线, errorDetail, errorSolution));
        }

        public static Task<bool> 金属键盘被占用(string errorDetail, string errorSolution)
        {
            return ReportExceptionAsync(new ExceptionModel(ErrorCode.金属键盘被占用, errorDetail, errorSolution));
        }

        public static Task<bool> 社保读卡器离线(string errorDetail, string errorSolution)
        {
            return ReportExceptionAsync(new ExceptionModel(ErrorCode.社保读卡器离线, errorDetail, errorSolution));
        }

        public static Task<bool> 社保读卡器被占用(string errorDetail, string errorSolution)
        {
            return ReportExceptionAsync(new ExceptionModel(ErrorCode.社保读卡器被占用, errorDetail, errorSolution));
        }

        public static Task<bool> 未发现摄像头(string errorDetail, string errorSolution)
        {
            return ReportExceptionAsync(new ExceptionModel(ErrorCode.未发现摄像头, errorDetail, errorSolution));
        }

        public static Task<bool> 网关请求超时(string errorDetail, string errorSolution)
        {
            return ReportExceptionAsync(new ExceptionModel(ErrorCode.网关请求超时, errorDetail, errorSolution));
        }

        public static Task<bool> HIS请求超时(string errorDetail, string errorSolution)
        {
            return ReportExceptionAsync(new ExceptionModel(ErrorCode.HIS请求超时, errorDetail, errorSolution));
        }

        public static Task<bool> 第三方请求超时(string errorDetail, string errorSolution)
        {
            return ReportExceptionAsync(new ExceptionModel(ErrorCode.第三方请求超时, errorDetail, errorSolution));
        }

        public static Task<bool> 银联请求超时(string errorDetail, string errorSolution)
        {
            return ReportExceptionAsync(new ExceptionModel(ErrorCode.银联请求超时, errorDetail, errorSolution));
        }

        public static Task<bool> POS签到失败(string errorDetail, string errorSolution)
        {
            return ReportExceptionAsync(new ExceptionModel(ErrorCode.POS签到失败, errorDetail, errorSolution));
        }

        public static Task<bool> POS冲正失败(string errorDetail, string errorSolution)
        {
            return ReportExceptionAsync(new ExceptionModel(ErrorCode.POS冲正失败, errorDetail, errorSolution));
        }

        public static Task<bool> 网关返回异常(string errorDetail, string errorSolution)
        {
            return ReportExceptionAsync(new ExceptionModel(ErrorCode.网关返回异常, errorDetail, errorSolution));
        }

        public static Task<bool> HIS请求失败(string errorDetail, string errorSolution)
        {
            return ReportExceptionAsync(new ExceptionModel(ErrorCode.HIS请求失败, errorDetail, errorSolution));
        }

        public static Task<bool> 健康小屋闸门离线(string errorDetail, string errorSolution)
        {
            return ReportExceptionAsync(new ExceptionModel(ErrorCode.健康小屋闸门离线, errorDetail, errorSolution));
        }
        public static Task<bool> 健康小屋身高体重仪离线(string errorDetail, string errorSolution)
        {
            return ReportExceptionAsync(new ExceptionModel(ErrorCode.健康小屋身高体重仪离线, errorDetail, errorSolution));
        }
        public static Task<bool> 健康小屋体脂仪离线(string errorDetail, string errorSolution)
        {
            return ReportExceptionAsync(new ExceptionModel(ErrorCode.健康小屋体脂仪离线, errorDetail, errorSolution));
        }
        public static Task<bool> 健康小屋血压仪离线(string errorDetail, string errorSolution)
        {
            return ReportExceptionAsync(new ExceptionModel(ErrorCode.健康小屋血压仪离线, errorDetail, errorSolution));
        }
        public static Task<bool> 健康小屋血氧仪离线(string errorDetail, string errorSolution)
        {
            return ReportExceptionAsync(new ExceptionModel(ErrorCode.健康小屋血氧仪离线, errorDetail, errorSolution));
        }
        public static Task<bool> 健康小屋体温仪离线(string errorDetail, string errorSolution)
        {
            return ReportExceptionAsync(new ExceptionModel(ErrorCode.健康小屋体温仪离线, errorDetail, errorSolution));
        }
        public static Task<bool> 健康小屋心电仪离线(string errorDetail, string errorSolution)
        {
            return ReportExceptionAsync(new ExceptionModel(ErrorCode.健康小屋心电仪离线, errorDetail, errorSolution));
        }
    }
}
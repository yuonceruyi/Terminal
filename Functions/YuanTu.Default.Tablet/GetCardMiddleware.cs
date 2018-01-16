using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Practices.ServiceLocation;
using YuanTu.Consts;
using YuanTu.Consts.FrameworkBase;
using YuanTu.Core.Extension;
using YuanTu.Core.Reporter;
using YuanTu.Devices.CardReader;

namespace YuanTu.Default.Tablet
{
    public class GetCardMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IRFCardReader _rfCardReader;

        public GetCardMiddleware(RequestDelegate next)
        {
            _next = next;

            _rfCardReader = ServiceLocator.Current.GetAllInstances(typeof(IRFCardReader))
                .FirstOrDefault(p => (p as IRFCardReader).DeviceId == "HuaDa_RF") as IRFCardReader;
        }

        protected Result<string> ReadRF()
        {
            if (FrameworkConst.VirtualHardWare)
                return Result<string>.Success("123456");

            try
            {
                var ret = _rfCardReader.Connect();
                if (!ret.IsSuccess)
                {
                    ReportService.读卡器离线(null, ErrorSolution.读卡器离线);
                    return Result<string>.Fail("读卡器离线");
                }
                if (!_rfCardReader.Initialize().IsSuccess)
                    return Result<string>.Fail($"初始化失败({ret.ResultCode})");
                {
                    var rest = _rfCardReader.GetCardId();
                    if (rest.IsSuccess)
                    {
                        var track = BitConverter.ToUInt32(rest.Value, 0).ToString();
                        return Result<string>.Success(track);
                    }
                    return Result<string>.Fail("未读到卡");
                }
            }
            finally
            {
                _rfCardReader.UnInitialize();
            }
        }

        public async Task Invoke(HttpContext context)
        {
            // localhost:9090/GetCard
            if (context.Request.Path == "/GetCard")
            {
                context.Response.ContentType = "application/json;charset=utf-8";

                var result = ReadRF();

                if (result.IsSuccess)
                    await context.Response.WriteAsync((new
                    {
                        code = 10000,
                        message = "成功",
                        sub_code = "BUSINESS_SUCCESS",
                        sub_message = "读卡成功",
                        card_type = 2,
                        card_no = result.Value,
                    }).ToJsonString());
                else
                    await context.Response.WriteAsync((new
                    {
                        code = 10000,
                        message = "成功",
                        sub_code = "BUSINESS_FAILED",
                        sub_message = result.Message,
                    }).ToJsonString());
                return;
            }

            await _next(context);
        }
    }
}
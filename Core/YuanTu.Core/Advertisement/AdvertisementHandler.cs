using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using YuanTu.Consts.FrameworkBase;
using YuanTu.Core.Advertisement.Base;
using YuanTu.Core.Advertisement.Data;
using YuanTu.Core.Extension;
using YuanTu.Core.Log;

namespace YuanTu.Core.Advertisement
{
    public static class AdvertisementHandler
    {
        public static GetAdvertisementRes 上传使用信息(UploadUsageInfoReq req)
        {
            try
            {
                var res = Handle(req);
                if (res.IsSuccess)
                {
                    return res.Value.ToJsonObject<GetAdvertisementRes>();
                }
                return null;
            }
            catch (Exception)
            {
                return null;
            }

        }
        public static GetAdvertisementRes 获取广告(GetAdvertisementReq req)
        {
            try
            {
                var res = Handle(req);
                if (res.IsSuccess)
                {
                    return res.Value.ToJsonObject<GetAdvertisementRes>();
                }
                return null;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static FindDoubleScreenAdRes 获取双屏广告(FindDoubleScreenAdReq req)
        {
            try
            {
                var res = Handle(req,20);
                if (res.IsSuccess)
                {
                    return res.Value.ToJsonObject<FindDoubleScreenAdRes>();
                }
                return null;
            }
            catch (Exception ex)
            {
                Logger.Maintenance.Error($"{Prefix}获取异常 "+ex);
                return null;
            }
        }


        private const string Prefix = "广告平台";
        public static string AdServerUrl { get; set; }
        private static readonly HttpClient Client = new HttpClient();

        public static Result<string> Handle(ReqBase obj,int timeOutSencods=2)
        {
            try
            {
                Client.Timeout = TimeSpan.FromSeconds(timeOutSencods);
                var watch = Stopwatch.StartNew();
                var text = obj.ToJsonString();
                var param = obj.BuldDict();
                var formContent = new FormUrlEncodedContent(param);
                Logger.Maintenance.Info($"[{Prefix}] 发送:{text}");
                var res = Client.PostAsync(AdServerUrl + obj.UrlPath, formContent);
                var content = res.Result.Content.ReadAsStringAsync().Result;
                watch.Stop();
                var time = watch.ElapsedMilliseconds;
                Logger.Maintenance.Info($"[{Prefix}]  耗时:{time}毫秒 接收内容:{content}");
                return Result<string>.Success(content);
            }
            catch (Exception ex)
            {
                return Result<string>.Fail(ex.Message);
            }
        }

        //private static bool 
    }
}

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Practices.ServiceLocation;
using YuanTu.Consts;
using YuanTu.Consts.FrameworkBase;
using YuanTu.Core.Extension;
using YuanTu.Core.Log;
using YuanTu.Core.Systems;
using YuanTu.YiWuArea.Commons;
using YuanTu.YiWuArea.Insurance.Dtos;
using YuanTu.YiWuArea.Insurance.Models.Base;
using YuanTu.YiWuArea.Insurance.Services;

namespace YuanTu.YiWuArea.Insurance.Models
{
    public static partial class SipayHandler
    {
        public static string TradeApi => YiWuAreaConsts.SiPlatformUrl + "/api/InsuranceOperate";

        /// <summary>
        /// 社保操作员编号
        /// </summary>
        public static string SiOperatorNo { get; set; } = YiWuAreaConsts.SiOperatorNo;
        /// <summary>
        /// 医保区域编号
        /// </summary>
        public static string SiHospitalCode => YiWuAreaConsts.SiHospitalCode;

        /// <summary>
        /// 社保业务周期号/对账流水号
        /// </summary>
        public static string SiToken { get; set; }

        public static TRes Handler<TReq, TRes>(TReq req,int tradeCode) where TReq : InsuranceRequestBase where TRes : InsuranceResponseBase, new()
        {
            var param = new InsuranceAllInfoRequest
            {
                TradeName=req.TradeName,
                TradeCode=tradeCode.ToString(),
                TradeInput="",
                TradeRet=0,
                TradeResult="",
                IpAddress=NetworkManager.IP

            };
            try
            {
                if (!hasInit)
                {
                    var initRet = Init();
                    if (!initRet.IsSuccess)
                    {
                        var res = new TRes
                        {
                            交易状态 = initRet.ResultCode.ToString(),
                            错误信息 = initRet.Message
                        };
                        return res;
                    }
                  
                }
                if (tradeCode!=94)//不是签到的情况下，优先保证签到成功
                {
                    var service = ServiceLocator.Current.GetInstance<ISipayService>();
                    var signInRest = service.SureSignIn();
                    if (!signInRest.IsSuccess)
                    {
                        return  new TRes
                        {
                            交易状态 = "-200",
                            错误信息 = "社保服务签到失败！"+ signInRest.Message
                        };
                    }
                }
                var input = req.BuildRequest();
                param.TradeInput = input;
                Logger.Net.Info($"[社保][{req.TradeName}][{tradeCode}] 入参:{input}");
                var watch = Stopwatch.StartNew();
                var sbuilder = MakeStringBuilder(20480);
                var ret = YiWuNativeProvider.f_UserBargaingApply(tradeCode, 0.0, input, sbuilder, SiHospitalCode);
                var resultRet = sbuilder.ToString().Trim();
                watch.Stop();
                param.TradeRet = ret;
                param.TradeResult = resultRet;
                
                Logger.Net.Info($"[社保][{req.TradeName}][{tradeCode}] 返回值:{ret} 出参:{resultRet} 耗时:{watch.ElapsedMilliseconds}ms");
                var model= InsuranceResponseBase.BuildResponse<TRes>(resultRet);
                model.报文入参 = input;
                model.报文出参 = resultRet;
                return model;
            }
            catch (Exception ex)
            {
                Logger.Net.Error($"[社保][{req.TradeName}][{tradeCode}] 发生异常:{ex.Message} {ex.StackTrace}");
                var res = new TRes
                {
                    交易状态 = "-200",
                    错误信息 = "社保服务异常，请联系医院工作人员处理！"
                };
                param.TradeRet = -9810;
                param.TradeResult = ex.Message;
                return res;
            }
            finally
            {
              //  Uninit();
                Tools.YiWuMiddlwHttpMethod<object>(TradeApi, HttpMethodEnum.Post, param.ToJsonString());
            }
        }



        #region[辅助功能]

        private static bool hasInit = false;
        private static Result Init()
        {
            var watch = Stopwatch.StartNew();
            var sb = MakeStringBuilder(1024);
            var ret = YiWuNativeProvider.f_UserBargaingInit("$$$$", sb, SiHospitalCode);
            watch.Stop();
            Logger.Net.Info($"[社保][初始化] 返回值:{ret} 返回内容:{sb} 医保医院编号:{SiHospitalCode} 耗时:{watch.ElapsedMilliseconds}ms");
            if (ret != 0)
            {
                return Result.Fail(ret,sb.ToString());
            }
            hasInit = true;
            return Result.Success();
        }

        public static Result Uninit()
        {
            var watch = Stopwatch.StartNew();
            var sb = MakeStringBuilder(1024);
            var ret = YiWuNativeProvider.f_UserBargaingClose("$$$$", sb, SiHospitalCode);
            watch.Stop();
            Logger.Net.Info($"[社保][反初始化] 返回值:{ret} 返回内容:{sb} 医保医院编号:{SiHospitalCode} 耗时:{watch.ElapsedMilliseconds}ms");
            hasInit = false;
            return Result.Success();
        }

        private static StringBuilder MakeStringBuilder(int capacity=2048)
        {
            var sbuilder=new StringBuilder(capacity);
            sbuilder.Append(' ', capacity);
            return sbuilder;
        }
        #endregion
    }
}

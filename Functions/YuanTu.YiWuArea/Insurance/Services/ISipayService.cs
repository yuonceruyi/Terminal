using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using YuanTu.Consts;
using YuanTu.Consts.FrameworkBase;
using YuanTu.Core.DB;
using YuanTu.Core.Extension;
using YuanTu.Core.Log;
using YuanTu.Core.Reporter;
using YuanTu.Core.Systems;
using YuanTu.YiWuArea.Commons;
using YuanTu.YiWuArea.Insurance.Dtos;
using YuanTu.YiWuArea.Insurance.Models;
using YuanTu.YiWuArea.Models;

namespace YuanTu.YiWuArea.Insurance.Services
{
    public interface ISipayService:IService
    {
        /// <summary>
        /// 确保当前业务周期号是最新的
        /// </summary>
        /// <returns></returns>
        Result SureSignIn();

        Result<Guid> UploadTradeInfo(InsuranceTradeRequest request);
        Result ConfirmTradeInfo(string tradeCode, string ybTradeNo, bool issuccess,string reason);
        Result TradeRefund(Guid tradeId, string reason);
    }

    public class SipayService : ISipayService
    {
        public string ServiceName => "义乌平台社保服务";
        protected DateTime FirsTime { get; private set; }

        public string SignApi => YiWuAreaConsts.SiPlatformUrl+ "/api/InsuranceSignIn";
        public string TradeApi => YiWuAreaConsts.SiPlatformUrl+ "/api/InsuranceTradeUpload";

        public virtual Result SureSignIn()
        {
            if ((DateTimeCore.Now - FirsTime).TotalSeconds > 30|| YiWuAreaConsts.SiOperatorNo.IsNullOrWhiteSpace()) //30秒内有效
            {
               var rest = Tools.YiWuMiddlwHttpMethod<InsuranceSignInResponse>($"{SignApi}", HttpMethodEnum.Get, null);
                SipayHandler.SiOperatorNo = YiWuAreaConsts.SiOperatorNo = rest.Value?.SiOperator;
                SipayHandler.SiToken = YiWuAreaConsts.SiToken= rest.Value?.SiToken;
                if (rest.IsSuccess && rest.ResultCode == 0)
                {
                    FirsTime = DateTimeCore.Now;
                    return Result.Success();
                }
                else if(rest.ResultCode == -2)//平台没有操作员编号
                {
                    return rest.Convert();
                }
                else if (rest.ResultCode == -200) //网络问题
                {
                    rest.Convert();
                }
                else
                {
                    return SyncSignInFromSi();
                }
            }
            SipayHandler.SiOperatorNo = YiWuAreaConsts.SiOperatorNo;
            SipayHandler.SiToken = YiWuAreaConsts.SiToken;
            return Result.Success();
        }

        public virtual Result<Guid> UploadTradeInfo(InsuranceTradeRequest request)
        {

            var rest = Tools.YiWuMiddlwHttpMethod<Guid>(TradeApi, HttpMethodEnum.Post, request.ToJsonString());
            return rest;
        }

        public virtual Result ConfirmTradeInfo(string tradeCode,string ybTradeNo, bool issuccess,string reason)
        {
            var retsult = issuccess ? 1 : -1;
            var req = new Req交易确认()
            {
                交易类型 = tradeCode,
                医保交易流水号 = ybTradeNo,
                是否需要诊间结算 = "0",
                HIS事务结果 = retsult.ToString()
            };
            var res = SipayHandler.调用交易确认(req);
            var obj = new InsuranceTradeConfirmRequest()
            {
                OrderNo = ybTradeNo,
                ConfirmResult = res.IsSuccess? retsult:0,
                ConfirmRequest = res.报文入参,
                ConfirmResponse = res.报文出参,
                Reason = reason
            };
            var middleRet = Tools.YiWuMiddlwHttpMethod<object>(TradeApi, HttpMethodEnum.Put, obj.ToJsonString());
           
            if (res.IsSuccess)
            {
                if (!middleRet.IsSuccess)
                {
                    DBManager.Insert(new YiWuAreaInsuranceConfirmFailedModel
                    {
                        OrderNo = ybTradeNo,
                        ConfirmResult = retsult,//原始确认成功还是失败
                        ConfirmRequest = res.报文入参,
                        ConfirmResponse = res.报文出参,
                        Reason = reason,
                        MiddlareFailedMessage = middleRet.Message
                    });
                    return new Result(true, -2, "交易已经确认，但是上传中间件平台失败:"+ middleRet.Message);
                }
                return Result.Success();
            }
            ReportService.ReportExceptionAsync(new ExceptionModel(ErrorCode.社保返回异常, "发送交易确认指令，社保返回失败："+res.错误信息, "联系病人到窗口处理，有单边账风险！"));
            return Result.Fail(res.错误信息);
        }

        public Result TradeRefund(Guid tradeId, string reason)
        {
            var obj = new
            {
                TradeId=tradeId,
                Reason=reason
            };
            var middleRet = Tools.YiWuMiddlwHttpMethod<object>(TradeApi, HttpMethodEnum.Delete, obj.ToJsonString());
            return middleRet.Convert();
        }


        protected virtual Result SyncSignInFromSi()
        {
            var res = SipayHandler.调用签到(new Req签到()
            {
                操作员账号 = SipayHandler.SiOperatorNo
            });
            if (res.交易状态 == "0")//签到成功
            {
                Tools.YiWuMiddlwHttpMethod<object>(SignApi, HttpMethodEnum.Post, new
                {
                    SiOperatorNo = SipayHandler.SiOperatorNo,
                    SiToken = res.日对账流水号,
                    CreateIpAddress = NetworkManager.IP
                }.ToJsonString());
                SipayHandler.SiToken = res.日对账流水号;
                FirsTime = DateTimeCore.Now;
                return Result.Success();
            }
            else
            {
                return Result.Fail(res.错误信息);
            }
        }

       
    }

   
}

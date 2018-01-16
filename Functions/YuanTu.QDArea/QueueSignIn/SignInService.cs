using Microsoft.Practices.ServiceLocation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YuanTu.Consts.Enums;
using YuanTu.Consts.FrameworkBase;
using YuanTu.Consts.Services;
using YuanTu.Core.Log;

namespace YuanTu.QDArea.QueueSignIn
{
    public class SignInService
    {
        private static string deviceSecret;
        private static string token;
        private static DataHandlerQD dataHandler;
        private static bool init;
        public static string Msg;

        public  static Result Init()
        {
            if(init)
                return Result.Success();

            var config = ServiceLocator.Current.GetInstance<IConfigurationManager>();
            dataHandler = new DataHandlerQD { Uri = new Uri(config.GetValue("SignInUrl")) };

            var result = GetSecret(NetHelper.MAC);            
            if (!result.IsSuccess)
                return Result.Fail("获取设备注册信息失败\n" + result.Message);
            deviceSecret = result.Value;
            result = GetAccessToken(deviceSecret);            
            if (!result.IsSuccess)
                return Result.Fail("获取Token失败\n" + result.Message);
            token = result.Value;
            init = true;
            return Result.Success();
        }

        public static List<ResQueryQueueByDevice.Data> QueryQueue(string cardNo,CardType cardType)
        {
            var req = new ReqQueryQueueByDevice
            {
                token = token,
                cardType = cardType.GetHashCode().ToString(),
                cardNo = cardNo
            };
            var result = dataHandler.Query(req);
            if (!result.IsSuccess)
            {
                Msg = result.Message;
                return null;
            }
            var res = result.Value;
            if (!res.success)
            {
                if (res.resultCode.ParseQueueResultCode().IsInvalidTokenError())
                {
                    var resToken = GetAccessToken(deviceSecret);
                    if (!resToken.IsSuccess)
                    {
                        Msg = res.msg;
                        return null;
                    }
                    token = resToken.Value;
                    return QueryQueue(cardNo, cardType);
                }
                else
                {
                    Msg = res.msg;
                    return null;
                }
            }
            return res.data;
        }

         public static ResConfirmTakeNo.Data TakeNo(string cardNo,CardType cardType,string queueCode)
        {
            var req = new ReqConfirmTakeNo
            {
                token = token,
                cardType = cardType.GetHashCode().ToString(),
                cardNo = cardNo,
                queueCode = queueCode
            };
            var result = dataHandler.Query(req);
            
            if (!result.IsSuccess)
            {
                Msg = result.Message;
                return null;
            }
            var res = result.Value;
            if (!res.success)
            {
                if (res.resultCode.ParseQueueResultCode().IsInvalidTokenError())
                {
                    var resToken = GetAccessToken(deviceSecret);
                    if (!resToken.IsSuccess)
                    {
                        Msg = res.msg;
                        return null;
                    }
                    token = resToken.Value;
                    return TakeNo(cardNo, cardType,queueCode);
                }
                else
                {
                    Msg = res.msg;
                    return null;
                }
            }
            return res.data;
        }
        private static Result<string> GetSecret(string deviceMac)
        {
            var req = new ReqGetSecret
            {
                deviceMac = deviceMac
            };
            var result = dataHandler.Query(req);
            if (!result.IsSuccess)
                return Result<string>.Fail(result.Message);

            var res = result.Value;
            if (!res.success)
                return Result<string>.Fail(res.msg);
            var deviceInfo = res.data;
            return Result<string>.Success(deviceInfo);
        }

        private static Result<string> GetAccessToken(string deviceSecret)
        {
            var req = new ReqGetAccessToken
            {
                deviceSecret = deviceSecret
            };
            var result = dataHandler.Query(req);
            if (!result.IsSuccess)
                return Result<string>.Fail(result.Message);

            var res = result.Value;
            if (!res.success)
                return Result<string>.Fail(res.msg);
            var token = res.data;
            return Result<string>.Success(token);
        }
    }
    internal static class DataHandlerExtention
    {
        public static Result<ResGetSecret> Query(this DataHandlerQD dataHandler, ReqGetSecret req)
        {
            return dataHandler.Query<ResGetSecret, ReqGetSecret>(req);
        }

        public static Result<ResGetAccessToken> Query(this DataHandlerQD dataHandler, ReqGetAccessToken req)
        {
            return dataHandler.Query<ResGetAccessToken, ReqGetAccessToken>(req);
        }
        public static Result<ResQueryQueueByDevice> Query(this DataHandlerQD dataHandler, ReqQueryQueueByDevice req)
        {
            return dataHandler.Query<ResQueryQueueByDevice, ReqQueryQueueByDevice>(req);
        }
        public static Result<ResConfirmTakeNo> Query(this DataHandlerQD dataHandler, ReqConfirmTakeNo req)
        {
            return dataHandler.Query<ResConfirmTakeNo, ReqConfirmTakeNo>(req);
        }
    }
    public static class QueueResultCodeEnumExtention
    {
        public static QueueResultCodeEnums ParseQueueResultCode(this string code)
        {
            int n;
            int.TryParse(code, out n);
            return (QueueResultCodeEnums)n;
        }

        public static bool IsInvalidTokenError(this QueueResultCodeEnums code)
        {
            return (code == QueueResultCodeEnums.SESSION_EXPIRE) || (code == QueueResultCodeEnums.TOKEN_ERROR) ||
                   (code == QueueResultCodeEnums.TOKEN_EXPIRE);
        }
    }
    public enum QueueResultCodeEnums
    {
        UNKNOWN = 0,
        OK = 100,
        SERVICE_ERROR = 201,
        ACCOUNT_FAILURE = 401,
        TOKEN_EXPIRE = 403,
        DEVICE_OFFLINE = 405,
        DEFAULT_ERROR = 1000,
        ERROR = 1001,
        PARAM_ERROR = 1002,
        SERVER_INNER_ERROR = 1003,
        SESSION_EXPIRE = 1004,
        TOKEN_ERROR = 2000
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Practices.Unity;
using YuanTu.Consts.Enums;
using YuanTu.Consts.FrameworkBase;
using YuanTu.Consts.Services;
using YuanTu.Core.Extension;
using YuanTu.Core.Log;

namespace YuanTu.ZheJiangZhongLiuHospital.Component.QueueSignIn
{
    public interface IQueueSignInService : IService
    {
        Result<QueueResBase<RegisterInfo[]>> GetRegisterList(string cardno, CardType cardType);
        Result QueueSignIn(SignType sType, RegisterInfo info);
    }

    public class QueueSignInService : IQueueSignInService
    {
        [Dependency]
        public IConfigurationManager Config { get; set; }

        public string ServiceName => "签到服务";

        public Result<QueueResBase<RegisterInfo[]>> GetRegisterList(string cardno, CardType cardtype)
        {
            var token =GetToken();
            if (!token.IsSuccess)
            {
                return Result<QueueResBase<RegisterInfo[]>>.Fail(token.Message);
            }
            var url = $"{token.Value.Uri.TrimEnd('/')}/confirmDetail.do";
            var ctp = 0;
            switch (cardtype)
            {
                case CardType.NoCard:
                    break;
                case CardType.身份证:
                    ctp = 1;
                    break;
                case CardType.就诊卡:
                    ctp = 10;
                    break;
                case CardType.社保卡:
                    ctp = 11;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(cardtype), cardtype, null);
            }
            var ret = HttpTool.Post<QueueResBase<RegisterInfo[]>>(url, new Dictionary<string, string>()
            {
                ["cardNo"] = cardno,
                ["cardType"] = ctp.ToString(),
                ["token"] = token.Value.Token,
                ["corpId"] = token.Value.CorpId
            });
            if (ret.success && ret.data != null && ret.data.Any())
            {
                return Result<QueueResBase<RegisterInfo[]>>.Success(ret);
            }
            return Result<QueueResBase<RegisterInfo[]>>.Fail(ret.msg?.Replace("$", "\r\n"));
        }

        public Result QueueSignIn(SignType sType, RegisterInfo info)
        {
            var token = GetToken();
            if (!token.IsSuccess)
            {
                return Result.Fail(token.Message);
            }
            var url = $"{token.Value.Uri.TrimEnd('/')}/confirm.do";
            var ret = HttpTool.Post<QueueResBase<string>>(url, new Dictionary<string, string>()
            {
                ["patientId"] = info.id.ToString(),
                ["token"] = token.Value.Token,
                ["corpId"] = token.Value.CorpId,
                ["confirmType"] = ((int)sType).ToString(),

            });
            if (ret.success && ret.data == "true")
            {
                return Result.Success();
            }
            return Result.Fail(ret.msg?.Replace("$", "\r\n"));
        }

        public Result<QueueToken> GetToken()
        {
            try
            {
                
                var filePath = Config.GetValue("SignInTokenPath");
                if (!File.Exists(filePath))
                {
                    Logger.Main.Error($"[排队叫号]配置文件不存在,路径:{filePath}");
                    return Result<QueueToken>.Fail("排队系统初始化中，请稍后再试！");
                }
                var content = File.ReadAllText(filePath);
                var obj = content.ToJsonObject<QueueToken>();
                return Result<QueueToken>.Success(obj);
            }
            catch (Exception ex)
            {
                Logger.Main.Error($"[排队叫号]操作时发生异常 {ex.Message}");
                return Result<QueueToken>.Fail("排队系统初始化中，请稍后再试！");
            }
        }
    }
}

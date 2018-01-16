using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using YuanTu.Consts.FrameworkBase;
using YuanTu.Core.Log;

namespace YuanTu.ZheJiangZhongLiuHospital.NativeService
{
    public class HisReadCardService
    {
        [DllImport("readcard.dll")]
        private static extern IntPtr IReadCard();

        public static Result<string> ReadCard()
        {
            try
            {
                var cardNo = string.Empty;
                var res = Task.Factory.StartNew(() =>
                {
                    try
                    {
                        cardNo =Marshal.PtrToStringAnsi(IReadCard());
                    }
                    catch (Exception e)
                    {
                        Logger.Net.Info($"建档HIS读取卡号异常{e}");
                    }
                }).Wait(1000 * 30);
                if (res && !string.IsNullOrWhiteSpace(cardNo))
                {
                    return Result<string>.Success(cardNo);
                }
                Logger.Net.Error($"建档HIS读取卡号异常，{res}，卡号：{cardNo}");
                return Result<string>.Fail("建档HIS读取卡号异常，请稍后再试");
            }
            catch (Exception e)
            {
                Logger.Net.Error($"建档HIS读取卡号异常:{e.Message}");
                return Result<string>.Fail($"建档HIS读取卡号异常:{e.Message}，请稍后再试");
            }
           
        }
    }
}

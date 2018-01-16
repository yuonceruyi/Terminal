using YuanTu.Consts.FrameworkBase;
using YuanTu.Core.Log;
using YuanTu.ISO8583.IO;

namespace YuanTu.ISO8583.深圳
{
    internal class Manager : ISO8583.Manager
    {
        public override Result<Output> DoSale(Input input)
        {
            if (!icMode)
                return POS.DoSale(input);
            firstHalfList = IcPos.FirstHalf();
            if (firstHalfList == null)
                return Result<Output>.Fail("IC卡处理失败");

            var doRes = POS.DoSaleIC(input, firstHalfList);
            if (!doRes.IsSuccess)
            {
                IcPos.OnlineFail();
                return Result<Output>.Fail(doRes.Message);
            }
            var output = doRes.Value;
            if (!IcPos.SecondHalf(output))
            {
                var reRes = POS.DoReverseIC(input, firstHalfList);
                if (reRes.IsSuccess)
                    return Result<Output>.Fail("IC卡确认失败 冲正成功");
                Logger.Main.Info("冲正失败，错误原因为：" + reRes.Message);
                return Result<Output>.Fail("IC卡确认失败 冲正失败");
            }
            return Result<Output>.Success(output);
        }
    }
}
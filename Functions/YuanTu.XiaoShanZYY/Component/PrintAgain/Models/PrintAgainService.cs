using Microsoft.Practices.ServiceLocation;
using YuanTu.Consts;
using YuanTu.Consts.FrameworkBase;
using YuanTu.XiaoShanZYY.Component.Auth.Models;
using YuanTu.XiaoShanZYY.HIS;

namespace YuanTu.XiaoShanZYY.Component.PrintAgain.Models
{
    public interface IPrintAgainService : IService
    {
        Result 补打查询();
    }

    internal class PrintAgainService : IPrintAgainService
    {
        public string ServiceName { get; }

        private IAuthModel GetAuthModel()
        {
            return ServiceLocator.Current.GetInstance<IAuthModel>();
        }

        private IPrintAgainModel GetPrintAgainModel()
        {
            return ServiceLocator.Current.GetInstance<IPrintAgainModel>();
        }

        public Result 补打查询()
        {
            var Auth = GetAuthModel();
            var PrintAgain = GetPrintAgainModel();
            var today = DateTimeCore.Today;
            var req = new Req补打查询()
            {
                cardNo = Auth.人员信息.就诊卡号,
                cardType = Auth.人员信息.病人类别,

                startDate = today.AddDays(-1).ToString("yyyy-MM-dd"),
                endDate = today.ToString("yyyy-MM-dd"),

                extend = " "
            };
            var result = DataHandler.补打查询(req);
            if (!result.IsSuccess)
                return result.Convert();
            var res = result.Value;
            if (res.BILLMX == null || res.BILLMX.Count == 0)
                return Result.Fail("取到的列表为空");

            PrintAgain.Res补打查询 = res;
            return Result.Success();
        }
    }
}
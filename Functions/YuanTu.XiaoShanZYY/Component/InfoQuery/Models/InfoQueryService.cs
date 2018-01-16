using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Practices.ServiceLocation;
using YuanTu.Consts.FrameworkBase;
using YuanTu.XiaoShanZYY.Component.Auth.Models;
using YuanTu.XiaoShanZYY.HIS;

namespace YuanTu.XiaoShanZYY.Component.InfoQuery.Models
{
    public interface IInfoQueryService : IService
    {
        Result 缴费结算查询();
    }
    class InfoQueryService: IInfoQueryService
    {
        public string ServiceName { get; }

        private IAuthModel GetAuthModel()
        {
            return ServiceLocator.Current.GetInstance<IAuthModel>();
        }
        private IInfoQueryModel GetInfoQueryModel()
        {
            return ServiceLocator.Current.GetInstance<IInfoQueryModel>();
        }
        public Result 缴费结算查询()
        {
            var Auth = GetAuthModel();
            var InfoQuery = GetInfoQueryModel();
            var req = new Req缴费结算查询()
            {
                cardNo = Auth.人员信息.就诊卡号,
                cardType = Auth.人员信息.病人类别,

                startDate = InfoQuery.DateTimeStart.ToString("yyyy-MM-dd"),
                endDate = InfoQuery.DateTimeEnd.ToString("yyyy-MM-dd"),

                extend = " "
            };
            var result = DataHandler.缴费结算查询(req);
            if (!result.IsSuccess)
                return result.Convert();
            var res = result.Value;
            if (res.BILLMX == null || res.BILLMX.Count == 0)
                return Result.Fail("取到的费用列表为空");

            InfoQuery.Res缴费结算查询 = res;
            return Result.Success();
        }
    }
}

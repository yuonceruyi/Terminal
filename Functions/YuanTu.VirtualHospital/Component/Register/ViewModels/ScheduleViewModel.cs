using System;
using System.Linq;
using YuanTu.Consts.Enums;
using YuanTu.Consts.Gateway;
using YuanTu.Core.Extension;
using YuanTu.Default.Component.Tools.Models;

namespace YuanTu.VirtualHospital.Component.Register.ViewModels
{
    class ScheduleViewModel:Default.Component.Register.ViewModels.ScheduleViewModel
    {
        protected override void FillRechargeRequest(req预约挂号 req)
        {
            base.FillRechargeRequest(req);
            var extraPaymentModel = GetInstance<IExtraPaymentModel>();
            if (extraPaymentModel.CurrentPayMethod == PayMethod.先诊疗后付费)
                req.transNo = new string(req.flowId.Where(char.IsDigit).ToArray());
            //req.extend = new {version = "0"}.ToJsonString();
        }
    }
}

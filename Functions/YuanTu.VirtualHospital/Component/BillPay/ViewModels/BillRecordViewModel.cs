using System.Linq;
using YuanTu.Consts.Enums;
using YuanTu.Consts.Gateway;
using YuanTu.Default.Component.Tools.Models;

namespace YuanTu.VirtualHospital.Component.BillPay.ViewModels
{
    class BillRecordViewModel:Default.Component.BillPay.ViewModels.BillRecordViewModel
    {
        protected override void FillRechargeRequest(req缴费结算 req)
        {
            base.FillRechargeRequest(req);
            var extraPaymentModel = GetInstance<IExtraPaymentModel>();
            if (extraPaymentModel.CurrentPayMethod == PayMethod.先诊疗后付费)
                req.transNo = new string(req.flowId.Where(char.IsDigit).ToArray());
        }
    }
}

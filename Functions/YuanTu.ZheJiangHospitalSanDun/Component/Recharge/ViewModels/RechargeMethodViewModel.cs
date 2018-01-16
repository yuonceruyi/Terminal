using YuanTu.Consts.Gateway;

namespace YuanTu.ZheJiangHospitalSanDun.Component.Recharge.ViewModels
{
    class RechargeMethodViewModel:Default.Component.Recharge.ViewModels.RechargeMethodViewModel
    {
        protected override void FillRechargeRequest(req预缴金充值 req)
        {
            var track = PatientModel.Req病人信息查询.extend;
            req.cardNo = CardModel.CardNo;
            req.extend = $"{track}#";

            base.FillRechargeRequest(req);
        }
    }
}

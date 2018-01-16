using System.Collections.Generic;
using YuanTu.Consts.Enums;
using YuanTu.Consts.Gateway;
using YuanTu.Consts.Models.Payment;
using YuanTu.Core.Extension;

namespace YuanTu.ZheJiangHospitalSanDun.Component.BillPay.ViewModels
{
    class BillRecordViewModel : Default.Component.BillPay.ViewModels.BillRecordViewModel
    {
        protected override void Do()
        {
            BillRecordModel.所选缴费概要 = SelectData.Tag.As<缴费概要信息>();
            ChangeNavigationContent(SelectData.CatalogContent);

            var recordInfo = BillRecordModel.所选缴费概要;
            
            PaymentModel.Self = decimal.Parse(recordInfo.billFee);
            PaymentModel.Insurance = decimal.Parse("0");
            PaymentModel.Total = decimal.Parse(recordInfo.billFee);
            PaymentModel.NoPay = true; // 固定用账户余额支付
            PaymentModel.ConfirmAction = Confirm;

            var dateTime = recordInfo.billDate?.SafeToSplit(' ', 2);
            PaymentModel.LeftList = new List<PayInfoItem>
            {
                new PayInfoItem("日期：", dateTime?[0] ?? recordInfo.billDate),
                new PayInfoItem("时间：", dateTime?[1] ?? null),
                new PayInfoItem("科室：", recordInfo.deptName),
                new PayInfoItem("医生：", recordInfo.doctName)
            };

            PaymentModel.RightList = new List<PayInfoItem>
            {
                new PayInfoItem("个人支付：", PaymentModel.Self.In元()),
                new PayInfoItem("医保报销：", PaymentModel.Insurance.In元()),
                new PayInfoItem("支付金额：", PaymentModel.Total.In元(), true)
            };
            Next();
        }

        protected override void FillRechargeRequest(req缴费结算 req)
        {
            base.FillRechargeRequest(req);
            var recordInfo = BillRecordModel.所选缴费概要;
            req.tradeMode = PayMethod.现金.GetEnumDescription();
            req.extend = $"{recordInfo.billType}#{recordInfo.extend}";
        }
    }
}

using System.Collections.Generic;
using YuanTu.Consts.Gateway;
using YuanTu.Consts.Models.Payment;
using YuanTu.Core.Extension;

namespace YuanTu.NanYangFirstPeopleHospital.Component.BillPay.ViewModels
{
    public class BillRecordViewModel : Default.Component.BillPay.ViewModels.BillRecordViewModel
    {
        protected override void Do()
        {
            BillRecordModel.所选缴费概要 = SelectData.Tag.As<缴费概要信息>();
            ChangeNavigationContent(SelectData.CatalogContent);

            var recordInfo = BillRecordModel.所选缴费概要;

            PaymentModel.Self = decimal.Parse(recordInfo.billFee);
            PaymentModel.Insurance = decimal.Parse("0");
            PaymentModel.Total = decimal.Parse(recordInfo.billFee);
            PaymentModel.NoPay = false;
            PaymentModel.ConfirmAction = Confirm;

            PaymentModel.LeftList = new List<PayInfoItem>()
            {
                new PayInfoItem("日期：",recordInfo.billDate?.SafeToSplit(' ')?[0] ?? recordInfo.billDate),
                //new PayInfoItem("时间：",recordInfo.billDate?.SafeToSplit(' ')?[1] ?? null),
                new PayInfoItem("科室：",recordInfo.deptName),
                new PayInfoItem("医生：",recordInfo.doctName),
            };

            PaymentModel.RightList = new List<PayInfoItem>()
            {
                new PayInfoItem("个人支付：",PaymentModel.Self.In元()),
                new PayInfoItem("医保报销：",PaymentModel.Insurance.In元()),
                new PayInfoItem("支付金额：",PaymentModel.Total.In元(),true),
            };
            Next();
        }
    }
}
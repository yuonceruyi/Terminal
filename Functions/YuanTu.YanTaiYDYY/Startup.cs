using System;
using YuanTu.Consts;
using YuanTu.Core.Navigating;
using YuanTu.Default.Component.Recharge.Views;
using YuanTu.Default.Component.Register.Views;
using YuanTu.Default.Component.Tools.Views;
using YuanTu.Default.Component.Views;
using YuanTu.Default.Component.ZYRecharge.Views;
using YuanTu.YanTaiYDYY.Component.Auth.Views;
using YuanTu.YanTaiYDYY.Component.TakeNum.Views;
using YuanTu.YanTaiYDYY.Component.InfoQuery.Views;
using YuanTu.YanTaiYDYY.Component.WaiYuan.Views;
using YuanTu.YanTaiYDYY.Component.BillPay.Views;
using DefAuthVi = YuanTu.Default.Component.Auth.Views;
using DefRechargeVi = YuanTu.Default.Component.Recharge.Views;
using DefQuHaoVi = YuanTu.Default.Component.TakeNum.Views;
using DefInfoQueryVi = YuanTu.Default.Component.InfoQuery.Views;
using DefBillPayVi = YuanTu.Default.Component.BillPay.Views;
using DefRealAuthVi= YuanTu.Default.Component.RealAuth.Views;


namespace YuanTu.YanTaiYDYY
{
    public class Startup : YuanTu.Core.FrameworkBase.DefaultStartup
    {
        public override int Order => 90;

        public override string[] UseConfigPath()
        {
            return new[] { "ConfigYanTaiYDYY.xml" };
        }
        public override void AfterStartup()
        {
            FrameworkConst.HospitalName = "烟台业达医院";
            YanTaiArea.ConfigYanTai.HighPwd = "821027";
            Devices.CashBox.CashInputBox.AcceptBills =
                Devices.CashBox.CashInputBox.Bills.Bill_10 |
                Devices.CashBox.CashInputBox.Bills.Bill_20 |
                Devices.CashBox.CashInputBox.Bills.Bill_50 |
                Devices.CashBox.CashInputBox.Bills.Bill_100;
        }
        public override bool RegisterTypes(ViewCollection children)
        {
            children.Add(A.Home, null, "主页", typeof(YuanTu.Default.Component.Views.ChoiceView));
            children.Add(A.AdminPart, null, "后台", typeof(YuanTu.YanTaiYDYY.Part.Views.AdminPageView));
            children.Add(A.Third.PosUnion, null, "投币/刷卡/扫码", typeof(PosView));
            children.Add(A.Third.Cash, null, "投币/刷卡/扫码", typeof(CashView));
            children.Add(A.Third.JCMCash, null, "现金缴住院押金", typeof(JCMView));
            children.Add(A.Third.AtmCash, null, "现金缴住院押金", typeof(HatmView));
            children.Add(A.Third.ScanQrCode, null, "投币/刷卡/扫码", typeof(ScanQrCodeView));

            children.Context = A.ChaKa_Context;
            children.Add(A.CK.Select, "个人信息", typeof(DefAuthVi.SelectTypeView), A.CK.Choice);
            children.Add(A.CK.Choice, "个人信息", typeof(DefAuthVi.ChoiceView), A.CK.Card);
            children.Add(A.CK.Card, "个人信息", typeof(DefAuthVi.CardView), A.CK.Info);
            children.Add(A.CK.IDCard, "个人信息", typeof(IDCardView), A.CK.Info);
            children.Add(A.CK.InputIDCard, "个人信息", typeof(InputIDView), A.CK.Info);
            children.Add(A.CK.Info, "个人信息", typeof(PatientInfoView), A.Home);

            children.Context = A.JianDang_Context;
            children.Add(A.JD.Print, "建档结果", typeof(PrintView), A.Home);

            children.Context = A.XianChang_Context;
            children.Add(A.XC.Dept, "选择挂号科室", typeof(DeptsView), A.XC.Doctor);
            children.Add(A.XC.Doctor, "选择医生", typeof(DoctorView), A.XC.Confirm);
            children.Add(A.XC.Confirm, "选择支付方式", typeof(ConfirmView), A.XC.Print);
            children.Add(Guid.NewGuid().ToString(), "投币/刷卡/扫码", null, A.XC.Print);
            children.Add(A.XC.Print, "挂号凭条", typeof(PrintView), A.Home);


            children.Context = A.YuYue_Context;
            children.Add(A.YY.Date, "选择预约日期", typeof(RegDateView), A.YY.Wether);
            children.Add(A.YY.Wether, "选择预约类别", typeof(RegTypesView), A.YY.Dept);
            children.Add(A.YY.Dept, "选择预约科室", typeof(DeptsView), A.YY.Doctor);
            children.Add(A.YY.Doctor, "选择医生排班", typeof(DoctorView), A.YY.Schedule);
            children.Add(A.YY.Schedule, "选择医生排班", typeof(ScheduleView), A.YY.Confirm);
            children.Add(A.YY.Time, "选择医生排班", typeof(SourceView), A.YY.Confirm);
            children.Add(A.YY.Confirm, "确认预约信息", typeof(ConfirmView), A.YY.Print);
            children.Add(A.YY.Print, "预约凭条", typeof(PrintView), A.Home);

            children.Context = A.QuHao_Context;
            children.Add(A.QH.Record, "选择预约记录", typeof(DefQuHaoVi.ApptRecordView), A.QH.TakeNum);
            children.Add(A.QH.TakeNum, "确认取号信息", typeof(TakeNumView), A.QH.Confirm);
            children.Add(A.QH.Confirm, "确认取号支付", typeof(ConfirmView), A.QH.Print);
            children.Add(A.QH.Print, "取号凭条", typeof(PrintView), A.Home);

            children.Context = A.JiaoFei_Context;
            //children.Add(A.JF.BillRecord, "待缴费信息", typeof(BillRecordView), A.JF.Confirm);
            children.Add(A.JF.BillRecord, "待缴费信息", typeof(BillRecordView), A.Home);//只查看，不缴费

            children.Context = A.ChongZhi_Context;
            children.Add(A.CZ.RechargeWay, "选择充值方式", typeof(DefRechargeVi.RechargeMethodView), A.CZ.InputAmount);
            children.Add(A.CZ.InputAmount, "输入充值金额", typeof(DefRechargeVi.InputAmountView), A.CZ.Print);
            children.Add(Guid.NewGuid().ToString(), "投币/刷卡/扫码", null, A.CZ.Print);
            children.Add(A.CZ.Print, "充值完成", typeof(PrintView), A.Home);

            children.Add(A.QueryChoice, null, "查询选择", typeof(DefInfoQueryVi.QueryChoiceView));

            children.Context = A.PayCostQuery;
            children.Add(A.JFJL.Date, "选择查询日期", typeof(DefInfoQueryVi.DateTimeView), A.JFJL.PayCostRecord);
            children.Add(A.JFJL.PayCostRecord, "已缴费信息", typeof(Component.InfoQuery.Views.PayCostRecordView), A.Home);

            children.Context = A.ReChargeQuery;
            children.Add(A.CZJL.Date, "选择查询日期", typeof(DefInfoQueryVi.DateTimeView), A.CZJL.ReChargeRecord);
            children.Add(A.CZJL.ReChargeRecord, "交易记录信息", typeof(DefInfoQueryVi.RechargeRecordView), A.Home);

            children.Context = A.MedicineQuery;
            children.Add(A.YP.Query, "输入查询条件", typeof(DefInfoQueryVi.InputView), A.YP.Medicine);
            children.Add(A.YP.Medicine, "药品信息列表", typeof(DefInfoQueryVi.MedicineItemsView), A.Home);

            children.Context = A.ChargeItemsQuery;
            children.Add(A.XM.Query, "输入查询条件", typeof(DefInfoQueryVi.InputView), A.XM.ChargeItems);
            children.Add(A.XM.ChargeItems, "诊疗项列表", typeof(ChargeItemsView), A.Home);
			
            children.Context = InnerA.MaterialItemsQuery;
            children.Add(InnerA.CLF.Query, "输入查询条件", typeof(DefInfoQueryVi.InputView), InnerA.CLF.ChargeItems);
            children.Add(InnerA.CLF.ChargeItems, "材料费列表", typeof(MaterialItemsView), A.Home);

            children.Context = A.ZhuYuan_Context;
            children.Add(A.ZY.InPatientNo, "住院患者信息", typeof(DefAuthVi.InPatientNoView), A.ZY.InPatientInfo);
            children.Add(A.ZY.InPatientInfo, "住院患者信息", typeof(DefAuthVi.InPatientInfoView), A.Home);

            children.Context = A.InDayDetailList_Context;
            children.Add(A.ZYYRQD.Date, "选择查询日期", typeof(DefInfoQueryVi.DateTimeView), A.ZYYRQD.DailyDetail);
            children.Add(A.ZYYRQD.DailyDetail, "住院费用清单", typeof(InDailyDetailView), A.Home);

            children.Context = A.DiagReportQuery;
            children.Add(A.JYJL.Date, "选择查询日期", typeof(DefInfoQueryVi.DateTimeView), A.JYJL.DiagReport);
            children.Add(A.JYJL.DiagReport, "检验报告信息", typeof(DefInfoQueryVi.DiagReportView), A.Home);

            children.Context = A.IpRecharge_Context;
            children.Add(A.ZYCZ.RechargeWay, "选择支付方式", typeof(MethodView), A.ZYCZ.InputAmount);
            children.Add(A.ZYCZ.InputAmount, "输入缴纳金额", typeof(ZYInputAmountView), A.ZYCZ.Print);
            //children.Add(Guid.NewGuid().ToString(), "投币/刷卡/扫码", null, A.ZYCZ.Print);
            children.Add(A.ZYCZ.Print, "缴押金完成", typeof(PrintView), A.Home);

            children.Context = InnerA.JDChoneZhi_Context;
            children.Add(InnerA.JDCZ.RechargeWay, "选择充值方式", typeof(RechargeMethodView), InnerA.JDCZ.InputAmount);
            children.Add(InnerA.JDCZ.InputAmount, "输入充值金额", typeof(InputAmountView), InnerA.JDCZ.Print);
            children.Add(InnerA.JDCZ.Print, "建档结果", typeof(PrintView), A.Home);

            children.Context = InnerA.WaiYuanCard_Contenxt;
            children.Add(InnerA.WYC.WYCard, "插入外院卡", typeof(WaiYuanCardView), InnerA.WYC.WYPatientInfo);
            children.Add(InnerA.WYC.WYPatientInfo, "完善信息", typeof(WaiYuanPatientInfoView), A.Home);

            children.Context = InnerA.ScheduleQuery_Context;
            children.Add(InnerA.MZPBCX.ScheduleQuery, "门诊排班查询", typeof(ScheduleQueryView), A.Home);

            children.Context = A.RealAuth_Context;
            children.Add(A.SMRZ.Card, "插入就诊卡", typeof(DefRealAuthVi.CardView), A.SMRZ.PatientInfo);
            children.Add(A.SMRZ.PatientInfo, "密码校验", typeof(DefRealAuthVi.PatientInfoView), A.SMRZ.IDCard);
            children.Add(A.SMRZ.IDCard, "刷身份证", typeof(DefRealAuthVi.IDCardView), A.Home);

            return true;
        }
    }
}
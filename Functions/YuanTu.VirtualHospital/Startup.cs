using System;
using System.Collections.Generic;
using YuanTu.Consts;
using YuanTu.Core.Navigating;
using YuanTu.Default.Component.Auth.Views;
using YuanTu.Default.Component.BillPay.Views;
using YuanTu.Default.Component.InfoQuery.Views;
using YuanTu.Default.Component.Recharge.Views;
using YuanTu.Default.Component.Register.Views;
using YuanTu.Default.Component.TakeNum.Views;
using YuanTu.Default.Component.Tools.Views;
using YuanTu.Default.Component.Views;
using YuanTu.Default.Component.ZYRecharge.Views;
using YuanTu.VirtualHospital.Component.WaiYuan.Views;
using FaceRecView = YuanTu.VirtualHospital.Component.Auth.Views.FaceRecView;

namespace YuanTu.VirtualHospital
{
    public class Startup : YuanTu.Default.Startup
    {
        #region Overrides of DefaultStartup

        /// <summary>
        /// 注册视图引擎
        /// </summary>
        /// <param name="children"/>
        /// <returns/>
        public override bool RegisterTypes(ViewCollection children)
        {
            children.Add(A.Home, null, "主页", typeof(YuanTu.Default.Component.Views.ChoiceView));
            children.Add(A.AdminPart, null, "后台", typeof(YuanTu.Default.Part.Views.AdminPageView));
            children.Add(A.Maintenance, null, "维护", typeof(YuanTu.Default.Part.Views.MaintenanceView));

            children.Add(A.Third.PosUnion, null, "刷银行卡", typeof(PosView));
            children.Add(A.Third.Cash, null, "塞入纸币", typeof(CashView));
            children.Add(A.Third.ScanQrCode, null, "扫码支付", typeof(ScanQrCodeView));
            children.Add(A.Third.AtmCash, null, "现金缴住院押金", typeof(HatmView));

            children.Context = A.ChaKa_Context;
            children.Add(A.CK.Select, "个人信息", typeof(SelectTypeView), A.CK.Choice);
            children.Add(A.CK.Choice, "个人信息", typeof(YuanTu.Default.Component.Auth.Views.ChoiceView), A.CK.Card);
            children.Add(A.CK.Card, "个人信息", typeof(CardView), A.CK.Info);
            children.Add(A.CK.FaceRec, "个人信息", typeof(FaceRecView), A.CK.Info);
            children.Add(A.CK.IDCard, "个人信息", typeof(IDCardView), A.CK.Info);
            //children.Add(A.CK.InputIDCard, "个人信息", typeof(Default.Component.Auth.Views.InputIDView), A.CK.Info);
            children.Add(A.CK.HICard, "个人信息", typeof(SiCardView), A.CK.Info);
            children.Add(A.CK.QrCode, "个人信息", typeof(QrCodeView), A.CK.Info);
            children.Add(A.CK.Info, "个人信息", typeof(PatientInfoView), A.Home);
            children.Add(A.CK.InfoEx, "个人信息", typeof(WaiYuanPatientInfoView), A.Home);

            children.Context = A.JianDang_Context;
            children.Add(A.JD.Print, "建档结果", typeof(PrintView), A.Home);

            children.Context = A.XianChang_Context;
            children.Add(A.XC.Wether, "选择挂号类别", typeof(RegTypesView), A.XC.Dept);
            children.Add(A.XC.Dept, "选择挂号科室", typeof(DeptsView), A.XC.Doctor);
            children.Add(A.XC.Doctor, "选择医生排班", typeof(DoctorView), A.XC.Schedule);
            children.Add(A.XC.Schedule, "选择医生排班", typeof(ScheduleView), A.XC.Confirm);
            children.Add(A.XC.Confirm, "选择支付方式", typeof(ConfirmView), A.XC.Print);
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
            children.Add(A.QH.Record, "选择预约记录", typeof(ApptRecordView), A.QH.TakeNum);
            children.Add(A.QH.TakeNum, "确认取号信息", typeof(TakeNumView), A.QH.Confirm);
            children.Add(A.QH.Confirm, "确认取号支付", typeof(ConfirmView), A.QH.Print);
            children.Add(A.QH.Print, "取号凭条", typeof(PrintView), A.Home);

            children.Context = A.JiaoFei_Context;
            children.Add(A.JF.BillRecord, "待缴费信息", typeof(BillRecordView), A.JF.Confirm);
            children.Add(A.JF.Confirm, "确认结算支付", typeof(ConfirmView), A.JF.Print);
            children.Add(A.JF.Print, "缴费完成", typeof(PrintView), A.Home);

            children.Context = A.ChongZhi_Context;
            children.Add(A.CZ.RechargeWay, "选择充值方式", typeof(RechargeMethodView), A.CZ.InputAmount);
            children.Add(A.CZ.InputAmount, "输入充值金额", typeof(InputAmountView), A.CZ.Print);
            children.Add(A.CZ.Print, "充值完成", typeof(PrintView), A.Home);

            children.Add(A.QueryChoice, null, "查询选择", typeof(QueryChoiceView));

            children.Context = A.PayCostQuery;
            children.Add(A.JFJL.Date, "选择查询日期", typeof(DateTimeView), A.JFJL.PayCostRecord);
            children.Add(A.JFJL.PayCostRecord, "已缴费信息", typeof(PayCostRecordView), A.Home);

            children.Context = A.MedicineQuery;
            children.Add(A.YP.Query, "输入查询条件", typeof(InputView), A.YP.Medicine);
            children.Add(A.YP.Medicine, "药品信息列表", typeof(MedicineItemsView), A.Home);

            children.Context = A.ChargeItemsQuery;
            children.Add(A.XM.Query, "输入查询条件", typeof(InputView), A.XM.ChargeItems);
            children.Add(A.XM.ChargeItems, "诊疗项列表", typeof(ChargeItemsView), A.Home);


            children.Context = A.ZhuYuan_Context;
            children.Add(A.ZY.InPatientNo, "个人信息", typeof(InPatientNoView), A.ZY.InPatientInfo);
            children.Add(A.ZY.InPatientInfo, "个人信息", typeof(InPatientInfoView), A.Home);

            children.Context = InnerA.ChuYuan_Context;
            children.Add(InnerA.ChuYuan.Confirm, "出院确认", typeof(ConfirmView), InnerA.ChuYuan.Print);
            children.Add(InnerA.ChuYuan.Print, "出院完成", typeof(PrintView), A.Home);

            children.Context = A.InDayDetailList_Context;
            children.Add(A.ZYYRQD.Date, "选择查询日期", typeof(InDailyDateView), A.ZYYRQD.DailyDetail);
            children.Add(A.ZYYRQD.DailyDetail, "住院一日清单", typeof(YuanTu.VirtualHospital.Component.InfoQuery.Views.InDailyDetailView), A.Home);

            children.Context = A.IpRecharge_Context;
            children.Add(A.ZYCZ.RechargeWay, "选择支付方式", typeof(MethodView), A.ZYCZ.InputAmount);
            children.Add(A.ZYCZ.InputAmount, "输入缴纳金额", typeof(ZYInputAmountView), A.ZYCZ.Print);
            children.Add(A.ZYCZ.Print, "缴押金完成", typeof(PrintView), A.Home);

            children.Context = A.DiagReportQuery;
            children.Add(A.JYJL.Date, "选择查询日期", typeof(DateTimeView), A.JYJL.DiagReport);
            children.Add(A.JYJL.DiagReport, "检验报告信息", typeof(DiagReportView), A.Home);


            children.Context = InnerA.JDChoneZhi_Context;
            children.Add(InnerA.JDCZ.RechargeWay, "选择充值方式", typeof(RechargeMethodView), InnerA.JDCZ.InputAmount);
            children.Add(InnerA.JDCZ.InputAmount, "输入充值金额", typeof(InputAmountView), InnerA.JDCZ.Print);
            children.Add(InnerA.JDCZ.Print, "建档结果", typeof(PrintView), A.Home);

            children.Context = InnerA.WaiYuanCard_Contenxt;
            children.Add(InnerA.WYC.WYCard, "插入外院卡", typeof(WaiYuanCardView), InnerA.WYC.WYPatientInfo);
            children.Add(InnerA.WYC.WYPatientInfo, "完善信息", typeof(WaiYuanPatientInfoView), A.Home);

            children.Context = A.Biometric_Context;
            children.Add(A.Bio.Choice, "选择录入信息", typeof(Component.Biometric.Views.ChoiceView), A.Bio.FaceRec);
            children.Add(A.Bio.FaceRec, "面部信息录入", typeof(Component.Biometric.Views.FaceRecView), A.Home);
            children.Add(A.Bio.FingerPrint, "指纹信息录入", typeof(Component.Biometric.Views.FingerPrintView), A.Home);

            children.Context = InnerA.Loan_Context;
            children.Add(InnerA.Loan.Choice, "功能选择", typeof(Component.Loan.Views.ChoiceView), A.Home);
            children.Add(InnerA.Loan.Info, "借款人当前状态", typeof(Component.Loan.Views.InfoView), InnerA.Loan.Choice);
            children.Add(InnerA.Loan.Date, "选择查询日期", typeof(Component.Loan.Views.DateTimeView), InnerA.Loan.Records);
            children.Add(InnerA.Loan.Records, "选择还款记录", typeof(Component.Loan.Views.RecordsView), InnerA.Loan.RepayMethod);
            children.Add(InnerA.Loan.RepayMethod, "选择还款方式", typeof(Component.Loan.Views.RepayMethodView), InnerA.Loan.Print);
            children.Add(InnerA.Loan.Print, "还款结果", typeof(PrintView), A.Home);

            string deviceType = CurrentStrategyType();
            if (deviceType == DeviceType.Clinic)
                RegisterClinicTypes(children);
            if (deviceType == DeviceType.Tablet)
                RegisterTabletTypes(children);
            return true;
        }


        public bool RegisterClinicTypes(ViewCollection children)
        {
            children.Add(A.Home, null, "主页", typeof(Default.Clinic.Component.Views.ChoiceView));
            children.Add(A.AdminPart, null, "后台", typeof(Default.Part.Views.AdminPageView));
            children.Add(A.Third.PosUnion, null, "刷卡", typeof(PosView));
            children.Add(A.Third.Cash, null, "投币", typeof(Default.Clinic.Component.Recharge.Views.CashView));
            children.Add(A.Third.ScanQrCode, null, "扫码", typeof(ScanQrCodeView));

            children.Context = A.ChaKa_Context;
            children.Add(A.CK.Select, "个人信息", typeof(SelectTypeView), A.CK.Choice);
            children.Add(A.CK.Choice, "个人信息", typeof(Default.Clinic.Component.Auth.Views.ChoiceView), A.CK.Card);
            children.Add(A.CK.Card, "个人信息", typeof(CardView), A.CK.Info);
            children.Add(A.CK.IDCard, "个人信息", typeof(IDCardView), A.CK.Info);
            children.Add(A.CK.HICard, "个人信息", typeof(SiCardView), A.CK.Info);
            children.Add(A.CK.Info, "个人信息", typeof(PatientInfoView), A.Home);
            children.Add(A.CK.InfoEx, "个人信息", typeof(PatientInfoExView), A.Home);

            children.Context = A.JianDang_Context;
            children.Add(A.JD.Print, "建档结果", typeof(PrintView), A.Home);

            children.Context = A.XianChang_Context;
            children.Add(A.XC.Wether, "选择挂号类别", typeof(Default.Clinic.Component.Register.Views.RegTypesView), A.XC.Dept);
            children.Add(A.XC.Dept, "选择挂号科室", typeof(Default.Clinic.Component.Register.Views.DeptsView), A.XC.Doctor);
            children.Add(A.XC.Doctor, "选择医生排班", typeof(Default.Clinic.Component.Register.Views.DoctorView), A.XC.Schedule);
            children.Add(A.XC.Schedule, "选择医生排班", typeof(Default.Clinic.Component.Register.Views.ScheduleView), A.XC.Confirm);
            children.Add(A.XC.Confirm, "选择支付方式", typeof(Default.Clinic.Component.Views.ConfirmView), A.XC.Print);
            //children.Add(Guid.NewGuid().ToString(), "投币/刷卡/扫码", null, A.XC.Print);
            children.Add(A.XC.Print, "挂号凭条", typeof(PrintView), A.Home);

            children.Context = A.YuYue_Context;
            children.Add(A.YY.Date, "选择预约日期", typeof(Default.Clinic.Component.Register.Views.RegDateView), A.YY.Wether);
            children.Add(A.YY.Wether, "选择预约类别", typeof(Default.Clinic.Component.Register.Views.RegTypesView), A.YY.Dept);
            children.Add(A.YY.Dept, "选择预约科室", typeof(Default.Clinic.Component.Register.Views.DeptsView), A.YY.Doctor);
            children.Add(A.YY.Doctor, "选择医生排班", typeof(Default.Clinic.Component.Register.Views.DoctorView), A.YY.Schedule);
            children.Add(A.YY.Schedule, "选择医生排班", typeof(Default.Clinic.Component.Register.Views.ScheduleView), A.YY.Confirm);
            children.Add(A.YY.Time, "选择医生排班", typeof(Default.Clinic.Component.Register.Views.SourceView), A.YY.Confirm);
            children.Add(A.YY.Confirm, "确认预约信息", typeof(Default.Clinic.Component.Views.ConfirmView), A.YY.Print);
            children.Add(A.YY.Print, "预约凭条", typeof(PrintView), A.Home);

            children.Context = A.QuHao_Context;
            children.Add(A.QH.Record, "选择预约记录", typeof(Default.Clinic.Component.TakeNum.Views.ApptRecordView), A.QH.TakeNum);
            children.Add(A.QH.TakeNum, "确认取号信息", typeof(TakeNumView), A.QH.Confirm);
            children.Add(A.QH.Confirm, "确认取号支付", typeof(Default.Clinic.Component.Views.ConfirmView), A.QH.Print);
            children.Add(A.QH.Print, "取号凭条", typeof(PrintView), A.Home);

            children.Context = A.JiaoFei_Context;
            children.Add(A.JF.BillRecord, "待缴费信息", typeof(Default.Clinic.Component.BillPay.Views.BillRecordView), A.JF.Confirm);
            children.Add(A.JF.Confirm, "确认结算支付", typeof(Default.Clinic.Component.Views.ConfirmView), A.JF.Print);
            children.Add(A.JF.Print, "缴费完成", typeof(PrintView), A.Home);

            children.Context = A.ChongZhi_Context;
            children.Add(A.CZ.RechargeWay, "选择充值方式", typeof(Default.Clinic.Component.Recharge.Views.RechargeMethodView), A.CZ.InputAmount);
            children.Add(A.CZ.InputAmount, "输入充值金额", typeof(InputAmountView), A.CZ.Print);
            //children.Add(Guid.NewGuid().ToString(), "投币/刷卡/扫码", null, A.CZ.Print);
            children.Add(A.CZ.Print, "充值完成", typeof(PrintView), A.Home);

            children.Add(A.QueryChoice, null, "查询选择", typeof(QueryChoiceView));

            children.Context = A.PayCostQuery;
            children.Add(A.JFJL.Date, "选择查询日期", typeof(DateTimeView), A.JFJL.PayCostRecord);
            children.Add(A.JFJL.PayCostRecord, "已缴费信息", typeof(PayCostRecordView), A.Home);

            children.Context = A.MedicineQuery;
            children.Add(A.YP.Query, "输入查询条件", typeof(InputView), A.YP.Medicine);
            children.Add(A.YP.Medicine, "药品信息列表", typeof(MedicineItemsView), A.Home);

            children.Context = A.ChargeItemsQuery;
            children.Add(A.XM.Query, "输入查询条件", typeof(InputView), A.XM.ChargeItems);
            children.Add(A.XM.ChargeItems, "诊疗项列表", typeof(ChargeItemsView), A.Home);


            children.Context = A.ZhuYuan_Context;
            children.Add(A.ZY.InPatientNo, "个人信息", typeof(InPatientNoView), A.ZY.InPatientInfo);
            children.Add(A.ZY.InPatientInfo, "个人信息", typeof(InPatientInfoView), A.Home);

            children.Context = A.InDayDetailList_Context;
            children.Add(A.ZYYRQD.Date, "选择查询日期", typeof(InDailyDateView), A.ZYYRQD.DailyDetail);
            children.Add(A.ZYYRQD.DailyDetail, "住院一日清单", typeof(InDailyDetailView), A.Home);

            children.Context = A.IpRecharge_Context;
            children.Add(A.ZYCZ.RechargeWay, "选择支付方式", typeof(MethodView), A.ZYCZ.InputAmount);
            children.Add(A.ZYCZ.InputAmount, "输入缴纳金额", typeof(ZYInputAmountView), A.ZYCZ.Print);
            //children.Add(Guid.NewGuid().ToString(), "投币/刷卡/扫码", null, A.ZYCZ.Print);
            children.Add(A.ZYCZ.Print, "缴押金完成", typeof(PrintView), A.Home);

            return true;
        }

        public bool RegisterTabletTypes(ViewCollection children)
        {
            children.Add(A.Home, null, "主页", typeof(YuanTu.Default.Component.Views.ChoiceView));
            children.Add(A.AdminPart, null, "后台", typeof(YuanTu.Default.Part.Views.AdminPageView));
            children.Add(A.Maintenance, null, "维护", typeof(YuanTu.Default.Part.Views.MaintenanceView));
            children.Add(A.Third.PosUnion, null, "刷卡", typeof(PosView));
            children.Add(A.Third.ScanQrCode, null, "扫码", typeof(ScanQrCodeView));
            children.Add(YuanTu.Default.Tablet.AInner.SY.Choice, null, "选择业务", typeof(YuanTu.Default.Tablet.Component.Cashier.Views.ChoiceView));

            children.Context = A.ChaKa_Context;
            children.Add(A.CK.Choice, "个人信息", typeof(Default.Component.Auth.Views.ChoiceView), A.CK.Card);
            children.Add(A.CK.Card, "个人信息", typeof(Default.Tablet.Component.Auth.Views.CardView), A.CK.Info);
            children.Add(A.CK.IDCard, "个人信息", typeof(IDCardView), A.CK.Info);
            children.Add(A.CK.HICard, "个人信息", typeof(SiCardView), A.CK.Info);
            children.Add(A.CK.Info, "个人信息", typeof(PatientInfoView), A.Home);
            children.Add(A.CK.InfoEx, "个人信息", typeof(PatientInfoExView), A.Home);

            children.Context = A.XianChang_Context;
            children.Add(YuanTu.Default.Tablet.AInner.XC.Hospitals, "选择医院", typeof(YuanTu.Default.Tablet.Component.Register.Views.HospitalsView), A.XC.Dept);
            children.Add(A.XC.Dept, "选择科室", typeof(DeptsView), A.XC.Schedule);
            children.Add(A.XC.Schedule, "选择排班", typeof(ScheduleView), A.XC.Confirm);
            children.Add(A.XC.Confirm, "选择支付方式", typeof(ConfirmView), A.XC.Print);
            children.Add(A.XC.Print, "挂号结果", typeof(PrintView), A.Home);

            children.Context = A.YuYue_Context;
            children.Add(YuanTu.Default.Tablet.AInner.YY.Hospitals, "选择医院", typeof(YuanTu.Default.Tablet.Component.Register.Views.HospitalsView), A.YY.Dept);
            children.Add(A.YY.Dept, "选择科室", typeof(DeptsView), A.YY.Schedule);
            children.Add(A.YY.Schedule, "选择排班", typeof(ScheduleView), A.YY.Confirm);
            children.Add(A.YY.Time, "选择时间", typeof(SourceView), A.YY.Confirm);
            children.Add(A.YY.Confirm, "确认信息", typeof(ConfirmView), A.YY.Print);
            children.Add(A.YY.Print, "预约结果", typeof(PrintView), A.Home);

            children.Context = YuanTu.Default.Tablet.AInner.Sale;
            children.Add(YuanTu.Default.Tablet.AInner.SY.Amount, "输入金额", typeof(YuanTu.Default.Tablet.Component.Cashier.Views.AmountView), YuanTu.Default.Tablet.AInner.SY.Card);
            children.Add(YuanTu.Default.Tablet.AInner.SY.Card, "刷卡", typeof(YuanTu.Default.Tablet.Component.Cashier.Views.CardView), YuanTu.Default.Tablet.AInner.SY.Print);
            children.Add(YuanTu.Default.Tablet.AInner.SY.Scan, "扫码", typeof(YuanTu.Default.Tablet.Component.Cashier.Views.ScanView), YuanTu.Default.Tablet.AInner.SY.Print);
            children.Add(YuanTu.VirtualHospital.InnerA.SY.FaceRec, "刷脸", typeof(YuanTu.VirtualHospital.Tablet.Component.Cashier.Views.FaceRecView), YuanTu.Default.Tablet.AInner.SY.Print);
            children.Add(YuanTu.Default.Tablet.AInner.SY.Print, "支付结果", typeof(YuanTu.Default.Tablet.Component.Cashier.Views.PrintView), A.Home);

            children.Context = YuanTu.Default.Tablet.AInner.Refund;
            children.Add(YuanTu.Default.Tablet.AInner.SY.Card, "刷卡", typeof(YuanTu.Default.Tablet.Component.Cashier.Views.CardView), YuanTu.Default.Tablet.AInner.SY.Select);
            children.Add(YuanTu.Default.Tablet.AInner.SY.Select, "选择订单", typeof(YuanTu.Default.Tablet.Component.Cashier.Views.SelectView), YuanTu.Default.Tablet.AInner.SY.Confirm);
            children.Add(YuanTu.Default.Tablet.AInner.SY.Confirm, "确认订单", typeof(YuanTu.Default.Tablet.Component.Cashier.Views.ConfirmView), YuanTu.Default.Tablet.AInner.SY.Print);
            children.Add(YuanTu.Default.Tablet.AInner.SY.Print, "退款结果", typeof(YuanTu.Default.Tablet.Component.Cashier.Views.PrintView), A.Home);

            return true;
        }

        /// <summary>
        /// 向主程序提供配置信息，优先采用插件内部配置
        /// </summary>
        /// <returns>
        /// 返回配置文件完整路径(支持xml,json,ini)
        /// </returns>
        public override string[] UseConfigPath()
        {
            return new[] { "CurrentResource\\虚拟医院.xml" };
        }

        public override void AfterStartup()
        {
            FrameworkConst.HospitalName = "远图智慧医院";
            先诊疗后付费 = true;
        }

        public static bool 先诊疗后付费 { get; set; }

        #endregion
    }
}

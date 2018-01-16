using System;
using System.Collections.Generic;
using System.IO;
using YuanTu.Consts;
using YuanTu.Core.Navigating;

namespace YuanTu.ShenZhen.BaoAnPeopleHospital
{
    public class Startup : Default.Startup
    {
        public override string[] UseConfigPath()
        {
            var file = Path.Combine("CurrentResource", FrameworkConst.HospitalId, "Config.xml");
            var file1 = Path.Combine("CurrentResource", FrameworkConst.HospitalId + ".xml");
            return new[] { file, file1 };
        }

        public override void AfterStartup()
        {
            base.AfterStartup();
            //FrameworkConst.HospitalName = "深圳市宝安人民医院(集团)第一人民医院";
            FrameworkConst.HospitalName = "深圳市宝安区人民医院";
            ConfigBaoAnPeopleHospital.Init();
        }

        public override bool RegisterTypes(ViewCollection children)
        {
            children.Add(A.Home, null, "主页", typeof(Default.Component.Views.ChoiceView));
            children.Add(A.AdminPart, null, "后台", typeof(Default.Part.Views.AdminPageView));
            children.Add(A.Maintenance, null, "维护", typeof(Default.Part.Views.MaintenanceView));
            children.Add(A.Third.PosUnion, null, "刷卡", typeof(Default.Component.Tools.Views.PosView));
            children.Add(A.Third.Cash, null, "投币", typeof(Default.Component.Recharge.Views.CashView));
            children.Add(A.Third.AtmCash, null, "现金缴住院押金", typeof(Default.Component.ZYRecharge.Views.HatmView));
            children.Add(A.Third.JCMCash, null, "现金缴住院押金", typeof(Default.Component.ZYRecharge.Views.JCMView));
            children.Add(A.Third.ScanQrCode, null, "扫码", typeof(Default.Component.Tools.Views.ScanQrCodeView));

            children.Context = A.ChaKa_Context;
            children.Add(A.CK.Select, "个人信息", typeof(Default.Component.Auth.Views.SelectTypeView), A.CK.Choice);
            children.Add(A.CK.Choice, "个人信息", typeof(Default.Component.Auth.Views.ChoiceView), A.CK.Card);
            children.Add(A.CK.Card, "个人信息", typeof(Default.Component.Auth.Views.CardView), A.CK.Info);
            children.Add(A.CK.IDCard, "个人信息", typeof(Default.Component.Auth.Views.IDCardView), A.CK.Info);
            children.Add(A.CK.HICard, "个人信息", typeof(Component.Auth.Views.SiCardView), InnerA.CK.HICardPassword);
            children.Add(InnerA.CK.HICardPassword, "个人信息", typeof(Component.Auth.Views.SiCardPasswordView), A.CK.Info);
            children.Add(A.CK.QrCode, "个人信息", typeof(Default.Component.Auth.Views.QrCodeView), A.CK.Info);
            children.Add(InnerA.CK.PatientNumber, "个人信息", typeof(Component.Auth.Views.PatientNumberView), A.CK.Info);
            children.Add(A.CK.Info, "个人信息", typeof(Default.Component.Auth.Views.PatientInfoView), A.Home);
            children.Add(A.CK.InfoEx, "个人信息", typeof(Default.Component.Auth.Views.PatientInfoExView), A.Home);

            children.Context = A.JianDang_Context;
            children.Add(A.JD.Print, "建档结果", typeof(Default.Component.Views.PrintView), A.Home);

            children.Context = A.XianChang_Context;
            children.Add(A.XC.Wether, "选择挂号类别", typeof(Default.Component.Register.Views.RegTypesView), A.XC.Dept);
            children.Add(A.XC.Dept, "选择挂号科室", typeof(Default.Component.Register.Views.DeptsView), A.XC.Doctor);
            children.Add(A.XC.Doctor, "选择医生排班", typeof(Default.Component.Register.Views.DoctorView), A.XC.Schedule);
            children.Add(A.XC.Schedule, "选择医生排班", typeof(Default.Component.Register.Views.ScheduleView), A.XC.Confirm);
            children.Add(A.XC.Confirm, "选择支付方式", typeof(Default.Component.Views.ConfirmView), A.XC.Print);
            //children.Add(Guid.NewGuid().ToString(), "投币/刷卡/扫码", null, A.XC.Print);
            children.Add(A.XC.Print, "挂号凭条", typeof(Default.Component.Views.PrintView), A.Home);

            children.Context = A.YuYue_Context;
            children.Add(A.YY.Date, "选择预约日期", typeof(Default.Component.Register.Views.RegDateView), A.YY.Wether);
            children.Add(A.YY.Wether, "选择预约类别", typeof(Default.Component.Register.Views.RegTypesView), A.YY.Dept);
            children.Add(A.YY.Dept, "选择预约科室", typeof(Default.Component.Register.Views.DeptsView), A.YY.Doctor);
            children.Add(A.YY.Doctor, "选择医生排班", typeof(Default.Component.Register.Views.DoctorView), A.YY.Schedule);
            children.Add(A.YY.Schedule, "选择医生排班", typeof(Default.Component.Register.Views.ScheduleView), A.YY.Confirm);
            children.Add(A.YY.Time, "选择排班分时", typeof(Default.Component.Register.Views.SourceView), A.YY.Confirm);
            children.Add(A.YY.Confirm, "确认预约信息", typeof(Default.Component.Views.ConfirmView), A.YY.Print);
            children.Add(A.YY.Print, "预约凭条", typeof(Default.Component.Views.PrintView), A.Home);

            //输入订单号后直接去取号。
            children.Context = A.QuHao_Context;
            children.Add(A.QH.Query, "输入取号订单号", typeof(Component.TakeNum.Views.QueryView), A.QH.Print);
            children.Add(A.QH.Print, "取号凭条", typeof(Default.Component.Views.PrintView), A.Home);

            children.Context = A.JiaoFei_Context;
            children.Add(A.JF.BillRecord, "待缴费信息", typeof(Default.Component.BillPay.Views.BillRecordView), A.JF.Confirm);
            children.Add(A.JF.Confirm, "确认结算支付", typeof(Default.Component.Views.ConfirmView), A.JF.Print);
            children.Add(A.JF.Print, "缴费完成", typeof(Default.Component.Views.PrintView), A.Home);

            children.Context = A.ChongZhi_Context;
            children.Add(A.CZ.RechargeWay, "选择充值方式", typeof(Default.Component.Recharge.Views.RechargeMethodView), A.CZ.InputAmount);
            children.Add(A.CZ.InputAmount, "输入充值金额", typeof(Default.Component.Recharge.Views.InputAmountView), A.CZ.Print);
            //children.Add(Guid.NewGuid().ToString(), "投币/刷卡/扫码", null, A.CZ.Print);
            children.Add(A.CZ.Print, "充值完成", typeof(Default.Component.Views.PrintView), A.Home);

            children.Add(A.QueryChoice, null, "查询选择", typeof(Default.Component.InfoQuery.Views.QueryChoiceView));

            children.Context = A.PayCostQuery;
            children.Add(A.JFJL.Date, "选择查询日期", typeof(Default.Component.InfoQuery.Views.DateTimeView), A.JFJL.PayCostRecord);
            children.Add(A.JFJL.PayCostRecord, "已缴费信息", typeof(Default.Component.InfoQuery.Views.PayCostRecordView), A.Home);

            children.Context = A.MedicineQuery;
            children.Add(A.YP.Query, "输入查询条件", typeof(Default.Component.InfoQuery.Views.InputView), A.YP.Medicine);
            children.Add(A.YP.Medicine, "药品信息列表", typeof(Component.InfoQuery.Views.MedicineItemsView), A.Home);

            children.Context = A.ChargeItemsQuery;
            children.Add(A.XM.Query, "输入查询条件", typeof(Default.Component.InfoQuery.Views.InputView), A.XM.ChargeItems);
            children.Add(A.XM.ChargeItems, "诊疗项列表", typeof(Component.InfoQuery.Views.ChargeItemsView), A.Home);

            children.Context = A.DiagReportQuery;
            children.Add(A.JYJL.Date, "选择查询日期", typeof(Default.Component.InfoQuery.Views.DateTimeView), A.JYJL.DiagReport);
            children.Add(A.JYJL.DiagReport, "检验报告信息", typeof(Default.Component.InfoQuery.Views.DiagReportView), A.Home);

            children.Context = A.PacsReportQuery;
            children.Add(A.YXBG.Date, "选择查询日期", typeof(Default.Component.InfoQuery.Views.DateTimeView), A.YXBG.PacsReport);
            children.Add(A.YXBG.PacsReport, "影像报告信息", typeof(Default.Component.InfoQuery.Views.PacsReportView), A.Home);

            children.Context = A.ReChargeQuery;
            children.Add(A.CZJL.Date, "选择查询日期", typeof(Default.Component.InfoQuery.Views.DateTimeView), A.CZJL.ReChargeRecord);
            children.Add(A.CZJL.ReChargeRecord, "预交金记录信息", typeof(Component.InfoQuery.Views.RechargeRecordView), A.Home);

            children.Context = A.ZhuYuan_Context;
            children.Add(A.ZY.Choice, "个人信息", typeof(Default.Component.Auth.Views.ChoiceView), A.ZY.Card);
            children.Add(A.ZY.Card, "个人信息", typeof(Default.Component.Auth.Views.InCardView), A.ZY.InPatientInfo);
            children.Add(A.ZY.IDCard, "个人信息", typeof(Default.Component.Auth.Views.InIDCardView), A.ZY.InPatientInfo);
            children.Add(A.ZY.HICard, "个人信息", typeof(Default.Component.Auth.Views.InSiCardView), A.ZY.InPatientInfo);
            children.Add(A.ZY.InPatientNo, "住院患者信息", typeof(Component.Auth.Views.InPatientNoView), A.ZY.InPatientInfo);
            children.Add(A.ZY.InPatientInfo, "住院患者信息", typeof(Default.Component.Auth.Views.InPatientInfoView), A.Home);


            children.Context = A.InDayDetailList_Context;
            children.Add(A.ZYYRQD.Date, "选择查询日期", typeof(Default.Component.InfoQuery.Views.InDailyDateView), A.ZYYRQD.DailyDetail);
            children.Add(A.ZYYRQD.DailyDetail, "住院费用清单", typeof(Default.Component.InfoQuery.Views.InDailyDetailView), InnerA.ZYYRQD.Print);
            children.Add(InnerA.ZYYRQD.Print, "打印住院一日清单", typeof(Default.Component.Views.PrintView), A.Home);


            children.Context =InnerA.InHospitalFeeList_Context;
            children.Add(InnerA.ZYFYCX.Date, "选择查询日期", typeof(Default.Component.InfoQuery.Views.DateTimeView), A.ZYYRQD.DailyDetail);
            children.Add(InnerA.ZYFYCX.DailyDetail, "住院费用清单", typeof(Default.Component.InfoQuery.Views.InDailyDetailView), A.Home);


            children.Context = A.IpRecharge_Context;
            children.Add(A.ZYCZ.RechargeWay, "选择支付方式", typeof(Default.Component.ZYRecharge.Views.MethodView), A.ZYCZ.InputAmount);
            children.Add(A.ZYCZ.InputAmount, "输入缴纳金额", typeof(Default.Component.ZYRecharge.Views.ZYInputAmountView), A.ZYCZ.Print);
            //children.Add(Guid.NewGuid().ToString(), "投币/刷卡/扫码", null, A.ZYCZ.Print);
            children.Add(A.ZYCZ.Print, "缴押金完成", typeof(Default.Component.Views.PrintView), A.Home);

            children.Add(A.PrintAgainChoice, null, "补打选择", typeof(Default.Component.PrintAgain.Views.PrintAgainChoiceView));
            children.Context = A.PayCostQuery;
            children.Add(A.JFBD.Date, "选择查询日期", typeof(Default.Component.PrintAgain.Views.DateTimeView), A.JFBD.PayCostRecord);
            children.Add(A.JFBD.PayCostRecord, "已缴费信息", typeof(Default.Component.PrintAgain.Views.PayCostRecordView), A.Home);
            children.Add(A.JFBD.Print, "补打完成", typeof(Default.Component.Views.PrintView), A.Home);

            children.Context = A.RealAuth_Context;
            children.Add(A.SMRZ.Card, "插入就诊卡", typeof(Default.Component.RealAuth.Views.CardView), A.SMRZ.PatientInfo);
            children.Add(A.SMRZ.PatientInfo, "密码校验", typeof(Default.Component.RealAuth.Views.PatientInfoView), A.SMRZ.IDCard);
            children.Add(A.SMRZ.IDCard, "刷身份证", typeof(Default.Component.RealAuth.Views.IDCardView), A.Home);

            return true;
        }

    }
}
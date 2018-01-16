using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Practices.ServiceLocation;
using YuanTu.Consts;
using YuanTu.Consts.Services;
using YuanTu.Core.Extension;
using YuanTu.Core.Navigating;
using YuanTu.Default.Component.Auth.Views;
using YuanTu.Default.Component.BillPay.Views;
using YuanTu.Default.Component.InfoQuery.Views;
using YuanTu.Default.Component.PrintAgain.Views;
using YuanTu.Default.Component.Recharge.Views;
using YuanTu.Default.Component.Register.Views;
using YuanTu.Default.Component.SignIn.Views;
using YuanTu.Default.Component.TakeNum.Views;
using YuanTu.Default.Component.Tools.Views;
using YuanTu.Default.Component.Views;
using YuanTu.Default.Component.ZYRecharge.Views;
using YuanTu.Default.Part.Views;
using ChoiceView = YuanTu.Default.Component.Views.ChoiceView;
using DateTimeView = YuanTu.Default.Component.InfoQuery.Views.DateTimeView;
using InPatientInfoView = YuanTu.NingXiaHospital.Component.Auth.Views.InPatientInfoView;
using PatientInfoView = YuanTu.NingXiaHospital.Component.Auth.Views.PatientInfoView;
using PayCostRecordView = YuanTu.Default.Component.InfoQuery.Views.PayCostRecordView;
using SiCardView = YuanTu.NingXiaHospital.Component.Auth.Views.SiCardView;

namespace YuanTu.NingXiaHospital
{
    public class Startup : Default.Startup
    {
        /// <summary>
        ///     优先级，数值越小优先级越高，内部配置越优先被使用
        /// </summary>
        public override int Order => 1;

        public static string VideoPath { get; set; }

        public override bool RegisterTypes(ViewCollection children)
        {
            children.Add(A.Home, null, "主页", typeof(ChoiceView));
            children.Add(A.AdminPart, null, "后台", typeof(AdminPageView));
            children.Add(A.Maintenance, null, "维护", typeof(MaintenanceView));
            children.Add(A.Third.PosUnion, null, "刷卡", typeof(PosView));
            children.Add(A.Third.Cash, null, "投币", typeof(CashView));
            children.Add(A.Third.AtmCash, null, "现金缴住院押金", typeof(HatmView));
            children.Add(A.Third.JCMCash, null, "现金缴住院押金", typeof(JCMView));
            children.Add(A.Third.ScanQrCode, null, "扫码", typeof(ScanQrCodeView));

            children.Context = A.ChaKa_Context;
            children.Add(A.CK.Select, "个人信息", typeof(SelectTypeView), A.CK.Choice);
            children.Add(A.CK.Choice, "个人信息", typeof(Default.Component.Auth.Views.ChoiceView), A.CK.Card);
            children.Add(A.CK.Card, "个人信息", typeof(Component.Auth.Views.CardView), A.CK.Info);
            children.Add(A.CK.IDCard, "个人信息", typeof(IDCardView), A.CK.Info);
            children.Add(A.CK.HICard, "个人信息", typeof(SiCardView), A.CK.Info);
            children.Add(A.CK.QrCode, "个人信息", typeof(QrCodeView), A.CK.Info);
            children.Add(A.CK.Info, "个人信息", typeof(PatientInfoView), A.Home);
            children.Add(A.CK.InfoEx, "个人信息", typeof(PatientInfoExView), A.Home);

            children.Context = A.JianDang_Context;
            children.Add(A.JD.Print, "建档结果", typeof(PrintView), A.Home);

            children.Context = A.XianChang_Context;
            //children.Add(A.XC.Wether, "选择挂号类别", typeof(RegTypesView), A.XC.Dept);
            children.Add(A.XC.Dept, "选择挂号科室", typeof(DeptsView), A.XC.Doctor);
            children.Add(A.XC.Doctor, "选择医生", typeof(DoctorView), A.XC.Confirm);
            //children.Add(A.XC.Schedule, "选择医生排班", typeof(ScheduleView), A.XC.Confirm);
            children.Add(A.XC.Confirm, "选择支付方式", typeof(ConfirmView), A.XC.Print);
            children.Add(A.XC.Print, "挂号凭条", typeof(PrintView), A.Home);

            children.Context = A.YuYue_Context;
            children.Add(A.YY.Date, "选择预约日期", typeof(RegDateView), A.YY.Wether);
            children.Add(A.YY.Wether, "选择预约类别", typeof(RegTypesView), A.YY.Dept);
            children.Add(A.YY.Dept, "选择预约科室", typeof(DeptsView), A.YY.Doctor);
            children.Add(A.YY.Doctor, "选择医生排班", typeof(DoctorView), A.YY.Schedule);
            children.Add(A.YY.Schedule, "选择医生排班", typeof(ScheduleView), A.YY.Confirm);
            children.Add(A.YY.Time, "选择排班分时", typeof(SourceView), A.YY.Confirm);
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

            children.Context = A.DiagReportQuery;
            children.Add(A.JYJL.Date, "选择查询日期", typeof(DateTimeView), A.JYJL.DiagReport);
            children.Add(A.JYJL.DiagReport, "检验报告信息", typeof(DiagReportView), A.Home);

            children.Context = A.PacsReportQuery;
            children.Add(A.YXBG.Date, "选择查询日期", typeof(DateTimeView), A.YXBG.PacsReport);
            children.Add(A.YXBG.PacsReport, "影像报告信息", typeof(PacsReportView), A.Home);

            children.Context = A.ReChargeQuery;
            children.Add(A.CZJL.Date, "选择查询日期", typeof(DateTimeView), A.CZJL.ReChargeRecord);
            children.Add(A.CZJL.ReChargeRecord, "预交金记录信息", typeof(RechargeRecordView), A.Home);

            children.Context = A.ZhuYuan_Context;
            children.Add(A.ZY.Choice, "个人信息", typeof(Default.Component.Auth.Views.ChoiceView), A.ZY.Card);
            children.Add(A.ZY.Card, "个人信息", typeof(InCardView), A.ZY.InPatientInfo);
            children.Add(A.ZY.IDCard, "个人信息", typeof(Default.Component.Auth.Views.InIDCardView), A.ZY.InPatientInfo);
            children.Add(A.ZY.HICard, "个人信息", typeof(InSiCardView), A.ZY.InPatientInfo);
            children.Add(A.ZY.InPatientNo, "住院患者信息", typeof(InPatientNoView), A.ZY.InPatientInfo);
            children.Add(A.ZY.InPatientInfo, "住院患者信息", typeof(InPatientInfoView), A.Home);

            children.Context = A.InDayDetailList_Context;
            children.Add(A.ZYYRQD.Date, "选择查询日期", typeof(InDailyDateView), A.ZYYRQD.DailyDetail);
            children.Add(A.ZYYRQD.DailyDetail, "住院一日清单", typeof(InDailyDetailView), A.Home);

            children.Context = A.IpRecharge_Context;
            children.Add(A.ZYCZ.RechargeWay, "选择支付方式", typeof(MethodView), A.ZYCZ.InputAmount);
            children.Add(A.ZYCZ.InputAmount, "输入缴纳金额", typeof(ZYInputAmountView), A.ZYCZ.Print);
            //children.Add(Guid.NewGuid().ToString(), "投币/刷卡/扫码", null, A.ZYCZ.Print);
            children.Add(A.ZYCZ.Print, "缴押金完成", typeof(PrintView), A.Home);

            children.Add(A.PrintAgainChoice, null, "补打选择", typeof(PrintAgainChoiceView));
            children.Context = A.PayCostQuery;
            children.Add(A.JFBD.Date, "选择查询日期", typeof(Default.Component.PrintAgain.Views.DateTimeView),
                A.JFBD.PayCostRecord);
            children.Add(A.JFBD.PayCostRecord, "已缴费信息", typeof(Default.Component.PrintAgain.Views.PayCostRecordView),
                A.Home);
            children.Add(A.JFBD.Print, "补打完成", typeof(PrintView), A.Home);


            children.Context = A.QueneSelect_Context;
            children.Add(A.SignIn.RegisterInfoSelect, "排队叫号签到", typeof(QueueSelectView), A.Home);
            if (CurrentStrategyType() == DeviceType.Clinic)
                RegisterClinicTypes(children);
            return true;
        }

        private bool RegisterClinicTypes(ViewCollection children)
        {
            children.Add(A.Home, null, "主页", typeof(Default.Clinic.Component.Views.ChoiceView));
            children.Add(A.Third.Cash, null, "投币", typeof(Default.Clinic.Component.Recharge.Views.CashView));


            children.Context = A.ChaKa_Context;
            children.Add(A.CK.Choice, "个人信息", typeof(Default.Clinic.Component.Auth.Views.ChoiceView), A.CK.Card);

            children.Context = A.XianChang_Context;
            children.Add(A.XC.Dept, "选择挂号科室", typeof(Default.Clinic.Component.Register.Views.DeptsView), A.XC.Doctor);
            children.Add(A.XC.Doctor, "选择医生", typeof(Default.Clinic.Component.Register.Views.DoctorView), A.XC.Confirm);
            children.Add(A.XC.Confirm, "选择支付方式", typeof(Default.Clinic.Component.Views.ConfirmView), A.XC.Print);

            children.Context = A.JiaoFei_Context;
            children.Add(A.JF.Confirm, "确认结算支付", typeof(Default.Clinic.Component.Views.ConfirmView), A.JF.Print);

            children.Context = A.ZhuYuan_Context;
            children.Add(A.ZY.Choice, "个人信息", typeof(Default.Clinic.Component.Auth.Views.ChoiceView), A.ZY.Card);

            children.Context = A.IpRecharge_Context;
            children.Add(A.ZYCZ.RechargeWay, "选择支付方式", typeof(Default.Clinic.Component.ZYRecharge.Views.MethodView), A.ZYCZ.InputAmount);
            return true;


        }

        public override string[] UseConfigPath()
        {
            return new[] {"CurrentResource\\Ningxia.xml"};
        }

        public override void AfterStartup()
        {
            FrameworkConst.HospitalName = "宁夏回族自治区中医医院";
            var manager = ServiceLocator.Current.GetInstance<IConfigurationManager>();
            StaticResource.yljgdm= manager.GetValue("His:yljgdm");
            StaticResource.secretid = manager.GetValue("His:secretid");
            if (CurrentStrategyType() == DeviceType.Clinic)
            {
                
                var path = manager.GetValue("Clinic:VideoPath");
                if (!path.IsNullOrWhiteSpace())
                {
                    if ((path?.StartsWith("h") ?? false) || Path.IsPathRooted(path))
                    {
                        VideoPath = path;
                    }
                    else
                    {
                        VideoPath = Path.Combine(FrameworkConst.RootDirectory, manager.GetValue("Clinic:VideoPath"));
                    }
                }
            }
        }

        public override List<Uri> GetResourceDictionaryUris()
        {
            var uris = base.GetResourceDictionaryUris();
            uris.Add(new Uri("pack://application:,,,/YuanTu.NingXiaHospital;component/Theme/DoctorCard.xaml"));
            return uris;
        }
    }
}
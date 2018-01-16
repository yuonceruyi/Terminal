using Microsoft.Practices.ServiceLocation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using YuanTu.Consts;
using YuanTu.Consts.Services;
using YuanTu.Core.Extension;
using YuanTu.Core.Navigating;
using YuanTu.Default.Component.Auth.Views;
using YuanTu.Default.Component.InfoQuery.Views;
using YuanTu.Default.Component.Recharge.Views;
using YuanTu.Default.Component.Register.Views;
using YuanTu.Default.Component.Tools.Views;
using YuanTu.Default.Component.Views;
using YuanTu.Default.Component.ZYRecharge.Views;


namespace YuanTu.WeiHaiZXYY
{
    public class Startup  :YuanTu.Core.FrameworkBase.DefaultStartup
    {
        public override int Order => 90;
        public static string VideoPath { get; set; }
        public override string[] UseConfigPath()
        {
            return null;
        }
        public override void AfterStartup()
        {
            WeiHaiArea.ConfigWeiHai.HighPwd = DateTimeCore.Now.ToString("yyMMdd");
            FrameworkConst.HospitalName = "威海市中心医院";

            if (CurrentStrategyType() == DeviceType.Clinic)
            {
                var manager = ServiceLocator.Current.GetInstance<IConfigurationManager>();
                var path = manager.GetValue("Clinic:VideoPath");
                if (!path.IsNullOrWhiteSpace())
                {
                    VideoPath = Path.Combine(FrameworkConst.RootDirectory, manager.GetValue("Clinic:VideoPath"));
                }
            }
        }
        public override bool RegisterTypes(ViewCollection children)
        {
            children.Add(A.Home, null, "主页", typeof(YuanTu.Default.Component.Views.ChoiceView));
            children.Add(A.AdminPart, null, "后台", typeof(Default.Part.Views.AdminPageView));
            children.Add(A.Maintenance, null, "维护", typeof(YuanTu.Default.Part.Views.MaintenanceView));
            children.Add(A.Third.PosUnion, null, "投币/刷卡", typeof(PosView));
            children.Add(A.Third.Cash, null, "投币/刷卡", typeof(CashView));
            children.Add(A.Third.JCMCash, null, "现金缴住院押金", typeof(JCMView));
            children.Add(A.Third.AtmCash, null, "现金缴住院押金", typeof(HatmView));
            children.Add(A.Third.ScanQrCode, null, "投币/刷卡", typeof(ScanQrCodeView));

            children.Context = A.ChaKa_Context;
            children.Add(A.CK.Select, "个人信息", typeof(SelectTypeView), A.CK.Card);
            //children.Add(A.CK.Choice, "个人信息", typeof(YuanTu.Default.Component.Auth.Views.ChoiceView), A.CK.Card);
            children.Add(A.CK.Card, "个人信息", typeof(CardView), A.CK.Info);
            children.Add(A.CK.IDCard, "个人信息", typeof(IDCardView), A.CK.Info);
            children.Add(A.CK.Info, "个人信息", typeof(Component.Auth.Views.PatientInfoView), A.Home);
            children.Add(A.CK.InfoEx, "个人信息", typeof(Component.Auth.Views.PatientInfoExView), A.Home);

            children.Context = A.JianDang_Context;
            children.Add(A.JD.Print, "建档结果", typeof(PrintView), A.Home);

            children.Context = A.XianChang_Context;
            // children.Add(A.XC.Wether, "选择挂号类别", typeof(RegTypesView), A.XC.Dept);
            children.Add(A.XC.Dept, "选择挂号科室", typeof(DeptsView), A.XC.Doctor);
            children.Add(A.XC.Doctor, "选择医生", typeof(DoctorView), A.XC.Schedule);
            children.Add(A.XC.Schedule, "选择排班", typeof(ScheduleView), A.XC.Confirm);
            children.Add(A.XC.Confirm, "选择支付方式", typeof(ConfirmView), A.XC.Print);
            children.Add(Guid.NewGuid().ToString(), "投币/刷卡", null, A.XC.Print);
            children.Add(A.XC.Print, "挂号凭条", typeof(PrintView), A.Home);


            children.Context = A.ChongZhi_Context;
            children.Add(A.CZ.RechargeWay, "选择充值方式", typeof(RechargeMethodView), A.CZ.InputAmount);
            children.Add(A.CZ.InputAmount, "输入充值金额", typeof(InputAmountView), A.CZ.Print);
            children.Add(Guid.NewGuid().ToString(), "投币/刷卡", null, A.CZ.Print);
            children.Add(A.CZ.Print, "充值完成", typeof(PrintView), A.Home);

            children.Add(A.QueryChoice, null, "查询选择", typeof(QueryChoiceView));

            children.Context = A.PayCostQuery;
            children.Add(A.JFJL.Date, "选择查询日期", typeof(DateTimeView), A.JFJL.PayCostRecord);
            children.Add(A.JFJL.PayCostRecord, "已缴费信息", typeof(Component.InfoQuery.Views.PayCostRecordView), A.Home);

            children.Context = A.MedicineQuery;
            children.Add(A.YP.Query, "输入查询条件", typeof(InputView), A.YP.Medicine);
            children.Add(A.YP.Medicine, "药品信息列表", typeof(YuanTu.WeiHaiZXYY.Component.InfoQuery.Views.MedicineItemsView), A.Home);

            children.Context = A.ChargeItemsQuery;
            children.Add(A.XM.Query, "输入查询条件", typeof(InputView), A.XM.ChargeItems);
            children.Add(A.XM.ChargeItems, "诊疗项列表", typeof(YuanTu.WeiHaiZXYY.Component.InfoQuery.Views.ChargeItemsView), A.Home);

            children.Context = A.DiagReportQuery;
            children.Add(A.JYJL.DiagReport, "检验报告信息", typeof(DiagReportView), A.Home);

            children.Context = A.ZhuYuan_Context;
            children.Add(A.ZY.InPatientNo, "住院患者信息", typeof(Component.Auth.Views.InPatientNoCardView), A.ZY.InPatientInfo);
            children.Add(A.ZY.InPatientInfo, "住院患者信息", typeof(InPatientInfoView), A.Home);

            children.Context = A.InDayDetailList_Context;
            children.Add(A.ZYYRQD.Date, "选择查询日期", typeof(InDailyDateView), A.ZYYRQD.DailyDetail);
            children.Add(A.ZYYRQD.DailyDetail, "住院一日清单", typeof(InDailyDetailView), A.Home);


            children.Context = A.IpRecharge_Context;
            children.Add(A.ZYCZ.RechargeWay, "选择支付方式", typeof(MethodView), A.ZYCZ.InputAmount);
            children.Add(A.ZYCZ.InputAmount, "输入缴纳金额", typeof(ZYInputAmountView), A.ZYCZ.Print);
            children.Add(Guid.NewGuid().ToString(), "投币/刷卡", null, A.ZYCZ.Print);
            children.Add(A.ZYCZ.Print, "缴押金完成", typeof(PrintView), A.Home);

            children.Context = AInner.BindLisencePlate;
            children.Add(A.CK.Card, "绑定车牌", typeof(CardView), A.CK.Info);

            if (CurrentStrategyType() == DeviceType.Clinic)
                RegisterClinicTypes(children);
            return true;
        }

        private bool RegisterClinicTypes(ViewCollection children)
        {
            children.Add(A.Home, null, "主页", typeof(Default.Clinic.Component.Views.ChoiceView));
            children.Add(A.Third.Cash, null, "投币", typeof(Default.Clinic.Component.Recharge.Views.CashView));

            children.Context = A.XianChang_Context;
            children.Add(A.XC.Dept, "选择挂号科室", typeof(Default.Clinic.Component.Register.Views.DeptsView), A.XC.Doctor);
            children.Add(A.XC.Doctor, "选择医生", typeof(Default.Clinic.Component.Register.Views.DoctorView), A.XC.Schedule);
            children.Add(A.XC.Schedule, "选择排班", typeof(Default.Clinic.Component.Register.Views.ScheduleView), A.XC.Confirm);
            children.Add(A.XC.Confirm, "选择支付方式", typeof(Default.Clinic.Component.Views.ConfirmView), A.XC.Print);

            children.Context = A.ChongZhi_Context;
            children.Add(A.CZ.RechargeWay, "选择充值方式", typeof(Default.Clinic.Component.Recharge.Views.RechargeMethodView), A.CZ.InputAmount);

            children.Add(A.QueryChoice, null, "查询选择", typeof(YuanTu.Default.Clinic.Component.InfoQuery.Views.QueryChoiceView));

            //children.Add(A.JFJL.PayCostRecord, "已缴费信息", typeof(YuanTu.Default.Clinic.Component.InfoQuery.Views.PayCostRecordView), A.Home);
            children.Context = A.MedicineQuery;
            children.Add(A.YP.Query, "输入查询条件", typeof(YuanTu.Default.Clinic.Component.InfoQuery.Views.InputView), A.YP.Medicine);

            children.Context = A.ChargeItemsQuery;
            children.Add(A.XM.Query, "输入查询条件", typeof(YuanTu.Default.Clinic.Component.InfoQuery.Views.InputView), A.XM.ChargeItems);

            children.Context = A.ZhuYuan_Context;
            children.Add(A.ZYCZ.RechargeWay, "选择支付方式", typeof(YuanTu.Default.Clinic.Component.ZYRecharge.Views.MethodView), A.ZYCZ.InputAmount);
            return true;
        }

        public override List<Uri> GetResourceDictionaryUris()
        {
            var uris = base.GetResourceDictionaryUris();
            uris.Add(new Uri("pack://application:,,,/YuanTu.WeiHaiZXYY;component/Theme/DoctorCard.xaml"));
            return uris;
        }
    }
}
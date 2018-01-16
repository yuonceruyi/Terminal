using YuanTu.Consts;
using YuanTu.Core.Navigating;
using YuanTu.Default.House;
using YuanTu.Default.House.Common;
using YuanTu.Default.House.Component.Auth.Views;
using YuanTu.Default.House.Component.Create.Views;
using YuanTu.Default.House.Component.HealthDetection.Views;
using YuanTu.Default.House.Component.InfoQuery.Views;
using YuanTu.Default.House.Component.Register.Views;
using YuanTu.Default.House.Component.Views;
using YuanTu.Default.House.Part.Views;
using YuanTu.QingDao.House.Component.Register.Views;

namespace YuanTu.QingDao.House
{
    public class Startup : Default.House.Startup
    {
        public override bool RegisterTypes(ViewCollection children)
        {
            children.Add(A.Home, null, "主页", typeof(ChoiceView));
            children.Add(AInner.ScreenSaver, null, "屏保", typeof(ScreenSaverView));
            children.Add(A.AdminPart, null, "后台", typeof(AdminPageView));
            children.Add(A.Maintenance, null, "维护", typeof(MaintenanceView));

            children.Context = A.ChaKa_Context;
            children.Add(A.CK.Card, "登录界面", typeof(LoginView), A.CK.Info);
            children.Add(A.CK.Info, "个人信息", typeof(PatientInfoView), A.Home);

            children.Context = AInner.Health_Context;
            GetViewConfig();
            HealthDetectCount = ViewContexts.ViewContextList.Count;
            for (var len = 0; len < ViewContexts.ViewContextList.Count; len++)
                children.Add(ViewContexts.ViewContextList[len].Address, ViewContexts.ViewContextList[len].Title,
                    ViewContexts.ViewContextList[len].Type,
                    len < ViewContexts.ViewContextList.Count - 1
                        ? ViewContexts.ViewContextList[len + 1].Address
                        : AInner.Health.Report);

            children.Add(AInner.Health.Report, "健康服务测量结果", typeof(ReportView), AInner.Health.ReportPreview);
            children.Add(AInner.Health.ReportPreview, "健康服务测量结果打印预览", typeof(ReportPreviewView), A.Home);

            children.Context = A.XianChang_Context;
            children.AddWithIcon(AInner.XC.ChoiceHospital, "选择挂号医院", typeof(HospitalsView), A.XC.Dept, "选择预约医院");
            children.AddWithIcon(A.XC.Dept, "选择挂号科室", typeof(DeptsView), A.XC.Schedule, "选择预约科室");
            children.AddWithIcon(A.XC.Schedule, "选择挂号排班", typeof(ScheduleView), A.XC.Confirm, "选择预约排班");
            children.AddWithIcon(A.XC.Confirm, "选择支付方式", typeof(ConfirmView), A.XC.Print, "确认预约信息");
            children.AddWithIcon(A.XC.Print, "打印挂号凭条", typeof(PrintView), A.Home, "打印预约凭条");

            children.Context = A.YuYue_Context;
            children.Add(AInner.YY.ChoiceHospital, "选择预约医院", typeof(HospitalsView), A.YY.Dept);
            children.Add(A.YY.Dept, "选择预约科室", typeof(DeptsView), A.YY.Schedule);
            children.Add(A.YY.Schedule, "选择预约排班", typeof(ScheduleView), A.YY.Time);
            children.Add(A.YY.Time, "选择预约号源", typeof(SourceView), A.YY.Confirm);
            children.Add(A.YY.Confirm, "确认预约信息", typeof(ConfirmView), A.YY.Print);
            children.Add(A.YY.Print, "打印预约凭条", typeof(PrintView), A.Home);

            children.Context = AInner.Query_Context;
            children.Add(AInner.Query.DateTimeView, "时间选择页面", typeof(DateTimeView), AInner.Query.QueryView);
            children.Add(AInner.Query.QueryView, "查询结果页面", typeof(QueryView), AInner.Query.ReportPreview);
            children.Add(AInner.Query.ReportPreview, "健康服务测量结果打印预览", typeof(ReportPreviewView), A.Home);

            children.Context = AInner.Create_Context;
            children.Add(AInner.JD.SelectType, "选择发卡类型", typeof(SelectTypeView), AInner.JD.IdCard);
            children.Add(AInner.JD.IdCard, "刷身份证", typeof(IdCardView), AInner.JD.PatInfo);
            children.Add(AInner.JD.PatInfo, "输入手机号", typeof(PatientInfoView), AInner.JD.Print);
            children.Add(AInner.JD.PatInfoEx, "儿童信息收集", typeof(PatientInfoExView), AInner.JD.Print);
            children.Add(AInner.JD.Print, "发卡打印", typeof(PrintView), A.Home);
            return true;
        }
    }
}
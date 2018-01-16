using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using YuanTu.Consts;
using YuanTu.Consts.Enums;
using YuanTu.Core.FrameworkBase;
using YuanTu.Core.Infrastructure;
using YuanTu.Core.Navigating;
using YuanTu.Default.House.Common;
using YuanTu.Default.House.Component.Auth.Views;
using YuanTu.Default.House.Component.HealthDetection.Views;
using YuanTu.Default.House.Component.Views;
using YuanTu.Default.House.Part.Views;
using YuanTu.Core.Extension;
using YuanTu.Default.House.HealthManager;

namespace YuanTu.Default.House
{
    public class Startup : DefaultStartup
    {
        /// <summary>
        /// 优先级，数值越小优先级越高，内部配置越优先被使用
        /// </summary>
        public override int Order => int.MaxValue - 1;

        public static int HealthDetectCount { get; set; }

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
            {
                children.Add(ViewContexts.ViewContextList[len].Address, ViewContexts.ViewContextList[len].Title,
                    ViewContexts.ViewContextList[len].Type,
                    len < ViewContexts.ViewContextList.Count - 1
                        ? ViewContexts.ViewContextList[len + 1].Address
                        : AInner.Health.Report);
            }
            //children.Add(AInner.Health.HeightWeight, "身高体重测量", typeof(HeightWeightView), AInner.Health.Fat);
            //children.Add(AInner.Health.Fat, "体脂测量", typeof(FatView), AInner.Health.BloodPressure);
            //children.Add(AInner.Health.BloodPressure, "血压测量", typeof(BloodPressureView), AInner.Health.SpO2);
            //children.Add(AInner.Health.SpO2, "血氧测量", typeof(SpO2View), AInner.Health.Report);
            children.Add(AInner.Health.Report, "健康服务测量结果", typeof(ReportView), AInner.Health.ReportPreview);
            children.Add(AInner.Health.ReportPreview, "健康服务测量结果打印预览", typeof(ReportPreviewView), A.Home);

            children.Context = A.YuYue_Context;
            children.Add(AInner.YY.ChoiceHospital, "选择预约医院", typeof(Component.Register.Views.ChoiceHospitalView), A.YY.Wether);
            children.Add(A.YY.Wether, "选择预约类型", typeof(Component.Register.Views.RegTypesView), A.YY.Dept);
            children.Add(A.YY.Dept, "选择预约科室", typeof(Component.Register.Views.DeptsView), A.YY.Doctor);
            children.Add(A.YY.Doctor, "选择预约医生", typeof(Component.Register.Views.DoctorView), A.YY.Schedule);
            children.Add(A.YY.Schedule, "选择预约排班", typeof(Component.Register.Views.ScheduleView), A.YY.Time);
            children.Add(A.YY.Time, "选择预约号源", typeof(Component.Register.Views.SourceView), A.YY.Confirm);
            children.Add(A.YY.Confirm, "确认预约信息", typeof(ConfirmView), A.YY.Print);
            children.Add(A.YY.Print, "打印预约凭条", typeof(PrintView), A.Home);

            children.Context = AInner.Query_Context;
            children.Add(AInner.Query.DateTimeView, "时间选择页面", typeof(Component.InfoQuery.Views.DateTimeView), AInner.Query.QueryView);
            children.Add(AInner.Query.QueryView, "查询结果页面", typeof(Component.InfoQuery.Views.QueryView), AInner.Query.ReportPreview);
            children.Add(AInner.Query.ReportPreview, "健康服务测量结果打印预览", typeof(ReportPreviewView), A.Home);

            children.Context = AInner.Create_Context;
            children.Add(AInner.JD.SelectType, "选择发卡类型", typeof(Component.Create.Views.SelectTypeView), AInner.JD.IdCard);
            children.Add(AInner.JD.IdCard, "刷身份证", typeof(Component.Create.Views.IdCardView), AInner.JD.PatInfo);
            children.Add(AInner.JD.PatInfo, "输入手机号", typeof(Component.Auth.Views.PatientInfoView), AInner.JD.Print);
            children.Add(AInner.JD.PatInfoEx, "儿童信息收集", typeof(Component.Create.Views.PatientInfoExView), AInner.JD.Print);
            children.Add(AInner.JD.Print, "发卡打印", typeof(PrintView), A.Home);
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
            return new[] { "Config.Debug.json" };
        }

        public override void AfterStartup()
        {
            FrameworkConst.HospitalName = "健康自助终端";
            Task.Factory.StartNew(() =>
            {
                var res = HealthDataHandlerEx.获取服务器当前时间(new req获取服务器当前时间());
                if (res.success)
                {
                    DateTimeCore.Now = new DateTime(1970, 1, 1, 8, 0, 0, DateTimeKind.Local).AddMilliseconds(res.data);
                }
            });
        }

        public override List<Uri> GetResourceDictionaryUris()
        {
            if (CurrentStrategyType() == DeviceType.House)
            {
                return new List<Uri>
                {
                    new Uri(Theme.HouseDefault.GetEnumDescription())
                };
            }
            return new List<Uri>();
        }

        protected virtual void GetViewConfig()
        {
            if (GetIntValue("HealthDetection:HeightWeight") == 1)
            {
                ViewContexts.ViewContextList.Add(new ViewContexts.ViewContext
                {
                    Address = AInner.Health.HeightWeight,
                    Title = "身高体重测量",
                    Type = typeof(HeightWeightView)
                });
            }
            if (GetIntValue("HealthDetection:HeightWeight") == 1 && GetIntValue("HealthDetection:Fat") == 1)
            {
                ViewContexts.ViewContextList.Add(new ViewContexts.ViewContext
                {
                    Address = AInner.Health.Fat,
                    Title = "体脂测量",
                    Type = typeof(FatView)
                });
            }
            if (GetIntValue("HealthDetection:BloodPressure") == 1)
            {
                ViewContexts.ViewContextList.Add(new ViewContexts.ViewContext
                {
                    Address = AInner.Health.BloodPressure,
                    Title = "血压测量",
                    Type = typeof(BloodPressureView)
                });
            }
            if (GetIntValue("HealthDetection:Temperature") == 1)
            {
                ViewContexts.ViewContextList.Add(new ViewContexts.ViewContext
                {
                    Address = AInner.Health.Temperature,
                    Title = "体温测量",
                    Type = typeof(TemperatureView)
                });
            }
            if (GetIntValue("HealthDetection:ECG") == 1)
            {
                ViewContexts.ViewContextList.Add(new ViewContexts.ViewContext
                {
                    Address = AInner.Health.Ecg,
                    Title = "心电测量",
                    Type = typeof(EcgView)
                });
            }
            if (GetIntValue("HealthDetection:SpO2") == 1)
            {
                ViewContexts.ViewContextList.Add(new ViewContexts.ViewContext
                {
                    Address = AInner.Health.SpO2,
                    Title = "血氧测量",
                    Type = typeof(SpO2View)
                });
            }
        }

        protected virtual int GetIntValue(string key)
        {
            return int.Parse(SystemStartup.Configuration.GetSection(key).Value);
        }
    }
}
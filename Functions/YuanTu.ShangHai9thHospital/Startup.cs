using System;
using System.Collections.Generic;
using YuanTu.Consts;
using YuanTu.Core.Navigating;

namespace YuanTu.ShangHai9thHospital
{
    public class Startup : Default.Clinic.Startup
    {
        public override bool RegisterTypes(ViewCollection children)
        {
            base.RegisterTypes(children);

            children.Context = null;
            children.Add(A.Home, null, "主页", typeof(Clinic.Component.Views.ChoiceView));

            children.Context = A.ChaKa_Context;
            children.Add(A.CK.Choice, "个人信息", typeof(Clinic.Component.Auth.Views.ChoiceView), A.CK.Card);

            children.Context = A.XianChang_Context;
            children.Add(A.XC.Wether, "选择挂号类别", typeof(Clinic.Component.Register.Views.RegTypesView), A.XC.Dept);
            children.Add(A.XC.Dept, "选择挂号科室", typeof(Clinic.Component.Register.Views.DeptsView), A.XC.Doctor);
            children.Add(A.XC.Doctor, "选择医生排班", typeof(Clinic.Component.Register.Views.DoctorView), A.XC.Schedule);
            children.Add(A.XC.Schedule, "选择医生排班", typeof(Clinic.Component.Register.Views.ScheduleView), A.XC.Confirm);

            children.Context = A.YuYue_Context;
            children.Add(A.YY.Date, "选择预约日期", typeof(Clinic.Component.Register.Views.RegDateView), A.YY.Wether);
            children.Add(A.YY.Wether, "选择预约类别", typeof(Clinic.Component.Register.Views.RegTypesView), A.YY.Dept);
            children.Add(A.YY.Dept, "选择预约科室", typeof(Clinic.Component.Register.Views.DeptsView), A.YY.Doctor);
            children.Add(A.YY.Doctor, "选择医生排班", typeof(Clinic.Component.Register.Views.DoctorView), A.YY.Schedule);
            children.Add(A.YY.Schedule, "选择医生排班", typeof(Clinic.Component.Register.Views.ScheduleView), A.YY.Confirm);
            children.Add(A.YY.Time, "选择医生排班", typeof(Clinic.Component.Register.Views.SourceView), A.YY.Confirm);

            return true;
        }

        public override List<Uri> GetResourceDictionaryUris()
        {
            var uris = base.GetResourceDictionaryUris();
            uris.Add(new Uri("pack://application:,,,/YuanTu.ShangHai9thHospital;component/Theme/Default.xaml"));
            return uris;
        }

        public override string[] UseConfigPath()
        {
            return new[] { "CurrentResource\\YuanTu.ShangHai9thHospital\\上海市第九人民医院.xml" };
        }
    }
}

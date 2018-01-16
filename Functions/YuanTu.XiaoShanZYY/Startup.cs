using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Practices.ServiceLocation;
using YuanTu.Consts;
using YuanTu.Consts.Services;
using YuanTu.Core.Navigating;
using YuanTu.Core.Reporter;
using YuanTu.XiaoShanZYY.HIS;

namespace YuanTu.XiaoShanZYY
{
    public class Startup : YuanTu.Default.Startup
    {
        public override bool RegisterTypes(ViewCollection children)
        {
            children.Add(A.Home, null, "主页",
                typeof(YuanTu.Default.Component.Views.ChoiceView));
            children.Add(A.AdminPart, null, "后台",
                typeof(YuanTu.Default.Part.Views.AdminPageView));
            children.Add(A.Maintenance, null, "维护",
                typeof(YuanTu.Default.Part.Views.MaintenanceView));
            children.Add(A.Third.PosUnion, null, "刷卡",
                typeof(YuanTu.Default.Component.Tools.Views.PosView));
            children.Add(A.Third.Cash, null, "投币",
                typeof(YuanTu.Default.Component.Recharge.Views.CashView));
            children.Add(A.Third.ScanQrCode, null, "扫码",
                typeof(YuanTu.Default.Component.Tools.Views.ScanQrCodeView));

            children.Context = A.ChaKa_Context;
            children.Add(A.CK.Select, "个人信息",
                typeof(YuanTu.Default.Component.Auth.Views.SelectTypeView), A.CK.Choice);
            children.Add(A.CK.Choice, "个人信息",
                typeof(YuanTu.Default.Component.Auth.Views.ChoiceView), A.CK.Card);
            children.Add(A.CK.Card, "个人信息",
                typeof(YuanTu.Default.Component.Auth.Views.CardView), A.CK.Info);
            children.Add(A.CK.IDCard, "个人信息",
                typeof(YuanTu.Default.Component.Auth.Views.IDCardView), A.CK.Info);
            children.Add(A.CK.HICard, "个人信息",
                typeof(YuanTu.Default.Component.Auth.Views.SiCardView), A.CK.Info);
            children.Add(A.CK.QrCode, "个人信息",
                typeof(YuanTu.Default.Component.Auth.Views.QrCodeView), A.CK.Info);
            children.Add(A.CK.Info, "个人信息",
                typeof(YuanTu.XiaoShanZYY.Component.Auth.Views.PatientInfoView), A.Home);
            children.Add(A.CK.InfoEx, "个人信息",
                typeof(YuanTu.Default.Component.Auth.Views.PatientInfoExView), A.Home);

            children.Context = A.XianChang_Context;
            children.Add(A.XC.Wether, "选择挂号类别",
                typeof(YuanTu.Default.Component.Register.Views.RegTypesView), A.XC.AMPM);
            children.Add(A.XC.AMPM, "选择挂号时段",
                typeof(YuanTu.Default.Component.Register.Views.RegAmPmView), A.XC.Dept);
            children.Add(A.XC.Dept, "选择挂号科室",
                typeof(YuanTu.Default.Component.Register.Views.DeptsView), A.XC.Schedule);
            children.Add(A.XC.Schedule, "选择挂号排班",
                typeof(YuanTu.Default.Component.Register.Views.ScheduleView), A.XC.Doctor);
            children.Add(A.XC.Doctor, "选择挂号医生",
                typeof(YuanTu.XiaoShanZYY.Component.Register.Views.DoctorView), A.XC.Confirm);
            children.Add(A.XC.Confirm, "选择支付方式",
                typeof(YuanTu.Default.Component.Views.ConfirmView), A.XC.Print);
            children.Add(A.XC.Print, "挂号凭条",
                typeof(YuanTu.Default.Component.Views.PrintView), A.Home);

            children.Context = A.YuYue_Context;
            children.Add(A.YY.Wether, "选择预约类别",
                typeof(YuanTu.Default.Component.Register.Views.RegTypesView), A.YY.Dept);
            children.Add(A.YY.Dept, "选择预约科室",
                typeof(YuanTu.Default.Component.Register.Views.DeptsView), A.YY.Schedule);
            children.Add(A.YY.Schedule, "选择预约排班",
                typeof(YuanTu.Default.Component.Register.Views.ScheduleView), A.YY.Doctor);
            children.Add(A.YY.Doctor, "选择预约医生",
                typeof(YuanTu.XiaoShanZYY.Component.Register.Views.DoctorView), A.YY.Date);
            children.Add(A.YY.Date, "选择预约日期",
                typeof(YuanTu.Default.Component.Register.Views.RegDateView), A.YY.Time);
            children.Add(A.YY.Time, "选择排班分时",
                typeof(YuanTu.Default.Component.Register.Views.SourceView), A.YY.Confirm);
            children.Add(A.YY.Confirm, "确认预约信息",
                typeof(YuanTu.Default.Component.Views.ConfirmView), A.YY.Print);
            children.Add(A.YY.Print, "预约凭条",
                typeof(YuanTu.Default.Component.Views.PrintView), A.Home);

            children.Context = A.QuHao_Context;
            children.Add(A.QH.Query, "输入取号密码",
                typeof(YuanTu.XiaoShanZYY.Component.TakeNum.Views.QueryView), A.QH.TakeNum);
            children.Add(A.QH.TakeNum, "确认取号信息",
                typeof(YuanTu.Default.Component.TakeNum.Views.TakeNumView), A.QH.Confirm);
            children.Add(A.QH.Confirm, "确认取号支付",
                typeof(YuanTu.Default.Component.Views.ConfirmView), A.QH.Print);
            children.Add(A.QH.Print, "取号凭条",
                typeof(YuanTu.Default.Component.Views.PrintView), A.Home);

            children.Context = A.JiaoFei_Context;
            children.Add(A.JF.BillRecord, "待缴费信息",
                typeof(YuanTu.XiaoShanZYY.Component.BillPay.Views.BillRecordView), A.JF.Confirm);
            children.Add(A.JF.Confirm, "确认结算支付",
                typeof(YuanTu.Default.Component.Views.ConfirmView), A.JF.Print);
            children.Add(A.JF.Print, "缴费完成",
                typeof(YuanTu.Default.Component.Views.PrintView), A.Home);

            children.Context = A.ChongZhi_Context;
            children.Add(A.CZ.RechargeWay, "选择充值方式",
                typeof(YuanTu.Default.Component.Recharge.Views.RechargeMethodView), A.CZ.InputAmount);
            children.Add(A.CZ.InputAmount, "输入充值金额",
                typeof(YuanTu.Default.Component.Recharge.Views.InputAmountView), A.CZ.Print);
            children.Add(A.CZ.Print, "充值完成",
                typeof(YuanTu.Default.Component.Views.PrintView), A.Home);

            children.Add(A.QueryChoice, null, "查询选择",
                typeof(YuanTu.Default.Component.InfoQuery.Views.QueryChoiceView));

            children.Context = A.PayCostQuery;
            children.Add(A.JFJL.Date, "选择查询日期",
                typeof(YuanTu.Default.Component.InfoQuery.Views.DateTimeView), A.JFJL.PayCostRecord);
            children.Add(A.JFJL.PayCostRecord, "已缴费信息",
                typeof(YuanTu.XiaoShanZYY.Component.InfoQuery.Views.PayCostRecordView), A.Home);

            children.Context = A.MedicineQuery;
            children.Add(A.YP.Query, "输入查询条件",
                typeof(YuanTu.Default.Component.InfoQuery.Views.InputView), A.YP.Medicine);
            children.Add(A.YP.Medicine, "药品信息列表",
                typeof(YuanTu.Default.Component.InfoQuery.Views.MedicineItemsView), A.Home);

            children.Context = A.ChargeItemsQuery;
            children.Add(A.XM.Query, "输入查询条件",
                typeof(YuanTu.Default.Component.InfoQuery.Views.InputView), A.XM.ChargeItems);
            children.Add(A.XM.ChargeItems, "诊疗项列表",
                typeof(YuanTu.Default.Component.InfoQuery.Views.ChargeItemsView), A.Home);

            children.Context = A.DiagReportQuery;
            children.Add(A.JYJL.Date, "选择查询日期",
                typeof(YuanTu.Default.Component.InfoQuery.Views.DateTimeView), A.JYJL.DiagReport);
            children.Add(A.JYJL.DiagReport, "检验报告信息",
                typeof(YuanTu.Default.Component.InfoQuery.Views.DiagReportView), A.Home);

            children.Context = A.PacsReportQuery;
            children.Add(A.YXBG.Date, "选择查询日期",
                typeof(YuanTu.Default.Component.InfoQuery.Views.DateTimeView), A.YXBG.PacsReport);
            children.Add(A.YXBG.PacsReport, "影像报告信息",
                typeof(YuanTu.Default.Component.InfoQuery.Views.PacsReportView), A.Home);

            children.Context = A.ReChargeQuery;
            children.Add(A.CZJL.Date, "选择查询日期",
                typeof(YuanTu.Default.Component.InfoQuery.Views.DateTimeView), A.CZJL.ReChargeRecord);
            children.Add(A.CZJL.ReChargeRecord, "预交金记录信息",
                typeof(YuanTu.Default.Component.InfoQuery.Views.RechargeRecordView), A.Home);

            children.Context = A.BuDa_Context;
            children.Add(A.JFBD.PayCostRecord, "选择补打记录", typeof(YuanTu.XiaoShanZYY.Component.PrintAgain.Views.ChoiceView), A.JFBD.Print);
            children.Add(A.JFBD.Print, "补打完成", typeof(YuanTu.Default.Component.Views.PrintView), A.Home);

            if (CurrentStrategyType() == DeviceType.Clinic)
                RegisterTypesClinic(children);
            return true;
        }
        public bool RegisterTypesClinic(ViewCollection children)
        {
            children.Add(A.Home, null, "主页",
                typeof(YuanTu.Default.Clinic.Component.Views.ChoiceView));
            children.Add(A.AdminPart, null, "后台",
                typeof(YuanTu.Default.Part.Views.AdminPageView));
            children.Add(A.Maintenance, null, "维护",
                typeof(YuanTu.Default.Part.Views.MaintenanceView));
            children.Add(A.Third.Cash, null, "投币",
                typeof(YuanTu.Default.Clinic.Component.Recharge.Views.CashView));

            children.Context = A.ChaKa_Context;
            children.Add(A.CK.Choice, "个人信息",
                typeof(YuanTu.Default.Clinic.Component.Auth.Views.ChoiceView), A.CK.Card);
            children.Add(A.CK.Card, "个人信息",
                typeof(YuanTu.Default.Clinic.Component.Auth.Views.CardView), A.CK.Info);

            children.Context = A.XianChang_Context;
            children.Add(A.XC.Wether, "选择挂号类别",
                typeof(YuanTu.Default.Clinic.Component.Register.Views.RegTypesView), A.XC.AMPM);
            children.Add(A.XC.AMPM, "选择挂号时段",
                typeof(YuanTu.Default.Clinic.Component.Register.Views.RegAmPmView), A.XC.Dept);
            children.Add(A.XC.Dept, "选择挂号科室",
                typeof(YuanTu.Default.Clinic.Component.Register.Views.DeptsView), A.XC.Schedule);
            children.Add(A.XC.Schedule, "选择挂号排班",
                typeof(YuanTu.Default.Clinic.Component.Register.Views.ScheduleView), A.XC.Doctor);
            children.Add(A.XC.Doctor, "选择挂号医生",
                typeof(YuanTu.XiaoShanZYY.Component.Register.Views.DoctorView), A.XC.Confirm);
            children.Add(A.XC.Confirm, "选择支付方式",
                typeof(YuanTu.Default.Clinic.Component.Views.ConfirmView), A.XC.Print);

            children.Context = A.YuYue_Context;
            children.Add(A.YY.Wether, "选择预约类别",
                typeof(YuanTu.Default.Clinic.Component.Register.Views.RegTypesView), A.YY.Dept);
            children.Add(A.YY.Dept, "选择预约科室",
                typeof(YuanTu.Default.Clinic.Component.Register.Views.DeptsView), A.YY.Schedule);
            children.Add(A.YY.Schedule, "选择预约排班",
                typeof(YuanTu.Default.Clinic.Component.Register.Views.ScheduleView), A.YY.Doctor);
            children.Add(A.YY.Doctor, "选择预约医生",
                typeof(YuanTu.XiaoShanZYY.Component.Register.Views.DoctorView), A.YY.Date);
            children.Add(A.YY.Date, "选择预约日期",
                typeof(YuanTu.Default.Clinic.Component.Register.Views.RegDateView), A.YY.Time);
            children.Add(A.YY.Time, "选择排班分时",
                typeof(YuanTu.Default.Clinic.Component.Register.Views.SourceView), A.YY.Confirm);
            children.Add(A.YY.Confirm, "确认预约信息",
                typeof(YuanTu.Default.Clinic.Component.Views.ConfirmView), A.YY.Print);

            children.Context = A.QuHao_Context;
            children.Add(A.QH.Query, "输入取号密码",
               typeof(YuanTu.XiaoShanZYY.Component.TakeNum.Views.QueryView), A.QH.TakeNum);
            children.Add(A.QH.TakeNum, "确认取号信息",
                typeof(YuanTu.Default.Component.TakeNum.Views.TakeNumView), A.QH.Confirm);
            children.Add(A.QH.Confirm, "确认取号支付",
                typeof(YuanTu.Default.Component.Views.ConfirmView), A.QH.Print);
            children.Add(A.QH.Print, "取号凭条",
                typeof(YuanTu.Default.Component.Views.PrintView), A.Home);

            children.Context = A.JiaoFei_Context;
            children.Add(A.JF.BillRecord, "待缴费信息",
                typeof(YuanTu.XiaoShanZYY.Component.BillPay.Views.BillRecordView), A.JF.Confirm);
            children.Add(A.JF.Confirm, "确认结算支付",
                typeof(YuanTu.Default.Clinic.Component.Views.ConfirmView), A.JF.Print);

            children.Context = A.ChongZhi_Context;
            children.Add(A.CZ.RechargeWay, "选择充值方式",
                typeof(YuanTu.Default.Clinic.Component.Recharge.Views.RechargeMethodView), A.CZ.InputAmount);

            children.Add(A.QueryChoice, null, "查询选择",
                typeof(YuanTu.Default.Clinic.Component.InfoQuery.Views.QueryChoiceView));

            children.Context = A.PayCostQuery;
            children.Add(A.JFJL.PayCostRecord, "已缴费信息",
                typeof(YuanTu.XiaoShanZYY.Component.InfoQuery.Views.PayCostRecordView), A.Home);

            children.Context = A.MedicineQuery;
            children.Add(A.YP.Query, "输入查询条件",
                typeof(YuanTu.Default.Clinic.Component.InfoQuery.Views.InputView), A.YP.Medicine);

            children.Context = A.ChargeItemsQuery;
            children.Add(A.XM.Query, "输入查询条件",
                typeof(YuanTu.Default.Clinic.Component.InfoQuery.Views.InputView), A.XM.ChargeItems);

            return true;
        }

        public static string HISAddress { get; set; }
            //= "http://192.168.0.107/bs_hisservice/hissvr.asmx";
            = "http://ngrok.yuantutech.com:12002/bs_hisservice/hissvr.asmx";
        public static hissvrSoapClient HisService { get; set; }
        public static bool SMK { get; set; }
        public static bool HISDll { get; set; }
        public static bool HISWebService { get; set; }

        public override void AfterStartup()
        {
            //ReporterDataHandler.Disabled = true;

            var config = ServiceLocator.Current.GetInstance<IConfigurationManager>();

            var hisAddress = config.GetValue("萧山中医院:HISAddress");
            if (!string.IsNullOrEmpty(hisAddress))
                HISAddress = hisAddress;
            HisService = new hissvrSoapClient(
                new BasicHttpBinding(),
                new EndpointAddress(HISAddress)
            );

            RunExeHelper.ExePath = config.GetValue("萧山中医院:ExePath");

            SMK = Environment.GetCommandLineArgs().Any(one => one == "SMK");
            HISDll = Environment.GetCommandLineArgs().Any(one => one == "HISDll");
            HISWebService = Environment.GetCommandLineArgs().Any(one => one == "HISWebService");

            FrameworkConst.HospitalName = "杭州市萧山区中医院\n浙江中医药大学附属江南医院";
        }

        public override string[] UseConfigPath()
        {
            return new[] { "CurrentResource\\YuanTu.XiaoShanZYY\\萧山中医院.xml" };
        }
    }
}

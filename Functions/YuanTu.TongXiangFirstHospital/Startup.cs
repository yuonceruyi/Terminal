using Microsoft.Practices.ServiceLocation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using YuanTu.Consts;
using YuanTu.Consts.Services;
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
using YuanTu.TongXiangHospitals;
using YuanTu.TongXiangHospitals.HealthInsurance;
using SiCardView = YuanTu.TongXiangHospitals.Component.Auth.Views.SiCardView;
using CardView = YuanTu.TongXiangHospitals.Component.Auth.Views.CardView;

namespace YuanTu.TongXiangFirstHospital
{
    public class Startup : TongXiangHospitals.Startup
    {
        /// <summary>
        /// 优先级，数值越小优先级越高，内部配置越优先被使用
        /// </summary>
        public override int Order => 1;

        public static bool IsDeptWhiteList { get; set; }
        public static string[] DeptWhiteList { get; set; }

        public override void AfterStartup()
        {
            base.AfterStartup();
            FrameworkConst.HospitalName = "桐乡市第一人民医院";
            var config = ServiceLocator.Current.GetInstance<IConfigurationManager>();

            var value = config.GetValue("WhiteList:Depts");
            if (!string.IsNullOrEmpty(value))
            {
                IsDeptWhiteList = true;
                DeptWhiteList = value.Split(new[] {',', '|', ';', ' '}, StringSplitOptions.RemoveEmptyEntries);
            }

            UnSafeMethods.DoNothing();
            //TongXiangHospitals.Startup.TestRefund = config.GetValueInt("TestRefund") == 1;
            //Task.Factory.StartNew(() =>
            //{
            //    CopyFolder(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "External", "TongXiang"), Path.Combine(AppDomain.CurrentDomain.BaseDirectory));
            //});
        }

        public override bool RegisterTypes(ViewCollection children)
        {
            children.Add(A.Home, null, "主页", typeof(YuanTu.Default.Component.Views.ChoiceView));
            children.Add(A.AdminPart, null, "后台", typeof(YuanTu.Default.Part.Views.AdminPageView));
            children.Add(A.Maintenance, null, "维护", typeof(Default.Part.Views.MaintenanceView));
            children.Add(A.Third.PosUnion, null, "刷卡", typeof(PosView));
            children.Add(A.Third.Cash, null, "投币", typeof(CashView));
            children.Add(A.Third.ScanQrCode, null, "扫码", typeof(ScanQrCodeView));

            children.Context = A.ChaKa_Context;
            children.Add(A.CK.Select, "个人信息", typeof(SelectTypeView), A.CK.Choice);
            children.Add(A.CK.Choice, "个人信息", typeof(YuanTu.Default.Component.Auth.Views.ChoiceView), A.CK.Card);
            children.Add(A.CK.Card, "个人信息", typeof(CardView), A.CK.Info);
            children.Add(A.CK.IDCard, "个人信息", typeof(IDCardView), A.CK.Info);
            children.Add(A.CK.HICard, "个人信息", typeof(SiCardView), A.CK.Info);
            children.Add(A.CK.Info, "个人信息", typeof(PatientInfoView), A.Home);
            children.Add(A.CK.InfoEx, "个人信息", typeof(PatientInfoExView), A.Home);

            children.Context = A.JianDang_Context;
            children.Add(AInner.JD.Confirm, "确认支付信息", typeof(ConfirmView), A.JD.Print);
            //children.Add(Guid.NewGuid().ToString(), "投币/刷卡/扫码", null, A.JD.Print);
            children.Add(A.JD.Print, "建档结果", typeof(PrintView), A.Home);

            children.Context = A.XianChang_Context;
            children.Add(A.XC.Wether, "选择挂号类别", typeof(RegTypesView), A.XC.Dept);
            children.Add(A.XC.Dept, "选择挂号科室", typeof(DeptsView), A.XC.Doctor);
            children.Add(A.XC.Doctor, "选择医生排班", typeof(DoctorView), A.XC.Schedule);
            children.Add(A.XC.Schedule, "选择医生排班", typeof(ScheduleView), A.XC.Confirm);
            //children.Add(AInner.XC.PayChoice, "选择支付方式", typeof(TongXiangHospitals.Component.Views.PayChoiceView), A.XC.Confirm);
            children.Add(A.XC.Confirm, "确认挂号信息", typeof(ConfirmView), A.XC.Print);
            //children.Add(Guid.NewGuid().ToString(), "投币/刷卡/扫码", null, A.XC.Print);
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
            //children.Add(AInner.QH.PayChoice, "选择支付方式", typeof(TongXiangHospitals.Component.Views.PayChoiceView), A.QH.Confirm);
            children.Add(A.QH.Confirm, "确认取号支付", typeof(ConfirmView), A.QH.Print);
            children.Add(A.QH.Print, "取号凭条", typeof(PrintView), A.Home);

            children.Context = A.JiaoFei_Context;
            children.Add(A.JF.BillRecord, "待缴费信息", typeof(BillRecordView), A.JF.Confirm);
            //children.Add(AInner.JF.PayChoice, "选择支付方式", typeof(TongXiangHospitals.Component.Views.PayChoiceView), A.JF.Confirm);
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

        /// <summary>
        /// 向主程序提供配置信息，优先采用插件内部配置
        /// </summary>
        /// <returns>
        /// 返回配置文件完整路径(支持xml,json,ini)
        /// </returns>
        public override string[] UseConfigPath()
        {
            var file = Path.Combine(FrameworkConst.RootDirectory, "CurrentResource", FrameworkConst.HospitalId, "TongXiangConfig.xml");
            return new[] { file };
        }

        public static void CopyFolder(string strFromPath, string strToPath)
        {
            if (!Directory.Exists(strFromPath))
            {
                MessageBox.Show("医保文件缺失");
                return;
            }
            var strFiles = Directory.GetFiles(strFromPath);
            if (strFiles.Length == 0)
            {
                MessageBox.Show("医保文件缺失");
                return;
            }
            foreach (var file in strFiles)
            {
                var strFileName = file.Substring(file.LastIndexOf("\\", StringComparison.Ordinal) + 1, file.Length - file.LastIndexOf("\\", StringComparison.Ordinal) - 1);
                File.Copy(file, Path.Combine(strToPath, strFileName), true);
            }
        }

        public override List<Uri> GetResourceDictionaryUris()
        {
            var uris = base.GetResourceDictionaryUris();
            if (uris.Any() && uris.Last().ToString().Contains("YellowBlue"))
                uris.Add(new Uri("pack://application:,,,/YuanTu.TongXiangHospitals;component/Theme/YellowBlue.xaml"));
            return uris;
        }
    }
}
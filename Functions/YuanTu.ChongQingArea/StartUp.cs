using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Windows;
using Microsoft.Practices.ServiceLocation;
using YuanTu.ChongQingArea.Component.Auth.Views;
using YuanTu.ChongQingArea.Component.InfoQuery.Views;
using YuanTu.ChongQingArea.Component.Views;
using YuanTu.ChongQingArea.Component.ZYRecharge;
using YuanTu.ChongQingArea.SiHandler;
using YuanTu.Consts;
using YuanTu.Consts.Services;
using YuanTu.Core.Extension;
using YuanTu.Core.FrameworkBase;
using YuanTu.Core.Log;
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
using InPatientNoView = YuanTu.Default.Component.Auth.Views.InPatientNoView;
using InSiCardView = YuanTu.ChongQingArea.Component.Auth.Views.InSiCardView;
using SiCardView = YuanTu.ChongQingArea.Component.Auth.Views.SiCardView;
using YuanTu.ChongQingArea.Component.InfoQuery.ViewModels;
using YuanTu.Core.Systems;

namespace YuanTu.ChongQingArea
{
    internal class Startup : DefaultStartup
    {
        public static AddressInfo AddressInfo { get; set; }
        public static bool Biometric { get; set; }
        public static int PayCostRecordDay { get; set; }
        public static int PrintSpaceLine { get; set; }
        public static decimal ReSendCost { get; set; }
        public static bool SendCardBySiCard { get; set; }
        public static byte[] SiKey { get; private set; } = new byte[] {0x6E, 0x6F, 0xE8, 0x5C, 0x76, 0xA2};

        public override void AfterStartup()
        {
            string name = "";
            switch (FrameworkConst.HospitalId)
            {
                case "1101":
                    name = "重庆市人民医院";
                    break;
                case "1102":
                    name = "重庆医科大学附属儿童医院";
                    break;
                case "1103":
                    name = "重庆肿瘤医院";
                    break;
                case "1104":
                    name = "重庆大渡口区人民医院";
                    break;
            }
            FrameworkConst.HospitalName = name;

            //ReporterDataHandler.Disabled = true;

            var config = ServiceLocator.Current.GetInstance<IConfigurationManager>();
            Biometric = config.GetValue("Biometric") == "1";
            PayCostRecordDay = config.GetValueInt("PayCostRecordDay", 7);
            SendCardBySiCard = (config.GetValue("SendCardBySiCard") ?? "1") == "1";
            PrintSpaceLine = config.GetValueInt("PrintSpaceLine", 4);
            ReSendCost = config.GetValueInt("ReSendCost", 500);
            //Decompress
            using (var zipArchive =
                new ZipArchive(File.OpenRead(Path.Combine(FrameworkConst.RootDirectory, @"CurrentResource\YuanTu.ChongQingArea\区域信息.zip")), ZipArchiveMode.Read))
            {
                var entry = zipArchive.GetEntry("区域信息.txt");
                using (var stream = entry.Open())
                {
                    using (var sr = new StreamReader(stream))
                    {
                        var s = sr.ReadToEnd();
                        var list = s.ToJsonObject<List<AddressItem>>();
                        AddressInfo = new AddressInfo(list);
                    }
                   
                }
            }
            if (CurrentStrategyType() == DeviceType.Default)
            {
                string AdExeName = config.GetValue("AdExeName");
                if (string.IsNullOrEmpty(AdExeName))
                {
                    AdExeName = "AD2.exe";
                }
                string AdExeArgs = config.GetValue("AdExeArgs");
                if (string.IsNullOrEmpty(AdExeArgs))
                {
                    AdExeArgs = "AD";
                }
                if (File.Exists(AdExeName))
                {
                    System.Diagnostics.Process.Start(AdExeName, AdExeArgs);
                    Application.Current.Exit += new ExitEventHandler(CloseAd);
                }
            }

            //var local = config.GetValue("LOCAL")??"";
            //InnerConsts.MockSocialSecurity= local.Split(new[] {'|', '-', ','}).Any(p => p == "SI");
            //if (NetworkManager.IP == "192.168.1.193")
            //{
            //    SiKey = new byte[] {0xff, 0xff, 0xff, 0xff, 0xff, 0xff,};
            //    Logger.Main.Info("测试环境 切换默认密钥");
            //}

        }

        public void CloseAd(object sender, EventArgs e)
        {
            var l = System.Diagnostics.Process.GetProcessesByName("AD2");
            Logger.Main.Info("关闭上屏"+ l.Length.ToString());
            if (l.Length > 0) { foreach (var x in l) { x.Kill(); } }
            l = System.Diagnostics.Process.GetProcessesByName("AD");
            if (l.Length > 0) { foreach (var x in l) { x.Kill(); } }
        }

        public override string[] UseConfigPath()
        {
            return new[] {@"CurrentResource\YuanTu.ChongQingArea\重庆.xml"};
        }

        public override bool RegisterTypes(ViewCollection children)
        {
            children.Add(A.Home, null, "主页", typeof(YuanTu.Default.Component.Views.ChoiceView));
            children.Add(A.AdminPart, null, "后台", typeof(YuanTu.Default.Part.Views.AdminPageView));
            children.Add(A.Maintenance, null, "维护", typeof(YuanTu.Default.Part.Views.MaintenanceView));
            children.Add(A.Third.PosUnion, null, "刷卡", typeof(YuanTu.ChongQingArea.Component.Tools.Views.PosView));
            children.Add(A.Third.Cash, null, "投币", typeof(YuanTu.ChongQingArea.Component.Recharge.Views.CashView));
            children.Add(A.Third.AtmCash, null, "现金缴住院押金", typeof(YuanTu.ChongQingArea.Component.Recharge.Views.CashView));
            children.Add(A.Third.ScanQrCode, null, "扫码", typeof(Component.Tools.Views.ScanQrCodeView));

            children.Context = A.ChaKa_Context;
            children.Add(A.CK.Select, "个人信息", typeof(SelectTypeView), A.CK.Choice);
            children.Add(A.CK.Choice, "个人信息", typeof(YuanTu.Default.Component.Auth.Views.ChoiceView), A.CK.Card);
            children.Add(A.CK.Card, "个人信息", typeof(YuanTu.ChongQingArea.Component.Auth.Views.CardView), A.CK.Info);
            children.Add(A.CK.IDCard, "个人信息", typeof(IDCardView), A.CK.Info);
            children.Add(A.CK.HICard, "个人信息", typeof(SiCardView), A.CK.Info);
            children.Add(A.CK.Info, "个人信息", typeof(Component.Auth.Views.PatientInfoView), A.Home);
            children.Add(A.CK.InfoEx, "个人信息", typeof(PatientInfoExView), A.Home);
            children.Add(B.CK.InputText, "个人信息", typeof(InputChildrenNameView), A.Home);
            children.Add(B.Bioc.FingerPrintValidation, "个人信息", typeof(FingerPrintValidationView), A.CK.Info);

            children.Context = A.JianDang_Context;
            children.Add(A.JD.Print, "建档结果", typeof(PrintView), A.Home);

            children.Context = A.XianChang_Context;
            children.Add(A.XC.Wether, "选择挂号类别", typeof(RegTypesView), A.XC.Dept);
            children.Add(A.XC.Dept, "选择挂号科室", typeof(DeptsView), A.XC.Doctor);
            children.Add(A.XC.Doctor, "选择医生排班", typeof(DoctorView), A.XC.Schedule);
            children.Add(A.XC.Schedule, "选择医生排班", typeof(ScheduleView), B.XC.SelectSi);

            if (FrameworkConst.HospitalId == "1103")
            {
                children.Add(A.XC.Schedule, "选择医生排班", typeof(ScheduleView), A.XC.Time);
                children.Add(A.XC.Time, "选择排班时段", typeof(SourceView), A.XC.Confirm);
            }

            children.Add(B.XC.SelectSi, "选择是否社保", typeof(SelectSiView), A.XC.Confirm);
            children.Add(B.XC.InsertSiCard, "插入社保卡(社保支付)", typeof(SiCardView), A.XC.Confirm);
            children.Add(A.XC.Confirm, "选择支付方式", typeof(ConfirmView), A.XC.Print);
            children.Add(A.XC.Print, "挂号凭条", typeof(PrintView), A.Home);

            children.Context = A.YuYue_Context;     
            children.Add(A.YY.Date, "选择预约日期", typeof(RegDateView), A.YY.Wether);
            children.Add(A.YY.Wether, "选择预约类别", typeof(RegTypesView), A.YY.Dept);
            children.Add(A.YY.Dept, "选择预约科室", typeof(DeptsView), A.YY.Doctor);
            children.Add(A.YY.Doctor, "选择医生排班", typeof(DoctorView), A.YY.Schedule);
            children.Add(A.YY.Schedule, "选择医生排班", typeof(ScheduleView), A.YY.Time);
            children.Add(A.YY.Time, "选择排班分时", typeof(SourceView), A.YY.Confirm);
            children.Add(A.YY.Confirm, "确认预约信息", typeof(ConfirmView), A.YY.Print);
            children.Add(A.YY.Print, "预约凭条", typeof(PrintView), A.Home);


            children.Context = A.QuHao_Context;
            children.Add(A.QH.Record, "选择预约记录", typeof(ApptRecordView), A.QH.TakeNum);
            children.Add(A.QH.TakeNum, "确认取号信息", typeof(TakeNumView), B.QH.SelectSi);
            children.Add(B.QH.SelectSi, "选择是否社保", typeof(SelectSiView), A.QH.Confirm);
            children.Add(B.QH.InsertSiCard, "插入社保卡(社保支付)", typeof(SiCardView), A.QH.Confirm);
            children.Add(A.QH.Confirm, "确认取号支付", typeof(ConfirmView), A.QH.Print);
            children.Add(A.QH.Print, "取号凭条", typeof(PrintView), A.Home);


            children.Context = A.JiaoFei_Context;
            children.Add(A.JF.BillRecord, "待缴费信息", typeof(BillRecordView), B.JF.SelectSi);
            children.Add(B.JF.SelectSi, "选择是否社保", typeof(SelectSiView), A.JF.Confirm);
            children.Add(B.JF.InsertSiCard, "插入社保卡(社保支付)", typeof(SiCardView), A.JF.Confirm);
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
            children.Add(A.YP.Query, "输入查询条件", typeof(Default.Component.InfoQuery.Views.InputView), A.YP.Medicine);
            children.Add(A.YP.Medicine, "药品信息列表", typeof(Component.InfoQuery.Views.MedicineItemsView), A.Home);

            children.Context = A.ChargeItemsQuery;
            children.Add(A.XM.Query, "输入查询条件", typeof(Component.InfoQuery.Views.InputView), A.XM.ChargeItems);
            children.Add(A.XM.ChargeItems, "诊疗项列表", typeof(ChargeItemsView), A.Home);

            children.Context = A.DiagReportQuery;
            //children.Add(A.JYJL.Date, "选择查询日期", typeof(DateTimeView), A.JYJL.DiagReport);
            children.Add(A.JYJL.DiagReport, "检验报告信息", typeof(DiagReportView), A.Home);

            //children.Context = A.PacsReportQuery;
            //children.Add(A.YXBG.Date, "选择查询日期", typeof(DateTimeView), A.YXBG.PacsReport);
            //children.Add(A.YXBG.PacsReport, "影像报告信息", typeof(PacsReportView), A.Home);

            #region 医改变动价格信息显示

            // testtest  testtest testtest: 2017.8.15

            children.Context = B.NewMedicineItemsQuery;
            children.Add(B.NMIQ.Query, "输入查询条件", typeof(Component.InfoQuery.Views.InputView), B.NMIQ.NewMedicineItems);
            children.Add(B.NMIQ.NewMedicineItems, "医改药品信息列表", typeof(NewMedicineItemsView), A.Home);

            children.Context = B.NewMedicineProjectQuery;
            children.Add(B.NMPQ.Query, "输入查询条件", typeof(Component.InfoQuery.Views.InputView), B.NMPQ.NewMedicineProject);
            children.Add(B.NMPQ.NewMedicineProject, "医改项目信息列表", typeof(NewMedicineProjectView), A.Home);
            

            #endregion


            children.Context = A.ZhuYuan_Context;
            children.Add(A.ZY.Choice, "个人信息", typeof(Default.Component.Auth.Views.ChoiceView), A.ZY.Card);
            children.Add(A.ZY.Card, "个人信息", typeof(InCardView), A.ZY.InPatientInfo);
            children.Add(A.ZY.IDCard, "个人信息", typeof(InIDCardView), A.ZY.InPatientInfo);
            children.Add(A.ZY.HICard, "个人信息", typeof(InSiCardView), A.ZY.InPatientInfo);
            children.Add(A.ZY.InPatientNo, "住院患者信息", typeof(Component.Auth.Views.InPatientNoView), A.ZY.InPatientInfo);
            children.Add(A.ZY.InPatientInfo, "住院患者信息", typeof(InPatientInfoView), A.Home);

            children.Context = A.InDayDetailList_Context;
            children.Add(A.ZYYRQD.Date, "选择查询日期", typeof(InDailyDateView), A.ZYYRQD.DailyDetail);
            children.Add(A.ZYYRQD.DailyDetail, "住院一日清单", typeof(InDailyDetailView), A.Home);

            children.Context = A.IpRecharge_Context;
            children.Add(A.ZYCZ.RechargeWay, "选择支付方式", typeof(MethodView), A.ZYCZ.InputAmount);
            children.Add(A.ZYCZ.InputAmount, "输入缴纳金额", typeof(ZYInputAmountView), A.ZYCZ.Print);
            children.Add(A.ZYCZ.Print, "缴押金完成", typeof(PrintView), A.Home);

            children.Context = A.InPrePayRecordQuery_Context;
            children.Add(A.ZYYJ.Date, "选择查询日期", typeof(DateTimeView), A.ZYYJ.InPrePayRecord);
            children.Add(A.ZYYJ.InPrePayRecord, "住院押金记录", typeof(InPrePayRecordView), A.Home);

            children.Context = B.ChuYuanJieSuan_Context;
            //children.Add(B.CY.CYInfo, "出院确认", typeof(ZYBillRecordView), B.CY.SelectSi);
            children.Add(B.CY.SelectSi, "选择是否社保", typeof(SelectSiView), B.CY.Confirm);
            children.Add(B.CY.InsertSiCard, "插入社保卡(社保支付)", typeof(SiCardView), B.CY.Confirm);
            children.Add(B.CY.Confirm, "确认结算支付", typeof(Component.ZYRecharge.Views.MethodView), B.CY.Print);
            children.Add(B.CY.Print, "结算完成", typeof(PrintView), A.Home);

            children.Context = B.BuKa_Context;
            //SelectTypeView->IDCardView/SiCardView->
            children.Add(B.BK.Confirm, "确认补卡信息", typeof(ConfirmView), B.BK.Print);
            children.Add(B.BK.Print, "补卡完成", typeof(PrintView), A.Home);

            if (CurrentStrategyType() == DeviceType.Clinic)
            {
                children.Context = null;
                children.Add(A.Home, null, "主页", typeof(Default.Clinic.Component.Views.ChoiceView));
                children.Add(A.AdminPart, null, "后台", typeof(Default.Part.Views.AdminPageView));
                children.Add(A.Third.PosUnion, null, "刷卡", typeof(PosView));
                children.Add(A.Third.Cash, null, "投币", typeof(Default.Clinic.Component.Recharge.Views.CashView));
                children.Add(A.Third.ScanQrCode, null, "扫码", typeof(ScanQrCodeView));

                children.Context = A.ChaKa_Context;
                children.Add(A.CK.Select, "个人信息", typeof(SelectTypeView), A.CK.Choice);
                children.Add(A.CK.Choice, "个人信息", typeof(Default.Clinic.Component.Auth.Views.ChoiceView), A.CK.Card);
                children.Add(A.CK.Card, "个人信息", typeof(Default.Clinic.Component.Auth.Views.CardView), A.CK.Info);
                //children.Add(A.CK.Card, "个人信息", typeof(CardView), A.CK.Info);
                children.Add(A.CK.IDCard, "个人信息", typeof(IDCardView), A.CK.Info);
                children.Add(A.CK.HICard, "个人信息", typeof(Clinic.Component.Auth.Views.ZjSiCardView), A.CK.Info);
                children.Add(A.CK.Info, "个人信息", typeof(Component.Auth.Views.PatientInfoView), A.Home);
                children.Add(A.CK.InfoEx, "个人信息", typeof(PatientInfoExView), A.Home);


                children.Context = A.XianChang_Context;
                children.Add(A.XC.Wether, "选择挂号类别", typeof(Default.Clinic.Component.Register.Views.RegTypesView), A.XC.Dept);
                children.Add(A.XC.Dept, "选择挂号科室", typeof(Default.Clinic.Component.Register.Views.DeptsView), A.XC.Doctor);
                children.Add(A.XC.Doctor, "选择医生排班", typeof(Default.Clinic.Component.Register.Views.DoctorView), A.XC.Schedule);
                children.Add(A.XC.Schedule, "选择医生排班", typeof(Default.Clinic.Component.Register.Views.ScheduleView), B.XC.SelectSi);

                if (FrameworkConst.HospitalId == "1103")
                {
                    children.Add(A.XC.Schedule, "选择医生排班", typeof(Default.Clinic.Component.Register.Views.ScheduleView), A.XC.Time);
                    children.Add(A.XC.Time, "选择排班时段", typeof(Default.Clinic.Component.Register.Views.SourceView), A.XC.Confirm);
                }

                children.Add(B.XC.SelectSi, "选择是否社保", typeof(SelectSiView), A.XC.Confirm);
                children.Add(B.XC.InsertSiCard, "插入社保卡(社保支付)", typeof(SiCardView), A.XC.Confirm);
                children.Add(A.XC.Confirm, "选择支付方式", typeof(Default.Clinic.Component.Views.ConfirmView), A.XC.Print);
                children.Add(A.XC.Print, "挂号凭条", typeof(PrintView), A.Home);


                children.Context = A.YuYue_Context;            
                children.Add(A.YY.Date, "选择预约日期", typeof(Default.Clinic.Component.Register.Views.RegDateView), A.YY.Wether);
                children.Add(A.YY.Wether, "选择预约类别", typeof(Default.Clinic.Component.Register.Views.RegTypesView), A.YY.Dept);
                children.Add(A.YY.Dept, "选择预约科室", typeof(Default.Clinic.Component.Register.Views.DeptsView), A.YY.Doctor);
                children.Add(A.YY.Doctor, "选择医生排班", typeof(Default.Clinic.Component.Register.Views.DoctorView), A.YY.Schedule);
                children.Add(A.YY.Schedule, "选择医生排班", typeof(Default.Clinic.Component.Register.Views.ScheduleView), A.YY.Time);
                children.Add(A.YY.Time, "选择医生排班", typeof(Default.Clinic.Component.Register.Views.SourceView), A.YY.Confirm);
                children.Add(A.YY.Confirm, "确认预约信息", typeof(Default.Clinic.Component.Views.ConfirmView), A.YY.Print);
                children.Add(A.YY.Print, "预约凭条", typeof(PrintView), A.Home);

                children.Context = A.QuHao_Context;
                children.Add(A.QH.Record, "选择预约记录", typeof(Default.Clinic.Component.TakeNum.Views.ApptRecordView), A.QH.TakeNum);
                children.Add(A.QH.TakeNum, "确认取号信息", typeof(TakeNumView), B.QH.SelectSi);
                children.Add(B.QH.SelectSi, "选择是否社保", typeof(SelectSiView), A.QH.Confirm);
                children.Add(B.QH.InsertSiCard, "插入社保卡(社保支付)", typeof(SiCardView), A.QH.Confirm);
                children.Add(A.QH.Confirm, "确认取号支付", typeof(Default.Clinic.Component.Views.ConfirmView), A.QH.Print);
                children.Add(A.QH.Print, "取号凭条", typeof(PrintView), A.Home);


                children.Context = A.JiaoFei_Context;
                children.Add(A.JF.BillRecord, "待缴费信息", typeof(Default.Clinic.Component.BillPay.Views.BillRecordView), B.JF.SelectSi);
                children.Add(B.JF.SelectSi, "选择是否社保", typeof(SelectSiView), A.JF.Confirm);
                children.Add(B.JF.InsertSiCard, "插入社保卡(社保支付)", typeof(SiCardView), A.JF.Confirm);
                children.Add(A.JF.Confirm, "确认结算支付", typeof(Default.Clinic.Component.Views.ConfirmView), A.JF.Print);
                children.Add(A.JF.Print, "缴费完成", typeof(PrintView), A.Home);

                children.Context = A.ChongZhi_Context;
                children.Add(A.CZ.RechargeWay, "选择充值方式", typeof(Default.Clinic.Component.Recharge.Views.RechargeMethodView), A.CZ.InputAmount);
                children.Add(A.CZ.InputAmount, "输入充值金额", typeof(InputAmountView), A.CZ.Print);
                children.Add(A.CZ.Print, "充值完成", typeof(PrintView), A.Home);

                children.Add(A.QueryChoice, null, "查询选择", typeof(Default.Clinic.Component.InfoQuery.Views.QueryChoiceView));

                children.Context = A.PayCostQuery;
                children.Add(A.JFJL.Date, "选择查询日期", typeof(DateTimeView), A.JFJL.PayCostRecord);
                children.Add(A.JFJL.PayCostRecord, "已缴费信息", typeof(Default.Clinic.Component.InfoQuery.Views.PayCostRecordView), A.Home);

                children.Context = A.MedicineQuery;
                children.Add(A.YP.Query, "输入查询条件", typeof(Default.Clinic.Component.InfoQuery.Views.InputView), A.YP.Medicine);
                children.Add(A.YP.Medicine, "药品信息列表", typeof(Component.InfoQuery.Views.MedicineItemsView), A.Home);

                children.Context = A.ChargeItemsQuery;
                children.Add(A.XM.Query, "输入查询条件", typeof(Default.Clinic.Component.InfoQuery.Views.InputView), A.XM.ChargeItems);
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
                children.Add(A.ZYCZ.Print, "缴押金完成", typeof(PrintView), A.Home);

                children.Context = A.InPrePayRecordQuery_Context;
                children.Add(A.ZYYJ.Date, "选择查询日期", typeof(DateTimeView), A.ZYYJ.InPrePayRecord);
                children.Add(A.ZYYJ.InPrePayRecord, "住院押金记录", typeof(InPrePayRecordView), A.Home);
            }
            else
            {

            }

            return true;
        }

        public override List<Uri> GetResourceDictionaryUris()
        {
            return new List<Uri> { new Uri("pack://application:,,,/YuanTu.ChongQingArea;component/Theme/default.xaml") };
        }
    }
}
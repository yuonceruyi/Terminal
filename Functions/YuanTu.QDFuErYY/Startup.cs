using System;
using System.IO;
using System.Linq;
using System.Windows;
using YuanTu.Consts;
using YuanTu.Core.FrameworkBase;
using YuanTu.Core.Navigating;
using YuanTu.Default.Component.BillPay.Views;
using YuanTu.Default.Component.Recharge.Views;
using YuanTu.Default.Component.Register.Views;
using YuanTu.Default.Component.TakeNum.Views;
using YuanTu.Default.Component.Tools.Views;
using YuanTu.Default.Component.Views;
using YuanTu.Default.Component.ZYRecharge.Views;
using QDAuthVi = YuanTu.QDKouQiangYY.Component.Auth.Views;
using QDQueryVi = YuanTu.QDKouQiangYY.Component.InfoQuery.Views;
using QDBiometricVi = YuanTu.QDKouQiangYY.Component.Biometric.Views;
using DefAuthVi = YuanTu.Default.Component.Auth.Views;
using DefQueryVi = YuanTu.Default.Component.InfoQuery.Views;
using YuanTu.Default.Part.Views;
using YuanTu.QDFuErYY.Component.InfoQuery.Views;
using YuanTu.QDFuErYY.Current.Services;
using YuanTu.QDFuErYY.Current.Models;
using Microsoft.Practices.Unity;
using DefClinicVi = YuanTu.Default.Clinic.Component;
using YuanTu.QDFuErYY.Clinic.Component.SignIn;
using Microsoft.Practices.ServiceLocation;
using YuanTu.Consts.Services;
using YuanTu.Consts.Enums;
using YuanTu.QDFuErYY.Component.Auth.Views;

namespace YuanTu.QDFuErYY
{
    public class Startup : DefaultStartup
    {
        public override int Order => 1;

        public override string[] UseConfigPath()
        {
            return null;
        }
        public override void AfterStartup()
        {
            FrameworkConst.HospitalName = "青岛市妇女儿童医院";
            QDArea.ConfigQD.HighPwd = "870113";
            Devices.CashBox.CashInputBox.AcceptBills =
                Devices.CashBox.CashInputBox.Bills.Bill_50 |
                Devices.CashBox.CashInputBox.Bills.Bill_100;

            QualificationService.QualificationInit();
            QDArea.ConfigQD.ScheduleVersion = "1";
            var config = ServiceLocator.Current.GetInstance<IConfigurationManager>();
            var v = config.GetValue($"Functions:{Business.签到}:Visabled");
            if (v == "1")
            {
                ISignInService signIn = ServiceLocator.Current.GetInstance<ISignInService>();
            }
            ConfigPatch();
        }

        private void ConfigPatch()
        {
            try
            {
                var config = ServiceLocator.Current.GetInstance<IConfigurationManager>();
                var amPm = config.GetValue("AmPmSession:上午:Order");
                if (amPm == null)
                {
                    var dirName = AppDomain.CurrentDomain.BaseDirectory;
                    var cfgFileName = dirName + "Config.xml";

                    var oldLines = File.ReadAllLines(cfgFileName);

                    if (File.Exists(cfgFileName))
                    {
                        string[] newLines = {
                            "<!--就诊场次-->",
                            "<AmPmSession>",
                            "	<上午>",
                            "		<Order>0</Order>",
                            "		<Enable>1</Enable>",
                            "		<Visiable>1</Visiable>",
                            "		<ImageName>科室类别_专家门诊</ImageName>",
                            "		<Color>248,180,117</Color>",
                            "		<StartTime>07:00</StartTime>",
                            "		<EndTime>12:00</EndTime>",
                            "		<Remark>07:00-12:00</Remark>",
                            "	</上午>",
                            "	<下午>",
                            "		<Order>0</Order>",
                            "		<Enable>1</Enable>",
                            "		<Visiable>1</Visiable>",
                            "		<ImageName>科室类别_专家门诊</ImageName>",
                            "		<Color>248,180,117</Color>",
                            "		<StartTime>12:00</StartTime>",
                            "		<EndTime>17:00</EndTime>",
                            "		<Remark>12:00-17:00</Remark>",
                            "	</下午>",
                            "</AmPmSession>"
                        };

                        using (StreamWriter file = new StreamWriter(cfgFileName, false))
                        {
                            if (oldLines != null && oldLines.Count() > 0)
                            {
                                foreach (string oldLine in oldLines)
                                {
                                    file.WriteLine(oldLine);
                                    if (oldLine.Contains("</RegType>"))
                                    {
                                        foreach (string newLine in newLines)
                                        {
                                            file.WriteLine(newLine);
                                        }
                                    }

                                }
                            }

                        }
                    }
                    else
                    {
                        YuanTu.Core.Log.Logger.Main.Error("启动时未找到Config.xml文件");
                        return;
                    }

                    System.Diagnostics.Process.Start(System.Windows.Forms.Application.ExecutablePath);
                    Environment.Exit(0);
                }
            }
            catch (Exception ex)
            {
                YuanTu.Core.Log.Logger.Main.Error("[系统错误0x000000003]" + ex.ToString());
                return;
            }


        }
        public override bool RegisterTypes(ViewCollection children)
        {
            children.Add(A.Home, null, "主页", typeof(ChoiceView));
            children.Add(A.AdminPart, null, "后台", typeof(AdminPageView));
            children.Add(A.Maintenance, null, "维护", typeof(YuanTu.Default.Part.Views.MaintenanceView));
            children.Add(A.Third.PosUnion, null, "投币/刷卡/扫码", typeof(PosView));
            children.Add(A.Third.Cash, null, "投币/刷卡/扫码", typeof(CashView));
            children.Add(A.Third.AtmCash, null, "现金缴住院押金", typeof(HatmView));
            children.Add(A.Third.JCMCash, null, "现金缴住院押金", typeof(JCMView));
            children.Add(A.Third.ScanQrCode, null, "投币/刷卡/扫码", typeof(ScanQrCodeView));
            children.Add(A.Third.SiPay, null, "投币/刷卡/扫码", typeof(YuanTu.QDKouQiangYY.Component.Tools.Views.SiPayView));

            children.Context = A.ChaKa_Context;
            children.Add(A.CK.Select, "个人信息", typeof(SelectTypeView), A.CK.Choice);
            children.Add(A.CK.Choice, "个人信息", typeof(DefAuthVi.ChoiceView), A.CK.Card);
            children.Add(A.CK.Card, "个人信息", typeof(DefAuthVi.CardView), A.CK.Info);
            children.Add(A.CK.FaceRec, "个人信息", typeof(QDAuthVi.FaceRecView), A.CK.Info);
            children.Add(A.CK.IDCard, "个人信息", typeof(QDAuthVi.IDCardView), A.CK.Info);
            children.Add(A.CK.InputIDCard, "个人信息", typeof(QDAuthVi.InputIDView), A.CK.Info);
            children.Add(A.CK.HICard, "个人信息", typeof(DefAuthVi.SiCardView), A.CK.Info);
            children.Add(A.CK.QrCode, "个人信息", typeof(DefAuthVi.QrCodeView), A.CK.Info);
            children.Add(A.CK.Info, "个人信息", typeof(QDAuthVi.PatientInfoView), A.Home);
            children.Add(A.CK.InfoEx, "个人信息", typeof(QDAuthVi.PatientInfoExView), A.Home);

            children.Context = A.JianDang_Context;
            children.Add(A.JD.Print, "建档结果", typeof(PrintView), A.Home);

            children.Context = A.XianChang_Context;
            children.Add(A.XC.Wether, "选择挂号类别", typeof(RegTypesView), A.XC.Dept);
            //children.Add(A.XC.ParentDept, "选择科室分类", typeof(ParentDeptsView), A.XC.Dept);
            children.Add(A.XC.Dept, "选择挂号科室", typeof(DeptsView), A.XC.AMPM);
            children.Add(A.XC.AMPM, "选择挂号场次", typeof(RegAmPmView), A.XC.Doctor);
            children.Add(A.XC.Doctor, "选择医生排班", typeof(DoctorView), A.XC.Schedule);
            children.Add(A.XC.Schedule, "选择医生排班", typeof(ScheduleView), A.XC.Confirm);
            children.Add(A.XC.Confirm, "选择支付方式", typeof(ConfirmView), A.XC.Print);
            children.Add(Guid.NewGuid().ToString(), "投币/刷卡/扫码", null, A.XC.Print);
            children.Add(A.XC.Print, "挂号凭条", typeof(PrintView), A.Home);

            children.Context = A.YuYue_Context;
            children.Add(A.YY.Date, "选择预约日期", typeof(RegDateView), A.YY.Wether);
            children.Add(A.YY.Wether, "选择预约类别", typeof(RegTypesView), A.YY.Dept);
            //children.Add(A.YY.ParentDept, "选择科室分类", typeof(ParentDeptsView), A.YY.Dept);
            children.Add(A.YY.Dept, "选择预约科室", typeof(DeptsView), A.YY.AMPM);
            children.Add(A.YY.AMPM, "选择预约场次", typeof(RegAmPmView), A.YY.Doctor);
            children.Add(A.YY.Doctor, "选择医生排班", typeof(DoctorView), A.YY.Schedule);
            children.Add(A.YY.Schedule, "选择医生排班", typeof(ScheduleView), A.YY.Confirm);
            children.Add(A.YY.Time, "选择医生排班", typeof(SourceView), A.YY.Confirm);
            children.Add(A.YY.Confirm, "确认预约信息", typeof(ConfirmView), A.YY.Print);
            children.Add(A.YY.Print, "预约凭条", typeof(PrintView), A.Home);

            children.Context = A.QuHao_Context;
            children.Add(A.QH.Record, "选择预约记录", typeof(ApptRecordView), A.QH.TakeNum);
            children.Add(A.QH.TakeNum, "确认取号信息", typeof(QDKouQiangYY.Component.TakeNum.Views.TakeNumView), A.QH.Confirm);
            children.Add(A.QH.Confirm, "确认取号支付", typeof(ConfirmView), A.QH.Print);
            children.Add(A.QH.Print, "取号凭条", typeof(PrintView), A.Home);

            children.Context = A.JiaoFei_Context;
            children.Add(A.JF.BillRecord, "待缴费信息", typeof(BillRecordView), A.JF.Confirm);
            children.Add(A.JF.Confirm, "确认结算支付", typeof(ConfirmView), A.JF.Print);
            children.Add(A.JF.Print, "缴费完成", typeof(PrintView), A.Home);

            children.Context = A.ChongZhi_Context;
            children.Add(A.CZ.RechargeWay, "选择充值方式", typeof(RechargeMethodView), A.CZ.InputAmount);
            children.Add(A.CZ.InputAmount, "输入充值金额", typeof(InputAmountView), A.CZ.Print);
            children.Add(Guid.NewGuid().ToString(), "投币/刷卡/扫码", null, A.CZ.Print);
            children.Add(A.CZ.Print, "充值完成", typeof(PrintView), A.Home);

            children.Add(A.QueryChoice, null, "查询选择", typeof(DefQueryVi.QueryChoiceView));

            children.Context = A.PayCostQuery;
            children.Add(A.JFJL.Date, "选择查询日期", typeof(DefQueryVi.DateTimeView), A.JFJL.PayCostRecord);
            children.Add(A.JFJL.PayCostRecord, "已缴费信息", typeof(DefQueryVi.PayCostRecordView), A.Home);

            children.Context = A.MedicineQuery;
            children.Add(A.YP.Query, "输入查询条件", typeof(DefQueryVi.InputView), A.YP.Medicine);
            children.Add(A.YP.Medicine, "药品信息列表", typeof(DefQueryVi.MedicineItemsView), A.Home);

            children.Context = A.ChargeItemsQuery;
            children.Add(A.XM.Query, "输入查询条件", typeof(DefQueryVi.InputView), A.XM.ChargeItems);
            children.Add(A.XM.ChargeItems, "诊疗项列表", typeof(DefQueryVi.ChargeItemsView), A.Home);

            children.Context = A.DiagReportQuery;
            children.Add(A.JYJL.Date, "选择查询日期", typeof(DefQueryVi.DateTimeView), A.JYJL.DiagReport);
            children.Add(A.JYJL.DiagReport, "检验报告信息", typeof(DefQueryVi.DiagReportView), A.Home);

            children.Context = A.PacsReportQuery;
            children.Add(A.YXBG.Date, "选择查询日期", typeof(DefQueryVi.DateTimeView), A.YXBG.PacsReport);
            children.Add(A.YXBG.PacsReport, "影像报告信息", typeof(DefQueryVi.PacsReportView), A.Home);

            children.Context = A.ZhuYuan_Context;
            children.Add(A.ZY.InPatientNo, "住院患者信息", typeof(DefAuthVi.InPatientNoView), A.ZY.InPatientInfo);
            children.Add(A.ZY.InPatientInfo, "住院患者信息", typeof(DefAuthVi.InPatientInfoView), A.Home);

            children.Context = A.InDayDetailList_Context;
            children.Add(A.ZYYRQD.Date, "选择查询日期", typeof(DefQueryVi.InDailyDateView), A.ZYYRQD.DailyDetail);
            children.Add(A.ZYYRQD.DailyDetail, "住院一日清单", typeof(QDQueryVi.InDailyDetailView), A.Home);

            children.Context = A.IpRecharge_Context;
            children.Add(A.ZYCZ.RechargeWay, "选择支付方式", typeof(MethodView), A.ZYCZ.InputAmount);
            children.Add(A.ZYCZ.InputAmount, "输入缴纳金额", typeof(ZYInputAmountView), A.ZYCZ.Print);
            children.Add(Guid.NewGuid().ToString(), "投币/刷卡/扫码", null, A.ZYCZ.Print);
            children.Add(A.ZYCZ.Print, "缴押金完成", typeof(PrintView), A.Home);

            children.Context = AInner.QualificationQuery_Context;
            children.Add(AInner.ZYZG.Query, "输入查询条件", typeof(DefQueryVi.InputView), AInner.ZYZG.Query);
            children.Add(AInner.ZYZG.Qualification, "执业资格列表", typeof(QualificationView), A.Home);

            children.Context = A.Biometric_Context;
            children.Add(A.Bio.Choice, "选择录入信息", typeof(QDBiometricVi.ChoiceView), A.Bio.FaceRec);
            children.Add(A.Bio.FaceRec, "面部信息录入", typeof(QDBiometricVi.FaceRecView), A.Home);
            children.Add(A.Bio.FingerPrint, "指纹信息录入", typeof(QDBiometricVi.FingerPrintView), A.Home);

            if (CurrentStrategyType() == DeviceType.Clinic)
                RegisterClinicTypes(children);

            return true;
        }
        public bool RegisterClinicTypes(ViewCollection children)
        {
            children.Add(A.Home, null, "主页", typeof(DefClinicVi.Views.ChoiceView));
            children.Add(A.AdminPart, null, "后台", typeof(Default.Part.Views.AdminPageView));
            children.Add(A.Third.PosUnion, null, "刷卡", typeof(YuanTu.QDKouQiangYY.Component.Tools.Views.PosView));
            children.Add(A.Third.Cash, null, "投币", typeof(DefClinicVi.Recharge.Views.CashView));
            children.Add(A.Third.ScanQrCode, null, "扫码", typeof(ScanQrCodeView));
            children.Add(A.Third.SiPay, null, "投币/刷卡/扫码", typeof(YuanTu.QDKouQiangYY.Component.Tools.Views.SiPayView));

            children.Context = A.ChaKa_Context;
            children.Add(A.CK.Select, "个人信息", typeof(SelectTypeView), A.CK.Choice);
            children.Add(A.CK.Choice, "个人信息", typeof(DefClinicVi.Auth.Views.ChoiceView), A.CK.Card);
            children.Add(A.CK.Card, "个人信息", typeof(QDAuthVi.CardView), A.CK.Info);
            children.Add(A.CK.IDCard, "个人信息", typeof(QDAuthVi.IDCardView), A.CK.Info);
            children.Add(A.CK.HICard, "个人信息", typeof(DefAuthVi.SiCardView), A.CK.Info);
            children.Add(A.CK.Info, "个人信息", typeof(QDAuthVi.PatientInfoView), A.Home);
            children.Add(A.CK.InfoEx, "个人信息", typeof(QDAuthVi.PatientInfoExView), A.Home);

            children.Context = A.JianDang_Context;
            children.Add(A.JD.Print, "建档结果", typeof(PrintView), A.Home);

            children.Context = A.XianChang_Context;
            children.Add(A.XC.Wether, "选择挂号类别", typeof(DefClinicVi.Register.Views.RegTypesView), A.XC.Dept);
            //children.Add(A.XC.ParentDept, "选择科室分类", typeof(DefClinicVi.Register.Views.ParentDeptsView), A.XC.Dept);
            children.Add(A.XC.Dept, "选择挂号科室", typeof(DefClinicVi.Register.Views.DeptsView), A.XC.AMPM);
            children.Add(A.XC.AMPM, "选择挂号场次", typeof(DefClinicVi.Register.Views.RegAmPmView), A.XC.Doctor);
            children.Add(A.XC.Doctor, "选择医生排班", typeof(DefClinicVi.Register.Views.DoctorView), A.XC.Schedule);
            children.Add(A.XC.Schedule, "选择医生排班", typeof(DefClinicVi.Register.Views.ScheduleView), A.XC.Confirm);
            children.Add(A.XC.Confirm, "选择支付方式", typeof(DefClinicVi.Views.ConfirmView), A.XC.Print);
            //children.Add(Guid.NewGuid().ToString(), "投币/刷卡/扫码", null, A.XC.Print);
            children.Add(A.XC.Print, "挂号凭条", typeof(PrintView), A.Home);

            children.Context = A.YuYue_Context;
            children.Add(A.YY.Date, "选择预约日期", typeof(DefClinicVi.Register.Views.RegDateView), A.YY.Wether);
            children.Add(A.YY.Wether, "选择预约类别", typeof(DefClinicVi.Register.Views.RegTypesView), A.YY.Dept);
            //children.Add(A.YY.ParentDept, "选择科室分类", typeof(DefClinicVi.Register.Views.ParentDeptsView), A.YY.Dept);
            children.Add(A.YY.Dept, "选择预约科室", typeof(DefClinicVi.Register.Views.DeptsView), A.YY.AMPM);
            children.Add(A.YY.AMPM, "选择预约场次", typeof(DefClinicVi.Register.Views.RegAmPmView), A.YY.Doctor);
            children.Add(A.YY.Doctor, "选择医生排班", typeof(DefClinicVi.Register.Views.DoctorView), A.YY.Schedule);
            children.Add(A.YY.Schedule, "选择医生排班", typeof(DefClinicVi.Register.Views.ScheduleView), A.YY.Confirm);
            children.Add(A.YY.Time, "选择医生排班", typeof(DefClinicVi.Register.Views.SourceView), A.YY.Confirm);
            children.Add(A.YY.Confirm, "确认预约信息", typeof(DefClinicVi.Views.ConfirmView), A.YY.Print);
            children.Add(A.YY.Print, "预约凭条", typeof(PrintView), A.Home);

            children.Context = A.QuHao_Context;
            children.Add(A.QH.Record, "选择预约记录", typeof(DefClinicVi.TakeNum.Views.ApptRecordView), A.QH.TakeNum);
            children.Add(A.QH.TakeNum, "确认取号信息", typeof(TakeNumView), A.QH.Confirm);
            children.Add(A.QH.Confirm, "确认取号支付", typeof(DefClinicVi.Views.ConfirmView), A.QH.Print);
            children.Add(A.QH.Print, "取号凭条", typeof(PrintView), A.Home);

            children.Context = A.JiaoFei_Context;
            children.Add(A.JF.BillRecord, "待缴费信息", typeof(DefClinicVi.BillPay.Views.BillRecordView), A.JF.Confirm);
            children.Add(A.JF.Confirm, "确认结算支付", typeof(DefClinicVi.Views.ConfirmView), A.JF.Print);
            children.Add(A.JF.Print, "缴费完成", typeof(PrintView), A.Home);

            children.Context = A.ChongZhi_Context;
            children.Add(A.CZ.RechargeWay, "选择充值方式", typeof(DefClinicVi.Recharge.Views.RechargeMethodView), A.CZ.InputAmount);
            children.Add(A.CZ.InputAmount, "输入充值金额", typeof(InputAmountView), A.CZ.Print);
            //children.Add(Guid.NewGuid().ToString(), "投币/刷卡/扫码", null, A.CZ.Print);
            children.Add(A.CZ.Print, "充值完成", typeof(PrintView), A.Home);

            children.Add(A.QueryChoice, null, "查询选择", typeof(DefClinicVi.InfoQuery.Views.QueryChoiceView));

            children.Context = A.PayCostQuery;
            children.Add(A.JFJL.Date, "选择查询日期", typeof(DefQueryVi.DateTimeView), A.JFJL.PayCostRecord);
            children.Add(A.JFJL.PayCostRecord, "已缴费信息", typeof(DefClinicVi.InfoQuery.Views.PayCostRecordView), A.Home);

            children.Context = A.MedicineQuery;
            children.Add(A.YP.Query, "输入查询条件", typeof(DefClinicVi.InfoQuery.Views.InputView), A.YP.Medicine);
            children.Add(A.YP.Medicine, "药品信息列表", typeof(DefQueryVi.MedicineItemsView), A.Home);

            children.Context = A.ChargeItemsQuery;
            children.Add(A.XM.Query, "输入查询条件", typeof(DefClinicVi.InfoQuery.Views.InputView), A.XM.ChargeItems);
            children.Add(A.XM.ChargeItems, "诊疗项列表", typeof(DefQueryVi.ChargeItemsView), A.Home);


            children.Context = A.ZhuYuan_Context;
            children.Add(A.ZY.InPatientNo, "个人信息", typeof(DefAuthVi.InPatientNoView), A.ZY.InPatientInfo);
            children.Add(A.ZY.InPatientInfo, "个人信息", typeof(DefAuthVi.InPatientInfoView), A.Home);

            children.Context = A.InDayDetailList_Context;
            children.Add(A.ZYYRQD.Date, "选择查询日期", typeof(DefQueryVi.InDailyDateView), A.ZYYRQD.DailyDetail);
            children.Add(A.ZYYRQD.DailyDetail, "住院一日清单", typeof(QDQueryVi.InDailyDetailView), A.Home);

            children.Context = A.IpRecharge_Context;
            children.Add(A.ZYCZ.RechargeWay, "选择支付方式", typeof(MethodView), A.ZYCZ.InputAmount);
            children.Add(A.ZYCZ.InputAmount, "输入缴纳金额", typeof(ZYInputAmountView), A.ZYCZ.Print);
            //children.Add(Guid.NewGuid().ToString(), "投币/刷卡/扫码", null, A.ZYCZ.Print);
            children.Add(A.ZYCZ.Print, "缴押金完成", typeof(PrintView), A.Home);

            children.Context = AInner.QualificationQuery_Context;
            children.Add(AInner.ZYZG.Query, "输入查询条件", typeof(DefClinicVi.InfoQuery.Views.InputView), AInner.ZYZG.Query);
            children.Add(AInner.ZYZG.Qualification, "执业资格列表", typeof(Clinic.Component.InfoQuery.Views.QualificationView), A.Home);

            children.Context = AInner.QueneSelect_Context;
            children.Add(AInner.PDJH.Select, "排队叫号签到", typeof(Clinic.Component.SignIn.Views.QueueSelectView), A.Home);
            return true;
        }
    }
}
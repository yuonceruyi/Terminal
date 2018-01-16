using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Practices.ServiceLocation;
using YuanTu.Consts;
using YuanTu.Consts.Services;
using YuanTu.Core.FrameworkBase;
using YuanTu.Core.Log;
using YuanTu.Core.Navigating;
using YuanTu.Default.Component.Auth.Views;
using YuanTu.Default.Component.BillPay.Views;
using YuanTu.Default.Component.InfoQuery.Views;
using YuanTu.Default.Component.PrintAgain.Views;
using YuanTu.Default.Component.Recharge.Views;
using YuanTu.Default.Component.Register.Views;
using YuanTu.Default.Component.TakeNum.Views;
using YuanTu.Default.Component.Tools.Views;
using YuanTu.Default.Component.Views;
using YuanTu.Default.Component.ZYRecharge.Views;
using YuanTu.Default.Part.Views;
using YuanTu.YuHangFYBJY.NativeService;
using ChoiceView = YuanTu.Default.Component.Views.ChoiceView;
using DateTimeView = YuanTu.Default.Component.InfoQuery.Views.DateTimeView;
using PayCostRecordView = YuanTu.Default.Component.InfoQuery.Views.PayCostRecordView;
using SiCardView = YuanTu.YuHangFYBJY.Component.Auth.Views.SiCardView;


namespace YuanTu.YuHangFYBJY
{
    public class Startup : YuanTu.Default.Clinic.Startup
    {
        /// <summary>
        ///     优先级，数值越小优先级越高，内部配置越优先被使用
        /// </summary>
        public override int Order => 1;

        public override bool RegisterTypes(ViewCollection children)
        {

            children.Add(A.Home, null, "主页", typeof(Default.Clinic.Component.Views.ChoiceView));
            children.Add(A.AdminPart, null, "后台", typeof(AdminPageView));
            children.Add(A.Maintenance, null, "维护", typeof(MaintenanceView));
            children.Add(A.Third.PosUnion, null, "刷卡", typeof(PosView));
            children.Add(A.Third.Cash, null, "投币", typeof(CashView));
            children.Add(A.Third.AtmCash, null, "现金缴住院押金", typeof(HatmView));
            children.Add(A.Third.JCMCash, null, "现金缴住院押金", typeof(JCMView));
            children.Add(A.Third.ScanQrCode, null, "扫码", typeof(ScanQrCodeView));

            children.Context = A.ChaKa_Context;
            children.Add(A.CK.Select, "个人信息", typeof(SelectTypeView), A.CK.Choice);
            children.Add(A.CK.Choice, "个人信息", typeof(Default.Clinic.Component.Auth.Views.ChoiceView), A.CK.Card);
            children.Add(A.CK.Card, "个人信息", typeof(YuanTu.YuHangFYBJY.Component.Auth.Views.CardView), A.CK.Info);
            children.Add(A.CK.IDCard, "个人信息", typeof(IDCardView), A.CK.Info);
            children.Add(A.CK.HICard, "个人信息", typeof(SiCardView), A.CK.Info);
            children.Add(A.CK.QrCode, "个人信息", typeof(QrCodeView), A.CK.Info);
            children.Add(A.CK.Info, "个人信息", typeof(PatientInfoView), A.Home);
            children.Add(A.CK.InfoEx, "个人信息", typeof(PatientInfoExView), A.Home);


            //children.Context = A.JianDang_Context;
            //children.Add(InnerA.JD.Confirm, "选择支付方式", typeof(ConfirmView), A.JD.Print);
            //children.Add(A.JD.Print, "建档结果", typeof(PrintView), A.Home);

            children.Context = A.XianChang_Context;
            children.Add(A.XC.Wether, "选择挂号类别", typeof(Default.Clinic.Component.Register.Views.RegTypesView), A.XC.Dept);
            children.Add(A.XC.Dept, "选择挂号科室", typeof(Default.Clinic.Component.Register.Views.DeptsView), A.XC.Doctor);
            children.Add(A.XC.Doctor, "选择医生排班", typeof(Default.Clinic.Component.Register.Views.DoctorView), A.XC.Schedule);
            children.Add(A.XC.Schedule, "选择医生排班", typeof(Default.Clinic.Component.Register.Views.ScheduleView), A.XC.Confirm);
            children.Add(A.XC.Confirm, "选择支付方式", typeof(Default.Clinic.Component.Views.ConfirmView), A.XC.Print);
            //children.Add(Guid.NewGuid().ToString(), "投币/刷卡/扫码", null, A.XC.Print);
            children.Add(A.XC.Print, "挂号凭条", typeof(PrintView), A.Home);

            children.Context = A.YuYue_Context;
            children.Add(A.YY.Date, "选择预约日期", typeof(Default.Clinic.Component.Register.Views.RegDateView), A.YY.Wether);
            children.Add(A.YY.Wether, "选择预约类别", typeof(Default.Clinic.Component.Register.Views.RegTypesView), A.YY.Dept);
            children.Add(A.YY.Dept, "选择预约科室", typeof(Default.Clinic.Component.Register.Views.DeptsView), A.YY.Doctor);
            children.Add(A.YY.Doctor, "选择医生排班", typeof(Default.Clinic.Component.Register.Views.DoctorView), A.YY.Schedule);
            children.Add(A.YY.Schedule, "选择医生排班", typeof(Default.Clinic.Component.Register.Views.ScheduleView), A.YY.Confirm);
            children.Add(A.YY.Time, "选择排班分时", typeof(Default.Clinic.Component.Register.Views.SourceView), A.YY.Confirm);
            children.Add(A.YY.Confirm, "确认预约信息", typeof(Default.Clinic.Component.Views.ConfirmView), A.YY.Print);
            children.Add(A.YY.Print, "预约凭条", typeof(PrintView), A.Home);

            children.Context = A.QuHao_Context;
            //children.Add(InnerA.QH.Date,"选择预约日期",typeof(TakeNumDateView),A.QH.Record);
            children.Add(A.QH.Record, "选择预约记录", typeof(Default.Clinic.Component.TakeNum.Views.ApptRecordView), A.QH.TakeNum);
            children.Add(A.QH.TakeNum, "确认取号信息", typeof(TakeNumView), A.QH.Confirm);
            children.Add(A.QH.Confirm, "确认取号支付", typeof(ConfirmView), A.QH.Print);
            children.Add(A.QH.Print, "取号凭条", typeof(PrintView), A.Home);

            children.Context = A.JiaoFei_Context;
            children.Add(A.JF.BillRecord, "待缴费信息", typeof(Default.Clinic.Component.BillPay.Views.BillRecordView), A.JF.Confirm);
            children.Add(A.JF.Confirm, "确认结算支付", typeof(Default.Clinic.Component.Views.ConfirmView), A.JF.Print);
            children.Add(A.JF.Print, "缴费完成", typeof(PrintView), A.Home);

            children.Context = A.ChongZhi_Context;
            children.Add(A.CZ.RechargeWay, "选择充值方式", typeof(Default.Clinic.Component.Recharge.Views.RechargeMethodView), A.CZ.InputAmount);
            children.Add(A.CZ.InputAmount, "输入充值金额", typeof(InputAmountView), A.CZ.Print);
            //children.Add(Guid.NewGuid().ToString(), "投币/刷卡/扫码", null, A.CZ.Print);
            children.Add(A.CZ.Print, "充值完成", typeof(PrintView), A.Home);

            children.Add(A.QueryChoice, null, "查询选择", typeof(Default.Clinic.Component.InfoQuery.Views.QueryChoiceView));

            children.Context = A.PayCostQuery;
            children.Add(A.JFJL.Date, "选择查询日期", typeof(DateTimeView), A.JFJL.PayCostRecord);
            children.Add(A.JFJL.PayCostRecord, "已缴费信息", typeof(Default.Clinic.Component.InfoQuery.Views.PayCostRecordView), A.Home);

            children.Context = A.MedicineQuery;
            children.Add(A.YP.Query, "输入查询条件", typeof(Default.Clinic.Component.InfoQuery.Views.InputView), A.YP.Medicine);
            children.Add(A.YP.Medicine, "药品信息列表", typeof(MedicineItemsView), A.Home);

            children.Context = A.ChargeItemsQuery;
            children.Add(A.XM.Query, "输入查询条件", typeof(Default.Clinic.Component.InfoQuery.Views.InputView), A.XM.ChargeItems);
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
            children.Add(A.ZY.Choice, "个人信息", typeof(Default.Clinic.Component.Auth.Views.ChoiceView), A.ZY.Card);
            children.Add(A.ZY.Card, "个人信息", typeof(InCardView), A.ZY.InPatientInfo);
            children.Add(A.ZY.IDCard, "个人信息", typeof(InIDCardView), A.ZY.InPatientInfo);
            children.Add(A.ZY.HICard, "个人信息", typeof(InSiCardView), A.ZY.InPatientInfo);
            children.Add(A.ZY.InPatientNo, "住院患者信息", typeof(InPatientNoView), A.ZY.InPatientInfo);
            children.Add(A.ZY.InPatientInfo, "住院患者信息", typeof(InPatientInfoView), A.Home);

            children.Context = A.InDayDetailList_Context;
            children.Add(A.ZYYRQD.Date, "选择查询日期", typeof(InDailyDateView), A.ZYYRQD.DailyDetail);
            children.Add(A.ZYYRQD.DailyDetail, "住院一日清单", typeof(InDailyDetailView), A.Home);

            children.Context = A.IpRecharge_Context;
            children.Add(A.ZYCZ.RechargeWay, "选择支付方式", typeof(Default.Clinic.Component.ZYRecharge.Views.MethodView), A.ZYCZ.InputAmount);
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

            children.Context = A.RealAuth_Context;
            children.Add(A.SMRZ.Card, "插入就诊卡", typeof(Default.Component.RealAuth.Views.CardView), A.SMRZ.PatientInfo);
            children.Add(A.SMRZ.PatientInfo, "密码校验", typeof(Default.Component.RealAuth.Views.PatientInfoView),
                A.SMRZ.IDCard);
            children.Add(A.SMRZ.IDCard, "刷身份证", typeof(Default.Component.RealAuth.Views.IDCardView), A.Home);

            return true;
        }


        /// <summary>
        ///     向主程序提供配置信息，优先采用插件内部配置
        /// </summary>
        /// <returns>
        ///     返回配置文件完整路径(支持xml,json,ini)
        /// </returns>
        public override string[] UseConfigPath()
        {
            CfgModify();
            return new[] { "CurrentResource\\余杭妇幼保健院.xml" };
        }

        public override void AfterStartup()
        {
            base.AfterStartup();
            FrameworkConst.HospitalName = "余杭区妇幼保健院";
            var config = ServiceLocator.Current.GetInstance<IConfigurationManager>();
            LianZhongHisService.HisExePath = config.GetValue("HisExePath");
            try
            {
                var fileName = "QueueInfo.exe";
                var dirName = Path.Combine(FrameworkConst.RootDirectory, "QueueInfo");
                if (Directory.Exists(dirName) && File.Exists(Path.Combine(dirName, fileName)))
                {
                    var process = Process.GetProcesses().Any(p => p.ProcessName == fileName);
                    if (!process)
                    {
                        Process.Start(new ProcessStartInfo()
                        {
                            FileName = Path.Combine(dirName, fileName),
                            WorkingDirectory = dirName,
                        });
                    }
                }
            }
            catch (Exception e)
            {

            }
        }

        public override List<Uri> GetResourceDictionaryUris()
        {
            var uris = base.GetResourceDictionaryUris();
            //"pack://application:,,,/YuanTu.Default.Clinic;component/Theme/default.xaml"
            uris.Add(new Uri("pack://application:,,,/YuanTu.YuHangFYBJY;component/Theme/default.xaml"));
            return uris;
        }

        private void CfgModify()
        {
            try
            {
                var modifyVersion = "1";
                var fpath = Path.Combine(FrameworkConst.RootDirectory, "CurrentResource\\余杭妇幼保健院.xml");
                if (File.Exists(fpath))
                {
                    var content = File.ReadAllText(fpath, Encoding.UTF8);
                    if (!content.Contains($"<!--modifyVersion{modifyVersion}-->"))
                    {
                        var ret = content
                                .Replace("<Name>自助挂号</Name>", "<Name>当天挂号</Name>")
                                .Replace("<Name>自助预约</Name>", "<Name>预约挂号</Name>")
                                .Replace("<Name>自助取号</Name>", "<Name>预约取号</Name>")
                                .Replace("<Name>自助缴费</Name>", "<Name>费用缴纳</Name>")
                            ;
                        if (ret.Contains("modifyVersion"))
                        {
                            ret = Regex.Replace(ret, "\\<!--modifyVersion\\.*\\--\\>", $"<!--modifyVersion{modifyVersion}-->");
                        }
                        else
                        {
                            ret = ret.Replace("<root>", $"<root>\n\t<!--modifyVersion{modifyVersion}-->");
                        }
                        File.WriteAllText(fpath, ret, Encoding.UTF8);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);

            }

        }
    }
}
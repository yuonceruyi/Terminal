using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
using YuanTu.Devices.CashBox;
using YuanTu.JiaShanHospital.ISO8583;
using YuanTu.JiaShanHospital.ISO8583.External;
using YuanTu.JiaShanHospital.NativeServices;

namespace YuanTu.JiaShanHospital
{
    public class Startup : DefaultStartup
    {
        /// <summary>
        ///     优先级，数值越小优先级越高，内部配置越优先被使用
        /// </summary>
        public override int Order => 1;

        public override bool RegisterTypes(ViewCollection children)
        {
            children.Add(A.Home, null, "主页", typeof(YuanTu.Default.Component.Views.ChoiceView));
            children.Add(A.AdminPart, null, "后台", typeof(YuanTu.Default.Part.Views.AdminPageView));
            children.Add(A.Maintenance, null, "维护", typeof(YuanTu.Default.Part.Views.MaintenanceView));
            children.Add(A.Third.PosUnion, null, "刷卡", typeof(Component.Tools.Views.PosView));
            children.Add(A.Third.Cash, null, "投币", typeof(CashView));
            children.Add(A.Third.AtmCash, null, "现金缴住院押金", typeof(HatmView));
            children.Add(A.Third.JCMCash, null, "现金缴住院押金", typeof(JCMView));
            children.Add(A.Third.ScanQrCode, null, "扫码", typeof(Component.Tools.Views.ScanQrCodeView));

            children.Context = A.ChaKa_Context;
            children.Add(A.CK.Select, "个人信息", typeof(SelectTypeView), A.CK.Choice);
            children.Add(A.CK.Choice, "个人信息", typeof(YuanTu.Default.Component.Auth.Views.ChoiceView), A.CK.Card);
            children.Add(A.CK.Card, "个人信息", typeof(Component.Auth.Views.CardView), A.CK.Info);
            children.Add(A.CK.IDCard, "个人信息", typeof(IDCardView), A.CK.Info);
            children.Add(A.CK.HICard, "个人信息", typeof(Component.Auth.Views.SiCardView), A.CK.Info);
            children.Add(A.CK.QrCode, "个人信息", typeof(QrCodeView), A.CK.Info);
            children.Add(A.CK.Info, "个人信息", typeof(Component.Auth.Views.PatientInfoView), A.Home);
            children.Add(A.CK.InfoEx, "个人信息", typeof(PatientInfoExView), A.Home);

            children.Context = A.JianDang_Context;
            children.Add(AInner.JD.Confirm, "确认支付信息", typeof(ConfirmView), A.JD.Print);
            children.Add(Guid.NewGuid().ToString(), "支付卡费", null, A.JD.Print);
            children.Add(A.JD.Print, "建档结果", typeof(PrintView), A.Home);

            children.Context = A.XianChang_Context;
            children.Add(A.XC.Wether, "选择挂号类别", typeof(RegTypesView), A.XC.Dept);
            children.Add(A.XC.Dept, "选择挂号科室", typeof(DeptsView), A.XC.Doctor);
            children.Add(A.XC.Doctor, "选择医生排班", typeof(DoctorView), A.XC.Schedule);
            children.Add(A.XC.Schedule, "选择医生排班", typeof(ScheduleView), A.XC.Confirm);
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
            children.Add(A.QH.TakeNum, "确认取号信息", typeof(Component.TakeNum.Views.TakeNumView), A.QH.Confirm);
            children.Add(A.QH.Confirm, "确认取号支付", typeof(ConfirmView), A.QH.Print);
            children.Add(A.QH.Print, "取号凭条", typeof(PrintView), A.Home);

            children.Context = A.JiaoFei_Context;
            //children.Add(A.JF.BillRecord, "待缴费信息", typeof(BillRecordView), A.JF.Confirm);
            children.Add(A.JF.Confirm, "确认结算支付", typeof(ConfirmView), A.JF.Print);
            children.Add(A.JF.Print, "缴费完成", typeof(PrintView), A.Home);

            children.Context = A.ChongZhi_Context;
            children.Add(A.CZ.RechargeWay, "选择充值方式", typeof(RechargeMethodView), A.CZ.InputAmount);
            children.Add(A.CZ.InputAmount, "输入充值金额", typeof(InputAmountView), A.CZ.Print);
            //children.Add(Guid.NewGuid().ToString(), "投币/刷卡/扫码", null, A.CZ.Print);
            children.Add(A.CZ.Print, "充值完成", typeof(PrintView), A.Home);

            children.Add(A.QueryChoice, null, "查询选择", typeof(QueryChoiceView));

            children.Context = A.PayCostQuery;
            children.Add(A.JFJL.Date, "选择查询日期", typeof(YuanTu.Default.Component.InfoQuery.Views.DateTimeView), A.JFJL.PayCostRecord);
            children.Add(A.JFJL.PayCostRecord, "已缴费信息", typeof(Default.Component.InfoQuery.Views.PayCostRecordView), A.Home);

            children.Context = A.MedicineQuery;
            children.Add(A.YP.Query, "输入查询条件", typeof(InputView), A.YP.Medicine);
            children.Add(A.YP.Medicine, "药品信息列表", typeof(MedicineItemsView), A.Home);

            children.Context = A.ChargeItemsQuery;
            children.Add(A.XM.Query, "输入查询条件", typeof(InputView), A.XM.ChargeItems);
            children.Add(A.XM.ChargeItems, "诊疗项列表", typeof(ChargeItemsView), A.Home);

            children.Context = A.DiagReportQuery;
            children.Add(A.JYJL.Date, "确认病人信息", typeof(Default.Component.InfoQuery.Views.DateTimeView), A.JYJL.DiagReport);
            children.Add(A.JYJL.DiagReport, "等待打印", typeof(DiagReportView), A.Home);

            children.Context = A.PacsReportQuery;
            children.Add(A.YXBG.Date, "选择查询日期", typeof(Default.Component.InfoQuery.Views.DateTimeView), A.YXBG.PacsReport);
            children.Add(A.YXBG.PacsReport, "影像报告信息", typeof(PacsReportView), A.Home);

            children.Context = A.ReChargeQuery;
            children.Add(A.CZJL.Date, "选择查询日期", typeof(Default.Component.InfoQuery.Views.DateTimeView), A.CZJL.ReChargeRecord);
            children.Add(A.CZJL.ReChargeRecord, "预交金记录信息", typeof(RechargeRecordView), A.Home);

            children.Context = A.ZhuYuan_Context;
            children.Add(A.ZY.Choice, "个人信息", typeof(YuanTu.Default.Component.Auth.Views.ChoiceView), A.ZY.Card);
            children.Add(A.ZY.Card, "个人信息", typeof(InCardView), A.ZY.InPatientInfo);
            children.Add(A.ZY.IDCard, "个人信息", typeof(InIDCardView), A.ZY.InPatientInfo);
            children.Add(A.ZY.HICard, "个人信息", typeof(InSiCardView), A.ZY.InPatientInfo);
            //children.Add(A.ZY.InPatientNo, "住院患者信息", typeof(InPatientNoView), A.ZY.InPatientInfo);
            children.Add(A.ZY.InPatientNo, "住院患者信息", typeof(Component.Auth.Views.InPatientNoView), A.ZY.InPatientInfo);
            children.Add(A.ZY.InPatientInfo, "住院患者信息", typeof(Component.Auth.Views.InPatientInfoView), A.Home);

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
            children.Add(A.JFBD.Date, "选择查询日期", typeof(Default.Component.PrintAgain.Views.DateTimeView), A.JFBD.PayCostRecord);
            children.Add(A.JFBD.PayCostRecord, "已缴费信息", typeof(Default.Component.PrintAgain.Views.PayCostRecordView), A.Home);
            children.Add(A.JFBD.Print, "补打完成", typeof(PrintView), A.Home);

            children.Context = A.RealAuth_Context;
            children.Add(A.SMRZ.Card, "插入就诊卡", typeof(Default.Component.RealAuth.Views.CardView), A.SMRZ.PatientInfo);
            children.Add(A.SMRZ.PatientInfo, "密码校验", typeof(Default.Component.RealAuth.Views.PatientInfoView), A.SMRZ.IDCard);
            children.Add(A.SMRZ.IDCard, "刷身份证", typeof(Default.Component.RealAuth.Views.IDCardView), A.Home);

            return true;
        }


        public override void AfterStartup()
        {
            if (FrameworkConst.DeviceType == "YT-740")
                HATM.Initialize();
            FrameworkConst.HospitalName = "嘉善第一人民医院";
            var config = ServiceLocator.Current.GetInstance<IConfigurationManager>();
            Instance.AbcIp = config.GetValue("AbcPos:IP");
            Instance.AbcPort = config.GetValue("AbcPos:Port");
            Instance.AbcTerminalId = config.GetValue("AbcPos:TerminalId");
            Instance.AbcMerchantId = config.GetValue("AbcPos:MerchantId");
            Instance.AbcTPDU = config.GetValue("AbcPos:TPDU");
            //Logger.Net.Info($"IP：{Instance.AbcIp}，Port:{Instance.AbcPort},TerminalId:{Instance.AbcTerminalId},MerchantId:{Instance.AbcMerchantId},TPDU:{Instance.AbcTPDU}");
            LianZhongHisService.HisExePath = config.GetValue("LianZhongHisExePath");
            KeyBoard_ZT.Port = short.Parse(config.GetValue("ZT:Port"));
            KeyBoard_ZT.Baud = config.GetValueInt("ZT:Baud");
            ACT_A6_V2.Port = config.GetValueInt("Act_A6:Port");
            ACT_A6_V2.Baud = config.GetValueInt("Act_A6:Baud");
        }

        public override string[] UseConfigPath()
        {
            var file = Path.Combine(FrameworkConst.RootDirectory, "CurrentResource", "ConfigJiaShan.xml");
            return new[] { file };
        }

        public override List<Uri> GetResourceDictionaryUris()
        {
            var uris = base.GetResourceDictionaryUris();
            uris.Add(new Uri("pack://application:,,,/YuanTu.JiaShanHospital;component/Component/Views/RegTypesTemplates.xaml"));
            uris.Add(new Uri("pack://application:,,,/YuanTu.JiaShanHospital;component/Component/Views/RegTypesInfoCard.xaml"));
            return uris;
        }
    }
}

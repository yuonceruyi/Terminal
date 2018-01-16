using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Practices.ServiceLocation;
using YuanTu.Consts;
using YuanTu.Consts.Gateway;
using YuanTu.Consts.Models.Print;
using YuanTu.Consts.Services;
using YuanTu.Core.FrameworkBase;
using YuanTu.Core.Gateway;
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
using YuanTu.YiWuArea;
using YuanTu.YiWuArea.Insurance.Services;

namespace YuanTu.YiWuFuBao
{
    public class Startup : YuanTu.Default.Startup
    {
        #region Overrides of DefaultStartup

        /// <summary>
        /// 注册视图引擎
        /// </summary>
        /// <param name="collection"/>
        /// <returns/>
        public override bool RegisterTypes(ViewCollection children)
        {
            children.Add(A.Home, null, "主页", typeof(YuanTu.YiWuFuBao.Component.Views.ChoiceView));
            children.Add(A.AdminPart, null, "后台", typeof(YuanTu.Default.Part.Views.AdminPageView));
            children.Add(A.Maintenance, null, "维护", typeof(YuanTu.Default.Part.Views.MaintenanceView));

            children.Add(A.Third.PosUnion, null, "刷银行卡", typeof(PosView));
            children.Add(A.Third.Cash, null, "塞入纸币", typeof(CashView));
            children.Add(A.Third.ScanQrCode, null, "扫码支付", typeof(ScanQrCodeView));
            children.Add(A.Third.AtmCash, null, "现金缴住院押金", typeof(HatmView));

            children.Context = A.ChaKa_Context;
            children.Add(A.CK.Select, "个人信息", typeof(SelectTypeView), A.CK.Choice);
            children.Add(A.CK.Choice, "个人信息", typeof(YuanTu.Default.Component.Auth.Views.ChoiceView), A.CK.Card);
            children.Add(A.CK.Card, "个人信息", typeof(YuanTu.YiWuFuBao.Component.Auth.Views.CardView), A.CK.Info);
            children.Add(A.CK.IDCard, "个人信息", typeof(IDCardView), A.CK.Info);
            children.Add(A.CK.HICard, "个人信息", typeof(SiCardView), A.CK.Info);
            children.Add(A.CK.Info, "个人信息", typeof(PatientInfoView), A.Home);
            children.Add(A.CK.InfoEx, "个人信息", typeof(YuanTu.YiWuFuBao.Component.Auth.Views.PatientInfoExView), A.Home);

            children.Context = A.JianDang_Context;
            children.Add(AInner.JD.Confirm, "确认支付信息", typeof(ConfirmView), A.JD.Print);
            children.Add(A.JD.Print, "建档结果", typeof(PrintView), A.Home);

            children.Context = A.XianChang_Context;
            children.Add(A.XC.Wether, "选择挂号类别", typeof(RegTypesView), A.XC.AMPM);
            children.Add(A.XC.AMPM, "选择挂号场次", typeof(RegAmPmView), A.XC.Dept);
            children.Add(A.XC.Dept, "选择挂号科室", typeof(DeptsView), A.XC.Doctor);
            children.Add(A.XC.Doctor, "选择医生排班", typeof(DoctorView), A.XC.Schedule);
            children.Add(A.XC.Schedule, "选择医生排班", typeof(ScheduleView), A.XC.Confirm);
            children.Add(A.XC.Confirm, "选择支付方式", typeof(ConfirmView), A.XC.Print);
            children.Add(A.XC.Print, "挂号凭条", typeof(PrintView), A.Home);



            children.Context = A.YuYue_Context;
            children.Add(A.YY.Date, "选择预约日期", typeof(RegDateView), A.YY.Wether);
            children.Add(A.YY.Wether, "选择预约类别", typeof(RegTypesView), A.YY.AMPM);
            children.Add(A.YY.AMPM, "选择挂号场次", typeof(RegAmPmView), A.YY.Dept);

            children.Add(A.YY.Dept, "选择预约科室", typeof(DeptsView), A.YY.Doctor);
            children.Add(A.YY.Doctor, "选择医生排班", typeof(DoctorView), A.YY.Schedule);
            children.Add(A.YY.Schedule, "选择医生排班", typeof(ScheduleView), A.YY.Confirm);
            children.Add(A.YY.Time, "选择医生排班", typeof(SourceView), A.YY.Confirm);
            children.Add(A.YY.Confirm, "确认预约信息", typeof(ConfirmView), A.YY.Print);
            children.Add(A.YY.Print, "预约凭条", typeof(PrintView), A.Home);

            children.Context = A.QuHao_Context;
            children.Add(A.QH.Record, "选择预约记录", typeof(ApptRecordView), A.QH.TakeNum);
            children.Add(A.QH.TakeNum, "确认取号信息", typeof(TakeNumView), A.QH.Confirm);
            children.Add(A.QH.Confirm, "确认取号支付", typeof(ConfirmView), A.QH.Print);
            children.Add(A.QH.Print, "取号凭条", typeof(PrintView), A.Home);

            children.Context = A.JiaoFei_Context;
            children.Add(A.JF.BillRecord, "待缴费信息", typeof(BillRecordView), A.JF.Confirm);
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
            children.Add(A.YP.Query, "输入查询条件", typeof(InputView), A.YP.Medicine);
            children.Add(A.YP.Medicine, "药品信息列表", typeof(MedicineItemsView), A.Home);

            children.Context = A.ChargeItemsQuery;
            children.Add(A.XM.Query, "输入查询条件", typeof(InputView), A.XM.ChargeItems);
            children.Add(A.XM.ChargeItems, "诊疗项列表", typeof(ChargeItemsView), A.Home);


            children.Context = A.ZhuYuan_Context;
            children.Add(A.ZY.InPatientNo, "个人信息", typeof(InPatientNoView), A.ZY.InPatientInfo);
            children.Add(A.ZY.InPatientInfo, "个人信息", typeof(YuanTu.YiWuFuBao.Component.Auth.Views.InPatientInfoView), A.Home);

            children.Context = A.InDayDetailList_Context;
            children.Add(A.ZYYRQD.Date, "选择查询日期", typeof(InDailyDateView), A.ZYYRQD.DailyDetail);
            children.Add(A.ZYYRQD.DailyDetail, "住院一日清单", typeof(InDailyDetailView), A.Home);

            children.Context = A.IpRecharge_Context;
            children.Add(A.ZYCZ.RechargeWay, "选择支付方式", typeof(MethodView), A.ZYCZ.InputAmount);
            children.Add(A.ZYCZ.InputAmount, "输入缴纳金额", typeof(ZYInputAmountView), A.ZYCZ.Print);
            children.Add(A.ZYCZ.Print, "缴押金完成", typeof(PrintView), A.Home);

            children.Context = A.DiagReportQuery;
            //children.Add(A.JYJL.Date, "选择查询日期", typeof(DateTimeView), A.JYJL.DiagReport);
            //children.Add(A.JYJL.DiagReport, "检验报告信息", typeof(YuanTu.YiWuFuBao.Component.InfoQuery.Views.DiagReportView), A.Home);
            children.Add(AInner.JYJL.Print, "打印报告单", typeof(PrintView), A.Home);

            children.Context = AInner.ChuYuanBillpay_Context;
            children.Add(AInner.ChuYuan.Confirm, "确认金额", typeof(ConfirmView), AInner.ChuYuan.Print);
            children.Add(AInner.ChuYuan.Print, "出院完成", typeof(PrintView), A.Home);


            if (CurrentStrategyType() == DeviceType.Clinic)
            {
                children.Add(A.Home, null, "主页", typeof(YuanTu.Default.Clinic.Component.Views.ChoiceView));
            }

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
            DataHandlerEx.Handler = new YuanTu.YiWuArea.Gateway.DataHandler();
            var arr = new[] { "CurrentResource\\义乌妇保.json", "CurrentResource\\义乌妇保.xml" };
            try
            {
                var fpath = Path.Combine(FrameworkConst.RootDirectory, "CurrentResource\\义乌妇保.xml");
                if (File.Exists(fpath))
                {
                    var haschange = false;
                    var content = File.ReadAllText(fpath, Encoding.UTF8);
                    if (content.Contains("<!--就诊场次 Mod-->"))
                    {
                        haschange = true;
                        content = Regex.Replace(content, "\\<AmPmSession\\>[\\s\\S]*\\</AmPmSession\\>", "");
                        content = content.Replace("<!--就诊场次 Mod-->", @"
  <!--就诊场次 Mod2-->
  <AmPmSession>
    <上午>
      <Order>0</Order>
      <Enable>1</Enable>
      <Visiable>1</Visiable>
      <ImageName>科室类别_专家门诊</ImageName>
      <Color>248,180,117</Color>
      <StartTime>07:00</StartTime>
      <EndTime>11:30</EndTime>
      <Remark>当天07:00-11:30</Remark>
    </上午>
    <下午>
      <Order>0</Order>
      <Enable>1</Enable>
      <Visiable>1</Visiable>
      <ImageName>科室类别_专家门诊</ImageName>
      <Color>248,180,117</Color>
      <Remark>当天13:00-16:30</Remark>
      <StartTime>07:00</StartTime>
      <EndTime>16:30</EndTime>
    </下午>
  </AmPmSession>");
                    }
                    if (content.Contains("<StartTime>07:00</StartTime>"))
                    {
                        haschange = true;
                        content = content.Replace("<StartTime>07:00</StartTime>", "<StartTime>06:30</StartTime>");
                    }

                    if (content.Contains("<主治医生>"))
                    {
                        haschange = true;
                        content = Regex.Replace(content, "\\<主治医生\\>[\\s\\S]*\\</副主任医生\\>", "");

                    }
                    if (!content.Contains("<特需>"))
                    {
                        haschange = true;
                        content = content.Replace("</RegType>", @"
    <特需>
      <Name>特需</Name>
      <Order>3</Order>
      <Visabled>1</Visabled>
      <SearchDoctor>0</SearchDoctor>
      <ImageName>科室类别_知名专家</ImageName>
      <Color>162,169,239</Color>
      <Remark>具备高超技术的特需专家来帮助您</Remark>
    </特需>
    <名医>
      <Name>名医</Name>
      <Order>4</Order>
      <Visabled>1</Visabled>
      <SearchDoctor>0</SearchDoctor>
      <ImageName>科室类别_知名专家</ImageName>
      <Color>162,169,239</Color>
      <Remark>具备高超技术的知名专家来帮助您</Remark>
    </名医>
    <义诊>
      <Name>义诊</Name>
      <Order>4</Order>
      <Visabled>1</Visabled>
      <SearchDoctor>0</SearchDoctor>
      <ImageName>科室类别_知名专家</ImageName>
      <Color>162,169,239</Color>
      <Remark>尽义务无偿给人诊察治病</Remark>
    </义诊>
</RegType>
");

                    }
                    if (!content.Contains("<专科门诊>"))
                    {
                        haschange = true;
                        content = content.Replace("</RegType>", @" <专科门诊>
      <Name>专科门诊</Name>
      <Order>4</Order>
      <Visabled>1</Visabled>
      <SearchDoctor>0</SearchDoctor>
      <ImageName>科室类别_知名专家</ImageName>
      <Color>162,169,239</Color>
      <Remark>特定类型疾病的专门治疗门诊</Remark>
    </专科门诊>
</RegType>
");
                    }
                    if (!content.Contains("<夜间特需_义乌妇保>"))
                    {
                        haschange = true;
                        content = content.Replace("</RegType>", @"     <夜间特需_义乌妇保>
      <Name>夜间特需</Name>
      <Order>6</Order>
      <Visabled>1</Visabled>
      <SearchDoctor>0</SearchDoctor>
      <ImageName>科室类别_急诊门诊</ImageName>
      <Color>245,145,145</Color>
      <Remark>用于夜间特需门诊</Remark>
    </夜间特需_义乌妇保>
</RegType>
");
                    }
                    if (haschange)
                    {
                        File.WriteAllText(fpath, content, Encoding.UTF8);

                    }
                }

            }
            catch (Exception ex)
            {


            }
            return arr;
        }

        public override List<Uri> GetResourceDictionaryUris()
        {
            var uris = base.GetResourceDictionaryUris();
            uris.Add(new Uri("pack://application:,,,/YuanTu.YiWuFuBao;component/Theme/default.xaml"));
            return uris;
        }

        public override void AfterStartup()
        {
            FrameworkConst.HospitalName = "义乌市妇幼保健院";
            var config = ServiceLocator.Current.GetInstance<IConfigurationManager>();
            YiWuAreaConsts.SiPlatformUrl = config.GetValue("SiPlatform:Url")?.TrimEnd('/');
            YiWuAreaConsts.SiOperatorNo = config.GetValue("SiPlatform:SiOperatorNo");
            YiWuAreaConsts.SiHospitalCode = config.GetValue("SiPlatform:SiHospitalCode");

            YiWuFuBaoYuanConnst.IsEnable = config.GetValueInt("OldCardSupport:IsEnable") == 1;
            YiWuFuBaoYuanConnst.CanScreenInput = config.GetValueInt("OldCardSupport:CanScreenInput") == 1;
            YiWuFuBaoYuanConnst.ScreenInputText = config.GetValue("OldCardSupport:ScreenInputText");

            //FrameworkConst.EnableUploadTradeInfo = true;//上传流水


        }

        public override Dictionary<string, string[]> GetStrategy()
        {
            var strage = base.GetStrategy();
            strage["13471-16L"] = new[] { DeviceType.Default };
            return strage;
        }

        #endregion
    }
}

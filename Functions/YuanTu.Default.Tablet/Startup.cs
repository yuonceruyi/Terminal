using System.Threading.Tasks;
using Microsoft.Practices.ServiceLocation;
using YuanTu.Consts;
using YuanTu.Consts.UserCenter;
using YuanTu.Core.FrameworkBase;
using YuanTu.Core.Models;
using YuanTu.Core.Navigating;
using YuanTu.Core.Reporter.Kestrel;
using YuanTu.Core.Systems;
using YuanTu.Default.Component.Auth.Views;
using YuanTu.Default.Component.Register.Views;
using YuanTu.Default.Component.Tools.Views;
using YuanTu.Default.Component.Views;
using ChoiceView = YuanTu.Default.Component.Auth.Views.ChoiceView;
using YuanTu.Default.Tablet.Component.Register.Views;

namespace YuanTu.Default.Tablet
{
    public class Startup : DefaultStartup
    {
        /// <summary>
        ///     优先级，数值越小优先级越高，内部配置越优先被使用
        /// </summary>
        public override int Order => int.MaxValue;


        public override bool RegisterTypes(ViewCollection children)
        {
            children.Add(A.Home, null, "主页", typeof(YuanTu.Default.Component.Views.ChoiceView));
            children.Add(A.AdminPart, null, "后台", typeof(YuanTu.Default.Part.Views.AdminPageView));
            children.Add(A.Maintenance, null, "维护", typeof(YuanTu.Default.Part.Views.MaintenanceView));
            children.Add(A.Third.PosUnion, null, "刷卡", typeof(PosView));
            children.Add(A.Third.ScanQrCode, null, "扫码", typeof(ScanQrCodeView));
            children.Add(AInner.SY.Choice, null, "选择业务", typeof(Component.Cashier.Views.ChoiceView));

            children.Context = A.ChaKa_Context;
            children.Add(A.CK.Choice, "个人信息", typeof(ChoiceView), A.CK.Card);
            children.Add(A.CK.Card, "个人信息", typeof(Default.Tablet.Component.Auth.Views.CardView), A.CK.Info);
            children.Add(A.CK.IDCard, "个人信息", typeof(IDCardView), A.CK.Info);
            children.Add(A.CK.HICard, "个人信息", typeof(SiCardView), A.CK.Info);
            children.Add(A.CK.Info, "个人信息", typeof(PatientInfoView), A.Home);
            children.Add(A.CK.InfoEx, "个人信息", typeof(PatientInfoExView), A.Home);

            children.Context = A.XianChang_Context;
            children.Add(AInner.XC.Hospitals, "选择医院", typeof(HospitalsView), A.XC.Dept);
            children.Add(A.XC.Dept, "选择科室", typeof(DeptsView), A.XC.Schedule);
            children.Add(A.XC.Schedule, "选择排班", typeof(ScheduleView), A.XC.Confirm);
            children.Add(A.XC.Confirm, "选择支付方式", typeof(ConfirmView), A.XC.Print);
            children.Add(A.XC.Print, "挂号结果", typeof(PrintView), A.Home);

            children.Context = A.YuYue_Context;
            children.Add(AInner.YY.Hospitals, "选择医院", typeof(HospitalsView), A.YY.Dept);
            children.Add(A.YY.Dept, "选择科室", typeof(DeptsView), A.YY.Schedule);
            children.Add(A.YY.Schedule, "选择排班", typeof(ScheduleView), A.YY.Confirm);
            children.Add(A.YY.Time, "选择时间", typeof(SourceView), A.YY.Confirm);
            children.Add(A.YY.Confirm, "确认信息", typeof(ConfirmView), A.YY.Print);
            children.Add(A.YY.Print, "预约结果", typeof(PrintView), A.Home);

            children.Context = AInner.Sale;
            children.Add(AInner.SY.Amount, "输入金额", typeof(Component.Cashier.Views.AmountView), AInner.SY.Card);
            children.Add(AInner.SY.Card, "刷卡", typeof(Component.Cashier.Views.CardView), AInner.SY.Print);
            children.Add(AInner.SY.Scan, "扫码", typeof(Component.Cashier.Views.ScanView), AInner.SY.Print);
            children.Add(AInner.SY.Print, "支付结果", typeof(Component.Cashier.Views.PrintView), A.Home);

            children.Context = AInner.Refund;
            children.Add(AInner.SY.Card, "刷卡", typeof(Component.Cashier.Views.CardView), AInner.SY.Select);
            children.Add(AInner.SY.Select, "选择订单", typeof(Component.Cashier.Views.SelectView), AInner.SY.Confirm);
            children.Add(AInner.SY.Confirm, "确认订单", typeof(Component.Cashier.Views.ConfirmView), AInner.SY.Print);
            children.Add(AInner.SY.Print, "退款结果", typeof(Component.Cashier.Views.PrintView), A.Home);

            return true;
        }

        public override void AfterStartup()
        {
            Task.Factory.StartNew(() =>
            {
                //获取token
                var req = new req获取deviceSecret
                {
                    deviceMac = NetworkManager.MAC
                };
                var res = DataHandlerEx.获取deviceSecret(req, FrameworkConst.DeviceUrl);

                if (res.success)
                {
                    var req2 = new req获取token
                    {
                        deviceSecret = res.data
                    };
                    var res2 = DataHandlerEx.获取token(req2, FrameworkConst.DeviceUrl);
                    FrameworkConst.Token = res2.data;
                }
            });
            Server.AddMiddlewares(typeof(GetCardMiddleware));
            ServiceLocator.Current.GetInstance<ITopBottomModel>().MainTitle = "「区域诊疗一号通」";
        }

        public override string[] UseConfigPath()
        {
            return new[] { "CurrentResource\\Tablet\\Config.xml" };
        }
    }
}
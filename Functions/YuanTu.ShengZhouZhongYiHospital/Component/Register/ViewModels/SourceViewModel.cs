using Microsoft.Practices.ServiceLocation;
using Microsoft.Practices.Unity;
using Prism.Commands;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using YuanTu.Consts;
using YuanTu.Consts.Dtos.UnionPos;
using YuanTu.Consts.Enums;
using YuanTu.Consts.FrameworkBase;
using YuanTu.Consts.Gateway;
using YuanTu.Consts.Models;
using YuanTu.Consts.Models.Auth;
using YuanTu.Consts.Models.Payment;
using YuanTu.Consts.Models.Print;
using YuanTu.Consts.Models.Register;
using YuanTu.Consts.Services;
using YuanTu.Consts.Sounds;
using YuanTu.Core.Extension;
using YuanTu.Core.FrameworkBase;
using YuanTu.Core.FrameworkBase.Loadings;
using YuanTu.Core.Log;
using YuanTu.Core.Navigating;
using YuanTu.Core.Reporter;
using YuanTu.Core.Services.ConfigService;
using YuanTu.Core.Services.PrintService;
using YuanTu.Default.Component.Tools.Models;
using YuanTu.ShengZhouZhongYiHospital.Extension;
using YuanTu.ShengZhouZhongYiHospital.HisNative.Models;

namespace YuanTu.ShengZhouZhongYiHospital.Component.Register.ViewModels
{
    public class SourceViewModel : YuanTu.Default.Component.Register.ViewModels.SourceViewModel
    {

        public override string Title => "选择预约号源";
        #region IOC
        [Dependency]
        public IPatientModel PatientModel { get; set; }

        [Dependency]
        public IChoiceModel ChoiceModel { get; set; }

        [Dependency]
        public ISourceModel SourceModel { get; set; }

        [Dependency]
        public IScheduleModel ScheduleModel { get; set; }

        [Dependency]
        public IDeptartmentModel DepartmentModel { get; set; }
        [Dependency]
        public IDoctorModel DoctorModel { get; set; }
        [Dependency]
        public IRegTypesModel RegTypesModel { get; set; }
        [Dependency]
        public IPaymentModel PaymentModel { get; set; }
        [Dependency]
        public IRegDateModel RegDateModel { get; set; }
        [Dependency]
        public IRegisterModel RegisterModel { get; set; }
        [Dependency]
        public ICardModel CardModel { get; set; }
        [Dependency]
        public IConfigurationManager ConfigurationManager { get; set; }
        [Dependency]
        public IExtraPaymentModel ExtraPaymentModel { get; set; }
        [Dependency]
        public IPrintModel PrintModel { get; set; }
        [Dependency]
        public IPrintManager PrintManager { get; set; }
        [Dependency]
        public IBusinessConfigManager BusinessConfigManager { get; set; }
        #endregion
        public override void OnEntered(NavigationContext navigationContext)
        {
            ChangeNavigationContent("");
            var confirmCommand = new DelegateCommand<Info>(Confirm);
            var list = SourceModel.Res号源明细查询.data.Select(p => new InfoMore
            {

                Title = p.appoNo,
                SubTitle = p.restNum,
                Tag = p,
                ConfirmCommand = confirmCommand,
                TemplateKey = "InfoItemMore2"
            });
            Data = new ObservableCollection<InfoMore>(list);
            PlaySound(ChoiceModel.Business == Business.挂号 ? SoundMapping.选择挂号号源 : SoundMapping.选择预约号源);
        }
        protected override void Confirm(Info i)
        {

            SourceModel.所选号源 = i.Tag.As<号源明细>();
            ChangeNavigationContent(i.Title);
            PaymentModel.Self = decimal.Parse(DepartmentModel.所选科室.deptIntro) * 100;
            PaymentModel.Insurance = decimal.Parse("0");
            PaymentModel.Total = decimal.Parse(DepartmentModel.所选科室.deptIntro) * 100;
            PaymentModel.NoPay = true; //默认预约或者自费金额为0时不支付            
            PaymentModel.ConfirmAction = Confirm;

            PaymentModel.LeftList = new List<PayInfoItem>()
                {
                    new PayInfoItem("日期：", ChoiceModel.Business == Business.挂号
                        ? DateTimeCore.Today.ToString("yyyy-MM-dd")
                        : RegDateModel.RegDate),
                    new PayInfoItem("时间：", SourceModel.所选号源.appoNo),
                    new PayInfoItem("科室：", DepartmentModel.所选科室.deptName),
                };

            PaymentModel.RightList = new List<PayInfoItem>()
                {
                    new PayInfoItem("个人支付：", PaymentModel.Self.In元()),
                    new PayInfoItem("支付金额：", PaymentModel.Total.In元(), true),
                };
            Next();

        }


        protected Result Confirm()
        {
            return DoCommand(lp =>
            {
                var rmModel = RegisterModel as Models.RegisterModel;
                var scheduleId = RegTypesModel.SelectRegType.RegType == RegType.专家门诊
                || RegTypesModel.SelectRegType.RegType == RegType.夜间特需
                || RegTypesModel.SelectRegType.RegType == RegType.专科专家 ? ScheduleModel.所选排班.scheduleId : DepartmentModel.所选科室.parentDeptName;
                var treatfee = DepartmentModel.所选科室.fullPy.InRMB();
                var regAmount = DepartmentModel.所选科室.deptIntro.InRMB();
                lp.ChangeText("正在进行预约挂号，请稍候...");
                var patientInfo = PatientModel.当前病人信息;
                var scheduleInfo = ScheduleModel.所选排班;
                var deptInfo = DepartmentModel.所选科室;
                var sexCode = patientInfo.sex == "女" ? "2" : "1";
                RegisterModel.Req预约挂号 = new req预约挂号
                {
                    patientId = patientInfo.patientId,
                    cardType = ((int)CardModel.CardType).ToString(),
                    cardNo = CardModel.CardNo,
                    operId = FrameworkConst.OperatorId,
                    tradeMode = PaymentModel.PayMethod.GetEnumDescription(),
                    accountNo = patientInfo.patientId,
                    cash = PaymentModel.Total.ToString(),
                    regMode = ChoiceModel.Business == Business.挂号 ? "01" : "02",//嵊州 01挂号 02预约
                    regType = "0" + ((int)RegTypesModel.SelectRegType.RegType).ToString(),
                    medAmPm = SourceModel.所选号源?.extend,
                    medDate = ChoiceModel.Business == Business.挂号
                            ? DateTimeCore.Today.ToString("yyyy-MM-dd")
                            : RegDateModel.RegDate,
                    medTime = SourceModel.所选号源?.appoNo,
                    deptCode = deptInfo.deptCode,
                    deptName = deptInfo.deptName,
                    scheduleId = scheduleId,
                    doctCode = RegTypesModel.SelectRegType.RegType == RegType.专家门诊 ? scheduleInfo?.doctCode : "",
                    doctName = RegTypesModel.SelectRegType.RegType == RegType.专家门诊 ? scheduleInfo?.doctName : "",
                    extend = $"{patientInfo.name},{patientInfo.address},{SourceModel.所选号源?.medBegtime},{SourceModel.所选号源?.medEndtime},{patientInfo.phone},{sexCode},{patientInfo.birthday}, {patientInfo.idNo},"
                };

                FillRechargeRequest(RegisterModel.Req预约挂号);

                RegisterModel.Res预约挂号 = DataHandlerEx.预约挂号(RegisterModel.Req预约挂号);
                if (RegisterModel.Res预约挂号?.success ?? false)
                {
                    ExtraPaymentModel.Complete = true;
                    PrintModel.SetPrintInfo(true, new PrintInfo
                    {
                        TypeMsg = "预约成功",
                        TipMsg = $"您已成功于{DateTimeCore.Now:HH:mm}分" + "预约",
                        PrinterName = ConfigurationManager.GetValue("Printer:Receipt"),
                        Printables = AppointPrintables(),
                        TipImage = "提示_凭条"
                    });
                    Navigate(ChoiceModel.Business == Business.挂号 ? A.XC.Print : A.YY.Print);
                    return Result.Success();
                }
                //第三方支付失败时去支付流程里面处理，不在业务里面处理
                if (NavigationEngine.State != A.Third.PosUnion)
                {
                    PrintModel.SetPrintInfo(false, new PrintInfo
                    {
                        TypeMsg = "预约失败",
                        DebugInfo = RegisterModel.Res预约挂号?.msg
                    });
                    Navigate(ChoiceModel.Business == Business.挂号 ? A.XC.Print : A.YY.Print);
                }
                ExtraPaymentModel.Complete = true;
                return Result.Fail(RegisterModel.Res预约挂号?.code ?? -100, RegisterModel.Res预约挂号?.msg);

            }).Result;
        }

        protected void FillRechargeRequest(req预约挂号 req)
        {
            var extraPaymentModel = GetInstance<IExtraPaymentModel>();
            if (extraPaymentModel.CurrentPayMethod == PayMethod.银联)
            {
                var posinfo = extraPaymentModel.PaymentResult as TransResDto;
                if (posinfo != null)
                {
                    req.bankCardNo = posinfo.CardNo;
                    req.bankTime = posinfo.TransTime;
                    req.bankDate = posinfo.TransDate;
                    req.posTransNo = posinfo.Trace;
                    req.bankTransNo = posinfo.Ref;
                    req.deviceInfo = posinfo.TId;
                    req.sellerAccountNo = posinfo.MId;
                }
            }
            else if (extraPaymentModel.CurrentPayMethod == PayMethod.支付宝 || extraPaymentModel.CurrentPayMethod == PayMethod.微信支付)
            {
                var thirdpayinfo = extraPaymentModel.PaymentResult as 订单状态;
                if (thirdpayinfo != null)
                {
                    req.bankCardNo = thirdpayinfo.buyerAccount;
                }
            }
        }

        protected Queue<IPrintable> AppointPrintables()
        {
            var queue = PrintManager.NewQueue("（预约挂号凭条)");
            var register = RegisterModel.Res预约挂号.data;
            var department = DepartmentModel.所选科室;
            var schedule = ScheduleModel?.所选排班;
            var patientInfo = PatientModel.当前病人信息;
            var amPm = SourceModel.所选号源?.extend == "1" ? "上午" : "下午";// DepartmentModel?.所选科室?.parentDeptCode ScheduleModel?.所选排班.medAmPm == "1" ? "上午" : "下午";
            var sb = new StringBuilder();
            sb.Append($"------------------------------------\n");
            sb.Append($"姓    名：{patientInfo?.name}\n");
            sb.Append($"就诊卡号：{patientInfo?.patientId}\n");
            sb.Append($"类    别：{RegTypesModel.SelectRegType.RegType.GetRegType()?.Name}\n");
            sb.Append($"------------------------------------\n");
            sb.Append($"科    室：{department?.deptName}\n");
            if (RegTypesModel.SelectRegType.RegType == RegType.专家门诊)
            {
                sb.Append($"医    生：{schedule?.doctName}\n");
            }
            else
            {
                sb.Append($"医    生：无\n");
            }
            sb.Append($"就诊日期：{RegDateModel.RegDate}\n");
            sb.Append($"预约序号：{register.appoNo}({amPm})\n");
            sb.Append($"就诊时间：{SourceModel.所选号源?.appoNo}\n");
            var cardtype = CardModel?.CardType == CardType.社保卡 ? CardType.社保卡.ToString() : "自费卡";
            sb.Append($"卡 类 型：{cardtype}\n");
            sb.Append($"------------------------------------\n");
            sb.Append($"打印时间：{DateTimeCore.Now:yyyy-MM-dd   HH:mm:ss}\n");
            sb.Append($"机器号：{FrameworkConst.OperatorId}\n");
            sb.Append(SourceModel.所选号源?.extend == "1"
                ? $"请您在{RegDateModel.RegDate}上午10点前取号\n"
                : $"请您在{RegDateModel.RegDate}下午15点前取号\n");
            sb.Append($"过期不取视为违约，请见谅！\n");
            queue.Enqueue(new PrintItemText { Text = sb.ToString(), Font = new Font("微软雅黑", 9, FontStyle.Regular) });
            sb.Clear();
            if (!string.IsNullOrEmpty(patientInfo.patientId))
            {
                var image = BarCode128.GetCodeImage(patientInfo.patientId, BarCode.Code128.Encode.Code128A);
                queue.Enqueue(new PrintItemImage
                {
                    Align = ImageAlign.Center,
                    Image = image,
                    Height = image.Height / 2f,
                    Width = image.Width / 2f
                });
            }
            sb.Append(patientInfo?.patientId);
            queue.Enqueue(new PrintItemText { Text = sb.ToString(), StringFormat = PrintConfig.Center, Font = new Font("微软雅黑", 9, System.Drawing.FontStyle.Regular) });
            sb.Clear();
            sb.AppendLine(".");
            sb.AppendLine(".");
            queue.Enqueue(new PrintItemText { Text = sb.ToString(), Font = new Font("微软雅黑", 9, FontStyle.Regular) });
            return queue;
        }

        protected static readonly BarCode.Code128 BarCode128 = new BarCode.Code128
        {
            Magnify = 1,
            Height = 80
        };

        protected virtual void FillRechargeRequest(req预约挂号记录同步到his系统 req)
        {
            var extraPaymentModel = GetInstance<IExtraPaymentModel>();
            if (extraPaymentModel.CurrentPayMethod == PayMethod.银联)
            {
                var posinfo = extraPaymentModel.PaymentResult as TransResDto;
                if (posinfo != null)
                {
                    req.bankCardNo = posinfo.CardNo;
                    req.bankTime = posinfo.TransTime;
                    req.bankDate = posinfo.TransDate;
                    req.posTransNo = posinfo.Trace;
                    req.bankTransNo = posinfo.Ref;
                    req.deviceInfo = posinfo.TId;
                    req.sellerAccountNo = posinfo.MId;
                }
            }
            else if (extraPaymentModel.CurrentPayMethod == PayMethod.支付宝 || extraPaymentModel.CurrentPayMethod == PayMethod.微信支付)
            {
                var thirdpayinfo = extraPaymentModel.PaymentResult as 订单状态;
                if (thirdpayinfo != null)
                {
                    req.payAccountNo = thirdpayinfo.buyerAccount;
                    req.transNo = thirdpayinfo.outPayNo;
                    //req.outTradeNo = thirdpayinfo.outTradeNo;
                    req.tradeTime = thirdpayinfo.paymentTime;
                }
            }
        }

        protected virtual void FillRechargeRequest(req交易记录同步到his系统 req)
        {
            var extraPaymentModel = GetInstance<IExtraPaymentModel>();
            if (extraPaymentModel.CurrentPayMethod == PayMethod.银联)
            {
                var posinfo = extraPaymentModel.PaymentResult as TransResDto;
                if (posinfo != null)
                {
                    req.bankCardNo = posinfo.CardNo;
                    req.bankTime = posinfo.TransTime;
                    req.bankDate = posinfo.TransDate;
                    req.posTransNo = posinfo.Trace;
                    req.bankTransNo = posinfo.Ref;
                    req.deviceInfo = posinfo.TId;
                    req.sellerAccountNo = posinfo.MId;
                }
            }
            else if (extraPaymentModel.CurrentPayMethod == PayMethod.支付宝 || extraPaymentModel.CurrentPayMethod == PayMethod.微信支付)
            {
                var thirdpayinfo = extraPaymentModel.PaymentResult as 订单状态;
                if (thirdpayinfo != null)
                {
                    req.payAccountNo = thirdpayinfo.buyerAccount;
                    req.transNo = thirdpayinfo.outPayNo;
                    //req.outTradeNo = thirdpayinfo.outTradeNo;
                    req.tradeTime = thirdpayinfo.paymentTime;
                }
            }
        }

    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Collections.ObjectModel;
using System.Windows.Controls;
using System.Windows;
using Prism.Regions;
using Prism.Commands;
using Microsoft.Practices.Unity;
using YuanTu.Consts;
using YuanTu.Consts.Dtos;
using YuanTu.Consts.Models;
using YuanTu.Consts.Enums;
using YuanTu.Consts.FrameworkBase;
using YuanTu.Consts.Gateway;
using YuanTu.Consts.Models.GateWayStatus;
using YuanTu.Consts.Services;
using YuanTu.Consts.Models.Register;
using YuanTu.Core.Navigating;
using YuanTu.Default.Component.Tools.Models;
using YuanTu.QDArea.Enums;
using YuanTu.QDArea.Models.Register;
using YuanTu.Core.Extension;
using YuanTu.Default.Component.ViewModels;

namespace YuanTu.QDKouQiangYY.Component.ViewModels
{
    public class ConfirmViewModel : YuanTu.Default.Component.ViewModels.ConfirmViewModel
    {

        [Dependency]
        public IRegisterModel RegisterModel { get; set; }

        [Dependency]
        public IRegLockExtendModel RegLockExtendModel { get; set; }

        public override void OnEntered(NavigationContext navigationContext)
        {
            ChangeNavigationContent("");

            LeftList = PaymentModel.LeftList;
            RightList = PaymentModel.RightList;
            MidList = PaymentModel.MidList;

            NoPay = PaymentModel.NoPay;
            ViewTitle = NoPay ? $"请点击确定完成{ChoiceModel.Business}" : "请点击下方卡片选择支付方式";
            Hint = $"{ChoiceModel.Business}信息";

            var resource = ResourceEngine;
            var config = GetInstance<IConfigurationManager>();

            var spex = $"PayIn:充值";

            QuickRechargeContent = config.GetValue($"{spex}:Name") ?? "未定义";

            ImgTxtBtnIconUri = resource.GetImageResourceUri(config.GetValue($"{spex}:ImageName"));
            CanQuickRecharge = config.GetValue($"{spex}:Visabled") == "1" && !NoPay;

            var listOut = PayMethodDto.GetInfoPays(config, resource, "PayOut", new DelegateCommand<Info>(Confirm),
            p =>
            {
                //if (ChoiceModel.Business == Business.缴费 && (p.PayMethod == PayMethod.支付宝 || p.PayMethod == PayMethod.微信支付))
                //    p.Visabled = false;
            });
            PayOut = new ObservableCollection<InfoIcon>(listOut);

            PlaySound();
        }
        protected override void Confirm(Info i)
        {
            PaymentModel.PayMethod = (PayMethod)i.Tag;
            var extra = GetInstance<IExtraPaymentModel>();
            extra.CurrentBusiness = ChoiceModel.Business;
            extra.TotalMoney = PaymentModel.Total;
            extra.CurrentPayMethod = PaymentModel.PayMethod;
            //准备门诊支付所需病人信息

            ExtraPaymentModel.PatientInfo = new PatientInfo
            {
                Name = PatientModel.当前病人信息.name,
                PatientId = PatientModel.当前病人信息.patientId,
                IdNo = PatientModel.当前病人信息.idNo,
                GuardianNo = PatientModel.当前病人信息.guardianNo,
                CardNo = CardModel.CardNo,
                CardType = CardModel.CardType,
                Remain = decimal.Parse(PatientModel.当前病人信息.accBalance)
            };
            switch (PaymentModel.PayMethod)
            {
                case PayMethod.预缴金:
                    var patientModel = PatientModel.Res病人信息查询.data[PatientModel.PatientInfoIndex];

                    if (decimal.Parse(patientModel.accBalance) < PaymentModel.Total)
                    {
                        var textblock = new TextBlock
                        {
                            TextWrapping = TextWrapping.Wrap,
                            TextAlignment = TextAlignment.Center,
                            Margin = new Thickness(0, 15, 0, 0)
                        };
                        textblock.Inlines.Add("应支付金额：");
                        textblock.Inlines.Add(new TextBlock { Text = $"{PaymentModel.Total.In元()}", Foreground = new SolidColorBrush(Color.FromRgb(245, 162, 81)) });
                        textblock.Inlines.Add("\r\n账户余额：");
                        textblock.Inlines.Add(new TextBlock { Text = $"{patientModel.accBalance.In元()}", Foreground = new SolidColorBrush(Color.FromRgb(245, 162, 81)) });
                        textblock.Inlines.Add("\r\n差额：");
                        textblock.Inlines.Add(new TextBlock { Text = $"{(PaymentModel.Total - decimal.Parse(patientModel.accBalance)).In元()}", Foreground = new SolidColorBrush(Color.FromRgb(245, 162, 81)) });

                        if (CanQuickRecharge)
                        {
                            ShowConfirm("账户余额不足", textblock, cp =>
                            {
                                if (cp && CheckCanRecharge())
                                {
                                    StackNavigate(A.ChongZhi_Context, A.CZ.RechargeWay);
                                    return;
                                }
                            }, 30, ConfirmExModel.Build("去充值", "其他支付方式", true));
                        }
                        else
                        {
                            ShowAlert(false, "余额不足", "您的余额不足以支付该次诊疗费用，请充值");
                        }
                        return;
                    }

                    PaymentModel.ConfirmAction?.BeginInvoke(cp =>
                    {
                        var rest = PaymentModel.ConfirmAction?.EndInvoke(cp);
                        if (rest?.IsSuccess ?? false)
                            ChangeNavigationContent(i.Title);
                    }, null);
                    break;
                case PayMethod.银联:
                    extra.FinishFunc = () => Task.Run(PaymentModel.ConfirmAction);
                    GoToUnion();
                    break;
                case PayMethod.社保:
                    extra.FinishFunc = () => Task.Run(PaymentModel.ConfirmAction);
                    GoToSiPay();
                    break;
                case PayMethod.支付宝:
                    extra.FinishFunc = () => Task.Run(PaymentModel.ConfirmAction);
                    Navigate(A.Third.ScanQrCode);
                    break;
                case PayMethod.微信支付:
                    extra.FinishFunc = () => Task.Run(PaymentModel.ConfirmAction);
                    Navigate(A.Third.ScanQrCode);
                    break;
                case PayMethod.苹果支付:
                    ExtraPaymentModel.FinishFunc = () => Task.Run(PaymentModel.ConfirmAction);
                    GoToUnion();
                    break;
                case PayMethod.银联闪付:
                    ExtraPaymentModel.FinishFunc = () => Task.Run(PaymentModel.ConfirmAction);
                    GoToUnion();
                    break;
            }
        }

        protected override void NoPayConfirm()
        {
            if (ChoiceModel.Business == Business.预约 && RegisterModel.Res挂号锁号?.data?.appointMode == ((int)ApptChargeMode.用户可选收费).ToString())
            {
                var block = new TextBlock() { TextAlignment = TextAlignment.Center, FontSize = 17 };

                block.Inlines.Add("【立即支付】  就诊当天，您可直接取号，不需另行支付；\r\n");
                block.Inlines.Add("【取号支付】  挂号费用在就诊当天取号时支付。\r\n");

                ShowConfirm("提示信息", block, cb =>
                {
                    if (!cb)//取号支付
                    {
                        RegLockExtendModel.isCharge = false;
                        PaymentModel.ConfirmAction?.BeginInvoke(cp =>
                        {
                            var rest = PaymentModel.ConfirmAction?.EndInvoke(cp);
                            if (rest?.IsSuccess ?? false)
                                ChangeNavigationContent("");
                        }, null);
                        return;
                    };

                    //用户选择立即支付
                    RegLockExtendModel.isCharge = true;
                    //加载预交金支付方式
                    LoadOCPayMethod();

                }, 60, ConfirmExModel.Build("立即支付", "取号时支付"));
            }
            else
            {
                RegLockExtendModel.isCharge = false;
                PaymentModel.ConfirmAction?.BeginInvoke(cp =>
                {
                    var rest = PaymentModel.ConfirmAction?.EndInvoke(cp);
                    if (rest?.IsSuccess ?? false)
                        ChangeNavigationContent("");
                }, null);
            }
        }

        protected void GoToSiPay()
        {
            Navigate(A.Third.SiPay);
        }


        protected virtual void LoadOCPayMethod()
        {
            NoPay = false;
            ViewTitle = NoPay ? $"请点击确定完成{ChoiceModel.Business}" : "请点击下方卡片选择支付方式";
            Hint = $"{ChoiceModel.Business}信息";

            var resource = ResourceEngine;
            var config = GetInstance<IConfigurationManager>();
            var confirmCommand = new DelegateCommand<Info>(Confirm);

            var spex = "PayIn:充值";
            QuickRechargeContent = config.GetValue($"{spex}:Name") ?? "未定义";
            ImgTxtBtnIconUri = resource.GetImageResourceUri(config.GetValue($"{spex}:ImageName"));
            CanQuickRecharge = config.GetValue($"{spex}:Visabled") == "1" && !NoPay;
            
            var listOut = PayMethodDto.GetInfoPays(config, resource, "PayOut", confirmCommand,
                p =>
                {
                    if (p.PayMethod != PayMethod.预缴金)
                        p.Visabled = false;
                });
            PayOut = new ObservableCollection<InfoIcon>(listOut);

            PlaySound();

        }

        protected override void QuickRecharge()
        {
            if (CheckCanRecharge())
                StackNavigate(A.ChongZhi_Context, A.CZ.RechargeWay);
        }

        protected virtual bool CheckCanRecharge()
        {
            //脱机状态下不允许充值
            var gateWayStatusModel = GetInstance<IGateWayStatusModel>();
            gateWayStatusModel.Res查询网关状态 = DataHandlerEx.查询网关状态(new req查询网关状态());
            if (gateWayStatusModel.Res查询网关状态?.success ?? false)
            {
                if (gateWayStatusModel.Res查询网关状态?.data?.platform == "true")
                {
                    ShowAlert(false, "自助充值", "脱机状态下账户不可用，不可充值；\r\n请直接进行银联或医保缴费");
                    return false;
                }
            }

            //临时卡不允许充值
            var patientModel = PatientModel.Res病人信息查询.data[PatientModel.PatientInfoIndex];

            if (string.IsNullOrWhiteSpace(patientModel?.idNo) &&
                string.IsNullOrWhiteSpace(patientModel?.guardianNo))
            {
                ShowAlert(false, "自助充值", "临时卡不允许充值，请到人工窗口补充身份信息\r\n或直接进行银联或医保缴费");
                return false;
            }
            return true;
        }
    }
}

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using Microsoft.Practices.Unity;
using Prism.Regions;
using YuanTu.Consts.Dtos;
using YuanTu.Consts.Enums;
using YuanTu.Consts.Gateway;
using YuanTu.Consts.Models;
using YuanTu.Core.Extension;
using YuanTu.Core.FrameworkBase.Loadings;
using YuanTu.Default.Component.Tools.Models;
using YuanTu.VirtualHospital.Component.Loan.Models;

namespace YuanTu.VirtualHospital.Component.ViewModels
{
    public class ConfirmViewModel : Default.Component.ViewModels.ConfirmViewModel
    {
        [Dependency]
        public ILoanModel LoanModel { get; set; }

        public override void OnEntered(NavigationContext navigationContext)
        {
            if (Startup.先诊疗后付费 && !PaymentModel.NoPay)
                DoCommand(lp =>
                {
                    QueryLoanerInfo(lp);
                    base.OnEntered(navigationContext);
                }, false);
            else
                base.OnEntered(navigationContext);
        }

        protected virtual void QueryLoanerInfo(LoadingProcesser lp)
        {
            lp.ChangeText("正在查询借款权限...");

            var req = new req查询借款权限
            {
                cardNo = CardModel.CardNo,
                cardType = ((int)CardModel.CardType).ToString(),
            };
            LoanModel.Req查询借款权限 = req;
            LoanModel.Valid = false;
            LoanModel.HospitalRemainingAmount = 0m;
            var res = DataHandlerEx.查询借款权限(req);
            LoanModel.Res查询借款权限 = res;
            if (res == null || !res.success || res.data == null)
                return;
            if (res.data.loanAuthority != "true" || res.data.hospLoanAuthority != "true")
                return;
            LoanModel.Valid = true;
            if (decimal.TryParse(res.data.hospRemainingAmt, out var amount))
                LoanModel.HospitalRemainingAmount = amount;
        }

        protected virtual bool LoanerAgreement(LoadingProcesser lp)
        {
            lp.ChangeText("正在确认签署协议...");

            var req = new req借款签署协议
            {
                cardNo = CardModel.CardNo,
                cardType = ((int)CardModel.CardType).ToString(),
            };
            LoanModel.Req借款签署协议 = req;
            var res = DataHandlerEx.借款签署协议(req);
            LoanModel.Res借款签署协议 = res;
            if (res == null || !res.success)
            {
                ShowAlert(false, "借款签署协议", "借款签署协议确认失败\n" + res?.msg);
                return false;
            }
            return true;
        }

        protected override void FilterPayMethods(PayMethodDto payMethodDto)
        {
            if (payMethodDto.PayMethod != PayMethod.先诊疗后付费)
                return;
            if (!Startup.先诊疗后付费)
            {
                payMethodDto.Visabled = false;
            }
            else if (!LoanModel.Valid)
            {
                payMethodDto.IsEnabled = false;
            }
            else if (LoanModel.HospitalRemainingAmount < PaymentModel.Self)
            {
                payMethodDto.IsEnabled = false;
                payMethodDto.DisableText = "额度不足";
            }

        }

        protected override void SecondConfirm(Action<bool> act)
        {
            act?.Invoke(true);
            //base.SecondConfirm(act);
        }

        protected override void Confirm(Info i)
        {
            SecondConfirm(p =>
            {
                if (!p)
                    return;
                var payMethod = (PayMethod)i.Tag;
                PaymentModel.PayMethod = payMethod;

                ExtraPaymentModel.CurrentBusiness = ChoiceModel.Business;
                ExtraPaymentModel.TotalMoney = PaymentModel.Self;
                ExtraPaymentModel.CurrentPayMethod = payMethod;
                ExtraPaymentModel.Complete = false;
                //准备门诊支付所需病人信息
                if (ExtraPaymentModel.CurrentBusiness == Business.出院结算)
                {
                    var zy = PatientModel.住院患者信息;
                    ExtraPaymentModel.PatientInfo = new PatientInfo
                    {
                        Name = zy.name,
                        PatientId = zy.patientId,
                        IdNo = zy.idNo,
                        GuardianNo = "",
                        CardNo = CardModel.CardNo,
                        Remain = decimal.Parse(zy.accBalance),
                        CardType = CardModel.CardType,
                    };
                }
                else
                {
                    ExtraPaymentModel.PatientInfo = new PatientInfo
                    {
                        Name = PatientModel.当前病人信息.name,
                        PatientId = PatientModel.当前病人信息.patientId,
                        IdNo = PatientModel.当前病人信息.idNo,
                        GuardianNo = PatientModel.当前病人信息.guardianNo,
                        CardNo = CardModel.CardNo,
                        Remain = decimal.Parse(PatientModel.当前病人信息.accBalance),
                        CardType = CardModel.CardType,
                    };
                }

                HandlePaymethod(i, payMethod);
            });
        }

        protected override void HandlePaymethod(Info i, PayMethod payMethod)
        {
            switch (payMethod)
            {
                case PayMethod.先诊疗后付费:
                    var info = LoanModel.Res查询借款权限.data;

                    ShowConfirm("先诊疗后付费", GetLoanConfirmInfo(info), b =>
                    {
                        if (!b)
                            return;
                        if (info.signStatus == "true")
                        {
                            DoConfirm(i);
                            return;
                        }

                        ShowConfirm("先诊疗后付费协议", GetLoanAggrement(info), bb =>
                        {
                            if (!bb)
                                return;
                            DoCommand(lp =>
                            {
                                if (LoanerAgreement(lp))
                                    DoConfirm(i);
                            }, false);
                        }, extend: ConfirmExModel.Build("同意", "不同意", false));
                    });
                    break;
                default:
                    base.HandlePaymethod(i, payMethod);
                    break;
            }
        }

        private StackPanel GetLoanConfirmInfo(借款权限详情 info)
        {
            var textBlock = GetTextBlock("确认继续操作吗？", 20, TextAlignment.Center);
            textBlock.Margin = new Thickness(10);
            var stackPanel = new StackPanel
            {
                Children =
                {
                    GetTextBlock("您即将为:", textAlignment:TextAlignment.Center),
                    new StackPanel
                    {
                        HorizontalAlignment = HorizontalAlignment.Center,
                        Orientation = Orientation.Horizontal,
                        Children =
                        {
                            GetTextBlock("姓名:", textAlignment:TextAlignment.Right),
                            GetTextBlock(PatientModel.当前病人信息.name, Colors.Red),
                            new Grid { Width = 10 },
                            GetTextBlock("卡号:", textAlignment:TextAlignment.Right),
                            GetTextBlock(CardModel.CardNo, Colors.Coral),
                        }
                    },
                    new StackPanel
                    {
                        HorizontalAlignment = HorizontalAlignment.Center,
                        Orientation = Orientation.Horizontal,
                        Margin = new Thickness(10),
                        Children =
                        {
                            GetTextBlock("借款:", textAlignment:TextAlignment.Right),
                            GetTextBlock(PaymentModel.Self.In元(), Colors.Red),
                            new Grid { Width = 10 },
                            GetTextBlock("执行:", textAlignment:TextAlignment.Right),
                            GetTextBlock(ChoiceModel.Business.ToString(), Colors.Coral),
                        }
                    },
                    GetTextBlock("当前信用详情:", textAlignment:TextAlignment.Center),
                    new UniformGrid
                    {
                        HorizontalAlignment = HorizontalAlignment.Center,
                        Columns = 2,
                        Children =
                        {
                            GetTextBlock("总信用额度:", textAlignment:TextAlignment.Right),
                            GetTextBlock(info.creditsAmt.In元(), Colors.Red),
                            GetTextBlock("已借款金额:", textAlignment:TextAlignment.Right),
                            GetTextBlock(info.loanedAmt.In元(), Colors.Red),
                            GetTextBlock("剩余额度:", textAlignment:TextAlignment.Right),
                            GetTextBlock(info.remainingAmt.In元(), Colors.Red),
                            GetTextBlock("医院借款剩余额度:", textAlignment:TextAlignment.Right),
                            GetTextBlock(info.hospRemainingAmt.In元(), Colors.Red),
                        }
                    },
                    textBlock
                }
            };
            return stackPanel;
        }

        private static DockPanel GetLoanAggrement(借款权限详情 info)
        {
            var webBrowser = new WebBrowser();
            //webBrowser.LoadCompleted += (s, a) =>
            //{
            //    dynamic document = webBrowser.Document;
            //    document.oncontextmenu += new Func<bool>(() => false);
            //};
            webBrowser.NavigateToString(info.agreement);
            var viewer = new ScrollViewer
            {
                HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                Content = webBrowser,
                Height = 500,
            };
            var tblock = new TextBlock
            {
                TextAlignment = TextAlignment.Center,
                FontSize = 18,
                Foreground = new SolidColorBrush(Colors.Red),
                Text = "是否确认同意本协议?"
            };
            DockPanel.SetDock(tblock, Dock.Bottom);
            var dockPanel = new DockPanel
            {
                Margin = new Thickness(10),
                Children = { tblock, viewer, }
            };
            return dockPanel;
        }

        private static TextBlock GetTextBlock(string name, Color color)
        {
            return new TextBlock { Text = name, Foreground = new SolidColorBrush(color), FontWeight = FontWeights.Bold, FontSize = 20, VerticalAlignment = VerticalAlignment.Center};
        }

        private static TextBlock GetTextBlock(string name, double fontSize = 17, TextAlignment textAlignment = TextAlignment.Left)
        {
            return new TextBlock { Text = name, FontSize = fontSize, TextAlignment = textAlignment, VerticalAlignment = VerticalAlignment.Center};
        }

        protected void DoConfirm(Info i)
        {
            PaymentModel.ConfirmAction?.BeginInvoke(cp =>
            {
                var rest = PaymentModel.ConfirmAction?.EndInvoke(cp);
                if (rest?.IsSuccess ?? false)
                    ChangeNavigationContent(i.Title);
            }, null);
        }
    }
}
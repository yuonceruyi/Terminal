using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.Practices.Unity;
using Prism.Commands;
using Prism.Regions;
using YuanTu.Consts;
using YuanTu.Consts.Enums;
using YuanTu.Consts.Models;
using YuanTu.Consts.Models.Auth;
using YuanTu.Consts.Models.Payment;
using YuanTu.Consts.Services;
using YuanTu.Consts.Sounds;
using YuanTu.Core.FrameworkBase;
using YuanTu.Default.Component.Tools.Models;

namespace YuanTu.Default.House.Component.ViewModels
{
    public class ConfirmViewModel : ViewModelBase
    {
       
        public ConfirmViewModel()
        {
            ConfirmCommand = new DelegateCommand(NoPayConfirm);
            QuickRechargeCommand = new DelegateCommand(QuickRecharge);
        }

        public override string Title => "确认支付";

        public override void OnEntered(NavigationContext navigationContext)
        {
            //ChangeNavigationContent(".");

            LeftList = PaymentModel.LeftList;
            RightList = PaymentModel.RightList;
            MidList = PaymentModel.MidList;

            NoPay = PaymentModel.NoPay;
            ViewTitle = NoPay ? $"请点击确定完成{ChoiceModel.Business}" : "请点击下方卡片选择支付方式";
            Hint = $"{ChoiceModel.Business}信息";

            var resource =ResourceEngine;
            var config = GetInstance<IConfigurationManager>();
            var confirmCommand = new DelegateCommand<Info>(Confirm);

            var spex = $"PayIn:充值";

            QuickRechargeContent = config.GetValue($"{spex}:Name") ?? "未定义";

            ImgTxtBtnIconUri = resource.GetImageResourceUri(config.GetValue($"{spex}:ImageName"));
            CanQuickRecharge = config.GetValue($"{spex}:Visabled") == "1" && !NoPay;

            var btsOut = new List<PayMethodModel>();
            var kOut = Enum.GetValues(typeof(PayMethod));
            foreach (PayMethod payMethod in kOut)
            {
                spex = $"PayOut:{payMethod}";
                var v = config.GetValue($"{spex}:Visabled");

                if (v != "1") continue;
                var color = config.GetValue($"{spex}:Color").Split(',');
                btsOut.Add(new PayMethodModel
                {
                    PayMethod = payMethod,
                    Name = config.GetValue($"{spex}:Name") ?? "未定义",
                    Order = config.GetValueInt($"{spex}:Order"),
                    ImageSource = resource.GetImageResourceUri(config.GetValue($"{spex}:ImageName")),
                    Color = Color.FromRgb(byte.Parse(color[0]), byte.Parse(color[1]), byte.Parse(color[2]))
                });
            }
            var listOut = btsOut
                .OrderBy(p => p.Order)
                .Select(p => new InfoIcon
                {
                    Title = p.Name,
                    ConfirmCommand = confirmCommand,
                    IconUri = p.ImageSource,
                    Tag = p.PayMethod,
                    Color = p.Color
                });
            PayOut = new ObservableCollection<InfoIcon>(listOut);

            if (!NoPay)
            {
                PlaySound(SoundMapping.选择支付方式);
            }
        }
        
        protected virtual void Confirm(Info i)
        {
            SecondConfirm(p =>
            {
                if (p)
                {
                    PaymentModel.PayMethod = (PayMethod)i.Tag;

                    ExtraPaymentModel.CurrentBusiness = ChoiceModel.Business;
                    ExtraPaymentModel.TotalMoney = PaymentModel.Total;
                    ExtraPaymentModel.CurrentPayMethod = PaymentModel.PayMethod;
                    ExtraPaymentModel.Complete = false;
                    //准备门诊支付所需病人信息

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

                    switch (PaymentModel.PayMethod)
                    {
                        case PayMethod.预缴金:
                            var patientModel = PatientModel.Res病人信息查询.data[PatientModel.PatientInfoIndex];

                            if (decimal.Parse(patientModel.accBalance) < PaymentModel.Total)
                            {
                                ShowAlert(false, "余额不足", "您的余额不足以支付该次诊疗费用，请充值");
                                return;
                            }

                            PaymentModel.ConfirmAction?.BeginInvoke(cp =>
                            {
                                PaymentModel.ConfirmAction?.EndInvoke(cp);
                                ChangeNavigationContent(i.Title);
                            }, null);
                            break;

                        case PayMethod.银联:
                            ExtraPaymentModel.FinishFunc = () => Task.Run(PaymentModel.ConfirmAction);
                            GoToUnion();
                            break;

                        case PayMethod.支付宝:
                            ExtraPaymentModel.FinishFunc = () => Task.Run(PaymentModel.ConfirmAction);
                            Navigate(A.Third.ScanQrCode);
                            break;

                        case PayMethod.微信支付:
                            ExtraPaymentModel.FinishFunc = () => Task.Run(PaymentModel.ConfirmAction);
                            Navigate(A.Third.ScanQrCode);
                            break;

                        case PayMethod.苹果支付:
                            ShowAlert(false, "支付确认", "业务未实现");
                            break;
                    }
                }
            });
        }

        protected void GoToUnion()
        {
            Navigate(A.Third.PosUnion);
        }

        protected virtual void NoPayConfirm()
        {
            PaymentModel.ConfirmAction?.BeginInvoke(cp =>
            {
                PaymentModel.ConfirmAction?.EndInvoke(cp);
                ChangeNavigationContent(".");
            }, null);
        }

        //处理二次确认
        protected virtual void SecondConfirm(Action<bool> act)
        {
            var block = new TextBlock() { TextAlignment = TextAlignment.Center, FontSize = 17 };
            block.Inlines.Add("您即将为\r\n姓名:");
            block.Inlines.Add(new TextBlock() { Text = PatientModel.当前病人信息.name, Foreground = new SolidColorBrush(Colors.Red), FontWeight = FontWeights.Bold, FontSize = 20 });
            block.Inlines.Add(" 卡号:");
            block.Inlines.Add(new TextBlock() { Text = CardModel.CardNo, Foreground = new SolidColorBrush(Colors.Coral), FontWeight = FontWeights.Bold, FontSize = 20 });
            block.Inlines.Add($"\r\n执行{ChoiceModel.Business}，\r\n确认继续操作吗？");
            ShowConfirm("信息确认", block, act);
        }

        protected virtual void QuickRecharge()
        {
            // PaymentModel.PayMethod = PayMethod.充值;
            StackNavigate(A.ChongZhi_Context, A.CZ.RechargeWay);
        }

        #region

        private string _viewTitle;
        private string _hint;
        private bool _noPay;
        private string _buttonContent = "确定";
        private List<PayInfoItem> _leftList;
        private List<PayInfoItem> _rightList;
        private List<PayInfoItem> _midList;
        private string _quickRechargeContent;
        private Uri _imgTxtBtnIconUri;
        private bool _canQuickRecharge;

        public string ViewTitle
        {
            get { return _viewTitle; }
            set
            {
                _viewTitle = value;
                OnPropertyChanged();
            }
        }

        public string Hint
        {
            get { return _hint; }
            set
            {
                _hint = value;
                OnPropertyChanged();
            }
        }

        public bool NoPay
        {
            get { return _noPay; }
            set
            {
                _noPay = value;
                OnPropertyChanged();
            }
        }

        public string ButtonContent
        {
            get { return _buttonContent; }
            set
            {
                _buttonContent = value;
                OnPropertyChanged();
            }
        }

        public List<PayInfoItem> LeftList
        {
            get { return _leftList; }
            set
            {
                _leftList = value;
                OnPropertyChanged();
            }
        }

        public List<PayInfoItem> RightList
        {
            get { return _rightList; }
            set
            {
                _rightList = value;
                OnPropertyChanged();
            }
        }

        public List<PayInfoItem> MidList
        {
            get { return _midList; }
            set
            {
                _midList = value;
                OnPropertyChanged();
            }
        }

        public string QuickRechargeContent
        {
            get { return _quickRechargeContent; }
            set
            {
                _quickRechargeContent = value;
                OnPropertyChanged();
            }
        }

        public Uri ImgTxtBtnIconUri
        {
            get { return _imgTxtBtnIconUri; }
            set
            {
                _imgTxtBtnIconUri = value;
                OnPropertyChanged();
            }
        }

        public bool CanQuickRecharge
        {
            get { return _canQuickRecharge; }
            set
            {
                _canQuickRecharge = value;
                OnPropertyChanged();
            }
        }

        #endregion

        #region

        [Dependency]
        public IPaymentModel PaymentModel { get; set; }

        [Dependency]
        public IPatientModel PatientModel { get; set; }

        [Dependency]
        public IChoiceModel ChoiceModel { get; set; }

        [Dependency]
        public ICardModel CardModel { get; set; }

        [Dependency]
        public IExtraPaymentModel ExtraPaymentModel { get; set; }

        #endregion

        #region Binding

        private ObservableCollection<InfoIcon> _payIn;

        public ObservableCollection<InfoIcon> PayIn
        {
            get { return _payIn; }
            set
            {
                _payIn = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<InfoIcon> _payOut;

        public ObservableCollection<InfoIcon> PayOut
        {
            get { return _payOut; }
            set
            {
                _payOut = value;
                OnPropertyChanged();
            }
        }

        public ICommand ConfirmCommand { get; set; }
        public ICommand QuickRechargeCommand { get; set; }

        public class PayMethodModel
        {
            public PayMethod PayMethod { get; set; }
            public string Name { get; set; }
            public int Order { get; set; }
            public Uri ImageSource { get; set; }

            public bool Visabled { get; set; }
            public Color Color { get; set; }
        }

        #endregion
    }
}
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.Practices.Unity;
using Prism.Commands;
using Prism.Regions;
using YuanTu.Consts;
using YuanTu.Consts.Dtos;
using YuanTu.Consts.Dtos.UnionPos;
using YuanTu.Consts.Enums;
using YuanTu.Consts.FrameworkBase;
using YuanTu.Consts.Gateway;
using YuanTu.Consts.Models;
using YuanTu.Consts.Models.Auth;
using YuanTu.Consts.Models.Payment;
using YuanTu.Consts.Models.Print;
using YuanTu.Consts.Models.Recharge;
using YuanTu.Consts.Services;
using YuanTu.Consts.Sounds;
using YuanTu.Core.DB;
using YuanTu.Core.Extension;
using YuanTu.Core.FrameworkBase;
using YuanTu.Core.Log;
using YuanTu.Default.Component.Tools.Models;
using YuanTu.VirtualHospital.Component.Loan.Models;

namespace YuanTu.VirtualHospital.Component.Loan.ViewModels
{
    public class RepayMethodViewModel : ViewModelBase
    {
        public override string Title => "选择还款方式";

        public override void OnEntered(NavigationContext navigationContext)
        {
            ChangeNavigationContent("");

            LeftList = PaymentModel.LeftList;
            RightList = PaymentModel.RightList;
            MidList = PaymentModel.MidList;
            
            ViewTitle = "请点击下方卡片选择支付方式";
            Hint = $"{ChoiceModel.Business}信息";
            
            var list = PayMethodDto.GetInfoPays(
                GetInstance<IConfigurationManager>(),
                ResourceEngine,
                "Repay",
                new DelegateCommand<Info>(Confirm));
            var item = LoanModel.所选借款账单;
            var methods = item.allowPayType.Split(new []{ ',' }, StringSplitOptions.RemoveEmptyEntries);
            Data = new ObservableCollection<InfoIcon>(list.Where(i=>  
                methods.Contains(((PayMethod) i.Tag).GetEnumDescription())));

            PlaySound(SoundMapping.选择支付方式);
        }
        
        protected virtual void Confirm(Info i)
        {
            var payMethod = (PayMethod) i.Tag;
            PaymentModel.PayMethod = payMethod;

            ExtraPaymentModel.CurrentBusiness = ChoiceModel.Business;
            ExtraPaymentModel.TotalMoney = PaymentModel.Self;
            ExtraPaymentModel.CurrentPayMethod = payMethod;
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

            HandlePaymethod(i, payMethod);
        }

        protected virtual void HandlePaymethod(Info i, PayMethod payMethod)
        {
            switch (payMethod)
            {
                case PayMethod.预缴金:

                    var patientModel = PatientModel.当前病人信息;

                    var accBalance = decimal.Parse(patientModel.accBalance);
                    if (accBalance < PaymentModel.Self)
                    {
                        ShowAlert(false, "余额不足", "您的余额不足以支付该次诊疗费用，请充值");
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
                case PayMethod.智慧医疗:
                    ShowAlert(false, "支付确认", "业务未实现");
                    break;
            }
        }

        protected void GoToUnion()
        {
            Navigate(A.Third.PosUnion);
        }

        #region DataBindings

        private string _viewTitle;

        public string ViewTitle
        {
            get { return _viewTitle; }
            set
            {
                _viewTitle = value;
                OnPropertyChanged();
            }
        }

        private string _hint;
        public string Hint
        {
            get { return _hint; }
            set
            {
                _hint = value;
                OnPropertyChanged();
            }
        }

        private List<PayInfoItem> _leftList;
        public List<PayInfoItem> LeftList
        {
            get { return _leftList; }
            set
            {
                _leftList = value;
                OnPropertyChanged();
            }
        }

        private List<PayInfoItem> _rightList;
        public List<PayInfoItem> RightList
        {
            get { return _rightList; }
            set
            {
                _rightList = value;
                OnPropertyChanged();
            }
        }

        private List<PayInfoItem> _midList;
        public List<PayInfoItem> MidList
        {
            get { return _midList; }
            set
            {
                _midList = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<InfoIcon> _data;

        public ObservableCollection<InfoIcon> Data
        {
            get { return _data; }
            set
            {
                _data = value;
                OnPropertyChanged();
            }
        }

        #endregion

        #region DI

        [Dependency]
        public IPaymentModel PaymentModel { get; set; }

        [Dependency]
        public IChoiceModel ChoiceModel { get; set; }
        [Dependency]
        public ILoanModel LoanModel { get; set; }

        [Dependency]
        public IExtraPaymentModel ExtraPaymentModel { get; set; }

        [Dependency]
        public IPatientModel PatientModel { get; set; }

        [Dependency]
        public ICardModel CardModel { get; set; }

        [Dependency]
        public IPrintModel PrintModel { get; set; }

        [Dependency]
        public IPrintManager PrintManager { get; set; }

        [Dependency]
        public IConfigurationManager ConfigurationManager { get; set; }

        #endregion
    }
}
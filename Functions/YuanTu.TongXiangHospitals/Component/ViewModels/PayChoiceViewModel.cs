using Microsoft.Practices.Unity;
using Prism.Commands;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Media;
using YuanTu.Consts.Enums;
using YuanTu.Consts.Models;
using YuanTu.Consts.Models.Auth;
using YuanTu.Consts.Models.Payment;
using YuanTu.Consts.Services;
using YuanTu.Consts.Sounds;
using YuanTu.Core.FrameworkBase;
using YuanTu.Core.Log;
using YuanTu.Default.Component.Tools.Models;
using YuanTu.Default.Component.ViewModels;
using YuanTu.TongXiangHospitals.HealthInsurance.Model;

namespace YuanTu.TongXiangHospitals.Component.ViewModels
{
    public class PayChoiceViewModel : ViewModelBase
    {
        private string _hint;
        private ObservableCollection<InfoIcon> _payOut;

        public string Hint
        {
            get { return _hint; }
            set
            {
                _hint = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<InfoIcon> PayOut
        {
            get { return _payOut; }
            set
            {
                _payOut = value;
                OnPropertyChanged();
            }
        }

        [Dependency]
        public ISiModel SiModel { get; set; }
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

        public override string Title => "请点击下方卡片选择支付方式";

        public override void OnEntered(NavigationContext navigationContext)
        {
            ChangeNavigationContent("");

            Hint = $"{ChoiceModel.Business}信息";

            var resource =ResourceEngine;
            var config = GetInstance<IConfigurationManager>();
            var confirmCommand = new DelegateCommand<Info>(Confirm);

           
            var btsOut = new List<PayMethodModel>();
            var kOut = Enum.GetValues(typeof(PayMethod));
            foreach (PayMethod payMethod in kOut)
            {
                var spex = $"PayOut:{payMethod}";
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

            PlaySound(SoundMapping.选择支付方式);
        }

        protected virtual void Confirm(Info i)
        {
            PaymentModel.PayMethod = (PayMethod)i.Tag;
            if (CardModel.CardType == CardType.就诊卡 && PaymentModel.PayMethod == PayMethod.预缴金)
            {
                ShowAlert(false,"温馨提示","自费病人请选择其他支付方式");
                return;
            }
            SiModel.诊间结算 = PaymentModel.PayMethod == PayMethod.预缴金;//智慧医疗
            
            Logger.Main.Info($"诊间结算 {SiModel.诊间结算}");
            //DoCommand(lp =>
            //{
            //    lp.ChangeText($"正在进行预结算...");
            //    var result = SiModel.PreSettleAction.Invoke();
            //    if (result.IsSuccess)
            //        Next();
            //});



            SiModel.PreSettleAction?.BeginInvoke(cp =>
            {
                var rest = SiModel.PreSettleAction?.EndInvoke(cp);
                if (rest?.IsSuccess ?? false)
                {
                    ChangeNavigationContent(i.Title);
                    Next();
                }
                
            }, null);

        }
        public class PayMethodModel
        {
            public PayMethod PayMethod { get; set; }
            public string Name { get; set; }
            public int Order { get; set; }
            public Uri ImageSource { get; set; }

            public bool Visabled { get; set; }
            public Color Color { get; set; }
        }
    }
}
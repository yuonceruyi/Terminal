using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Practices.Unity;
using Prism.Commands;
using Prism.Regions;
using YuanTu.Consts.FrameworkBase;
using YuanTu.Consts.Services;
using YuanTu.Core.FrameworkBase;
using YuanTu.Core.Navigating;
using YuanTu.Default.Tablet.Component.Cashier.Models;

namespace YuanTu.Default.Tablet.Component.Cashier.ViewModels
{
    internal class ChoiceViewModel : ViewModelBase
    {
        private IReadOnlyCollection<CashierButtonInfo> _data;
        public override string Title => "业务选择";

        public IReadOnlyCollection<CashierButtonInfo> Data
        {
            get { return _data; }
            set
            {
                _data = value;
                OnPropertyChanged();
            }
        }

        public DelegateCommand<CashierButtonInfo> Command { get; set; }
        [Dependency]
        public ICashierModel Cashier { get; set; }

        /// <summary>
        ///     进入当期页面时触发
        /// </summary>
        /// <param name="navigationContext"></param>
        public override void OnEntered(NavigationContext navigationContext)
        {
            base.OnEntered(navigationContext);
            HideNavigating = true;
            DisablePreviewButton = true;
            GetInstance<INavigationModel>().Items.Clear();
        }

        /// <summary>
        ///     仅当在实例化试调用
        /// </summary>
        public override void OnSet()
        {
            Command = new DelegateCommand<CashierButtonInfo>(OnButtonClick, p => p.IsEnabled);
            OnSetButton();
        }

        protected virtual void OnSetButton()
        {
            var resource = ResourceEngine;
            var config = GetInstance<IConfigurationManager>();

            var buttons = new List<CashierButtonInfo>();
            var businesses = Enum.GetNames(typeof(CashierTypeEnum));
            foreach (var business in businesses)
            {
                var visible = config.GetValue($"Cashier:{business}:Visabled");
                if (visible != "1")
                    continue;
                CashierTypeEnum b;
                Enum.TryParse(business, out b);
                buttons.Add(new CashierButtonInfo
                {
                    Name = config.GetValue($"Cashier:{business}:Name") ?? "未定义",
                    Type = b,
                    Order = config.GetValueInt($"Cashier:{business}:Order"),
                    IsEnabled = config.GetValueInt($"Cashier:{business}:IsEnabled") == 1,
                    ImageSource = resource.GetImageResource(config.GetValue($"Cashier:{business}:ImageName"))
                });
            }
            Data = buttons.OrderBy(p => p.Order).ToList();
        }

        protected virtual void OnButtonClick(CashierButtonInfo obj)
        {
            //手动清空导航栏
            GetInstance<INavigationModel>().Items.Clear();
            var engine = NavigationEngine;
            Cashier.Business = obj.Type;
            switch (obj.Type)
            {
                case CashierTypeEnum.收款:
                    engine.JumpAfterFlow(null, null,
                        new FormContext(AInner.Sale, AInner.SY.Amount), obj.Name);
                    break;

                case CashierTypeEnum.退款:
                    engine.JumpAfterFlow(null, null,
                        new FormContext(AInner.Refund, AInner.SY.Card), obj.Name);
                    break;

                default:
                    ShowAlert(false, "友好提示", "业务未实现");
                    break;
            }
        }
    }
}
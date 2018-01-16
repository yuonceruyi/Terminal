using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Practices.Unity;
using Prism.Commands;
using Prism.Regions;
using YuanTu.Consts.Enums;
using YuanTu.Consts.Gateway;
using YuanTu.Consts.Models;
using YuanTu.Consts.Models.Auth;
using YuanTu.Consts.Services;
using YuanTu.Core.FrameworkBase;
using YuanTu.VirtualHospital.Component.Loan.Models;

namespace YuanTu.VirtualHospital.Component.Loan.ViewModels
{
    class ChoiceViewModel : ViewModelBase
    {
        public override string Title => "功能选择";

        public ChoiceViewModel()
        {
            Command = new DelegateCommand<ChoiceButtonInfo>(Do, info => info.IsEnabled);
        }

        private List<ChoiceButtonInfo> _data;

        public List<ChoiceButtonInfo> Data
        {
            get { return _data; }
            set
            {
                _data = value; 
                OnPropertyChanged();
            }
        }

        private string _layoutRule;

        public string LayoutRule
        {
            get { return _layoutRule; }
            set
            {
                _layoutRule = value; 
                OnPropertyChanged();
            }
        }

        public DelegateCommand<ChoiceButtonInfo> Command { get; set; }

        [Dependency]
        public ILoanModel LoanModel { get; set; }

        [Dependency]
        public ICardModel CardModel { get; set; }

        private readonly Dictionary<string, int> _dic = new Dictionary<string, int>()
        {
            ["状态查询"] = -1,
            ["自助还款"] = -2
        };

        public override void OnEntered(NavigationContext navigationContext)
        {
            var buttons = new List<ChoiceButtonInfo>();
            var resource = GetInstance<IResourceEngine>();
            var config = GetInstance<IConfigurationManager>();
            foreach (var kvp in _dic)
            {
                var prefix = $"Loan:{kvp.Key}";
                var v = config.GetValue($"{prefix}:Visabled");
                if (v != "1") continue;
                buttons.Add(new ChoiceButtonInfo
                {
                    Name = config.GetValue($"{prefix}:Name") ?? "未定义",
                    ButtonBusiness = (Business)kvp.Value,
                    Order = config.GetValueInt($"{prefix}:Order"),
                    IsEnabled = config.GetValueInt($"{prefix}:IsEnabled") == 1,
                    ImageSource = resource.GetImageResource(config.GetValue($"{prefix}:ImageName"))
                });
            }

            Data = buttons.OrderBy(b => b.Order).ToList();
        }

        private void Do(ChoiceButtonInfo info)
        {
            switch ((int)info.ButtonBusiness)
            {
                case -1:
                    QueryInfoExecute();
                    break;
                case -2:
                    RepayExecute();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(info));
            }
        }

        protected virtual void QueryInfoExecute()
        {
            DoCommand(lp =>
            {
                lp.ChangeText("正在查询状态...");

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
                {
                    ShowAlert(false, "查询状态", $"查询状态失败\n{res?.msg}");
                    return;
                }
                Navigate(InnerA.Loan.Info);
            });
        }

        protected virtual void RepayExecute()
        {
            Navigate(InnerA.Loan.Date);
        }
    }
}

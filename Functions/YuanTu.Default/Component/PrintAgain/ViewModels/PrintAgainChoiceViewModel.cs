using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Prism.Commands;
using Prism.Regions;
using YuanTu.Consts;
using YuanTu.Consts.Enums;
using YuanTu.Consts.FrameworkBase;
using YuanTu.Consts.Models;
using YuanTu.Consts.Models.InfoQuery;
using YuanTu.Consts.Services;
using YuanTu.Core.FrameworkBase;
using YuanTu.Core.Navigating;
using YuanTu.Consts.Models.PrintAgain;

namespace YuanTu.Default.Component.PrintAgain.ViewModels
{
    public class PrintAgainChoiceViewModel : ViewModelBase
    {
        private IReadOnlyCollection<PrintAgainButtonInfo> _data;

        public IReadOnlyCollection<PrintAgainButtonInfo> Data
        {
            get { return _data; }
            set
            {
                _data = value;
                OnPropertyChanged();
            }
        }

        public DelegateCommand<PrintAgainButtonInfo> Command { get; set; }

        protected virtual void OnSetButton()
        {
            var resource = ResourceEngine;
            var config = GetInstance<IConfigurationManager>();

            var bts = new List<PrintAgainButtonInfo>();
            var k = Enum.GetValues(typeof(PrintAgainTypeEnum));
            foreach (PrintAgainTypeEnum buttonInfo in k)
            {
                var v = config.GetValue($"PrintAgain:{buttonInfo}:Visabled");
                if (v == "1")
                    bts.Add(new PrintAgainButtonInfo
                    {
                        Name = config.GetValue($"PrintAgain:{buttonInfo}:Name") ?? "未定义",
                        PrintAgainType = buttonInfo,
                        Order = config.GetValueInt($"PrintAgain:{buttonInfo}:Order"),
                        IsEnabled = config.GetValueInt($"PrintAgain:{buttonInfo}:IsEnabled") == 1,
                        ImageSource = resource.GetImageResource(config.GetValue($"PrintAgain:{buttonInfo}:ImageName"))
                    });
            }
            Data = bts.OrderBy(p => p.Order).ToList();
        }

        protected virtual void OnButtonClick(PrintAgainButtonInfo obj)
        {
            var engine = NavigationEngine;
            var choiceModel = GetInstance<IChoiceModel>();
            var printAgainChoiceModel = GetInstance<IPrintAgainModel>();
            choiceModel.HasAuthFlow = true;
            //手动清空导航栏
            GetInstance<INavigationModel>().Items.Clear();
            printAgainChoiceModel.PrintAgainType = obj.PrintAgainType;
            switch (obj.PrintAgainType)
            {
                case PrintAgainTypeEnum.缴费补打:
                    engine.JumpAfterFlow(new FormContext(A.ChaKa_Context, A.CK.Choice),
                    CreateJump,
                    new FormContext(A.PayCostQuery, A.JFBD.Date), obj.Name);
                    break;
                default:
                    ShowAlert(false, "友好提示", "业务未实现");
                    break;
            }
        }

        protected virtual Task<Result<FormContext>> CreateJump()
        {
            return null;
            //return Task.Run(() => Result<FormContext>.Success(default(FormContext)));
        }

        #region Overrides of ViewModelBase

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

        #endregion Overrides of ViewModelBase

        #region Overrides of ViewModelBase

        public override string Title => "信息查询选择";

        /// <summary>
        ///     仅当在实例化试调用
        /// </summary>
        public override void OnSet()
        {
            Command = new DelegateCommand<PrintAgainButtonInfo>(OnButtonClick, p => p.IsEnabled);
            OnSetButton();
        }

        #endregion Overrides of ViewModelBase
    }
}
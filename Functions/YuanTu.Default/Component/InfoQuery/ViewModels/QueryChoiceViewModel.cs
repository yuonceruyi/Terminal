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

namespace YuanTu.Default.Component.InfoQuery.ViewModels
{
    public class QueryChoiceViewModel : ViewModelBase
    {
        private IReadOnlyCollection<InfoQueryButtonInfo> _data;
        private string _layoutRule;

        public IReadOnlyCollection<InfoQueryButtonInfo> Data
        {
            get { return _data; }
            set
            {
                _data = value;
                OnPropertyChanged();
            }
        }

        public DelegateCommand<InfoQueryButtonInfo> Command { get; set; }


        public string LayoutRule
        {
            get { return _layoutRule; }
            set { _layoutRule = value; OnPropertyChanged();}
        }

        protected virtual void OnSetButton()
        {
            var resource =ResourceEngine;
            var config = GetInstance<IConfigurationManager>();

            LayoutRule = config.GetValue("LayoutRule");
            var bts = new List<InfoQueryButtonInfo>();
            var k = Enum.GetValues(typeof(InfoQueryTypeEnum));
            foreach (InfoQueryTypeEnum buttonInfo in k)
            {
                var v = config.GetValue($"InfoQuery:{buttonInfo}:Visabled");
                if (v == "1")
                    bts.Add(new InfoQueryButtonInfo
                    {
                        Name = config.GetValue($"InfoQuery:{buttonInfo}:Name") ?? "未定义",
                        InfoQueryType = buttonInfo,
                        Order = config.GetValueInt($"InfoQuery:{buttonInfo}:Order"),
                        IsEnabled = config.GetValueInt($"InfoQuery:{buttonInfo}:IsEnabled") == 1,
                        ImageSource = resource.GetImageResource(config.GetValue($"InfoQuery:{buttonInfo}:ImageName"))
                    });
            }
            Data = bts.OrderBy(p => p.Order).ToList();
        }

        protected virtual void OnButtonClick(InfoQueryButtonInfo obj)
        {
            var engine = NavigationEngine;
            var choiceModel = GetInstance<IChoiceModel>();
            var queryChoiceModel = GetInstance<IQueryChoiceModel>();
            choiceModel.HasAuthFlow = true;
            //choiceModel.Business=obj.
            //手动清空导航栏
            GetInstance<INavigationModel>().Items.Clear();
            queryChoiceModel.InfoQueryType = obj.InfoQueryType;
            switch (obj.InfoQueryType)
            {
                case InfoQueryTypeEnum.药品查询:
                    choiceModel.HasAuthFlow = false;
                    engine.JumpAfterFlow(null, 
                        CreateJump,
                        new FormContext(A.MedicineQuery, A.YP.Query), obj.Name);
                    break;

                case InfoQueryTypeEnum.项目查询:
                    choiceModel.HasAuthFlow = false;
                    engine.JumpAfterFlow(null,
                        CreateJump,
                        new FormContext(A.ChargeItemsQuery, A.XM.Query), obj.Name);
                    break;

                case InfoQueryTypeEnum.已缴费明细:
                    engine.JumpAfterFlow(new FormContext(A.ChaKa_Context, A.CK.Choice),
                        CreateJump,
                        new FormContext(A.PayCostQuery, A.JFJL.Date), obj.Name);
                    break;

                case InfoQueryTypeEnum.住院一日清单:
                    //choiceModel.HasAuthFlow = false;
                    choiceModel.AuthContext = A.ZhuYuan_Context;
                    engine.JumpAfterFlow(new FormContext(A.ZhuYuan_Context, A.ZY.InPatientNo),
                        CreateJump,
                        new FormContext(A.InDayDetailList_Context, A.ZYYRQD.Date), obj.Name);
                    break;

                case InfoQueryTypeEnum.检验结果:
                    engine.JumpAfterFlow(new FormContext(A.ChaKa_Context, A.CK.Choice),
                        CreateJump,
                        new FormContext(A.DiagReportQuery, A.JYJL.Date), obj.Name);
                    break;

                case InfoQueryTypeEnum.影像结果:
                    engine.JumpAfterFlow(new FormContext(A.ChaKa_Context, A.CK.Choice),
                        CreateJump,
                        new FormContext(A.PacsReportQuery, A.YXBG.Date), obj.Name);
                    break;
                case InfoQueryTypeEnum.交易记录查询:
                    engine.JumpAfterFlow(new FormContext(A.ChaKa_Context, A.CK.Choice),
                        CreateJump,
                        new FormContext(A.ReChargeQuery, A.CZJL.Date), obj.Name);
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
            Command = new DelegateCommand<InfoQueryButtonInfo>(OnButtonClick, p => p.IsEnabled);
            OnSetButton();
        }

        #endregion Overrides of ViewModelBase
    }
}
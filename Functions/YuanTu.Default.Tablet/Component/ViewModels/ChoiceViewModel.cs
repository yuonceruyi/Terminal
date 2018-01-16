using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Prism.Commands;
using Prism.Regions;
using YuanTu.Consts;
using YuanTu.Consts.Enums;
using YuanTu.Consts.FrameworkBase;
using YuanTu.Consts.UserCenter;
using YuanTu.Consts.Models;
using YuanTu.Consts.Models.Auth;
using YuanTu.Consts.Services;
using YuanTu.Core.FrameworkBase;
using YuanTu.Core.Navigating;
using YuanTu.Consts.Models.UserCenter.Register;
using YuanTu.Core.MultipScreen;

namespace YuanTu.Default.Tablet.Component.ViewModels
{
    public class ChoiceViewModel : ViewModelBase
    {
        private string _content;
        private IReadOnlyCollection<ChoiceButtonInfo> _data;

        public ChoiceViewModel()
        {
            _content = DateTimeCore.Now.ToString("HHmmss");
            Command = new DelegateCommand<ChoiceButtonInfo>(Do, info => info.IsEnabled);
        }

        public string Content
        {
            get { return _content; }
            set
            {
                _content = value;
                OnPropertyChanged();
            }
        }

        public IReadOnlyCollection<ChoiceButtonInfo> Data
        {
            get { return _data; }
            set
            {
                _data = value;
                OnPropertyChanged();
            }
        }

        public override string Title => "主页";

        public DelegateCommand<ChoiceButtonInfo> Command { get; set; }

        #region Overrides of ViewModelBase

        /// <summary>
        ///     仅当在实例化时调用
        /// </summary>
        public override void OnSet()
        {
            OnSetButton();
            new MultipScreen.Views.MultipDefaultView().ShowInScreen();
        }

        #endregion Overrides of ViewModelBase

        public virtual void OnSetButton()
        {
            var resource = ResourceEngine;
            var config = GetInstance<IConfigurationManager>();

            var bts = new List<ChoiceButtonInfo>();
            var businesses = Enum.GetNames(typeof(Business));
            foreach (var business in businesses)
            {
                var visible = config.GetValue($"Functions:{business}:Visabled");
                if (visible != "1") continue;
                Business b;
                Enum.TryParse(business, out b);
                bts.Add(new ChoiceButtonInfo
                {
                    Name = config.GetValue($"Functions:{business}:Name") ?? "未定义",
                    ButtonBusiness = b,
                    Order = config.GetValueInt($"Functions:{business}:Order"),
                    IsEnabled = config.GetValueInt($"Functions:{business}:IsEnabled") == 1,
                    ImageSource = resource.GetImageResource(config.GetValue($"Functions:{business}:ImageName"))
                });
            }
            Data = bts.OrderBy(p => p.Order).ToList();
        }

        public override void OnEntered(NavigationContext navigationContext)
        {
            base.OnEntered(navigationContext);
            NavigationEngine.DestinationStack.Clear();
            TimeOut = 0;
        }

        protected virtual void Do(ChoiceButtonInfo param)
        {
            var choiceModel = GetInstance<IChoiceModel>();
            choiceModel.Business = param.ButtonBusiness;
            choiceModel.HasAuthFlow = true; //默认有插卡流程
            var engine = NavigationEngine;

            switch (param.ButtonBusiness)
            {
                case Business.挂号:
                    engine.JumpAfterFlow(
                        new FormContext(A.ChaKa_Context, A.CK.Card),
                        RegisterJump,
                        new FormContext(A.XianChang_Context, AInner.XC.Hospitals),
                        param.Name);
                    break;

                case Business.预约:
                    engine.JumpAfterFlow(
                        new FormContext(A.ChaKa_Context, A.CK.Card),
                        AppointJump,
                        new FormContext(A.YuYue_Context, AInner.YY.Hospitals),
                        param.Name);
                    break;

                case Business.收银:
                    engine.JumpAfterFlow(null, null,
                        new FormContext(AInner.Sale, AInner.SY.Amount), "收款");
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        protected virtual Task<Result<FormContext>> RegisterJump()
        {
            return DoCommand(lp =>
            {
                var camera = GetInstance<ICameraService>();
                camera.SnapShot("主界面 挂号");
                var registerModel = GetInstance<IRegisterModel>();
                lp.ChangeText("正在查询医院列表，请稍候...");
                var req = new req医院列表
                {
                    unionId = FrameworkConst.UnionId
                };

                var res = DataHandlerEx.医院列表(req);
                if (res?.success ?? false)
                {
                    if (res?.data?.Count > 0)
                    {
                        registerModel.Res医院列表 = res;
                        return Result<FormContext>.Success(default(FormContext));
                    }
                    ShowAlert(false, "获取医院列表", "没有获得医院列表信息(列表为空)");
                    return Result<FormContext>.Fail("");
                }
                ShowAlert(false, "获取医院列表", $"没有获得医院列表信息:{res?.msg}");
                return Result<FormContext>.Fail("");
            });
        }

        protected virtual Task<Result<FormContext>> AppointJump()
        {
            return DoCommand(lp =>
            {
                var camera = GetInstance<ICameraService>();
                camera.SnapShot("主界面 预约");
                var registerModel = GetInstance<IRegisterModel>();
                lp.ChangeText("正在查询医院列表，请稍候...");
                var req = new req医院列表
                {
                    unionId = FrameworkConst.UnionId
                };

                var res = DataHandlerEx.医院列表(req);
                if (res?.success ?? false)
                {
                    if (res?.data?.Count > 0)
                    {
                        registerModel.Res医院列表 = res;
                        return Result<FormContext>.Success(default(FormContext));
                    }
                    ShowAlert(false, "获取医院列表", "没有获得医院列表信息(列表为空)");
                    return Result<FormContext>.Fail("");
                }
                ShowAlert(false, "获取医院列表", $"没有获得医院列表信息:{res?.msg}");
                return Result<FormContext>.Fail("");
            });
        }
    }
}

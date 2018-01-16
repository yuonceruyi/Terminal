using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using Microsoft.Practices.ServiceLocation;
using Microsoft.Practices.Unity;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using YuanTu.Consts;
using YuanTu.Consts.EventModels;
using YuanTu.Consts.FrameworkBase;
using YuanTu.Consts.Models;
using YuanTu.Consts.Services;
using YuanTu.Core.Log;
using YuanTu.Core.Navigating;

namespace YuanTu.Core.Models
{
    public interface ITopBottomModel
    {
        bool BackEnable { get; set; }
        bool HomeEnable { get; set; }
        Uri BackUri { get; set; }
        Uri HomeUri { get; set; }
        IReadOnlyCollection<ImageSource> LogoGroup { get; set; }
        ImageSource MainLogo { get; set; }
        DateTime Now { get; set; }
        string MainTitle { get; set; }
        string NotificMessage { get; set; }
        ICommand SuperDoubleClick { get; set; }
        DelegateCommand<string> SystemButtonCommand { get; }
        ObservableCollection<InfoItem> InfoItems { get; set; }
    }

    public class TopBottomModel : ModelBase, ITopBottomModel
    {
        #region 动态按钮分配
       protected readonly bool _isNewMode;
       protected Action _modulesChangeAction;
       protected Action _resetAction;
       protected readonly IEventAggregator _eventAggregator;
       protected Stack<List<ChoiceButtonInfo>> _buttonStack = new Stack<List<ChoiceButtonInfo>>();
        #endregion

      //  private DispatcherTimer _timer;

        public TopBottomModel(IEventAggregator eventAggregator)
        {
            eventAggregator.GetEvent<ViewChangeEvent>().Subscribe(ViewHasChanged);
            eventAggregator.GetEvent<SystemInfoEvent>().Subscribe(SystemInfoChanged);
            eventAggregator.GetEvent<PubSubEvent<DateTime>>().Subscribe(time => Now = time);
            eventAggregator.GetEvent<PubSubEvent<DateTime>>().Subscribe(CheckSystemShutDown, ThreadOption.UIThread);

            SuperDoubleClick = new DelegateCommand(SuperClickDo);
            SystemButtonCommand = new DelegateCommand<string>(ButtonClick);

            var resource = GetInstance<IResourceEngine>();
            LogoGroup = new List<ImageSource>
            {
                resource.GetImageResource("Logo_APP"),
                resource.GetImageResource("Logo_公众号"),
                resource.GetImageResource("Logo_银行"),
                resource.GetImageResource("Logo_远图")
            }.Where(p=>p!=null).ToList();
            MainLogo = resource.GetImageResource("MainLogo");
            HomeUri = resource.GetImageResourceUri("按钮图标_主页");
            BackUri = resource.GetImageResourceUri("按钮图标_返回");

            var assembly = Assembly.GetEntryAssembly().GetName();
            WorkVersion = $"{FrameworkConst.OperatorId}  V{assembly.Version} Beta";
            MainTitle = "医疗自助服务系统";
            NotificMessage = FrameworkConst.NotificMessage;

            var config = GetInstance<IConfigurationManager>();
            _isNewMode = config.GetValue("IsNewMode") == "1";
            if (_isNewMode)
            {
                eventAggregator.GetEvent<ModulesChangeEvent>().Subscribe(ModulesChanged);
                _eventAggregator = eventAggregator;
            }
        }

        [Dependency]
        public NavigationEngine NavigationEngine { get; set; }

        protected static TService GetInstance<TService>(string name = "")
        {
            if (string.IsNullOrWhiteSpace(name))
                return ServiceLocator.Current.GetInstance<TService>();
            return ServiceLocator.Current.GetInstance<TService>(name);
        }

        protected virtual void ViewHasChanged(ViewChangeEvent eveEvent)
        {
            var ishome = NavigationEngine.IsHome(eveEvent.To);
            if (ishome)
                InfoItems = null;
            ShowSystemButtons = !ishome;
            SystemButtonCommand.RaiseCanExecuteChanged();
        }

        protected virtual void SystemInfoChanged(SystemInfoEvent eveEvent)
        {
            BackEnable = !eveEvent.DisablePreviewButton;
            HomeEnable = !eveEvent.DisableHomeButton;
            ShowSystemButtons = BackEnable || HomeEnable;
            SystemButtonCommand.RaiseCanExecuteChanged();
        }

        protected virtual void ButtonClick(string cmd)
        {
            var engine = NavigationEngine;
            switch (cmd)
            {
                case "主页":
                    if (_isNewMode)
                    {
                        ShowSystemButtons = false;
                        HomeEnable = false;
                        BackEnable = false;
                        _buttonStack.Clear();
                        _resetAction?.Invoke();
                        _eventAggregator.GetEvent<TimeOutChangeEvent>().Publish(new TimeOutChangeEvent());
                    }
                    engine.State = engine.HomeAddress;
                    break;

                case "返回":
                    {
                        var ctx = new FormContext(engine.Context, engine.State);
                        if (engine.HasPrev(ctx) && BackEnable)
                        {
                            engine.Prev(ctx);
                        }
                        else
                        {
                            engine.State = engine.HomeAddress;
                            if (_isNewMode)
                            {
                                _modulesChangeAction?.Invoke();
                            }
                        }
                        if (_buttonStack.Count == 1)
                        {
                            ShowSystemButtons = false;
                            HomeEnable = false;
                            BackEnable = false;
                            _eventAggregator.GetEvent<TimeOutChangeEvent>().Publish(new TimeOutChangeEvent());
                        }
                        else if (_buttonStack.Count > 1)
                        {
                            ShowSystemButtons = true;
                            HomeEnable = true;
                            BackEnable = true;
                            _eventAggregator.GetEvent<TimeOutChangeEvent>().Publish(new TimeOutChangeEvent { TimeOut = 15 });
                        }
                    }
                    break;
            }
        }

        protected virtual void SuperClickDo()
        {
            var engine = NavigationEngine;
            if (engine.IsHome(engine.State) || engine.State == A.Maintenance)
                engine.Navigate(A.AdminPart);
            //Logger.Main.Info($"[系统关闭]双击系统关闭！");
            //Application.Current.Shutdown();
        }

        #region Timer

        private DateTime _now;

        public DateTime Now
        {
            get { return _now; }
            set
            {
                _now = value;
                OnPropertyChanged();
            }
        }

        #endregion Timer

        #region Binding

        public ICommand SuperDoubleClick { get; set; }

        public DelegateCommand<string> SystemButtonCommand { get; }

        public ImageSource MainLogo { get; set; }

        public Uri HomeUri { get; set; }

        public Uri BackUri { get; set; }

        private string _mainTitle;

        public string MainTitle
        {
            get { return _mainTitle; }
            set
            {
                _mainTitle = value;
                OnPropertyChanged();
            }
        }

        public string NotificMessage { get; set; }
        public string WorkVersion { get; set; }

        private IReadOnlyCollection<ImageSource> _logoGroup;

        public IReadOnlyCollection<ImageSource> LogoGroup
        {
            get { return _logoGroup; }
            set
            {
                _logoGroup = value;
                OnPropertyChanged();
            }
        }

        private bool _showSystemButtons;

        public bool ShowSystemButtons
        {
            get { return _showSystemButtons; }
            set
            {
                _showSystemButtons = value;
                OnPropertyChanged();
            }
        }

        private bool _homeEnable = true;

        public bool HomeEnable
        {
            get { return _homeEnable; }
            set
            {
                _homeEnable = value;
                OnPropertyChanged();
            }
        }

        private bool _backEnable = true;

        public bool BackEnable
        {
            get { return _backEnable; }
            set
            {
                _backEnable = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<InfoItem> _infoItems;

        public ObservableCollection<InfoItem> InfoItems
        {
            get { return _infoItems; }
            set
            {
                _infoItems = value;
                OnPropertyChanged();
            }
        }

        #endregion Binding

        #region Shutdown

        protected bool Inconfirm;
        protected int Laydown;
        protected int Shutdownretry;

        protected virtual void CheckSystemShutDown(DateTime now)
        {
            if (!FrameworkConst.AutoShutdownEnable || Inconfirm)
                return;
            if (Laydown++ < 60)
                return;
            Laydown = 0;
            if (now < FrameworkConst.AutoShutdownTime.AddMinutes(Shutdownretry * 5))
                return;
            var poweroffAct = new Action(() =>
            {
                Logger.Main.Info("[系统关闭]达到设定时间，系统关闭");
                Process.Start("shutdown.exe", "-s -t 0");
            });

            var engine = NavigationEngine;
            if (engine.IsHome(engine.State))
            {
                poweroffAct.Invoke();
                return;
            }

            Inconfirm = true;
            ShowConfirm("自动关机", "点击确定，机器将会自动关机!", cp =>
            {
                if (cp)
                    poweroffAct.Invoke();
                else
                    Shutdownretry++;
                Inconfirm = false;
            });
        }

        #endregion Shutdown

        #region ShowConfirm

        public void ShowConfirm(string title, string content, Action<bool> callback,
            int countdown = 0, ConfirmExModel extend = null)
        {
            ShowConfirm(title, content, null, callback, countdown, extend);
        }

        public void ShowConfirm(string title, FrameworkElement element, Action<bool> callback, int countdown = 0,
            ConfirmExModel extend = null)
        {
            ShowConfirm(title, null, element, callback, countdown, extend);
        }

        public void ShowConfirm(string title, string content, FrameworkElement element, Action<bool> callback,
            int countdown = 0, ConfirmExModel extend = null)
        {
            Logger.Main.Info($"[消息弹窗]标题:{title} 内容:{content}");
            Application.Current.MainWindow.Dispatcher.BeginInvoke(DispatcherPriority.ContextIdle, (Action)(() =>
           {
               var sm = GetInstance<IShellViewModel>();
               if (!sm.Confirm.Display)
               {
                   sm.Confirm.Display = true;
                   sm.Confirm.CountDown = countdown <= 0 ? 30 : countdown;
               }
               sm.Confirm.Title = title;
               sm.Confirm.Content = content;
               sm.Confirm.ClickResult = callback;
               sm.Confirm.ConfirmEx = extend ?? ConfirmExModel.Build();
               sm.Confirm.MutiContent = element;
           }));
        }

        #endregion ShowConfirm

        private void ModulesChanged(ModulesChangeEvent obj)
        {
            ShowSystemButtons = true;
            HomeEnable = true;
            BackEnable = true;
            _buttonStack = obj.ButtonStack;
            _modulesChangeAction = obj.ModulesChangeAction;
            _resetAction = obj.ResetAction;
        }
    }

    public class InfoItem : BindableBase
    {
        private Uri _icon;

        public Uri Icon
        {
            get { return _icon; }
            set
            {
                _icon = value;
                OnPropertyChanged();
            }
        }

        private string _title;

        public string Title
        {
            get { return _title; }
            set
            {
                _title = value;
                OnPropertyChanged();
            }
        }

        private string _value;

        public string Value
        {
            get { return _value; }
            set
            {
                _value = value;
                OnPropertyChanged();
            }
        }
    }
}
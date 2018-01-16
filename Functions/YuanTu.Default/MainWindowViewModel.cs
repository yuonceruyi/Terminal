using System;
using System.Collections.Generic;
using System.Reflection;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using Prism.Commands;
using Prism.Regions;
using YuanTu.Consts;
using YuanTu.Consts.Enums;
using YuanTu.Consts.FrameworkBase;
using YuanTu.Consts.Models;
using YuanTu.Consts.Services;
using YuanTu.Core.FrameworkBase;
using YuanTu.Default.Part.Views;
using System.IO;
using Prism.Events;
using YuanTu.Consts.EventModels;
using System.Linq;
using System.Threading;
using Microsoft.Practices.ServiceLocation;
using YuanTu.Core.Log;
using YuanTu.Core.MultipScreen;
using YuanTu.Default.Component.Advertisement.Views;

namespace YuanTu.Default
{
    public class MainWindowViewModel : ViewModelBase, IShellViewModel
    {
        private readonly DispatcherTimer _timer;

        private bool _click;
        private bool _showNavigating;

        private int _timeOutSeconds;
        private bool _timeOutStop;
        private bool _viewhasInit;
        protected Dictionary<string, Size> SizeWithStrategy = new Dictionary<string, Size>()
        {
            [DeviceType.Default] = new Size(1280, 1024),
            [DeviceType.Clinic] = new Size(1080, 1920),
            [DeviceType.Tablet] = new Size(1280, 1024),
        };

        private Action _modulesChangeAction;
        private bool _isNewMode;
        private Action _resetAction;
        private Stack<List<ChoiceButtonInfo>> _buttonStack = new Stack<List<ChoiceButtonInfo>>();

        protected Size GetSizeWithStrategy()
        {
            var allstrategy = DeviceType.FallBackToDefaultStrategy;
            var current = allstrategy.ContainsKey(FrameworkConst.DeviceType) ? allstrategy[FrameworkConst.DeviceType].First() : DeviceType.Default;
            return SizeWithStrategy.ContainsKey(current) ? SizeWithStrategy[current] : SizeWithStrategy[DeviceType.Default];

        }

        public MainWindowViewModel()
        {
            _timer = new DispatcherTimer();
            _timer.Tick += Timer_Tick;
            _timer.Interval = new TimeSpan(0, 0, 1);
            BackBorderDoubleClickCommand = new DelegateCommand(BorderDoubleClick);
            Mask = new MaskModel(ShowContent);
        }

        public virtual double ScaleX
        {
            get { return View?.Width / GetSizeWithStrategy().Width ?? 1; }
        }

        public virtual double ScaleY
        {
            get { return View?.Height / GetSizeWithStrategy().Height ?? 1; }
        }

        public override string Title => "首页";
        public ICommand BackBorderDoubleClickCommand { get; private set; }

        public virtual ImageBrush Background
        {
            get
            {
                try
                {
                    if (FrameworkConst.CurrentTheme != Theme.Default &&
                        FrameworkConst.CurrentTheme != Theme.ClinicDefault)
                        return new ImageBrush(ResourceEngine.GetImageResource($"Main_{FrameworkConst.CurrentTheme}"));
                    return new ImageBrush(ResourceEngine.GetImageResource("Main"));
                }
                catch
                {
                    return new ImageBrush(ResourceEngine.GetImageResource("Main"));
                }
            }
        }

        public virtual Uri ADImageUrl
        {
            get { return _adImageUrl; }
            set { _adImageUrl = value;OnPropertyChanged(); }
        }

        private int _formCount { get; set; }

        public string FormCount => _formCount <= 0 ? "" : _formCount.ToString();

        public string WorkVersion
        {
            get
            {
                var assembly = Assembly.GetEntryAssembly().GetName();

                return $"{FrameworkConst.OperatorId}  V{assembly.Version} Beta";
            }
        }

        public Visibility LocalTestVisibility
            => FrameworkConst.Local && !FrameworkConst.IsDemo ? Visibility.Visible : Visibility.Collapsed;

        public int TransitionIndex => FrameworkConst.AnimationEnabled ? 1 : 0;

        public virtual bool ShowBack => ShowNavigating;

        public bool Click
        {
            get { return _click; }
            set
            {
                _click = value;
                OnPropertyChanged();
            }
        }

        public int TimeOutSeconds
        {
            get { return _timeOutSeconds; }
            set
            {
                _timeOutSeconds = value;
                _formCount = _timeOutSeconds;
                if (_formCount > 0)
                    _timer.Start();
                else
                {
                    _timer.Stop();
                }
                OnPropertyChanged("FormCount");
            }
        }

        public bool TimeOutStop
        {
            get { return _timeOutStop; }
            set
            {
                _timeOutStop = value;
                if (_timeOutStop)
                    _timer.Stop();
            }
        }

        public virtual void OnViewInit()
        {
            if (_viewhasInit)
                return;
            _viewhasInit = true;

            ShowNavigating = false;

            var manager = NavigationEngine;
            manager.HomeAddress = A.Home;
            manager.State = A.Home;
            InitRegisterView();
            ADImageUrl= ResourceEngine.GetImageResourceUri("医院广告图");



            var config = ServiceLocator.Current.GetInstance<IConfigurationManager>();
            if (config.GetValue("Advertisement:ShowWindow") == "1")
            {
                var adWin = new AdWindow();
                adWin.ShowInScreen();
                var shell= ServiceLocator.Current.GetInstance<IShell>() as Window;
                if (shell!=null)
                {
                    shell.Closed += (a, b) =>
                    {
                        try
                        {
                            adWin.Close();
                        }
                        catch (Exception e)
                        {

                        }
                    };
                }
            }

            var eventAggregator = GetInstance<IEventAggregator>();
            _isNewMode = config.GetValue("IsNewMode") == "1";
            if (_isNewMode)
            {
                eventAggregator.GetEvent<ModulesChangeEvent>().Subscribe(ModulesChanged);
                eventAggregator.GetEvent<TimeOutChangeEvent>().Subscribe(TimeOutChanged);
            }
        }
        

        public BusyModel Busy { get; set; } = new BusyModel();
        public AlertModel Alert { get; set; } = new AlertModel();
        public ConfirmModel Confirm { get; set; } = new ConfirmModel();
        public MaskModel Mask { get; set; }

        public virtual bool ShowNavigating
        {
            get { return _showNavigating; }
            set
            {
                _showNavigating = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(ShowBack));
            }
        }

        protected virtual void InitRegisterView()
        {
            var regionManager = GetInstance<IRegionManager>();
            regionManager.RegisterViewWithRegion(RegionNames.导航, typeof(NavigateBarView));
            regionManager.RegisterViewWithRegion(RegionNames.页尾, typeof(BottomBarView));
            regionManager.RegisterViewWithRegion(RegionNames.页首, typeof(TopBarView));
            regionManager.RegisterViewWithRegion(RegionNames.广告, typeof(CarouselView));
        }

        //public bool ShowNavigating
        //{
        //    get
        //    {
        //        var navigating = Application.Current.MainWindow.FindName("Navigating") as ColumnDefinition;
        //        return navigating.Width.Value != 0;
        //    }
        //    set
        //    {
        //        var navigating = Application.Current.MainWindow.FindName("Navigating") as ColumnDefinition;
        //        navigating.Width = new GridLength(value ? 1 : 0, GridUnitType.Star);
        //        var bckBorder = Application.Current.MainWindow.FindName("BackBorder") as Border;
        //        bckBorder.Opacity = (value ? 1 : -1)*Math.Abs(bckBorder.Opacity);
        //        OnPropertyChanged();
        //    }
        //}

        private void Timer_Tick(object sender, EventArgs e)
        {
            _formCount--;
            OnPropertyChanged("FormCount");
            if (_formCount > 0) return;
            _timer.Stop();
            if (_isNewMode)
            {
                DisablePreviewButton = true;
                DisableHomeButton = true;
            }
            var engine = NavigationEngine;
            if (engine.Current.Address == A.AdminPart)
            {
                Preview();
            }
            else
            {
                engine.State = engine.HomeAddress;
                _modulesChangeAction?.Invoke();
                _buttonStack.Clear();
                _resetAction?.Invoke();
            }

        }

        private void ShowContent(FrameworkElement element)
        {
            BeginInvoke(DispatcherPriority.ContextIdle, (Action)(() =>
           {
               var contentCtr = GetInstance<IShell>().Mask;
               contentCtr.Children.Clear();
               if (element != null)
                   contentCtr.Children.Add(element);
           }));
        }

        private void BorderDoubleClick()
        {
            if (FrameworkConst.DoubleClick)
            {
                var engine = NavigationEngine;
                var destVm = engine.Children?.Resolve(engine.Context, engine.State)?.DataContext;
                (destVm as ViewModelBase)?.DoubleClick();
            }
        }

        private int _clinicTopBorderHeight = 680;
        private Uri _adImageUrl;

        /// <summary>
        /// 诊间屏 上屏白底大小
        /// </summary>
        public int ClinicTopBorderHeight
        {
            get { return _clinicTopBorderHeight; }
            set
            {
                _clinicTopBorderHeight = value;
                OnPropertyChanged();
            }
        }

        private void ModulesChanged(ModulesChangeEvent obj)
        {
            _modulesChangeAction = obj.ModulesChangeAction;
            _resetAction = obj.ResetAction;
            _buttonStack = obj.ButtonStack;
            TimeOut = 15;
        }


        private void TimeOutChanged(TimeOutChangeEvent obj)
        {
            TimeOut = obj.TimeOut;
        }
    }
}
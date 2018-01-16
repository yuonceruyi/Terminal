using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using Prism.Commands;
using Prism.Events;
using YuanTu.Consts.EventModels;
using YuanTu.Consts.Services;
using YuanTu.Core.FrameworkBase;
using YuanTu.Core.Navigating;

namespace YuanTu.Default.House.Part.ViewModels
{
    public class BottomBarViewModel : ViewModelBase
    {
        private Visibility _systemButtonsVisibility = Visibility.Collapsed;
        private IReadOnlyCollection<ImageSource> _logoGroup;
        private bool _homeEnable=true;
        private bool _backEnable=true;

        public BottomBarViewModel(IEventAggregator eventAggregator)
        {
            var res = GetInstance<IResourceEngine>();
            BackUri = res.GetImageResourceUri("按钮图标_返回");
            HomeUri = res.GetImageResourceUri("按钮图标_主页");
            SystemButtonCommand = new DelegateCommand<string>(ButtonClick, tx =>
            {
                //var engine = NavigationEngine;
                //if (tx == "返回")
                //{
                //    var ctx = new FormContext {Context = engine.Context, Address = engine.State};
                //    return true/*engine.HasPrev(ctx)&& BackEnable*/;
                //}else if (tx== "主页")
                //{
                //    return true/*HomeEnable*/;
                //}
                return true;
            });
            eventAggregator.GetEvent<ViewChangeEvent>().Subscribe(ViewHasChanged);
            eventAggregator.GetEvent<SystemInfoEvent>().Subscribe(SystemInfoChanged);
            var resource =res;
            LogoGroup=new List<ImageSource>
            {
                resource.GetImageResource("Logo_APP"),
                resource.GetImageResource("Logo_公众号"),
                resource.GetImageResource("Logo_银行"),
                resource.GetImageResource("Logo_远图"),
            };
        }

        public override string Title => "底部任务栏";
        public DelegateCommand<string> SystemButtonCommand { get; }
        public Uri HomeUri { get; set; }
        public Uri BackUri { get; set; }

        public IReadOnlyCollection<ImageSource> LogoGroup
        {
            get { return _logoGroup; }
            set
            {
                _logoGroup = value;
                OnPropertyChanged();
            }
        }

        public Visibility SystemButtonsVisibility
        {
            get { return _systemButtonsVisibility; }
            set
            {
                _systemButtonsVisibility = value;
                OnPropertyChanged();
            }
        }

        public bool HomeEnable
        {
            get { return _homeEnable; }
            set
            {
                _homeEnable = value;
                OnPropertyChanged();
            }
        }

        public bool BackEnable
        {
            get { return _backEnable; }
            set
            {
                _backEnable = value;
                OnPropertyChanged();
            }
        }

        public string NotificMessage { get; set; } = "客服:0571-89916777";

        private void ViewHasChanged(ViewChangeEvent eveEvent)
        {
            var ishome = NavigationEngine.IsHome(eveEvent.To);
            SystemButtonsVisibility = ishome ? Visibility.Collapsed : Visibility.Visible;
            SystemButtonCommand.RaiseCanExecuteChanged();
          
        }

        private void SystemInfoChanged(SystemInfoEvent eveEvent)
        {
            BackEnable = !eveEvent.DisablePreviewButton;
            HomeEnable = !eveEvent.DisableHomeButton;
            SystemButtonCommand.RaiseCanExecuteChanged();
        }

        protected virtual void ButtonClick(string cmd)
        {
            var engine = NavigationEngine;
            switch (cmd)
            {
                case "主页":
                    engine.State = engine.HomeAddress;
                    break;

                case "返回":
                {
                    var ctx = new FormContext (engine.Context, engine.State);
                    if (engine.HasPrev(ctx) && BackEnable)
                    {
                        engine.Prev(ctx);
                    }
                    else
                    {
                        engine.State = engine.HomeAddress;
                    }

                }
                    break;
            }
        }
    }
}
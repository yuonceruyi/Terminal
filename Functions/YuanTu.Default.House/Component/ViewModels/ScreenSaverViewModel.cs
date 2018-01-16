using Prism.Commands;
using Prism.Regions;
using System;
using System.Windows.Input;
using YuanTu.Consts;
using YuanTu.Consts.Services;
using YuanTu.Core.FrameworkBase;
using YuanTu.Default.House.Device.Gate;

namespace YuanTu.Default.House.Component.ViewModels
{
    public class ScreenSaverViewModel : ViewModelBase
    {
        public override string Title => "屏保";

        public ScreenSaverViewModel()
        {
            ClickCommand = new DelegateCommand(DoClick);
        }

        public override void OnSet()
        {
            HideNavigating = true;
            TimeOut = 0;
            屏保二维码2Uri = ResourceEngine.GetImageResourceUri("屏保民生卡二维码_House");
            屏保二维码1Uri = ResourceEngine.GetImageResourceUri("屏保慧医二维码_House");
            屏保手机Uri = ResourceEngine.GetImageResourceUri("屏保手机_House");
        }

        public ICommand ClickCommand { get; set; }
        public override bool DisableHomeButton { get; set; } = true;
        public override bool DisablePreviewButton { get; set; } = true;

        private Uri _屏保手机Uri;

        public Uri 屏保手机Uri
        {
            get { return _屏保手机Uri; }
            set
            {
                _屏保手机Uri = value;
                OnPropertyChanged();
            }
        }

        private Uri _屏保二维码1Uri;

        public Uri 屏保二维码1Uri
        {
            get { return _屏保二维码1Uri; }
            set
            {
                _屏保二维码1Uri = value;
                OnPropertyChanged();
            }
        }

        private Uri _屏保二维码2Uri;

        public Uri 屏保二维码2Uri
        {
            get { return _屏保二维码2Uri; }
            set
            {
                _屏保二维码2Uri = value;
                OnPropertyChanged();
            }
        }

        public virtual string 屏保文本1 { get; set; } = "下载慧医APP/番禺民生卡APP\n手机上查看您的家庭健康数据";
        public virtual string 屏保文本2 { get; set; } = "下载「慧医」APP\n您随身的健康管家";
        public virtual string 屏保文本3 { get; set; } = "下载「民生卡」APP\n     体验更多服务";

        public virtual void DoClick()
        {
            Navigate(A.Home);
        }

        public override void OnEntered(NavigationContext navigationContext)
        {
            GetInstance<IGateService>().CloseGateAsync();
        }
    }
}
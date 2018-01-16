using System;
using Prism.Regions;
using YuanTu.Consts.Services;
using YuanTu.Core.FrameworkBase;

namespace YuanTu.Default.Part.ViewModels
{
    public class MaintenanceViewModel : ViewModelBase
    {
        private Uri _image;
        public override string Title { get; }

        public Uri Image
        {
            get { return _image; }
            set
            {
                _image = value;
                OnPropertyChanged();
            }
        }

        public override bool DisableHomeButton { get; set; } = true;
        public override bool DisablePreviewButton { get; set; } = true;

        public override void OnEntered(NavigationContext navigationContext)
        {
            HideNavigating = true;
            TimeOut = 0;
            Image = ResourceEngine.GetImageResourceUri("提示_维护");
            base.OnEntered(navigationContext);
        }
    }
}
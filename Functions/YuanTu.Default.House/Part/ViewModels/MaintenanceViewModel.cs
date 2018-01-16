using System;
using YuanTu.Consts.Services;
using YuanTu.Core.FrameworkBase;

namespace YuanTu.Default.House.Part.ViewModels
{
    public class MaintenanceViewModel : ViewModelBase
    {
        private Uri _image;
        private string _reason;

        public override string Title { get; } = "维护";

        public string Reason
        {
            get { return _reason; }
            set
            {
                _reason = value;
                OnPropertyChanged();
            }
        }

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

        public override void OnSet()
        {
            HideNavigating = true;
            TimeOut = 0;
            Image = ResourceEngine.GetImageResourceUri("系统维护中_House");
        }
    }
}
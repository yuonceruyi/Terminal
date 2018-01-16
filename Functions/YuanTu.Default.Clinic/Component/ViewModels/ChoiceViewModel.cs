using System;
using Prism.Regions;
using YuanTu.Consts.Services;
using YuanTu.Consts.UserControls;

namespace YuanTu.Default.Clinic.Component.ViewModels
{
    public class ChoiceViewModel : Default.Component.ViewModels.ChoiceViewModel
    {
        private VideoPlayerState _videoPlayerState;
        private Uri _videoUri;
        private int _volume = GetInstance<IConfigurationManager>().GetValueInt("Clinic:Volume", 80);

        public VideoPlayerState VideoPlayerState
        {
            get { return _videoPlayerState; }
            set
            {
                _videoPlayerState = value;
                OnPropertyChanged();
            }
        }

        public Uri VideoUri
        {
            get { return _videoUri; }
            set
            {
                _videoUri = value;
                OnPropertyChanged();
            }
        }

        public int Volume
        {
            get { return _volume; }
            set
            {
                _volume = value;
                OnPropertyChanged();
            }
        }

        public override void OnSet()
        {
            if (!string.IsNullOrWhiteSpace(Startup.VideoPath))
            {
                var uri = new Uri(Startup.VideoPath, UriKind.Absolute);
                VideoUri = uri;
            }
        }

        public override void OnEntered(NavigationContext navigationContext)
        {
            base.OnEntered(navigationContext);
            Play();
        }

        public override bool OnLeaving(NavigationContext navigationContext)
        {
            Pause();
            return base.OnLeaving(navigationContext);
        }

        protected virtual void Play()
        {
            VideoPlayerState = VideoPlayerState.Play;
        }

        protected virtual void Pause()
        {
            VideoPlayerState = VideoPlayerState.Pause;
        }
    }
}
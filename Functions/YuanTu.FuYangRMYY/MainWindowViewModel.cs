using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;
using Microsoft.Practices.ServiceLocation;
using Prism.Events;
using Prism.Regions;
using YuanTu.Consts;
using YuanTu.Consts.Services;
using YuanTu.Consts.UserControls;

namespace YuanTu.FuYangRMYY.Clinic
{
    public class MainWindowViewModel:YuanTu.Default.MainWindowViewModel
    {
        
        public MainWindowViewModel(IEventAggregator  eventAggregator)
        {
            eventAggregator.GetEvent<VideoEvent>().Subscribe((e) =>
            {
                if (e.eventStatus == 1)
                {
                    SetVideoUrl();
                }
                else
                {
                    VideoPlayerState = VideoPlayerState.Play;
                }
            }); 
        }

        private void SetVideoUrl()
        {

            var manager = ServiceLocator.Current.GetInstance<IConfigurationManager>();
            var path = manager.GetValue("Clinic:VideoPath");
            string VideoPath = "";
            if (!string.IsNullOrWhiteSpace(path))
            {
                if ((path?.StartsWith("h") ?? false) || Path.IsPathRooted(path))
                {
                    VideoPath = path;
                }
                else
                {
                    VideoPath = Path.Combine(FrameworkConst.RootDirectory, manager.GetValue("Clinic:VideoPath"));
                }
                if (!string.IsNullOrWhiteSpace(VideoPath))
                {
                    var uri = new Uri(VideoPath, UriKind.Absolute);
                    VideoUri = uri;
                    Volume= ServiceLocator.Current.GetInstance<IConfigurationManager>().GetValueInt("Clinic:Volume", 80);
                }
            }
        }

        private VideoPlayerState _videoPlayerState;
        private Uri _videoUri;
        private int _volume = 0;
        

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

        public VideoPlayerState VideoPlayerState
        {
            get { return _videoPlayerState; }
            set
            {
                _videoPlayerState = value;
                OnPropertyChanged();
            }
        }
    }
    public class VideoEvent : PubSubEvent<VideoEvent>
    {
        public int eventStatus;
    }
}

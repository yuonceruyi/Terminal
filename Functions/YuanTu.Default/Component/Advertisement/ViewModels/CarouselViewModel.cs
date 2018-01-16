using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Windows.Threading;
using Microsoft.Practices.ServiceLocation;
using YuanTu.Consts;
using YuanTu.Consts.Services;
using YuanTu.Consts.UserControls;
using YuanTu.Core.Advertisement;
using YuanTu.Core.Advertisement.Data;
using YuanTu.Core.Extension;
using YuanTu.Core.FrameworkBase;

namespace YuanTu.Default.Component.Advertisement.ViewModels
{
    internal class CarouselViewModel : ViewModelBase
    {
        private DoubleScreenAdContent _currentContent;
        private long _currentId;
        private readonly string _defaultUrl;
        private readonly CacheService _cacheService;
        private readonly IConfigurationManager _config;

        public CarouselViewModel()
        {
            var config = ServiceLocator.Current.GetInstance<IConfigurationManager>();

            _config = config;

            var defaultUrl = config.GetValue("Advertisement:DefaultUrl");
            if (Uri.TryCreate(defaultUrl, UriKind.Absolute, out var uri))
            {
                _defaultUrl = defaultUrl;
            }
            else
            {
                var resource = ServiceLocator.Current.GetInstance<IResourceEngine>();
                _defaultUrl = resource.GetResourceFullPath(defaultUrl);
            }

            var root = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Cache");
            if (!Directory.Exists(root))
                Directory.CreateDirectory(root);
            _cacheService = new CacheService(root, defaultUrl);

            new Thread(RefreshAd)
            {
                Name = "RefreshAd",
                IsBackground = true
            }.Start();
        }

        public override string Title => "幻灯片";

        protected virtual void RefreshAd()
        {
            var failDelay = 30 * 1000;
            var defaultDelay = 10 * 1000;
            var totalDelay = defaultDelay;

            while (true)
            {
                AdvertisementHandler.AdServerUrl = _config.GetValue("Advertisement:Url");
                FindDoubleScreenAdRes res;
                if (FrameworkConst.FakeServer)
                {
                    var result = AdvertisementHandler.Handle(
                        new MockReq("/adApi/open/findDoubleScreenAd.json",
                            new Dictionary<string, string>()
                            {
                                ["corpId"] = "261",
                                ["adPositionId"] = "27",
                                ["deviceMac"] = "EC-D6-8A-0A-2F-54",
                            }));
                    res = result ? result.Value.ToJsonObject<FindDoubleScreenAdRes>() : null;
                }
                else
                {
                    res = AdvertisementHandler.获取双屏广告(new FindDoubleScreenAdReq());
                }
                if (res?.data == null)
                {
                    // 启动时第一个请求失败
                    if (_currentContent == null)
                        PlayThreadSafe(null);
                    Thread.Sleep(failDelay);
                    continue;
                }
                try
                {
                    var contents = res.data.adList.SelectMany(i => i.contentList).ToList();
                    _cacheService.Request(contents.Select(c => c.url));
                    _currentId++;
                    var id = _currentId;
                    if (!contents.Any())
                        PlayThreadSafe(null);
                    else
                        new Thread(Start)
                        {
                            Name = $"PlayList[{id}]",
                            IsBackground = true
                        }.Start(new PlayList(id, contents));

                    totalDelay = contents.Sum(i => i.showTime) * 1000;
                    Thread.Sleep(totalDelay > 0 ? totalDelay : defaultDelay);
                }
                catch
                {
                    Thread.Sleep(defaultDelay);
                }
            }
        }

        private void Start(object o)
        {
            try
            {
                var playList = (PlayList)o;
                var id = playList.Id;
                var contents = playList.Contents;
                while (true)
                    foreach (var content in contents)
                    {
                        if (_currentId != id)
                            return;
                        PlayThreadSafe(content);
                        Thread.Sleep(content.showTime * 1000);
                    }
            }
            catch
            {
                //
            }
        }

        private void PlayThreadSafe(DoubleScreenAdContent content)
        {
            Invoke(DispatcherPriority.ContextIdle, () => { Play(content); });
        }

        private void Play(DoubleScreenAdContent content)
        {
            try
            {
                if (content == null)
                {
                    BackgroundUri = _defaultUrl;
                    VideoMode = false;
                    VideoPlayerState = VideoPlayerState.Stop;
                    return;
                }
                var url = _cacheService.Get(content.url, out var ready);

                if (content.type == 1 || !ready)
                {
                    BackgroundUri = url;
                    VideoMode = false;
                    VideoPlayerState = VideoPlayerState.Stop;
                }
                else if (content.type == 2)
                {
                    if (_currentContent?.type == 2)
                        VideoPlayerState = VideoPlayerState.Stop;
                    Volume = _config.GetValueInt("Advertisement:Volume", 80);
                    VideoUri = new Uri(url);
                    VideoMode = true;
                    VideoPlayerState = VideoPlayerState.Play;
                }
            }
            catch
            {
                //
            }
            finally
            {
                _currentContent = content;
            }
        }

        private class PlayList
        {
            public PlayList()
            {
            }

            public PlayList(long id, List<DoubleScreenAdContent> contents)
            {
                Id = id;
                Contents = contents;
            }

            public long Id { get; }

            public List<DoubleScreenAdContent> Contents { get; }
        }

        #region Bindings

        private string _backgroundUri;

        public string BackgroundUri
        {
            get => _backgroundUri;
            set
            {
                _backgroundUri = value;
                OnPropertyChanged();
            }
        }

        private VideoPlayerState _videoPlayerState;

        public VideoPlayerState VideoPlayerState
        {
            get => _videoPlayerState;
            set
            {
                _videoPlayerState = value;
                OnPropertyChanged();
            }
        }

        private Uri _videoUri;

        public Uri VideoUri
        {
            get => _videoUri;
            set
            {
                _videoUri = value;
                OnPropertyChanged();
            }
        }

        private int _volume;

        public int Volume
        {
            get => _volume;
            set
            {
                _volume = value;
                OnPropertyChanged();
            }
        }

        private bool _videoMode;

        public bool VideoMode
        {
            get => _videoMode;
            set
            {
                _videoMode = value;
                OnPropertyChanged();
            }
        }

        #endregion Bindings
    }
}
using System;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms.Integration;
using Vlc.DotNet.Core;
using Vlc.DotNet.Forms;

namespace YuanTu.Consts.UserControls
{
    public class VideoPlayer : WindowsFormsHost
    {
        private const int DefaultVolume = 80;
        public static string VlcLibDirectory { get; set; }
            = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "External", "vlc");

        protected readonly VlcControl MediaPlayer;

        public VideoPlayer()
        {
            if (!EnsureVlcLib())
                return;
            MediaPlayer = new VlcControl
            {
                VlcLibDirectory = new DirectoryInfo(VlcLibDirectory)
            };
            MediaPlayer.EndInit();
            MediaPlayer.EndReached += MediaPlayerOnEndReached;
            MediaPlayer.EncounteredError += MediaPlayerOnEncounteredError;
            Child = MediaPlayer;
            MediaPlayer.Audio.Volume = DefaultVolume;
            MediaPlayer.Video.AspectRatio = "1920:808";
        }

        private bool EnsureVlcLib()
        {
            if (Directory.Exists(VlcLibDirectory))
                return true;
            var root = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "External");
            var zipFilePath = Path.Combine(root, "vlc.zip");
            var zipFile = new FileInfo(zipFilePath);
            if (!zipFile.Exists)
                return false;
            try
            {
                using (var fs = zipFile.OpenRead())
                using (var zip = new ZipArchive(fs, ZipArchiveMode.Read))
                {
                    foreach (var entry in zip.Entries)
                    {
                        var path = Path.Combine(root, entry.FullName);
                        var dir = Path.GetDirectoryName(path);
                        if (!Directory.Exists(dir))
                            Directory.CreateDirectory(dir);
                        if (entry.FullName.EndsWith("/"))
                            continue;
                        using (var efs = File.OpenWrite(path))
                        using (var es = entry.Open())
                        {
                            es.CopyTo(efs);
                        }
                    }
                }
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private void MediaPlayerOnEndReached(object sender, VlcMediaPlayerEndReachedEventArgs args)
        {
            Dispatcher.BeginInvoke(new Action(() => { MediaPlayer.Play(Uri); }));
        }

        private void MediaPlayerOnEncounteredError(object sender, VlcMediaPlayerEncounteredErrorEventArgs args)
        {
            Task.Run(async () =>
            {
                // 10秒后重试
                await Task.Delay(10000);
                await Dispatcher.BeginInvoke(new Action(() =>
                {
                    if(!MediaPlayer.IsPlaying)
                        MediaPlayer.Play(Uri);
                }));
            });
        }

        #region Uri

        /// <summary>
        /// 视频的地址
        /// </summary>
        public static readonly DependencyProperty UriProperty = DependencyProperty.Register(
            "Uri", typeof (Uri), typeof (VideoPlayer), new FrameworkPropertyMetadata((obj, e) =>
            {
                var control = obj as VideoPlayer;
                var uri = e.NewValue as Uri;
                control?.MediaPlayer.SetMedia(uri);
            }));
        
        public Uri Uri
        {
            get { return (Uri) GetValue(UriProperty); }
            set { SetValue(UriProperty, value); }
        }

        #endregion

        #region VideoPlayerState
        
        /// <summary>
        /// 设置视频状态
        /// </summary>
        public static readonly DependencyProperty VideoPlayerStateProperty = DependencyProperty.Register(
            "VideoPlayerState", typeof (VideoPlayerState), typeof (VideoPlayer), new FrameworkPropertyMetadata(
                (obj, e) =>
                {
                    var control =  obj as VideoPlayer;
                    var state = (VideoPlayerState)e.NewValue;
                    if (control!=null)
                    {
                        switch (state)
                        {
                            case VideoPlayerState.Stop:
                                control.MediaPlayer.Stop();
                                break;
                            case VideoPlayerState.Play:
                                control.MediaPlayer.Play();
                                break;
                            case VideoPlayerState.Pause:
                                control.MediaPlayer.Pause();
                                break;
                            default:
                                break;
                        }
                    }
                }));
        
        public VideoPlayerState VideoPlayerState
        {
            get { return (VideoPlayerState) GetValue(VideoPlayerStateProperty); }
            set { SetValue(VideoPlayerStateProperty, value); }
        }

        #endregion

        #region Volume

        /// <summary>
        ///  设置音量 0-100
        /// </summary>
        public static readonly DependencyProperty VolumeProperty = DependencyProperty.Register(
            "Volume", typeof (int), typeof (VideoPlayer), new FrameworkPropertyMetadata(DefaultVolume, (obj, e) =>
            {
                var control = obj as VideoPlayer;
                var vol = (int)e.NewValue;
                if (control != null)
                {
                    control.MediaPlayer.Audio.Volume = vol < 0 ? 0 : Math.Min(100,vol);
                }
            }));

        public int Volume
        {
            get { return (int) GetValue(VolumeProperty); }
            set { SetValue(VolumeProperty, value); }
        }

        #endregion

        #region AspectRatio

        public static readonly DependencyProperty AspectRatioProperty = DependencyProperty.Register(
            nameof(AspectRatio), typeof(string), typeof(VideoPlayer), new FrameworkPropertyMetadata("1920:808",
                (d, e) =>
                {
                    var control = d as VideoPlayer;
                    var ar = (string)e.NewValue;
                    if (control != null)
                    {
                        control.MediaPlayer.Video.AspectRatio = ar;
                    }
                }));

        public string AspectRatio
        {
            get { return (string) GetValue(AspectRatioProperty); }
            set { SetValue(AspectRatioProperty, value); }
        }

        #endregion
    }

    public enum VideoPlayerState
    {
        Stop,
        Play,
        Pause
    }
}
using System;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;
using Microsoft.Practices.ServiceLocation;
using YuanTu.Consts;
using YuanTu.Consts.FrameworkBase;
using YuanTu.Consts.Services;
using YuanTu.Core.Extension;

namespace YuanTu.Core.Services.AudioService
{
    public class AudioPlayer : IAudioPlayer
    {
        private MediaPlayer _player = null;

        public string ServiceName => "AudioPlayer";

        public AudioPlayer()
        {
            var shell = ServiceLocator.Current.GetInstance<IShell>() as Window;
            if (shell.Dispatcher.CheckAccess())
            {
                _player = new MediaPlayer();
            }
            else
            {
                shell.Dispatcher.Invoke(DispatcherPriority.ContextIdle,
                    (Action)(() => { _player = new MediaPlayer(); }));
            }
        }

        /// <summary>
        ///     开始播放
        /// </summary>
        /// <param name="fileName"></param>
        public void StartPlayAsync(string fileName)
        {
            if (!FrameworkConst.AudioGuideEnabled)
                return;
            if (fileName.IsNullOrWhiteSpace() || !File.Exists(fileName))
                return;
            StopPlayerAsync();
            if (_player.Dispatcher.CheckAccess())
            {
                _player.Open(new Uri(Path.GetFullPath(fileName), UriKind.Absolute));
                _player.Play();
            }
            else
            {
                _player?.Dispatcher.Invoke(DispatcherPriority.ContextIdle, (Action)(() =>
                {
                    _player.Open(new Uri(Path.GetFullPath(fileName), UriKind.Absolute));
                    _player.Play();
                }));
            }
        }

        /// <summary>
        ///     结束播放
        /// </summary>
        public void StopPlayerAsync()
        {
            if (!FrameworkConst.AudioGuideEnabled)
                return;

            if (_player.Dispatcher.CheckAccess())
            {
                _player.Stop();
            }
            else
            {
                _player?.Dispatcher.Invoke(DispatcherPriority.ContextIdle, (Action)(() => { _player.Stop(); }));
            }
        }
    }
}
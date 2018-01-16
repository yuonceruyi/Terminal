using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Accord.Video;
using Accord.Video.DirectShow;
using Prism.Commands;
using Prism.Regions;
using YuanTu.Consts.Services;
using YuanTu.Consts.Sounds;
using YuanTu.Core.FrameworkBase;
using YuanTu.Core.FrameworkBase.Loadings;
using YuanTu.Core.Log;
using YuanTu.QDKouQiangYY.Component.Auth.Views;
using Image = System.Windows.Controls.Image;

namespace YuanTu.QDKouQiangYY.Component.Auth.ViewModels
{
    public abstract class FaceRecViewModelBase : ViewModelBase
    {
        #region DataBinding


        private DelegateCommand _confirmCommand;

        public DelegateCommand ConfirmCommand
        {
            get { return _confirmCommand; }
            set
            {
                _confirmCommand = value;
                OnPropertyChanged();
            }
        }

        private DelegateCommand _delayCommand;

        public DelegateCommand DelayCommand
        {
            get { return _delayCommand; }
            set
            {
                _delayCommand = value;
                OnPropertyChanged();
            }
        }

        private Uri _maskUri;

        public Uri MaskUri
        {
            get { return _maskUri; }
            set
            {
                _maskUri = value;
                OnPropertyChanged();
            }
        }

        private int? _snapShotTimeOut;

        public int? SnapShotTimeOut
        {
            get { return _snapShotTimeOut; }
            set
            {
                _snapShotTimeOut = value;
                OnPropertyChanged();
            }
        }

        private string _hint;

        public string Hint
        {
            get { return _hint; }
            set
            {
                _hint = value;
                OnPropertyChanged();
            }
        }

        #endregion

        public override void OnSet()
        {
            InitCamera();

            _timer = new DispatcherTimer()
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            _timer.Tick += OnTick;

            MaskUri = ResourceEngine.GetImageResourceUri("刷脸_Mask");

            Hint = "请脱掉帽子、眼镜、口罩等遮挡物、正对屏幕站立，确保光线充足尽可能的充满头像轮廓";

            ConfirmCommand = new DelegateCommand(Confirm);
            DelayCommand = new DelegateCommand(Delay);
        }

        private void OnTick(object s, EventArgs a)
        {
            SnapShotTimeOut -= 1;
            if (SnapShotTimeOut != 0)
                return;
            Confirm();
        }

        public override void OnEntered(NavigationContext navigationContext)
        {
            PlaySound(SoundMapping.调整站姿使面部图像处于头部轮廓中);
            _running = true;
            _device?.Start();
        }

        public override bool OnLeaving(NavigationContext navigationContext)
        {
            _running = false;
            _device?.SignalToStop();
            return true;
        }

        private void Confirm()
        {
            _timer.Stop();
            SnapShotTimeOut = null;


            DoCommand(lp =>
            {
                _snapShotImage = null;
                _trigger = true;

                while (_trigger)
                {
                    Thread.Sleep(10);
                }

                if (_snapShotImage == null)
                    return;

                string imageData;
                using (var ms = new MemoryStream())
                {
                    _snapShotImage.Save(ms, ImageFormat.Jpeg);
                    imageData = Convert.ToBase64String(ms.ToArray());
                }
                Act(lp, imageData);
            });
        }

        protected virtual void Act(LoadingProcesser lp, string imageData)
        {
        }

        private void Delay()
        {
            SnapShotTimeOut = 5;
            _timer.Start();
        }

        private DispatcherTimer _timer;

        private bool _trigger = false;
        private Bitmap _snapShotImage;

        #region Camera

        private VideoCaptureDevice _device;

        private long _frameCount;

        private Image _image;
        //private long _lastFrameCount;
        //private DateTime _lastTime;
        //private long _renderframeCount;

        private bool _running;

        private void InitCamera()
        {
            _image = (View as FaceRecView)?.Image;
            if (_image == null)
                return;

            var devices = new FilterInfoCollection(FilterCategory.VideoInputDevice);

            if (devices.Count == 0)
            {
                Logger.Device.Info("未找到摄像设备");
                return;
            }
            var builder = new StringBuilder();
            foreach (FilterInfo d in devices)
                builder.AppendLine($"{d.Name} {d.MonikerString}");
            Logger.Device.Info($"找到{devices.Count}个摄像设备:\n{builder}");

            var device = devices[devices.Count - 1];
            Logger.Device.Info($"选择设备:{device.Name} {device.MonikerString}");
            var monikerString = device.MonikerString;
            _device = new VideoCaptureDevice(monikerString);

            _device.VideoResolution = _device.VideoCapabilities
                .OrderByDescending(vc => vc.FrameSize.Width * vc.FrameSize.Height)
                .First();

            _device.NewFrame += DeviceOnNewFrame;
        }

        private void DeviceOnNewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            if (_trigger)
            {
                _snapShotImage = (Bitmap)eventArgs.Frame.Clone();
                _trigger = false;
            }

            _frameCount++;
            if (!_running || _frameCount % 2 != 0)
                return;
            var frame = eventArgs.Frame;
            try
            {
                View.Dispatcher.Invoke(() =>
                {
                    (_image.Source as BitmapImage)?.StreamSource?.Dispose();

                    _image.Source = BitmapToImageSource(frame);
                    //_renderframeCount++;
                    //var now = DateTime.Now;
                    //var timeSpan = now - _lastTime;
                    //if (!(timeSpan.TotalMilliseconds > 500))
                    //    return;
                    //_lastTime = now;
                    //var frames = renderframeCount - lastFrameCount;
                    //var fps = frames / timeSpan.TotalSeconds;
                    //Title = $"FPS: {fps:F2}";
                    //_lastFrameCount = _renderframeCount;
                });
            }
            catch
            {
                _device.SignalToStop();
            }
        }

        private BitmapImage BitmapToImageSource(Bitmap bitmap)
        {
            using (var memory = new MemoryStream())
            {
                bitmap.Save(memory, ImageFormat.Bmp);
                memory.Position = 0;
                var bitmapimage = new BitmapImage();
                bitmapimage.BeginInit();
                bitmapimage.StreamSource = memory;
                bitmapimage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapimage.EndInit();

                return bitmapimage;
            }
        }

        #endregion Camera
    }
}
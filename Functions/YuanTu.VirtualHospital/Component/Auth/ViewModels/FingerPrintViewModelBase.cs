using System;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Prism.Regions;
using YuanTu.Consts.Services;
using YuanTu.Consts.Sounds;
using YuanTu.Core.FrameworkBase;
using YuanTu.Core.FrameworkBase.Loadings;
using YuanTu.Core.Log;
using YuanTu.Core.Reporter;
using YuanTu.Devices.FingerPrint;

namespace YuanTu.VirtualHospital.Component.Auth.ViewModels
{
    internal abstract class FingerPrintViewModelBase : ViewModelBase
    {
        protected IFingerPrintDevice _fingerPrintDevice;

        public FingerPrintViewModelBase(IFingerPrintDevice[] fingerPrintDevices)
        {
            _fingerPrintDevice = fingerPrintDevices?.FirstOrDefault(p=>p.DeviceId== "WE_FP");
        }
        protected bool _working;

        public override string Title => "指纹识别";

        public override void OnSet()
        {
            BackUri = ResourceEngine.GetImageResourceUri("提示_指纹识别");
        }

        public override void OnEntered(NavigationContext navigationContext)
        {
            PlaySound(SoundMapping.请按操作示范将右手食指平放于指纹仪);
            Task.Run(() => StartRead());
        }

        protected virtual void StartRead()
        {
            string imageData = null;
            try
            {
                var ret = _fingerPrintDevice.Connect();
                if (!ret.IsSuccess)
                {
                    ReportService.读卡器离线(null, ErrorSolution.读卡器离线);
                    ShowAlert(false, "友好提示", $"指纹识别器打开失败({ret.ResultCode})", debugInfo: ret.Message);
                    return;
                }
                if (!_fingerPrintDevice.Initialize().IsSuccess)
                {
                    ShowAlert(false, "友好提示", $"指纹识别器初始化失败({ret.ResultCode})", debugInfo: ret.Message);
                    return;
                }
                _working = true;
                while (_working)
                {
                    var rest = _fingerPrintDevice.GetFingerImage();
                    if (rest.IsSuccess)
                    {
                        var image = rest.Value;
                        using (var ms = new MemoryStream())
                        {
                            image.Save(ms, ImageFormat.Jpeg);
                            imageData = Convert.ToBase64String(ms.ToArray());
                        }
                        Logger.Main.Info("[指纹识别成功]");
                        break;
                    }
                    Thread.Sleep(300);
                }
            }
            finally
            {
                _fingerPrintDevice.UnInitialize();
                if (_working && imageData != null)
                    OnGetInfo(imageData);
            }
        }

        protected virtual void StopRead()
        {
            _fingerPrintDevice?.DisConnect();
        }

        public override bool OnLeaving(NavigationContext navigationContext)
        {
            _working = false;

            StopRead();
            return base.OnLeaving(navigationContext);
        }

        protected virtual void OnGetInfo(string imageData)
        {
            DoCommand(lp => { Act(lp, imageData); });
        }

        protected virtual void Act(LoadingProcesser lp, string imageData)
        {

        }

        public override void DoubleClick()
        {
            OnGetInfo(null);
        }

        #region Bindings

        private string _hint = "请按提示按指纹";

        public string Hint
        {
            get { return _hint; }
            set
            {
                _hint = value;
                OnPropertyChanged();
            }
        }

        private Uri _backUri;

        public Uri BackUri
        {
            get { return _backUri; }
            set
            {
                _backUri = value;
                OnPropertyChanged();
            }
        }

        private string _tipContent = "请将右手食指按示范操作平放于指纹仪";

        public string TipContent
        {
            get { return _tipContent; }
            set
            {
                _tipContent = value;
                OnPropertyChanged();
            }
        }

        #endregion Bindings

    }
}
using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Prism.Commands;
using Prism.Regions;
using YuanTu.Consts;
using YuanTu.Consts.Services;
using YuanTu.Consts.Sounds;
using YuanTu.Core.Extension;
using YuanTu.Core.FrameworkBase;
using YuanTu.Core.FrameworkBase.Loadings;
using YuanTu.Core.Log;
using YuanTu.Core.Reporter;
using YuanTu.Devices.FingerPrint;

namespace YuanTu.ChongQingArea.Component.Auth.ViewModels
{
    public abstract class FingerPrintViewModelBase : ViewModelBase
    {
        protected IFingerPrintDevice _fingerPrintDevice;

        public FingerPrintViewModelBase(IFingerPrintDevice[] fingerPrintDevices)
        {
            _fingerPrintDevice = fingerPrintDevices?.FirstOrDefault();
            SkipCommand = new DelegateCommand(SkipCmd);
        }
        protected bool _working;

        public override string Title => "指纹识别";

        public ICommand JumpFingerPrint { get; set; }

        public ICommand SkipCommand { get; set; }
        private string _skipContent;
        public string SkipContent
        {
            get { return _skipContent; }
            set
            {
                _skipContent = value;
                OnPropertyChanged();
            }
        }
        public override void OnSet()
        {
            BackUri = ResourceEngine.GetImageResourceUri("提示_指纹识别");
        }

        public override void OnEntered(NavigationContext navigationContext)
        {
            Logger.Main.Info($"[进入页面]");
            Task.Run(() => StartRead());
            SkipContent = "跳过";

        }
        protected virtual void SkipCmd()
        {
            
            Logger.Main.Info($"跳过指纹");
            _working = false;
            _fingerPrintDevice.UnInitialize();
            _fingerPrintDevice.DisConnect();
            OnGetInfo(new List<string>(2));
        }

        protected virtual void StartRead()
        {
            List<string> imageDataList = new List<string>();
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
                PlaySound(SoundMapping.请按操作示范将右手食指平放于指纹仪);
                imageDataList.Add(GetFingerDataS("右手食指"));
                if (_working)
                {
                    PlaySound(SoundMapping.请按操作示范将左手食指平放于指纹仪);
                    BackUri = ResourceEngine.GetImageResourceUri("提示_指纹识别l");
                    imageDataList.Add(GetFingerDataS("左手食指"));
                }

            }
            finally
            {
                Logger.Main.Info($"[识别指纹结束]");
                _fingerPrintDevice.UnInitialize();
                StopRead();
                if (_working && imageDataList != null)
                {
                    Logger.Main.Info($"[指纹正常]");
                    OnGetInfo(imageDataList);
                }
            }
        }

        protected string GetFingerData(string FingerName)
        {
            _working = true;
            string imageData =null;
            TipContent = $"请将{FingerName}按示范操作平放于指纹仪";
            while (_working)
            {
                var rest = _fingerPrintDevice.GetFingerImage();
                if (rest.IsSuccess)
                {
                    try
                    {
                        var image = rest.Value;
                        string imageDataS = "";

                        Logger.Main.Info("保存文件= " + "FingerImage/" + DateTimeCore.Now.ToString("yyyyMMddHHmmss") + ".bmp");
                        image.Save("FingerImage/" + DateTimeCore.Now.ToString("yyyyMMddHHmmss") + ".bmp", ImageFormat.Bmp);
                        Logger.Main.Info("保存文件成功");
                        using (var ms = new MemoryStream())
                        {
                            image.Save(ms, ImageFormat.Jpeg);
                            byte[] bs = ms.ToArray();
                            imageData = Convert.ToBase64String(bs);
                            imageDataS = bs.ByteToString();
                        }
                        Logger.Main.Info($"[指纹识别成功]尺寸={rest.Value.Height}*{rest.Value.Width}={imageDataS.Length}byte");
                        //Logger.Main.Info("指纹信息= " + imageDataS);
                        break;
                    }
                    catch (Exception ex)
                    {
                        Logger.Main.Info(ex.ToString());
                    }
                }
                Thread.Sleep(300);
            }
            return imageData;
        }
        protected string GetFingerDataS(string FingerName)
        {
            Logger.Main.Info($"[准备识别指纹]{FingerName}");
            _working = true;
            string imageData = "";
            TipContent = $"请将{FingerName}按示范操作平放于指纹仪";
            int st = 0;
            while (_working)
            {
                st++;
                var rest = _fingerPrintDevice.GetFingerTemplate();
                if (rest.IsSuccess)
                {
                    try
                    {
                        var tmp = rest.Value;
                        imageData = Convert.ToBase64String(tmp);
                        Logger.Main.Info($"[指纹识别成功]共识别{st}次，尺寸={imageData.Length}byte");
                        //Logger.Main.Info("指纹信息= " + imageData);
                        break;
                    }
                    catch (Exception ex)
                    {
                        Logger.Main.Info(ex.ToString());
                    }
                }
                Thread.Sleep(300);
            }

            return imageData;
        }

        protected virtual void StopRead()
        {
            _fingerPrintDevice?.DisConnect();
        }

        public override bool OnLeaving(NavigationContext navigationContext)
        {
            Logger.Main.Info($"[离开页面]");
            _working = false;

            StopRead();     
            return true;
        }

        protected virtual void OnGetInfo(List<string> imageDataList)
        {
            DoCommand(lp => { Act(lp, imageDataList); });
        }

        protected virtual void Act(LoadingProcesser lp, List<string> imageDataList)
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

    public class FingerPrintViewModel : FingerPrintViewModelBase
    {
        public FingerPrintViewModel(IFingerPrintDevice[] fingerPrintDevices) : base(fingerPrintDevices)
        {
        }

        public Action<LoadingProcesser, List<string>> Action { get; set; }

        protected override void Act(LoadingProcesser lp, List<string> imageDataList)
        {
            Logger.Main.Info($"FingerPrintViewModel->Act");
            Action?.Invoke(lp, imageDataList);
        }
    }
}
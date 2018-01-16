using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Practices.Unity;
using System.Threading.Tasks;
using System.Windows.Input;
using YuanTu.Core.FrameworkBase;
using Prism.Regions;
using YuanTu.Core.Log;
using YuanTu.Consts.Services;
using YuanTu.Devices.FingerPrint;
using YuanTu.Consts.Sounds;
using YuanTu.Core.Reporter;
using System.Threading;
using YuanTu.Consts.Gateway;
using YuanTu.ChongQingArea.Component.Auth.Models;
using YuanTu.Consts.Enums;
using System.Runtime.CompilerServices;
using YuanTu.Core.FrameworkBase.Loadings;
using YuanTu.Consts;
using YuanTu.Core.Models;
using System.Collections.ObjectModel;
using YuanTu.ChongQingArea.Component.Auth.Views;
using System.IO;
using System.Drawing.Imaging;
using YuanTu.Consts.Models.Auth;
using YuanTu.Core.Extension;

namespace YuanTu.ChongQingArea.Component.Auth.ViewModels
{
    public class FingerPrintValidationViewModel : ViewModelBase
    {
        protected IFingerPrintDevice _fingerPrintDevice;

        public FingerPrintValidationViewModel(IFingerPrintDevice[] fingerPrintDevices)
        {
            _fingerPrintDevice = fingerPrintDevices?.FirstOrDefault();
        }

        protected bool _working;

        public override string Title => "指纹识别";

        public ICommand JumpFingerPrint { get; set; }

        public override void OnSet()
        {
            BackUri = ResourceEngine.GetImageResourceUri("提示_指纹识别");
        }


        public override void OnEntered(NavigationContext navigationContext)
        {
            Logger.Main.Info($"[进入指纹验证页面]");
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
                    var rest = _fingerPrintDevice.GetFingerTemplate();
                    if (rest.IsSuccess)
                    {
                        var image = rest.Value;
                        imageData = Convert.ToBase64String(image);
                        Logger.Main.Info("[指纹识别成功]");

                        DoCommand(lp =>
                        {

                            lp.ChangeText("正在识别指纹身份，请稍后......");

                            PatientModel.Req病人信息查询 = new req病人信息查询
                            {
                                cardNo = imageData,
                                cardType = "23",
                            };

                            PatientModel.Res病人信息查询 = DataHandlerEx.病人信息查询(PatientModel.Req病人信息查询);

                            if (!PatientModel.Res病人信息查询.success)
                            {
                                ShowAlert(false, "病人信息查询", "未查询到病人的信息", debugInfo: PatientModel.Res病人信息查询.msg);
                                return;
                            }
                            if (PatientModel.Res病人信息查询.data == null || PatientModel.Res病人信息查询.data.Count == 0)
                            {
                                ShowAlert(false, "病人信息查询", "未查询到病人的信息(列表为空)");
                                return;
                            }

                            Logger.Main.Info("指纹识别结果" + (PatientModel.Res病人信息查询.success ? "成功" : "失败"));

                            var patientInfo = PatientModel.Res病人信息查询.data[PatientModel.PatientInfoIndex];
                            ChangeNavigationContent($"{patientInfo.name}\r\n余额{patientInfo.accBalance.In元()}");

                            Next();

                        });
                    }
                    Thread.Sleep(300);
                }
            }
            finally
            {
                Logger.Main.Info($"[识别指纹结束]");
                _fingerPrintDevice.UnInitialize();
                if (_working && imageData != null)
                {
                    Logger.Main.Info($"[指纹正常]");
                    OnGetInfo(imageData);
                }

            }
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

        public ISiModel SiModel { get; set; }

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

        #region ioc

        [Microsoft.Practices.Unity.Dependency]
        public IPatientModel PatientModel { get; set; }

        #endregion

        public class FingerPrintViewModel : FingerPrintValidationViewModel
        {
            public FingerPrintViewModel(IFingerPrintDevice[] fingerPrintDevices) : base(fingerPrintDevices)
            {
            }

            public Action<LoadingProcesser, string> Action { get; set; }

            protected override void Act(LoadingProcesser lp, string imageData)
            {
                Logger.Main.Info($"FingerPrintViewModel->Act");
                Action?.Invoke(lp, imageData);
            }
        }
    }
}

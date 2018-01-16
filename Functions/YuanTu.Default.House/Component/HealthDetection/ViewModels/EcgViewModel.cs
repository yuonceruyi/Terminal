using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Practices.Unity;
using Prism.Commands;
using Prism.Regions;
using YuanTu.Consts.Services;
using YuanTu.Consts.Sounds;
using YuanTu.Core.FrameworkBase;
using YuanTu.Default.House.Component.Auth.Models;
using YuanTu.Default.House.Device.Ecg;

namespace YuanTu.Default.House.Component.HealthDetection.ViewModels
{
    public class EcgViewModel : ViewModelBase
    {
        private bool _canRecieveData;

        private Uri _标准图Uri;

        private Uri _测量步骤Uri;

        public EcgViewModel()
        {
            StartMeasureCommand = new DelegateCommand(StartMeasure);
            ResetMeasureCommand = new DelegateCommand(ResetMeasure);
            NextCommand = new DelegateCommand(Next);
        }

        public override string Title => "体温测量";

        public Uri 测量步骤Uri
        {
            get => _测量步骤Uri;
            set
            {
                _测量步骤Uri = value;
                OnPropertyChanged();
            }
        }

        public Uri 标准图Uri
        {
            get => _标准图Uri;
            set
            {
                _标准图Uri = value;
                OnPropertyChanged();
            }
        }

        public ICommand StartMeasureCommand { get; set; }
        public ICommand ResetMeasureCommand { get; set; }
        public ICommand NextCommand { get; set; }

        [Dependency]
        public IEcgService EcgService { get; set; }

        [Dependency]
        public IEcgModel EcgModel { get; set; }

        [Dependency]
        public IHealthModel HealthModel { get; set; }

        public override void OnSet()
        {
            标准图Uri = ResourceEngine.GetImageResourceUri("心电参考");
            测量步骤Uri = ResourceEngine.GetImageResourceUri("步骤心电");
        }

        public override void OnEntered(NavigationContext navigationContext)
        {
            base.OnEntered(navigationContext);
            PlaySound(SoundMapping.开始测量);
        }

        public override bool OnLeaving(NavigationContext navigationContext)
        {
            if (!_canRecieveData)
                return base.OnLeaving(navigationContext);
            EcgService.StopMeasure();
            _canRecieveData = false;
            ShowStatus = false;
            StartMeasureEnable = true;
            ResetMeasureEnable = true;
            ShowAlert(false, "温馨提示", "测量失败，请重试！（显示结果为上次测量结果）");
            return false;
        }

        public virtual void RecieveData()
        {
            StartMeasureEnable = false;
            ResetMeasureEnable = false;
            ShowStatus = true;
            _canRecieveData = true;
            Task.Run(() =>
            {
                while (_canRecieveData)
                {
                    if (!EcgService.MeasureFinished)
                    {
                        Thread.Sleep(200);
                        continue;
                    }
                    _canRecieveData = false;
                    ShowStatus = false;

                    if (!EcgService.MeasureSuccess)
                    {
                        PlaySound(SoundMapping.测量失败请重新测量);
                        ShowAlert(false, "温馨提示", "测试失败，请重试！（显示结果为上次测量结果）");
                        Diag = EcgModel.Diag.ToString();
                        PR = EcgModel.PR.ToString();
                        参考结果 = EcgModel.参考结果;
                        Thread.Sleep(3000);
                        StartMeasureEnable = true;
                        ResetMeasureEnable = true;
                    }
                    else
                    {
                        PlaySound(SoundMapping.测量完成点击下一步);
                        ChangeNavigationContent("."); //改变导航栏状态为已测量
                        Diag = EcgModel.Diag.ToString();
                        PR = EcgModel.PR.ToString();
                        参考结果 = EcgModel.参考结果;
                        Thread.Sleep(3000);
                        ShowResetMeasure = true;
                        ShowPrintReporter = true;
                        ShowStartMeasure = false;
                        StartMeasureEnable = true;
                        ResetMeasureEnable = true;
                    }
                }
            });
        }

        public virtual void StartMeasure()
        {
            var result = EcgService.StartMeasure();
            if (!result.IsSuccess)
            {
                ShowAlert(false, "温馨提示", result.Message);
                return;
            }
            RecieveData();
        }

        public virtual void ResetMeasure()
        {
            var result = EcgService.StartMeasure();
            if (!result.IsSuccess)
            {
                ShowAlert(false, "温馨提示", result.Message);
                return;
            }
            RecieveData();
        }

        public override void DoubleClick()
        {
            ChangeNavigationContent("."); //改变导航栏状态为已测量
            ShowResetMeasure = true;
            ShowPrintReporter = true;
            ShowStartMeasure = false;
            EcgModel.Diag = 1;
            EcgModel.PR = 76;
            EcgModel.参考结果 = "正常";
            Diag = EcgModel.Diag.ToString();
            PR = EcgModel.PR.ToString();
            参考结果 = EcgModel.参考结果;
        }

        #region Bindings

        private bool _showStartMeasure = true;
        private bool _showResetMeasure;
        private bool _showPrintReporter;
        private string _statusText = "测量中";
        private bool _showStatus;
        private bool _startMeasureEnable = true;
        private bool _resetMeasureEnable = true;

        public bool ResetMeasureEnable
        {
            get => _resetMeasureEnable;
            set
            {
                _resetMeasureEnable = value;
                OnPropertyChanged();
            }
        }

        public bool StartMeasureEnable
        {
            get => _startMeasureEnable;
            set
            {
                _startMeasureEnable = value;
                OnPropertyChanged();
            }
        }

        public bool ShowStatus
        {
            get => _showStatus;
            set
            {
                _showStatus = value;
                OnPropertyChanged();
            }
        }

        public string StatusText
        {
            get => _statusText;
            set
            {
                _statusText = value;
                OnPropertyChanged();
            }
        }

        public bool ShowStartMeasure
        {
            get => _showStartMeasure;
            set
            {
                _showStartMeasure = value;
                OnPropertyChanged();
            }
        }

        public bool ShowResetMeasure
        {
            get => _showResetMeasure;
            set
            {
                _showResetMeasure = value;
                OnPropertyChanged();
            }
        }

        public bool ShowPrintReporter
        {
            get => _showPrintReporter;
            set
            {
                _showPrintReporter = value;
                OnPropertyChanged();
            }
        }

        private string _diag;
        private string _pr;
        private string _参考结果;

        public string Diag
        {
            get => _diag;
            set
            {
                _diag = value;
                OnPropertyChanged();
            }
        }

        public string PR
        {
            get => _pr;
            set
            {
                _pr = value;
                OnPropertyChanged();
            }
        }

        public string 参考结果
        {
            get => _参考结果;
            set
            {
                _参考结果 = value;
                OnPropertyChanged();
            }
        }

        #endregion binging
    }
}
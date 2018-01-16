using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
using YuanTu.Default.House.Component.HealthDetection.Services;
using YuanTu.Default.House.Device.Fat;
using YuanTu.Default.House.Device.Gate;
using YuanTu.Default.House.Device.HeightWeight;
using YuanTu.Default.House.Device.Temperature;

namespace YuanTu.Default.House.Component.HealthDetection.ViewModels
{
    public class TemperatureViewModel : ViewModelBase
    {
        public TemperatureViewModel()
        {
            StartMeasureCommand = new DelegateCommand(StartMeasure);
            ResetMeasureCommand = new DelegateCommand(ResetMeasure);
            NextCommand = new DelegateCommand(Next);
        }
        public override string Title => "体温测量";

        private Uri _测量步骤Uri;
        public Uri 测量步骤Uri
        {
            get { return _测量步骤Uri; }
            set
            {
                _测量步骤Uri = value;
                OnPropertyChanged();
            }
        }

        private Uri _标准图Uri;
        public Uri 标准图Uri
        {
            get { return _标准图Uri; }
            set
            {
                _标准图Uri = value;
                OnPropertyChanged();
            }
        }

        private bool _canRecieveData;

        #region Bindings

        private bool _showStartMeasure = true;
        private bool _showResetMeasure = false;
        private bool _showPrintReporter = false;
        private string _表面温度;
        private string _人体温度;
        private string _环境温度;
        private string _statusText = "测量中";
        private bool _showStatus;
        private bool _startMeasureEnable = true;
        private bool _resetMeasureEnable = true;

        public bool ResetMeasureEnable
        {
            get { return _resetMeasureEnable; }
            set
            {
                _resetMeasureEnable = value;
                OnPropertyChanged();
            }
        }

        public bool StartMeasureEnable
        {
            get { return _startMeasureEnable; }
            set
            {
                _startMeasureEnable = value;
                OnPropertyChanged();
            }
        }

        public bool ShowStatus
        {
            get { return _showStatus; }
            set
            {
                _showStatus = value;
                OnPropertyChanged();
            }
        }

        public string StatusText
        {
            get { return _statusText; }
            set
            {
                _statusText = value;
                OnPropertyChanged();
            }
        }

        public bool ShowStartMeasure
        {
            get { return _showStartMeasure; }
            set
            {
                _showStartMeasure = value;
                OnPropertyChanged();
            }
        }

        public bool ShowResetMeasure
        {
            get { return _showResetMeasure; }
            set
            {
                _showResetMeasure = value;
                OnPropertyChanged();
            }
        }

        public bool ShowPrintReporter
        {
            get { return _showPrintReporter; }
            set
            {
                _showPrintReporter = value;
                OnPropertyChanged();
            }
        }

        public string 表面温度
        {
            get { return _表面温度; }
            set
            {
                _表面温度 = value;
                OnPropertyChanged();
            }
        }

        public string 人体温度
        {
            get { return _人体温度; }
            set
            {
                _人体温度 = value;
                OnPropertyChanged();
            }
        }

        public string 环境温度
        {
            get { return _环境温度; }
            set
            {
                _环境温度 = value;
                OnPropertyChanged();
            }
        }

        #endregion binging

        public ICommand StartMeasureCommand { get; set; }
        public ICommand ResetMeasureCommand { get; set; }
        public ICommand NextCommand { get; set; }

        public override void OnSet()
        {
            标准图Uri = ResourceEngine.GetImageResourceUri("体温参考");
            测量步骤Uri = ResourceEngine.GetImageResourceUri("步骤体温");
        }

        public override void OnEntered(NavigationContext navigationContext)
        {
            SmartGateService.OnEntered(navigationContext);
        }

        public override bool OnLeaving(NavigationContext navigationContext)
        {
            SmartGateService.OnLeaving(navigationContext);
            if (!_canRecieveData)
                return true;
            TemperatureService.StopMeasure();
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
                    if (!TemperatureService.MeasureFinished)
                    {
                        Thread.Sleep(200);
                        continue;
                    }
                    _canRecieveData = false;
                    ShowStatus = false;

                    if (!TemperatureService.MeasureSuccess)
                    {
                        PlaySound(SoundMapping.测量失败请重新测量);
                        ShowAlert(false, "温馨提示", "测试失败，请重试！（显示结果为上次测量结果）");
                        表面温度 = TemperatureModel.表面温度.ToString();
                        人体温度 = TemperatureModel.人体温度.ToString();
                        环境温度 = TemperatureModel.环境温度.ToString();
                        Thread.Sleep(3000);
                        StartMeasureEnable = true;
                        ResetMeasureEnable = true;
                    }
                    else
                    {
                        PlaySound(SoundMapping.测量完成点击下一步);
                        ChangeNavigationContent(".");//改变导航栏状态为已测量
                        表面温度 = TemperatureModel.表面温度.ToString();
                        人体温度 = TemperatureModel.人体温度.ToString();
                        环境温度 = TemperatureModel.环境温度.ToString();
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
            var result = TemperatureService.StartMeasure();
            if (!result.IsSuccess)
            {
                ShowAlert(false, "温馨提示", result.Message);
                return;
            }
            RecieveData();
        }

        public virtual void ResetMeasure()
        {
            var result = TemperatureService.StartMeasure();
            if (!result.IsSuccess)
            {
                ShowAlert(false, "温馨提示", result.Message);
                return;
            }
            RecieveData();
        }

        [Dependency]
        public ITemperatureService TemperatureService { get; set; }

        [Dependency]
        public ITemperatureModel TemperatureModel { get; set; }

        [Dependency]
        public ISmartGateService SmartGateService { get; set; }

        public override void DoubleClick()
        {
            ChangeNavigationContent(".");//改变导航栏状态为已测量
            ShowResetMeasure = true;
            ShowPrintReporter = true;
            ShowStartMeasure = false;
            TemperatureModel.表面温度 = 35.2m;
            TemperatureModel.环境温度 = 24m;
            TemperatureModel.人体温度 = 36.4m;
            TemperatureModel.参考结果 = "正常";
            表面温度 = TemperatureModel.表面温度.ToString();
            人体温度 = TemperatureModel.人体温度.ToString();
            环境温度 = TemperatureModel.环境温度.ToString();
        }
    }
}

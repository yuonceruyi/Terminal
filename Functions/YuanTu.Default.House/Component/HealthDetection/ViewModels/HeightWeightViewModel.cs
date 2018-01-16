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
using YuanTu.Default.House.Device.HeightWeight;

namespace YuanTu.Default.House.Component.HealthDetection.ViewModels
{
    public class HeightWeightViewModel : ViewModelBase
    {
        public HeightWeightViewModel()
        {
            StartMeasureCommand = new DelegateCommand(StartMeasure);
            ResetMeasureCommand = new DelegateCommand(ResetMeasure);
            NextCommand = new DelegateCommand(Next);
        }

        private bool _canRecieveData;

        #region Bindings

        private bool _showStartMeasure = true;
        private bool _showResetMeasure = false;
        private bool _showPrintReporter = false;
        private string _height;
        private string _weight;
        private string _bmi;
        private string _result;
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

        public string Height
        {
            get { return _height; }
            set
            {
                _height = value;
                OnPropertyChanged();
            }
        }

        public string Weight
        {
            get { return _weight; }
            set
            {
                _weight = value;
                OnPropertyChanged();
            }
        }

        public string Bmi
        {
            get { return _bmi; }
            set
            {
                _bmi = value;
                OnPropertyChanged();
            }
        }

        public string Result
        {
            get { return _result; }
            set
            {
                _result = value;
                OnPropertyChanged();
            }
        }

        public ICommand StartMeasureCommand { get; set; }
        public ICommand ResetMeasureCommand { get; set; }
        public ICommand NextCommand { get; set; }
        public override string Title => "身高体重测量";

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

        #endregion binding

        public override void OnSet()
        {
            标准图Uri = ResourceEngine.GetImageResourceUri("BMI参考");
            测量步骤Uri = ResourceEngine.GetImageResourceUri("步骤身高体重");
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
            HeightWeightService.StopMeasure();
            _canRecieveData = false;
            ShowStatus = false;
            StartMeasureEnable = true;
            ResetMeasureEnable = true;
            ShowAlert(false, "温馨提示", "测量失败，请重试！（显示结果为上次测量结果）");
            return false;
        }

        public virtual void RecieveData()
        {
            PlaySound(SoundMapping.身高体重测量提示);
            StartMeasureEnable = false;
            ResetMeasureEnable = false;
            ShowStatus = true;
            _canRecieveData = true;
            Task.Run(() =>
            {
                while (_canRecieveData)
                {
                    if (!HeightWeightService.MeasureFinished)
                    {
                        Thread.Sleep(200);
                        continue;
                    }
                    _canRecieveData = false;
                    ShowStatus = false;
                    StartMeasureEnable = true;
                    ResetMeasureEnable = true;
                    if (!HeightWeightService.MeasureSuccess)
                    {
                        PlaySound(SoundMapping.测量失败请重新测量);
                        ShowAlert(false, "温馨提示", "测量失败，请重试！（显示结果为上次测量结果）");
                    }
                    else
                    {
                        PlaySound(SoundMapping.测量完成点击下一步);
                        ChangeNavigationContent(".");//改变导航栏状态为已测量
                        ShowResetMeasure = true;
                        ShowPrintReporter = true;
                        ShowStartMeasure = false;
                    }
                    Height = HeightWeightModel.身高.ToString();
                    Weight = HeightWeightModel.体重.ToString();
                    Bmi = HeightWeightModel.体质指数.ToString();
                    Result = HeightWeightModel.参考结果;
                }
            });
        }

        public virtual void StartMeasure()
        {
            var result = HeightWeightService.StartMeasure();
            if (!result.IsSuccess)
            {
                ShowAlert(false, "温馨提示", result.Message);
                return;
            }
            RecieveData();
        }

        public virtual void ResetMeasure()
        {
            var result = HeightWeightService.StartMeasure();
            if (!result.IsSuccess)
            {
                ShowAlert(false, "温馨提示", result.Message);
                return;
            }
            RecieveData();
        }

        [Dependency]
        public IHeightWeightService HeightWeightService { get; set; }

        [Dependency]
        public IHeightWeightModel HeightWeightModel { get; set; }

        public override void DoubleClick()
        {
            ChangeNavigationContent(".");//改变导航栏状态为已测量
            ShowResetMeasure = true;
            ShowPrintReporter = true;
            ShowStartMeasure = false;
            HeightWeightModel.身高 = 188;
            HeightWeightModel.体重 = 80;
            HeightWeightModel.体质指数 = 20;
            HeightWeightModel.参考结果 = "正常";
            Height = HeightWeightModel.身高.ToString();
            Weight = HeightWeightModel.体重.ToString();
            Bmi = HeightWeightModel.体质指数.ToString();
            Result = HeightWeightModel.参考结果;
        }
    }
}
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
using YuanTu.Default.House.Device.BloodPressure;

namespace YuanTu.Default.House.Component.HealthDetection.ViewModels
{
    public class BloodPressureViewModel : ViewModelBase
    {
        public BloodPressureViewModel()
        {
            StartMeasureCommand = new DelegateCommand(StartMeasure);
            ResetMeasureCommand = new DelegateCommand(ResetMeasure);
            NextCommand = new DelegateCommand(Next);
        }

        public override string Title => "血压测量";

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

        #region binging

        private bool _showStartMeasure = true;
        private bool _showResetMeasure = false;
        private bool _showPrintReporter = false;
        private string _收缩压;
        private string _舒张压;
        private string _脉搏;
        private string _参考结果;
        private string _statusText = "检测中";
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

        public string 收缩压
        {
            get { return _收缩压; }
            set
            {
                _收缩压 = value;
                OnPropertyChanged();
            }
        }

        public string 舒张压
        {
            get { return _舒张压; }
            set
            {
                _舒张压 = value;
                OnPropertyChanged();
            }
        }

        public string 脉搏
        {
            get { return _脉搏; }
            set
            {
                _脉搏 = value;
                OnPropertyChanged();
            }
        }

        public string 参考结果
        {
            get { return _参考结果; }
            set
            {
                _参考结果 = value;
                OnPropertyChanged();
            }
        }

        #endregion binging

        public ICommand StartMeasureCommand { get; set; }
        public ICommand ResetMeasureCommand { get; set; }
        public ICommand NextCommand { get; set; }

        public override void OnSet()
        {
            标准图Uri = ResourceEngine.GetImageResourceUri("血压参考");
            测量步骤Uri = ResourceEngine.GetImageResourceUri("步骤血压");
        }

        public override void OnEntered(NavigationContext navigationContext)
        {
            base.OnEntered(navigationContext);
            PlaySound(SoundMapping.血压仪测量提示);
        }

        public override bool OnLeaving(NavigationContext navigationContext)
        {
            if (!_canRecieveData)
                return base.OnLeaving(navigationContext);
            BloodPressureService.StopMeasure();
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
                    if (!BloodPressureService.MeasureFinished)
                    {
                        Thread.Sleep(200);
                        continue;
                    }
                    _canRecieveData = false;
                    ShowStatus = false;
                    StartMeasureEnable = true;
                    ResetMeasureEnable = true;
                    if (!BloodPressureService.MeasureSuccess)
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
                    收缩压 = BloodPressureModel.收缩压.ToString();
                    舒张压 = BloodPressureModel.舒张压.ToString();
                    脉搏 = BloodPressureModel.脉搏.ToString();
                    参考结果 = BloodPressureModel.参考结果;
                }
            });
        }

        public virtual void StartMeasure()
        {
            var result = BloodPressureService.StartMeasure();
            if (!result.IsSuccess)
            {
                ShowAlert(false, "温馨提示", result.Message);
                return;
            }
            RecieveData();
        }

        public virtual void ResetMeasure()
        {
            var result = BloodPressureService.StartMeasure();
            if (!result.IsSuccess)
            {
                ShowAlert(false, "温馨提示", result.Message);
                return;
            }
            RecieveData();
        }

        public virtual void PrintReporter()
        { }

        [Dependency]
        public IBloodPressureService BloodPressureService { get; set; }

        [Dependency]
        public IBloodPressureModel BloodPressureModel { get; set; }

        public override void DoubleClick()
        {
            ChangeNavigationContent(".");//改变导航栏状态为已测量
            ShowResetMeasure = true;
            ShowPrintReporter = true;
            ShowStartMeasure = false;
            BloodPressureModel.舒张压 = 80;
            BloodPressureModel.收缩压 = 120;//高压数据需要+25才是真正的测量数据
            BloodPressureModel.脉搏 = 80;
            BloodPressureModel.参考结果 = "三级高血压";
            收缩压 = BloodPressureModel.收缩压.ToString();
            舒张压 = BloodPressureModel.舒张压.ToString();
            脉搏 = BloodPressureModel.脉搏.ToString();
            参考结果 = BloodPressureModel.参考结果;
        }
    }
}
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
using YuanTu.Default.House.Component.HealthDetection.Services;
using YuanTu.Default.House.Device.Gate;
using YuanTu.Default.House.Device.SpO2;

namespace YuanTu.Default.House.Component.HealthDetection.ViewModels
{
    public class SpO2ViewModel:ViewModelBase
    {
        public SpO2ViewModel()
        {
            StartMeasureCommand = new DelegateCommand(StartMeasure);
            ResetMeasureCommand = new DelegateCommand(ResetMeasure);
            PrintReporterCommand = new DelegateCommand(PrintReporter);
        }

        public override void OnSet()
        {
            标准图Uri = ResourceEngine.GetImageResourceUri("血氧参考");
            测量步骤Uri = ResourceEngine.GetImageResourceUri("步骤血氧");
        }

        public override string Title => "血氧测量";

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
        #region
        private bool _showStartMeasure=true;
        private bool _showResetMeasure=false;
        private bool _showPrintReporter=false;
        private string _血氧饱和度;
        private string _脉搏;
        private string _灌注指数;
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
        public string 血氧饱和度
        {
            get { return _血氧饱和度; }
            set
            {
                _血氧饱和度 = value;
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

        public string 灌注指数
        {
            get { return _灌注指数; }
            set
            {
                _灌注指数 = value;
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
        #endregion
        public ICommand StartMeasureCommand { get; set; }
        public ICommand ResetMeasureCommand { get; set; }
        public ICommand PrintReporterCommand { get; set; }

        public override void OnEntered(NavigationContext navigationContext)
        {
            SmartGateService.OnEntered(navigationContext);
            
            PlaySound(SoundMapping.血氧仪测量提示);
            PlaySound(SoundMapping.请待闸门完全打开);
        }

        public override bool OnLeaving(NavigationContext navigationContext)
        {
            _canRecieveData = false;
            SmartGateService.OnLeaving(navigationContext);

            return true;
        }

        private int _recieceDataCount;
        public virtual void RecieveData()
        {
            _recieceDataCount = 0;
            StartMeasureEnable = false;
            ResetMeasureEnable = false;
            ShowStatus = true;
            _canRecieveData = true;
            Task.Run(() =>
            {
                while (_canRecieveData)
                {
                   
                    if (!SpO2Service.MeasureFinished)
                    {
                        Thread.Sleep(200);
                        _recieceDataCount++;
                        if (_recieceDataCount > 150)
                        {
                            //超时
                            PlaySound(SoundMapping.测量失败请重新测量);
                            SpO2Service.StopMeasure();
                            ShowAlert(false, "温馨提示", "测量失败，请重试！（显示结果为上次测量结果）");
                            _canRecieveData = false;
                            ShowStatus = false;
                            StartMeasureEnable = true;
                            ResetMeasureEnable = true;
                            血氧饱和度 = SpO2Model.SpO2.ToString();
                            脉搏 = SpO2Model.PR.ToString();
                            灌注指数 = SpO2Model.PI.ToString();
                            参考结果 = SpO2Model.参考结果;
                            break;
                        }
                        continue;
                    }
                    PlaySound(SoundMapping.血氧仪测量完成);
                    PlaySound(SoundMapping.测量完成点击下一步);
                    ChangeNavigationContent(".");//改变导航栏状态为已测量
                    _canRecieveData = false;
                    ShowStatus = false;
                    StartMeasureEnable = true;
                    ResetMeasureEnable = true;
                    血氧饱和度 = SpO2Model.SpO2.ToString();
                    脉搏 = SpO2Model.PR.ToString();
                    灌注指数 = SpO2Model.PI.ToString();
                    参考结果 = SpO2Model.参考结果;
                    ShowResetMeasure = true;
                    ShowPrintReporter = true;
                    ShowStartMeasure = false;
                }
            });
        }
        public virtual void StartMeasure()
        {
            var result= SpO2Service.StartMeasure();
            if (!result.IsSuccess)
            {
                ShowAlert(false, "温馨提示", result.Message);
                return;
            }
            RecieveData();
        }
        public virtual void ResetMeasure()
        {
            var result = SpO2Service.StartMeasure();
            if (!result.IsSuccess)
            {
                ShowAlert(false, "温馨提示", result.Message);
                return;
            }
            RecieveData();
        }

        public virtual void PrintReporter()
        {
            Next();
        }

        [Dependency]
        public ISpO2Service SpO2Service { get; set; }

        [Dependency]
        public ISpO2Model SpO2Model { get; set; }

        [Dependency]
        public ISmartGateService SmartGateService { get; set; }

        public override void DoubleClick()
        {
            ChangeNavigationContent(".");//改变导航栏状态为已测量
            SpO2Model.SpO2 = 99;
            SpO2Model.PR = 70;
            SpO2Model.PI = 80;
            SpO2Model.参考结果 = "正常";
            血氧饱和度 = SpO2Model.SpO2.ToString();
            脉搏 = SpO2Model.PR.ToString();
            灌注指数 = SpO2Model.PI.ToString();
            参考结果 = SpO2Model.参考结果;
            ShowResetMeasure = true;
            ShowPrintReporter = true;
            ShowStartMeasure = false;
        }
    }
}

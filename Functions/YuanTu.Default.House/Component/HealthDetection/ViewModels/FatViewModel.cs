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
using YuanTu.Default.House.Device.Fat;
using YuanTu.Default.House.Device.HeightWeight;

namespace YuanTu.Default.House.Component.HealthDetection.ViewModels
{
    public class FatViewModel : ViewModelBase
    {
        public FatViewModel()
        {
            StartMeasureCommand = new DelegateCommand(StartMeasure);
            ResetMeasureCommand = new DelegateCommand(ResetMeasure);
            NextCommand = new DelegateCommand(Next);
        }

        public override string Title => "体脂测量";

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
        private string _脂肪含量;
        private string _基础代谢值;
        private string _体质指数;
        private string _体质参考结果;
        private string _体型参考结果;
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

        public string 脂肪含量
        {
            get { return _脂肪含量; }
            set
            {
                _脂肪含量 = value;
                OnPropertyChanged();
            }
        }

        public string 基础代谢值
        {
            get { return _基础代谢值; }
            set
            {
                _基础代谢值 = value;
                OnPropertyChanged();
            }
        }

        public string 体质指数
        {
            get { return _体质指数; }
            set
            {
                _体质指数 = value;
                OnPropertyChanged();
            }
        }

        public string 体质参考结果
        {
            get { return _体质参考结果; }
            set
            {
                _体质参考结果 = value;
                OnPropertyChanged();
            }
        }

        public string 体型参考结果
        {
            get { return _体型参考结果; }
            set
            {
                _体型参考结果 = value;
                OnPropertyChanged();
            }
        }

        #endregion binging

        public ICommand StartMeasureCommand { get; set; }
        public ICommand ResetMeasureCommand { get; set; }
        public ICommand NextCommand { get; set; }

        public override void OnSet()
        {
            标准图Uri = ResourceEngine.GetImageResourceUri("脂肪参考");
            测量步骤Uri = ResourceEngine.GetImageResourceUri("步骤体脂");
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
            FatService.StopMeasure();
            _canRecieveData = false;
            ShowStatus = false;
            StartMeasureEnable = true;
            ResetMeasureEnable = true;
            ShowAlert(false, "温馨提示", "测量失败，请重试！（显示结果为上次测量结果）");
            return false;
        }

        public virtual void RecieveData()
        {
            PlaySound(SoundMapping.请双手紧握体脂仪金属部分);
            StartMeasureEnable = false;
            ResetMeasureEnable = false;
            ShowStatus = true;
            _canRecieveData = true;
            Task.Run(() =>
            {
                while (_canRecieveData)
                {
                    if (!FatService.MeasureFinished)
                    {
                        Thread.Sleep(200);
                        continue;
                    }
                    _canRecieveData = false;
                    ShowStatus = false;
                   
                    if (!FatService.MeasureSuccess)
                    {
                       
                        PlaySound(SoundMapping.测量失败请重新测量);
                        ShowAlert(false, "温馨提示", "测试失败，请重试！（显示结果为上次测量结果）");
                        脂肪含量 = FatModel.脂肪含量.ToString();
                        基础代谢值 = FatModel.基础代谢值.ToString();
                        体质指数 = FatModel.体质指数.ToString();
                        体质参考结果 = FatModel.体质参考结果;
                        体型参考结果 = FatModel.体型参考结果;
                         Thread.Sleep(3000);
                        StartMeasureEnable = true;
                        ResetMeasureEnable = true;
                    }
                    else
                    {
                        PlaySound(SoundMapping.测量完成点击下一步);
                        ChangeNavigationContent(".");//改变导航栏状态为已测量
                        脂肪含量 = FatModel.脂肪含量.ToString();
                        基础代谢值 = FatModel.基础代谢值.ToString();
                        体质指数 = FatModel.体质指数.ToString();
                        体质参考结果 = FatModel.体质参考结果;
                        体型参考结果 = FatModel.体型参考结果;
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
            var result = FatService.StartMeasure(new Input
            {
                Height = Convert.ToInt32(HeightWeightModel.身高),
                Weight = Convert.ToDouble(HeightWeightModel.体重),
                Age = int.Parse(HealthModel.Res查询是否已建档?.data?.age ?? "0"),
                Sex = HealthModel.Res查询是否已建档?.data?.sex == "男" ? 1 : 0
            });
            if (!result.IsSuccess)
            {
                ShowAlert(false, "温馨提示", result.Message);
                return;
            }
            RecieveData();
        }

        public virtual void ResetMeasure()
        {
            var result = FatService.StartMeasure(new Input
            {
                Height = Convert.ToInt32(HeightWeightModel.身高),
                Weight = Convert.ToDouble(HeightWeightModel.体重),
                Age = int.Parse(HealthModel.Res查询是否已建档?.data?.age ?? "0"),
                Sex = HealthModel.Res查询是否已建档?.data?.sex == "男" ? 1 : 0
            });
            if (!result.IsSuccess)
            {
                ShowAlert(false, "温馨提示", result.Message);
                return;
            }
            RecieveData();
        }

        [Dependency]
        public IFatService FatService { get; set; }

        [Dependency]
        public IFatModel FatModel { get; set; }

        [Dependency]
        public IHeightWeightModel HeightWeightModel { get; set; }

        [Dependency]
        public IHealthModel HealthModel { get; set; }

        public override void DoubleClick()
        {
            ChangeNavigationContent(".");//改变导航栏状态为已测量
            ShowResetMeasure = true;
            ShowPrintReporter = true;
            ShowStartMeasure = false;
            FatModel.脂肪含量 = 18;
            FatModel.体质指数 = 20;
            FatModel.基础代谢值 = 1028;
            FatModel.体质参考结果 = "标准";
            FatModel.体型参考结果 = "隐藏性肥胖";
            脂肪含量 = FatModel.脂肪含量.ToString();
            基础代谢值 = FatModel.基础代谢值.ToString();
            体质指数 = FatModel.体质指数.ToString();
            体质参考结果 = FatModel.体质参考结果;
            体型参考结果 = FatModel.体型参考结果;
        }
    }
}
using Microsoft.Practices.Unity;
using Prism.Commands;
using Prism.Regions;
using System;
using System.Linq;
using System.Threading.Tasks;
using YuanTu.Consts.Enums;
using YuanTu.Consts.FrameworkBase;
using YuanTu.Consts.Models.Auth;
using YuanTu.Consts.Services;
using YuanTu.Consts.Sounds;
using YuanTu.Core.DB;
using YuanTu.Core.Extension;
using YuanTu.Core.FrameworkBase;
using YuanTu.Core.Log;
using YuanTu.Core.Reporter;
using YuanTu.Default.Component.Tools.Models;
using YuanTu.Devices.CashBox;

namespace YuanTu.Default.Component.ZYRecharge.ViewModels
{
    public class JCMViewModel : ViewModelBase
    {
        public override string Title => "现金缴住院押金";
        private readonly ICashInputBox _cashInputBox;
        public DelegateCommand ConfirmCommand { get; private set; }
        //        public Uri CardUri { get; set; } =ResourceEngine.GetImageResourceUri("动画素材_人民币");
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

        [Dependency]
        public IPatientModel PatientModel { get; set; }
        //[Dependency]
        //public ICardModel CardModel { get; set; }
        [Dependency]
        public IExtraPaymentModel ExtraPaymentModel { get; set; }

        //[Dependency]
        //public IChoiceModel ChoiceModel { get; set; }

        public JCMViewModel(ICashInputBox[] cashInputBoxs)
        {
            _cashInputBox = cashInputBoxs.FirstOrDefault(p => p.DeviceId == "JCM");
            ConfirmCommand = new DelegateCommand(Confirm);
        }

        public override void OnSet()
        {
            BackUri = ResourceEngine.GetImageResourceUri("JCM入钞");
        }

        public override void OnEntered(NavigationContext navigationContext)
        {
            ChangeNavigationContent("");
            ExtraPaymentModel.Complete = false;
            Name = PatientModel.住院患者信息.name;
            AccBalance =decimal.Parse(PatientModel.住院患者信息.accBalance).InRMB();
            InPatientNo = PatientModel.住院患者信息.patientHosId;
            Name = ExtraPaymentModel.PatientInfo.Name;
                     
            InputAmount = 0;
            Tips = "请在钱箱灯变蓝后投入50或100元纸币\n最多放入50张钞票";

            Result ret;
            if ((ret = _cashInputBox.Connect()).IsSuccess && (ret = _cashInputBox.Initialize()).IsSuccess)
            {
                Task.Run(() => { _cashInputBox.StartPoll(OnMoneyStacked); });
            }
            else
            {
                ReportService.钱箱离线(ret.Message, ErrorSolution.钱箱离线);
                ShowAlert(false, "钱箱异常", "钱箱暂时不可用", debugInfo: ret.Message);
#if !DEBUG
                Preview();
#endif
            }
            
            
            PlaySound(SoundMapping.现金充值);

            var camera = GetInstance<ICameraService>();
            camera.StartRecording("现金充值");
        }

        public override bool OnLeaving(NavigationContext navigationContext)
        {
            var ret = _cashInputBox.GetDeviceStatus();
            if (ret.IsSuccess)
            {
                if (ret.Value == DeviceStatus.Busy)
                {
                    ShowAlert(false, "正在充值", "纸币识别器正忙\n请稍候", debugInfo: ret.Value.ToString());
                    return false;
                }
            }
            if (InputAmount == 0 || ExtraPaymentModel.Complete)
            {
                _cashInputBox.UnInitialize();
                _cashInputBox.DisConnect();
                var camera = GetInstance<ICameraService>();
                camera.StopRecording();
                return base.OnLeaving(navigationContext);
            }
            ShowAlert(false, "正在充值", "请点击结束投币完成充值！");
            return false;
        }

        protected virtual void OnMoneyStacked(int money)
        {
            StartTimer();
            DBManager.Insert(new CashInputInfo
            {
                TotalSeconds = money * 100
            });
            InputAmount += money;
        }

        protected virtual void OnStatusCallBack(byte status, string msg)
        {
            ShowAlert(false, "钞箱异常", msg);
        }

        protected virtual void Confirm()
        {
            if (InputAmount <= 0)
            {
                ShowAlert(false, "请投币", "请您塞入纸币");
                return;
            }
            var ret = _cashInputBox.GetDeviceStatus();
            if (ret.IsSuccess)
            {
                if (ret.Value == DeviceStatus.Busy)
                {
                    ShowAlert(false, "正在充值", "纸币识别器正忙\n请稍候", debugInfo: ret.Value.ToString());
                    return;
                }
            }
            _cashInputBox.UnInitialize();
            _cashInputBox.DisConnect();
            Logger.Main.Info("投币金额是:" + InputAmount + ",开始充值");
        
            ExtraPaymentModel.TotalMoney = InputAmount * 100;
            ExtraPaymentModel.FinishFunc?.Invoke();
        }

        #region 信息绑定

        private string _hint = "提示信息";
        private string _name;
        private string _inPatientNo;
        private string _accBalance;        
        private decimal _inputAmount;
        private string _tips;

        public string Hint
        {
            get { return _hint; }
            set
            {
                _hint = value;
                OnPropertyChanged();
            }
        }

        public string Name
        {
            get { return _name; }
            set
            {
                _name = value;
                OnPropertyChanged();
            }
        }

        public string InPatientNo
        {
            get { return _inPatientNo; }
            set
            {
                _inPatientNo = value;
                OnPropertyChanged();
            }
        }

        public string AccBalance
        {
            get { return _accBalance; }
            set
            {
                _accBalance = value;
                OnPropertyChanged();
            }
        }


        public decimal InputAmount
        {
            get { return _inputAmount; }
            set
            {
                _inputAmount = value;
                OnPropertyChanged();
            }
        }

        public string Tips
        {
            get { return _tips; }
            set
            {
                _tips = value;
                OnPropertyChanged();
            }
        }

        #endregion 信息绑定

        #region 双击测试

        /// <summary>
        ///     当处于IsLocal状态下时，双击工作区会引发此事件
        /// </summary>
        public override void DoubleClick()
        {
            InputAmount += 100;
        }

        #endregion 双击测试
    }
}
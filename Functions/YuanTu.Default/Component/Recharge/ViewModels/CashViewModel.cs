using Microsoft.Practices.Unity;
using Prism.Commands;
using Prism.Regions;
using System;
using System.Linq;
using System.Threading.Tasks;
using YuanTu.Consts;
using YuanTu.Consts.Enums;
using YuanTu.Consts.FrameworkBase;
using YuanTu.Consts.Services;
using YuanTu.Consts.Sounds;
using YuanTu.Core.Extension;
using YuanTu.Core.FrameworkBase;
using YuanTu.Core.Log;
using YuanTu.Core.Reporter;
using YuanTu.Default.Component.Tools.Models;
using YuanTu.Devices.CashBox;

namespace YuanTu.Default.Component.Recharge.ViewModels
{
    public class CashViewModel : ViewModelBase
    {
        public override string Title => "现金充值";
        public ICashInputBox _cashInputBox;
        public DelegateCommand ConfirmCommand { get; private set; }
        private Uri _cardUri;
        public Uri CardUri
        {
            get { return _cardUri; }
            set
            {
                _cardUri = value;
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

        //[Dependency]
        //public IPatientModel PatientModel { get; set; }
        //[Dependency]
        //public ICardModel CardModel { get; set; }
        [Dependency]
        public IExtraPaymentModel ExtraPaymentModel { get; set; }

        //[Dependency]
        //public IChoiceModel ChoiceModel { get; set; }

        public CashViewModel(ICashInputBox[] cashInputBoxs)
        {
            _cashInputBox = cashInputBoxs.FirstOrDefault(p => p.DeviceId == "CashCode");
            ConfirmCommand = new DelegateCommand(Confirm);
        }

        public override void OnSet()
        {
            BackUri = ResourceEngine.GetImageResourceUri("插卡口");
            CardUri = ResourceEngine.GetImageResourceUri("动画素材_人民币");
        }

        public override void OnEntered(NavigationContext navigationContext)
        {
            ChangeNavigationContent("");
            ExtraPaymentModel.Complete = false;
            //var patientInfo = PatientModel.当前病人信息;
            //Name = patientInfo.name;
            //CardNo = CardModel.CardNo;
            //Remain =decimal.Parse(patientInfo.accBalance).In元();
            Name = ExtraPaymentModel.PatientInfo.Name;
            CardNo = ExtraPaymentModel.PatientInfo.CardNo;
            Remain = ExtraPaymentModel.PatientInfo.Remain.In元();

            Business = $"{ExtraPaymentModel.CurrentBusiness}";
            InputAmount = 0;
            Tips = "请在钱箱灯变绿后投入50或100元纸币";

            Result ret;
            if ((ret = _cashInputBox.Connect()).IsSuccess && (ret = _cashInputBox.Initialize()).IsSuccess)
            {
                Task.Run(() => {
                        _cashInputBox.StartPoll(OnMoneyStacked, OnStatuscallback);
                });
            }
            else
            {
                ReportService.钱箱离线(ret.Message, ErrorSolution.钱箱离线);
                ShowAlert(false, "钱箱异常", "钱箱暂时不可用", debugInfo: ret.Message);
#if !DEBUG
                Preview();
#endif
                return;
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
            InputAmount += money;
        }

        protected virtual void OnStatuscallback(byte z1,string errMsg)
        {
            ShowAlert(false, "钞箱异常", $"钞箱返回码：{z1},详情：{errMsg}");
            Preview();
            return;
        }

        protected virtual void Confirm()
        {
            if (NavigationEngine.State!=A.Third.Cash)
            {
                Logger.Main.Error($"[充值系统异常]当前处于[{NavigationEngine.Current}]不可以进行充值！");
                return;
            }
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

            ExtraPaymentModel.TotalMoney = InputAmount * 100;
            ExtraPaymentModel.FinishFunc?.Invoke();
        }

        #region 信息绑定

        private string _hint = "提示信息";
        private string _name;
        private string _cardNo;
        private string _remain;
        private string _business;
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

        public string CardNo
        {
            get { return _cardNo; }
            set
            {
                _cardNo = value;
                OnPropertyChanged();
            }
        }

        public string Remain
        {
            get { return _remain; }
            set
            {
                _remain = value;
                OnPropertyChanged();
            }
        }

        public string Business
        {
            get { return _business; }
            set
            {
                _business = value;
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
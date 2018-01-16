using System;
using System.Linq;
using System.Windows.Input;
using Microsoft.Practices.Unity;
using Prism.Commands;
using Prism.Regions;
using YuanTu.Consts.Models;
using YuanTu.Consts.Models.Auth;
using YuanTu.Consts.Models.Print;
using YuanTu.Consts.Models.Register;
using YuanTu.Consts.Services;
using YuanTu.Consts.Sounds;
using YuanTu.Core.FrameworkBase;
using YuanTu.Default.Component.Tools.Models;
using YuanTu.Default.Tools;
using YuanTu.Devices.CardReader;

namespace YuanTu.Default.Component.Tools.ViewModels
{
    public class SiPayViewModel : ViewModelBase
    {
        protected readonly IIcCardReader _icCardReader;
        protected readonly IRFCpuCardReader _rfCpuCardReader;

        public SiPayViewModel(IIcCardReader[] icCardReaders, IRFCpuCardReader[] rfCpuCardReaders)
        {
            ConfirmCommand = new DelegateCommand(Confirm);
            _icCardReader = icCardReaders.FirstOrDefault(p => p.DeviceId == "HuaDa_IC");
            _rfCpuCardReader = rfCpuCardReaders?.FirstOrDefault(p => p.DeviceId == "ACT_A6_RFIC");
        }

        public override string Title => "医保支付";
        public string Hint { get; set; } = "请按提示插卡";
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

        public ICommand ConfirmCommand { get; set; }

        [Dependency]
        public ICardModel CardModel { get; set; }

        [Dependency]
        public IPatientModel PatientModel { get; set; }

        [Dependency]
        public IIdCardModel IdCardModel { get; set; }

        [Dependency]
        public IChoiceModel ChoiceModel { get; set; }

        [Dependency]
        public IScheduleModel ScheduleModel { get; set; }

        [Dependency]
        public IExtraPaymentModel ExtraPaymentModel { get; set; }

        [Dependency]
        public IPrintManager PrintManager { get; set; }

        [Dependency]
        public IPrintModel PrintModel { get; set; }

        [Dependency]
        public IConfigurationManager ConfigurationManager { get; set; }

        public override void OnSet()
        {
            BackUri = ResourceEngine.GetImageResourceUri("插卡口");
            CardUri = ResourceEngine.GetImageResourceUri("动画素材_社保卡");
        }

        public override void OnEntered(NavigationContext navigationContext)
        {
            base.OnEntered(navigationContext);

            PlaySound(SoundMapping.请插入医保卡);
        }

        public virtual void Confirm()
        {
        }

        public override void DoubleClick()
        {
            var ret = InputTextView.ShowDialogView("输入测试卡号");
        }
    }
}
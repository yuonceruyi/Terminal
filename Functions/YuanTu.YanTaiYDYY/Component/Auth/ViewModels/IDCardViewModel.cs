using Prism.Commands;
using Prism.Regions;
using System;
using System.Linq;
using System.Windows.Input;
using YuanTu.Consts;
using YuanTu.Consts.Enums;
using YuanTu.Consts.Services;
using YuanTu.Consts.Sounds;
using YuanTu.Devices.CardReader;

namespace YuanTu.YanTaiYDYY.Component.Auth.ViewModels
{
    public class IDCardViewModel : Default.Component.Auth.ViewModels.IDCardViewModel
    {
        public IDCardViewModel(IIdCardReader[] idCardReaders) : base(idCardReaders)
        {
            _idCardReader = idCardReaders.FirstOrDefault(p => p.DeviceId == "HuaDa_900");
            InputIDCommand = new DelegateCommand(InputIDConfirm);
        }

        public ICommand InputIDCommand { get; set; }
        private Uri _imgTxtBtnIconUri;
        public Uri ImgTxtBtnIconUri
        {
            get { return _imgTxtBtnIconUri; }
            set
            {
                _imgTxtBtnIconUri = value;
                OnPropertyChanged();
            }
        }

        public override void OnSet()
        {
            base.OnSet();
            ImgTxtBtnIconUri = ResourceEngine.GetImageResourceUri("键盘输入");
        }

        public override void OnEntered(NavigationContext navigationContext)
        {
            var config = GetInstance<IConfigurationManager>();
            InputID = (config.GetValue("InputIdNo") ?? "1") == "1";

            TimeOut = 60;

            PlaySound(InputID
                ? SoundMapping.请刷身份证未携带者可手工输入
                : SoundMapping.请刷身份证);
            StartRead();
            CardModel.CardType = CardType.身份证;

            Hint = CreateModel.CreateType == CreateType.儿童 ? "请输入监护人身份证号码" : "请输入身份证号码";
        }

        public virtual void InputIDConfirm()
        {
            Navigate(A.CK.InputIDCard);
        }

        #region Binding

        private bool _inputID;

        public bool InputID
        {
            get { return _inputID; }
            set
            {
                _inputID = value;
                OnPropertyChanged();
            }
        }

        #endregion Binding

    }
}
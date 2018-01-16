using Prism.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using YuanTu.Consts;
using YuanTu.Consts.Enums;
using YuanTu.Core.Log;
using YuanTu.Devices.CardReader;
using Prism.Regions;
using YuanTu.Consts.Services;
using YuanTu.Consts.Sounds;

namespace YuanTu.BJJingDuETYY.Component.Auth.ViewModels
{
    public class IDCardViewModel : YuanTu.Default.Component.Auth.ViewModels.IDCardViewModel
    {
        public IDCardViewModel(IIdCardReader[] idCardReaders) : base(idCardReaders)
        {
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


            PlaySound(SoundMapping.请刷身份证未携带者可手工输入);
            StartRead();
            CardModel.CardType = CardType.身份证;

            Hint = "请输入监护人身份证号码" ;
        }
        public virtual void InputIDConfirm()
        {
            Navigate(A.CK.InputIDCard);
        }
        protected override void OnGetInfo(string idCardNo)
        {
            string b = idCardNo.Substring(6, 4) + "-" + idCardNo.Substring(10, 2) + "-" + idCardNo.Substring(12, 2);
            DateTime d = new DateTime();

            if (DateTime.TryParse(b, out d) && DateTime.Now < d.AddYears(18))
            {
                ShowAlert(false, "未成年", "您未年满18岁，不允许作为监护人办卡");
                return;
            }
            else
            {
                Navigate(A.CK.InfoEx);
            }
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

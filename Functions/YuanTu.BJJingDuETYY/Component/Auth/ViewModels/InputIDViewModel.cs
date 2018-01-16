using Microsoft.Practices.Unity;
using Prism.Commands;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using YuanTu.Consts;
using YuanTu.Consts.Enums;
using YuanTu.Consts.Models.Auth;
using YuanTu.Consts.Models.Create;
using YuanTu.Consts.Services;
using YuanTu.Core.FrameworkBase;
using YuanTu.BJArea.Card;
using YuanTu.Core.Extension;

namespace YuanTu.BJJingDuETYY.Component.Auth.ViewModels
{
    public class InputIDViewModel : ViewModelBase
    {

        private string iDNo;

        public InputIDViewModel()
        {
            ReadIDCommand = new DelegateCommand(ReadIDConfirm);
            ConfirmCommand = new DelegateCommand(Confirm);
        }

        public override string Title => "输入身份证";

        //public string Hint { get; set; } = "请输入身份证号码";
        public ICommand ReadIDCommand { get; set; }

        public ICommand ConfirmCommand { get; set; }

        #region Bindings

        private string _hint = "请输入身份证号码";
        private string _iDNoTip;
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

        public string IDNo
        {
            get { return iDNo; }
            set
            {
                iDNo = value;
                OnPropertyChanged();
            }
        }

        public string IDNoTip
        {
            get { return _iDNoTip; }
            set
            {
                _iDNoTip = value;
                OnPropertyChanged();
            }
        }

        public string Hint
        {
            get { return _hint; }
            set
            {
                _hint = value;
                OnPropertyChanged();
            }
        }

        #endregion

        [Dependency]
        public ICardModel CardModel { get; set; }

        [Dependency]
        public IIdCardModel IdCardModel { get; set; }

        [Dependency]
        public ICreateModel CreateModel { get; set; }

        public override void OnSet()
        {
            ImgTxtBtnIconUri = ResourceEngine.GetImageResourceUri("卡_身份证");
        }

        public override void OnEntered(NavigationContext navigationContext)
        {
            base.OnEntered(navigationContext);
            IDNo = null;
            CardModel.CardType = CardType.NoCard;

            Hint = "请输入监护人身份证号码";
            IDNoTip = "监护人身份证号：";
        }

        public virtual void ReadIDConfirm()
        {
            Preview();
        }

        public virtual void Confirm()
        {
            if (string.IsNullOrEmpty(iDNo) || iDNo.Length != 18 || iDNo.Take(17).Any(c => c == 'X') ||
                !iDNo.IDCheck())
            {
                ShowAlert(false, "输入身份证", "身份证号不准确，请重新检查!");
                return;
            }
            CardModel.CardNo = IDNo;
            IdCardModel.IdCardNo = IDNo;
            IdCardModel.Sex = Convert.ToInt32(IDNo[16]) % 2 == 0 ? Sex.女 : Sex.男;
            IdCardModel.Birthday = DateTime.Parse(string.Format("{0}-{1}-{2}",
                IDNo.Substring(6, 4), IDNo.Substring(10, 2), IDNo.Substring(12, 2)));
            IdCardModel.Address = "";
            IdCardModel.Name = "";
            IdCardModel.Nation = "";
            if (DateTime.Now < IdCardModel.Birthday.AddYears(18))
            {
                ShowAlert(false, "未成年", "您未年满18岁，不允许作为监护人办卡");
                return;
            }
            else
            {
                Navigate(A.CK.InfoEx);
            }
        }
    }
}
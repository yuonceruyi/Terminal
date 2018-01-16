using System;
using System.Linq;
using System.Windows.Input;
using Microsoft.Practices.Unity;
using Prism.Commands;
using Prism.Regions;
using YuanTu.Consts;
using YuanTu.Consts.Models.Auth;
using YuanTu.Consts.Services;
using YuanTu.Core.Extension;
using YuanTu.Core.FrameworkBase;

namespace YuanTu.NanYangFirstPeopleHospital.Component.Auth.ViewModels
{
    public class InputIdNoViewModel : ViewModelBase
    {
        private string iDNo;

        public InputIdNoViewModel()
        {
            ConfirmCommand = new DelegateCommand(Confirm);
        }

        public override string Title => "输入身份证";
        public string Hint { get; set; } = "请输入身份证号码";

        public ICommand ConfirmCommand { get; set; }
        public Uri ImgTxtBtnIconUri { get; set; }

        public string IDNo
        {
            get { return iDNo; }
            set
            {
                iDNo = value;
                OnPropertyChanged();
            }
        }

        [Dependency]
        public IIdCardModel IdCardModel { get; set; }

        public override void OnSet()
        {
            ImgTxtBtnIconUri = ResourceEngine.GetImageResourceUri("卡_身份证");
        }

        public override void OnEntered(NavigationContext navigationContext)
        {
            base.OnEntered(navigationContext);
            IDNo = null;
        }

        public virtual void Confirm()
        {
            if (string.IsNullOrEmpty(iDNo) || iDNo.Length != 18 || iDNo.Take(17).Any(c => c == 'X') ||
                !iDNo.IDCheck())
            {
                ShowAlert(false, "输入身份证", "身份证号不准确，请重新检查!");
                return;
            }
            IdCardModel.IdCardNo = IDNo;
            ChangeNavigationContent($"身份证\r\n{IDNo}");
            Next();
        }
    }
}
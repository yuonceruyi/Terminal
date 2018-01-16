using Prism.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using YuanTu.Core.FrameworkBase;
using Prism.Regions;
using Microsoft.Practices.Unity;
using YuanTu.Consts.Models.Auth;
using YuanTu.Core.Extension;

namespace YuanTu.Default.Component.Auth.ViewModels
{
    public class InPatientInfoViewModel : ViewModelBase
    {
        public InPatientInfoViewModel()
        {
            ConfirmCommand = new DelegateCommand(Confirm);
        }
        public override string Title => "个人信息";
        public string Hint { get; set; } = "住院患者信息";
        public ICommand ConfirmCommand { get; set; }

        [Dependency]
        public IPatientModel PatientModel { get; set; }

        public override void OnEntered(NavigationContext navigationContext)
        {
            base.OnEntered(navigationContext);
            Name = PatientModel.住院患者信息.name;
            Sex = PatientModel.住院患者信息.sex;
            Birth = PatientModel.住院患者信息.birthday;
            IdNo = PatientModel.住院患者信息.idNo.Mask(14, 3);
            AccBalance = PatientModel.住院患者信息.accBalance.In元();
        }
        public virtual void Confirm()
        {
            ChangeNavigationContent($"{Name}\r\n余额{AccBalance}");
            Next();
        }
        #region Binding
        private string name;
        private string sex;
        private string birth;
        private string idNo;
        private string accBalance;
        
        public string Name
        {
            get { return name; }
            set
            {
                name = value;
                OnPropertyChanged();
            }
        }
        public string Sex
        {
            get { return sex; }
            set
            {
                sex = value;
                OnPropertyChanged();
            }
        }
        public string Birth
        {
            get { return birth; }
            set
            {
                birth = value;
                OnPropertyChanged();
            }
        }
        public string IdNo
        {
            get { return idNo; }
            set
            {
                idNo = value;
                OnPropertyChanged();
            }
        }
        public string AccBalance
        {
            get { return accBalance; }
            set
            {
                accBalance = value;
                OnPropertyChanged();
            }
        }
        
        #endregion Binding
    }
}

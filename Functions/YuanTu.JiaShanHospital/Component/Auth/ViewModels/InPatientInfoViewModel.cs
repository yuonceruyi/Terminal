using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prism.Regions;
using YuanTu.Core.Extension;

namespace YuanTu.JiaShanHospital.Component.Auth.ViewModels
{
    public class InPatientInfoViewModel:YuanTu.Default.Component.Auth.ViewModels.InPatientInfoViewModel
    {
        private string _phone;
        public string Phone
        {
            get { return _phone; }
            set
            {
                _phone = value;
                OnPropertyChanged();
            }
        }
        public override void OnEntered(NavigationContext navigationContext)
        {
            base.OnEntered(navigationContext);
            Name = PatientModel.住院患者信息.name;
            Sex = PatientModel.住院患者信息.sex;
            Birth = PatientModel.住院患者信息.birthday;
            IdNo = PatientModel.住院患者信息.idNo.Mask(14, 3);
            AccBalance = PatientModel.住院患者信息.accBalance.In元();
            Phone = PatientModel.住院患者信息.phone;
        }

        public override void Confirm()
        {
            var extendArr = PatientModel.住院患者信息.extend.Split(',');
            ChangeNavigationContent($"{Name}\r\n总存额{extendArr[1].In元()}\n总消费{extendArr[2].In元()}");
            Next();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prism.Regions;
using YuanTu.Core.Extension;

namespace YuanTu.NingXiaHospital.Component.Auth.ViewModels
{
    public class InPatientInfoViewModel:YuanTu.Default.Component.Auth.ViewModels.InPatientInfoViewModel
    {
        public override void OnEntered(NavigationContext navigationContext)
        {
            base.OnEntered(navigationContext);
            Name = PatientModel.住院患者信息.name;
            Sex = PatientModel.住院患者信息.sex;
            Birth = PatientModel.住院患者信息.birthday;
            IdNo = PatientModel.住院患者信息.idNo.Mask(14, 3);
            AccBalance = PatientModel.住院患者信息.accBalance.In元();
            CreateDate = PatientModel.住院患者信息.createDate;
        }

        private string _createDate;

        public string CreateDate
        {
            get { return _createDate; }
            set
            {
                _createDate = value;
                OnPropertyChanged();
            }
        }
    }
}

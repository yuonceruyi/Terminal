using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Practices.Unity;
using Prism.Regions;
using YuanTu.Consts.Models.Register;
using YuanTu.Core.FrameworkBase;

namespace YuanTu.ShengZhouZhongYiHospital.Component.InfoQuery.ViewModels
{
    public class DoctorInfoViewModel : ViewModelBase
    {

        [Dependency]
        public IDoctorModel DoctorModel { get; set; }

        public override string Title => "医生信息";

        private string _introduceContent;
        public string IntroduceContent
        {
            get => _introduceContent;
            set
            {
                _introduceContent = value;
                OnPropertyChanged();
            }
        }

        private string _introduceTitle;
        public string IntroduceTitle
        {
            get => _introduceTitle;
            set
            {
                _introduceTitle = value;
                OnPropertyChanged();
            }
        }

        public override void OnEntered(NavigationContext navigationContext)
        {
            base.OnEntered(navigationContext);
            IntroduceTitle = DoctorModel.所选医生.doctName + "简介";
            IntroduceContent ="    "+DoctorModel.所选医生?.doctIntro;
        }
    }
}

using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.Practices.Unity;
using Prism.Commands;
using Prism.Regions;
using YuanTu.Consts;
using YuanTu.Consts.Enums;
using YuanTu.Consts.Gateway;
using YuanTu.Consts.Models;
using YuanTu.Consts.Models.Register;
using YuanTu.Consts.Services;
using YuanTu.Consts.Sounds;
using YuanTu.Core.Extension;
using YuanTu.Core.FrameworkBase;
using YuanTu.ShenZhenArea.Models.DepartModel;

namespace YuanTu.ShenZhen.BaoAnCenterHospital.Component.Register.ViewModels
{
    public class DeptTypeViewModel : ViewModelBase
    {
        [Dependency]
        public IDeptartmentModel DeptartmentModel { get; set; }

        [Dependency]
        public IRegTypesModel RegTypesModel { get; set; }

        [Dependency]
        public IDoctorModel DoctorModel { get; set; }

        [Dependency]
        public IScheduleModel ScheduleModel { get; set; }

        [Dependency]
        public IChoiceModel ChoiceModel { get; set; }

        [Dependency]
        public IRegDateModel RegDateModel { get; set; }

        [Dependency]
        public ITriageModel TriageModel { get; set; }


        

        public override string Title => "请触摸下方卡片选择科室";

        public override void OnEntered(NavigationContext navigationContext)
        {
            ChangeNavigationContent("");
            var confirmCommand = new DelegateCommand<Info>(Confirm);

            var listFirst = ConfigBaoAnCenterHospital.ListDepartments.Select(p => new Info
            {
                Title = p.DepartName,
                Tag = p,
                ConfirmCommand = confirmCommand
            });
            Data = new ObservableCollection<Info>(listFirst);
            PlaySound(SoundMapping.选择挂号科室);
        }

        protected virtual void Confirm(Info i)
        {
            var NowDepartment = i.Tag.As<BaoAnCenterHospitalDepartment>();
            RegTypesModel.SelectRegTypeName = NowDepartment.DepartName;
            Navigate(A.XC.Dept);
            return;
        }

        #region Binding

        private ObservableCollection<Info> _data;

        public ObservableCollection<Info> Data
        {
            get { return _data; }
            set
            {
                _data = value;
                OnPropertyChanged();
            }
        }

        #endregion Binding
    }
}

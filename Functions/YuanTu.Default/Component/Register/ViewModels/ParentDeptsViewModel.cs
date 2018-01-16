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

namespace YuanTu.Default.Component.Register.ViewModels
{
    public class ParentDeptsViewModel : ViewModelBase
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

        public override string Title => "请触摸下方卡片选择挂号科室";

        public override void OnEntered(NavigationContext navigationContext)
        {
            ChangeNavigationContent("");
            var confirmCommand = new DelegateCommand<Info>(Confirm);

            var list2 = DeptartmentModel.Res排班科室信息查询.data.Distinct(new Models.ParentDeptEqualityComparer()).Select(p => new Info
            {
                Title = p.parentDeptName,//一级科室
                Tag = p,
                ConfirmCommand = confirmCommand
            });
            Data = new ObservableCollection<Info>(list2);
            
            
            PlaySound(ChoiceModel.Business == Business.挂号 ? SoundMapping.选择挂号科室 : SoundMapping.选择预约科室);
        }

        protected virtual void Confirm(Info i)
        {
            DeptartmentModel.所选父科室 = i.Tag.As<排班科室信息>();
            ChangeNavigationContent(i.Title);
            Navigate(ChoiceModel.Business == Business.挂号 ? A.XC.Dept : A.YY.Dept);
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
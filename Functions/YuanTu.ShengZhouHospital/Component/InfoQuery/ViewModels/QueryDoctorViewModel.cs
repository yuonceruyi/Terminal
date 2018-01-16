using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Practices.Unity;
using Prism.Commands;
using Prism.Regions;
using YuanTu.Consts.Enums;
using YuanTu.Consts.Gateway;
using YuanTu.Consts.Models;
using YuanTu.Consts.Models.Register;
using YuanTu.Consts.Services;
using YuanTu.Consts.Sounds;
using YuanTu.Core.Extension;
using YuanTu.Core.FrameworkBase;

namespace YuanTu.ShengZhouHospital.Component.InfoQuery.ViewModels
{
    public class QueryDoctorViewModel : ViewModelBase
    {
        public override string Title => "专家选择";

        [Dependency]
        public IDoctorModel DoctorModel { get; set; }

        [Dependency]
        public IScheduleModel ScheduleModel { get; set; }

        [Dependency]
        public IRegTypesModel RegTypesModel { get; set; }

        [Dependency]
        public IDeptartmentModel DeptartmentModel { get; set; }

        [Dependency]
        public IChoiceModel ChoiceModel { get; set; }

        [Dependency]
        public IRegDateModel RegDateModel { get; set; }


        public override void OnEntered(NavigationContext navigationContext)
        {
            ChangeNavigationContent("");
            var confirmCommand = new DelegateCommand<Info>(Confirm);
            var list = DoctorModel.Res医生信息查询.data.Select(p => new Info
            {
                Title = p.doctName,
                Tag = p,
                ConfirmCommand = confirmCommand,
            });
            Data = new ObservableCollection<Info>(list);
        }

        protected virtual void Confirm(Info i)
        {
            DoctorModel.所选医生 = i.Tag.As<医生信息>();
            Next();
        }

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
    }
}

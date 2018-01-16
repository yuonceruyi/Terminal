using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Practices.Unity;
using Prism.Regions;
using YuanTu.Consts.Models.InfoQuery;
using YuanTu.Consts.UserControls;
using YuanTu.Core.FrameworkBase;
using YuanTu.QDQLYY.Current.Models;
using YuanTu.Consts.Gateway;
using System.Windows.Input;
using Prism.Commands;
using YuanTu.Consts;
using System.Collections.ObjectModel;
using YuanTu.Consts.Models;
using YuanTu.Core.Extension;

namespace YuanTu.QDQLYY.Component.InfoQuery.ViewModels
{
    public  class DeptsInfoListViewModel : ViewModelBase
    {
        public override string Title => "科室信息列表";
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


        [Dependency]
        public IDeptDocModel DeptDocModel { get; set; }

        public override void OnEntered(NavigationContext navigationContext)
        {
            DeptDocModel.Req科室信息查询 = new req科室信息查询
            {
                deptType = "0",
            };
            var confirmCommand = new DelegateCommand<Info>(DoJump);
            DeptDocModel.Res科室信息查询 = DataHandlerEx.科室信息查询(DeptDocModel.Req科室信息查询);
            if (DeptDocModel.Res科室信息查询.success && DeptDocModel.Res科室信息查询.data.Count > 0)
            {
                 var list = DeptDocModel.Res科室信息查询.data.Select(p => new Info
                {
                    Title = p.deptName,
                    Tag = p,
                    ConfirmCommand = confirmCommand
                });
                Data = new ObservableCollection<Info>(list);
            }
            else
            {
                ShowAlert(false, "查询失败", "未查询到科室信息");
                Navigate(A.Home);
            }
            HideNavigating = true;
        }
        protected void DoJump(Info i)
        {
            DeptDocModel.所选科室信息 = i.Tag.As<科室信息>();
            DeptDocModel.Req医生信息查询 = new req医生信息查询
            {
                deptCode = DeptDocModel.所选科室信息.deptCode,
            };
            DeptDocModel.Res医生信息查询 = DataHandlerEx.医生信息查询(DeptDocModel.Req医生信息查询);
            Next();
        }
    }
}

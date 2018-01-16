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
    public  class DocsInfoListViewModel : ViewModelBase
    {
        public override string Title => "专家信息列表";
        private string _hint= "专家信息列表";

        public string Hint
        {
            get { return _hint; }
            set
            {
                _hint = value;
                OnPropertyChanged();
            }
        }
        private ObservableCollection<InfoMore> _data;

        public ObservableCollection<InfoMore> Data
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
            if (DeptDocModel.Res医生信息查询 == null)
            {
                DeptDocModel.Req医生信息查询 = new req医生信息查询();
                DeptDocModel.Res医生信息查询 = DataHandlerEx.医生信息查询(DeptDocModel.Req医生信息查询);
            }
            var confirmCommand = new DelegateCommand<Info>(DoJump);
            if (DeptDocModel.Res医生信息查询.success && DeptDocModel.Res医生信息查询.data.Count > 0)
            {
                var list = DeptDocModel.Res医生信息查询.data.Select(p => new InfoMore
                {
                    Title = p.doctName,
                    SubTitle=p.doctProfe,
                    Tag = p,
                    ConfirmCommand = confirmCommand
                });
                Data = new ObservableCollection<InfoMore>(list);
                Hint = DeptDocModel.所选科室信息.deptName + "专家信息列表";
            }
            else
            {
                ShowAlert(false, "查询失败", "未查询到专家信息");
                Navigate(A.Home);
            }
            HideNavigating = true;
        }
        private void DoJump(Info i)
        {
            DeptDocModel.所选医生信息 = i.Tag.As< 医生信息>();
            Next();
        }
    }
}

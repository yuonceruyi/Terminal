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

namespace YuanTu.QDQLYY.Component.InfoQuery.ViewModels
{
    public class DocsInfoViewModel : ViewModelBase
    {
        #region Binding
        public override string Title => "专家信息";
        private string _hint = "专家信息";

        public string Hint
        {
            get { return _hint; }
            set
            {
                _hint = value;
                OnPropertyChanged();
            }
        }

        private string _docName = "姓名";
        public string DocName
        {
            get { return _docName; }
            set
            {
                _docName = value;
                OnPropertyChanged();
            }
        }
        private string _doctProfe = "职称";

        public string DoctProfe
        {
            get { return _doctProfe; }
            set
            {
                _doctProfe = value;
                OnPropertyChanged();
            }
        }
        private string _sex = "性别";

        public string Sex
        {
            get { return _sex; }
            set
            {
                _sex = value;
                OnPropertyChanged();
            }
        }
        private string _deptName = "科室";

        public string DeptName
        {
            get { return _deptName; }
            set
            {
                _deptName = value;
                OnPropertyChanged();
            }
        }
        private string _docLevel = "级别";

        public string DocLevel
        {
            get { return _docLevel; }
            set
            {
                _docLevel = value;
                OnPropertyChanged();
            }
        }
        private string _doctIntro = "专家介绍";

        public string DoctIntro
        {
            get { return _doctIntro; }
            set
            {
                _doctIntro = value;
                OnPropertyChanged();
            }
        }
        #endregion
        [Dependency]
        public IDeptDocModel DeptDocModel { get; set; }

        public override void OnEntered(NavigationContext navigationContext)
        {
            DocName = DeptDocModel.所选医生信息.doctName;
            DeptName = DeptDocModel.所选医生信息.deptName;
            DoctProfe = DeptDocModel.所选医生信息.doctProfe;
            Sex = DeptDocModel.所选医生信息.sex;
            DoctIntro = DeptDocModel.所选医生信息.doctIntro;
            DocLevel = DeptDocModel.所选医生信息.doctLevel;
            
        }
    }
}

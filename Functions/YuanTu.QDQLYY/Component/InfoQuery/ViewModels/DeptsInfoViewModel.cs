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
    public class DeptsInfoViewModel : ViewModelBase
    {
        #region Binding
        public override string Title => "科室信息";
        private string _hint = "科室信息";

        public string Hint
        {
            get { return _hint; }
            set
            {
                _hint = value;
                OnPropertyChanged();
            }
        }

        private string _deptName = "科室名称";
        public string DeptName
        {
            get { return _deptName; }
            set
            {
                _deptName = value;
                OnPropertyChanged();
            }
        }
        private string _deptType = "科室类别";

        public string DeptType
        {
            get { return _deptType; }
            set
            {
                _deptType = value;
                OnPropertyChanged();
            }
        }
        private string _address = "地址";

        public string Address
        {
            get { return _address; }
            set
            {
                _address = value;
                OnPropertyChanged();
            }
        }
        private string _phone = "电话";

        public string Phone
        {
            get { return _phone; }
            set
            {
                _phone = value;
                OnPropertyChanged();
            }
        }
        private string _deptIntro = "科室介绍";

        public string DeptIntro
        {
            get { return _deptIntro; }
            set
            {
                _deptIntro = value;
                OnPropertyChanged();
            }
        }
        private bool _hasDoc = true;

        public bool HasDoc
        {
            get { return _hasDoc; }
            set
            {
                _hasDoc = value;
                OnPropertyChanged();
            }
        }
        #endregion
        [Dependency]
        public IDeptDocModel DeptDocModel { get; set; }

        public override void OnEntered(NavigationContext navigationContext)
        {
            DeptName = DeptDocModel.所选科室信息.deptName;
            DeptType = DeptDocModel.所选科室信息.deptType;
            Address = DeptDocModel.所选科室信息.address;
            Phone = DeptDocModel.所选科室信息.phone;
            DeptIntro = DeptDocModel.所选科室信息.deptIntro;

            HasDoc = DeptDocModel.Res医生信息查询.success && DeptDocModel.Res医生信息查询.data.Count > 0;
        }
        public ICommand JumpCommand
        {
            get
            {
                return new DelegateCommand<string>(DoJump);
            }
        }
        private void DoJump(string DeptCode)
        {
            if (DeptDocModel.Res医生信息查询.success && DeptDocModel.Res医生信息查询.data.Count > 0)
                Next();
            else
            {
                ShowAlert(false, "查询失败", "此科室没有医生信息");
            }
        }
    }
}

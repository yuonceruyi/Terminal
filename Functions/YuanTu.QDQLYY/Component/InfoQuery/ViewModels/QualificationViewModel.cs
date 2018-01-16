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

namespace YuanTu.QDQLYY.Component.InfoQuery.ViewModels
{
    public  class QualificationViewModel : ViewModelBase
    {
        public override string Title => "执业资格列表";
        private string _hint="执业资格信息";

        public string Hint
        {
            get { return _hint; }
            set
            {
                _hint = value;
                OnPropertyChanged();
            }
        }
        private ListDataGrid.PageDataEx _qualificationData;
        public ListDataGrid.PageDataEx QualificationData
        {
            get { return _qualificationData; }
            set
            {
                _qualificationData = value;
                OnPropertyChanged();
            }
        }

        [Dependency]
        public IQualificationModel QualificationModel { get; set; }

        public override void OnEntered(NavigationContext navigationContext)
        {
            QualificationData = new ListDataGrid.PageDataEx
            {
                CatalogContent = "",
                List = QualificationModel.当前执业资格信息,
                Tag = ""
            };
        }
    }
}

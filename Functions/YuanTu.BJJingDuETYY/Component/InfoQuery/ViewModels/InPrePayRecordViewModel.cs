using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.Practices.Unity;
using Prism.Regions;
using YuanTu.Consts.Services;
using YuanTu.Consts.Sounds;
using YuanTu.Consts.UserControls;
using YuanTu.Core.Extension;
using YuanTu.Core.FrameworkBase;
using YuanTu.Consts.Models.InfoQuery;

namespace YuanTu.BJJingDuETYY.Component.InfoQuery.ViewModels
{
    public class InPrePayRecordViewModel : ViewModelBase
    {
        public override string Title => "住院押金记录列表";
        private string _hint = "住院押金信息";

        public string Hint
        {
            get { return _hint; }
            set
            {
                _hint = value;
                OnPropertyChanged();
            }
        }
        private ListDataGrid.PageDataEx _inPrePayItemsData;
        public ListDataGrid.PageDataEx InPrePayItemsData
        {
            get { return _inPrePayItemsData; }
            set
            {
                _inPrePayItemsData = value;
                OnPropertyChanged();
            }
        }

        [Dependency]
        public IInPrePayRecordModel InPrePayRecordModel { get; set; }
        public override void OnEntered(NavigationContext navigationContext)
        {
            InPrePayItemsData = new ListDataGrid.PageDataEx
            {
                CatalogContent = "",
                List = InPrePayRecordModel.Res住院预缴金充值记录查询.data,
                Tag = ""
            };
        }
    }
}

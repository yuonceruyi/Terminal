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
    public class InBedInfoViewModel : ViewModelBase
    {
        public override string Title => "住院床位列表";
        private string _hint = "住院床位信息";

        public string Hint
        {
            get { return _hint; }
            set
            {
                _hint = value;
                OnPropertyChanged();
            }
        }
        private ListDataGrid.PageDataEx _inBedInfoItemsData;
        public ListDataGrid.PageDataEx InBedInfoItemsData
        {
            get { return _inBedInfoItemsData; }
            set
            {
                _inBedInfoItemsData = value;
                OnPropertyChanged();
            }
        }

        [Dependency]
        public IInBedInfoModel InBedInfoModel { get; set; }
        public override void OnEntered(NavigationContext navigationContext)
        {
            InBedInfoItemsData = new ListDataGrid.PageDataEx
            {
                CatalogContent = "",
                List = InBedInfoModel.Res住院床位信息查询.data,
                Tag = ""
            };
        }
    }
}

using System.Linq;
using Microsoft.Practices.Unity;
using Prism.Regions;
using YuanTu.Consts.Models.Auth;
using YuanTu.Consts.Models.InfoQuery;
using YuanTu.Consts.UserControls;
using YuanTu.Core.Extension;
using YuanTu.Core.FrameworkBase;

namespace YuanTu.Default.Component.InfoQuery.ViewModels
{
    public class InDailyDetailViewModel : ViewModelBase
    {
        private ListDataGrid.PageDataEx _dailyDetailData;
        private string _hint = "住院一日清单";
        public override string Title => "住院一日清单";

        public string Hint
        {
            get { return _hint; }
            set
            {
                _hint = value;
                OnPropertyChanged();
            }
        }

        public ListDataGrid.PageDataEx DailyDetailData
        {
            get { return _dailyDetailData; }
            set
            {
                _dailyDetailData = value;
                OnPropertyChanged();
            }
        }

        [Dependency]
        public IPatientModel PatientModel { get; set; }

        [Dependency]
        public IInDailyDetailModel InDailyDetailModel { get; set; }

        public override void OnEntered(NavigationContext navigationContext)
        {
            var totalAmount = InDailyDetailModel.Res住院患者费用明细查询?.data.Sum(p => decimal.Parse(p.cost)).In元();
            Hint = $"住院一日清单  总计：{totalAmount}";
            DailyDetailData = new ListDataGrid.PageDataEx
            {
                CatalogContent = "",
                List = InDailyDetailModel.Res住院患者费用明细查询?.data,
                Tag = ""
            };
        }
    }
}
using System.Globalization;
using Microsoft.Practices.Unity;
using Prism.Regions;
using System.Linq;
using YuanTu.Consts.Gateway;
using YuanTu.Consts.Models.InfoQuery;
using YuanTu.Consts.UserControls;
using YuanTu.Core.Extension;
using YuanTu.Core.FrameworkBase;

namespace YuanTu.ChongQingArea.Component.InfoQuery.ViewModels
{

    //医改项目价格变动信息
    public class NewMedicineProjectViewModel : ViewModelBase
    {
        public override string Title => "医改项目信息列表";
        private ListDataGrid.PageDataEx _chargeItemsData;

        private string _hint = "医改项目信息列表";

        public string Hint
        {
            get { return _hint; }
            set
            {
                _hint = value;
                OnPropertyChanged();
            }
        }

        public ListDataGrid.PageDataEx ChargeItemsData
        {
            get { return _chargeItemsData; }
            set
            {
                _chargeItemsData = value;
                OnPropertyChanged();
            }
        }

        [Dependency]
        public IChargeItemsModel ChargeItemsModel { get; set; }


        public override void OnEntered(NavigationContext navigationContext)
        {
            HideNavigating = true;

            ChargeItemsData = new ListDataGrid.PageDataEx
            {
                CatalogContent = "",
                List = ChargeItemsModel.Res收费项目查询.data.Select(p => new
                {
                    itemCode=p.itemCode,
                    itemName = p.itemName,
                    priceUnit = p.priceUnit,
                    price = BuildMoney(p.price),
                    type=p.type,                   
                }),
                Tag = ""
            };
        }


        private string BuildMoney(string originmoney)
        {
            var money = originmoney;
            var pattern = "0.00";
            if (originmoney.Contains("."))
            {
                money = originmoney.TrimEnd(' ', '0');
                var k = money.Length - money.IndexOf('.');
                pattern = "0.".PadRight(k - 1 + 2 + 2, '0');
            }
            return (decimal.Parse(money) / 100).ToString(pattern) + "元";
        }
    }

}

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
    //医改药品价格信息
    public class NewMedicineItemsViewModel : ViewModelBase
    {
        private string _hint = "医改药品信息";

        public override string Title => "医改药品信息列表";

        private ListDataGrid.PageDataEx _medicineItemsData;

        public string Hint
        {
            get { return _hint; }
            set
            {
                _hint = value;
                OnPropertyChanged();
            }
        }

        public ListDataGrid.PageDataEx MedicineItemsData
        {
            get { return _medicineItemsData; }
            set
            {
                _medicineItemsData = value;
                OnPropertyChanged();
            }
        }


        [Dependency]
        public IMedicineModel MedicineModel { get; set; }

        public override void OnEntered(NavigationContext navigationContext)
        {
            HideNavigating = true;
            MedicineItemsData = new ListDataGrid.PageDataEx
            {
                CatalogContent = "",
                List = MedicineModel.Res药品项目查询.data.Where( t => (t.producer.IndexOf("自制") < 0 && t.type != null ) && ( t.price != t.extend )).Select(t => new
                {
                    medicineName = t.medicineName,
                    specifications=t.specifications,
                    priceUnit = t.priceUnit,
                    extend = BuildMoney(t.extend) ,
                    price = BuildMoney(t.price) ,
                    producer=t.producer,
                    type=t.type,
                    pricediscount = BuildMoney((decimal.Parse(t.price) - decimal.Parse(t.extend)).ToString(CultureInfo.InvariantCulture)) ,
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

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Input;
using Prism.Commands;
using Prism.Regions;
using YuanTu.Consts;
using YuanTu.Consts.Gateway;
using YuanTu.Consts.UserControls;
using YuanTu.Core.Extension;
using YuanTu.Core.FrameworkBase;

namespace YuanTu.Default.Component.ViewModels
{
    public class NameViewModel : ViewModelBase
    {
        private IReadOnlyCollection<PageData> _collection;
        public override string Title => "插卡";
      
        public NameViewModel()
        {
            TimeOut = 2000;
            Command = new DelegateCommand(Do);
            Console.WriteLine(DateTimeCore.Now);
        }

        public override bool OnLeaving(NavigationContext navigationContext)
        {
            return base.OnLeaving(navigationContext);
        }

        public override void OnEntered(NavigationContext navigationContext)
        {
            var content = File.ReadAllText("FakeServer\\获取缴费概要信息.json", Encoding.UTF8);
            //Collection=new List<PageData>()
            //{
            //    new PageData() {ButtonInfo = "王文文 神经外科\r\n金额20.00元",List = content.ToJsonObject<res获取缴费概要信息>().data },
            //    new PageData() {ButtonInfo = "王文文 神经外科\r\n金额20.00元",List = content.ToJsonObject<res获取缴费概要信息>().data },
            //    new PageData() {ButtonInfo = "王文文 神经外科\r\n金额20.00元",List = content.ToJsonObject<res获取缴费概要信息>().data },
            //};
            Collection = content.ToJsonObject<res获取缴费概要信息>().data.Select(p => new PageData
            {
                CatalogContent = $"{p.doctName} {p.deptName}\r\n金额 {p.billFee.In元()}",
                List = p.billItem
            }).ToArray();
            base.OnEntered(navigationContext);
        }

        public IReadOnlyCollection<PageData> Collection
        {
            get { return _collection; }
            set
            {
                _collection = value; 
                OnPropertyChanged();
            }
        }

        public ICommand Command { get; set; }

        private void Do()
        {
            ChangeNavigationContent("西门吹雪\r\n余额:2400.00元");
            Next();
        }
    }

    public class CurrentFeeDetail
    {
        public string 项目类型 { get; set; }
        public string 名称 { get; set; }
        public string 单价 { get; set; }
        public string 数量 { get; set; }
        public string 金额 { get; set; }
    }
}
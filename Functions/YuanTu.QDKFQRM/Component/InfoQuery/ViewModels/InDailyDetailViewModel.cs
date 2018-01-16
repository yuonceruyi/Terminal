using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prism.Regions;
using System.Windows;
using System.Windows.Documents;
using YuanTu.Core.Extension;

namespace YuanTu.QDKFQRM.Component.InfoQuery.ViewModels
{
    class InDailyDetailViewModel:YuanTu.QDKouQiangYY.Component.InfoQuery.ViewModels.InDailyDetailViewModel
    {
        protected override string Caption
        {
            get { return "青岛开发区第一人民医院"; }
        }
    }
}

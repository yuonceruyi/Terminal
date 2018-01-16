using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YuanTu.YiWuArea.Insurance.Models.Base;

namespace YuanTu.YiWuArea.Commons
{
    public static class ItemListExtendsion
    {
        public static ItemList<T> ToItemList<T>(this IEnumerable<T> ienum)
        {
            var lst = new ItemList<T>();
             lst.AddRange(ienum);
            return lst;
        }
    }
}

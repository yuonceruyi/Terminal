using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using YuanTu.YiWuArea.Insurance.Models.Base;

namespace YuanTu.YiWuArea.Insurance.Models
{
    public static class InternalTools
    {
        public static T GetValueBack<T>(T item, string text)
        {
            if (typeof(T) == typeof(string))
            {
                return (T) (object) text;
            }

            //if (typeof(ItemList<>).IsAssignableFrom(typeof(T)))
            //{
            //    var t = (ItemList<T>)(((object)item) ?? ((object)Activator.CreateInstance<T>()));
            //    t.Describe(text);
            //    return (T)(object)t;
            //}
            if (typeof(ItemBase).IsAssignableFrom(typeof(T)))
            {
                var t = (ItemBase)(((object)item) ?? ((object)Activator.CreateInstance<T>()));
                if (!string.IsNullOrWhiteSpace(text))
                {
                    var contentArr = Regex.Split(text, "%%");
                    if (contentArr.Length % t.ItemCount != 0)
                    {
                        throw new Exception($"数据长度不符合要求，要求{t.ItemCount} 实际{contentArr.Length}");
                    }
                    t.Descirbe(contentArr);
                }
               
                return (T)(object)t;
            }
           else
           {
                var t = (dynamic)((object)Activator.CreateInstance<T>());
                
                t.Describe(text);
                return (T)(object)t;
            }
            return default(T);
        }
    }
}

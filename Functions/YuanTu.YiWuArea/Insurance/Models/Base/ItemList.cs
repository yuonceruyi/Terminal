using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace YuanTu.YiWuArea.Insurance.Models.Base
{
    public class ItemList<T>:List<T> /*where T:ItemBase*/
    {
        public override string ToString()
        {
            return string.Join("%%",this);
        }

        public void Describe(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return;
            }
            var t = (ItemBase)(object)Activator.CreateInstance<T>();
            var contentArr = Regex.Split(text, "%%");
            if (contentArr.Length%t.ItemCount!=0)
            {
                throw new Exception($"数据长度不符合要求，要求{t.ItemCount} 实际{contentArr.Length}");
            }
            var total = contentArr.Length/t.ItemCount;
            for (int i = 0; i < total; i++)
            {
                var val = (ItemBase)(object)Activator.CreateInstance<T>();
                val.Descirbe(contentArr.Skip(t.ItemCount * i).Take(t.ItemCount).ToArray());
                this.Add((T)(object)val);
            }
        }

    }
}

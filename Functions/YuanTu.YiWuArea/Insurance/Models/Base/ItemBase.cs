using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YuanTu.YiWuArea.Insurance.Models.Base
{
    public abstract class ItemBase
    {
        public override string ToString()
        {
            return FormatData();
        }

        public abstract string FormatData();
        public abstract int ItemCount { get; }
        public abstract void Descirbe(string[] arr);
    }
}

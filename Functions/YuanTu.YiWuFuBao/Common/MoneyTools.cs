using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YuanTu.Core.Log;
using YuanTu.YiWuFuBao.Models;

namespace YuanTu.YiWuFuBao.Common
{
    public static class MoneyTools
    {
        public static decimal TrimToFen(this decimal fen)
        {
            return fen; //(int)(fen / 10 + 0.5m) * 10;
            //return Math.Round(fen / 10, MidpointRounding.AwayFromZero) * 10;
        }

        /// <summary>
        /// HIS传入的数量和贴数存在冲突，现在对数量进行更改
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public static string SafeGetQty(Chargeitem item)
        {
            var originQty = item.itemQty;
            var itemcount = item.itemCount;
            try
            {

                if (int.TryParse(originQty, out int qty))
                {
                    int count;
                    int.TryParse(itemcount, out count);
                    if (count == 0)
                    {
                        count = 1;
                    }
                    return (qty / count).ToString();
                }
                return originQty;
            }
            catch (Exception e)
            {
                Logger.Main.Error($"[社保]转换社保项目数量时出现异常入参：originQty：{originQty}，itemcount：{itemcount},详情：{e.Message}");
                return originQty;
            }
        }
    }
}

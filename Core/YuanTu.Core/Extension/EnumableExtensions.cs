using System;
using System.Collections.Generic;
using System.Linq;

namespace YuanTu.Core.Extension
{
    public static class EnumableExtensions
    {
        public static T Random<T>(this IEnumerable<T> @this, Func<T, bool> predicate=null)
        {
            if (@this == null)
            {
                return default(T);
            }
            var rnd = new Random();
            var enumerable = @this as T[] ?? @this.ToArray();
            var tp = predicate == null ? enumerable : enumerable.Where(predicate).ToArray();
            if (tp.Length==0)
            {
                return default(T);
            }
            var index = rnd.Next(0, tp.Length);
            return tp[index];

        }
    }
}

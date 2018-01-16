using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YuanTu.Core.Extension
{
    public static class NumberExtensions
    {
        /// <summary>
        /// 确保输入的参数是符合condition要求的
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data"></param>
        /// <param name="condition"></param>
        /// <param name="faildValue"></param>
        /// <returns></returns>
        public static T Sure<T>(this T data, Predicate<T> condition, T faildValue) //where T :IComparable<T>
        {
            if (condition.Invoke(data))
            {
                return data;
            }
            return faildValue;
        }
       
    }

   
}

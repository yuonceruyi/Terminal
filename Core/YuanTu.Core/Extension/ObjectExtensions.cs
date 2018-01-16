using System;
using System.Collections.Generic;
using System.Reflection;
using Newtonsoft.Json;

namespace YuanTu.Core.Extension
{
    public static class ObjectExtensions
    {
       

        public static T As<T>(this object @this )
        {
            if (@this is T)
            {
                if (@this is IConvertible)
                {
                    return (T)Convert.ChangeType(@this, typeof(T));
                }
                return (T) @this;
            }
            return default(T);
        }


        /// <summary>
        /// 将对象转换为string字符串
        /// </summary>
        /// <param name="this"></param>
        /// <returns></returns>
        public static string ToJsonString(this object @this)
        {
            return JsonConvert.SerializeObject(@this);
        }

        /// <summary>
        /// 将对象的属性转换为词典
        /// </summary>
        /// <param name="this"></param>
        /// <returns></returns>
        public static Dictionary<string, object> ToDictionary(this object @this)
        {
            var type = @this.GetType();
            var props= type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            var dic=new Dictionary<string,object>();
            foreach (var info in props)
            {
                dic.Add(info.Name,info.GetValue(@this));
            }
            return dic;
        } 

        /// <summary>
        /// 将Json字符串转换为对应的Json对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="this"></param>
        /// <returns></returns>
        public static T ToJsonObject<T>(this string @this)
        {
            return JsonConvert.DeserializeObject<T>(@this);
        }

    }

    
}

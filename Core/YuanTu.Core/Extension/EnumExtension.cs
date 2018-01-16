using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Media.TextFormatting;

namespace YuanTu.Core.Extension
{
    public static class EnumExtension
    {
        public static Attribute GetEnumAttribute(this Enum value,Type attribute)
        {
            var enumType = value.GetType();
            var name = Enum.GetName(enumType, value);
            if (name != null)
            {
                // 获取枚举字段。
                var fieldInfo = enumType.GetField(name);
                if (fieldInfo != null)
                {
                    // 获取描述的属性。
                    var attr = Attribute.GetCustomAttribute(fieldInfo,
                        attribute, false);
                    return attr;
                }
            }
            return null;
        }
        public static T GetEnumAttribute<T>(this Enum value)
        {
            var enumType = value.GetType();
            var name = Enum.GetName(enumType, value);
            if (name != null)
            {
                // 获取枚举字段。
                var fieldInfo = enumType.GetField(name);
                if (fieldInfo != null)
                {
                    // 获取描述的属性。
                    var attr = Attribute.GetCustomAttribute(fieldInfo, typeof(T), false);
                    return (T)(object)attr;
                }
            }
            return default(T);
        }

        public static string GetEnumDescription(this Enum value,string defaultval="")
        {
            var attr = GetEnumAttribute(value, typeof (DescriptionAttribute));
            return (attr as DescriptionAttribute)?.Description?? defaultval;
        }

        public static List<T> GetEnums<T>(this T value)
        {
            var lst = new List<T>();
            if (value is Enum)
            {
                var valData =Convert.ToInt32((T)Enum.Parse(typeof(T), value.ToString())) ;
                var tps =Enum.GetValues(typeof (T));

                lst.AddRange(from object tp in tps where ((int)Convert.ToInt32((T)Enum.Parse(typeof(T), tp.ToString())) & valData) == valData select (T) tp);
            }
            return lst;
        } 
    }
}

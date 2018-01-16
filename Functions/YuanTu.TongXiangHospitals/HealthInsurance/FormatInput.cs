using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace YuanTu.TongXiangHospitals.HealthInsurance
{
    public static class FormatInput
    {
        public static string MakeInput<T>(this T req, string split)
        {
            string reqString;
            var reqType = req.GetType();
            var basePropertyInfos = reqType.BaseType?.GetProperties();
            if (basePropertyInfos != null)
            {
                reqString = string.Join(split, basePropertyInfos
                    .Concat(reqType.GetProperties()
                    .Where(p => p.DeclaringType == reqType))
                    .Select(p => p.GetValue(req)));
            }
            else
            {
                reqString = string.Join(split, reqType.GetProperties()
                    .Where(p => p.DeclaringType == reqType)
                    .Select(p => p.GetValue(req)));
            }

            return reqString;
        }

        public class PropertyInfoComparer : IEqualityComparer<PropertyInfo>
        {
            public bool Equals(PropertyInfo x, PropertyInfo y)
            {
                return x.Name == y.Name;
            }

            public int GetHashCode(PropertyInfo obj)
            {
                return 0;
            }
        }

        public static T Decode<T>(this T res,string resString, char split)
        {
            var resArray = resString.Split(split);
            return Decode(res, resArray);
            //var resType = res.GetType();
            //var resPropertyInfos = resType.GetProperties();
            //var resBasePropertyInfos = resType.BaseType?.GetProperties();
            //if (resBasePropertyInfos != null)
            //{
            //    resPropertyInfos = resBasePropertyInfos.Concat(resPropertyInfos.Where(p=>p.DeclaringType==resType)).ToArray();
            //}
            //var count = resArray.Length;
            //var index = 0;
            //foreach (var p in resPropertyInfos.Where(p => index <= count - 1))
            //{
            //    p.SetValue(res, resArray[index]);
            //    index++;
            //}
            //return res;
        }
        public static T Decode<T>(this T res, string[] resStringArray)
        {
           
            var resType = res.GetType();
            var resPropertyInfos = resType.GetProperties();
            var resBasePropertyInfos = resType.BaseType?.GetProperties();
            if (resBasePropertyInfos != null)
            {
                resPropertyInfos = resBasePropertyInfos.Concat(resPropertyInfos.Where(p => p.DeclaringType == resType)).ToArray();
            }
            var count = resStringArray.Length;
            var index = 0;
            foreach (var p in resPropertyInfos.Where(p => index <= count - 1))
            {
                p.SetValue(res, resStringArray[index]);
                index++;
            }
            return res;
        }
    }
}
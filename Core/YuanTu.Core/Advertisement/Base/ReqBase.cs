using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using YuanTu.Consts;
using YuanTu.Consts.Enums;
using YuanTu.Core.Extension;
using YuanTu.Core.Systems;

namespace YuanTu.Core.Advertisement.Base
{
    public abstract class ReqBase
    {
        private static readonly Dictionary<Type, PropertyInfo[]> CacheDic = new Dictionary<Type, PropertyInfo[]>();
        public abstract string UrlPath { get; }
        public abstract int adPositionId { get; }

        public string corpId => FrameworkConst.HospitalId;
        public string deviceMac => NetworkManager.MAC;
        public int age { get; set; }
        public string cardNo { get; set; }
        public string idNo { get; set; }
        public Sex sex { get; set; }

        public virtual Dictionary<string, string> BuldDict()
        {
            var dic = new Dictionary<string, string>
            {
                {nameof(corpId), corpId},
                {nameof(deviceMac), deviceMac},
                {nameof(age), age.ToString()},
                {nameof(cardNo), cardNo},
                {nameof(idNo), idNo},
                {nameof(sex), ((int) sex).ToString()}
            };
            var kv = Build("", this);
            foreach (var pair in kv)
                dic[pair.Key] = pair.Value;
            return dic;
        }

        protected static Dictionary<string, string> Build(string prefix, object obj)
        {
            var realPrefx = prefix.IsNullOrWhiteSpace() ? "" : $"{prefix}.";
            var dic = new Dictionary<string, string>();
            if (obj != null)
            {
                var tp = obj.GetType();
                PropertyInfo[] props = null;
                if (!CacheDic.ContainsKey(tp))
                    CacheDic[tp] =
                        tp.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly);
                props = CacheDic[tp];

                foreach (var info in props)
                    if (info.PropertyType == typeof(string)) //字符串
                    {
                        dic[$"{realPrefx}{info.Name}"] = info.GetValue(obj) as string;
                    }
                    else if (typeof(IList).IsAssignableFrom(info.PropertyType)) //数组集合
                    {
                        var val = info.GetValue(obj) as IList;
                        if (val != null)
                            for (var i = 0; i < val.Count; i++)
                                if (val[i] is string)
                                {
                                    dic[$"{realPrefx}{info.Name}[{i}]"] = val[i] as string;
                                }
                                else
                                {
                                    var innDic = Build($"{realPrefx}{info.Name}[{i}]", val[i]);
                                    foreach (var kv in innDic)
                                        dic[kv.Key] = kv.Value;
                                }
                    }
                    else //自定义实体
                    {
                        var val = info.GetValue(obj);
                        if (val != null)
                        {
                            var innDic = Build($"{prefix}.{info.Name}", val);
                            foreach (var kv in innDic)
                                dic[kv.Key] = kv.Value;
                        }
                    }
            }

            return dic;
        }
    }
}
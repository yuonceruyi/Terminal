using System;
using System.Collections.Generic;
using System.Linq;

namespace YuanTu.Default.House.HealthManager
{
    public class Wrapper
    {
        public IDictionary<string, string> Query { get; set; }

        private string BeforeSign()
        {
            var arr = new Dictionary<string, string>();
            //限定用这个三个字段做签名算法
            arr.Add("idNo", Query["idNo"]);
            arr.Add("noncestr", Query["noncestr"]);
            arr.Add("timestamp", Query["timestamp"]);
            var array = arr.Where(one => !string.IsNullOrEmpty(one.Value))
                .OrderBy(one => one.Key, StringComparer.Ordinal)
                .Select(one => $"{one.Key}={one.Value}");
            return string.Join("&", array.ToArray());
        }

        public IDictionary<string, string> Content()
        {
            var before = BeforeSign();
            var sign = AES.Sign(before);
            var dic = Query.ToDictionary(one => one.Key, one => one.Value);
            dic.Add("signature", sign);
            return dic;
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;

namespace YuanTu.Consts.Gateway.Base
{
    public class Wrapper
    {
        public IDictionary<string, string> Query { get; set; }

        private string BeforeSign()
        {
            var array = Query.Where(one => !string.IsNullOrEmpty(one.Value))
                .OrderBy(one => one.Key, StringComparer.Ordinal)
                .Select(one => $"{one.Key}={one.Value}");
            return string.Join("&", array.ToArray());
        }

        public IDictionary<string, string> Content()
        {
            var before = BeforeSign();
            var sign = RSA.Sign(before);
            var dic = Query.ToDictionary(one => one.Key, one => one.Value);
            dic.Add("sign", sign);
            dic.Add("sign_type", "RSA");
            return dic;
        }
    }
}
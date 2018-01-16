using System;
using System.Collections.Generic;
using System.Linq;


namespace YuanTu.PanYu.House.PanYuGateway.Base
{
    public class wrapper
    {
        public IDictionary<string, string> query { get; set; }

        private string BeforeSign()
        {
            var array = query.Where(one=>!string.IsNullOrEmpty(one.Value))
                .OrderBy(one=>one.Key,StringComparer.Ordinal)
                .Select(one=> $"{one.Key}={one.Value}");
            return string.Join("&", array.ToArray());
        }

        public IDictionary<string, string> Content()
        {
            string before = BeforeSign();
            string sign = RSA.Sign(before);
            var dic = query.ToDictionary(one => one.Key, one => one.Value);
            dic.Add("sign", sign);
            dic.Add("sign_type", "RSA");
            return dic;
        }
    }
}
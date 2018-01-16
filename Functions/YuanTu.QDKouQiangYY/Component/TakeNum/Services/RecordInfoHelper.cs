using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YuanTu.Consts.FrameworkBase;

namespace YuanTu.QDKouQiangYY.Component.TakeNum.Services
{
    public class RecordInfoHelper
    {
        private static readonly Dictionary<string,Tuple<bool,string>>_mapping=new Dictionary<string, Tuple<bool, string>>
        {
            ["0"]=new Tuple<bool, string>(true,"【待取号】"),
            ["1"]=new Tuple<bool, string>(false, "【已取号】"),
            ["2"]=new Tuple<bool, string>(false, "【已取消】"),
            ["3"]=new Tuple<bool, string>(false, "【已过期】"),
            ["4"] = new Tuple<bool, string>(false, "【已停诊】"),
            ["5"] = new Tuple<bool, string>(false, "【已退号】"),
            ["101"] = new Tuple<bool, string>(false, "【下单中】"),
            ["102"] = new Tuple<bool, string>(false, "【订单已取消】"),
            ["404"] = new Tuple<bool, string>(false, "【订单已失效】"),

        }; 
        public static Result<Tuple<bool, string>> GetStatusEnables(string status)
        {
            if (_mapping.ContainsKey(status))
            {
                return Result<Tuple<bool, string>>.Success(_mapping[status]);
            }
            return Result<Tuple<bool, string>>.Fail($"{status}非法");


        } 
    }
}

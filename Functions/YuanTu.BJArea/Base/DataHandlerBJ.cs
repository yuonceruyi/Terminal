using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YuanTu.Consts.Gateway;
using YuanTu.Consts.Gateway.Base;

namespace YuanTu.BJArea.Base
{
    public partial class DataHandlerBJ: YuanTu.Consts.Gateway.DataHandlerEx
    {
        public static res病人建档发卡 病人建档发卡(req病人建档发卡 req)
        {
            return Handler.Query<res病人建档发卡, req病人建档发卡>(req);
        }
    }
    public class req病人建档发卡 : YuanTu.Consts.Gateway.req病人建档发卡
    {
        public string relation { get; set; }
        public string education { get; set; }
        public string nationality { get; set; }
        public string religion { get; set; }

        public override Dictionary<string, string> GetParams()
        {
            var dic = base.GetParams();
            dic[nameof(relation)] = relation;
            dic[nameof(education)] = education;
            dic[nameof(nationality)] = nationality;
            dic[nameof(religion)] = religion;
            return dic;
        }

    }
}

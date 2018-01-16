using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YuanTu.Consts.FrameworkBase;
using YuanTu.Consts.Gateway;

namespace YuanTu.Consts.Models.GateWayStatus
{

    public interface IGateWayStatusModel : IModel
    {
        req查询网关状态 Req查询网关状态 { get; set; }
        res查询网关状态 Res查询网关状态 { get; set; }
    }

    public class GateWayStatusModel : ModelBase, IGateWayStatusModel
    {
        public req查询网关状态 Req查询网关状态 { get; set; }
        public res查询网关状态 Res查询网关状态 { get; set; }
    }
}

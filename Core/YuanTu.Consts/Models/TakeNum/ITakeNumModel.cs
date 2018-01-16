using System.Collections.Generic;
using YuanTu.Consts.FrameworkBase;
using YuanTu.Consts.Gateway;
using YuanTu.Consts.Models.Payment;

namespace YuanTu.Consts.Models.TakeNum
{
    public interface ITakeNumModel : IModel
    {
       
        req预约取号 Req预约取号 { get; set; }
        res预约取号 Res预约取号 { get; set; }

        List<PayInfoItem> List { get; set; }
    }

    public class TakeNumModel : ModelBase, ITakeNumModel
    {

        public req预约取号 Req预约取号 { get; set; }
        public res预约取号 Res预约取号 { get; set; }
        public List<PayInfoItem> List { get; set; }
    }
}
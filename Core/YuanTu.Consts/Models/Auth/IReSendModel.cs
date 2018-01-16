using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YuanTu.Consts.FrameworkBase;
using YuanTu.Consts.Gateway;

namespace YuanTu.Consts.Models.Auth
{
    public interface IReSendModel : IModel
    {
        string idNo { get; set; }
        string name { get; set; }
        string guarderId { get; set; }
        req补卡查询 Req补卡查询 { get; set; }
        res补卡查询 Res补卡查询 { get; set; }
        req补卡 Req补卡 { get; set; }
        res补卡 Res补卡 { get; set; }
        Func<Result> Check { get; set; }
        Func<Result> Confirm { get; set; }
    }

    public class ReSendModel : ModelBase, IReSendModel
    {
        public string idNo { get; set; }
        public string name { get; set; }
        public string guarderId { get; set; }
        public req补卡查询 Req补卡查询 { get; set; }
        public res补卡查询 Res补卡查询 { get; set; }
        public req补卡 Req补卡 { get; set; }
        public res补卡 Res补卡 { get; set; }
        public Func<Result> Confirm { get; set; }
        public Func<Result> Check { get; set; }
    }
}

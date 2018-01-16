using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YuanTu.Consts.FrameworkBase;
using YuanTu.Consts.Gateway;

namespace YuanTu.Consts.Models.InfoQuery
{
    public interface  IChargeItemsModel:IModel
    {
        req收费项目查询 Req收费项目查询 { get; set; }
        res收费项目查询 Res收费项目查询 { get; set; }
    }

    public class ChargeItemsModel : ModelBase, IChargeItemsModel
    {
        public req收费项目查询 Req收费项目查询 { get; set; }
        public res收费项目查询 Res收费项目查询 { get; set; }
    }
}

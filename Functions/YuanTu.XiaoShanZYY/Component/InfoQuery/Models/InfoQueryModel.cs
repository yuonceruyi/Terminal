using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YuanTu.Consts.FrameworkBase;
using YuanTu.XiaoShanZYY.HIS;

namespace YuanTu.XiaoShanZYY.Component.InfoQuery.Models
{
    public interface IInfoQueryModel : IModel
    {
        DateTime DateTimeStart { get; set; }
        DateTime DateTimeEnd { get; set; }
        Res缴费结算查询 Res缴费结算查询 { get; set; }
    }
    class InfoQueryModel:ModelBase, IInfoQueryModel
    {
        public DateTime DateTimeStart { get; set; }
        public DateTime DateTimeEnd { get; set; }
        public Res缴费结算查询 Res缴费结算查询 { get; set; }
    }
}

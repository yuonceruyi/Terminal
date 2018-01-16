using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YuanTu.Consts.Gateway;
using YuanTu.YiWuArea.Insurance.Models;

namespace YuanTu.YiWuFuBao.Component.TakeNum.Models
{
    public class TakeNumModel:YuanTu.Consts.Models.TakeNum.TakeNumModel
    {

        //public req预约挂号预处理 Req预约挂号预处理 { get; set; }
        //public res预约挂号预处理 Res预约挂号预处理 { get; set; }

        public req缴费预结算 Req缴费预结算 { get; set; }
        public res缴费预结算 Res缴费预结算 { get; set; }


        public req获取缴费概要信息 Req获取缴费概要信息 { get; set; }
        public res获取缴费概要信息 Res获取缴费概要信息 { get; set; }
        public req缴费结算 Req缴费结算 { get; set; }
        public res缴费结算 Res缴费结算 { get; set; }

        public Req门诊预结算 Req社保门诊预结算 { get; set; }
        public Res门诊预结算 Res社保门诊预结算 { get; set; }

        public Req门诊结算 Req社保门诊结算 { get; set; }
        public Res门诊结算 Res社保门诊结算 { get; set; }

        public Guid TradeId { get; set; }
        public Res交易确认 Res交易确认 { get; set; }
        public 自负比例列表[] 所有自负比例列表 { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YuanTu.Consts.Gateway;
using YuanTu.YiWuArea.Insurance.Models;

namespace YuanTu.YiWuFuBao.Component.BillPay.Models
{
    public class BillPayModel: YuanTu.Consts.Models.BillPay.BillPayModel
    {

        public req缴费预结算 Req缴费预结算 { get; set; }
        public res缴费预结算 Res缴费预结算 { get; set; }

        #region[社保]

        public Req门诊预结算 Req社保门诊预结算 { get; set; }
        public Res门诊预结算 Res社保门诊预结算 { get; set; }

        public Req门诊结算 Req社保门诊结算 { get; set; }
        public Res门诊结算 Res社保门诊结算 { get; set; }

        /// <summary>
        /// 上传到中间件的交易Id
        /// </summary>
        public Guid TradeId { get; set; }

        public Res交易确认 Res交易确认 { get; set; }

        public 自负比例列表[] 所有自负比例列表 { get; set; }
        #endregion
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YuanTu.Consts;

namespace YuanTu.JiaShanHospital.NativeServices.Dto
{
    public abstract class RequestBase
    {
        private static readonly DateTime OriginTime=new DateTime(1970,1,1);
        /// <summary>
        /// 业务类型
        /// </summary>
        public abstract int BussinessType { get; set; }

        /// <summary>
        /// 就诊卡ID
        /// </summary>
        public string CardNo { get; set; }

        /// <summary>
        /// 本地HIS登陆名称，接口改变后，该参数失效，无需提供
        /// </summary>
        [Obsolete]
        public string LoginName { get; set; } = "";

        /// <summary>
        /// 本地HIS登陆密码，接口改变后，该参数失效，无需提供
        /// </summary>
        [Obsolete]
        public string Password { get; set; } = "";
        /// <summary>
        /// 时间戳
        /// </summary>
        public string TimeSpan { get; } = ((long)(DateTimeCore.Now - OriginTime).TotalMilliseconds).ToString();

        /// <summary>
        /// 支付宝交易号
        /// </summary>
        public string AlipayTradeNo { get; set; }
        /// <summary>
        /// 支付宝总金额(元)
        /// </summary>
        public string AlipayAmount { get; set; }
        /// <summary>
        /// 自费标识  1为自费
        /// </summary>
        public int SelfPayTag { get; set; }
    }
}

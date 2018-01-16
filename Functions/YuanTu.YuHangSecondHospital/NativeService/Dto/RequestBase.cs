using System;
using YuanTu.Consts;

namespace YuanTu.YuHangSecondHospital.NativeService.Dto
{
    public abstract class RequestBase
    {
        private static readonly DateTime OriginTime=new DateTime(1970,1,1);
        /// <summary>
        /// 业务类型
        /// </summary>
        public abstract int BussinessType { get; }

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
    }
}

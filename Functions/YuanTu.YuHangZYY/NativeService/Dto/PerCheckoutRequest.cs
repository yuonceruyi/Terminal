using System.Collections.Generic;

namespace YuanTu.YuHangZYY.NativeService.Dto
{
    public class PerCheckoutRequest: RequestBase
    {
        private string _checkoutType;

        /// <summary>
        /// 业务类型
        /// </summary>
        public override int BussinessType { get; } = 1;
        /// <summary>
        /// 支付方式
        /// </summary>
        public PayMedhodFlag PayFlag { get; set; }

        /// <summary>
        /// 预结账类型
        /// </summary>
        public string CheckoutType
        {
            get
            {
                return InternamMapping.ContainsKey(_checkoutType) ? InternamMapping[_checkoutType] : _checkoutType;
            }
            set { _checkoutType = value; }
        }

        private static readonly Dictionary<string,string> InternamMapping=new Dictionary<string, string>()
        {
            {"0000","00" },
            {"0010","01" },
            {"0100","03" },
        };

    }
}

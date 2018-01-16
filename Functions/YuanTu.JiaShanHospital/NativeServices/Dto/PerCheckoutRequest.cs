using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YuanTu.JiaShanHospital.NativeServices.Dto
{
    public class PerCheckoutRequest: RequestBase
    {
        private string _checkoutType;

        /// <summary>
        /// 业务类型
        /// </summary>
        public override int BussinessType { get; set; } = 1;
        /// <summary>
        /// 支付方式
        /// </summary>
        public PayMedhodFlag PayFlag { get; set; }

        /// <summary>
        /// 医疗类别
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

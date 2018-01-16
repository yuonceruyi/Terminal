using System.Text;

namespace YuanTu.TongXiangHospitals.HealthInsurance.Request
{
    public class ReqBase
    {
        /// <summary>
        /// 卡类型
        /// </summary>
        public string 卡类型 { get; set; }

        /// <summary>
        /// IC卡信息
        /// </summary>
        public string Ic卡信息 { get; set; }

        /// <summary>
        /// 现金支付方式
        /// </summary>
        public string 现金支付方式 { get; set; }

        /// <summary>
        /// 银行卡信息
        /// </summary>
        public string 银行卡信息 { get; set; }

        public new virtual string ToString()
        {
            var sb = new StringBuilder();
            sb.Append($"卡类型:{卡类型}\n");
            sb.Append($"Ic卡信息:{Ic卡信息}\n");
            sb.Append($"现金支付方式:{现金支付方式}\n");
            sb.Append($"银行卡信息:{银行卡信息}\n");
            return sb.ToString();
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YuanTu.Consts.FrameworkBase;

namespace YuanTu.QDArea.Models.TakeNum
{
    public interface ITakeNumExtendModel : IModel
    {
        /// <summary>
        /// 0 默认，网关调排班调HIS挂号
        /// 1 网关调排班锁号，排班调HIS挂号，预约收费专用
        /// 2 调HIS挂号
        /// </summary>
        string version { get; set; }

        /// <summary>
        /// 支付状态 C 只支持预缴金支付 100 未支付，200 支付成功，201 支付失败，500 已退费
        /// </summary>
        string payStatus { get; set; }

    }

    public class TakeNumExtendModel : ModelBase, ITakeNumExtendModel
    {
        public string version { get; set; } = "0";
        public string payStatus { get; set; } = "100";
    }
}

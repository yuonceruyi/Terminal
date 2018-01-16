using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YuanTu.Consts.FrameworkBase;

namespace YuanTu.QDArea.Models.Register
{
    public interface IRegisterExtendModel : IModel
    {
        /// <summary>
        /// 0 默认，网关调排班调HIS挂号
        /// 1 网关调排班锁号，排班调HIS挂号，预约收费专用
        /// 2 调HIS挂号
        /// </summary>
        string version { get; set; }

        /// <summary>
        /// 是否立即支付C	可空	挂号时可空，预约时必传 0否 1是
        /// </summary>
        string isPayNow { get; set; }

    }

    public class RegisterExtendModel : ModelBase, IRegisterExtendModel
    {
        public string version { get; set; } = "0";
        public string isPayNow { get; set; } = "";
    }
}

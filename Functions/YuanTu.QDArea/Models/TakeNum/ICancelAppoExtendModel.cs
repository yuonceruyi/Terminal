using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YuanTu.Consts.FrameworkBase;

namespace YuanTu.QDArea.Models.TakeNum
{
    public interface ICancelAppoExtendModel : IModel
    {
        /// <summary>
        /// 0 默认，网关调排班调HIS挂号
        /// 1 网关调排班锁号，排班调HIS挂号，预约收费专用
        /// 2 调HIS挂号
        /// </summary>
        string version { get; set; }

        /// <summary>
        /// 锁号id 可空 与orderNo必传一个
        /// </summary>
        string lockId { get; set; }

    }

    public class CancelAppoExtendModel : ModelBase, ICancelAppoExtendModel
    {
        public string version { get; set; } = "0";
        public string lockId { get; set; }
    }
}

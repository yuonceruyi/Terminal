using System;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YuanTu.BJArea.Enums
{
    [Flags]
    public enum ApptChargeMode
    {
        /// <summary>
        /// 不收费模式
        /// </summary>
        不收费 = 0,

        /// <summary>
        /// 必须收费模式,自助机逻辑暂未实现
        /// </summary>
        收费 = 1,

        /// <summary>
        /// 用户可选收费模式
        /// </summary>
        用户可选收费 = 2,
    }

}

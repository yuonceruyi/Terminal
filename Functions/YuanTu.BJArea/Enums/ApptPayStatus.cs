using System;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YuanTu.BJArea.Enums
{
    [Flags]
    public enum ApptPayStatus
    {
        [Description("【未支付】")]
        未支付 = 100,

        [Description("【已支付】")]
        支付成功 = 200,

        [Description("【支付失败】")]
        支付失败 = 201,

        [Description("【已退费】")]
        已退费 = 500,

        [Description("【未知】")]
        未知 = 999,
    }

}

using YuanTu.Consts.FrameworkBase;
using YuanTu.Consts.Gateway;

namespace YuanTu.Consts.Models.Payment
{
    public interface IScanQrCodeModel : IModel
    {
        req创建扫码订单 Req创建扫码订单 { get; set; }
        res创建扫码订单 Res创建扫码订单 { get; set; }
        req取消扫码订单 Req取消扫码订单 { get; set; }
        res取消扫码订单 Res取消扫码订单 { get; set; }
        req查询订单状态 Req查询订单状态 { get; set; }
        res查询订单状态 Res查询订单状态 { get; set; }
    }

    public class ScanQrCodeModel : ModelBase, IScanQrCodeModel
    {
        public req创建扫码订单 Req创建扫码订单 { get; set; }
        public res创建扫码订单 Res创建扫码订单 { get; set; }
        public req取消扫码订单 Req取消扫码订单 { get; set; }
        public res取消扫码订单 Res取消扫码订单 { get; set; }
        public req查询订单状态 Req查询订单状态 { get; set; }
        public res查询订单状态 Res查询订单状态 { get; set; }
    }
}
﻿using YuanTu.Consts.Gateway;

namespace YuanTu.YiWuZYY.Component.ViewModels.Models
{
    public class PaymentModel: YuanTu.Consts.Models.Payment.PaymentModel
    {
        public req预缴金消费 Req预缴金消费 { get; set; }
        public res预缴金消费 Res预缴金消费 { get; set; }

        public req预缴金消费冲正 Req预缴金消费冲正 { get; set; }
        public res预缴金消费冲正 Res预缴金消费冲正 { get; set; }
    }
}

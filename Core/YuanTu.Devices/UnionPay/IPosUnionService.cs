using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YuanTu.Consts.Dtos.UnionPos;
using YuanTu.Consts.Enums;
using YuanTu.Consts.FrameworkBase;
using YuanTu.Devices.CardReader;
using YuanTu.Devices.MKeyBoard;

namespace YuanTu.Devices.UnionPay
{
    public interface IPosUnionService:IDevice
    {
        /// <summary>
        /// 初始化POS引擎
        /// </summary>
        /// <param name="bankMediaType">可使用的介质类型</param>
        /// <param name="magCardReader"></param>
        /// <param name="icCardReader"></param>
        /// <param name="keyboard"></param>
        /// <returns></returns>
        Result Init(BanCardMediaType bankMediaType, IMagCardReader magCardReader, IIcCardReader icCardReader, IMKeyboard keyboard);

        /// <summary>
        /// 读卡
        /// </summary>
        /// <returns></returns>
        Result<string> ReadCard();

        /// <summary>
        /// 开始键盘录入
        /// </summary>
        /// <param name="keyctx"></param>
        /// <returns></returns>
        Result StartKeyboard(Action<string> keyctx);

        /// <summary>
        /// 执行扣费
        /// </summary>
        /// <param name="totalSeconds"></param>
        /// <returns></returns>
        Result<TransResDto> DoSale(decimal totalSeconds);

        /// <summary>
        /// 执行冲正
        /// </summary>
        /// <param name="dto"></param>
        /// <param name="reason"></param>
        /// <returns></returns>
        Result Refund(TransResDto dto,string reason);

        /// <summary>
        /// 结束操作，注意：如有必要请在该方法内主动退卡和关闭键盘
        /// </summary>
        /// <param name="reason"></param>
        /// <returns></returns>
        Result Uninit(string reason);

    }
}

using YuanTu.Consts.Enums;
using YuanTu.Consts.Models.Auth;
using YuanTu.YiWuArea.Insurance.Models;

namespace YuanTu.YiWuArea.Models
{
    public class CardModel: YuanTu.Consts.Models.Auth.DefaultCardModel
    {
        public Res获取参保人员信息 参保人员信息 { get; set; }
        public string SiPassword { get; set; } = "123456";

        /// <summary>
        /// 社保病人是否使用社保卡交易（针对特殊网络原因导致无法社保交易）
        /// </summary>
        public virtual bool SiCardUseSiNetWork { get; set; }

       
    }


    public static class CardModelHelper
    {
        public static bool CanUseInsurance(this ICardModel card)
        {
            return card.CardType == CardType.社保卡 && ((card as CardModel)?.SiCardUseSiNetWork ?? true);
        }
    }
}

using YuanTu.Consts.Enums;
using YuanTu.YuHangArea.CitizenCard;

namespace YuanTu.YuHangZYY.Component.Auth.Models
{
    public class CardModel:YuanTu.Consts.Models.Auth.DefaultCardModel
    {
        public Res读接触卡号 Res读接触卡号 { get; set; }
        public Res读非接触卡号 Res读非接触卡号 { get; set; }
        public CardType RealCardType { get; set; }
    }
}

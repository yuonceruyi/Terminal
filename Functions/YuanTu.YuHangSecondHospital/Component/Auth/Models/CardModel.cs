using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YuanTu.Consts.Enums;
using YuanTu.YuHangArea.CitizenCard;

namespace YuanTu.YuHangSecondHospital.Component.Auth.Models
{
    public class CardModel:YuanTu.Consts.Models.Auth.DefaultCardModel
    {
        public Res读接触卡号 Res读接触卡号 { get; set; }
        public Res读非接触卡号 Res读非接触卡号 { get; set; }
        public CardType RealCardType { get; set; }
    }
}

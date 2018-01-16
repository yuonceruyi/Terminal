using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YuanTu.Consts.Enums;

namespace YuanTu.ShenZhenArea.Models
{
    public class ShenZhenCardModel : YuanTu.Consts.Models.Auth.DefaultCardModel
    {
        /// <summary>
        /// 实际用于验证身份的卡类型
        /// </summary>
        public CardType RealCardType { get; set; }
    }
}

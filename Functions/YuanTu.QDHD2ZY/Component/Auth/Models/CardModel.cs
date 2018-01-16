using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YuanTu.QDHD2ZY.Component.Auth.Models
{
    public class CardModel : YuanTu.Consts.Models.Auth.DefaultCardModel
    {
        /// <summary>
        ///个人编号
        /// </summary>
        public string PersonNo { get; set; }

        /// <summary>
        ///单位名称
        /// </summary>
        public string UnitName { get; set; }
    }
}

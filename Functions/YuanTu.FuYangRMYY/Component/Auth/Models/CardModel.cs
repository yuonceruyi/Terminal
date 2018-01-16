using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YuanTu.FuYangRMYY.Component.Auth.Models
{
   public class CardModel:YuanTu.Consts.Models.Auth.DefaultCardModel
    {
        /// <summary>
        /// 社保卡信息
        /// </summary>
        public SiCardInfo SiCardInfo { get; set; }
    }
    
}

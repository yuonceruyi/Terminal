using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using YuanTu.Consts.Enums;
using YuanTu.Consts.FrameworkBase;
using YuanTu.Consts.Models.Auth;

namespace YuanTu.QDHD2ZY.Component.Models
{
    public interface IYbInfoModel : IModel
    {
        /// <summary>
        /// 是否门诊大病
        /// </summary>
        bool IsMZDB { get; set; }

        /// <summary>
        /// 单位：珠海、胶南
        /// </summary>
        string LocalTreatType { get; set; }


    }

    public class YbInfoModel : ModelBase, IYbInfoModel
    {
        public bool IsMZDB { get; set; } = false;

        public string LocalTreatType { get; set; } = string.Empty;

    }
}

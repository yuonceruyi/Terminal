using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace YuanTu.ShenZhenArea.Enums
{
    /// <summary>
    /// 挂号模式
    /// </summary>
    public enum regMode
    {
        [XmlEnum("1")]
        预约挂号 = 1,

        [XmlEnum("2")]
        当天挂号 = 2,
    }
}

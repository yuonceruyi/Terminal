using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YuanTu.Consts.FrameworkBase;
using YuanTu.Consts.Models.Payment;
using YuanTu.ZheJiangHospital.HIS;

namespace YuanTu.ZheJiangHospital.Component.TakeNum.Models
{
    public interface ITakeNumModel : IModel
    {
        YUYUE_JILU Record { get; set; }
        List<YUYUE_JILU> Records { get; set; }
        List<PayInfoItem> List { get; set; }
    }

    public class TakeNumModel : ModelBase, ITakeNumModel
    {
        public YUYUE_JILU Record { get; set; }
        public List<YUYUE_JILU> Records { get; set; }
        public List<PayInfoItem> List { get; set; }
    }
}

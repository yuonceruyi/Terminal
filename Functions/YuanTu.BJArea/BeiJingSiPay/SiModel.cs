using MedicareComLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YuanTu.Consts.FrameworkBase;

namespace YuanTu.BJArea.BeiJingSiPay
{
    public interface ISiModel : IModel
    {
        OutpatientClass Siobj { get; set; }
        bool IsOpened { get; set; }
        Res获取个人信息 res获取个人信息 { get; set; }
        Res获取卡内个人信息 res获取卡内个人信息 { get; set; }
        Req费用分解 req费用分解 { get; set; }
        Res费用分解 res费用分解 { get; set; }
        Res交易确认 res交易确认 { get; set; }

    }

    public class SiModel : ModelBase, ISiModel
    {
        public OutpatientClass Siobj { get; set; }
        public bool IsOpened { get; set; }
        public Res获取个人信息 res获取个人信息 { get; set; }
        public Res获取卡内个人信息 res获取卡内个人信息 { get; set; }
        public Req费用分解 req费用分解 { get; set; }
        public Res费用分解 res费用分解 { get; set; }
        public Res交易确认 res交易确认 { get; set; }
    }
}

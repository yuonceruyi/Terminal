using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YuanTu.Consts.Gateway;
using YuanTu.Consts.Models;
using YuanTu.Consts.Models.Payment;
using YuanTu.Core.Extension;

namespace YuanTu.JiaShanHospital.Component.TakeNum.ViewModels
{
    public class ApptRecordViewModel:YuanTu.Default.Component.TakeNum.ViewModels.ApptRecordViewModel
    {
        protected override void Confirm(Info i)
        {
            RecordModel.所选记录 = i.Tag.As<挂号预约记录>();
            ChangeNavigationContent(RecordModel.所选记录.doctName);
            var record = RecordModel.所选记录;

            TakeNumModel.List = new List<PayInfoItem>
            {
                new PayInfoItem("就诊日期：", record.medDate.SafeConvertToDate("yyyy-MM-dd","yyyy年MM月dd日")),
                new PayInfoItem("就诊科室：", record.deptName),
                new PayInfoItem("就诊医生：", record.doctName),
                new PayInfoItem("就诊时段：", $"{record.medAmPm.SafeToAmPm()}"),
                new PayInfoItem("就诊序号：", record.appoNo),
                new PayInfoItem("挂号金额：", record.regAmount.In元(), true)
            };

            Next();
        }
    }
}

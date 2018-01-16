using Prism.Commands;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YuanTu.Consts.Gateway;
using YuanTu.Consts.Models;
using YuanTu.Consts.Models.Payment;
using YuanTu.Consts.Services;
using YuanTu.Consts.Sounds;
using YuanTu.Core.Extension;

namespace YuanTu.ShengZhouHospital.Component.TakeNum.ViewModels
{
    public class ApptRecordViewModel:YuanTu.Default.Component.TakeNum.ViewModels.ApptRecordViewModel
    {
        public override void OnEntered(NavigationContext navigationContext)
        {
            ChangeNavigationContent("");
            var confirmCommand = new DelegateCommand<Info>(Confirm);

            var list = RecordModel.Res挂号预约记录查询.data.Select(p => new InfoAppt
            {
                Date = $"{p.medDate.SafeConvertToDate("yyyy-MM-dd", "yyyy年MM月dd日")}",
                Time = $"{p.medAmPm.SafeToAmPm()}",
                Department = $"{p.deptName.SafeSubstring(0, 7)}",
                Doctor = $"{p.doctName}",
                ConfirmCommand = confirmCommand,
                Tag = p,
                IsEnabled = true
            });
            Data = new ObservableCollection<InfoAppt>(list);
            
            
            PlaySound(SoundMapping.选择预约记录);
        }

        protected override void Confirm(Info i)
        {
            RecordModel.所选记录 = i.Tag.As<挂号预约记录>();
            ChangeNavigationContent(RecordModel.所选记录.doctName);
            var record = RecordModel.所选记录;

            TakeNumModel.List = new List<PayInfoItem>
            {
                new PayInfoItem("就诊日期：", record.medDate.SafeConvertToDate("yyyy-MM-dd","yyyy年MM月dd日")),
                new PayInfoItem("就诊科室：", record.deptName),
                new PayInfoItem("就诊时段：", $"{record.medAmPm.SafeToAmPm()} {record.medTime}"),
                new PayInfoItem("就诊序号：", record.appoNo),
            };
            if (!string.IsNullOrEmpty(record.doctName))
            {
                TakeNumModel.List.Add(new PayInfoItem("就诊医生：", record.doctName));
            }
            Next();
        }
    }
}

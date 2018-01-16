using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prism.Commands;
using Prism.Regions;
using YuanTu.Consts;
using YuanTu.Consts.Gateway;
using YuanTu.Consts.Models;
using YuanTu.Consts.Models.Payment;
using YuanTu.Consts.Services;
using YuanTu.Consts.Sounds;
using YuanTu.Core.Extension;
using YuanTu.QDKouQiangYY.Component.TakeNum.Services;
using YuanTu.QDArea.Enums;

namespace YuanTu.QDKouQiangYY.Component.TakeNum.ViewModels
{
    public class ApptRecordViewModel: YuanTu.Default.Component.TakeNum.ViewModels.ApptRecordViewModel
    {
        #region Overrides of ApptRecordViewModel

        /// <summary>
        /// 进入当期页面时触发
        /// </summary>
        /// <param name="navigationContext"/>
        public override void OnEntered(NavigationContext navigationContext)
        {
            ChangeNavigationContent("");
            var confirmCommand = new DelegateCommand<Info>(Confirm);
          
            var now = DateTimeCore.Today;
            var list = RecordModel.Res挂号预约记录查询.data.Where(p =>
            {
                DateTime cpdate;
                if (!DateTime.TryParseExact(p.medDate,"yyyy-MM-dd",null,DateTimeStyles.None, out cpdate))
                {
                    cpdate=DateTime.MaxValue;
                }
                return RecordInfoHelper.GetStatusEnables(p.status).IsSuccess&&cpdate>= now;
            }).Select(p => new InfoAppt
            {
                Date = $"{p.medDate.SafeConvertToDate("yyyy-MM-dd", "yyyy年MM月dd日")}",
                Time = $"{p.medAmPm.SafeToAmPm()}" + $"{p.medTime}",
                Department = $"{p.deptName.SafeSubstring(0,7)}",
                Doctor = $"{p.doctName}" + $" {RecordInfoHelper.GetStatusEnables(p.status).Value.Item2}",
                ConfirmCommand = confirmCommand,
                Tag = p,
                IsEnabled = RecordInfoHelper.GetStatusEnables(p.status).Value.Item1 //p.status == "1"
            }).OrderBy(p=>p.Date);
            if (list.FirstOrDefault() == null)
            {
                ShowAlert(false, "友好提示", "没有可用的预约记录");
                Navigate(A.Home);
                return;
            }
         
            Data = new ObservableCollection<InfoAppt>(list);
            
            
            PlaySound(SoundMapping.选择预约记录);
        }
        protected override void Confirm(Info i)
        {
            RecordModel.所选记录 = i.Tag.As<挂号预约记录>();
            ChangeNavigationContent(RecordModel.所选记录.doctName);
            var record = RecordModel.所选记录;

            var payStatus = "";
            if (!record.payStatus.IsNullOrWhiteSpace())
            {
                payStatus =((ApptPayStatus)Enum.Parse(typeof (ApptPayStatus), record.payStatus)).GetEnumDescription();
            }
            TakeNumModel.List = new List<PayInfoItem>
            {
                new PayInfoItem("就诊日期：", record.medDate.SafeConvertToDate("yyyy-MM-dd","yyyy年MM月dd日")),
                new PayInfoItem("就诊科室：", record.deptName),
                new PayInfoItem("就诊医生：", record.doctName),
                new PayInfoItem("就诊时段：", $"{record.medAmPm.SafeToAmPm()} {record.medTime}"),
                new PayInfoItem("就诊序号：", record.appoNo),
                new PayInfoItem("挂号金额：", $"{record.regAmount.In元()} {payStatus}", true),
                new PayInfoItem("预约状态：", RecordInfoHelper.GetStatusEnables(record.status).Value.Item2)
            };
            Next();
        }
        #endregion
    }
}

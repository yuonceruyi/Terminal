using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.Practices.Unity;
using Prism.Commands;
using Prism.Regions;
using YuanTu.Consts.Gateway;
using YuanTu.Consts.Models;
using YuanTu.Consts.Models.Auth;
using YuanTu.Consts.Models.Payment;
using YuanTu.Consts.Models.Print;
using YuanTu.Consts.Models.TakeNum;
using YuanTu.Consts.Services;
using YuanTu.Consts.Sounds;
using YuanTu.Core.Extension;
using YuanTu.Core.FrameworkBase;

namespace YuanTu.ShenZhen.BaoAnShiYanHospital.Component.TakeNum.ViewModels
{
    public class ApptRecordViewModel : YuanTu.Default.Component.TakeNum.ViewModels.ApptRecordViewModel
    { 
        public override void OnEntered(NavigationContext navigationContext)
        {
            ChangeNavigationContent("");
            var confirmCommand = new DelegateCommand<Info>(Confirm);

            var list = RecordModel.Res挂号预约记录查询.data.Select(p => new InfoAppt
            {
                Date = $"{p.medDate.SafeConvertToDate("yyyy-MM-dd","yyyy年MM月dd日")}",
                Time = $"{p.medAmPm.SafeToAmPm()}",
                Department = $"{p.deptName.SafeSubstring(0, 7)}",
                Doctor = $"{p.doctName}",
                ConfirmCommand = confirmCommand,
                Tag = p,
                IsEnabled = p.status == "0"
            });
            Data = new ObservableCollection<InfoAppt>(list);
            
            
            PlaySound(SoundMapping.选择预约记录);
        }

    }
}
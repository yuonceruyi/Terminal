using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using Microsoft.Practices.Unity;
using Prism.Commands;
using Prism.Regions;
using YuanTu.Consts;
using YuanTu.Consts.Dtos.UnionPos;
using YuanTu.Consts.Enums;
using YuanTu.Consts.FrameworkBase;
using YuanTu.Consts.Gateway;
using YuanTu.Consts.Models;
using YuanTu.Consts.Models.Auth;
using YuanTu.Consts.Models.Payment;
using YuanTu.Consts.Models.Print;
using YuanTu.Consts.Models.Register;
using YuanTu.Consts.Services;
using YuanTu.Consts.Sounds;
using YuanTu.Core.Extension;
using YuanTu.Core.FrameworkBase;
using YuanTu.Core.Navigating;
using YuanTu.Default.Component.Tools.Models;
using System.Drawing;
using YuanTu.Core.Services.PrintService;
using YuanTu.ShenZhenArea.Services;
using YuanTu.ShenZhenArea.Models;
using YuanTu.Core.Log;
using YuanTu.ShenZhenArea.Enums;

namespace YuanTu.ShenZhen.BaoAnChineseMedicineHospital.Component.Register.ViewModels
{
    public class SourceViewModel : YuanTu.Default.Component.Register.ViewModels.SourceViewModel
    {


        [Dependency]
        public IDeptartmentModel DepartmentModel { get; set; }
        protected override void Confirm(Info i)
        {
            SourceModel.所选号源 = i.Tag.As<号源明细>();
            ChangeNavigationContent(i.Title);
            PaymentModel.LeftList = new List<PayInfoItem>()
            {
                new PayInfoItem("日期：",ScheduleModel.所选排班.medDate),
                new PayInfoItem("上下午：",$"{ScheduleModel.所选排班.medAmPm.SafeToAmPm()}"),
                new PayInfoItem("时间段：",$"{SourceModel.所选号源?.medBegtime}到{SourceModel.所选号源?.medEndtime}"),
            };

            PaymentModel.RightList = new List<PayInfoItem>()
            {
                new PayInfoItem("科室：",ScheduleModel.所选排班.deptName?? DepartmentModel.所选科室?.deptName),
                new PayInfoItem("医生：",ScheduleModel.所选排班.doctName),
                new PayInfoItem("诊查费：",ScheduleModel.所选排班.regAmount.In元()),
            };

            Next();
        }
    }
}
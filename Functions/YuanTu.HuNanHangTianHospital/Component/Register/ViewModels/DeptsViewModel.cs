using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prism.Regions;
using YuanTu.Consts;
using YuanTu.Consts.Enums;
using YuanTu.Consts.Gateway;
using YuanTu.Consts.Models;
using YuanTu.Core.Extension;
using YuanTu.HuNanHangTianHospital.Common;

namespace YuanTu.HuNanHangTianHospital.Component.Register.ViewModels
{
    public class DeptsViewModel : YuanTu.Default.Component.Register.ViewModels.DeptsViewModel
    {
        #region Overrides of ApptRecordViewModel
        protected override void Confirm(Info i)
        {
            DeptartmentModel.所选科室 = i.Tag.As<排班科室信息>();
            Next();
        }
        #endregion
    }

}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prism.Regions;
using YuanTu.Core.Extension;
using YuanTu.YiWuFuBao.Models;

namespace YuanTu.YiWuFuBao.Component.Auth.ViewModels
{
    public class InPatientInfoViewModel:YuanTu.Default.Component.Auth.ViewModels.InPatientInfoViewModel
    {
        public override void OnEntered(NavigationContext navigationContext)
        {
            base.OnEntered(navigationContext);
            var pInfo = PatientModel as PatientModel;
            Name = pInfo.住院患者信息.name;
            Sex = pInfo.住院患者信息.sex;
            Birth = pInfo.住院患者信息.birthday;
            IdNo = pInfo.住院患者信息.idNo.Mask(14, 3);
            AccBalance = (pInfo.ZhuyuanryxxOut.ZHUYUANRYMX.ZHUYUANXX.YUJIAOKZE?.BackNotNullOrEmpty("0")) +"元";
        }

        public override void Confirm()
        {
            ChangeNavigationContent($"{Name}\r\n押金总额:{AccBalance}");
            Next();
        }
    }
}

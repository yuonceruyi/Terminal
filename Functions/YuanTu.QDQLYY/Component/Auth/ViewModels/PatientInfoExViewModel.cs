using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prism.Regions;
using YuanTu.Consts.Enums;
using YuanTu.Core.Extension;
using YuanTu.Devices.CardReader;
using YuanTu.Consts.Gateway;
using YuanTu.Consts;
using YuanTu.Core.FrameworkBase.Loadings;
using YuanTu.Consts.Models.Print;
using YuanTu.QDArea.Card;
using YuanTu.QDArea;
using YuanTu.Core.Reporter;
using YuanTu.Core.Log;
using System.Windows.Input;
using Prism.Commands;
using YuanTu.Consts.Services;
using YuanTu.Consts.Sounds;

namespace YuanTu.QDQLYY.Component.Auth.ViewModels
{
    class PatientInfoExViewModel : YuanTu.QDKouQiangYY.Component.Auth.ViewModels.PatientInfoExViewModel
    {
        public PatientInfoExViewModel(IRFCardDispenser[] rfCardDispenser) : base(rfCardDispenser)
        {
        }
        public override void OnEntered(NavigationContext navigationContext)
        {
            base.OnEntered(navigationContext);
            Hint = CreateModel.CreateType == CreateType.儿童 ? "请录入儿童信息" : "就诊人信息";
            Phone = null;
        }
    }
}

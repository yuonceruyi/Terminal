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

namespace YuanTu.QDKFQRM.Component.Auth.ViewModels
{
    class PatientInfoViewModel : YuanTu.QDKouQiangYY.Component.Auth.ViewModels.PatientInfoViewModel
    {
        public PatientInfoViewModel(IRFCardDispenser[] rfCardDispenser) : base(rfCardDispenser)
        {

        }
    }
}

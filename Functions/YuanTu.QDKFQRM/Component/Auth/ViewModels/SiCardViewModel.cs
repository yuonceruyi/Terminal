using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prism.Regions;
using YuanTu.Consts.Services;
using YuanTu.Core.FrameworkBase.Loadings;
using YuanTu.Consts.Gateway;
using YuanTu.Consts;
using YuanTu.Consts.Models;
using YuanTu.Consts.Enums;
using YuanTu.Core.Log;
using YuanTu.QDArea.QingDaoSiPay;
using YuanTu.Devices;
using YuanTu.Devices.CardReader;

namespace YuanTu.QDKFQRM.Component.Auth.ViewModels
{
    class SiCardViewModel : YuanTu.QDKouQiangYY.Component.Auth.ViewModels.SiCardViewModel
    {
        public SiCardViewModel(IIcCardReader[] icCardReaders, IRFCpuCardReader[] rfCpuCardReaders) : base(icCardReaders, rfCpuCardReaders)
        {

        }
    }
}

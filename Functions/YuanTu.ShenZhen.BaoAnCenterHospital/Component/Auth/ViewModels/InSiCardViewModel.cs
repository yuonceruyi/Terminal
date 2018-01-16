using System;
using YuanTu.Consts;
using YuanTu.Consts.Enums;
using YuanTu.Consts.Gateway;
using YuanTu.Core.Extension;
using YuanTu.Core.Log;
using YuanTu.Devices.CardReader;
using YuanTu.ShenZhen.BaoAnCenterHospital.Component.Auth.ViewModels;

namespace YuanTu.ShenZhen.BaoAnCenterHospital.Component.Auth.ViewModels
{
    public class InSiCardViewModel : SiCardViewModel
    {
        public InSiCardViewModel(IIcCardReader[] icCardReaders, IRFCpuCardReader[] rfCpuCardReaders) : base(icCardReaders, rfCpuCardReaders)
        {

        }


    }
}
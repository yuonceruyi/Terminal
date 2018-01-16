using Prism.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using YuanTu.Consts;
using YuanTu.Consts.Enums;
using YuanTu.Core.Log;
using YuanTu.Devices.CardReader;
using Prism.Regions;
using YuanTu.Consts.Services;
using YuanTu.Consts.Sounds;

namespace YuanTu.QDQLYY.Component.Auth.ViewModels
{
    public class IDCardViewModel : YuanTu.QDKouQiangYY.Component.Auth.ViewModels.IDCardViewModel
    {
        public IDCardViewModel(IIdCardReader[] idCardReaders) : base(idCardReaders)
        {           
        }      
    }
}

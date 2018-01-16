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

namespace YuanTu.QDKFQRM.Component.Auth.ViewModels
{
    class IDCardViewModel : YuanTu.QDKouQiangYY.Component.Auth.ViewModels.IDCardViewModel
    {
        public IDCardViewModel(IIdCardReader[] idCardReaders) : base(idCardReaders)
        {

        }
        public override void OnEntered(NavigationContext navigationContext)
        {
            var config = GetInstance<IConfigurationManager>();
            InputID = (config.GetValue("InputIdNo") ?? "1") == "1";

            TimeOut = 60;
            
            
            PlaySound(SoundMapping.请插入身份证);
            StartRead();
            CardModel.CardType = CardType.身份证;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YuanTu.Consts.Services;
using YuanTu.Devices.CashBox;

namespace YuanTu.BJArea.Component.Recharge.ViewModels
{
    public class CashViewModel:YuanTu.Default.Component.Recharge.ViewModels.CashViewModel
    {
        public CashViewModel(ICashInputBox[] cashInputBoxs) : base(cashInputBoxs)
        {

        }

        public override void OnSet()
        {
            BackUri = ResourceEngine.GetImageResourceUri("钞箱插卡口");
            CardUri = ResourceEngine.GetImageResourceUri("动画素材_人民币");
        }

    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prism.Regions;
using YuanTu.Core.Log;
using YuanTu.Devices.CardReader;

namespace YuanTu.YiWuArea.Component.ViewModels
{
    public class ChoiceViewModel : YuanTu.Default.Component.ViewModels.ChoiceViewModel
    {
        public override void OnEntered(NavigationContext navigationContext)
        {
            base.OnEntered(navigationContext);
            ExitCard();
        }

        protected virtual void ExitCard()
        {
            Task.Run(() =>
            {


                try
                {
                    var vm = GetInstance<IRFCpuCardReader[]>().FirstOrDefault(p => p.DeviceId == "ACT_A630_RFIC");
                    if (vm != null)
                    {
                        if (vm.Connect().IsSuccess)
                        {
                            var ret = vm.GetCardPosition();
                            var ignore = new[] {CardPos.无卡, CardPos.持卡位, CardPos.不持卡位,};
                            if (ret.IsSuccess && !ignore.Contains(ret.Value))
                            {
                                vm.MoveCard(CardPos.不持卡位);
                            }
                            vm.UnInitialize();

                            vm.DisConnect();
                        }
                    }
                }
                catch (Exception ex)
                {

                    Logger.Main.Error($"[主动退卡]主页主动退卡发生异常,{ex.Message} {ex.StackTrace}");
                }
            });
        }
    }
}

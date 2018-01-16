using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Prism.Regions;
using YuanTu.Consts.Services;
using YuanTu.Consts.Sounds;
using YuanTu.Core.Navigating;
using YuanTu.Devices.CardReader;

namespace YuanTu.YiWuArea.Component.ViewModels
{
    public class PrintViewModel: YuanTu.Default.Component.ViewModels.PrintViewModel
    {
        public override void OnEntered(NavigationContext navigationContext)
        {
            // 阻止导航栏点击
            GetInstance<INavigationModel>().PreventClick = true;

            Success = PrintModel.Success;
            TypeMsg = PrintModel.PrintInfo.TypeMsg;
            TipMsg = PrintModel.PrintInfo.TipMsg;

            if (!Success)
            {
                TipMsg = PrintModel.PrintInfo.TipMsg + "\r\n错误详情:" + PrintModel.PrintInfo.DebugInfo;
            }

            var resource = ResourceEngine;

            Source = resource.GetImageResourceUri(PrintModel.PrintInfo.TipImage ?? (Success ? "提示_正确" : "提示_感叹号"));



            if (PrintModel.NeedPrint)
            {
                PlaySound(SoundMapping.取走卡片及凭条);
                Task.Run(() => { PrintManager.Print(); });
            }
            else
            {
                PlaySound(SoundMapping.请取走卡片);
            }
            _doCommand = false;
            ExitCard();
        }
        
        protected virtual void ExitCard()
        {
            var vm = GetInstance<IRFCpuCardReader[]>().FirstOrDefault(p => p.DeviceId == "ACT_A630_RFIC");
            if (vm != null)
            {
                if (vm.Connect().IsSuccess)
                {
                    var ret = vm.GetCardPosition();
                    var ignore = new[] { CardPos.无卡, CardPos.持卡位, CardPos.不持卡位, };
                    if (ret.IsSuccess && !ignore.Contains(ret.Value))
                    {
                        vm.MoveCard(CardPos.不持卡位);
                    }
                    vm.UnInitialize();
                    vm.DisConnect();
                }
            }
        }
    }
}

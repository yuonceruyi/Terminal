using System;
using System.Linq;
using System.Windows.Input;
using Microsoft.Practices.Unity;
using Prism.Commands;
using Prism.Regions;
using YuanTu.Consts.Models.Print;
using YuanTu.Consts.Services;
using YuanTu.Consts.Sounds;
using YuanTu.Core.FrameworkBase;
using YuanTu.Core.Extension;
using YuanTu.Core.Navigating;

namespace YuanTu.ShenZhen.BaoAnShiYanHospital.Component.ViewModels
{
    public class PrintViewModel : Default.Component.ViewModels.PrintViewModel
    {
        public override void OnEntered(NavigationContext navigationContext)
        {
            // 阻止导航栏点击
            GetInstance<INavigationModel>().PreventClick = true;

            Success = PrintModel.Success;
            TypeMsg = PrintModel.PrintInfo.TypeMsg+ "    请妥善保管好您的卡片！";
            TipMsg = PrintModel.PrintInfo.TipMsg;

            if (!Success)
            {
                TipMsg = PrintModel.PrintInfo.TipMsg + "\r\n详情:" + PrintModel.PrintInfo.DebugInfo;
            }

            var resource =ResourceEngine;
         
            Source = resource.GetImageResourceUri(PrintModel.PrintInfo.TipImage ?? (Success ? "提示_正确" : "提示_感叹号"));

            
            
            if (PrintModel.NeedPrint)
            {
                PlaySound(SoundMapping.取走卡片及凭条);
                PrintManager.Print();
            }
            else
            {
                PlaySound(SoundMapping.请取走卡片);
            }
            _doCommand = false;


        }

    }
}
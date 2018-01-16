using Microsoft.Practices.Unity;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YuanTu.Consts.Models;
using YuanTu.Consts.Services;
using YuanTu.Consts.Sounds;
using YuanTu.Core.Navigating;

namespace YuanTu.WeiHaiZXYY.Component.ViewModels
{
    public class PrintViewModel:YuanTu.Default.Component.ViewModels.PrintViewModel
    {
        [Dependency]
        public IChoiceModel ChoiceModel { get; set; }
        public override void OnEntered(NavigationContext navigationContext)
        {
            // 阻止导航栏点击
            GetInstance<INavigationModel>().PreventClick = true;

            Success = PrintModel.Success;
            TypeMsg = PrintModel.PrintInfo.TypeMsg;
            TipMsg = PrintModel.PrintInfo.TipMsg;
            var resource = ResourceEngine;

            Source = resource.GetImageResourceUri(PrintModel.PrintInfo.TipImage ?? (Success ? "提示_正确" : "提示_感叹号"));



            if (PrintModel.NeedPrint)
            {
                PlaySound(SoundMapping.取走卡片及凭条);
                if (ChoiceModel.Business != Consts.Enums.Business.建档)
                {
                    PrintManager.Print();
                }

            }
            else
            {
                PlaySound(SoundMapping.请取走卡片);
            }
        }

        protected override void Confirm()
        {
            Next();
        }
    }
}

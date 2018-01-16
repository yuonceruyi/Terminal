using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YuanTu.Consts;
using YuanTu.Consts.Enums;
using YuanTu.Consts.Models;
using YuanTu.Consts.Models.PrintAgain;
using YuanTu.Core.Navigating;

namespace YuanTu.ShengZhouHospital.Component.PrintAgain.ViewModels
{
    public class PrintAgainChoiceViewModel:Default.Component.PrintAgain.ViewModels.PrintAgainChoiceViewModel
    {
        protected override void OnButtonClick(PrintAgainButtonInfo obj)
        {
            var engine = NavigationEngine;
            var choiceModel = GetInstance<IChoiceModel>();
            var printAgainChoiceModel = GetInstance<IPrintAgainModel>();
            choiceModel.HasAuthFlow = true;
            //手动清空导航栏
            GetInstance<INavigationModel>().Items.Clear();
            printAgainChoiceModel.PrintAgainType = obj.PrintAgainType;
            engine.JumpAfterFlow(new FormContext(A.ChaKa_Context, A.CK.HICard),
                CreateJump,
                new FormContext(AInner.SilpPrint, AInner.PTBD.Date), obj.Name);
        }
    }
}

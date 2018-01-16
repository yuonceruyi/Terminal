using Microsoft.Practices.Unity;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YuanTu.Consts.Enums;
using YuanTu.Consts.Models;
using YuanTu.Consts.Sounds;
using YuanTu.Core.Extension;

namespace YuanTu.ZheJiangZhongLiuHospital.Component.Recharge.ViewModels
{
    public class InputAmountViewModel: Default.Component.Recharge.ViewModels.InputAmountViewModel
    {
        [Dependency]
        public IChoiceModel ChoiceModel { get; set; }

        public override void OnEntered(NavigationContext navigationContext)
        {
            ChangeNavigationContent(null);
            Amount = null;


            if (ChoiceModel.Business == Business.挂号 || ChoiceModel.Business == Business.缴费 || ChoiceModel.Business == Business.取号)
            {
                Amount = ExtraPaymentModel.TotalMoney.ToString().InRMB();
            }

            //PlaySound(SoundMapping.输入充值金额);
        }
    }
}

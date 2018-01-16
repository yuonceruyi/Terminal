using System;
using System.Threading.Tasks;
using YuanTu.Consts;
using YuanTu.Consts.Enums;
using YuanTu.Consts.FrameworkBase;
using YuanTu.Consts.Models;
using YuanTu.Core.Navigating;
using YuanTu.Default.Tablet;

namespace YuanTu.VirtualHospital.Tablet.Component.ViewModels
{
    public class ChoiceViewModel : Default.Tablet.Component.ViewModels.ChoiceViewModel
    {
        protected override void Do(ChoiceButtonInfo param)
        {
            var choiceModel = GetInstance<IChoiceModel>();
            choiceModel.Business = param.ButtonBusiness;
            choiceModel.HasAuthFlow = true; //默认有插卡流程
            var engine = NavigationEngine;

            switch (param.ButtonBusiness)
            {
                case Business.生物信息录入:
                    engine.JumpAfterFlow(new FormContext(A.ChaKa_Context, A.CK.Card),
                        BiometricJump,
                        new FormContext(A.Biometric_Context, A.Bio.Choice), param.Name);
                    break;

                default:
                    base.Do(param);
                    break;
            }
        }

        private Task<Result<FormContext>> BiometricJump()
        {
            return null;
        }
    }
}
using Microsoft.Practices.Unity;
using YuanTu.Consts.Dtos.Register;
using YuanTu.Consts.Enums;
using YuanTu.Consts.Models;
using YuanTu.Core.Extension;
using YuanTu.XiaoShanZYY.Component.Register.Models;

namespace YuanTu.XiaoShanZYY.Component.Register.ViewModels
{
    internal class RegTypesViewModel : Default.Component.Register.ViewModels.RegTypesViewModel
    {
        [Dependency]
        public IRegisterModel Register { get; set; }
        [Dependency]
        public IRegisterService RegisterService { get; set; }

        protected override void Confirm(Info i)
        {
            var dto = i.Tag.As<RegTypeDto>();

            var regType = "0";
            switch (dto.RegType)
            {
                case RegType.普通门诊:
                    regType = "1";
                    break;

                case RegType.专家门诊:
                    regType = "4";
                    break;

                case RegType.急诊门诊:
                    regType = "2";
                    break;
            }
            Register.RegType = regType;

            if (ChoiceModel.Business == Business.挂号)
            {
                ChangeNavigationContent(i.Title);
                Next();
                return;
            }
            DoCommand(lp =>
            {
                lp.ChangeText("正在查询医院排班信息");
                var result = RegisterService.医院排班信息();
                if (!result.IsSuccess)
                {
                    ShowAlert(false, "查询医院排班信息", $"查询医院排班信息失败:\n{result.Message}");
                    return;
                }
                ChangeNavigationContent(i.Title);
                Next();
            });
        }
    }
}
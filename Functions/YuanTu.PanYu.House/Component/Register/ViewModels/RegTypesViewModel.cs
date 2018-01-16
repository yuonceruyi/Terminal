using Microsoft.Practices.Unity;
using YuanTu.Consts.Dtos.Register;
using YuanTu.Consts.Enums;
using YuanTu.Consts.Models;
using YuanTu.PanYu.House.PanYuService;

namespace YuanTu.PanYu.House.Component.Register.ViewModels
{
    public class RegTypesViewModel:Default.House.Component.Register.ViewModels.RegTypesViewModel
    {
        [Dependency]
        public IHisService HisService { get; set; }
        protected override void Confirm(Info i)
        {
            RegTypesModel.SelectRegType = (RegTypeDto)i.Tag;
            RegTypesModel.SelectRegTypeName = i.Title;
            HisService.RegMode = PanYuGateway.regMode.预约;
            HisService.RegType = GetRegType(RegTypesModel.SelectRegType.RegType);
            DoCommand(lp =>
            {
                lp.ChangeText("正在查询排班科室，请稍候...");
                var result = HisService.Run排班科室信息查询();
                if (!result.IsSuccess)
                {
                    ShowAlert(false,"温馨提示", result.Message);
                    return;
                }
                ChangeNavigationContent(".");//导航栏状态改成已完成
                Next();
            });
        }

        public PanYuGateway.regType GetRegType(RegType regType)
        {
            var type=((int)regType);
            switch (type)
            {
                case 1:
                    return PanYuGateway.regType.普通;
                   
                case 2:
                    return PanYuGateway.regType.专家;

                case 3:
                    return PanYuGateway.regType.名医;
                case 4:
                    return PanYuGateway.regType.急诊;
                case 5:
                    return PanYuGateway.regType.免费;
                default:
                    return PanYuGateway.regType.普通;
            }
            
        }
    }
}

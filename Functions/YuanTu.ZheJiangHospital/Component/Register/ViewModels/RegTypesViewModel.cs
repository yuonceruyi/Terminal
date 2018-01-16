using System.Linq;
using Microsoft.Practices.Unity;
using YuanTu.Consts.Dtos.Register;
using YuanTu.Consts.Enums;
using YuanTu.Consts.Models;
using YuanTu.ZheJiangHospital.Component.Register.Models;
using YuanTu.ZheJiangHospital.HIS;

namespace YuanTu.ZheJiangHospital.Component.Register.ViewModels
{
    public class RegTypesViewModel : Default.Component.Register.ViewModels.RegTypesViewModel
    {
        [Dependency]
        public IRegisterModel Register { get; set; }

        protected override void Confirm(Info i)
        {
            var type = (RegTypeDto) i.Tag;

            int regType;
            if (type.RegType == RegType.普通门诊)
                regType = 1;
            else if (type.RegType == RegType.专家门诊)
                regType = 3;
            else
                return;
            Register.RegType = regType;
            RegTypesModel.SelectRegType = type;
            RegTypesModel.SelectRegTypeName = i.Title;

            DoCommand(lp =>
            {
                lp.ChangeText("正在查询排班科室，请稍候...");

                if (regType == 1)
                {
                    var result = DataHandler.GetDept(regType);
                    if (!result.IsSuccess)
                    {
                        ShowAlert(false, "科室列表查询", "没有获得科室信息", debugInfo: result.Exception?.Message);
                        return;
                    }
                    if (!result.Value.Any())
                    {
                        ShowAlert(false, "科室列表查询", "没有获得科室信息(返回列表为空)");
                        return;
                    }
                    Register.IsDoctor = false;
                    Register.Depts = result.Value;
                }
                else
                {
                    var docsResult = DataHandler.GetScheduleDoctor();
                    if (!docsResult.IsSuccess)
                    {
                        ShowAlert(false, "医生排班列表查询", "没有获得医生排班信息", debugInfo: docsResult.Exception?.Message);
                        return;
                    }
                    if (!docsResult.Value.Any())
                    {
                        ShowAlert(false, "科室列表查询", "没有获得科室信息(返回列表为空)");
                        return;
                    }
                    Register.IsDoctor = true;
                    Register.DoctSchedules = docsResult.Value;
                }
                ChangeNavigationContent(i.Title);
                Next();
            });
        }
    }
}
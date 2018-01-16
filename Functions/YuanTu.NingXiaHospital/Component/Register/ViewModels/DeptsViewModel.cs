using YuanTu.Consts;
using YuanTu.Consts.Dtos.Register;
using YuanTu.Consts.Enums;
using YuanTu.Consts.Gateway;
using YuanTu.Consts.Models;
using YuanTu.Core.Extension;

namespace YuanTu.NingXiaHospital.Component.Register.ViewModels
{
    public class DeptsViewModel : Default.Component.Register.ViewModels.DeptsViewModel
    {
        protected override void Confirm(Info i)
        {
            DeptartmentModel.所选科室 = i.Tag.As<排班科室信息>();
            RegTypesModel.SelectRegType = new RegTypeDto();
            RegTypesModel.SelectRegType.RegType = RegType.普通门诊;
            DoCommand(lp =>
            {
                lp.ChangeText("正在查询医生信息，请稍候...");
                DoctorModel.查询所有医生信息 = new req查询所有医生信息
                {
                    deptCode = DeptartmentModel.所选科室.deptCode
                };
                DoctorModel.Res查询所有医生信息 = DataHandlerEx.查询所有医生信息(DoctorModel.查询所有医生信息);
                if (DoctorModel.Res查询所有医生信息?.success ?? false)
                    if (DoctorModel.Res查询所有医生信息?.data?.Count > 0)
                    {
                        ChangeNavigationContent(i.Title);
                        Navigate(ChoiceModel.Business == Business.挂号 ? A.XC.Doctor : A.YY.Doctor);
                    }
                    else
                    {
                        ShowAlert(false, "医生列表查询", "没有获得医生信息(列表为空)");
                    }
                else
                    ShowAlert(false, "医生列表查询", "没有获得医生信息", debugInfo: DoctorModel.Res查询所有医生信息?.msg);
            });
        }
    }
}
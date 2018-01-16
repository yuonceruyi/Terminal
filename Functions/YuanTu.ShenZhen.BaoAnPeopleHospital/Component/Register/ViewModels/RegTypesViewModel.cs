using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Media;
using Microsoft.Practices.Unity;
using Prism.Commands;
using Prism.Regions;
using YuanTu.Consts;
using YuanTu.Consts.Dtos.Register;
using YuanTu.Consts.Enums;
using YuanTu.Consts.Gateway;
using YuanTu.Consts.Models;
using YuanTu.Consts.Models.Register;
using YuanTu.Consts.Services;
using YuanTu.Consts.Sounds;
using YuanTu.Core.FrameworkBase;

namespace YuanTu.ShenZhen.BaoAnPeopleHospital.Component.Register.ViewModels
{
    public class RegTypesViewModel : Default.Component.Register.ViewModels.RegTypesViewModel
    {
        protected override void Confirm(Info i)
        {
            RegTypesModel.SelectRegType = (RegTypeDto)i.Tag ;
            RegTypesModel.SelectRegTypeName = i.Title;
            DoCommand(lp =>
            {
                lp.ChangeText("正在查询排班科室，请稍候...");
                DeptartmentModel.排班科室信息查询 = new req排班科室信息查询
                {
                    regMode = ChoiceModel.Business == Business.挂号 ? "2" : "1",
                    regType = ((int)RegTypesModel.SelectRegType.RegType).ToString(),
                    startDate =
                        ChoiceModel.Business == Business.挂号
                            ? DateTimeCore.Today.ToString("yyyy-MM-dd")
                            : RegDateModel.RegDate,
                    endDate =
                        ChoiceModel.Business == Business.挂号
                            ? DateTimeCore.Today.ToString("yyyy-MM-dd")
                            : RegDateModel.RegDate,
                     
                };
                DeptartmentModel.Res排班科室信息查询 = DataHandlerEx.排班科室信息查询(DeptartmentModel.排班科室信息查询);
                if (DeptartmentModel.Res排班科室信息查询?.success ?? false)
                {
                    if (DeptartmentModel.Res排班科室信息查询?.data?.Count > 0)
                    {
                        ChangeNavigationContent(i.Title);
                        Next();
                    }
                    else
                    {
                        ShowAlert(false, "科室列表查询", "没有获得科室信息(列表为空)");
                    }
                }
                else
                {
                    ShowAlert(false, "科室列表查询", "没有获得科室信息", debugInfo: DeptartmentModel.Res排班科室信息查询?.msg);
                }
            });
        }

       

    }
}
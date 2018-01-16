using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Practices.Unity;
using YuanTu.Consts;
using YuanTu.Consts.Enums;
using YuanTu.Consts.Gateway;
using YuanTu.Consts.Models;
using YuanTu.Consts.Models.Auth;
using YuanTu.Core.Extension;

namespace YuanTu.JiaShanHospital.Component.Register.ViewModels
{
    public class DeptsViewModel:YuanTu.Default.Component.Register.ViewModels.DeptsViewModel
    {
        [Dependency]
        public ICardModel CardModel { get; set; }

        protected override void Confirm(Info i)
        {
            DeptartmentModel.所选科室 = i.Tag.As<排班科室信息>();

            if (CardModel.CardType == CardType.社保卡 && DeptartmentModel.所选科室.deptCode == "10050102")
            {
                ShowAlert(false, "温馨提示", "产科科室 请到人工窗口受理");
                return;
            }

            if (!RegTypesModel.SelectRegType.SearchDoctor) //普通
            {
                DoCommand(lp =>
                {
                    lp.ChangeText("正在查询排班信息，请稍候...");
                    ScheduleModel.排班信息查询 = new req排班信息查询
                    {
                        regMode = ChoiceModel.Business == Business.挂号 ? "2" : "1",
                        regType = ((int)RegTypesModel.SelectRegType.RegType).ToString(),
                        deptCode = DeptartmentModel.所选科室.deptCode,
                        parentDeptCode = DeptartmentModel.所选科室.parentDeptCode,
                        startDate =
                            ChoiceModel.Business == Business.挂号
                                ? DateTimeCore.Today.ToString("yyyy-MM-dd")
                                : RegDateModel.RegDate,
                        endDate =
                            ChoiceModel.Business == Business.挂号
                                ? DateTimeCore.Today.ToString("yyyy-MM-dd")
                                : RegDateModel.RegDate
                    };
                    ScheduleModel.Res排班信息查询 = DataHandlerEx.排班信息查询(ScheduleModel.排班信息查询);
                    if (ScheduleModel.Res排班信息查询?.success ?? false)
                    {
                        if (ScheduleModel.Res排班信息查询?.data?.Count > 0)
                        {
                            ChangeNavigationContent(i.Title);
                            Navigate(ChoiceModel.Business == Business.挂号 ? A.XC.Schedule : A.YY.Schedule);
                        }
                        else
                        {
                            ShowAlert(false, "排班列表查询", "没有获得排班信息(列表为空)");
                        }
                    }
                    else
                    {
                        ShowAlert(false, "排班列表查询", "没有获得排班信息", debugInfo: ScheduleModel.Res排班信息查询?.msg);
                    }
                });
            }
            else
            {
                DoCommand(lp =>
                {
                    lp.ChangeText("正在查询医生信息，请稍候...");
                    DoctorModel.查询所有医生信息 = new req查询所有医生信息
                    {
                        deptCode = DeptartmentModel.所选科室.deptCode,
                        extend = ((int)RegTypesModel.SelectRegType.RegType).ToString(),
                    };
                    DoctorModel.Res查询所有医生信息 = DataHandlerEx.查询所有医生信息(DoctorModel.查询所有医生信息);
                    if (DoctorModel.Res查询所有医生信息?.success ?? false)
                    {
                        if (DoctorModel.Res查询所有医生信息?.data?.Count > 0)
                        {
                            ChangeNavigationContent(i.Title);
                            Navigate(ChoiceModel.Business == Business.挂号 ? A.XC.Doctor : A.YY.Doctor);
                        }
                        else
                        {
                            ShowAlert(false, "医生列表查询", "没有获得医生信息(列表为空)");
                        }
                    }
                    else
                    {
                        ShowAlert(false, "医生列表查询", "没有获得医生信息", debugInfo: DoctorModel.Res查询所有医生信息?.msg);
                    }
                });
            }
        }
    }
}

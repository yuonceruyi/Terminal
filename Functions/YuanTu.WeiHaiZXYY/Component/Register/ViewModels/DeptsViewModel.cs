using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prism.Regions;
using YuanTu.Consts;
using YuanTu.Consts.Enums;
using YuanTu.Consts.Gateway;
using YuanTu.Consts.Models;
using YuanTu.Core.Extension;
using YuanTu.WeiHaiZXYY.Component.Register.Services;

namespace YuanTu.WeiHaiZXYY.Component.Register.ViewModels
{
    public class DeptsViewModel : YuanTu.Default.Component.Register.ViewModels.DeptsViewModel
    {
        protected override void Confirm(Info i)
        {
            DeptartmentModel.所选科室 = i.Tag.As<排班科室信息>();
            (ScheduleModel as Models.ScheduleModel).IsDoctor = false;
            (ScheduleModel as Models.ScheduleModel).DoctorCode = "";
            DoCommand(lp =>
            {
                lp.ChangeText("正在查询医生信息，请稍候...");

                DoctorModel.医生信息查询 = new req医生信息查询
                {
                    deptCode = DeptartmentModel.所选科室.deptCode,
                    extend = DateTimeCore.Now.Date.ToString("yyyy-MM-dd")
                };
                DoctorModel.Res医生信息查询 = DataHandlerEx.医生信息查询(DoctorModel.医生信息查询);
                if (DoctorModel.Res医生信息查询?.success ?? false)
                {
                    if (DoctorModel.Res医生信息查询?.data?.Count > 0)
                    {
                        DoctorModel.Res医生信息查询?.data.ForEach(p =>
                        {
                            if (!DocExtendDic.DocDictionary.ContainsKey(p.doctCode))
                            {
                                DocExtendDic.DocDictionary.Add(p.doctCode, p);
                            }
                        });
                        ChangeNavigationContent(i.Title);
                        Navigate(ChoiceModel.Business == Business.挂号 ? A.XC.Doctor : A.YY.Doctor);
                    }
                    else
                    {
                        ShowAlert(false, "医生信息查询", "没有获得医生信息", debugInfo: DoctorModel.Res医生信息查询?.msg);
                    }
                }
                else
                {
                    ShowAlert(false, "医生信息查询", "没有获得医生信息", debugInfo: DoctorModel.Res医生信息查询?.msg);
                }
            });
        }
    }
}

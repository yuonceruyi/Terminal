using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prism.Commands;
using Prism.Regions;
using YuanTu.Consts;
using YuanTu.Consts.Enums;
using YuanTu.Consts.Gateway;
using YuanTu.Consts.Models;
using YuanTu.Consts.Services;
using YuanTu.Consts.Sounds;
using YuanTu.Core.Extension;

namespace YuanTu.YiWuFuBao.Component.Register.ViewModels
{
    public class DoctorViewModel: YuanTu.Default.Component.Register.ViewModels.DoctorViewModel
    {
        public override void OnEntered(NavigationContext navigationContext)
        {
            ChangeNavigationContent("");
            var confirmCommand = new DelegateCommand<Info>(Confirm);

            var res = ResourceEngine;
            var list = DoctorModel.Res查询所有医生信息.data.Select(p => new InfoDoc
            {
                Title = p.doctName,
                Tag = p,
                ConfirmCommand = confirmCommand,
                IconUri = string.IsNullOrEmpty(p.doctLogo) ? (
                    p.sex == "女"
                    ? res.GetImageResourceUri("图标_通用医生_女")
                    : res.GetImageResourceUri("图标_通用医生_男")) :
                    new Uri(p.doctLogo),
                Description = p.doctSpec,
                Rank = p.doctProfe,
                // 无法获取
                Amount = null,
                Remain = null,
            });
            Data = new ObservableCollection<Info>(list);

            
            PlaySound(ChoiceModel.Business == Business.挂号 ? SoundMapping.选择挂号医生 : SoundMapping.选择预约医生);
        }

        protected override void Confirm(Info i)
        {
            DoctorModel.医生介绍 = i.Tag.As<医生介绍>();

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
                                : RegDateModel.RegDate,
                    medAmPm = ((int)RegDateModel.AmPm).ToString()
                };
                ScheduleModel.Res排班信息查询 = DataHandlerEx.排班信息查询(ScheduleModel.排班信息查询);
                if (ScheduleModel.Res排班信息查询?.success ?? false)
                {
                    if (ScheduleModel.Res排班信息查询?.data?.Count > 0)
                    {
                        ScheduleModel.Res排班信息查询.data.ForEach(p => p.medAmPm = ((int)RegDateModel.AmPm).ToString());
                        ChangeNavigationContent(i.Title);
                        Next();
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
    }
}

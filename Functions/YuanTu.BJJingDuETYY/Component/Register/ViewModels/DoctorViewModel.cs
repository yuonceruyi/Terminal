using System;
using System.Collections.ObjectModel;
using System.Linq;
using Prism.Commands;
using Prism.Regions;
using YuanTu.Consts;
using YuanTu.Consts.Enums;
using YuanTu.Consts.Models;
using YuanTu.Consts.Services;
using YuanTu.Consts.Sounds;
using YuanTu.Consts.Gateway;
using YuanTu.Core.Extension;
using YuanTu.BJArea.Services.Register;

namespace YuanTu.BJJingDuETYY.Component.Register.ViewModels
{
    public class DoctorViewModel : Default.Component.Register.ViewModels.DoctorViewModel
    {
        public override void OnEntered(NavigationContext navigationContext)
        {
            ChangeNavigationContent("");
            var confirmCommand = new DelegateCommand<Info>(Confirm);

            var res = ResourceEngine;
            var curAmpm = DateTimeCore.Now.Hour >= 12 ? "下午" : "上午";
            //医生           
            var listDoc =
                ScheduleModel.Res排班信息查询.data
                .Where(p => !p.doctCode.IsNullOrWhiteSpace() && p.medAmPm.SafeToAmPmEnum() == RegDateModel.AmPm)
                .Select(p => new InfoDoc
                {
                    Title = p.doctName ?? DeptartmentModel.所选科室?.deptName,
                    Tag = p,
                    ConfirmCommand = confirmCommand,
                    IconUri =
                        string.IsNullOrEmpty(DocExtendDic.DocDictionary.ContainsKey(p.doctCode)
                            ? DocExtendDic.DocDictionary[p.doctCode].doctPictureIntranetUrl
                            : "")
                            ? (
                                (DocExtendDic.DocDictionary.ContainsKey(p.doctCode)
                                    ? DocExtendDic.DocDictionary[p.doctCode].sex
                                    : "") == "女"
                                    ? res.GetImageResourceUri("图标_通用医生_女")
                                    : res.GetImageResourceUri("图标_通用医生_男"))
                            : new Uri(DocExtendDic.DocDictionary[p.doctCode].doctPictureIntranetUrl),
                    Description =
                        DocExtendDic.DocDictionary.ContainsKey(p.doctCode)
                            ? DocExtendDic.DocDictionary[p.doctCode].doctSpec
                            : "",
                    Rank =
                        DocExtendDic.DocDictionary.ContainsKey(p.doctCode)
                            ? DocExtendDic.DocDictionary[p.doctCode].doctProfe
                            : "",
                    Amount = decimal.Parse(p.regAmount),
                    Remain = int.Parse(p.restnum),
                    IsEnabled = int.Parse(p.restnum) > 0,
                    DisableText = GetDisableText(p,curAmpm)
                }).OrderByDescending(p => p.Remain);

            //科室
            var listDept =
                    ScheduleModel.Res排班信息查询.data
                    .Where(p => p.doctCode.IsNullOrWhiteSpace() && p.medAmPm.SafeToAmPmEnum() == RegDateModel.AmPm)
                    .Select(p => new InfoDoc
                    {
                        Title = DeptartmentModel.所选科室?.deptName,
                        Tag = p,
                        ConfirmCommand = confirmCommand,
                        IconUri = res.GetImageResourceUri("图标_通用科室"),
                        Description = "",
                        Rank = p.subRegType == null ? RegTypesModel.SelectRegTypeName : (p.subRegType == "1" ? "主任医师" : "副主任医师"),
                        Amount = decimal.Parse(p.regAmount),
                        Remain = int.Parse(p.restnum),
                        IsEnabled = int.Parse(p.restnum) > 0,
                        DisableText = GetDisableText(p, curAmpm)
                    });
            Data = new ObservableCollection<Info>(listDept.Union(listDoc).ToList());


            PlaySound(ChoiceModel.Business == Business.挂号 ? SoundMapping.选择挂号医生 : SoundMapping.选择预约医生);
        }

        private string GetDisableText(排班信息 p, string curAmpm)
        {
            string disableText = string.Empty;
            if (p.restnum == "-1")
            {
                disableText = "停诊";
            }
            if (p.restnum == "0")
            {
                disableText = "已满";
            }

            return disableText;
        }
        protected override void Confirm(Info i)
        {
            ScheduleModel.所选排班 = i.Tag.As<排班信息>();
            var local = (ScheduleModel as Models.ScheduleModel);
            //此处用于标记号源显示页面是否按照医生Id筛选
            local.IsDoctor = !ScheduleModel.所选排班.doctCode.IsNullOrWhiteSpace();
            local.DoctorCode = ScheduleModel.所选排班.doctCode;
            ChangeNavigationContent(i.Title);
            Next();
        }
    }
}

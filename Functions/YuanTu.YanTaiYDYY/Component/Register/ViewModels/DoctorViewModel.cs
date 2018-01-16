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
using YuanTu.YanTaiYDYY.Component.Register.Services;
using Microsoft.Practices.Unity;
using YuanTu.Consts.Models.Register;
using YuanTu.Consts.Models.Payment;
using YuanTu.Consts.FrameworkBase;
using YuanTu.Consts.Models.Auth;
using YuanTu.Consts.Models.Print;
using YuanTu.Default.Component.Tools.Models;
using YuanTu.Core.Navigating;
using YuanTu.Consts.Dtos.UnionPos;

namespace YuanTu.YanTaiYDYY.Component.Register.ViewModels
{
    public class DoctorViewModel : YuanTu.Default.Component.Register.ViewModels.DoctorViewModel
    {
        public override void OnEntered(NavigationContext navigationContext)
        {
            ChangeNavigationContent("");
            var confirmCommand = new DelegateCommand<Info>(Confirm);
            var res = ResourceEngine;

            //医生           
            var listDoc =
                ScheduleModel.Res排班信息查询.data
                .Where(p => !p.doctCode.IsNullOrWhiteSpace())
                .GroupBy(p => new { p.doctCode, p.doctName, p.regAmount })
                .Select(p => new InfoDoc
                {
                    Title = p.Key.doctName ?? DeptartmentModel.所选科室?.deptName,
                    Tag = p.FirstOrDefault(),
                    ConfirmCommand = confirmCommand,
                    IconUri =
                        string.IsNullOrEmpty(DocExtendDic.DocDictionary.ContainsKey(p.Key.doctCode)
                            ? DocExtendDic.DocDictionary[p.Key.doctCode].doctLogo
                            : "")
                            ? (
                                (DocExtendDic.DocDictionary.ContainsKey(p.Key.doctCode)
                                    ? DocExtendDic.DocDictionary[p.Key.doctCode].sex
                                    : "") == "女"
                                    ? res.GetImageResourceUri("图标_通用医生_女")
                                    : res.GetImageResourceUri("图标_通用医生_男"))
                            : new Uri(DocExtendDic.DocDictionary[p.Key.doctCode].doctLogo),
                    Description =
                        DocExtendDic.DocDictionary.ContainsKey(p.Key.doctCode)
                            ? DocExtendDic.DocDictionary[p.Key.doctCode].doctSpec
                            : "",
                    Rank =
                        DocExtendDic.DocDictionary.ContainsKey(p.Key.doctCode)
                            ? DocExtendDic.DocDictionary[p.Key.doctCode].doctProfe
                            : "",
                    Amount = decimal.Parse(p.Key.regAmount),
                    Remain = p.Sum(c => int.Parse(c.restnum)),
                    IsEnabled = (p.Sum(c => int.Parse(c.restnum)) > 0),
                });

            //科室
            var listDept =
                    ScheduleModel.Res排班信息查询.data
                    .Where(p => p.doctCode.IsNullOrWhiteSpace())
                    .GroupBy(p => new { p.regAmount })
                    .Select(p => new InfoDoc
                    {
                        Title = DeptartmentModel.所选科室?.deptName,
                        Tag = p.FirstOrDefault(),
                        ConfirmCommand = confirmCommand,
                        IconUri = res.GetImageResourceUri("图标_通用科室"),
                        Description = "",
                        Rank = RegTypesModel.SelectRegTypeName ?? "",
                        Amount = decimal.Parse(p.Key.regAmount),
                        Remain = p.Sum(c => int.Parse(c.restnum)),
                        IsEnabled = (p.Sum(c => int.Parse(c.restnum)) > 0),
                    });
            Data = new ObservableCollection<Info>(listDept.Union(listDoc).ToList());

            
            PlaySound(ChoiceModel.Business == Business.挂号 ? SoundMapping.选择挂号医生 : SoundMapping.选择预约医生);
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

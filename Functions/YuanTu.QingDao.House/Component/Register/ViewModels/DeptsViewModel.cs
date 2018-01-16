using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.Practices.Unity;
using Prism.Commands;
using Prism.Regions;
using YuanTu.Consts;
using YuanTu.Consts.Enums;
using YuanTu.Consts.Models;
using YuanTu.Consts.Models.UserCenter.Register;
using YuanTu.Consts.Sounds;
using YuanTu.Consts.UserCenter;
using YuanTu.Consts.UserCenter.Entities;
using YuanTu.Core.Extension;

namespace YuanTu.QingDao.House.Component.Register.ViewModels
{
    public class DeptsViewModel : Default.House.Component.Register.ViewModels.DeptsViewModel
    {
        [Dependency]
        public IRegisterModel RegisterModel { get; set; }

        public override void OnEntered(NavigationContext navigationContext)
        {
            ChangeNavigationContent("");
            var confirmCommand = new DelegateCommand<Info>(Confirm);

            //var list = RegisterModel.Res科室列表.data.multiDeptOutParams.deptOutParams
            //    .Where(p => p.children!=null && p.children.Count > 0)
            //    .Select(p => new Info
            //{
            //    Title = p.deptName,
            //    Tag = p,
            //    ConfirmCommand = confirmCommand
            //});
            var list = RegisterModel.Res科室列表.data.multiDeptOutParams.deptOutParams
                .Where(p => p.children != null && p.children.Count > 0).ToList();
            var infoList = (from l in list
                from c in l.children
                select new Info
                {
                    Title = c.deptName,
                    Tag = c,
                    ConfirmCommand = confirmCommand
                }).ToList();

            Data = new ObservableCollection<Info>(infoList);

            PlaySound(ChoiceModel.Business == Business.挂号 ? SoundMapping.选择挂号科室 : SoundMapping.选择预约科室);
        }

        protected override void Confirm(Info i)
        {
            RegisterModel.当前选择科室 = i.Tag.As<Dept>();

            DoCommand(lp =>
            {
                lp.ChangeText("正在查询排班信息，请稍候...");
                var req = new req按医生排班列表
                {
                    corpId = RegisterModel.当前选择医院.corpId.ToString(),
                    regMode = ChoiceModel.Business == Business.挂号 ? "2" : "1",
                    regType = null,
                    deptCode = RegisterModel.当前选择科室.deptCode,
                    parentDeptCode = RegisterModel.当前选择科室.parentDeptCode
                };
                var res = DataHandlerEx.按医生排班列表(req);
                if (res?.success ?? false)
                    if (res?.data.scheduleTypeVOList?.Count > 0)
                    {
                        RegisterModel.Res按医生排班列表 = res;
                        ChangeNavigationContent(".");
                        Navigate(ChoiceModel.Business == Business.挂号 ? A.XC.Schedule : A.YY.Schedule);
                    }
                    else
                    {
                        ShowAlert(false, "排班列表查询", "没有获得排班信息(列表为空)");
                    }
                else
                    ShowAlert(false, "排班列表查询", "没有获得排班信息", debugInfo: res?.msg);
            });
        }
    }
}
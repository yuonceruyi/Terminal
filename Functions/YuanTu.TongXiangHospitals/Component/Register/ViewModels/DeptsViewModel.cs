using System;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.Practices.Unity;
using Prism.Commands;
using Prism.Regions;
using YuanTu.Consts;
using YuanTu.Consts.Enums;
using YuanTu.Consts.FrameworkBase;
using YuanTu.Consts.Gateway;
using YuanTu.Consts.Models;
using YuanTu.Consts.Models.Auth;
using YuanTu.Consts.Sounds;
using YuanTu.Core.Extension;
using YuanTu.Core.FrameworkBase;

namespace YuanTu.TongXiangHospitals.Component.Register.ViewModels
{
    public class DeptsViewModel : Default.Component.Register.ViewModels.DeptsViewModel
    {
        [Dependency]
        public IPatientModel PatientModel { get; set; }

        public override void OnEntered(NavigationContext navigationContext)
        {
            ChangeNavigationContent("");
            var confirmCommand = new DelegateCommand<Info>(Confirm);
            
            var list = DeptartmentModel.Res排班科室信息查询.data
                .Select(p => new Info
                {
                    Title = p.deptName,
                    Tag = p,
                    ConfirmCommand = confirmCommand,
                });
            Data = new ObservableCollection<Info>(list);

            PlaySound(ChoiceModel.Business == Business.挂号 ? SoundMapping.选择挂号科室 : SoundMapping.选择预约科室);
        }

        protected virtual Result DeptCheck(排班科室信息 item)
        {
            var info = PatientModel.当前病人信息;
            bool? isAdult;
            if (DateTime.TryParse(info.birthday, out var birthday))
                isAdult = DateTimeCore.Now > birthday.AddYears(18);
            else
                isAdult = null;

            var isMale = info.sex.SafeToSex() == Sex.男;
            var deptName = item.deptName;
            // 成人不能挂儿科
            if (deptName.Contains("儿科"))
                if (isAdult.HasValue && isAdult.Value)
                    return Result.Fail("成人不能挂儿科");

            //男性不能挂妇科
            if (deptName.Contains("妇科") || deptName.Contains("产科"))
                if (isMale)
                    return Result.Fail("男性不能挂妇科产科");

            return Result.Success();
        }

        protected override void Confirm(Info i)
        {
            var item = i.Tag.As<排班科室信息>();

            var checkResult = DeptCheck(item);
            if (!checkResult.IsSuccess)
            {
                ShowAlert(false, "确认科室", checkResult.Message);
                return;
            }

            DeptartmentModel.所选科室 = item;

            DoCommand(lp =>
            {
                lp.ChangeText("正在查询排班信息，请稍候...");
                var req = new req排班信息查询
                {
                    regMode = ChoiceModel.Business == Business.挂号 ? "2" : "1",
                    regType = ((int)RegTypesModel.SelectRegType.RegType).ToString(),
                    deptCode = item.deptCode,
                    parentDeptCode = item.parentDeptCode,
                    startDate =
                        ChoiceModel.Business == Business.挂号
                            ? DateTimeCore.Today.ToString("yyyy-MM-dd")
                            : RegDateModel.RegDate,
                    endDate =
                        ChoiceModel.Business == Business.挂号
                            ? DateTimeCore.Today.ToString("yyyy-MM-dd")
                            : RegDateModel.RegDate
                };
                ScheduleModel.排班信息查询 = req;
                var res = DataHandlerEx.排班信息查询(req);
                ScheduleModel.Res排班信息查询 = res;
                if (res == null || !res.success)
                {
                    ShowAlert(false, "排班列表查询", "没有获得排班信息", debugInfo: res?.msg);
                    return;
                }

                if (res.data == null || res.data.Count <= 0)
                {
                    ShowAlert(false, "排班列表查询", "没有获得排班信息(列表为空)");
                    return;
                }

                ChangeNavigationContent(i.Title);

                // 普通门诊 挂号 默认只有一个排班 和科室名重复 这里自动选择
                if (ChoiceModel.Business == Business.挂号 
                    && RegTypesModel.SelectRegType.RegType == RegType.普通门诊
                    && res.data.Count == 1
                    )
                {
                    var vm = GetInstance<ViewModelBase>(GetScheduleViewModelName());
                    if (vm is ScheduleViewModel svm)
                    {
                        lp.ChangeText("正在进行门诊挂号预结算，请稍候...");
                        ScheduleModel.所选排班 = res.data[0];
                        var result = svm.PreSettle();
                        if (!result.IsSuccess)
                        {
                            ShowAlert(false, "温馨提示", result.Message);
                            return;
                        }

                        Navigate(A.XC.Confirm);
                        return;
                    }
                }

                Navigate(ChoiceModel.Business == Business.挂号 ? A.XC.Schedule : A.YY.Schedule);
            });
        }

        protected virtual string GetScheduleViewModelName()
        {
            return typeof(ScheduleViewModel).FullName;
        }
    }
}

using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.Practices.Unity;
using Prism.Commands;
using Prism.Regions;
using YuanTu.Consts;
using YuanTu.Consts.Enums;
using YuanTu.Consts.FrameworkBase;
using YuanTu.Consts.Models;
using YuanTu.Consts.Sounds;
using YuanTu.Core.Extension;
using YuanTu.XiaoShanZYY.Component.Register.Models;
using YuanTu.XiaoShanZYY.HIS;

namespace YuanTu.XiaoShanZYY.Component.Register.ViewModels
{
    internal class DoctorViewModel : Default.Component.Register.ViewModels.DoctorViewModel
    {
        [Dependency]
        public IRegisterModel Register { get; set; }

        [Dependency]
        public IRegisterService RegisterService { get; set; }

        public override void OnEntered(NavigationContext navigationContext)
        {
            ChangeNavigationContent("");
            var confirmCommand = new DelegateCommand<Info>(Confirm);
            var subTitle = $"{Register.RegDate.SafeConvertToDate("yyyy-MM-dd", "MM月dd日")} {Register.AmPm.SafeToAmPm()}";

            var list = Register.YISHENGMX.Select(p =>
            {
                var infoMore = new InfoMore
                {
                    Title = p.YISHENGXM,
                    SubTitle = subTitle,
                    Type = p.YISHENGZC,
                    ConfirmCommand = confirmCommand,
                    Amount = null,
                    Tag = p
                };
                if (ChoiceModel.Business == Business.挂号)
                {
                    var rest = int.Parse(p.YISHENGJS);
                    infoMore.Extends = $"已挂号:{p.YISHENGTC} 剩余:{p.YISHENGJS}";
                    infoMore.IsEnabled = rest > 0;
                    infoMore.DisableText = rest <= 0 ? "已满" : string.Empty;
                }
                return infoMore;
            });
            Data = new ObservableCollection<Info>(list);

            PlaySound(ChoiceModel.Business == Business.挂号 ? SoundMapping.选择挂号医生 : SoundMapping.选择预约医生);
        }

        protected override void Confirm(Info i)
        {
            DoCommand(lp =>
            {
                var doctor = i.Tag.As<YISHENGXX>();
                Register.所选医生 = doctor;
                Register.DoctorId = doctor.YISHENGDM;
                Register.DoctorName = doctor.YISHENGXM;

                if (ChoiceModel.Business == Business.挂号)
                {
                    lp.ChangeText("正在计算费用信息，请稍候...");
                    var result = RegisterService.汇总挂号(Confirm);
                    if (!result.IsSuccess)
                    {
                        ShowAlert(false, "汇总挂号信息", $"汇总挂号信息失败:{result.Message}");
                        return;
                    }
                    Next();
                }
                else
                {
                    lp.ChangeText("正在查询号源信息，请稍候...");
                    var result = RegisterService.挂号号源信息();
                    if (!result.IsSuccess)
                    {
                        ShowAlert(false, "查询挂号号源信息", $"查询挂号号源信息失败:{result.Message}");
                        return;
                    }
                    Next();
                }
            });
           
        }

        protected Result Confirm()
        {
            return DoCommand(lp =>
            {
                lp.ChangeText("正在挂号，请稍后...");
                var result = RegisterService.挂号();
                if (!result.IsSuccess)
                {
                    ShowAlert(false, "挂号处理", $"挂号处理失败:\n{result.Message}");
                    return result;
                }
                RegisterService.挂号打印();
                Navigate(A.XC.Print);
                return Result.Success();
            }).Result;
        }
    }
}
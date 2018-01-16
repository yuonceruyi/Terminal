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
using IRegisterModel = YuanTu.XiaoShanZYY.Component.Register.Models.IRegisterModel;

namespace YuanTu.XiaoShanZYY.Component.Register.ViewModels
{
    internal class ScheduleViewModel : Default.Component.Register.ViewModels.ScheduleViewModel
    {
        [Dependency]
        public IRegisterModel Register { get; set; }

        [Dependency]
        public IRegisterService RegisterService { get; set; }

        public override void OnEntered(NavigationContext navigationContext)
        {
            ChangeNavigationContent("");
            var confirmCommand = new DelegateCommand<Info>(Confirm);

            var item = Register.所选排班Item;
            var len = item.DeptName.Length + 1;
            var list = item.PAIBANLB.Select(p =>
            {
                var infoMore = new InfoMore
                {
                    Title = p.KESHIMC.Substring(len),
                    SubTitle = $"{p.PAIBANRQ.SafeConvertToDate("yyyy-MM-dd", "MM月dd日")} {p.GUAHAOBC.SafeToAmPm()}",
                   // Type = "总费用",
                  //  Amount = (decimal.Parse(p.ZHENLIAOF) + (decimal.Parse(p.ZHENLIAOJSF))) * 100,
                    ConfirmCommand = confirmCommand,
                    Tag = p
                };
                if (ChoiceModel.Business == Business.挂号)
                {
                    int count;
                    int rest;
                    switch (p.GUAHAOBC)
                    {
                        case "0":
                            count = p.SHANGWUHYZS + p.XIAWUHYZS;
                            rest = p.SHANGWUHYSYS + p.XIAWUHYSYS;
                            break;

                        case "1":
                            count = p.SHANGWUHYZS;
                            rest = p.SHANGWUHYSYS;
                            break;

                        case "2":
                            count = p.XIAWUHYZS;
                            rest = p.XIAWUHYSYS;
                            break;

                        default:
                            count = 0;
                            rest = 0;
                            break;
                    }
                    infoMore.Extends = $"已挂号:{count} 剩余:{rest}";
                    infoMore.IsEnabled = rest > 0;
                    infoMore.DisableText = rest <= 0 ? "已满" : string.Empty;
                }
                return infoMore;
            });
            Data = new ObservableCollection<InfoMore>(list);

            PlaySound(ChoiceModel.Business == Business.挂号 ? SoundMapping.选择挂号医生 : SoundMapping.选择预约医生);
        }

        protected override void Confirm(Info i)
        {
            var item = Register.所选排班Item;
            var d = i.Tag.As<PAIBANMX>();
            Register.所选排班 = d;
            Register.DeptId = d.KESHIDM.Substring(item.DeptId.Length + 1);
            Register.DeptName = d.KESHIMC.Substring(item.DeptName.Length + 1);
            Register.所选医生 = null;

            DoCommand(lp =>
            {
                lp.ChangeText("正在查询医生信息，请稍候...");
                var result = RegisterService.挂号医生信息();
                if (result.IsSuccess)
                {
                    ChangeNavigationContent(i.Title);
                    Next();
                    return;
                }
                if (result.ResultCode != -1)
                {
                    ShowAlert(false, "查询挂号医生信息", $"查询挂号医生信息失败:\n{result.Message}");
                    return;
                }
                if (ChoiceModel.Business == Business.挂号)
                {
                    lp.ChangeText("正在计算挂号费用，请稍候...");
                    var result3 = RegisterService.汇总挂号(Confirm);
                    if (!result3.IsSuccess)
                    {
                        ShowAlert(false, "汇总挂号信息", "汇总挂号信息失败:" + result3.Message);
                        return;
                    }
                    ChangeNavigationContent(i.Title);
                    Navigate(A.XC.Confirm);
                    return;
                }
                lp.ChangeText("正在查询挂号号源信息，请稍候...");
                var result2 = RegisterService.挂号号源信息();
                if (!result2.IsSuccess)
                {
                    ShowAlert(false, "查询挂号号源信息", $"查询挂号号源信息失败:\n{result2.Message}");
                    return;
                }
                ChangeNavigationContent(i.Title);
                Navigate(A.YY.Date);
            });
        }

        protected override Result Confirm()
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
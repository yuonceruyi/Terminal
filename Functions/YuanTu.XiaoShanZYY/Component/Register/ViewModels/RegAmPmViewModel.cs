using System;
using System.Linq;
using Microsoft.Practices.Unity;
using YuanTu.Consts;
using YuanTu.Consts.Enums;
using YuanTu.Consts.Models;
using YuanTu.Core.Extension;
using YuanTu.Default.Component.Register.Models;
using YuanTu.XiaoShanZYY.Component.Register.Models;
using YuanTu.XiaoShanZYY.HIS;

namespace YuanTu.XiaoShanZYY.Component.Register.ViewModels
{
    internal class RegAmPmViewModel : Default.Component.Register.ViewModels.RegAmPmViewModel
    {
        [Dependency]
        public IChoiceModel ChoiceModel { get; set; }
        [Dependency]
        public IRegisterModel Register { get; set; }

        [Dependency]
        public IRegisterService RegisterService { get; set; }

        protected override void Confirm(Info i)
        {
            var cfg = i.Tag as AmPmConfig;
            var date = DateTimeCore.Today.ToString("yyyy-MM-dd");
            var start = DateTime.ParseExact($"{date} {cfg.StartTime}", "yyyy-MM-dd HH:mm", null);
            var end = DateTime.ParseExact($"{date} {cfg.EndTime}", "yyyy-MM-dd HH:mm", null);
            if (DateTimeCore.Now < start || DateTimeCore.Now > end)
            {
                ShowAlert(false, "挂号限制", $"该场次仅能在{cfg.StartTime}-{cfg.EndTime}时间范围内操作");
                return;
            }
            var session = i.Title.SafeToAmPmEnum();
            RegDateModel.AmPm = session;
            switch (session)
            {
                case AmPmSession.全天:
                    Register.AmPm = "0";
                    break;

                case AmPmSession.上午:
                    Register.AmPm = "1";
                    break;

                case AmPmSession.下午:
                    Register.AmPm = "2";
                    break;

                case AmPmSession.昼夜:
                    Register.AmPm = "0";
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
            DoCommand(lp =>
            {
                lp.ChangeText("正在查询医院排班信息");
                var result = RegisterService.医院排班信息();
                if (!result.IsSuccess)
                {
                    ShowAlert(false, "查询医院排班信息", $"查询医院排班信息失败:\n{result.Message}");
                    return;
                }

                ChangeNavigationContent(i.Title);
                Next();
            });
        }
    }
}
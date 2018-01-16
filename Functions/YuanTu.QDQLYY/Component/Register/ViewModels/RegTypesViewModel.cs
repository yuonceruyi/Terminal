using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YuanTu.Consts;
using YuanTu.Consts.Dtos.Register;
using YuanTu.Consts.Enums;
using YuanTu.Consts.Gateway;
using YuanTu.Consts.Models;
using YuanTu.Consts.Services;
using YuanTu.Core.Extension;

namespace YuanTu.QDQLYY.Component.Register.ViewModels
{
    public class RegTypesViewModel:YuanTu.QDKouQiangYY.Component.Register.ViewModels.RegTypesViewModel
    {
        protected override void Confirm(Info i)
        {
            RegTypesModel.SelectRegType = (RegTypeDto)i.Tag;

            var config = GetInstance<IConfigurationManager>();

            if (ChoiceModel.Business == Business.挂号 && RegTypesModel.SelectRegType.RegType != RegType.急诊门诊 && config.GetValueInt("StopReg:Enabled") == 1)
            {
                DateTime dBegintime;
                DateTime dEndtime;

                var beginTime = config.GetValue("StopReg:BeginTime");
                var endTime = config.GetValue("StopReg:EndTime");
                if (DateTime.TryParseExact(beginTime, "HH:mm", null, System.Globalization.DateTimeStyles.None, out dBegintime) &&
                    DateTime.TryParseExact(endTime, "HH:mm", null, System.Globalization.DateTimeStyles.None, out dEndtime))
                {
                    var realbegin = DateTimeCore.Today.AddHours(dBegintime.Hour).AddMinutes(dBegintime.Minute);
                    var realend = DateTimeCore.Today.AddHours(dEndtime.Hour).AddMinutes(dEndtime.Minute);
                    if (DateTimeCore.Now > realbegin && DateTimeCore.Now < realend)
                    {
                        ShowAlert(false, $"{ChoiceModel.Business}", $"{beginTime}至{endTime}期间非急诊停止挂号\r\n如需帮助请咨询服务台。");
                        return;
                    }
                }
            }

            if (RegTypesModel.SelectRegType.RegType == RegType.急诊门诊)
            {
                ShowConfirm("温馨提示", "急诊挂号前请至分诊台分诊，\r\n以便明确就诊科室，避免延误就诊时机。\r\n是否已分诊？", cp =>
                {
                    if (!cp)
                    {
                        Navigate(A.Home);
                        return;
                    }
                },30, ConfirmExModel.Build("是", "否", true));
            }

            base.Confirm(i);
        }
    }
}

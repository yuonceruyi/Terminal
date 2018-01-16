using Prism.Commands;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using YuanTu.Consts;
using YuanTu.Consts.Dtos.Register;
using YuanTu.Consts.Enums;
using YuanTu.Consts.Gateway;
using YuanTu.Consts.Models;
using YuanTu.Consts.Services;
using YuanTu.Consts.Sounds;
using YuanTu.Core.Extension;

namespace YuanTu.QDKouQiangYY.Component.Register.ViewModels
{
    public class RegTypesViewModel : YuanTu.Default.Component.Register.ViewModels.RegTypesViewModel
    {
        public override void OnEntered(NavigationContext navigationContext)
        {
            ChangeNavigationContent("");

            var config = GetInstance<IConfigurationManager>();
            var no急诊门诊 = false;
            if (ChoiceModel.Business == Business.挂号 &&
                config.GetValueInt("StopEmergency:Enabled") == 1)
            {
                DateTime dBegintime;
                DateTime dEndtime;
                var beginTime = config.GetValue("StopEmergency:BeginTime");
                var endTime = config.GetValue("StopEmergency:EndTime");
                if (DateTime.TryParseExact(beginTime, "HH:mm", null, System.Globalization.DateTimeStyles.None, out dBegintime) &&
                    DateTime.TryParseExact(endTime, "HH:mm", null, System.Globalization.DateTimeStyles.None, out dEndtime))
                {
                    var realbegin = DateTimeCore.Today.AddHours(dBegintime.Hour).AddMinutes(dBegintime.Minute);
                    var realend = DateTimeCore.Today.AddHours(dEndtime.Hour).AddMinutes(dEndtime.Minute);

                    if (realbegin < realend)
                    {
                        if (DateTimeCore.Now > realbegin && DateTimeCore.Now < realend)
                        {
                            no急诊门诊 = true;
                        }
                    }
                    else
                    {
                        if (DateTimeCore.Now > realbegin || DateTimeCore.Now < realend)
                        {
                            no急诊门诊 = true;
                        }
                    }
                }
            }

            var list = RegTypeDto.GetInfoTypes(
                config,
                ResourceEngine,
                "RegType",
                new DelegateCommand<Info>(Confirm),
                p =>
                {
                    if (p.RegType == RegType.急诊门诊 && no急诊门诊)
                        p.Visabled = false;
                });
            Data = new ObservableCollection<InfoType>(list);

            PlaySound(ChoiceModel.Business == Business.挂号 ? SoundMapping.选择挂号科室类别 : SoundMapping.选择预约科室类别);
        }

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
                    if (realbegin < realend)
                    {
                        if (DateTimeCore.Now > realbegin && DateTimeCore.Now < realend)
                        {
                            ShowAlert(false, $"{ChoiceModel.Business}", $"{beginTime}至{endTime}期间非急诊停止挂号\r\n如需帮助请咨询服务台。");
                            return;
                        }
                    }
                    else
                    {
                        if (DateTimeCore.Now > realbegin || DateTimeCore.Now < realend)
                        {
                            ShowAlert(false, $"{ChoiceModel.Business}", $"{beginTime}至{endTime}期间非急诊停止挂号\r\n如需帮助请咨询服务台。");
                            return;
                        }
                    }
                }
            }
            base.Confirm(i);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Practices.Unity;
using Prism.Commands;
using Prism.Regions;
using YuanTu.Consts;
using YuanTu.Consts.Dtos.Register;
using YuanTu.Consts.Enums;
using YuanTu.Consts.FrameworkBase;
using YuanTu.Consts.Gateway;
using YuanTu.Consts.Models;
using YuanTu.Consts.Models.Register;
using YuanTu.Consts.Services;
using YuanTu.Consts.Sounds;
using YuanTu.Core.Extension;

namespace YuanTu.YiWuFuBao.Component.Register.ViewModels
{
    public class RegTypesViewModel:YuanTu.Default.Component.Register.ViewModels.RegTypesViewModel
    {
        private readonly RegType[] _noAmpmRegisterTypes = new[] {RegType.夜间特需_义乌妇保, RegType.急诊门诊 };
      
        public override void OnEntered(NavigationContext navigationContext)
        {
            ChangeNavigationContent("");
            var list = RegTypeDto.GetInfoTypes(
                GetInstance<IConfigurationManager>(),
                ResourceEngine,
                "RegType",
                new DelegateCommand<Info>(Confirm), dto =>
                {
                    if (_noAmpmRegisterTypes.Contains(dto.RegType)&&dto.Visabled)
                    {
                        if (ChoiceModel.Business != Business.挂号)
                        {
                            dto.Visabled = false;
                        }
                    }
                   
                });
            Data = new ObservableCollection<InfoType>(list);

            PlaySound(ChoiceModel.Business == Business.挂号 ? SoundMapping.选择挂号科室类别 : SoundMapping.选择预约科室类别);
        }

        protected override void Confirm(Info i)
        {
            RegTypesModel.SelectRegType = (RegTypeDto) i.Tag;
            RegTypesModel.SelectRegTypeName = i.Title;

            var judgeRet = JudgeRule(RegTypesModel.SelectRegType);
            if (!judgeRet.IsSuccess)
            {
                ShowAlert(false,"挂号限制",judgeRet.Message);
                return;
            }

            ChangeNavigationContent(i.Title);
            if (_noAmpmRegisterTypes.Contains(RegTypesModel.SelectRegType.RegType))
            {
                var model = GetInstance<IRegDateModel>();
                model.AmPm = AmPmSession.全天;
                GetDepts();
            }
            else
            {
                Next();

            }

        }

        protected virtual Result JudgeRule(RegTypeDto type)
        {
            /*
             联众-陈兴富(52097027)  11:16:05
             急诊儿科 下午4:45-早上 7点 
             特需下午4:45---晚上11点   
             已确认 
             */
            /*
            联众-陈兴富(52097027) 2017-10-9 17:28:19
                @远图研发部-叶飞 急诊和特需时间也调整一下，调整规则如下： 急诊下午16:15到第二早上7点，特需17:30到21:00   

            义乌妇保-许祖振 2017-11-27 16:36:50
                @远图研发部-叶飞    儿科特需号  挂号时间段更改一下   改成17：30-20：30
            */
            if (type.RegType==RegType.夜间特需_义乌妇保)
            {
                const string start = "17:30";
                const string end = "20:30";
                if (!JudgeTimeForNow(start,end))
                {
                    return Result.Fail($"只能在{start}~{end}点之间挂夜间特需的号");
                }
            }
            if (type.RegType == RegType.急诊门诊)
            {
                const string start = "16:15";
                const string end = "07:00";
                if (!JudgeTimeForNow(start, end))
                {
                    return Result.Fail($"只能在{start}~{end}点之间挂急诊的号");
                }
            }

                return Result.Success();
        }

     

        private bool JudgeTimeForNow(string startTime, string endTime)
        {
            var startT = DateTime.ParseExact(startTime, "HH:mm", null);
            var endT = DateTime.ParseExact(endTime, "HH:mm", null);

            var now = DateTimeCore.Now;
            var nowStartT = now.Date.Add(startT - startT.Date);
            var nowEndT = now.Date.Add(endT - endT.Date);

            if (nowStartT< nowEndT)//同一天
            {
                return now>nowStartT&&now<nowEndT;
            }

            return now < nowEndT || now > nowStartT;

        }

        private void GetDepts()
        {
            DoCommand(lp =>
            {
                var isRegister = ChoiceModel.Business == Business.挂号;
                if (isRegister)
                {
                    RegDateModel.RegDate = DateTimeCore.Today.ToString("yyyy-MM-dd");
                }
                lp.ChangeText("正在查询排班科室，请稍候...");
                DeptartmentModel.排班科室信息查询 = new req排班科室信息查询
                {
                    regMode = ChoiceModel.Business == Business.挂号 ? "2" : "1",
                    regType = ((int)RegTypesModel.SelectRegType.RegType).ToString(),

                    startDate = RegDateModel.RegDate,
                    endDate = RegDateModel.RegDate,
                };
                DeptartmentModel.Res排班科室信息查询 = DataHandlerEx.排班科室信息查询(DeptartmentModel.排班科室信息查询);
                if (DeptartmentModel.Res排班科室信息查询?.success ?? false)
                {
                    if (DeptartmentModel.Res排班科室信息查询?.data?.Count > 0)
                    {
                        Navigate(isRegister? A.XC.Dept:A.YY.Dept);
                    }
                    else
                    {
                        ShowAlert(false, "科室列表查询", "没有获得科室信息(列表为空)");
                    }
                }
                else
                {
                    ShowAlert(false, "科室列表查询", "没有获得科室信息", debugInfo: DeptartmentModel.Res排班科室信息查询?.msg);
                }
            });
        }
    }
}

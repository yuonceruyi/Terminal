using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prism.Regions;
using YuanTu.Consts;
using YuanTu.Consts.Enums;
using YuanTu.Consts.Gateway;
using YuanTu.Consts.Models;
using YuanTu.Core.Extension;
using YuanTu.QDKouQiangYY.Component.Register.Services;
using Prism.Commands;
using System.Collections.ObjectModel;
using YuanTu.Consts.Services;
using YuanTu.Consts.Sounds;

namespace YuanTu.QDKouQiangYY.Component.Register.ViewModels
{
    public class DeptsViewModel : YuanTu.Default.Component.Register.ViewModels.DeptsViewModel
    {

        #region Overrides of ApptRecordViewModel
        public override void OnEntered(NavigationContext navigationContext)
        {
            ChangeNavigationContent("");
            var confirmCommand = new DelegateCommand<Info>(Confirm);

            var config = GetInstance<IConfigurationManager>();
            var list = DeptartmentModel.Res排班科室信息查询.data;
            if (ChoiceModel.Business == Business.挂号 && 
                config.GetValueInt("RegDept:Enabled") == 1)
            {
                var depts = config.GetValue("RegDept:Dept").Split('|');
                list = list.Where(p => depts.Contains(p.deptCode)).ToList();
            }
            
            var list2 = list.Select(p => new Info
            {
                Title = deptNameFormat(p.deptName),
                Tag = p,
                ConfirmCommand = confirmCommand
            });
            Data = new ObservableCollection<Info>(list2);
            
            
            PlaySound(ChoiceModel.Business == Business.挂号 ? SoundMapping.选择挂号科室 : SoundMapping.选择预约科室);
        }

        protected override void Confirm(Info i)
        {
            DeptartmentModel.所选科室 = i.Tag.As<排班科室信息>();
            (ScheduleModel as Models.ScheduleModel).IsDoctor = false;
            (ScheduleModel as Models.ScheduleModel).DoctorCode = "";
            DoCommand(lp =>
            {
                lp.ChangeText("正在查询医生信息，请稍候...");

                ScheduleModel.排班信息查询 = new req排班信息查询
                {
                    regMode = ChoiceModel.Business == Business.挂号 ? "2" : "1",
                    regType = ((int)RegTypesModel.SelectRegType.RegType).ToString(),
                    deptCode = DeptartmentModel.所选科室.deptCode,
                    parentDeptCode = DeptartmentModel.所选科室.parentDeptCode,
                    startDate =
                        ChoiceModel.Business == Business.挂号
                            ? DateTimeCore.Today.ToString("yyyy-MM-dd")
                            : RegDateModel.RegDate,
                                    endDate =
                        ChoiceModel.Business == Business.挂号
                            ? DateTimeCore.Today.ToString("yyyy-MM-dd")
                            : RegDateModel.RegDate
                };
                ScheduleModel.Res排班信息查询 = DataHandlerEx.排班信息查询(ScheduleModel.排班信息查询);
                if (ScheduleModel.Res排班信息查询?.success ?? false)
                {
                    if (ScheduleModel.Res排班信息查询?.data?.Count > 0)
                    {

                        //只有科室排班
                        if (ScheduleModel.Res排班信息查询?.data.Where(p => p.doctCode.IsNullOrWhiteSpace()).Count(p =>p.deptCode !=null) ==
                            ScheduleModel.Res排班信息查询?.data?.Count)
                        {
                            ChangeNavigationContent(i.Title);
                            Navigate(ChoiceModel.Business == Business.挂号 ? A.XC.Schedule : A.YY.Schedule);
                        }
                        else
                        {

                            #region 医生简介信息

                            var isExist =
                                ScheduleModel.Res排班信息查询?.data.Where(p => !p.doctCode.IsNullOrWhiteSpace())
                                    .Select(p => p.doctCode)
                                    .All(p => DocExtendDic.DocDictionary.ContainsKey(p));
                            if ((bool) !isExist)
                            {
                                DoctorModel.查询所有医生信息 = new req查询所有医生信息
                                {
                                    deptCode = DeptartmentModel.所选科室.deptCode
                                };
                                DoctorModel.Res查询所有医生信息 = DataHandlerEx.查询所有医生信息(DoctorModel.查询所有医生信息);
                                if (DoctorModel.Res查询所有医生信息?.success ?? false)
                                {
                                    if (DoctorModel.Res查询所有医生信息?.data?.Count > 0)
                                    {
                                        DoctorModel.Res查询所有医生信息?.data.ForEach(p =>
                                        {
                                            if (!DocExtendDic.DocDictionary.ContainsKey(p.doctCode))
                                            {
                                                DocExtendDic.DocDictionary.Add(p.doctCode, p);
                                            }
                                        });
                                    }
                                }
                            }

                            //过滤掉当日挂号、非当前午别、挂到医生  的数据后，如没有则直接提示无排班
                            var curAmpm = DateTimeCore.Now.Hour >= 12 ? "下午" : "上午";
                            
                            if(ChoiceModel.Business == Business.挂号 && ScheduleModel.Res排班信息查询.data.Where(p => p.medAmPm.SafeToAmPm() == curAmpm).ToList().Count == 0)
                            {
                                    ShowAlert(false, "排班列表查询", "没有获得排班信息", debugInfo: ScheduleModel.Res排班信息查询?.msg);
                            }
                            else
                            {
                                ChangeNavigationContent(i.Title);
                                Navigate(ChoiceModel.Business == Business.挂号 ? A.XC.Doctor : A.YY.Doctor);
                            }

                            #endregion
                        }

                    }
                    else
                    {
                        ShowAlert(false, "排班列表查询", "没有获得排班信息(列表为空)");
                    }
                }
                else
                {
                    ShowAlert(false, "排班列表查询", "没有获得排班信息", debugInfo: ScheduleModel.Res排班信息查询?.msg);
                }


            });
        }
        #endregion
        protected virtual string deptNameFormat(string deptName)
        {
            if (deptName.Contains("("))
            {
                var index = deptName.IndexOf('(');
                return deptName.Substring(0,index);
            }

            return deptName;
        }
    }

}

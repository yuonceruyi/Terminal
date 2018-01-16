using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Practices.Unity;
using Prism.Regions;
using YuanTu.Consts;
using YuanTu.Consts.Dtos.Register;
using YuanTu.Consts.Enums;
using YuanTu.Consts.FrameworkBase;
using YuanTu.Consts.Gateway;
using YuanTu.Consts.Models;
using YuanTu.Consts.Models.Auth;
using YuanTu.Consts.Models.Register;
using YuanTu.Core.Extension;
using YuanTu.ShengZhouHospital.Component.Register.Models;
using YuanTu.ShengZhouHospital.Extension;

namespace YuanTu.ShengZhouHospital.Component.Register.ViewModels
{
    public class RegAmPmViewModel : YuanTu.Default.Component.Register.ViewModels.RegAmPmViewModel
    {

        [Dependency]
        public IRegTypesModel RegTypesModel { get; set; }

        [Dependency]
        public IChoiceModel ChoiceModel { get; set; }

        [Dependency]
        public IDeptartmentModel DeptartmentModel { get; set; }

        [Dependency]
        public IPatientModel PatientModel { get; set; }


        protected override void Confirm(Info i)
        {

            RegDateModel.AmPm = i.Title.SafeToAmPmEnum();

            string regType;
            if (RegTypesModel.SelectRegType.RegType == RegType.专科普通)
            {
                regType = "03";
            }
            else if (RegTypesModel.SelectRegType.RegType == RegType.专科专家)
            {
                regType = "06";
            }
            else if (RegTypesModel.SelectRegType.RegType == RegType.夜间特需)
            {
                regType = "05";
            }
            else
            {
                regType = "0" + ((int)RegTypesModel.SelectRegType.RegType);
            }
            DoCommand(lp =>
            {
                lp.ChangeText("正在查询排班科室，请稍候...");
                DeptartmentModel.排班科室信息查询 = new req排班科室信息查询
                {
                    regMode = ChoiceModel.Business == Business.挂号 ? "01" : "02",
                    regType = regType,
                    startDate =
                        ChoiceModel.Business == Business.挂号
                            ? DateTimeCore.Today.ToString("yyyy-MM-dd")
                            : RegDateModel.RegDate,
                    endDate =
                        ChoiceModel.Business == Business.挂号
                            ? DateTimeCore.Today.ToString("yyyy-MM-dd")
                            : RegDateModel.RegDate
                };
                DeptartmentModel.Res排班科室信息查询 = DataHandlerEx.排班科室信息查询(DeptartmentModel.排班科室信息查询);
                if (DeptartmentModel.Res排班科室信息查询?.success ?? false)
                {
                    if (DeptartmentModel.Res排班科室信息查询?.data?.Count > 0)
                    {
                        var data = new List<排班科室信息>();
                        DeptartmentModel.Res排班科室信息查询?.data.ForEach(p =>
                        {
                            if (((int)RegDateModel.AmPm).ToString() == p.parentDeptCode &&
                                data.FirstOrDefault(t => t.deptName == p.deptName) == null)
                            {
                                data.Add(p);
                            }
                        });
                        if (!data.Any())
                        {
                            ShowAlert(false, "科室列表查询", "该时段科室信息已经为空");
                            return;
                            ;
                        }
                        DeptartmentModel.Res排班科室信息查询.data = data;
                        ChangeNavigationContent(i.Title);
                        Next();
                    }
                    else
                    {
                        ShowAlert(false, "科室列表查询", "该时段科室信息已经为空");
                    }
                }
                else
                {
                    ShowAlert(false, "科室列表查询", "该时段科室信息已经为空", debugInfo: DeptartmentModel.Res排班科室信息查询?.msg);
                }
            });
        }
    }
}

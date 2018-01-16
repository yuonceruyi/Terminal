using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Practices.Unity;
using Prism.Regions;
using YuanTu.Consts.Enums;
using YuanTu.Consts.Gateway;
using YuanTu.Consts.Models.Register;

namespace YuanTu.ShengZhouZhongYiHospital.Component.InfoQuery.ViewModels
{
    public class InputViewModel:YuanTu.Default.Component.InfoQuery.ViewModels.InputViewModel
    {
        [Dependency]
        public IDoctorModel DoctorModel { get; set; }

        protected override void Confirm()
        {
            if (string.IsNullOrWhiteSpace(PinCode))
            {
                ShowAlert(false, "温馨提示", "请输入首字母");
                return;
            }
            ChangeNavigationContent(PinCode);
            DoCommand(ctx =>
            {
                ctx.ChangeText("正在查询，请稍候...");
                if (QueryChoiceModel.InfoQueryType == InfoQueryTypeEnum.药品查询)
                {
                    MedicineModel.Req药品项目查询 = new req药品项目查询
                    {
                        pinyinCode = PinCode.ToUpper()
                    };
                    MedicineModel.Res药品项目查询 = DataHandlerEx.药品项目查询(MedicineModel.Req药品项目查询);
                    if (MedicineModel.Res药品项目查询.success)
                    {
                        if (MedicineModel.Res药品项目查询.data == null || MedicineModel.Res药品项目查询.data.Count == 0)
                        {
                            ShowAlert(false, "药品信息查询", "未查询到相关药品的信息(列表为空)");
                            return;
                        }
                        Next();
                    }
                    else
                    {
                        ShowAlert(false, "药品信息查询", "未查询到相关药品的信息", debugInfo: MedicineModel.Res药品项目查询.msg);
                    }
                }
                else if (QueryChoiceModel.InfoQueryType == InfoQueryTypeEnum.项目查询)
                {
                    ChargeItemsModel.Req收费项目查询 = new req收费项目查询
                    {
                        pinyinCode = PinCode.ToUpper()
                    };
                    ChargeItemsModel.Res收费项目查询 = DataHandlerEx.收费项目查询(ChargeItemsModel.Req收费项目查询);
                    if (ChargeItemsModel.Res收费项目查询.success)
                    {
                        if (ChargeItemsModel.Res收费项目查询.data == null || ChargeItemsModel.Res收费项目查询.data.Count == 0)
                        {
                            ShowAlert(false, "诊疗项查询", "未查询到相关诊疗项的信息(列表为空)");
                            return;
                        }
                        Next();
                    }
                    else
                    {
                        ShowAlert(false, "诊疗项查询", "未查询到相关诊疗项的信息", debugInfo: ChargeItemsModel.Res收费项目查询.msg);
                    }
                }
                else if(QueryChoiceModel.InfoQueryType == InfoQueryTypeEnum.科室医生查询)
                {
                    DoctorModel.医生信息查询= new req医生信息查询
                    {
                        doctName = PinCode.ToUpper()
                    };
                    DoctorModel.Res医生信息查询 = DataHandlerEx.医生信息查询(DoctorModel.医生信息查询);
                    if (DoctorModel.Res医生信息查询.success)
                    {
                        if (DoctorModel.Res医生信息查询.data == null || DoctorModel.Res医生信息查询.data.Count == 0)
                        {
                            ShowAlert(false, "医生信息查询", "未查询到相关医生的信息(列表为空)");
                            return;
                        }
                        Next();
                    }
                    else
                    {
                        ShowAlert(false, "医生信息查询", "未查询到相关医生的信息", debugInfo: DoctorModel.Res医生信息查询.msg);
                    }
                }
            });
        }

        public override void OnEntered(NavigationContext navigationContext)
        {
            ChangeNavigationContent(null);
            HideNavigating = true;
            PinCode = null;
            switch (QueryChoiceModel.InfoQueryType)
            {
                case InfoQueryTypeEnum.药品查询:
                    QueryType = "药品首字母：";
                    Hint = "药品查询";
                    QueryTips = "请输入您要查询的药品信息的首字母";
                    break;
                case InfoQueryTypeEnum.项目查询:
                    QueryType = "诊疗项首字母：";
                    Hint = "诊疗项查询";
                    QueryTips = "请在下方输入诊疗项首字母进行查询";
                    break;
                case InfoQueryTypeEnum.科室医生查询:
                    QueryType = "医生字母：";
                    Hint = "医生信息查询";
                    QueryTips = "请在下方输入医生首字母进行查询";
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prism.Regions;
using YuanTu.Consts.Enums;
using YuanTu.Consts.Gateway;

namespace YuanTu.YanTaiYDYY.Component.InfoQuery.ViewModels
{
    public class InputViewModel : YuanTu.Default.Component.InfoQuery.ViewModels.InputViewModel
    {
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
                        if (ChargeItemsModel.Res收费项目查询.data == null 
                            || ChargeItemsModel.Res收费项目查询.data.Count == 0 
                            ||ChargeItemsModel.Res收费项目查询.data.Where(p => p.type != "材料费").ToList().Count == 0)
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
                else if (QueryChoiceModel.InfoQueryType == InfoQueryTypeEnum.材料费查询)
                {
                    ChargeItemsModel.Req收费项目查询 = new req收费项目查询
                    {
                        pinyinCode = PinCode.ToUpper()
                    };
                    ChargeItemsModel.Res收费项目查询 = DataHandlerEx.收费项目查询(ChargeItemsModel.Req收费项目查询);
                    if (ChargeItemsModel.Res收费项目查询.success)
                    {
                        if (ChargeItemsModel.Res收费项目查询.data == null
                            || ChargeItemsModel.Res收费项目查询.data.Count == 0
                            || ChargeItemsModel.Res收费项目查询.data.Where(p => p.type == "材料费").ToList().Count == 0)
                        {
                            ShowAlert(false, "材料费查询", "未查询到相关材料费的信息(列表为空)");
                            return;
                        }
                        Next();
                    }
                    else
                    {
                        ShowAlert(false, "材料费查询", "未查询到相关材料费的信息", debugInfo: ChargeItemsModel.Res收费项目查询.msg);
                    }
                }
            });
        }

        public override void OnEntered(NavigationContext navigationContext)
        {
            ChangeNavigationContent(null);
            //HideNavigating = true;
            PinCode = null;
            switch (QueryChoiceModel.InfoQueryType)
            {
                case InfoQueryTypeEnum.药品查询:
                    QueryType = "药品首字母：";
                    Hint = "药品查询" ;
                    QueryTips = "请输入您要查询的药品信息的首字母";
                    break;
                case InfoQueryTypeEnum.项目查询:
                    QueryType = "诊疗项首字母：";
                    Hint = "诊疗项查询";
                    QueryTips = "请在下方输入诊疗项首字母进行查询";
                    break;
                case InfoQueryTypeEnum.材料费查询:
                    QueryType = "材料费首字母：";
                    Hint = "材料费查询";
                    QueryTips = "请输入您要查询的材料费信息的首字母";
                    break;
                default:
                    break;
            }
        }
    }
}

using YuanTu.Consts.Gateway;

namespace YuanTu.NanYangFirstPeopleHospital.Component.InfoQuery.ViewModels
{
    public class InputViewModel:Default.Component.InfoQuery.ViewModels.InputViewModel
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
                if (QueryChoiceModel.InfoQueryType == Consts.Enums.InfoQueryTypeEnum.药品查询)
                {
                    MedicineModel.Req药品项目查询 = new req药品项目查询
                    {
                        pinyinCode = PinCode.ToLower()
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
                else if (QueryChoiceModel.InfoQueryType == Consts.Enums.InfoQueryTypeEnum.项目查询)
                {
                    ChargeItemsModel.Req收费项目查询 = new req收费项目查询
                    {
                        pinyinCode = PinCode.ToLower()
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


            });
        }
    }
}

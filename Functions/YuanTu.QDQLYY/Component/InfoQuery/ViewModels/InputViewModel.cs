using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YuanTu.Consts.Gateway;
using Microsoft.Practices.Unity;
using Prism.Regions;
using YuanTu.Consts;
using YuanTu.Consts.Models.InfoQuery;
using YuanTu.QDQLYY.Current.Models;

namespace YuanTu.QDQLYY.Component.InfoQuery.ViewModels
{
    public class InputViewModel:YuanTu.Default.Component.InfoQuery.ViewModels.InputViewModel
    {
        [Dependency]
        public IQualificationModel QualificationModel { get; set; }

        public override void OnEntered(NavigationContext navigationContext)
        {
            ChangeNavigationContent(null);
            PinCode = null;
            switch (QueryChoiceModel.InfoQueryType)
            {
                case Consts.Enums.InfoQueryTypeEnum.药品查询:
                    QueryType = "药品首字母：";
                    Hint = "药品查询";
                    QueryTips = "请输入您要查询的药品信息的首字母";
                    break;
                case Consts.Enums.InfoQueryTypeEnum.项目查询:
                    QueryType = "诊疗项首字母：";
                    Hint = "诊疗项查询";
                    QueryTips = "请在下方输入诊疗项首字母进行查询";
                    break;
                case Consts.Enums.InfoQueryTypeEnum.执业资格查询:
                    QueryType = "医生或护士姓名拼音首字母：";
                    Hint = "执业资格查询";
                    QueryTips = "请在下方输入医生或护士姓名拼音首字母进行查询";
                    break;
            }
        }

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
                else if (QueryChoiceModel.InfoQueryType == Consts.Enums.InfoQueryTypeEnum.项目查询)
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
                else if (QueryChoiceModel.InfoQueryType == Consts.Enums.InfoQueryTypeEnum.执业资格查询)
                {
                    QualificationModel.Res获取执业资格信息 = Current.Services.QualificationService.执业资格信息列表;
                    QualificationModel.当前执业资格信息 = QualificationModel.Res获取执业资格信息.data.Where(p => p.SpellCode.Contains(PinCode.ToUpper())).ToList();

                    if (QualificationModel.当前执业资格信息 == null || QualificationModel.当前执业资格信息.Count == 0)
                    {
                        ShowAlert(false, "执业资格查询", "未查询到执业资格信息");
                        return;
                    }
                    Navigate(AInner.ZYZG.Qualification);
                }
            });

        }
    }
}

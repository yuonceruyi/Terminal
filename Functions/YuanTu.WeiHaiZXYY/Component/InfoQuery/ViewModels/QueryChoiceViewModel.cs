using YuanTu.Consts;
using YuanTu.Consts.Enums;
using YuanTu.Consts.Models;
using YuanTu.Consts.Models.InfoQuery;
using YuanTu.Core.Navigating;

namespace YuanTu.WeiHaiZXYY.Component.InfoQuery.ViewModels
{
    public class QueryChoiceViewModel : Default.Component.InfoQuery.ViewModels.QueryChoiceViewModel
    {
        protected override void OnButtonClick(InfoQueryButtonInfo obj)
        {
            var engine = NavigationEngine;
            var choiceModel = GetInstance<IChoiceModel>();
            var queryChoiceModel = GetInstance<IQueryChoiceModel>();
            choiceModel.HasAuthFlow = true;
            //手动清空导航栏
            GetInstance<INavigationModel>().Items.Clear();
            queryChoiceModel.InfoQueryType = obj.InfoQueryType;
            switch (obj.InfoQueryType)
            {
                case InfoQueryTypeEnum.药品查询:
                    choiceModel.HasAuthFlow = false;
                    engine.JumpAfterFlow(null, CreateJump,
                        new FormContext(A.MedicineQuery, A.YP.Query), obj.Name);
                    break;

                case InfoQueryTypeEnum.项目查询:
                    choiceModel.HasAuthFlow = false;
                    engine.JumpAfterFlow(null,
                        CreateJump,
                        new FormContext(A.ChargeItemsQuery, A.XM.Query), obj.Name);
                    break;

                case InfoQueryTypeEnum.已缴费明细:
                    engine.JumpAfterFlow(new FormContext(A.ChaKa_Context, A.CK.Card),
                        CreateJump,
                        new FormContext(A.PayCostQuery, A.JFJL.Date), obj.Name);
                    break;

                case InfoQueryTypeEnum.住院一日清单:
                    //choiceModel.HasAuthFlow = false;
                    engine.JumpAfterFlow(new FormContext(A.ZhuYuan_Context, A.ZY.InPatientNo),
                        CreateJump,
                        new FormContext(A.InDayDetailList_Context, A.ZYYRQD.Date), obj.Name);
                    break;

                case InfoQueryTypeEnum.检验结果:
                    engine.JumpAfterFlow(new FormContext(A.ChaKa_Context, A.CK.Card),
                        CreateJump,
                        new FormContext(A.DiagReportQuery, A.JYJL.DiagReport), obj.Name);
                    break;

                case InfoQueryTypeEnum.影像结果:
                    engine.JumpAfterFlow(new FormContext(A.ChaKa_Context, A.CK.Card),
                        CreateJump,
                        new FormContext(A.PacsReportQuery, A.YXBG.Date), obj.Name);
                    break;

                default:
                    ShowAlert(false, "友好提示", "业务未实现");
                    break;
            }
        }
    }
}
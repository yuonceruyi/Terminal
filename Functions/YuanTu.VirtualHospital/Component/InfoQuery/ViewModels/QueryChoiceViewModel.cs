using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YuanTu.Consts;
using YuanTu.Consts.Enums;
using YuanTu.Consts.FrameworkBase;
using YuanTu.Consts.Gateway;
using YuanTu.Consts.Models;
using YuanTu.Consts.Models.InfoQuery;
using YuanTu.Core.Navigating;

namespace YuanTu.VirtualHospital.Component.InfoQuery.ViewModels
{
    public class QueryChoiceViewModel: YuanTu.Default.Component.InfoQuery.ViewModels.QueryChoiceViewModel
    {
        protected override void OnButtonClick(InfoQueryButtonInfo obj)
        {
            if (obj.InfoQueryType != InfoQueryTypeEnum.住院一日清单)
            {
                base.OnButtonClick(obj);
                return;
            }
            var choiceModel = GetInstance<IChoiceModel>();
            var queryChoiceModel = GetInstance<IQueryChoiceModel>();
            choiceModel.HasAuthFlow = true;
            //手动清空导航栏
            GetInstance<INavigationModel>().Items.Clear();
            queryChoiceModel.InfoQueryType = obj.InfoQueryType;
            choiceModel.AuthContext = A.ZhuYuan_Context;
            NavigationEngine.JumpAfterFlow(new FormContext(A.ChaKa_Context, A.CK.Choice),
                InOnedayList,
                new FormContext(A.InDayDetailList_Context, A.ZYYRQD.DailyDetail), obj.Name);
        }

        public Task<Result<FormContext>> InOnedayList()
        {
            return DoCommand(lp =>
            {
                var InDailyDetailModel = GetInstance<IInDailyDetailModel>();
                var req = new req住院患者费用明细查询
                {
                    patientId = "",
                    cardNo = "",
                    startDate = InDailyDetailModel.StartDate.ToString("yyyy-MM-dd"),
                    endDate = InDailyDetailModel.EndDate.ToString("yyyy-MM-dd")
                };

                InDailyDetailModel.Res住院患者费用明细查询 = DataHandlerEx.住院患者费用明细查询(req);
                if (InDailyDetailModel.Res住院患者费用明细查询?.success ?? false)
                    if (InDailyDetailModel.Res住院患者费用明细查询?.data?.Count > 0)
                    {
                        // ChangeNavigationContent($"{DateTimeStart.ToString("yyyy-MM-dd")}至\n{InDailyDetailModel.EndDate.ToString("yyyy-MM-dd")}");
                        Next();
                        return Result<FormContext>.Success(default(FormContext));
                    }
                ShowAlert(false, "住院患者费用明细查询", "没有获得住院患者费用明细");
                return Result<FormContext>.Fail("");
            });
        }

    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prism.Regions;
using YuanTu.Consts;
using YuanTu.Consts.Enums;
using YuanTu.Consts.FrameworkBase;
using YuanTu.Consts.Models;
using YuanTu.Consts.Models.InfoQuery;
using YuanTu.Consts.Services;
using YuanTu.Core.Navigating;
using YuanTu.YanTaiYDYY.Component.InfoQuery.Service;

namespace YuanTu.YanTaiYDYY.Component.InfoQuery.ViewModels
{
    public class QueryChoiceViewModel: YuanTu.Default.Component.InfoQuery.ViewModels.QueryChoiceViewModel
    {
        protected override void OnButtonClick(InfoQueryButtonInfo obj)
        {
            var engine = GetInstance<NavigationEngine>();
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
                    engine.JumpAfterFlow(null,
                        CreateJump,
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
                    choiceModel.AuthContext = A.ZhuYuan_Context;
                    engine.JumpAfterFlow(new FormContext(A.ZhuYuan_Context, A.ZY.InPatientNo),
                        CreateJump,
                        new FormContext(A.InDayDetailList_Context, A.ZYYRQD.Date), obj.Name);
                    break;

                case InfoQueryTypeEnum.检验结果:
                    engine.JumpAfterFlow(new FormContext(A.ChaKa_Context, A.CK.Card),
                        CreateJump,
                        new FormContext(A.DiagReportQuery, A.JYJL.Date), obj.Name);
                    break;

                case InfoQueryTypeEnum.影像结果:
                    engine.JumpAfterFlow(new FormContext(A.ChaKa_Context, A.CK.Card),
                        CreateJump,
                        new FormContext(A.PacsReportQuery, A.YXBG.Date), obj.Name);
                    break;
                case InfoQueryTypeEnum.交易记录查询:
                    engine.JumpAfterFlow(new FormContext(A.ChaKa_Context, A.CK.Card),
                        CreateJump,
                        new FormContext(A.ReChargeQuery, A.CZJL.Date), obj.Name);
                    break;
                case InfoQueryTypeEnum.材料费查询:
                    choiceModel.HasAuthFlow = false;
                    engine.JumpAfterFlow(null,
                        CreateJump,
                        new FormContext(InnerA.MaterialItemsQuery, InnerA.CLF.Query), obj.Name);
                    break;
                case InfoQueryTypeEnum.门诊排班查询:
                    choiceModel.HasAuthFlow = false;
                    var res = QueryScheduleJump();
                    if (res.Result.IsSuccess)
                    {
                        engine.JumpAfterFlow(null,
                            QueryScheduleJump,
                            new FormContext(InnerA.ScheduleQuery_Context, InnerA.MZPBCX.ScheduleQuery), obj.Name);
                    }
                    else
                    {
                        ShowAlert(false, "友好提示", res.Result.Message);
                    }
                    break;

                default:
                    ShowAlert(false, "友好提示", "业务未实现");
                    break;
            }
        }

        protected virtual Task<Result<FormContext>> QueryScheduleJump()
        {
            var config = GetInstance<IConfigurationManager>();
            var serverIp = config.GetValue("ImagePath:serverIp");
            var userid = config.GetValue("ImagePath:userid");
            var passWord = config.GetValue("ImagePath:passWord");
            var path = config.GetValue("ImagePath:path");
            var fileType = config.GetValue("ImagePath:fileType");

            var fileList = ScheduleService.GetFTPList(serverIp, userid, passWord, path, fileType);

            if (fileList != null && fileList.Count > 0)
            {
                ScheduleFile.ScheduleFileList.Clear();
                ScheduleFile.ScheduleFileList.AddRange(fileList);

                return Task.Run(() => Result<FormContext>.Success(default(FormContext)));
            }
            else
            {
                return Task.Run(() => Result<FormContext>.Fail("获取门诊排班列表失败"));
            }

        }
    }
}

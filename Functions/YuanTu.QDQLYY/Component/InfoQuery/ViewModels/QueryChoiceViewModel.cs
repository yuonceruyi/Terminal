﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YuanTu.Consts;
using YuanTu.Consts.Enums;
using YuanTu.Consts.FrameworkBase;
using YuanTu.Consts.Gateway;
using YuanTu.Consts.Models;
using YuanTu.Consts.Models.Auth;
using YuanTu.Consts.Models.BillPay;
using YuanTu.Consts.Models.InfoQuery;
using YuanTu.Core.Navigating;

namespace YuanTu.QDQLYY.Component.InfoQuery.ViewModels
{
    public class QueryChoiceViewModel : YuanTu.QDKouQiangYY.Component.InfoQuery.ViewModels.QueryChoiceViewModel
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
                       new FormContext (A.MedicineQuery,  A.YP.Query ), obj.Name);
                    break;
                case InfoQueryTypeEnum.项目查询:
                    choiceModel.HasAuthFlow = false;
                    engine.JumpAfterFlow(null,
                      CreateJump,
                      new FormContext (A.ChargeItemsQuery,  A.XM.Query ), obj.Name);
                    break;
                case InfoQueryTypeEnum.已缴费明细:
                    engine.JumpAfterFlow(new FormContext (A.ChaKa_Context,  A.CK.Choice ),
                       CreateJump,
                       new FormContext (A.PayCostQuery,  A.JFJL.Date ), obj.Name);
                    break;
                case InfoQueryTypeEnum.住院一日清单:
                    //choiceModel.HasAuthFlow = false;
                    engine.JumpAfterFlow(new FormContext (A.ZhuYuan_Context,  A.ZY.Choice ),
                       CreateJump,
                       new FormContext (A.InDayDetailList_Context,  A.ZYYRQD.Date ), obj.Name);
                    break;
                case InfoQueryTypeEnum.检验结果:
                    engine.JumpAfterFlow(new FormContext (A.ChaKa_Context,  A.CK.Choice ),
                       CreateJump,
                       new FormContext (A.DiagReportQuery,  A.JYJL.Date ), obj.Name);
                    break;
                case InfoQueryTypeEnum.住院押金查询:
                    engine.JumpAfterFlow(new FormContext (A.ZhuYuan_Context,  A.ZY.InPatientNo ),
                       CreateJump,
                       new FormContext (A.InPrePayRecordQuery_Context,  A.ZYYJ.Date ), obj.Name);
                    break;
                case InfoQueryTypeEnum.住院床位查询:
                    choiceModel.HasAuthFlow = false;
                    var res = InBedInfoQuery();
                    if (res.Result.IsSuccess)
                    {
                        engine.JumpAfterFlow(null,
                            CreateJump,
                            new FormContext (A.InBedInfoQuery_Context,  A.CW.InBedInfo ), obj.Name);
                    }
                    break;
                case InfoQueryTypeEnum.影像结果:
                    engine.JumpAfterFlow(new FormContext (A.ChaKa_Context,  A.CK.Choice ),
                       CreateJump,
                       new FormContext (A.PacsReportQuery,  A.YXBG.Date ), obj.Name);
                    break;
                case InfoQueryTypeEnum.执业资格查询:
                    choiceModel.HasAuthFlow = false;
                    engine.JumpAfterFlow(null,
                      CreateJump,
                      new FormContext (AInner.QualificationQuery_Context,  AInner.ZYZG.Query ), obj.Name);
                    break;
                case InfoQueryTypeEnum.科室医生查询:
                    choiceModel.HasAuthFlow = false;
                    engine.JumpAfterFlow(null,
                      CreateJump,
                      new FormContext(AInner.DeptInfo_Context, AInner.KSXX.DeptList), obj.Name);
                    break;
                default:
                    ShowAlert(false, "友好提示", "业务未实现");
                    break;
            }
        }
    }
}

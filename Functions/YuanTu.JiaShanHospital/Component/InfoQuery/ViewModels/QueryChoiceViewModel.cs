using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YuanTu.Consts;
using YuanTu.Consts.Enums;
using YuanTu.Consts.FrameworkBase;
using YuanTu.Consts.Models;
using YuanTu.Consts.Models.Auth;
using YuanTu.Consts.Models.InfoQuery;
using YuanTu.Consts.Services;
using YuanTu.Core.Log;
using YuanTu.Core.Navigating;

namespace YuanTu.JiaShanHospital.Component.InfoQuery.ViewModels
{
    public class QueryChoiceViewModel : YuanTu.Default.Component.InfoQuery.ViewModels.QueryChoiceViewModel
    {
        protected override void OnSetButton()
        {
            var resource = ResourceEngine;
            var config = GetInstance<IConfigurationManager>();

            var bts = new List<InfoQueryButtonInfo>();
            var k = Enum.GetValues(typeof(InfoQueryTypeEnum));
            foreach (InfoQueryTypeEnum buttonInfo in k)
            {
                if (buttonInfo == InfoQueryTypeEnum.检验结果)
                {
                    continue;
                }
                var v = config.GetValue($"InfoQuery:{buttonInfo}:Visabled");
                if (v == "1")
                    bts.Add(new InfoQueryButtonInfo
                    {
                        Name = config.GetValue($"InfoQuery:{buttonInfo}:Name") ?? "未定义",
                        InfoQueryType = buttonInfo,
                        Order = config.GetValueInt($"InfoQuery:{buttonInfo}:Order"),
                        IsEnabled = config.GetValueInt($"InfoQuery:{buttonInfo}:IsEnabled") == 1,
                        ImageSource = resource.GetImageResource(config.GetValue($"InfoQuery:{buttonInfo}:ImageName"))
                    });
            }
            Data = bts.OrderBy(p => p.Order).ToList();
        }

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
                    engine.JumpAfterFlow(new FormContext(A.ChaKa_Context, A.CK.Choice),
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

                //case InfoQueryTypeEnum.检验结果:
                //    engine.JumpAfterFlow(new FormContext(A.ChaKa_Context, A.CK.Card),
                //        CreateJump,
                //        new FormContext(A.DiagReportQuery, A.Home), obj.Name);
                //    break;

                case InfoQueryTypeEnum.影像结果:
                    engine.JumpAfterFlow(new FormContext(A.ChaKa_Context, A.CK.Choice),
                        CreateJump,
                        new FormContext(A.PacsReportQuery, A.YXBG.Date), obj.Name);
                    break;
                case InfoQueryTypeEnum.交易记录查询:
                    engine.JumpAfterFlow(new FormContext(A.ChaKa_Context, A.CK.Choice),
                        CreateJump,
                        new FormContext(A.ReChargeQuery, A.CZJL.Date), obj.Name);
                    break;

                default:
                    ShowAlert(false, "友好提示", "业务未实现");
                    break;
            }
        }
    }
}

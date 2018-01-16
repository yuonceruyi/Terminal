using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YuanTu.Consts.Enums;
using YuanTu.Consts.Gateway;
using YuanTu.Core.Extension;

namespace YuanTu.WeiHaiZXYY.Component.InfoQuery.ViewModels
{
    public class DateTimeViewModel : Default.Component.InfoQuery.ViewModels.DateTimeViewModel
    {
        protected override void Confirm()
        {
            switch (QueryChoiceModel.InfoQueryType)
            {
                case InfoQueryTypeEnum.已缴费明细:
                    if (DateTimeStart > DateTimeEnd)
                    {
                        ShowAlert(false, "已缴费信息查询", "开始时间不能晚于结束时间！");
                        return;
                    }
                    //var time = DateTimeEnd - DateTimeStart;
                    //if (time.Days >= 90)
                    //{
                    //    ShowAlert(true, "温馨提示", "缴费明细查询支持90天数据");
                    //    return;
                    //}
                    DoCommand(lp =>
                    {
                        lp.ChangeText("正在查询已缴费信息，请稍候...");
                        var req明细 = new req获取已结算明细记录
                        {
                            cardNo = CardModel.CardNo,
                            startDate = DateTimeStart.ToString("yyyy-MM-dd"),
                            endDate = DateTimeEnd.ToString("yyyy-MM-dd")
                        };
                        res获取已结算明细记录 res明细 = DataHandlerEx.获取已结算明细记录(req明细);
                        if (res明细?.success ?? false)
                        {
                            if (res明细?.data?.Count > 0)
                            {
                                ChangeNavigationContent($"{DateTimeStart.ToString("yyyy-MM-dd")}至\n{DateTimeEnd.ToString("yyyy-MM-dd")}");
                                PayCostRecordModel.Res获取已结算记录 = new res获取已结算记录();
                                PayCostRecordModel.Res获取已结算记录.data = new List<已缴费概要信息>();
                                res明细.data.ForEach((p)=> { p.billFee = p.billFee.InRMB(); p.cost = p.cost.InRMB(); p.itemPrice = p.itemPrice.InRMB(); });
                                PayCostRecordModel.Res获取已结算记录.data.Add(new 已缴费概要信息 { billItem = res明细.data });
                                Next();
                                return;
                            }
                            ShowAlert(false, "已缴费信息查询", "没有获得已缴费信息(列表为空)");
                            return;
                        }
                        ShowAlert(false, "已缴费信息查询", "没有获得已缴费信息", debugInfo: res明细?.msg);

                    });
                    break;
                case InfoQueryTypeEnum.药品查询:
                    break;
                case InfoQueryTypeEnum.项目查询:
                    break;
                case InfoQueryTypeEnum.检验结果:
                    DiagReportQuery();
                    break;
                case InfoQueryTypeEnum.住院押金查询:
                    InPrePayRecordQuery();
                    break;
                default:
                    PacsReportQuery();
                    break;
            }
        }
    }
}

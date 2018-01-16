using System;
using System.Windows.Input;
using Microsoft.Practices.Unity;
using Prism.Commands;
using Prism.Regions;
using YuanTu.Consts;
using YuanTu.Consts.Enums;
using YuanTu.Consts.Gateway;
using YuanTu.Consts.Models.Auth;
using YuanTu.Consts.Models.InfoQuery;
using YuanTu.Core.FrameworkBase;
using YuanTu.ShenZhenArea.Models;
using YuanTu.Core.Extension;
using System.Linq;

namespace YuanTu.ShenZhen.BaoAnChineseMedicineHospital.Component.InfoQuery.ViewModels
{
    public class DateTimeViewModel : YuanTu.Default.Component.InfoQuery.ViewModels.DateTimeViewModel
    {
        protected override void PayCostQuery()
        {
            if (DateTimeStart > DateTimeEnd)
            {
                ShowAlert(false, "已缴费信息查询", "开始时间不能晚于结束时间！");
                return;
            }
            DoCommand(lp =>
            {
                lp.ChangeText("正在查询已缴费信息，请稍候...");
                PayCostRecordModel.Req获取已结算记录 = new req获取已结算记录
                {
                    patientId = PatientModel.Res病人信息查询?.data[PatientModel.PatientInfoIndex].patientId,
                    cardNo = CardModel.CardNo,
                    cardType = ((int)CardModel.CardType).ToString(),
                    beginDate = DateTimeStart.ToString("yyyy-MM-dd"),
                    startDate = DateTimeStart.ToString("yyyy-MM-dd"),
                    endDate = DateTimeEnd.ToString("yyyy-MM-dd")
                };
                PayCostRecordModel.Res获取已结算记录 = DataHandlerEx.获取已结算记录(PayCostRecordModel.Req获取已结算记录);
                if (PayCostRecordModel.Res获取已结算记录?.success ?? false)
                {
                    if (PayCostRecordModel.Res获取已结算记录?.data?.Count > 0)
                    {
                        for (int i = 0; i < PayCostRecordModel.Res获取已结算记录.data.Count; i++)
                        {
                            //PayCostRecordModel.Res获取已结算记录.data[i].billItem = PayCostRecordModel.Res获取已结算记录.data[i].billItem?.Where(d => d.billType != "小计" && (!string.IsNullOrEmpty(d.itemName))).ToList();
                            for (int j = 0; j < PayCostRecordModel.Res获取已结算记录.data[i].billItem.Count; j++)
                            {
                                PayCostRecordModel.Res获取已结算记录.data[i].billItem[j].billType = PayCostRecordModel.Res获取已结算记录.data[i].billItem[j].itemSpecs;
                                if (!string.IsNullOrEmpty(PayCostRecordModel.Res获取已结算记录.data[i].billItem[j].itemPrice))
                                    PayCostRecordModel.Res获取已结算记录.data[i].billItem[j].itemPrice = (Convert.ToDecimal(PayCostRecordModel.Res获取已结算记录.data[i].billItem[j].itemPrice) * 100).ToString();
                            }
                            PayCostRecordModel.Res获取已结算记录.data[i].billItem = PayCostRecordModel.Res获取已结算记录.data[i].billItem?.Where(d => d.billType != "小计" ).ToList();
                        }
                        ChangeNavigationContent("已缴费信息查询",PatientModel.当前病人信息.name,$"{DateTimeStart.ToString("yyyy-MM-dd")}至\n{DateTimeEnd.ToString("yyyy-MM-dd")}");
                        Next();
                        return;
                    }
                    ShowAlert(false, "已缴费信息查询", "没有获得已缴费信息(列表为空)");
                    return;
                }
                ShowAlert(false, "已缴费信息查询", "没有获得已缴费信息", debugInfo: PayCostRecordModel.Res获取已结算记录?.msg);
            });
        }
    }
}
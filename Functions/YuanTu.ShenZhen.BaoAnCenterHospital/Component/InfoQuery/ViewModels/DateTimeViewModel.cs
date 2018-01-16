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
using YuanTu.Consts.Models;

namespace YuanTu.ShenZhen.BaoAnCenterHospital.Component.InfoQuery.ViewModels
{
    public class DateTimeViewModel : YuanTu.Default.Component.InfoQuery.ViewModels.DateTimeViewModel
    {

        [Dependency]
        public IChoiceModel ChoiceModel { get; set; }


        protected override void Confirm()
        {
            if (DateTimeStart > DateTimeEnd)
            {
                ShowAlert(false, "友好提示", "开始时间不能晚于结束时间！");
                return;
            }
            if (DateTimeStart.AddMonths(6) < DateTimeEnd)
            {
                ShowAlert(false, "友好提示", "查询时间区间不能超过6个月！");
                return;
            }

            switch (ChoiceModel.Business)
            {
                case Business.未定义:
                    break;
                case Business.建档:
                    break;
                case Business.挂号:
                    break;
                case Business.预约:
                    break;
                case Business.取号:
                    break;
                case Business.缴费:
                    break;
                case Business.充值:
                    break;
                case Business.查询:
                    break;
                case Business.住院押金:
                    break;
                case Business.出院结算:
                    break;
                case Business.健康服务:
                    break;
                case Business.体测查询:
                    break;
                case Business.外院卡注册:
                    break;
                case Business.补打:
                    break;
                case Business.实名认证:
                    break;
                case Business.生物信息录入:
                    break;
                case Business.药品查询:
                    break;
                case Business.项目查询:
                    break;
                case Business.已缴费明细:
                    break;
                case Business.检验结果:
                    QueryChoiceModel.InfoQueryType = InfoQueryTypeEnum.检验结果;
                    break;
                case Business.影像结果:
                    break;
                case Business.住院一日清单:
                    QueryChoiceModel.InfoQueryType = InfoQueryTypeEnum.住院一日清单;
                    break;
                case Business.住院押金查询:
                    break;
                case Business.住院床位查询:
                    break;
                case Business.执业资格查询:
                    break;
                case Business.交易记录查询:
                    break;
                case Business.门诊排班查询:
                    break;
                case Business.材料费查询:
                    break;
                case Business.收银:
                    break;
                case Business.签到:
                    break;
                default:
                    break;
            }

            switch (QueryChoiceModel.InfoQueryType)
            {
                case InfoQueryTypeEnum.已缴费明细:
                    PayCostQuery();
                    break;
                case InfoQueryTypeEnum.药品查询:
                    break;
                case InfoQueryTypeEnum.项目查询:
                    break;
                case InfoQueryTypeEnum.检验结果:
                    DiagReportQuery();
                    break;
                case InfoQueryTypeEnum.交易记录查询:
                    RechargeRecorQuery();
                    break;
                case InfoQueryTypeEnum.住院押金查询:
                    InPrePayRecordQuery();
                    break;
                case InfoQueryTypeEnum.影像结果:
                    PacsReportQuery();
                    break;
                case InfoQueryTypeEnum.住院一日清单:  
                    break;
                default:
                    ShowAlert(false, "友好提示", "业务未实现");
                    break;
            }
        }


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


        protected override void DiagReportQuery()
        {
            ShowAlert(false, "友好提示", "打印检验报告业务未实现");
            return;
        }
    }
}
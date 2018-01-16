using System;
using System.Windows.Input;
using Microsoft.Practices.Unity;
using Prism.Commands;
using Prism.Regions;
using YuanTu.Consts;
using YuanTu.Consts.Gateway;
using YuanTu.Consts.Models.Auth;
using YuanTu.Consts.Models.InfoQuery;
using YuanTu.Core.FrameworkBase;
using YuanTu.Consts.Services;
using YuanTu.Consts.Models.Print;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using YuanTu.Core.Extension;

namespace YuanTu.ShenZhen.BaoAnPeopleHospital.Component.InfoQuery.ViewModels
{
    public class InDailyDateViewModel : YuanTu.Default.Component.InfoQuery.ViewModels.InDailyDateViewModel
    {

        [Dependency]
        public IPrintModel PrintModel { get; set; }

        [Dependency]
        public IConfigurationManager ConfigurationManager { get; set; }

        [Dependency]
        public IPrintManager PrintManager { get; set; }

        public override void OnEntered(NavigationContext navigationContext)
        {
            DateTimeStart = DateTimeCore.Now.AddDays(-1);
            ConfirmCommand.Execute(null);
        }

        protected override void Confirm()
        {
            InDailyDetailModel.StartDate = DateTimeStart;
            InDailyDetailModel.EndDate = DateTimeStart;

            DoCommand(lp =>
            {
                lp.ChangeText("正在查询住院费用，请稍候...");
                var req = new req住院患者费用明细查询
                {
                    patientId = PatientModel.住院患者信息.patientHosId,
                    cardNo = PatientModel.住院患者信息.cardNo,
                    startDate = InDailyDetailModel.StartDate.ToString("yyyy-MM-dd"),
                    endDate = InDailyDetailModel.EndDate.ToString("yyyy-MM-dd")
                };

                InDailyDetailModel.Res住院患者费用明细查询 = DataHandlerEx.住院患者费用明细查询(req);
                if (InDailyDetailModel.Res住院患者费用明细查询?.success ?? false)
                {
                    if (InDailyDetailModel.Res住院患者费用明细查询?.data?.Count > 0)
                    {
                        //ChangeNavigationContent($"{DateTimeStart.ToString("yyyy-MM-dd")}至\n{InDailyDetailModel.EndDate.ToString("yyyy-MM-dd")}");
                        //Next();
                        var totalAmount = InDailyDetailModel.Res住院患者费用明细查询?.data.Sum(p => decimal.Parse(p.cost)).In元();
                        lp.ChangeText("正在打印住院一日清单");
                        PrintModel.SetPrintInfo(true, new PrintInfo
                        {
                            TypeMsg = "打印住院一日清单",
                            TipMsg = $"您已成功于{DateTimeCore.Now.ToString("HH:mm")}打印住院一日清单",
                            PrinterName = ConfigurationManager.GetValue("Printer:Receipt"),
                            Printables = InDailyDetailPrintables(totalAmount),
                            TipImage = "提示_住院一日清单"
                        });
                        Navigate(InnerA.ZYYRQD.Print);
                        ShowAlert(true, "打印完成", "住院一日清单已成功发送至打印机!");
                        return;
                    }
                    else
                    {
                        ShowAlert(false, "打印住院一日清单", "所选日期没有产生费用");
                    }
                }
                else
                {
                    ShowAlert(false, "打印住院一日清单", "没有获得住院患者费用明细");
                }
            });
        }



        protected virtual Queue<IPrintable> InDailyDetailPrintables(string totalAmount)
        {
            var res = InDailyDetailModel.Res住院患者费用明细查询;
            var patientInfo = PatientModel.住院患者信息;
            //var queue = PrintManager.NewQueue(Convert.ToDateTime(InDailyDetailModel.StartDate).Date.ToString("yyyy-MM-dd") + "住院日清单");
            var queue = PrintManager.NewQueue("每日费用汇总单");

            var sb = new StringBuilder();
            sb.Append($"----------------------------------\n");
            sb.Append($"病区：{patientInfo.area}　　押金余额：{patientInfo.accBalance.In元()}\n");
            sb.Append($"已交押金：{patientInfo.securityBalance.In元()}　　总费用：{patientInfo.cost.In元()}\n");
            sb.Append($"姓名：{patientInfo.name}　　性别：{patientInfo.sex}\n");
            sb.Append($"登记号：{patientInfo.patientId}\n床号：{patientInfo.bedNo}\n");
            sb.Append($"费用日期：{InDailyDetailModel.StartDate.ToString("yyyy-MM-dd")}\n");

            sb.Append($"分类           金额\n");

            Dictionary<string, decimal> listCost = new Dictionary<string, decimal>();
            foreach (var item in res?.data)
            {
                if (listCost.ContainsKey(item.productCode))
                {
                    listCost[item.productCode] = listCost[item.productCode] + Convert.ToDecimal(item.cost);
                }
                else
                {
                    listCost.Add(item.productCode, Convert.ToDecimal(item.cost));
                }
            }

            sb.Append($"----------------------------------\n");
            foreach (var item in listCost)
            {
                sb.Append($"{item.Key}           {item.Value.In元()}\n");
                sb.Append($"----------------------------------\n");

            }
            //foreach (var item in res?.data)
            //{
            //    sb.Append($"{item.productCode}-{item.itemName}           {item.cost.In元()}\n");
            //    sb.Append($"----------------------------------\n");
            //}
            sb.Append($"总计：           {totalAmount}\n");
            sb.Append($"----------------------------------\n");


            sb.Append($"\n\n温馨提示：此单据中总费用为住院之日起截止至{DateTimeCore.Now.ToString("yyyy-MM-dd")} 23:59:59的费用，此后产生的费用不计入此单据。由于医疗的特殊性，有些药物和项目是预收或者使用后补收费的，不明之处请及时到护士台咨询。出院结算以住院结算清单金额为准，以上现金余额不包括社保规定的自付起付线。\n");
            sb.Append($"打印时间：{DateTimeCore.Now.ToString("yyyy-MM-dd HH:mm:ss")}\n");
            sb.Append($"柜员编号：{FrameworkConst.OperatorId}\n");
            sb.Append($"该费用仅供参考，实际费用以出院结算为准\n");
            queue.Enqueue(new PrintItemText { Text = sb.ToString() });
            return queue;
        }
    }
}
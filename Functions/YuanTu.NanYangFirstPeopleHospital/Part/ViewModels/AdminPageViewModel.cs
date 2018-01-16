using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prism.Regions;
using YuanTu.Core.DB;
using YuanTu.Consts;
using YuanTu.Consts.Models.Print;
using System.Windows.Threading;
using YuanTu.Core.Log;
using YuanTu.Core.Extension;
using YuanTu.Consts.Services;

namespace YuanTu.NanYangFirstPeopleHospital.Part.ViewModels
{
    public class AdminPageViewModel:YuanTu.Default.Part.ViewModels.AdminPageViewModel
    {
        public override void OnEntered(NavigationContext navigationContext)
        {
            base.OnEntered(navigationContext);
            NeedUploadSuccess = true;//要求上报成功后才可清钱箱
        }

        protected override void DoClearCashBox()
        {
            DoCommand(lp => {
                lp.ChangeText("正在维护数据，请稍后...");

                if (DateTimeCore.Now.Year>2017|| DateTimeCore.Now.Month>7)
                {
                    return true;
                }
                var errorData = DBManager.Query<CashClearInfo>("Select * from CashClearInfo where DateTime>'2017-06-10 00:00:00' and DateTime<'2017-09-19 16:00:00' order by Id asc").ToArray();
                if (errorData.Any(p=>p.CashInputInfoId==0))
                {
                    //CashClearInfo last = null;
                    var maxId = -1;
                    foreach (var item in errorData)
                    {
                        if (item.CashInputInfoId == 0)
                        {
                            var currentMoney = DBManager.Query<CashInputInfo>("Select * from CashInputInfo Where id>$1 and DateTime<$2", maxId, item.DateTime);
                            var totalMoney=  currentMoney.Count == 0 ? 0 : currentMoney.Sum(p => p.TotalSeconds);
                            maxId =totalMoney==0? maxId: currentMoney.Max(p => p.Id);
                            var rows = DBManager.Excute("Update CashClearInfo set CashInputInfoId=$1 , CurrentCount=$2 where Id=$3", maxId, totalMoney, item.Id);
                        }
                        else
                        {
                            maxId = item.CashInputInfoId;
                        }
                    }
                }

                return true;
            }).ContinueWith(ctx =>
            {
                base.DoClearCashBox();
            });
        }

        protected override void PrintCashClear(DateTime lasTime, DateTime now, decimal totalmoney)
        {
            try
            {
                var info = DBManager.Query<RechargeInfo>($"Select * from RechargeInfo where RechargeMethod=1 and DateTime>='{lasTime.ToString("yyyy-MM-dd HH:mm:ss")}' and DateTime<'{now.ToString("yyyy-MM-dd HH:mm:ss")}'").ToArray();
                var success = info.Where(p => p.Success).ToArray();
                var failed =info.Where(p => !p.Success).ToArray();


                var printModel = GetInstance<IPrintModel>();

                var printerManager = GetInstance<IPrintManager>();
                var queue = printerManager.NewQueue("清钱箱");
                var sb = new StringBuilder();
                sb.Append("=================================\n");
                sb.Append("*********************************\n");
                sb.Append($"上次清钞：{lasTime.ToString("yyyy-MM-dd HH:mm:ss")}\n");
                sb.Append($"清钞时间：{now.ToString("yyyy-MM-dd HH:mm:ss")}\n");
                sb.Append($"钱箱金额：{totalmoney.In元()}\n");
                sb.Append("********************************\n");
                sb.Append($"成功笔数:{success.Length}  成功总额:{success.Sum(p=>p.TotalMoney).In元()}\n");
                sb.Append($"失败笔数:{failed.Length}  失败总额:{failed.Sum(p=>p.TotalMoney).In元()}\n");
                sb.Append("********************************\n");
                sb.Append($"柜员编号：{FrameworkConst.OperatorId}\n");
                sb.Append("*********************************\n");
                sb.Append("=================================\n");
                queue.Enqueue(new PrintItemText { Text = sb.ToString() });
                // printModel.SetPrintInfo(true, "清钱箱", "",ConfigurationManager.GetValue("Printer:Receipt"), queue);
                printModel.SetPrintInfo(true, new PrintInfo
                {
                    TypeMsg = "清钱箱",
                    PrinterName = ConfigurationManager.GetValue("Printer:Receipt"),
                    Printables = queue,
                    TipImage = "提示_凭条"
                });
                BeginInvoke(DispatcherPriority.ContextIdle, (Action)(() => { printerManager.Print(); }));
            }
            catch (Exception ex)
            {
                Logger.Main.Error($"[清钱箱]打印清钱箱凭条出现异常\r\n{ex.Message}\r\n{ex.StackTrace}");
            }
        }
    }
}

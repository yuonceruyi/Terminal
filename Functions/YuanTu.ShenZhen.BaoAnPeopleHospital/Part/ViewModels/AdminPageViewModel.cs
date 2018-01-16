using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
using YuanTu.Consts;
using YuanTu.Consts.Enums;
using YuanTu.Consts.Models.Print;
using YuanTu.Consts.Services;
using YuanTu.Core.DB;
using YuanTu.Core.Extension;
using YuanTu.Core.Log;
using YuanTu.Core.Reporter;
using YuanTu.Default.Part.ViewModels;

namespace YuanTu.ShenZhen.BaoAnPeopleHospital.Part.ViewModels
{
    public class AdminPageViewModel : YuanTu.Default.Part.ViewModels.AdminPageViewModel
    {
        protected override bool OnValidatePassword(string pwd)
        {
            AuthTypes.Clear();
            switch (pwd)
            {
                case ConfigBaoAnPeopleHospital.ExitPass:
                    AuthTypes.Add(AdminType.退出系统);
                    break;
                case ConfigBaoAnPeopleHospital.ClearCashboxPass:
                    AuthTypes.Add(AdminType.清钱箱);
                    break;
                default:
                    if (pwd == DateTimeCore.Now.ToString("yyMMdd"))
                    {
                        AuthTypes.Add(AdminType.自动更新);
                        AuthTypes.Add(AdminType.进入维护);
                        break;
                    }
                    else
                    {
                        return false;
                    }
            }
            Data = AuthTypes.Select(p => new AdminButtonInfo
            {
                Name = p.GetEnumDescription(),
                AdminType = p,
                Order = (int)p,
                IsEnabled = true,
            }).OrderBy(p => p.Order).ToList();
            return true;
        }
        /// <summary>
        /// 清钱箱并打印凭条
        /// </summary>
        protected override void DoClearCashBox()
        {
            var lastClear = DBManager.Query<CashClearInfo>("Select * from CashClearInfo order by Id desc limit 1")?.FirstOrDefault();
            if (lastClear == null)
            {
                lastClear = new CashClearInfo()
                {
                    DateTime = DateTime.MinValue,
                    CurrentCount = 0m,
                    CashInputInfoId = -1
                };
            }
            var currentMoney = DBManager.Query<CashInputInfo>("Select * from CashInputInfo Where id>$1",
                lastClear.CashInputInfoId);

            var totalMoney = currentMoney.Count == 0 ? 0 : currentMoney.Sum(p => p.TotalSeconds);
            var maxId = currentMoney.Count == 0 ? -1 : currentMoney.Max(p => p.Id);
            if (maxId == -1)
            {
                var max = DBManager.Query<CashInputInfo>("Select max(id) as Id from CashInputInfo").FirstOrDefault();
                if (max != null)
                {
                    maxId = max.Id;
                }
            }
            //if (totalMoney<=0)
            //{
            //    ShowAlert(false,"清钱箱",$"自{lastClear.DateTime.ToString("yyyy-MM-dd HH:mm:ss")}清钞后\r\n钱箱中没有现金，不用清理");
            //    return;
            //}

            var now = DateTimeCore.Now;
            var lastStr = $"上次清钱箱时间:{(lastClear.DateTime == DateTime.MinValue ? "无" : lastClear.DateTime.ToString("yyyy-MM-dd HH:mm:ss"))}";
            ShowConfirm("清钱箱", $"钱箱金额:{totalMoney.In元()}\r\n{lastStr}", cb =>
            {
                if (!cb) return;
                DBManager.Insert(new CashClearInfo
                {
                    DateTime = now,
                    CurrentCount = totalMoney,
                    CashInputInfoId = maxId
                });
                DoCommand(tt =>
                {
                    if (FrameworkConst.DeviceType == "YT-740")
                    {
                        PrintCashClearHatm(lastClear.DateTime, now, totalMoney);
                    }
                    else
                    {
                        PrintCashClear(lastClear.DateTime, now, totalMoney);
                    }
                    ShowAlert(true, "清钞成功!", $"{totalMoney.In元()}已于{now.ToString("yyyy年MM月dd日 HH:mm:ss")}成功清理！");
                });
            });
        }

        /// <summary>
        /// 清钱箱并打印凭条
        /// </summary>
        /// <param name="lasTime"></param>
        /// <param name="now"></param>
        /// <param name="totalmoney"></param>
        protected override void PrintCashClear(DateTime lasTime, DateTime now, decimal totalmoney)
        {
            try
            {
                var printModel = GetInstance<IPrintModel>();

                var printerManager = GetInstance<IPrintManager>();
                var queue = printerManager.NewQueue("清钱箱");
                var sb = new StringBuilder();
                sb.Append("=====================\n");
                sb.Append("*********************************\n");
                sb.Append($"上次清钞：{lasTime.ToString("yyyy-MM-dd HH:mm:ss")}\n");
                sb.Append($"清钞时间：{now.ToString("yyyy-MM-dd HH:mm:ss")}\n");
                sb.Append($"钱箱金额：{totalmoney.In元()}\n");
                sb.Append($"柜员编号：{FrameworkConst.OperatorId}\n");
                sb.Append("*********************************\n");
                sb.Append("=====================\n");
                queue.Enqueue(new PrintItemText { Text = sb.ToString() });
                // printModel.SetPrintInfo(true, "清钱箱", "",ConfigurationManager.GetValue("Printer:Receipt"), queue);
                printModel.SetPrintInfo(true, new PrintInfo
                {
                    TypeMsg = "清钱箱",
                    PrinterName = ConfigurationManager.GetValue("Printer:Receipt"),
                    Printables = queue,
                    TipImage = "提示_凭条"
                });
                //打印清钞凭条两次。。
                BeginInvoke(DispatcherPriority.ContextIdle, (Action)(() => { printerManager.Print(); printerManager.Print(); }));
            }
            catch (Exception ex)
            {
                Logger.Main.Error($"[清钱箱]打印清钱箱凭条出现异常\r\n{ex.Message}\r\n{ex.StackTrace}");
            }
            try
            {
                ReportService.清钱箱上报(totalmoney);
            }
            catch (Exception ex)
            {
                Logger.Main.Error($"[清钱箱]上报清钱箱出现异常\r\n{ex.Message}\r\n{ex.StackTrace}");
            }
        }
    }
}
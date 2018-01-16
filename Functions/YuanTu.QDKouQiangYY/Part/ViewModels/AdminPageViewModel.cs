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

namespace YuanTu.QDKouQiangYY.Part.ViewModels
{
    public class AdminPageViewModel : Default.Part.ViewModels.AdminPageViewModel
    {
        protected override bool OnValidatePassword(string pwd)
        {
            AuthTypes.Clear();
            if (pwd == DateTimeCore.Now.ToString("yyMMdd") && pwd != QDArea.ConfigQD.HighPwd)
            {
                AuthTypes.Add(AdminType.自动更新);
                AuthTypes.Add(AdminType.进入维护);
                AuthTypes.Add(AdminType.退出系统);

                Data = AuthTypes.Select(p => new AdminButtonInfo
                {
                    Name = p.GetEnumDescription(),
                    AdminType = p,
                    Order = (int)p,
                    IsEnabled = true,
                }).OrderBy(p => p.Order).ToList();

                return true;
            }
            if (pwd == QDArea.ConfigQD.HighPwd)
            {
                AuthTypes.Add(AdminType.自动更新);
                AuthTypes.Add(AdminType.清钱箱);
                AuthTypes.Add(AdminType.进入维护);
                AuthTypes.Add(AdminType.退出系统);
                Data = AuthTypes.Select(p => new AdminButtonInfo
                {
                    Name = p.GetEnumDescription(),
                    AdminType = p,
                    Order = (int)p,
                    IsEnabled = true,
                }).OrderBy(p => p.Order).ToList();

                return true;
            }
            return false;
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
                var sql = $@"Select * 
                               from RechargeInfo a 
                              Where a.Datetime > '{lasTime.ToString("yyyy-MM-dd HH:mm:ss")}' 
                                and a.Datetime < '{now.ToString("yyyy-MM-dd HH:mm:ss")}' 
                                and a.RechargeMethod ='1'
                                and a.Success ='1'";
                var SuccessInfo = DBManager.Query<RechargeInfo>(sql);
                var totalSuccess = SuccessInfo.Count == 0 ? 0 : SuccessInfo.Sum(p => p.TotalMoney);

                var printModel = GetInstance<IPrintModel>();
                var printerManager = GetInstance<IPrintManager>();
                var queue = printerManager.NewQueue("清钱箱");
                var sb = new StringBuilder();
                sb.Append("=================================\n");
                sb.Append("*********************************\n");
                sb.Append($"上次清钞：{lasTime.ToString("yyyy-MM-dd HH:mm:ss")}\n");
                sb.Append($"清钞时间：{now.ToString("yyyy-MM-dd HH:mm:ss")}\n");
                sb.Append($"钱箱金额：{totalmoney.In元()}\n");
                sb.Append($"充值成功金额：{totalSuccess.In元()}\n");
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
                View.Dispatcher.BeginInvoke(DispatcherPriority.ContextIdle, (Action)(() => { printerManager.Print(); }));
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

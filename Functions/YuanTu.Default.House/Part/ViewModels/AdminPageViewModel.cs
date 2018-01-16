using Microsoft.Practices.Unity;
using Prism.Commands;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using YuanTu.Consts;
using YuanTu.Consts.Models.Print;
using YuanTu.Consts.Services;
using YuanTu.Core.DB;
using YuanTu.Core.Extension;
using YuanTu.Core.FrameworkBase;
using YuanTu.Core.Log;
using YuanTu.Core.Reporter;
using YuanTu.Default.Part.ViewModels.AdminSub;
using SerialPrinter = YuanTu.Devices.Printer;

namespace YuanTu.Default.House.Part.ViewModels
{
    public class AdminPageViewModel : ViewModelBase
    {
        public DelegateCommand<AdminButtonInfo> Command { get; set; }
        private IReadOnlyCollection<AdminButtonInfo> _data;

        public IReadOnlyCollection<AdminButtonInfo> Data
        {
            get { return _data; }
            set
            {
                _data = value;
                OnPropertyChanged();
            }
        }

        [Dependency]
        public IConfigurationManager ConfigurationManager { get; set; }

        private string _password;
        private Visibility _showPanel;

        //public List<AdminTypes> AuthTypes = new List<AdminTypes>();
        public override string Title => "后台页面";

        public string Hint => "系统后台";
        public ICommand CancelCommand { get; set; }

        public Visibility ShowPanel
        {
            get { return _showPanel; }
            set
            {
                _showPanel = value;
                OnPropertyChanged();
            }
        }

        public string Password
        {
            get { return _password; }
            set
            {
                _password = value;
                OnPropertyChanged();
                if (OnValidatePassword(_password))
                {
                    ShowPanel = Visibility.Visible;
                    ShowMask(false, null);
                }
            }
        }

        public AdminPageViewModel()
        {
            Command = new DelegateCommand<AdminButtonInfo>(Confirm, info => info.IsEnabled);
            //ClearCashBox = new DelegateCommand(DoClearCashBox, () => AuthTypes.Contains(AdminTypes.清钱箱));
            //ExitCommand = new DelegateCommand(OnExitCommand, () => AuthTypes.Contains(AdminTypes.退出系统));
            //AutoUpdateCommand = new DelegateCommand(OnAutoUpdate, () => AuthTypes.Contains(AdminTypes.系统升级));
            //MaintenanceCommand = new DelegateCommand(OnMaintenance, () => AuthTypes.Contains(AdminTypes.进入维护));
            CancelCommand = new DelegateCommand(() =>
            {
                Navigate(A.Home);
            });
        }

        protected virtual void Confirm(AdminButtonInfo param)
        {
            switch (param.AdminTypes)
            {
                case AdminTypes.清钱箱:
                    DoClearCashBox();
                    break;

                case AdminTypes.系统升级:
                    OnAutoUpdate();
                    break;

                case AdminTypes.设置卡数量:
                    break;

                case AdminTypes.进入维护:
                    OnMaintenance();
                    break;

                case AdminTypes.退出系统:
                    OnExitCommand();
                    break;

                default:
                    ShowAlert(false, "温馨提示", "功能未开通！");
                    break;
            }
        }

        /// <summary>
        ///     进入当期页面时触发
        /// </summary>
        /// <param name="navigationContext"></param>
        public override void OnEntered(NavigationContext navigationContext)
        {
            HideNavigating = true;
            ShowPanel = Visibility.Collapsed;
            var login = new Login { DataContext = this };
            Password = null;
            ShowMask(true, login);
        }

        /// <summary>
        ///     离开当前页面时触发
        /// </summary>
        /// <param name="navigationContext"></param>
        /// <returns>是否允许跳转</returns>
        public override bool OnLeaving(NavigationContext navigationContext)
        {
            ShowMask(false);
            return base.OnLeaving(navigationContext);
        }

        //public DelegateCommand ClearCashBox { get; set; }
        //public DelegateCommand ExitCommand { get; set; }
        //public DelegateCommand AutoUpdateCommand { get; set; }
        //public DelegateCommand MaintenanceCommand { get; set; }

        protected virtual void DoClearCashBox()
        {
            var lastClear =
                DBManager.Query<CashClearInfo>("Select * from CashClearInfo order by Id desc limit 1")?.FirstOrDefault();
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
            ShowConfirm("清钱箱",
                $"钱箱金额:{totalMoney.In元()}\r\n上次清钱箱时间:{lastClear.DateTime.ToString("yyyy-MM-dd HH:mm:ss")}", cb =>
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
        /// 验证是否是正确的密码
        /// </summary>
        /// <param name="pwd"></param>
        /// <returns></returns>
        protected virtual bool OnValidatePassword(string pwd)
        {
            //AuthTypes.Clear();
            if (pwd == DateTimeCore.Now.ToString("yyMMdd"))
            {
                Data = GetAdminButtonInfoList().OrderBy(p => p.Order).ToList();
                //AuthTypes.Add(AdminTypes.清钱箱);
                //AuthTypes.Add(AdminTypes.退出系统);
                //AuthTypes.Add(AdminTypes.进入维护);
                //ClearCashBox.RaiseCanExecuteChanged();
                //ExitCommand.RaiseCanExecuteChanged();
                //AutoUpdateCommand.RaiseCanExecuteChanged();
                //AutoUpdateCommand.RaiseCanExecuteChanged();
                //MaintenanceCommand.RaiseCanExecuteChanged();
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
        protected virtual void PrintCashClear(DateTime lasTime, DateTime now, decimal totalmoney)
        {
            try
            {
                var printModel = GetInstance<IPrintModel>();

                var printerManager = GetInstance<IPrintManager>();
                var queue = printerManager.NewQueue("清钱箱");
                var sb = new StringBuilder();
                sb.Append("=================================\n");
                sb.Append("*********************************\n");
                sb.Append($"上次清钞：{lasTime.ToString("yyyy-MM-dd HH:mm:ss")}\n");
                sb.Append($"清钞时间：{now.ToString("yyyy-MM-dd HH:mm:ss")}\n");
                sb.Append($"钱箱金额：{totalmoney.In元()}\n");
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

        /// <summary>
        /// HATM清钱箱并打印凭条
        /// </summary>
        /// <param name="lasTime"></param>
        /// <param name="now"></param>
        /// <param name="totalmoney"></param>
        protected virtual void PrintCashClearHatm(DateTime lasTime, DateTime now, decimal totalmoney)
        {
            try
            {
                SerialPrinter.SerialPrinter printer = new SerialPrinter.SerialPrinter();
                var context = printer.GetContext();
                context[SerialPrinter.PrintableContext.SerialPort] = "COM6";
                var apt = printer.Connect(context);
                if (!apt.Success)
                {
                    ShowAlert(false, "打印失败", "连接打印机失败:" + apt.Message);
                }
                printer.SetLeftSpacing(30, 0);
                printer.SetFontSize(2, 2);
                printer.SetAlign(1); //居中
                printer.Text("清钱箱").FeedLine();
                printer.SetFontSize(1, 1);
                printer.SetAlign(0); //左侧
                printer.Text("=================================").FeedLine();
                printer.Text("*********************************").FeedLine();
                printer.Text($"上次清钞：{lasTime.ToString("yyyy-MM-dd HH: mm:ss")}").FeedLine();
                printer.Text($"清钞时间：{now.ToString("yyyy-MM-dd HH:mm:ss")}").FeedLine();
                printer.Text($"钱箱金额：{totalmoney.In元()}").FeedLine();
                printer.Text($"柜员编号：{FrameworkConst.OperatorId}").FeedLine();
                printer.Text("*********************************").FeedLine();
                printer.Text("=================================").FeedLine();

                printer.CutPaper(0);
                printer.Print();
                printer.Disconnect();
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

        /// <summary>
        /// 退出系统
        /// </summary>
        protected virtual void OnExitCommand()
        {
            Logger.Main.Info($"[系统关闭]退出系统！");
            Application.Current.Shutdown();
        }

        /// <summary>
        /// 自动更新
        /// </summary>
        protected virtual void OnAutoUpdate()
        {
            Logger.Main.Info($"[自动更新]自动更新重进系统！");
            AutoUpdater.Update.Do();
        }

        protected virtual void OnMaintenance()
        {
            Logger.Main.Info($"[进入维护]进入维护模式！");
            Navigate(A.Maintenance);
        }

        protected virtual List<AdminButtonInfo> GetAdminButtonInfoList()
        {
            return new List<AdminButtonInfo>()
            {
                new AdminButtonInfo
                {
                    Name = "清钱箱",
                    AdminTypes = AdminTypes.清钱箱,
                    Order = 0,
                    IsEnabled = false,
                },
                new AdminButtonInfo
                {
                    Name = "系统升级",
                    AdminTypes = AdminTypes.系统升级,
                    Order = 1,
                    IsEnabled = false,
                },
                new AdminButtonInfo
                {
                    Name = "进入维护",
                    AdminTypes = AdminTypes.进入维护,
                    Order = 2,
                    IsEnabled = true,
                },
                new AdminButtonInfo
                {
                    Name = "设置卡数量",
                    AdminTypes = AdminTypes.设置卡数量,
                    Order = 3,
                    IsEnabled = false,
                },
                new AdminButtonInfo
                {
                    Name = "退出系统",
                    AdminTypes = AdminTypes.退出系统,
                    Order = 4,
                    IsEnabled = true,
                },
            };
        }

        public enum AdminTypes
        {
            清钱箱,
            系统升级,
            进入维护,
            设置卡数量,
            退出系统,
        }

        public class AdminButtonInfo
        {
            public string Name { get; set; }
            public AdminTypes AdminTypes { get; set; }
            public int Order { get; set; }
            public bool Visabled { get; set; }
            public bool IsEnabled { get; set; }
        }
    }
}
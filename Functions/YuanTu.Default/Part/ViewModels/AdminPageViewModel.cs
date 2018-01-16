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
using YuanTu.Consts.Enums;
using YuanTu.Consts.FrameworkBase;
using YuanTu.Consts.Models.Print;
using YuanTu.Consts.Services;
using YuanTu.Core.DB;
using YuanTu.Core.Extension;
using YuanTu.Core.FrameworkBase;
using YuanTu.Core.Log;
using YuanTu.Core.Reporter;
using YuanTu.Default.Part.ViewModels.AdminSub;
using SerialPrinter = YuanTu.Devices.Printer;

namespace YuanTu.Default.Part.ViewModels
{
    public class AdminPageViewModel : ViewModelBase
    {
        [Dependency]
        public IConfigurationManager ConfigurationManager { get; set; }

        private string _password;
        private Visibility _showPanel;
        private string _fromAddress;

        public List<AdminType> AuthTypes = new List<AdminType>();

        /// <summary>
        /// 是否需要在上报成功后再清钱箱
        /// </summary>
        public bool NeedUploadSuccess { get; set; } = false;
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

            CancelCommand = new DelegateCommand(() =>
            {
               Navigate(_fromAddress);
            });
        }

        protected virtual void Confirm(AdminButtonInfo param)
        {
            switch (param.AdminType)
            {
                case AdminType.清钱箱:
                    DoClearCashBox();
                    break;

                case AdminType.自动更新:
                    OnAutoUpdate();
                    break;

                case AdminType.设置卡数量:
                    break;

                case AdminType.进入维护:
                    OnMaintenance();
                    break;

                case AdminType.退出系统:
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
            _fromAddress = navigationContext.Parameters["From"].ToString();
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

        /// <summary>
        /// 验证是否是正确的密码
        /// </summary>
        /// <param name="pwd"></param>
        /// <returns></returns>
        protected virtual bool OnValidatePassword(string pwd)
        {
            AuthTypes.Clear();
            if (pwd == DateTimeCore.Now.ToString("yyMMdd"))
            {
                AuthTypes.Add(AdminType.清钱箱);
                AuthTypes.Add(AdminType.退出系统);
                AuthTypes.Add(AdminType.进入维护);
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
        protected virtual void DoClearCashBox()
        {
            DoCommand(lp =>
            {
                lp.ChangeText("正在获取钱箱信息...");
                var dto = new ClearCashBoxDto();
                var lastClear =
                    DBManager.Query<CashClearInfo>("Select * from CashClearInfo order by Id desc limit 1")?
                        .FirstOrDefault();
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
                dto.LastDate = lastClear.DateTime;
                dto.TotalMoney = currentMoney.Count == 0 ? 0 : currentMoney.Sum(p => p.TotalSeconds);
                var maxId = currentMoney.Count == 0 ? -1 : currentMoney.Max(p => p.Id);
                dto.MaxId = maxId;
                if (maxId == -1)
                {
                    var max = DBManager.Query<CashInputInfo>("Select max(id) as Id from CashInputInfo").FirstOrDefault();
                    if (max != null)
                    {
                        dto.MaxId = max.Id;
                    }
                }
                return Result<ClearCashBoxDto>.Success(dto);
            }).ContinueWith(ret =>
            {
                if (!ret.Result.IsSuccess)
                {
                    return;
                }
                var dto = ret.Result.Value;
                var now = DateTimeCore.Now;
                ShowConfirm("清钱箱", $"钱箱金额:{dto.TotalMoney.In元()}\r\n上次清钱箱时间:{dto.LastDate.ToString("yyyy-MM-dd HH:mm:ss")}", cb =>
                {
                    if (!cb) return;

                    DoCommand(tt =>
                    {

                        var reportResult = false;
                        try
                        {
                            tt.ChangeText("上送交易数据....");
                            reportResult = ReportService.清钱箱上报(dto.TotalMoney);
                        }
                        catch (Exception ex)
                        {
                            Logger.Main.Error($"[清钱箱]上报钱箱数据出现异常\r\n{ex.Message}\r\n{ex.StackTrace}");

                        }
                        if (NeedUploadSuccess && (!reportResult))
                        {
                            ShowAlert(false, "钱箱清理失败", "钱箱信息上送失败，请检查网络是否正常！");
                            return;
                        }
                        tt.ChangeText("更新钱箱信息....");
                        DBManager.Insert(new CashClearInfo
                        {
                            DateTime = now,
                            CurrentCount = dto.TotalMoney,
                            CashInputInfoId = dto.MaxId
                        });
                        tt.ChangeText("打印凭条....");
                        if (FrameworkConst.DeviceType == "YT-740")
                        {
                            PrintCashClearHatm(dto.LastDate, now, dto.TotalMoney);
                        }
                        else
                        {
                            PrintCashClear(dto.LastDate, now, dto.TotalMoney);
                        }
                        ShowAlert(true, "清钞成功!", $"{dto.TotalMoney.In元()}已于{now.ToString("yyyy年MM月dd日 HH:mm:ss")}成功清理！");
                    });
                });

            });
           
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
               BeginInvoke(DispatcherPriority.ContextIdle, (Action)(() => { printerManager.Print(); }));
            }
            catch (Exception ex)
            {
                Logger.Main.Error($"[清钱箱]打印清钱箱凭条出现异常\r\n{ex.Message}\r\n{ex.StackTrace}");
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
                printer.SetAlign(1);//居中
                printer.Text("清钱箱").FeedLine();
                printer.SetFontSize(1, 1);
                printer.SetAlign(0);//左侧
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

        /// <summary>
        /// 进入维护模式
        /// </summary>
        protected virtual void OnMaintenance()
        {
            Logger.Main.Info($"[进入维护]进入维护模式！");
            Navigate(A.Maintenance);
        }

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
    }

    public class AdminButtonInfo
    {
        public string Name { get; set; }
        public AdminType AdminType { get; set; }
        public int Order { get; set; }
        public bool Visabled { get; set; }
        public bool IsEnabled { get; set; }
    }

    public class ClearCashBoxDto
    {
        public decimal TotalMoney { get; set; }
        public DateTime LastDate { get; set; }
        public int MaxId { get; set; }
    }
}
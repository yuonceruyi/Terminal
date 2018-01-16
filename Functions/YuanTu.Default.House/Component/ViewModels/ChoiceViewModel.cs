using Prism.Commands;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using YuanTu.Consts;
using YuanTu.Consts.Enums;
using YuanTu.Consts.FrameworkBase;
using YuanTu.Consts.Models;
using YuanTu.Consts.Services;
using YuanTu.Core.FrameworkBase;
using YuanTu.Core.Navigating;
using YuanTu.Core.Systems;
using YuanTu.Default.House.Common;
using YuanTu.Default.House.Device.Gate;
using YuanTu.Devices.PrinterCheck;
using YuanTu.Devices.PrinterCheck.CePrinterCheck;

namespace YuanTu.Default.House.Component.ViewModels
{
    public class ChoiceViewModel : ViewModelBase
    {
        public override string Title => "主页";
        private bool _checkToScreenSaver;

        public ChoiceViewModel()
        {
            Command = new DelegateCommand<ChoiceButtonInfo>(Confirm, info => info.IsEnabled);
        }

        public DelegateCommand<ChoiceButtonInfo> Command { get; set; }

        public override void OnSet()
        {
            HideNavigating = true;

            var config = GetInstance<IConfigurationManager>();
            LayoutRule = config.GetValue("LayoutRule");

            var resource = ResourceEngine;
            var businesses = Enum.GetValues(typeof(Business));
            var buttons = businesses.Cast<Business>()
                .Where(b => config.GetValue($"HouseFunctions:{b}:Visabled") == "1")
                .Select(b => new ChoiceButtonInfo
                {
                    Name = config.GetValue($"HouseFunctions:{b}:Name") ?? "未定义",
                    ButtonBusiness = b,
                    Order = config.GetValueInt($"HouseFunctions:{b}:Order"),
                    IsEnabled = config.GetValueInt($"HouseFunctions:{b}:IsEnabled") == 1,
                    ImageSource =
                        resource.GetImageResource(config.GetValue($"HouseFunctions:{b}:ImageName"))
                })
                .OrderBy(p => p.Order)
                .ToList();
            if (Startup.HealthDetectCount == 0)
                buttons.Remove(buttons.FirstOrDefault(p => p.ButtonBusiness == Business.健康服务));
            Data = buttons;
        }

        public override void OnEntered(NavigationContext navigationContext)
        {
            TimeOut = 0;
            _checkToScreenSaver = true;
            GetInstance<IGateService>().CloseGateAsync();
            CheckToScreenSaver();
        }

        public override bool OnLeaving(NavigationContext navigationContext)
        {
            _checkToScreenSaver = false;
            return true;
        }

        protected virtual void Confirm(ChoiceButtonInfo param)
        {
            DoCommand(p =>
            {
                var result = GetInstance<IReceiptPrinterCheckService>().CheckReceiptPrinter();
                if (!result.IsSuccess)
                {
                    ShowAlert(false, "打印机检测", result.Message);
                    return;
                }
                var choiceModel = GetInstance<IChoiceModel>();

                choiceModel.Business = param.ButtonBusiness;

                var engine = NavigationEngine;

                switch (param.ButtonBusiness)
                {
                    case Business.健康服务:
                        engine.JumpAfterFlow(new FormContext(A.ChaKa_Context, A.CK.Card),
                          null,
                          new FormContext(AInner.Health_Context, ViewContexts.ViewContextList[0].Address), "健康服务");
                        break;

                    case Business.预约:
                        engine.JumpAfterFlow(new FormContext(A.ChaKa_Context, A.CK.Card),
                           null,
                           new FormContext(A.YuYue_Context, AInner.YY.ChoiceHospital), "预约挂号");
                        break;

                    case Business.体测查询:
                        engine.JumpAfterFlow(new FormContext(A.ChaKa_Context, A.CK.Card),
                          null,
                          new FormContext(AInner.Query_Context, AInner.Query.DateTimeView), "时间选择");
                        break;

                    case Business.建档:
                        engine.JumpAfterFlow(new FormContext(AInner.Create_Context, AInner.JD.SelectType),
                         null,
                         new FormContext(AInner.Create_Context, AInner.JD.SelectType), "");
                        break;

                    case Business.切换终端:
                        var pid = Process.GetCurrentProcess().Id;
                        var other = Process.GetProcessesByName("Terminal")
                            .Concat(Process.GetProcessesByName("YuanTu.Test"))
                            .FirstOrDefault(proc => proc.Id != pid);
                        if (other != null)
                            WindowHelper.SetForegroundWindow(other.MainWindowHandle);
                        break;

                    case Business.远程问诊:
                        engine.JumpAfterFlow(new FormContext(A.ChaKa_Context, A.CK.Card),
                          RemoteDiag,
                          new FormContext(A.ChaKa_Context, A.Home), "远程问诊");
                        break;

                    default:
                        ShowAlert(false, "温馨提示", "业务未开通");
                        break;
                }
            });
        }

        private async Task<Result<FormContext>> RemoteDiag()
        {
            var path_x86 = @"C:\Program Files (x86)\Google\Chrome\Application\chrome.exe";
            var path_default = @"C:\Program Files\Google\Chrome\Application\chrome.exe";

            string path;
            if (File.Exists(path_x86))
            {
                path = path_x86;
            }
            else if (File.Exists(path_default))
            {
                path = path_default;
            }
            else
            {
                ShowAlert(false, "远程问诊","启动失败");
                return Result<FormContext>.Fail("");
            }
            var url = GetInstance<IConfigurationManager>().GetValue("RemoteDiagUrl");
            Process.Start(path, $"--kiosk --new-window {url}");
            return Result<FormContext>.Success(new FormContext(null, A.Home));
        }

        public virtual void CheckToScreenSaver()
        {
            var count = 0;
            Task.Run(() =>
            {
                while (_checkToScreenSaver)
                {
                    count++;
                    Thread.Sleep(1000);
                    if (count <= 10) continue;
                    if (_checkToScreenSaver)
                        Navigate(AInner.ScreenSaver);
                }
            });
        }

        #region DataBinding

        private IReadOnlyCollection<ChoiceButtonInfo> _data;

        public IReadOnlyCollection<ChoiceButtonInfo> Data
        {
            get { return _data; }
            set
            {
                _data = value;
                OnPropertyChanged();
            }
        }

        private string _layoutRule;
        public string LayoutRule
        {
            get { return _layoutRule; }
            set
            {
                _layoutRule = value;
                OnPropertyChanged();
            }
        }

        #endregion
    }
}
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Microsoft.Practices.ObjectBuilder2;
using Microsoft.Practices.Unity;
using Prism.Commands;
using Prism.Regions;
using YuanTu.Consts;
using YuanTu.Consts.Enums;
using YuanTu.Consts.Models;
using YuanTu.Consts.Models.Auth;
using YuanTu.Consts.Services;
using YuanTu.Consts.Sounds;
using YuanTu.Core.Log;
using YuanTu.Devices.CardReader;
using YuanTu.ShengZhouZhongYiHospital.Component.Auth.Dialog;

namespace YuanTu.ShengZhouZhongYiHospital.Component.Auth.ViewModels
{
    public class CardViewModel : Default.Component.Auth.ViewModels.CardViewModel
    {
        public CardViewModel(IRFCardReader[] rfCardReaders, IMagCardReader[] magCardReaders) : base(rfCardReaders, magCardReaders)
        {
            ShowInputMaskCommand = new DelegateCommand(ShowInputMask);
            CancelHospitalCardNoCommand = new DelegateCommand(CancelHospitalCardNo);
            ConfirmHospitalCardNoCommand = new DelegateCommand(ConfirmHospitalCardNo);
        }

        public override void OnEntered(NavigationContext navigationContext)
        {
            ChangeNavigationContent("");
        }

        public override void OnSet()
        {
            BackUri = ResourceEngine.GetImageResourceUri("就诊卡扫描处");
            CardUri = ResourceEngine.GetImageResourceUri("卡_诊疗卡");
            TipContent = "请在感应区扫条码";
            Hint = "请按提示扫条码";
        }

        private string _hospitalCardNo;
        private bool _hospitalInputFocus;
        #region[手动输入卡号]

        public string HospitalCardNo
        {
            get { return _hospitalCardNo; }
            set
            {
                _hospitalCardNo = value;
                OnPropertyChanged();
            }
        }

        public bool HospitalInputFocus
        {
            get { return _hospitalInputFocus; }
            set
            {
                _hospitalInputFocus = value;
                OnPropertyChanged();
            }
        }
        //取消手动输入卡号
        public ICommand CancelHospitalCardNoCommand { get; set; }
        //确认输入的卡号
        public ICommand ConfirmHospitalCardNoCommand { get; set; }
        //弹出手输卡号框
        public ICommand ShowInputMaskCommand { get; set; }
        private void ShowInputMask()
        {
            HospitalCardNo = null;
            HospitalInputFocus = true;
            ShowMask(true, new HospitalCardDialog() { DataContext = this });
            HospitalInputFocus = true;
        }

        private void CancelHospitalCardNo()
        {
            ShowMask(false);
        }
        [Dependency]
        public IConfigurationManager ConfigurationManager { get; set; }

        private void ConfirmHospitalCardNo()
        {
            DoCommand((lp) =>
            {
                var flag = false;
                lp.ChangeText("正在为您打印检验报告单，请稍后");
                var lisPath = ConfigurationManager.GetValue("LisPath");
                Logger.Net.Info($"准备检验报告单打印，卡号：{HospitalCardNo}  lisPath:{lisPath}");
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = lisPath,
                        UseShellExecute = false,
                        RedirectStandardInput = true,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        CreateNoWindow = true,
                        Arguments = $"{HospitalCardNo}"
                    }
                };
                var task = Task.Run(() =>
                {
                    Thread.Sleep(5000);
                    flag = true;
                });
                process.Start();
                process.WaitForExit();
                while (true)
                {
                    if (flag)
                    {
                        break;
                    }
                    Thread.Sleep(1000);
                }
                ShowMask(false);
                HospitalCardNo = String.Empty;
                Navigate(A.Home);
            });
        }

        #endregion
        public override bool OnLeaving(NavigationContext navigationContext)
        {
            _working = false;
            return true;
        }

    }
}

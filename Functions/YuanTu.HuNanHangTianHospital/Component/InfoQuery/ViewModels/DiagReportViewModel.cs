using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YuanTu.Consts;
using YuanTu.Consts.Gateway;
using YuanTu.Core.Log;

namespace YuanTu.HuNanHangTianHospital.Component.InfoQuery.ViewModels
{
    public class DiagReportViewModel : YuanTu.Default.Component.InfoQuery.ViewModels.DiagReportViewModel
    {
        public override void OnEntered(NavigationContext navigationContext)
        {
            Logger.Main.Info($"[确认打印] CardNo=" + CardModel.CardNo);
            Process printePro = new Process();
            printePro.StartInfo.WindowStyle = ProcessWindowStyle.Minimized; //隐藏压缩窗口
            printePro.StartInfo.FileName = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"自助打印\wxlis\SelfHelpReportProvider\SelfHelpReportProvider.exe");//
            if (!File.Exists(printePro.StartInfo.FileName))
            {
                ShowAlert(false, "检验结果打印", "本机不支持打印检验报告，请到其它自助机打印。");
                Task.Run(() => { Navigate(A.Home); });
                return;
            }
            printePro.StartInfo.CreateNoWindow = false;
            printePro.StartInfo.Arguments = CardModel.CardNo;
            printePro.Start();
            //printePro.WaitForExit();
            //int iExitCode = 0;
            //if (printePro.HasExited)
            //{
            //    iExitCode = printePro.ExitCode;
            //    printePro.Close();
            //    if (iExitCode != 0 && iExitCode != 1)
            //    {
            //        ShowAlert(false, "检验结果打印", "打印失败,LIS程序出错");
            //    }
            //}
            Task.Run(() => { Navigate(A.Home); });
        }
    }
}


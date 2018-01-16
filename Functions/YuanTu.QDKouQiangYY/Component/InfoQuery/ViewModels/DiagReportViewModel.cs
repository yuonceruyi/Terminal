using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YuanTu.Consts;
using YuanTu.Consts.FrameworkBase;
using YuanTu.Core.Log;

namespace YuanTu.QDKouQiangYY.Component.InfoQuery.ViewModels
{
    public class DiagReportViewModel:YuanTu.Default.Component.InfoQuery.ViewModels.DiagReportViewModel
    {
        protected override void Confirm()
        {
            const string dirName = @"..\自助打印";

            if (!Directory.Exists(dirName)) //判断目录是否存在
            {
                ShowAlert(false, "检验结果打印", "本机不支持打印检验报告，请到其它自助机打印。");
                return;
            }

            ShowConfirm("检验结果打印", "是否确定打印？", cb =>
            {
                if (!cb) return;

                Logger.Main.Info($"[确认打印] CardNo=" + CardModel.CardNo);
                Process.GetProcessesByName("report").FirstOrDefault()?.Kill();
                var myProcess = new Process
                {
                    StartInfo = new ProcessStartInfo(@"..\自助打印\report.exe")
                    {
                        Arguments = "P4 " + CardModel.CardNo
                    },
                    EnableRaisingEvents = true
                };
                myProcess.Start();
                Navigate(A.Home);
                ShowAlert(true, "正在打印", "正在打印\n请稍候...");

            });
        }
    }
}

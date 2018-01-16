using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YuanTu.Consts;
using YuanTu.Consts.FrameworkBase;
using YuanTu.Core.Log;

namespace YuanTu.QDJZZXYY.Component.InfoQuery.ViewModels
{
    public class DiagReportViewModel:YuanTu.QDKouQiangYY.Component.InfoQuery.ViewModels.DiagReportViewModel
    {
        protected override void Confirm()
        {
            ShowConfirm("检验结果打印", "是否确定打印？", cb =>
            {
                if (!cb) return;
                Logger.Main.Info($"[确认打印] CardNo=" + CardModel.CardNo);
                string para = string.Empty;

                Process.GetProcessesByName("report").FirstOrDefault()?.Kill();

                para = DiagReportModel.Res检验基本信息查询.data.Aggregate(para, (current, info) => current + info.reportId + ",").TrimEnd(',');
                Logger.Main.Info($"[确认打印] para=" + para);

                var myProcess = new Process
                {
                    StartInfo = new ProcessStartInfo(@"..\自助打印\PrintConvert.exe")
                    {
                        Arguments = " 123 " + para
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

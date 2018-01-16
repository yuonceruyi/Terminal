using Microsoft.Practices.Unity;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YuanTu.Consts;
using YuanTu.Consts.Enums;
using YuanTu.Consts.Gateway;
using YuanTu.Consts.Models.InfoQuery;
using YuanTu.Consts.UserControls;
using YuanTu.Core.Log;

namespace YuanTu.WeiHaiZXYY.Component.InfoQuery.ViewModels
{
    public class DiagReportViewModel : YuanTu.Default.Component.InfoQuery.ViewModels.DiagReportViewModel
    {
        [Dependency]
        public IQueryChoiceModel QueryChoiceModel { get; set; }
        public override void OnEntered(NavigationContext navigationContext)
        {
            DoCommand(lp =>
            {
                lp.ChangeText("正在查询检验报告信息，请稍候...");
                DiagReportModel.Req检验基本信息查询 = new req检验基本信息查询
                {
                    cardNo = CardModel.CardNo,
                    cardType =CardModel.CardType==CardType.身份证?"0":"1",//住院号【身份证重写作为住院号】 
                };
                DiagReportModel.Res检验基本信息查询 = DataHandlerEx.检验基本信息查询(DiagReportModel.Req检验基本信息查询);
                if (DiagReportModel.Res检验基本信息查询?.success ?? false)
                {
                    if (DiagReportModel.Res检验基本信息查询?.data?.Count > 0)
                    {
                        Collection = DiagReportModel.Res检验基本信息查询.data.Select(p => new PageData
                        {
                            CatalogContent = $"{p.examType}\r\n{p.checkPart}",
                            List = p.examItem,
                            Tag = p,
                        }).ToArray();
                        BillCount = $"{DiagReportModel.Res检验基本信息查询.data.Count}张报告单";
                        return;
                    }
                    ShowAlert(false, "检验报告信息查询", "没有获得检验报告信息(列表为空)");
                    return;
                }
                ShowAlert(false, "检验报告信息查询", "没有获得检验报告信息");

            });
        }

        protected override void Confirm()
        {
            ShowConfirm("检验结果打印", "是否确定打印？", cb =>
            {
                //var InOutFlag=
                if (!cb) return;
                Logger.Main.Info($"[确认打印] CardNo=" + CardModel.CardNo);
                string para = string.Empty;

                Process.GetProcessesByName("PrintConvert").FirstOrDefault()?.Kill();
                Logger.Main.Info($"[确认打印] para=" + para);

                var myProcess = new Process
                {
                    StartInfo = new ProcessStartInfo(@"..\自助打印\PrintConvert.exe")
                    {
                        Arguments = $"{CardModel.CardNo} "
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

using Microsoft.Practices.Unity;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Prism.Regions;
using YuanTu.Consts;
using YuanTu.Consts.FrameworkBase;
using YuanTu.Consts.Models.Auth;
using YuanTu.Consts.Services;
using YuanTu.Consts.UserControls;
using YuanTu.Core.Extension;
using YuanTu.Core.Log;
using YuanTu.QDHD2ZY.CurrentService;

namespace YuanTu.QDHD2ZY.Component.InfoQuery.ViewModels
{
    public class DiagReportViewModel : YuanTu.QDKouQiangYY.Component.InfoQuery.ViewModels.DiagReportViewModel
    {
        public override void OnEntered(NavigationContext navigationContext)
        {
            ChangeNavigationContent("");

            Collection = DiagReportModel.Res检验基本信息查询.data
                .Where(p=>!string.IsNullOrWhiteSpace(p.resultTime))
                .Select(p => new PageData
            {
                CatalogContent = (p.printTimes == "0" ? "[未打印]" : "[已打印]") + $"{p.examType}\r\n{p.checkPart}",
                List = p.examItem,
                Tag = p,
            }).ToArray();
            BillCount = $"{DiagReportModel.Res检验基本信息查询.data.Count}张报告单";
        }

        protected override void Confirm()
        {
            try
            { 
                Logger.Main.Info($"[确认打印]");
            
                #region 查询
                var config = GetInstance<IConfigurationManager>();
                var file = Path.Combine(FrameworkConst.RootDirectory, "CurrentResource", FrameworkConst.HospitalId, "LisPrint", "DHCLabtrakReportPrint.dll");
                var connectString = config.GetValue("Lis:ConnectString");//LIS的网站配置的webservice发布地址

                Assembly asm = Assembly.LoadFrom(file);
                #endregion 查询

                #region 打印    
                var typePr = asm.GetType("DHCLabtrakReportPrint.DHCLabtrakReportPrint");
                var printOut = typePr.GetMethod("PrintOut");
                var instancePr = asm.CreateInstance("DHCLabtrakReportPrint.DHCLabtrakReportPrint");
                const string userParam = ""; //打印用户，空代表没有
                const string param = "OT"; //标识其他打印
                var printCount = DiagReportModel.Res检验基本信息查询.data.Distinct()
                    .Count(n => !string.IsNullOrWhiteSpace(n.resultTime)
                                && Convert.ToInt32(n.printTimes) == 0 
                                && Convert.ToDateTime(n.resultTime)> DateTimeCore.Now.AddMonths(-3));
                var visitCount = DiagReportModel.Res检验基本信息查询.data.Distinct()
                                .Count(n => string.IsNullOrWhiteSpace(n.resultTime));
                if (printCount > 0)
                {
                    var reportKeys = string.Join("^", DiagReportModel.Res检验基本信息查询.data
                                                      .Where(n => !string.IsNullOrWhiteSpace(n.resultTime)
                                                             && Convert.ToInt32(n.printTimes) == 0 
                                                             && Convert.ToDateTime(n.resultTime) > DateTimeCore.Now.AddMonths(-3))
                                                     .Select(n => $"{n.reportId}").Distinct());

                    var args = new object[] {
                                reportKeys,
                                userParam,
                                param,
                                connectString,
                                string.Empty,
                                string.Empty
                            };
                    printOut.Invoke(instancePr, args);

                    Navigate(A.Home);
                    ShowAlert(true, "正在打印", $"正在打印{printCount}张报告,报告未审核{visitCount}张");
                }
                else
                {
                    ShowAlert(false, "打印报告", $"没有可以打印的报告,报告未审核{visitCount}张");
                }
                #endregion 打印
            }
            catch (Exception ex)
            {
                Logger.Main.Error($"异常，原因:{ex.Message}\r\n{ex.StackTrace}\r\n{ex.InnerException}");
                ShowAlert(false, "打印报告", "打印失败，发生系统异常 ");
            }
        }        

    }
}

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using Microsoft.Practices.Unity;
using Prism.Commands;
using Prism.Regions;
using YuanTu.Consts.Models;
using YuanTu.Consts.Models.Auth;
using YuanTu.Consts.Models.InfoQuery;
using YuanTu.Consts.UserControls;
using YuanTu.Core.FrameworkBase;
using YuanTu.Consts.Gateway;
using YuanTu.Core.Extension;
using System;

namespace YuanTu.ShenZhen.BaoAnPeopleHospital.Component.InfoQuery.ViewModels
{
    public class DiagReportViewModel : YuanTu.Default.Component.InfoQuery.ViewModels.DiagReportViewModel
    {
        public override void OnEntered(NavigationContext navigationContext)
        {
            ChangeNavigationContent("");

            Collection = DiagReportModel.Res检验基本信息查询.data.Select(p => new PageData
            {
                CatalogContent = $"{p.examType}\r\n{p.checkPart}",
                List = p.examItem,
                Tag = p
            }).ToArray();
            BillCount = $"{DiagReportModel.Res检验基本信息查询.data.Count}张报告单";
        }

        protected override void Confirm()
        {
            DiagReportModel.所选检验信息 = SelectData.Tag.As<检验基本信息>();

            if (DiagReportModel.所选检验信息.checkPart != "800") //未出
            {
                ShowAlert(false, "温馨提示", "该报告单尚未出结果", 5);
                return;
            }
            if ((!string.IsNullOrEmpty(DiagReportModel.所选检验信息.printTimes)) && Convert.ToInt32(DiagReportModel.所选检验信息.printTimes) > 0)
            {
                ShowAlert(false, "温馨提示", "您已经打印过该报告单，不能重复打印", 5);
                return;
            }

            Lis4Print.TCPrint Lis4 = new Lis4Print.TCPrint();
            string errorString = "";
            Lis4.PrintReportBySampleNo(Convert.ToInt32(DiagReportModel.所选检验信息.reportId), out errorString);
            if(string.IsNullOrEmpty(errorString))
            {

                ShowAlert(true, "温馨提示", "打印成功，请稍候拿走报告单", 5);
                return;
            }

            ShowAlert(false, "温馨提示", "打印失败，请联系导医或护士", 5);
            return;
        }
    }
}
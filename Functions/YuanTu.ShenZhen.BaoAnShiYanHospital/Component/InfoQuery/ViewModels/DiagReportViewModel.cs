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
using YuanTu.Core.Log;
using System.Diagnostics;
using System;
using YuanTu.Consts;
using System.IO;

namespace YuanTu.ShenZhen.BaoAnShiYanHospital.Component.InfoQuery.ViewModels
{
    public class DiagReportViewModel : YuanTu.Default.Component.InfoQuery.ViewModels.DiagReportViewModel
    {
        //public override void OnEntered(NavigationContext navigationContext)
        //{
        //    try
        //    {
        //        TimeOut = ConfigBaoAnShiYanHospital.LisTimeOut;
        //        var txt = "\"" + ConfigBaoAnShiYanHospital.LisFolderPath + "Lis.Client.TouchPrintReport.exe\" " + PatientModel.当前病人信息.patientId;
        //        var pathName = $"CallLishistory\\{DateTimeCore.Now.ToString("yyyyMMdd")}";
        //        var fileName = $"{pathName}\\{DateTimeCore.Now.ToString("HHmmssfff")}.bat";
        //        if (!Directory.Exists(pathName))
        //        {
        //            Directory.CreateDirectory(pathName);
        //        }
        //        FileStream fs1 = new FileStream(fileName, FileMode.Create, FileAccess.Write);//创建写入文件 
        //        StreamWriter sw = new StreamWriter(fs1);
        //        sw.WriteLine(txt);//开始写入值
        //        sw.Close();
        //        fs1.Close();
        //        var da = new Process
        //        {
        //            StartInfo = { FileName = fileName, WindowStyle = ProcessWindowStyle.Hidden, CreateNoWindow = true, UseShellExecute = false }
        //        };
        //        da.Start();
        //        //ShowAlert(true, "已发送查询并打印报告", "请耐心等待打印程序程序并打印报告\n如程序提示有报告请耐心等待您的报告打印完成并取走您的报告", 2);
        //        return;
        //    }
        //    catch (Exception ex)
        //    {
        //        Logger.Main.Info($"[DiagReportQuery][error][{ex.Message}]");
        //    }
        //}

        public override void OnEntered(NavigationContext navigationContext)
        {
            try
            {
                TimeOut = ConfigBaoAnShiYanHospital.LisTimeOut - 2;
                DoCommand(tt =>
                {
                    tt.ChangeText("请耐心等待界面提示您的报告信息，并根据提示取走您所有的报告……");
                    //tt.ChangeText("请耐心等待打印程序提示并打印完报告；请根据提示取走您所有的报告……");
                    var fileName = ConfigBaoAnShiYanHospital.LisFolderPath + "Lis.Client.TouchPrintReport.exe";
                    var da = new Process
                    {
                        StartInfo = { FileName = fileName, WindowStyle = ProcessWindowStyle.Hidden, CreateNoWindow = true, UseShellExecute = false, Arguments = PatientModel.当前病人信息.patientId }
                    };
                    da.Start();
                    System.Threading.Thread.Sleep(ConfigBaoAnShiYanHospital.LisTimeOut * 1000);
                }, false);
                return;
            }
            catch (Exception ex)
            {
                Logger.Main.Info($"[DiagReportQuery][error][{ex.Message}]");
            }
        }




        protected override void Confirm()
        {
            Navigate(A.Home);
            return;
        }
    }
}
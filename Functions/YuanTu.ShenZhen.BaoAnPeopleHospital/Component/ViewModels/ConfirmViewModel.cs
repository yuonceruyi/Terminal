using Microsoft.Practices.Unity;
using Prism.Commands;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using YuanTu.Consts;
using YuanTu.Consts.Dtos;
using YuanTu.Consts.Enums;
using YuanTu.Consts.Models;
using YuanTu.Consts.Models.Auth;
using YuanTu.Consts.Models.Payment;
using YuanTu.Consts.Services;
using YuanTu.Consts.Sounds;
using YuanTu.Core.FrameworkBase;
using YuanTu.Default.Component.Tools.Models;
using YuanTu.Core.Extension;
using YuanTu.Core.Log;

namespace YuanTu.ShenZhen.BaoAnPeopleHospital.Component.ViewModels
{
    public class ConfirmViewModel : YuanTu.Default.Component.ViewModels.ConfirmViewModel
    {
        protected static bool IsRunningNoPayConfirm = false;
        protected override void NoPayConfirm()
        {
            var patientInfo = PatientModel.Res病人信息查询.data[PatientModel.PatientInfoIndex];
            YuanTu.Core.Log.Logger.Main.Info($"ConfirmViewModel.NoPayConfirm病人{DateTimeCore.Now.ToString("yyyyMMddHHmmssfff")}。姓名：{patientInfo.name}-登记号：{patientInfo.patientId}IsRunningNoPayConfirm:{IsRunningNoPayConfirm}");
            if (IsRunningNoPayConfirm) return;
            IsRunningNoPayConfirm = true;

            var block = new TextBlock() { TextAlignment = TextAlignment.Center, FontSize = 17 };
            block.Inlines.Add("\r\n姓名:");
            block.Inlines.Add(new TextBlock() { Text = PatientModel.当前病人信息.name, Foreground = new SolidColorBrush(Colors.Red), FontWeight = FontWeights.Bold, FontSize = 20 });
            block.Inlines.Add("\r\n卡号:");
            block.Inlines.Add(new TextBlock() { Text = CardModel.CardNo, Foreground = new SolidColorBrush(Colors.Coral), FontWeight = FontWeights.Bold, FontSize = 20 });
            block.Inlines.Add("\r\n费用:");
            block.Inlines.Add(new TextBlock() { Text = PaymentModel.Total.In元(), Foreground = new SolidColorBrush(Colors.Coral), FontWeight = FontWeights.Bold, FontSize = 20 });
            block.Inlines.Add("\r\n 确定缴费后将从社保卡上扣取相应的金额！");
            block.Inlines.Add($"\r\n执行{ChoiceModel.Business}，\r\n确认继续操作吗？");

            ShowConfirm("提示信息", block, cb =>
            {
                if (cb)
                {
                    PaymentModel.ConfirmAction?.BeginInvoke(cp =>
                    {
                        var rest = PaymentModel.ConfirmAction?.EndInvoke(cp);
                        if (rest?.IsSuccess ?? false)
                            ChangeNavigationContent("");
                    }, null);
                    IsRunningNoPayConfirm = false;
                    return;
                }
                else
                {
                    IsRunningNoPayConfirm = false;
                    return;
                };
            }, 60, ConfirmExModel.Build("确定缴费", "取消缴费"));
        }
    }
}
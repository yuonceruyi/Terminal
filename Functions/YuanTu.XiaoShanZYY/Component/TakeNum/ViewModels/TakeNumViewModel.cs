using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.Practices.Unity;
using Prism.Commands;
using Prism.Regions;
using YuanTu.Consts;
using YuanTu.Consts.FrameworkBase;
using YuanTu.Consts.Models;
using YuanTu.Consts.Models.Payment;
using YuanTu.XiaoShanZYY.Component.TakeNum.Models;

namespace YuanTu.XiaoShanZYY.Component.TakeNum.ViewModels
{
    public class TakeNumViewModel : Default.Component.TakeNum.ViewModels.TakeNumViewModel
    {
        public TakeNumViewModel()
        {
            CancelCommand = new DelegateCommand(CancelAction, CanCancel);
            ConfirmCommand = new DelegateCommand(ConfirmAction, CanConfirm);
        }

        [Dependency]
        public ITakeNumModel TakeNum { get; set; }

        [Dependency]
        public ITakeNumService TakeNumService { get; set; }

        private bool IsToday { get; set; }

        public override void OnEntered(NavigationContext navigationContext)
        {
            ChangeNavigationContent("");
            var list = new List<PayInfoItem>();
            var info = TakeNum.Res挂号预结算;
            list.Add(new PayInfoItem("科室名称:", info.科室名称));
            list.Add(new PayInfoItem("医生名称:", info.医生名称));
            list.Add(new PayInfoItem("挂号日期:", info.挂号日期));
            list.Add(new PayInfoItem("候诊时间:", info.挂号序号));
            list.Add(new PayInfoItem("诊疗费:", info.诊疗费));
            list.Add(new PayInfoItem("诊疗费(加收):", info.诊疗费_加收));
            DateTime date;
            if (DateTime.TryParse(info.挂号日期, out date))
                IsToday = date == DateTimeCore.Today;

            List = list;
            ConfirmCommand.RaiseCanExecuteChanged();
            CancelCommand.RaiseCanExecuteChanged();
        }

        protected override bool CanCancel()
        {
            return true;
        }

        protected override bool CanConfirm()
        {
            return IsToday;
        }

        protected override void CancelAction()
        {
            var record = TakeNum.Res挂号预结算;
            var textblock = new TextBlock
            {
                TextWrapping = TextWrapping.Wrap,
                TextAlignment = TextAlignment.Center,
                Margin = new Thickness(0, 15, 0, 0)
            };
            textblock.Inlines.Add("\r\n您确定要取消");
            textblock.Inlines.Add(new TextBlock
            {
                Text = $"{record.挂号日期} {record.候诊时间} {record.科室名称} {record.医生名称}",
                Foreground = new SolidColorBrush(Color.FromRgb(255, 0, 0))
            });
            textblock.Inlines.Add("\r\n的预约吗？\r\n\r\n\r\n\r\n");
            ShowConfirm("友好提醒", textblock, b =>
            {
                if (!b) return;
                DoCommand(lp =>
                {
                    lp.ChangeText("正在进行取消预约，请稍候...");

                    var result = TakeNumService.取消();
                    if (result.IsSuccess)
                    {
                        ShowAlert(true, "取消预约", "您已成功取消预约", extend: new AlertExModel()
                        {
                            HideCallback = (_) => Navigate(A.Home)
                        });
                    }
                    else
                    {
                        ShowAlert(false, "取消预约", $"取消预约失败\n{result.Message}");
                    }
                });
            }, 60, ConfirmExModel.Build("是", "否", true));
        }

        protected override void ConfirmAction()
        {
            ChangeNavigationContent(TakeNum.Res挂号预结算.科室名称);
            Next();
        }
    }
}
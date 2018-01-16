using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Threading;
using Prism.Commands;
using YuanTu.Consts;
using YuanTu.VirtualHospital.Component.InfoQuery.ViewModels.SubViews;

namespace YuanTu.VirtualHospital.Component.InfoQuery.ViewModels
{
    public class InDailyDetailViewModel:YuanTu.Default.Component.InfoQuery.ViewModels.InDailyDetailViewModel
    {
        public ICommand ConfirmCommand { get; set; }

        public InDailyDetailViewModel()
        {
            ConfirmCommand=new DelegateCommand(Confirm);
        }

        private void Confirm()
        {
            DoCommand(lp =>
            {
                lp.ChangeText("正在准备打印机，请等待...");
                Thread.Sleep(1500);
                lp.ChangeText("正在打印，请等待...");
                View.Dispatcher.BeginInvoke(DispatcherPriority.ContextIdle, (Action) (() =>
                {
                    (new 出院清单()).PrintFromData();
                }));
                Thread.Sleep(3000);
                lp.ChangeText("打印完毕，请取走您的清单。");
                Navigate(A.Home);
            });
        }
    }
}

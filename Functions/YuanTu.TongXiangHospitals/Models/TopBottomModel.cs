using System;
using System.Diagnostics;
using Prism.Events;
using YuanTu.Consts;
using YuanTu.Core.Log;

namespace YuanTu.TongXiangHospitals.Models
{
    public class TopBottomModel : Core.Models.TopBottomModel
    {
        public TopBottomModel(IEventAggregator eventAggregator) : base(eventAggregator)
        {
        }

        protected override void CheckSystemShutDown(DateTime now)
        {
            if (!FrameworkConst.AutoShutdownEnable || Inconfirm)
                return;
            if (Laydown++ < 60)
                return;
            Laydown = 0;
            if (now < FrameworkConst.AutoShutdownTime.AddMinutes(Shutdownretry))
                return;
            var poweroffAct = new Action(() =>
            {
                Logger.Main.Info("[系统关闭]达到设定时间，系统关闭");
                Process.Start("shutdown.exe", "-s -t 0");
            });

            var engine = NavigationEngine;
            if (!engine.IsHome(engine.State)) // 不在主页不自动关机
                Shutdownretry++;
            else
                poweroffAct?.Invoke();
        }
    }
}
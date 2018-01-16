using System;
using System.Diagnostics;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using Prism.Commands;
using YuanTu.Consts;
using YuanTu.Consts.Services;
using YuanTu.Core.FrameworkBase;
using YuanTu.Core.Log;
using YuanTu.Core.Navigating;

namespace YuanTu.Default.House.Part.ViewModels
{
    public class TopBarViewModel : ViewModelBase
    {
        public override string Title => "页首";
        public TopBarViewModel()
        {
            SuperDoubleClick = new DelegateCommand(SuperClickDo);
            _timer = new DispatcherTimer();
            _timer.Tick += Timer_Tick;
            _timer.Interval = new TimeSpan(0, 0, 1);
            _timer.Start();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            TimeNow = DateTimeCore.Now.ToString("HH:mm");
            DateNow = DateTimeCore.Now.ToString("yyyy-MM-dd");
            CheckSystemShutDown();
        }

        private readonly DispatcherTimer _timer;

        protected string _dateNow { get; set; }

        public string DateNow
        {
            get
            {
                return _dateNow;
            }
            set
            {
                _dateNow = value;
                OnPropertyChanged();
            }
        }
        protected string _timeNow { get; set; }

        public string TimeNow
        {
            get
            {
                return _timeNow;
            }
            set
            {
                _timeNow = value;
                OnPropertyChanged();
            }
        }

        public ImageSource MainLogo { get; set; } = GetInstance<IResourceEngine>().GetImageResource("MainLogo");

        public string MainTitle { get; set; } = "医疗自助服务系统";

        public ICommand SuperDoubleClick { get; set; }

        protected virtual void SuperClickDo()
        {
            var engine = NavigationEngine;
            if (engine.IsHome(engine.State) || engine.State == A.Maintenance)
            {
                Navigate(A.AdminPart);
            }
            //Logger.Main.Info($"[系统关闭]双击系统关闭！");
            //Application.Current.Shutdown();
        }

        protected int Shutdownretry = 0;
        protected bool Inconfirm = false;
        protected int Laydown = 0;
        protected virtual void CheckSystemShutDown()
        {
            if (!FrameworkConst.AutoShutdownEnable || Inconfirm)
                return;
            if (Laydown++ < 60)
                return;
            Laydown = 0;
            if (DateTimeCore.Now < FrameworkConst.AutoShutdownTime.AddMinutes(Shutdownretry * 5))
                return;
            var poweroffAct = new Action(() =>
              {
                  Logger.Main.Info("[系统关闭]达到设定时间，系统关闭");
                  Process.Start("shutdown.exe", "-s -t 0");
              });

            var engine = NavigationEngine;
            if (engine.IsHome(engine.State))
            {
                poweroffAct.Invoke();
                return;
            }

            Inconfirm = true;
            ShowConfirm("自动关机", "点击确定，机器将会自动关机!", cp =>
            {
                if (cp)
                    poweroffAct.Invoke();
                else
                    Shutdownretry++;
                Inconfirm = false;
            });
        }
    }
}
using System;
using System.Threading;
using System.Windows;
using YuanTu.Consts;

namespace Terminal
{
    /// <summary>
    ///     App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            LimitOneTerminal();
            var bootstrap = new Bootstrapper();
            bootstrap.Run();
        }
        private static void LimitOneTerminal()
        {
            if (CommandLineSwitches.AllowMulitipleInstances)
                return;
            FrameworkConst.MutexLock = new Mutex(true, "TerminalMutex", out var caCreatedNew);
            if (!caCreatedNew)
            {
                YuanTu.Core.Log.Logger.Main.Info("[系统异常]:已经存在Terminal");
                Environment.Exit(-1);
            }
        }

    }
}
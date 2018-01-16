using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media;
using Microsoft.Practices.ServiceLocation;
using YuanTu.Consts;
using YuanTu.Consts.Models.Auth;
using YuanTu.Consts.Services;
using YuanTu.Consts.UserControls;
using YuanTu.Core.DB;
using YuanTu.Core.Extension;
using YuanTu.Core.FrameworkBase;
using YuanTu.Core.Log;
using YuanTu.Core.Navigating;

namespace YuanTu.Default
{
    public class MainWindowHelper
    {
        private readonly int BeepDur;
        private readonly int BeepFre;

        private readonly bool EnableBeep;
        private readonly Window _window;
        public MainWindowHelper(Window window)
        {
            _window = window;
            window.SourceInitialized += MainWindow_SourceInitialized;
            var cfg = ServiceLocator.Current.GetInstance<IConfigurationManager>();
            EnableBeep = cfg.GetValueInt("ButtonFeedBack:Enable", 0) == 1;
            if (EnableBeep)
            {
                BeepFre = cfg.GetValueInt("ButtonFeedBack:BeepFrequency", 0);
                BeepDur = cfg.GetValueInt("ButtonFeedBack:BeepDuration", 0);
            }

            var width = cfg.GetValueInt("Screen:Width");
            var height = cfg.GetValueInt("Screen:Height");
            if (width > 0 && height > 0)
            {
                window.Width = width;
                window.Height = height;
            }
        }

        private void MainWindow_SourceInitialized(object sender, EventArgs e)
        {
            var hwnd = new WindowInteropHelper(_window).Handle;
            HwndSource.FromHwnd(hwnd).AddHook(WndProc);
        }

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg != 513 && msg != 516)
                return hwnd;
            try
            {
                var cardInfo = ServiceLocator.Current.GetInstance<ICardModel>();
                var patientInfo = ServiceLocator.Current.GetInstance<IPatientModel>();
                var engine = ServiceLocator.Current.GetInstance<NavigationEngine>();

                var w32Mouse = new Win32Point();
                GetCursorPos(ref w32Mouse);
                var realpt = _window.PointFromScreen(w32Mouse.ToPoint());
                var cts = _window.InputHitTest(realpt) as FrameworkElement;
                var btn = GetClickControl<Button>(cts); //点击的按钮
                var sbtn = btn as SimpleButton;
                var ctr = GetClickControl<ViewsBase>(cts); //所在页面

                var workViewType = engine.Children.GetType(engine.Context, engine.State);
                var workvmType = engine.GetViewModelTypeByView(workViewType);
                //工作区的viewmodel
                var vmbase = ServiceLocator.Current.GetInstance<ViewModelBase>(workvmType.FullName);
                var vm = ctr?.DataContext as ViewModelBase;
                vm?.StartTimer();
                var realContent =
                ((sbtn == null
                     ? btn?.Content
                     : (sbtn.TagString.IsNullOrWhiteSpace() ? sbtn.Content : sbtn.TagString)) ?? "").ToString();
                var title = vm == vmbase ? (vm?.Title) : ($"{vm?.Title}({vmbase?.Title})");


                var tapInfo = new PointTouchTable
                {
                    ViewName = title ?? cts?.GetType().FullName,
                    ButtonContent = realContent,
                    CardNo = cardInfo.CardNo,
                    PatientId = patientInfo.当前病人信息?.patientId,
                    PointX = realpt.X,
                    PointY = realpt.Y
                };
#if DEBUG
                Console.WriteLine(tapInfo.ToJsonString());
#endif
                Task.Run(() => DBManager.Insert(FrameworkConst.DataMiningDataBasePath, tapInfo));
                if (btn != null && EnableBeep)
                    Task.Run(() => { Console.Beep(BeepFre, BeepDur); });
            }
            catch (Exception ex)
            {
                Logger.Main.Error($"[系统打点]打点时出现异常，详情:{ex.Message}\r\n{ex.StackTrace}");
            }

            return hwnd;
        }

        private T GetClickControl<T>(DependencyObject element) where T : class
        {
            var target = element;

            if (target == null)
                return default(T);
            if (target is T)
                return target as T;

            //如果Button的Enabled为false，只能从子集找
            if (VisualTreeHelper.GetChildrenCount(target) > 0 && VisualTreeHelper.GetChild(target, 0) is T)
                return VisualTreeHelper.GetChild(target, 0) as T;

            //按照可视化树往上找
            target = VisualTreeHelper.GetParent(target);

            return GetClickControl<T>(target);
        }

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool GetCursorPos(ref Win32Point pt);

        [StructLayout(LayoutKind.Sequential)]
        internal struct Win32Point
        {
            public int X;
            public int Y;

            public Point ToPoint()
            {
                return new Point(X, Y);
            }
        }
    }
}
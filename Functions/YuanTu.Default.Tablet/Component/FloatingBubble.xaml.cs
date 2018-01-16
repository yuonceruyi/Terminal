using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Threading;
using Microsoft.Practices.ServiceLocation;
using Prism.Commands;
using Prism.Mvvm;
using YuanTu.Consts;
using YuanTu.Consts.FrameworkBase;
using YuanTu.Consts.Services;
using YuanTu.Core.Navigating;
using YuanTu.Core.Systems.Ini;
using MouseEventArgs = System.Windows.Input.MouseEventArgs;
using Point = System.Windows.Point;

namespace YuanTu.Default.Tablet.Component
{
    /// <summary>
    ///     FloatingBubble.xaml 的交互逻辑
    /// </summary>
    public partial class FloatingBubble : Window
    {
        private readonly IniFile _iniFile;
        private readonly WindowPosHelper _mainPos;
        private readonly DispatcherTimer _screenBoundDispatcherTimer;

        private bool _dragged;
        private bool _dragging;
        private Point _mousePosition = new Point(-1, -1);

        private int _screenBoundCountDown;
        private Point _windowPosition = new Point(-1, -1);

        public FloatingBubble()
        {
            InitializeComponent();

            _screenBoundDispatcherTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(50)
            };
            _screenBoundDispatcherTimer.Tick += ScreenBoundDispatcherTimerOnTick;
            _iniFile = new IniFile("Positions.ini", true);
            _mainPos = new WindowPosHelper("Main", _iniFile);
            _mainPos.UpdateWindow(this);
        }

        private void ScreenBoundDispatcherTimerOnTick(object sender, EventArgs eventArgs)
        {
            _screenBoundCountDown--;
            if (_screenBoundCountDown > 0)
                return;
            _screenBoundDispatcherTimer.Stop();
            ScreenBoundsHelper.Coerce(this);
        }

        private void MainWindow_OnLocationChanged(object sender, EventArgs e)
        {
            _screenBoundCountDown = 5;
            _screenBoundDispatcherTimer.Start();
        }

        private void FloatingBubble_OnPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _windowPosition = new Point(Left, Top);
            _dragged = false;
            _dragging = true;
            GetCursorPos(out _mousePosition);
        }

        private void FloatingBubble_OnPreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (_dragged)
            {
                _mainPos.UpdatePosition(this);
                e.Handled = true;
            }
            var result = InputHitTest(e.GetPosition(this));
            if (result == null)
                e.Handled = true;
            _dragged = false;
            _dragging = false;
        }

        private void FloatingBubble_OnPreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton != MouseButtonState.Pressed || !_dragging)
                return;
            Point newMousePosition;
            GetCursorPos(out newMousePosition);
            if (newMousePosition == _mousePosition)
                return;
            _dragged = true;
            var newWindowPosition = newMousePosition - _mousePosition + _windowPosition;
            Left = newWindowPosition.X;
            Top = newWindowPosition.Y;
        }

        private static bool GetCursorPos(out Point point)
        {
            POINT pt;
            var ret = GetCursorPos(out pt);
            point = new Point(pt.X, pt.Y);
            return ret;
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern bool GetCursorPos(out POINT pt);

        [StructLayout(LayoutKind.Sequential)]
        private struct POINT
        {
            public readonly int X;
            public readonly int Y;

            public POINT(int x, int y)
            {
                X = x;
                Y = y;
            }
        }
    }

    public class ScreenBoundsHelper
    {
        public static Rectangle GetCurrentScreen(Window w)
        {
            if (Screen.AllScreens.Length > 1)
                foreach (var screen in Screen.AllScreens)
                {
                    var rect = screen.Bounds;
                    if (w.Left >= rect.Left && w.Top >= rect.Top && w.Left < rect.Right && w.Top < rect.Bottom)
                        return rect;
                }
            return Screen.PrimaryScreen.Bounds;
        }

        public static void Coerce(Window w)
        {
            var rect = GetCurrentScreen(w);
            if (w.Left + w.Width > rect.Right)
                w.Left = rect.Right - w.Width;
            if (w.Top + w.Height > rect.Bottom)
                w.Top = rect.Bottom - w.Height;

            if (w.Left < rect.Left)
                w.Left = rect.Left;
            if (w.Top < rect.Top)
                w.Top = rect.Top;
        }
    }

    public class FloatingBubbleViewModel : BindableBase
    {
        private readonly Uri _uriOff;
        private readonly Uri _uriOn;
        private bool _firstTime = true;

        public FloatingBubbleViewModel()
        {
            Command = new DelegateCommand(Do);

            var resourceEngine = GetInstance<IResourceEngine>();
            _uriOn = resourceEngine.GetImageResourceUri("悬浮窗_On");
            _uriOff = resourceEngine.GetImageResourceUri("悬浮窗_Off");

            _image = _uriOn;
        }

        private T GetInstance<T>()
        {
            return ServiceLocator.Current.GetInstance<T>();
        }

        private void Do()
        {
            var shell = GetInstance<IShell>();
            var window = shell as Window;
            if (window == null)
                return;
            window.Dispatcher.Invoke(() =>
            {
                if (!window.IsLoaded)
                    return;

                GetInstance<NavigationEngine>().State = A.Home;
                if (_firstTime)
                {
                    _firstTime = false;
                    window.Visibility = Visibility.Hidden;
                    window.Visibility = Visibility.Visible;
                }
                else if (window.Visibility != Visibility.Visible)
                {
                    window.Visibility = Visibility.Visible;
                    window.Activate();
                }
                else
                {
                    window.Visibility = Visibility.Hidden;
                }
            });
            Image = window.Visibility == Visibility.Visible ? _uriOff : _uriOn;
        }

        #region Bindings

        public DelegateCommand Command { get; set; }

        private Uri _image;

        public Uri Image
        {
            get { return _image; }
            set
            {
                _image = value;
                OnPropertyChanged();
            }
        }

        #endregion Bindings
    }
}
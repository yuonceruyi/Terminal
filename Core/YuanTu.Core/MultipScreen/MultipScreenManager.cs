using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace YuanTu.Core.MultipScreen
{
    public static class MultipScreenManager
    {
        #region Property

        internal static Screen[] AllScreens => Screen.AllScreens;

        internal static Screen PrimaryScreen => Screen.PrimaryScreen;

        internal static IEnumerable<Screen> MinorScreens
        {
            get
            {
                return Screen.AllScreens.Where(o => o.Primary == false);
            }
        }

        internal static Screen FirstMinorScreen => MinorScreens.FirstOrDefault();

        #endregion Property

        #region Method

        public static void ShowInScreen(this System.Windows.Window window)
        {
            SetScreen(window);
            window.Show();
        }
        public static void ShowDialogInScreen(this System.Windows.Window window)
        {
            SetScreen(window);
            window.ShowDialog();
        }

        private static void SetScreen(System.Windows.Window window)
        {
            var attribute = window.GetType()
                .GetCustomAttributes(typeof(MultipScreenAttribute), false)
                .FirstOrDefault(o => o is MultipScreenAttribute)
                as MultipScreenAttribute;

            var index = attribute?.Index ?? 0;
            var location = attribute?.Location ?? WindowStartupLocationInScreen.CenterScreen;
            var fullScreen = attribute?.FullScreen ?? false;
            var ingoreOperation = attribute?.IngoreMinorScreenError ?? false;

            Screen screen;

            if (index == 1 && FirstMinorScreen != null)
                screen = FirstMinorScreen;
            else if (index > 1 && index < MinorScreens.Count())
                screen = MinorScreens.ElementAt(index);
            else if (index > 0 && index >= MinorScreens.Count() && ingoreOperation)
                return;
            else
                screen = PrimaryScreen;

            if (fullScreen)
            {
                SetManual(window, screen);
                SetFullScreen(window, screen);
            }
            else
            {
                switch (location)
                {
                    case WindowStartupLocationInScreen.CenterScreen:
                        SetCenter(window, screen);
                        break;
                    case WindowStartupLocationInScreen.Manual:
                        SetManual(window, screen);
                        break;
                }
            }
        }

        private static void SetCenter(System.Windows.Window win, Screen screen)
        {
            win.Top = screen.WorkingArea.Y + (screen.WorkingArea.Height - win.Height) / 2;
            win.Left = screen.WorkingArea.X + (screen.WorkingArea.Width - win.Width) / 2;
        }

        private static void SetManual(System.Windows.Window win, Screen screen)
        {
            win.Top = screen.WorkingArea.Y;
            win.Left = screen.WorkingArea.X;
        }

        private static void SetFullScreen(System.Windows.Window win, Screen screen)
        {
            win.Width = screen.WorkingArea.Width;
            win.Height = screen.WorkingArea.Height;
        }

        #endregion Method
    }
}

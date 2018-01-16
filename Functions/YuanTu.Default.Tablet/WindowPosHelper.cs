using System;
using System.Windows;
using YuanTu.Core.Systems.Ini;

namespace YuanTu.Default.Tablet
{
    internal class WindowPosHelper
    {
        private bool sync;

        private int X;
        private int Y;

        public WindowPosHelper(string name, IniFile iniFile)
        {
            Name = name;
            IniFile = iniFile;
        }

        public string Name { get; }
        public IniFile IniFile { get; }

        public void UpdatePosition(Window window)
        {
            var left = Convert.ToInt32(window.Left);
            var top = Convert.ToInt32(window.Top);
            if (X != left)
            {
                X = left;
                IniFile.IniWriteValue(Name, "X", X.ToString());
            }
            if (Y != top)
            {
                Y = top;
                IniFile.IniWriteValue(Name, "Y", Y.ToString());
            }
            sync = true;
        }

        public void UpdateWindow(Window window)
        {
            if (!sync)
            {
                var xString = IniFile.IniReadValue(Name, "X");
                var yString = IniFile.IniReadValue(Name, "Y");
                if (!int.TryParse(xString, out X))
                    return;
                if (!int.TryParse(yString, out Y))
                    return;
            }
            window.Left = X;
            window.Top = Y;
        }
    }
}
using System;

namespace YuanTu.Core.MultipScreen
{
    [AttributeUsage(AttributeTargets.Class)]
    public class MultipScreenAttribute : Attribute
    {
        /// <summary>
        ///     在窗体初始化显示的位置
        /// </summary>
        public WindowStartupLocationInScreen Location { get; set; } = WindowStartupLocationInScreen.CenterScreen;

        /// <summary>
        ///     屏幕索引， 0为主屏，1+为次屏
        /// </summary>
        public int Index { get; set; }

        /// <summary>
        ///     是否全屏窗口
        /// </summary>
        public bool FullScreen { get; set; }

        /// <summary>
        ///     当任何指定次屏没有找到时，如果该值为TRUE，则忽略这个页面的显示，否则将显示在主屏
        /// </summary>
        public bool IngoreMinorScreenError { get; set; }
    }
}
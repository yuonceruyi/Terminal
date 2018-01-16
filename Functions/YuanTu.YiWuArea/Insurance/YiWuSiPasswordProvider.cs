using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;
using YuanTu.Consts.FrameworkBase;
using YuanTu.Core.Log;
using YuanTu.Core.Systems;
using YuanTu.YiWuArea.Dialog;

namespace YuanTu.YiWuArea.Insurance
{
    public static class YiWuSiPasswordProvider
    {
      //  private static bool _monitorRunning = false;
        public const string PasswordWinTitle = "输入密码";
        public static string[] ErrorWinTitles = new[] { "提示" };

        private static Action _findCallback = null;

        static YiWuSiPasswordProvider()
        {
            RunLoop();
        }

        /// <summary>
        /// 关闭特定提示的错误框，遇到密码输入框时，触发回调
        /// </summary>
        /// <param name="findcbk"></param>
        public static void StartMonitor(Action findcbk)
        {
            _findCallback = findcbk;
            //_monitorRunning = true;
            //(new Thread(_ =>
            //{
            //    while (_monitorRunning)
            //    {
            //        Thread.Sleep(200);
            //        foreach (var errorWinTitle in ErrorWinTitles)
            //        {
            //            var winPtr = WindowHelper.FindWindow(null, errorWinTitle);
            //            if (winPtr == IntPtr.Zero)
            //            {
            //                continue;
            //            }
            //            WindowHelper.PostMessage(winPtr, (uint)(WindowHelper.WindowMessage.CLOSE), IntPtr.Zero, IntPtr.Zero);
            //        }
            //        //获取密码框
            //        var pwdPtr = WindowHelper.FindWindow(null, PasswordWinTitle);
            //        if (pwdPtr == IntPtr.Zero)
            //        {
            //            continue;
            //        }
            //        WindowHelper.SetWindowPos(pwdPtr, WindowHelper.WindowFlags.Bottom, 0, 1024, 0, 0,
            //            //WindowHelper.SetWindowPosFlags.IgnoreMove |
            //            WindowHelper.SetWindowPosFlags.IgnoreResize);

            //        findcbk?.Invoke();
            //    }
            //})
            //{ IsBackground = true }).Start();
        }

        public static void StopMonitor()
        {
            _findCallback = null;
           // _monitorRunning = false;
        }


        private static void RunLoop()
        {
            (new Thread(_ =>
            {
                while (true)
                {
                    try
                    {
                        var isstop = _findCallback == null;
                        Thread.Sleep(isstop ? 1000 : 200);
                        foreach (var errorWinTitle in ErrorWinTitles)
                        {
                            var winPtr = WindowHelper.FindWindow(null, errorWinTitle);
                            if (winPtr == IntPtr.Zero)
                            {
                                continue;
                            }
                            WindowHelper.PostMessage(winPtr, (uint)(WindowHelper.WindowMessage.CLOSE), IntPtr.Zero, IntPtr.Zero);
                        }
                        //获取密码框
                        var pwdPtr = WindowHelper.FindWindow(null, PasswordWinTitle);
                        if (pwdPtr == IntPtr.Zero)
                        {
                            continue;
                        }
                        WindowHelper.SetWindowPos(pwdPtr, WindowHelper.WindowFlags.Bottom, 0, 1024, 0, 0,
                            //WindowHelper.SetWindowPosFlags.IgnoreMove |
                            WindowHelper.SetWindowPosFlags.IgnoreResize);
                        if (isstop)
                        {
                            CloseUnexceptedWindow();
                        }
                        _findCallback?.Invoke();
                    }
                    catch (Exception ex)
                    {
                        Logger.Main.Error($"[社保密码框]搜索时发送异常{ex.Message}\r\n{ex.StackTrace}");
                    }
                }
            })
            { IsBackground = true }).Start();
        }

        private static void CloseUnexceptedWindow()
        {
            var winPtr = WindowHelper.FindWindow(null, PasswordWinTitle);
            var btn = WindowHelper.FindWindowEx(winPtr, IntPtr.Zero, "Button", "取消");
            if (btn != IntPtr.Zero)
            {
                WindowHelper.PostMessage(btn, (uint)(WindowHelper.WindowMessage.CLICK), IntPtr.Zero, IntPtr.Zero);
            }
            else
            {
                WindowHelper.PostMessage(winPtr, (uint)(WindowHelper.WindowMessage.CLOSE), IntPtr.Zero, IntPtr.Zero);
            }
        }

    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using YuanTu.Core.Log;
using YuanTu.Core.Systems;

namespace YuanTu.FuYangRMYY.Managers
{
    public static class SiOperatorManager
    {
        private const string signFileDir = "C:\\keyboard";
        private const string signFilePath = "C:\\keyboard\\德卡密码键盘音频播放标识.txt";
        public static string[] ErrorWinTitles = new[] { "医保中心报错提示:","His医保端报错提示:" };
        private static Dictionary<string,Action<IntPtr>>_extrActions=new Dictionary<string, Action<IntPtr>>()
        {
            ["省市医保门诊结算"]= Close省市医保门诊结算,
            ["校验卡密码失败"] = Close校验卡密码失败,
        };
        private static Action _callback = null;
        static SiOperatorManager()
        {
            Loop();
        }

        private static void Loop()
        {
           (new Thread(_ =>
           {
               while (true)
               {
                   var haveCallbak = _callback != null;
                   Thread.Sleep(haveCallbak?200:1500);
                   try
                   {
                       foreach (var errorWinTitle in ErrorWinTitles)
                       {
                           var winPtr = WindowHelper.FindWindow(null, errorWinTitle);
                           if (winPtr == IntPtr.Zero)
                           {
                               continue;
                           }
                           WindowHelper.PostMessage(winPtr, (uint)(WindowHelper.WindowMessage.CLOSE), IntPtr.Zero, IntPtr.Zero);
                       }
                       foreach (var extrAction in _extrActions)
                       {
                           var winPtr = WindowHelper.FindWindow(null, extrAction.Key);
                           if (winPtr == IntPtr.Zero)
                           {
                               continue;
                           }
                           extrAction.Value.Invoke(winPtr);
                       }
                       //if (File.Exists(signFilePath))
                       //{
                       //    try
                       //    {
                       //        File.Delete(signFilePath);
                       //        _callback?.Invoke();
                       //    }
                       //    catch (Exception e)
                       //    {
                       //        Logger.Main.Error($"[阜阳社保]删除临时文件异常，稍后再试。{e}");
                       //    }
                       //}
                       if (Directory.Exists(signFileDir))
                       {
                           try
                           {
                               Directory.Delete(signFileDir,true);
                               _callback?.Invoke();
                           }
                           catch (Exception e)
                           {
                               Logger.Main.Error($"[阜阳社保]删除临时文件夹异常，稍后再试。{e}");
                           }
                       }
                   }
                   catch (Exception e)
                   {
                       Logger.Main.Error($"[阜阳社保]循环检测社保出现异常，异常详情:{e.Message}");
                   }
               }
           }){IsBackground = true,Name = "社保检测"}).Start();
        }

        public static void StartMonitor(Action callback)
        {
            _callback = callback;
        }

        public static void StopMonitor()
        {
            _callback = null;
            if (Directory.Exists(signFileDir))
            {
                try
                {
                    Directory.Delete(signFileDir, true);
                   
                }
                catch (Exception e)
                {
                    Logger.Main.Error($"[阜阳社保]删除临时文件夹异常，稍后再试.。{e}");
                }
            }
        }


        private static void Close省市医保门诊结算(IntPtr mainPtr)
        {
            var btn = WindowHelper.FindWindowEx(mainPtr, IntPtr.Zero, "WindowsForms10.BUTTON.app.0.141b42a_r11_ad1", "医保结算");
            if (btn != IntPtr.Zero)
            {
                WindowHelper.PostMessage(btn, (uint)(WindowHelper.WindowMessage.CLICK), IntPtr.Zero, IntPtr.Zero);
            }
            //else
            //{
            //    WindowHelper.PostMessage(mainPtr, (uint)(WindowHelper.WindowMessage.CLOSE), IntPtr.Zero, IntPtr.Zero);
            //}
        }

        private static void Close校验卡密码失败(IntPtr mainPtr)
        {
            var btn = WindowHelper.FindWindowEx(mainPtr, IntPtr.Zero, "Button", "否(&N)");
            if (btn != IntPtr.Zero)
            {
                WindowHelper.PostMessage(btn, (uint)(WindowHelper.WindowMessage.CLICK), IntPtr.Zero, IntPtr.Zero);
            }
            //else
            //{
            //    WindowHelper.PostMessage(mainPtr, (uint)(WindowHelper.WindowMessage.CLOSE), IntPtr.Zero, IntPtr.Zero);
            //}
        }
    }

   
}

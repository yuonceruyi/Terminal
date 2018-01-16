using System;
using System.Runtime.InteropServices;
using System.Text;

namespace YuanTu.QDArea.QingDaoSiPay.Common
{
    class ExternFun
    {
	    /// <summary>
        /// 动态库接口方法
        /// </summary>
        /// <returns></returns>
        [DllImport("SendRcv.dll")]
        public static extern ulong SendRcv(string pDatainput, StringBuilder pDataOutput);
		
        /// <summary>
        /// 动态库接口方法
        /// </summary>
        /// <returns></returns>
        [DllImport("SendPos.dll")]
        public static extern ulong SendPos(string pDatainput, StringBuilder pDataOutput);
        /// <summary>
        /// 获取社保卡上的身份证和姓名函数。
        ///函数名：ReadCard
        ///参数1：需要获取的身份证，类型为字符串，长度19
        ///参数2：需要获取的姓名，类型为字符串，长度31
        ///返回值：返回读取结果，0表示读取成功
        /// </summary>
        /// <returns></returns>
        [DllImport("SendPos.dll")]
        public static extern int ReadCard(StringBuilder idenno, StringBuilder name);

        #region 钩子
        public delegate int HookProc(int nCode, Int32 wParam, IntPtr lParam);
        // 安装钩子
        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public static extern int SetWindowsHookEx(int idHook, HookProc lpfn, IntPtr hInstance, int threadId);

        // 卸载钩子
        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public static extern bool UnhookWindowsHookEx(int idHook);

        // 继续下一个钩子
        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public static extern int CallNextHookEx(int idHook, int nCode, Int32 wParam, IntPtr lParam);

        // 取得当前线程编号    
        [DllImport("kernel32.dll")]
        public static extern int GetCurrentThreadId();
        #endregion 钩子
    }
}

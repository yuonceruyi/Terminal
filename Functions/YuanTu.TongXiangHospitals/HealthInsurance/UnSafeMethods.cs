using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using Newtonsoft.Json;
using YuanTu.Consts;
using YuanTu.Core.Log;
using YuanTu.Devices.UnionPay;

namespace YuanTu.TongXiangHospitals.HealthInsurance
{
    public class UnSafeMethods
    {
        #region[网新恩普社保接口]

        //private const string DllPathSiInterface = "External\\TongXiang\\BargaingApplyV3_01038.dll";
        private const string DllPathSiInterface = "BargaingApplyV3_01038.dll";

        #region DLL Import

        [DllImport(DllPathSiInterface, EntryPoint = "f_UserBargaingInit", CharSet = CharSet.Ansi,
        CallingConvention = CallingConvention.StdCall)]
        public static extern int f_UserBargaingInit(string data1, StringBuilder retMsg, string data2);

        [DllImport(DllPathSiInterface, EntryPoint = "f_UserBargaingClose", CharSet = CharSet.Ansi,
            CallingConvention = CallingConvention.StdCall)]
        public static extern int f_UserBargaingClose(string data1, StringBuilder retMsg, string data2);

        [DllImport(DllPathSiInterface, EntryPoint = "f_UserBargaingApply", CharSet = CharSet.Ansi,
            CallingConvention = CallingConvention.StdCall)]
        public static extern int f_UserBargaingApply(int code, double no, string data, StringBuilder retMsg, string data2);


        #endregion DLL Import

        #endregion

        #region 读医保卡接口
        private const string DllPathSiReader= "ICCInter_JX_SB.dll";
        //private const string DllPathSiReader = "External\\TongXiang\\ICCInter_JX_SB.dll";
        [DllImport(DllPathSiReader, EntryPoint = "IC_ReadCardInfo_NoPin", CharSet = CharSet.Ansi)]
        public static extern int IC_ReadCardInfo_NoPin(StringBuilder sb);
        #endregion


        public static void DoNothing()
        {

        }
        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern IntPtr LoadLibrary(string libraryName);

        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern bool FreeLibrary(IntPtr hModule);

        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern IntPtr GetProcAddress(IntPtr hModule, string procName);

        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern bool SetDllDirectory(string lpPathName);

        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern IntPtr GetModuleHandle(string lpPathName);


        public static bool Unload(string lpPathName)
        {
            var module = GetModuleHandle(lpPathName);
            if (module == IntPtr.Zero)
                return false;
            while (FreeLibrary(module))
            {
            }
            return true;
        }
    }
    public class SiReader_JX : IDisposable
    {
        public SiReader_JX(string baseDir)
        {
            Logger.Device.Debug($"[医保动态库] ICCInter_JX_SB.dll 加载开始");
            var fullPath = Path.Combine(baseDir, "ICCInter_JX_SB.dll");
            _libraryHandle = UnSafeMethods.LoadLibrary(fullPath);

            if (_libraryHandle == IntPtr.Zero)
            {
                var errorCode = Marshal.GetHRForLastWin32Error();
                Logger.Device.Debug($"[医保动态库] ICCInter_JX_SB.dll 加载异常:{errorCode}");
                Marshal.ThrowExceptionForHR(errorCode);
            }

            _IC_ReadCardInfo_NoPin = LoadExternalFunction<IntStringBuilderDelegate>("IC_ReadCardInfo_NoPin");
            Logger.Device.Debug($"[医保动态库] ICCInter_JX_SB.dll 加载结束");
        }

        public void Dispose()
        {
            Dispose(true);

            GC.SuppressFinalize(this);
        }

        ~SiReader_JX()
        {
            Dispose(false);
        }

        private T LoadExternalFunction<T>(string functionName)
            where T : class
        {
            var functionPointer = UnSafeMethods.GetProcAddress(_libraryHandle, functionName);

            if (functionPointer == IntPtr.Zero)
                Marshal.ThrowExceptionForHR(Marshal.GetHRForLastWin32Error());

            return Marshal.GetDelegateForFunctionPointer(functionPointer, typeof(T)) as T;
        }
        
        private void Dispose(bool disposing)
        {
            Logger.Device.Debug($"[医保动态库] ICCInter_JX_SB.dll 卸载开始");
            if (disposing)
            {
                _IC_ReadCardInfo_NoPin = null;
            }
            var list = new List<string>
            {
                "ICCInter_JX_SB.dll",
                "ICCDevInter_JX_HT.dll",
                //"mt_32.dll",
                //"FCT_DES.dll",
                //"DCIC32.dll",
                //"YLE902RD_API.dll",
                //"PCOMM.dll",
            };
            var sb = new StringBuilder();
            foreach (var s in list)
            {
                var found = UnSafeMethods.Unload(s);
                sb.AppendLine($"{s} {(found ? "Found" : "Missing")}");
            }
            if (_libraryHandle != IntPtr.Zero)
            {
                while (UnSafeMethods.FreeLibrary(_libraryHandle))
                {
                }
                _libraryHandle = IntPtr.Zero;
            }
            Logger.Device.Debug($"[医保动态库] ICCInter_JX_SB.dll 卸载结束:{sb}");
        }

        #region Fields & Delegates

        private IntPtr _libraryHandle;
        
        private IntStringBuilderDelegate _IC_ReadCardInfo_NoPin;

        private delegate int IntStringBuilderDelegate(StringBuilder sb);

        #endregion Fields & Delegates

        #region Dll Import

        public int IC_ReadCardInfo_NoPin(StringBuilder sb)
        {
            return _IC_ReadCardInfo_NoPin(sb);
        }

        #endregion Dll Import
    }
    public class SiHandler_JX : IDisposable
    {
        public SiHandler_JX(string baseDir)
        {
            Logger.Device.Debug($"[医保动态库] BargaingApplyV3_01038.dll 加载开始");
            var fullPath = Path.Combine(baseDir, "BargaingApplyV3_01038.dll");
            _libraryHandle = UnSafeMethods.LoadLibrary(fullPath);

            if (_libraryHandle == IntPtr.Zero)
            {
                var errorCode = Marshal.GetHRForLastWin32Error();
                Logger.Device.Debug($"[医保动态库] BargaingApplyV3_01038.dll 加载异常:{errorCode}");
                Marshal.ThrowExceptionForHR(errorCode);
            }

            _f_UserBargaingInit = LoadExternalFunction<IntStringStringBuilderStringDelegate>("f_UserBargaingInit");
            _f_UserBargaingClose = LoadExternalFunction<IntStringStringBuilderStringDelegate>("f_UserBargaingClose");
            _f_UserBargaingApply = LoadExternalFunction<IntIntIntStringStringBuilderStringDelegate>("f_UserBargaingApply");
            Logger.Device.Debug($"[医保动态库] BargaingApplyV3_01038.dll 加载结束");
        }

        public void Dispose()
        {
            Dispose(true);

            GC.SuppressFinalize(this);
        }

        ~SiHandler_JX()
        {
            Dispose(false);
        }

        private T LoadExternalFunction<T>(string functionName)
            where T : class
        {
            var functionPointer = UnSafeMethods.GetProcAddress(_libraryHandle, functionName);

            if (functionPointer == IntPtr.Zero)
                Marshal.ThrowExceptionForHR(Marshal.GetHRForLastWin32Error());

            return Marshal.GetDelegateForFunctionPointer(functionPointer, typeof(T)) as T;
        }
        
        private void Dispose(bool disposing)
        {
            Logger.Device.Debug($"[医保动态库] BargaingApplyV3_01038.dll 卸载开始");
            if (disposing)
            {
                _f_UserBargaingInit = null;
                _f_UserBargaingClose = null;
                _f_UserBargaingApply = null;
            }
            var list = new List<string>
            {
                "BargaingApplyV3_01038.dll",
                "ICCInter01038.dll",
                "ICCInter_JX_SB.dll",
                "ICCDevInter_JX_HT.dll",
                //"mt_32.dll",
                //"FCT_DES.dll",
                //"DCIC32.dll",
                //"YLE902RD_API.dll",
                //"PCOMM.dll",
            };
            var sb = new StringBuilder();
            foreach (var s in list)
            {
                var found = UnSafeMethods.Unload(s);
                sb.AppendLine($"{s} {(found ? "Found" : "Missing")}");
            }
            if (_libraryHandle != IntPtr.Zero)
            {
                while (UnSafeMethods.FreeLibrary(_libraryHandle))
                {
                }
                _libraryHandle = IntPtr.Zero;
            }
            Logger.Device.Debug($"[医保动态库] BargaingApplyV3_01038.dll 卸载结束:{sb}");
        }

        #region Fields & Delegates

        private IntPtr _libraryHandle;
        
        private IntStringStringBuilderStringDelegate _f_UserBargaingInit;
        private IntStringStringBuilderStringDelegate _f_UserBargaingClose;
        private IntIntIntStringStringBuilderStringDelegate _f_UserBargaingApply;

        private delegate int IntStringStringBuilderStringDelegate(string data1, StringBuilder sb, string data2);
        private delegate int IntIntIntStringStringBuilderStringDelegate(int code, double no, string data1, StringBuilder sb, string data2);

        #endregion Fields & Delegates

        #region Dll Import

        public int f_UserBargaingInit(string data1, StringBuilder retMsg, string data2)
        {
            return _f_UserBargaingInit(data1, retMsg, data2);
        }

        public int f_UserBargaingClose(string data1, StringBuilder retMsg, string data2)
        {
            return _f_UserBargaingClose(data1, retMsg, data2);
        }

        public int f_UserBargaingApply(int code, double no, string data, StringBuilder retMsg, string data2)
        {
            return _f_UserBargaingApply(code, no, data, retMsg, data2);
        }

        #endregion Dll Import
    }
}

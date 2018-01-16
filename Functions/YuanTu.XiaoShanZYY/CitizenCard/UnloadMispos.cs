using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using YuanTu.Consts;

namespace YuanTu.XiaoShanZYY.CitizenCard
{
    public class UnloadMispos:IDisposable
    {
        private const string DllPath = "External\\XiaoShanSMK\\MisPosDll.dll";
        private IntPtr _libraryHandle;
        private readonly IntIntDelegate _misPosHandle;

        private delegate int IntIntDelegate(int type, StringBuilder dest, int amount, string src);

        public UnloadMispos()
        {
            var libraryName = Path.Combine(FrameworkConst.RootDirectory, DllPath);

            var envPath = Environment.GetEnvironmentVariable("PATH");
            if (envPath != null)
                Environment.SetEnvironmentVariable("PATH",
                    string.Join(";",
                        envPath.Split(';')
                            .Concat(new[] {Path.GetDirectoryName(libraryName)})));

            _libraryHandle = UnSafeMethods.LoadLibrary(libraryName);

            if (_libraryHandle == IntPtr.Zero)
                Marshal.ThrowExceptionForHR(Marshal.GetHRForLastWin32Error());

            _misPosHandle = LoadExternalFunction<IntIntDelegate>("MisPos_Handle");
        }

        public int MisPos_Handle(int type, StringBuilder dest, decimal amount, string src)
        {
            return _misPosHandle(type, dest, (int)amount, src);
        }

        public void Dispose()
        {
            Dispose(true);
            Disposed = true;
            GC.SuppressFinalize(this);
        }

        public bool Disposed { get; private set; }

        ~UnloadMispos()
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
            var list = new List<string>()
            {
                DllPath
            };

            foreach (var s in list)
            {
                UnSafeMethods.Unload(s);
            }
            if (_libraryHandle != IntPtr.Zero)
            {
                while (UnSafeMethods.FreeLibrary(_libraryHandle))
                {
                }
                _libraryHandle = IntPtr.Zero;
            }
            Disposed = true;
        }
    }
}
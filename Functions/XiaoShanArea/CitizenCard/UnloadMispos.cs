using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using YuanTu.Consts;

namespace YuanTu.YuHangArea.CitizenCard
{
    public class UnloadMispos
    {
        private const string DllPath = "MisPosDll.dll";
        private IntPtr _libraryHandle;
        private readonly IntIntDelegate _misPosHandle;

        private delegate int IntIntDelegate(int type, StringBuilder dest, int amount, string src);

        public UnloadMispos()
        {
            _libraryHandle = UnSafeMethods.LoadLibrary(Path.Combine(FrameworkConst.RootDirectory, DllPath));

            if (_libraryHandle == IntPtr.Zero)
                Marshal.ThrowExceptionForHR(Marshal.GetHRForLastWin32Error());

            _misPosHandle = LoadExternalFunction<IntIntDelegate>("MisPos_Handle");
        }

        public int MisPos_Handle(decimal type, StringBuilder dest, decimal amount, string src)
        {
            return _misPosHandle((int)type, dest, (int)amount, src);
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
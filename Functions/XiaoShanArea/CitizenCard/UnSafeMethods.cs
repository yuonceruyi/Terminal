using System;
using System.Runtime.InteropServices;

namespace YuanTu.YuHangArea.CitizenCard
{
    public  class UnSafeMethods
    {
        private const string DllPath= "kernel32.dll";
        [DllImport(DllPath, SetLastError = true)]
        internal static extern IntPtr LoadLibrary(string libraryName);

        [DllImport(DllPath, SetLastError = true)]
        internal static extern bool FreeLibrary(IntPtr hModule);

        [DllImport(DllPath, SetLastError = true)]
        internal static extern IntPtr GetProcAddress(IntPtr hModule, string procName);

        [DllImport(DllPath, SetLastError = true)]
        internal static extern bool SetDllDirectory(string lpPathName);

        [DllImport(DllPath, SetLastError = true)]
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
}

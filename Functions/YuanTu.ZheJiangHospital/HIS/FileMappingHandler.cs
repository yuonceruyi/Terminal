using System;
using System.Runtime.InteropServices;

namespace YuanTu.ZheJiangHospital.HIS
{
    internal class FileMappingHandler
    {
        private const int ERROR_ALREADY_EXISTS = 183;

        private const int FILE_MAP_COPY = 0x0001;
        private const int FILE_MAP_WRITE = 0x0002;
        private const int FILE_MAP_READ = 0x0004;
        private const int FILE_MAP_ALL_ACCESS = 0x0002 | 0x0004;

        private const int PAGE_READONLY = 0x02;
        private const int PAGE_READWRITE = 0x04;
        private const int PAGE_WRITECOPY = 0x08;
        private const int PAGE_EXECUTE = 0x10;
        private const int PAGE_EXECUTE_READ = 0x20;
        private const int PAGE_EXECUTE_READWRITE = 0x40;

        private const int SEC_COMMIT = 0x8000000;
        private const int SEC_IMAGE = 0x1000000;
        private const int SEC_NOCACHE = 0x10000000;
        private const int SEC_RESERVE = 0x4000000;

        private const int INVALID_HANDLE_VALUE = -1;

        private bool alreadyExist;
        private long memSize;

        public IntPtr SharedMemoryFile { get; private set; } = IntPtr.Zero;

        public IntPtr SharedData { get; private set; } = IntPtr.Zero;

        public bool Inited { get; private set; }

        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr CreateFileMapping(int hFile, IntPtr lpAttributes, uint flProtect, uint dwMaxSizeHi,
            uint dwMaxSizeLow, string lpName);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr OpenFileMapping(int dwDesiredAccess,
            [MarshalAs(UnmanagedType.Bool)] bool bInheritHandle, string lpName);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr MapViewOfFile(IntPtr hFileMapping, uint dwDesiredAccess, uint dwFileOffsetHigh,
            uint dwFileOffsetLow, uint dwNumberOfBytesToMap);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        public static extern bool UnmapViewOfFile(IntPtr pvBaseAddress);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        public static extern bool CloseHandle(IntPtr handle);

        [DllImport("kernel32.dll", EntryPoint = "GetLastError")]
        public static extern int GetLastError();

        ~FileMappingHandler()
        {
            Close();
        }

        public int Init(string name, long length)
        {
            Inited = false;
            if (length <= 0 || length > 0x00800000)
                length = 0x00800000;

            memSize = length;
            if (name.Length <= 0)
                return 1;

            SharedMemoryFile = CreateFileMapping(INVALID_HANDLE_VALUE, IntPtr.Zero, PAGE_READWRITE, 0,
                (uint)length, name);

            if (SharedMemoryFile == IntPtr.Zero)
            {
                alreadyExist = false;
                Inited = false;
                return 2;
            }

            alreadyExist = GetLastError() == ERROR_ALREADY_EXISTS;

            SharedData = MapViewOfFile(SharedMemoryFile, FILE_MAP_WRITE, 0, 0, (uint)length);

            if (SharedData == IntPtr.Zero)
            {
                Inited = false;
                CloseHandle(SharedMemoryFile);
                return 3;
            }

            Inited = true;
            if (!alreadyExist)
            {
            }
            return 0;
        }

        public void Close()
        {
            if (!Inited) return;
            UnmapViewOfFile(SharedData);
            CloseHandle(SharedMemoryFile);
            Inited = false;
        }

        public int Read(ref byte[] data, int start, int length)
        {
            if (start + length > memSize)
                return 2;
            if (!Inited)
                return 1;
            Marshal.Copy(SharedData, data, start, length);
            return 0;
        }

        public int Write(byte[] data, int start, int length)
        {
            if (start + length > memSize)
                return 2;
            if (!Inited)
                return 1;
            Marshal.Copy(data, start, SharedData, length);
            return 0;
        }
    }
}
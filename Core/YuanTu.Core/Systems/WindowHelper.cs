using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace YuanTu.Core.Systems
{
    public static class WindowHelper
    {
        public static IntPtr OpenProcess(Process proc, ProcessAccessFlags flags)
        {
            return OpenProcess(flags, false, proc.Id);
        }

        public static void SendKey(IntPtr hPtr, int key)
        {
            PostMessage(hPtr, WM_KEYDOWN, key, 0);
            //PostMessage(hPtr, WM_KEYUP, key, 0);
            //SendMessage(hPtr, WM_KEYDOWN, key, 0);
            //SendMessage(hPtr, WM_KEYUP, key, 0);
        }
        public const Int32 WM_CHAR = 0x0102;
        public const Int32 WM_KEYDOWN = 0x0100;
        public const Int32 WM_KEYUP = 0x0101;
        public const Int32 VK_RETURN = 0x0D;

        public static bool SetTaskBarVisible(bool visible)
        {
          
            var hWnd = FindWindow("Shell_TrayWnd", null);
            if (hWnd == IntPtr.Zero)
                return false;
            ShowWindow(hWnd, visible ? ShowWindowCommands.Show : ShowWindowCommands.Hide);

            var button = FindWindowEx(GetDesktopWindow(), IntPtr.Zero, "button", null);
            if (button == IntPtr.Zero)
                return false;
            ShowWindow(button, visible ? ShowWindowCommands.Show : ShowWindowCommands.Hide);
            return true;
        }

        #region Structs

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public struct PROCESSENTRY32
        {
            private const int MAX_PATH = 260;
            internal uint dwSize;
            internal uint cntUsage;
            internal uint th32ProcessID;
            internal IntPtr th32DefaultHeapID;
            internal uint th32ModuleID;
            internal uint cntThreads;
            internal uint th32ParentProcessID;
            internal int pcPriClassBase;
            internal uint dwFlags;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MAX_PATH)] internal string szExeFile;
        }

        #endregion Structs

        #region Enums

        [Flags]
        public enum CreateToolhelp32SnapshotFlags : uint
        {
            SnapHeapList = 0x00000001,
            SnapProcess = 0x00000002,
            SnapThread = 0x00000004,
            SnapModule = 0x00000008,
            SnapModule32 = 0x00000010,
            SnapAll = SnapHeapList | SnapProcess | SnapThread | SnapModule,
            Inherit = 0x80000000
        }

        [Flags]
        public enum ProcessAccessFlags : uint
        {
            All = 0x001F0FFF,
            Terminate = 0x00000001,
            CreateThread = 0x00000002,
            VirtualMemoryOperation = 0x00000008,
            VirtualMemoryRead = 0x00000010,
            VirtualMemoryWrite = 0x00000020,
            DuplicateHandle = 0x00000040,
            CreateProcess = 0x000000080,
            SetQuota = 0x00000100,
            SetInformation = 0x00000200,
            QueryInformation = 0x00000400,
            QueryLimitedInformation = 0x00001000,
            Synchronize = 0x00100000
        }

        public enum ShowWindowCommands
        {
            /// <summary>
            ///     Hides the window and activates another window.
            /// </summary>
            Hide = 0,

            /// <summary>
            ///     Activates and displays a window. If the window is minimized or
            ///     maximized, the system restores it to its original size and position.
            ///     An application should specify this flag when displaying the window
            ///     for the first time.
            /// </summary>
            Normal = 1,

            /// <summary>
            ///     Activates the window and displays it as a minimized window.
            /// </summary>
            ShowMinimized = 2,

            /// <summary>
            ///     Maximizes the specified window.
            /// </summary>
            Maximize = 3, // is this the right value?

            /// <summary>
            ///     Activates the window and displays it as a maximized window.
            /// </summary>
            ShowMaximized = 3,

            /// <summary>
            ///     Displays a window in its most recent size and position. This value
            ///     is similar to <see cref="Win32.ShowWindowCommand.Normal" />, except
            ///     the window is not activated.
            /// </summary>
            ShowNoActivate = 4,

            /// <summary>
            ///     Activates the window and displays it in its current size and position.
            /// </summary>
            Show = 5,

            /// <summary>
            ///     Minimizes the specified window and activates the next top-level
            ///     window in the Z order.
            /// </summary>
            Minimize = 6,

            /// <summary>
            ///     Displays the window as a minimized window. This value is similar to
            ///     <see cref="Win32.ShowWindowCommand.ShowMinimized" />, except the
            ///     window is not activated.
            /// </summary>
            ShowMinNoActive = 7,

            /// <summary>
            ///     Displays the window in its current size and position. This value is
            ///     similar to <see cref="Win32.ShowWindowCommand.Show" />, except the
            ///     window is not activated.
            /// </summary>
            ShowNA = 8,

            /// <summary>
            ///     Activates and displays the window. If the window is minimized or
            ///     maximized, the system restores it to its original size and position.
            ///     An application should specify this flag when restoring a minimized window.
            /// </summary>
            Restore = 9,

            /// <summary>
            ///     Sets the show state based on the SW_* value specified in the
            ///     STARTUPINFO structure passed to the CreateProcess function by the
            ///     program that started the application.
            /// </summary>
            ShowDefault = 10,

            /// <summary>
            ///     <b>Windows 2000/XP:</b> Minimizes a window, even if the thread
            ///     that owns the window is not responding. This flag should only be
            ///     used when minimizing windows from a different thread.
            /// </summary>
            ForceMinimize = 11
        }

        [Flags]
        public enum SetWindowPosFlags : uint
        {
            /// <summary>Retains the current size (ignores the cx and cy parameters).</summary>
            /// <remarks>SWP_NOSIZE</remarks>
            IgnoreResize = 0x0001,

            /// <summary>Retains the current position (ignores X and Y parameters).</summary>
            /// <remarks>SWP_NOMOVE</remarks>
            IgnoreMove = 0x0002,

            /// <summary>Retains the current Z order (ignores the hWndInsertAfter parameter).</summary>
            /// <remarks>SWP_NOZORDER</remarks>
            IgnoreZOrder = 0x0004,

            /// <summary>
            ///     Does not redraw changes. If this flag is set, no repainting of any kind occurs. This applies to
            ///     the client area, the nonclient area (including the title bar and scroll bars), and any part of the parent
            ///     window uncovered as a result of the window being moved. When this flag is set, the application must
            ///     explicitly invalidate or redraw any parts of the window and parent window that need redrawing.
            /// </summary>
            /// <remarks>SWP_NOREDRAW</remarks>
            DoNotRedraw = 0x0008,

            /// <summary>
            ///     Does not activate the window. If this flag is not set, the window is activated and moved to the
            ///     top of either the topmost or non-topmost group (depending on the setting of the hWndInsertAfter
            ///     parameter).
            /// </summary>
            /// <remarks>SWP_NOACTIVATE</remarks>
            DoNotActivate = 0x0010,

            /// <summary>Draws a frame (defined in the window's class description) around the window.</summary>
            /// <remarks>SWP_DRAWFRAME</remarks>
            DrawFrame = 0x0020,

            /// <summary>
            ///     Applies new frame styles set using the SetWindowLong function. Sends a WM_NCCALCSIZE message to
            ///     the window, even if the window's size is not being changed. If this flag is not specified, WM_NCCALCSIZE
            ///     is sent only when the window's size is being changed.
            /// </summary>
            /// <remarks>SWP_FRAMECHANGED</remarks>
            FrameChanged = 0x0020,

            /// <summary>Displays the window.</summary>
            /// <remarks>SWP_SHOWWINDOW</remarks>
            ShowWindow = 0x0040,

            /// <summary>Hides the window.</summary>
            /// <remarks>SWP_HIDEWINDOW</remarks>
            HideWindow = 0x0080,

            /// <summary>
            ///     Discards the entire contents of the client area. If this flag is not specified, the valid
            ///     contents of the client area are saved and copied back into the client area after the window is sized or
            ///     repositioned.
            /// </summary>
            /// <remarks>SWP_NOCOPYBITS</remarks>
            DoNotCopyBits = 0x0100,

            /// <summary>Same as the SWP_NOOWNERZORDER flag.</summary>
            /// <remarks>SWP_NOREPOSITION</remarks>
            DoNotReposition = 0x0200,

            /// <summary>Does not change the owner window's position in the Z order.</summary>
            /// <remarks>SWP_NOOWNERZORDER</remarks>
            DoNotChangeOwnerZOrder = 0x0200,

            /// <summary>Prevents the window from receiving the WM_WINDOWPOSCHANGING message.</summary>
            /// <remarks>SWP_NOSENDCHANGING</remarks>
            DoNotSendChangingEvent = 0x0400,

            /// <summary>Prevents generation of the WM_SYNCPAINT message.</summary>
            /// <remarks>SWP_DEFERERASE</remarks>
            DeferErase = 0x2000,

            /// <summary>
            ///     If the calling thread and the thread that owns the window are attached to different input queues,
            ///     the system posts the request to the thread that owns the window. This prevents the calling thread from
            ///     blocking its execution while other threads process the request.
            /// </summary>
            /// <remarks>SWP_ASYNCWINDOWPOS</remarks>
            SynchronousWindowPosition = 0x4000
        }

        public enum WindowFlags
        {
            Top = 0,
            Bottom = 1,
            TopMost = -1,
            NoTopMost = -2
        }

        public enum WindowMessage : uint
        {
            NULL = 0x0000,
            CREATE = 0x0001,
            DESTROY = 0x0002,
            MOVE = 0x0003,
            SIZE = 0x0005,
            ACTIVATE = 0x0006,
            SETFOCUS = 0x0007,
            KILLFOCUS = 0x0008,
            ENABLE = 0x000A,
            SETREDRAW = 0x000B,
            SETTEXT = 0x000C,
            GETTEXT = 0x000D,
            GETTEXTLENGTH = 0x000E,
            PAINT = 0x000F,
            CLOSE = 0x0010,
            CLICK= 0x00F5,
            GETCHECK=0x00F0,
        }

        #endregion Enums

        #region DLL Import

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern int SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern IntPtr FindWindow(string strclassName, string strWindowName);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern IntPtr FindWindowEx(IntPtr parentHandle, IntPtr childAfter, string className,
            string windowTitle);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern bool SetWindowText(IntPtr hwnd, string lpString);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, StringBuilder lParam);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, int wParam, int lParam);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern IntPtr PostMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern IntPtr PostMessage(IntPtr hWnd, uint Msg, IntPtr wParam, StringBuilder lParam);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern IntPtr PostMessage(IntPtr hWnd, uint Msg, int wParam, int lParam);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern IntPtr PostMessage(IntPtr hWnd, uint Msg, uint wParam, uint lParam);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern int SetWindowPos(IntPtr hWnd, WindowFlags hWndInsertAfter, int x, int y, int Width,
            int Height,
            SetWindowPosFlags flags);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern int SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int x, int y, int Width, int Height,
            SetWindowPosFlags flags);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern IntPtr CreateToolhelp32Snapshot(CreateToolhelp32SnapshotFlags dwFlags, int th32ProcessID);

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern IntPtr CreateToolhelp32Snapshot([In] uint dwFlags, [In] uint th32ProcessID);

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern bool Process32First([In] IntPtr hSnapshot, ref PROCESSENTRY32 lppe);

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern bool Process32Next([In] IntPtr hSnapshot, ref PROCESSENTRY32 lppe);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool CloseHandle([In] IntPtr hObject);

        [DllImport("kernel32.dll")]
        public static extern IntPtr OpenProcess(ProcessAccessFlags processAccess, bool bInheritHandle, int processId);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern IntPtr SendDlgItemMessage(IntPtr hDlg, int nIDDlgItem, uint Msg, UIntPtr wParam,
            IntPtr lParam);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool ShowWindow(IntPtr hWnd, ShowWindowCommands nCmdShow);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool UpdateWindow(IntPtr hWnd);
        public struct Rect
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }
        [DllImport("user32.dll")]
        public static extern int GetWindowRect(IntPtr hwnd, out Rect lpRect);


        [DllImport("user32.dll")]
        private static extern IntPtr GetDesktopWindow();
        #endregion DLL Import
    }
}
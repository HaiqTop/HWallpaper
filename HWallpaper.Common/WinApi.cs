using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;  //调用WINDOWS API函数时要用到
using Microsoft.Win32;  //写入注册表时要用到
using System.Windows;
using System.Drawing;
using System.Messaging;

namespace HWallpaper.Common
{
    public class WinApi
    {
        #region 设置桌面壁纸
        //利用系统的用户接口设置壁纸
        [DllImport("user32.dll", EntryPoint = "SystemParametersInfo")]
        public static extern int SystemParametersInfo(
                int uAction,
                int uParam,
                string lpvParam,
                int fuWinIni
                );

        /// <summary>
        /// 设置电脑桌面壁纸
        /// </summary>
        /// <param name="strSavePath"></param>
        public static void SetWallpaper(string strSavePath)
        {
            SystemParametersInfo(20, 1, strSavePath, 1);
        }

        //调用系统的用户接口
        [DllImport("user32.dll", EntryPoint = "ShowWindow", SetLastError = true)]
        public static extern bool ShowWindow(IntPtr hWnd, uint nCmdShow);
        [DllImport("user32.dll", EntryPoint = "FindWindow", SetLastError = true)]
        public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        #endregion

        #region 设置锁屏壁纸 无效方法
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern Int32 SystemParametersInfo(UInt32 action, UInt32 uParam, string vParam, UInt32 winIni);
        private static readonly UInt32 SPI_SETDESKWALLPAPER = 20;
        private static UInt32 SPIF_UPDATEINIFILE = 0x1;
        private static uint MAX_PATH = 260;

        // then I call
        /// <summary>
        /// 设置锁屏壁纸
        /// </summary>
        /// <param name="strSavePath"></param>
        public static void SetLockWallpaper(string strSavePath)
        {
            SystemParametersInfo(SPI_SETDESKWALLPAPER, 0, strSavePath, SPIF_UPDATEINIFILE);
        }
        #endregion

        #region 监听空闲时间
        [StructLayout(LayoutKind.Sequential)]
        struct LASTINPUTINFO
        {
            [MarshalAs(UnmanagedType.U4)]
            public int cbSize;
            [MarshalAs(UnmanagedType.U4)]
            public uint dwTime;
        }

        [DllImport("user32.dll")]
        static extern bool GetLastInputInfo(ref LASTINPUTINFO plii);

        public static long GetLastInputTime()
        {
            LASTINPUTINFO vLastInputInfo = new LASTINPUTINFO();
            vLastInputInfo.cbSize = Marshal.SizeOf(vLastInputInfo);
            if (!GetLastInputInfo(ref vLastInputInfo)) return 0;
            return Environment.TickCount - (long)vLastInputInfo.dwTime;
        }
        #endregion

        #region 窗体显示隐藏效果
        /// <summary>
        /// 窗体显示隐藏效果类型
        /// </summary>
        [Flags]
        public enum AnimationType
        {
            /// <summary>
            /// 从左向右显示
            /// </summary>
            AW_HOR_POSITIVE = 0x0001,
            /// <summary>
            /// 从右向左显示
            /// </summary>
            AW_HOR_NEGATIVE = 0x0002,
            /// <summary>
            /// 从上到下显示
            /// </summary>
            AW_VER_POSITIVE = 0x0004,
            /// <summary>
            /// 从下到上显示
            /// </summary>
            AW_VER_NEGATIVE = 0x0008,
            /// <summary>
            /// 从中间向四周(若使用了AW_HIDE标志，则使窗口向内重叠；若未使用AW_HIDE标志，则使窗口向外扩展。)
            /// </summary>
            AW_CENTER = 0x0010,
            /// <summary>
            /// 透明渐变显示效果
            /// </summary>
            AW_BLEND = 0x80000,
            /// <summary>
            /// 使用滑动类型。缺省则为滚动动画类型。当使用AW_CENTER标志时，这个标志就被忽略。
            /// </summary>
            AW_SLIDE = 0x40000,
            /// <summary>
            /// 普通显示 激活窗口。在使用了AW_HIDE标志后不要使用这个标志。
            /// </summary>
            AW_ACTIVATE = 0x20000,
            /// <summary>
            /// 隐藏窗口，缺省则显示窗口。
            /// </summary>
            AW_HIDE = 0x10000,
        }

        [DllImport("user32")]
        public static extern bool AnimateWindow(IntPtr hwnd, int dwTime, AnimationType dwFlags);
        #endregion

        #region 判断当前是否锁屏
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern bool SystemParametersInfo(
           int uAction, int uParam, ref bool lpvParam,
           int flags);

        private const int SPI_GETSCREENSAVERRUNNING = 114; //获取是否处于屏保参数

        /// <summary>
        /// 桌面是否处于屏保
        /// </summary>
        /// <returns></returns>
        public static bool IsScreenSaverRunning()
        {
            bool isRunning = false;
            SystemParametersInfo(SPI_GETSCREENSAVERRUNNING, 0, ref isRunning, 0);
            return isRunning;
        }
        #endregion

        #region  判断文件是否被占用
        [DllImport("kernel32.dll")]
        public static extern IntPtr _lopen(string lpPathName, int iReadWrite);

        [DllImport("kernel32.dll")]
        public static extern bool CloseHandle(IntPtr hObject);

        public const int OF_READWRITE = 2;
        public const int OF_SHARE_DENY_NONE = 0x40;
        public static readonly IntPtr HFILE_ERROR = new IntPtr(-1);
        /// <summary>
        /// 判断文件是否被占用
        /// </summary>
        /// <param name="fileName">文件全路径名称</param>
        /// <returns></returns>
        public static bool FileIsOccupy(string fileName)
        {
            IntPtr vHandle = _lopen(fileName, OF_READWRITE | OF_SHARE_DENY_NONE);
            if (vHandle == HFILE_ERROR)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        #endregion

        #region 判断是否有全屏应用
        //https://www.cnblogs.com/Red-ButterFly/p/7726528.html
        [DllImport("SHELL32", CallingConvention = CallingConvention.StdCall)]
        public static extern uint SHAppBarMessage(int dwMessage, ref APPBARDATA pData);
        [DllImport("User32.dll", CharSet = CharSet.Auto)]
        public static extern int RegisterWindowMessage(string msg);
        /// <summary>
        /// 取得Shell窗口句柄函数
        /// </summary>
        /// <returns></returns>
        [DllImport("user32.dll")]
        public static extern IntPtr GetShellWindow();
        /// <summary>
        /// 取得桌面窗口句柄函数
        /// </summary>
        /// <returns></returns>
        [DllImport("user32.dll")]
        public static extern IntPtr GetDesktopWindow();
        /// <summary>
        /// 取得前台窗口句柄函数
        /// </summary>
        /// <returns></returns>
        [DllImport("user32.dll")]
        public static extern IntPtr GetForegroundWindow();
        #endregion
    }
    #region 判断是否有应用全屏应用
    [StructLayout(LayoutKind.Sequential)]
    public struct APPBARDATA
    {
        public int cbSize;
        public IntPtr hWnd;
        public int uCallbackMessage;
        public int uEdge;
        public Rect rc;
        public IntPtr lParam;
    }
    public enum ABMsg : int
    {
        ABM_NEW = 0,
        ABM_REMOVE,
        ABM_QUERYPOS,
        ABM_SETPOS,
        ABM_GETSTATE,
        ABM_GETTASKBARPOS,
        ABM_ACTIVATE,
        ABM_GETAUTOHIDEBAR,
        ABM_SETAUTOHIDEBAR,
        ABM_WINDOWPOSCHANGED,
        ABM_SETSTATE
    }
    public enum ABNotify : int
    {
        ABN_STATECHANGE = 0,
        ABN_POSCHANGED,
        ABN_FULLSCREENAPP,
        ABN_WINDOWARRANGE
    }
    public enum ABEdge : int
    {
        ABE_LEFT = 0,
        ABE_TOP,
        ABE_RIGHT,
        ABE_BOTTOM
    }
    #endregion

}

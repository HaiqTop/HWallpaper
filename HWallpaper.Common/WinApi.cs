using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;  //调用WINDOWS API函数时要用到
using Microsoft.Win32;  //写入注册表时要用到

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

        ///// <summary>
        ///// 利用win32 API 指定切换效果
        ///// </summary>
        ///// <param name="Handle">Control.Handle</param>
        ///// <param name="type">0-7</param>
        //public static void AnimateWindow(IntPtr Handle, int type)
        //{
        //    switch (type)
        //    {
        //        case 0://普通显示
        //            AnimateWindow(Handle, 1000, AW_ACTIVATE);
        //            break;
        //        case 1://从左向右显示
        //            AnimateWindow(Handle, 1000, AW_HOR_POSITIVE);
        //            break;
        //        case 2://从右向左显示
        //            AnimateWindow(Handle, 1000, AW_HOR_NEGATIVE);
        //            break;
        //        case 3://从上到下显示
        //            AnimateWindow(Handle, 1000, AW_VER_POSITIVE);
        //            break;
        //        case 4://从下到上显示
        //            AnimateWindow(Handle, 1000, AW_VER_NEGATIVE);
        //            break;
        //        case 5://透明渐变显示
        //            AnimateWindow(Handle, 1000, AW_BLEND);
        //            break;
        //        case 6://从中间向四周
        //            AnimateWindow(Handle, 1000, AW_CENTER);
        //            break;
        //        case 7://左上角伸展
        //            AnimateWindow(Handle, 1000, AW_SLIDE);
        //            break;
        //    }
        //}

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
    }
}

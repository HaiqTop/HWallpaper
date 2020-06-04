using HWallpaper.Business;
using HWallpaper.Common;
using HWallpaper.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;


namespace HWallpaper
{
    public enum MonitorEventType
    {
        PowerOff = 0,
        PowerOn = 1,
        Dimmed = 2
    }
    public partial class F_WndProc : Form
    {
        public delegate void MonitorDelegate(MonitorEventType type);
        public event MonitorDelegate MonitorEvent;

        public F_WndProc()
        {
            InitializeComponent();
        }

        private void F_WndProc_Load(object sender, EventArgs e)
        {
            RegisterAppBar(false);//注册该事件；
            this.Hide();
        }
        #region 屏幕开启关闭事件
        private IntPtr unRegPowerNotify = IntPtr.Zero;
        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            var settingGuid = new NativeMethods.PowerSettingGuid();
            Guid powerGuid = NativeMethods.IsWindows8Plus()
                           ? settingGuid.ConsoleDisplayState
                           : settingGuid.MonitorPowerGuid;

            unRegPowerNotify = NativeMethods.RegisterPowerSettingNotification(
                this.Handle, powerGuid, NativeMethods.DEVICE_NOTIFY_WINDOW_HANDLE);
        }
        #endregion 屏幕开启关闭事件


        #region 判断是否有全屏应用
        /// <summary>
        /// 判断是否存在全屏应用
        /// </summary>
        public bool RunningFullScreenApp = false;
        private IntPtr desktopHandle;
        private IntPtr shellHandle;
        int uCallBackMsg;

        private void RegisterAppBar(bool registered)
        {
            APPBARDATA abd = new APPBARDATA();
            abd.cbSize = Marshal.SizeOf(abd);
            abd.hWnd = this.Handle;

            desktopHandle = WinApi.GetDesktopWindow();
            shellHandle = WinApi.GetShellWindow();
            if (!registered)
            {
                //register
                uCallBackMsg = WinApi.RegisterWindowMessage("APPBARMSG_CSDN_HELPER");
                abd.uCallbackMessage = uCallBackMsg;
                uint ret = WinApi.SHAppBarMessage((int)ABMsg.ABM_NEW, ref abd);
            }
            else
            {
                WinApi.SHAppBarMessage((int)ABMsg.ABM_REMOVE, ref abd);
            }
        }
        #endregion 判断是否有全屏应用
        protected override void WndProc(ref Message m)
        {
            #region 屏幕打开和关闭事件
            switch (m.Msg)
            {
                case NativeMethods.WM_POWERBROADCAST:
                    if (m.WParam == (IntPtr)NativeMethods.PBT_POWERSETTINGCHANGE)
                    {
                        var settings = (NativeMethods.POWERBROADCAST_SETTING)m.GetLParam(
                            typeof(NativeMethods.POWERBROADCAST_SETTING));
                        if (this.MonitorEvent != null)
                        {
                            MonitorEventType type = (MonitorEventType)settings.Data;
                            this.MonitorEvent(type);
                        }
                    }
                    m.Result = (IntPtr)1;
                    break;
            }
            #endregion 屏幕打开和关闭事件
            #region 判断是否有全屏应用
            if (m.Msg == uCallBackMsg)
            {
                switch (m.WParam.ToInt32())
                {
                    case (int)ABNotify.ABN_FULLSCREENAPP:
                        {
                            IntPtr hWnd = WinApi.GetForegroundWindow();
                            //判断当前全屏的应用是否是桌面
                            if (hWnd.Equals(desktopHandle) || hWnd.Equals(shellHandle))
                            {
                                RunningFullScreenApp = false;
                                break;
                            }
                            //判断是否全屏
                            if ((int)m.LParam == 1)
                                this.RunningFullScreenApp = true;
                            else
                                this.RunningFullScreenApp = false;
                            break;
                        }
                    default:
                        break;
                }
            }
            #endregion 判断是否有全屏应用
            base.WndProc(ref m);
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            NativeMethods.UnregisterPowerSettingNotification(unRegPowerNotify);
            RegisterAppBar(true);//清除事件；
            base.OnFormClosing(e);
        }


    }
}

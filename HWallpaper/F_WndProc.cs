using HWallpaper.Business;
using HWallpaper.Common;
using HWallpaper.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
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

        protected override void WndProc(ref Message m)
        {
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
                            //switch (settings.Data)
                            //{
                            //    case 0:
                            //        this.MonitorEvent(MonitorEventType.PowerOn);
                            //        LogHelper.WriteLog("Monitor Power Off");
                            //        break;
                            //    case 1:
                            //        LogHelper.WriteLog("Monitor Power On");
                            //        break;
                            //    case 2:
                            //        LogHelper.WriteLog("Monitor Dimmed");
                            //        break;
                            //}
                        }
                    }
                    m.Result = (IntPtr)1;
                    break;
            }
            base.WndProc(ref m);
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            NativeMethods.UnregisterPowerSettingNotification(unRegPowerNotify);
            base.OnFormClosing(e);
        }

        private void F_WndProc_Load(object sender, EventArgs e)
        {
            this.Hide();
        }
    }
}

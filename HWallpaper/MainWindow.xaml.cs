using HWallpaper.Business;
using HWallpaper.Common;
using HWallpaper.Model;
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace HWallpaper
{
    public partial class MainWindow
    {
        private ImageHelper imgHelper ;
        Screensaver screensaver;
        Wallpaper wallpaper;
        //HandyControl.Controls.NotifyIcon notifyIcon;
        /// <summary>
        /// 屏保开启时间间隔（单位：毫秒）
        /// </summary>
        private long timerS_interval = 10000;
        /// <summary>
        /// 屏保 - 监听用户最后一次操作的Timer
        /// </summary>
        private System.Timers.Timer timerS = new System.Timers.Timer();
        private System.Timers.Timer timerW = new System.Timers.Timer();
        public MainWindow()
        {
            InitializeComponent();
            CleanCache();
            if (ConfigManage.Base.Cache)
            {
                imgHelper = new ImageHelper(ConfigManage.Wallpaper.TypeIndexs, ConfigManage.Base.CachePath);
            }
            else
            {
                imgHelper = new ImageHelper(ConfigManage.Wallpaper.TypeIndexs);
            }
            // 注册监听当前登录的用户变化（登录、注销和解锁屏）事件
            Microsoft.Win32.SystemEvents.SessionSwitch += SystemEvents_SessionSwitch;
            InitTimers_Screen();
            InitTimers_Wallpaper();

        }
        private void MenuItem_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            MenuItem menuItem = sender as MenuItem;
            switch (menuItem.Header)
            {
                case "打开壁纸":
                    if (wallpaper == null || wallpaper.IsClosed)
                    {
                        wallpaper = new Wallpaper();
                        wallpaper.Show();
                    }
                    wallpaper.Activate();
                    wallpaper.WindowState = System.Windows.WindowState.Normal;
                    break;
                case "软件设置":
                    Setting setting = new Setting();
                    setting.ShowDialog();
                    break;
                case "立即屏保":
                    screensaver = new Screensaver();
                    screensaver.Show();
                    break;
            }
        }

        private void NotifyIconContextContent_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            NextWallpaper();
        }

        private void NotifyIconContextContent_MouseDoubleClick(object sender, System.Windows.RoutedEventArgs e)
        {
            if (wallpaper == null || wallpaper.IsClosed)
            { 
                wallpaper = new Wallpaper();
                wallpaper.Show();
            }
            wallpaper.Activate();
            wallpaper.WindowState = System.Windows.WindowState.Normal;
        }

        private void Window_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            //wallpaper = new Wallpaper();
            //wallpaper.Show();
            this.Visibility = System.Windows.Visibility.Hidden;
        }

        public void InitTimers_Screen()
        {
            if (ConfigManage.Screen.Open)
            {
                timerS_interval = (long)ConfigManage.Screen.OpenInterval * 60 * 1000;
                timerS.Elapsed += new System.Timers.ElapsedEventHandler(timerS_Elapsed);
                timerS.Interval = timerS_interval;
                timerS.Start();
            }
        }
        public void InitTimers_Wallpaper()
        {
            if (!ConfigManage.Wallpaper.AutoReplace)
            {
                return;
            }
            DateTime lastTime = DateTime.Now;
            if (ConfigManage.Wallpaper.ReplaceLastTime != DateTime.MinValue)
            {
                lastTime = ConfigManage.Wallpaper.ReplaceLastTime;
            }
            double wallpaper_Time = ConfigManage.Wallpaper.TimeInterval;
            DateTime nextTime = DateTime.Now;
            switch (ConfigManage.Wallpaper.TimeType)
            {
                case TimeType.Day:
                    nextTime = lastTime.AddDays(wallpaper_Time);
                    break;
                case TimeType.Hour:
                    nextTime = lastTime.AddHours(wallpaper_Time);
                    break;
                case TimeType.Minute:
                    nextTime = lastTime.AddMinutes(wallpaper_Time);
                    break;
            }
            double totalSeconds = (DateTime.Now - nextTime).TotalMilliseconds;
            if (totalSeconds > 0)// 表示当前已超过更换壁纸时间，直接更换壁纸
            {
                // 更换壁纸
                NextWallpaper();
                timerW.Interval = (nextTime - lastTime).TotalMilliseconds;
            }
            else if (totalSeconds > -500)
            {
                timerW.Interval = 500;
            }
            else
            {
                timerW.Interval = -totalSeconds;
            }
            timerW.Elapsed += new System.Timers.ElapsedEventHandler(timerW_Elapsed);
            timerW.Start();
        }

        private void timerS_Elapsed(object source, System.Timers.ElapsedEventArgs e)
        {
            if (timerS == null) return;
            //检测一次是否达到屏保设定时间
            long lastInputTime = Common.WinApi.GetLastInputTime();
            long timeSpan = timerS_interval - lastInputTime;
            if (timeSpan > 0)
            {
                timerS.Stop();
                timerS.Interval = timeSpan > 10000 ? (double)timeSpan : 15000d;
                timerS.Start();
                return;
            }
            ShowScreen();
        }
        private void timerW_Elapsed(object source, System.Timers.ElapsedEventArgs e)
        {
            NextWallpaper();
        }
        private void ShowScreen()
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                timerS.Stop();
                screensaver = new Screensaver();
                screensaver.ShowDialog();
                timerS.Interval = 15000d;// 默认15秒检测一次是否达到屏保设定时间
                timerS.Start();
            }));
            
        }
        private void CleanCache()
        {
            if (ConfigManage.Base.AutoClearCache)
            {
                DateTime dTime = DateTime.Now.AddMonths(-1);
                string path = ConfigManage.Base.CachePath;
                DirectoryInfo dirInfo = new DirectoryInfo(path);
                if (dirInfo.Exists)
                {
                    FileInfo[] files = dirInfo.GetFiles();
                    foreach (var file in files)
                    {
                        if (file.CreationTime < dTime)
                        {
                            file.Delete();
                            Common.LogHelper.WriteLog("【删除】删除过期缓存的壁纸：" + file.Name, Common.EnumLogLevel.Info);
                        }
                    }
                }
            }
        }
        /// <summary>
        /// 下一张壁纸
        /// </summary>
        private void NextWallpaper()
        {
            timerW.Stop();
            LogHelper.WriteLog("更换壁纸", EnumLogLevel.Info);
            ImgInfo imgInfo = imgHelper.GetNextImage();
            string imgFullName = System.IO.Path.Combine(ConfigManage.Base.CachePath, imgInfo.GetFileName());
            if (!File.Exists(imgFullName))
            { 
                WebHelper.DownImage(imgInfo.url,imgFullName);
            }
            Common.WinApi.SetWallpaper(imgFullName);
            ConfigManage.Wallpaper.ReplaceLastTime = DateTime.Now;
            ConfigManage.Save();
            timerW.Start();
        }
        /// <summary>
        /// 当前登录的用户变化（登录、注销和解锁屏）
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SystemEvents_SessionSwitch(object sender, Microsoft.Win32.SessionSwitchEventArgs e)
        {
            switch (e.Reason)
            {
                case Microsoft.Win32.SessionSwitchReason.SessionLogon://用户登录
                case Microsoft.Win32.SessionSwitchReason.SessionUnlock://解锁屏
                    InitTimers_Screen();
                    //InitTimers_Wallpaper();
                    break;
                case Microsoft.Win32.SessionSwitchReason.SessionLock://锁屏
                case Microsoft.Win32.SessionSwitchReason.SessionLogoff://注销
                    timerS.Stop();
                    //timerW.Stop();
                    // 遍历所有子窗体，如果找到屏保窗体，则关闭
                    foreach (Window window in Application.Current.Windows)
                    {
                        if (window.Title == "Screensaver")
                        {
                            Screensaver frm = window as Screensaver;
                            frm.Close();
                            return;
                        }
                    }
                    break;
            }
        }
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            //析构，防止句柄泄漏
            //Do this during application close to avoid handle leak
            Microsoft.Win32.SystemEvents.SessionSwitch -= SystemEvents_SessionSwitch;
            // 保存配置
            //Properties.Settings.Default.Save();
        }
    }
}

using HWallpaper.Business;
using HWallpaper.Common;
using HWallpaper.Model;
using System;
using System.IO;
using System.Linq;
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
        F_WndProc f_WndProc = new F_WndProc();
        MonitorEventType curMonitorStatus = MonitorEventType.PowerOn;
        //HandyControl.Controls.NotifyIcon notifyIcon;
        /// <summary>
        /// 屏保开启时间间隔（单位：毫秒）
        /// </summary>
        private long timerS_interval = 10000;
        /// <summary>
        /// 屏保 - 监听用户最后一次操作的Timer
        /// </summary>
        private System.Timers.Timer timerS = new System.Timers.Timer();
        /// <summary>
        /// 壁纸 - 监听用户最后一次操作的Timer
        /// </summary>
        private System.Timers.Timer timerW = new System.Timers.Timer();
        // 监控是否有全屏应用的代码
        private IntPtr desktopHandle;
        private IntPtr shellHandle;
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="showWallpaper">是否显示壁纸界面</param>
        public MainWindow(bool showWallpaper)
        {
            InitializeComponent();
            InitDB();
            CleanCache();
            if (showWallpaper)
            {
                Wallpaper wall = new Wallpaper(this);
                wall.Show();
            }
        }
        private void Window_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            this.Visibility = Visibility.Hidden;
            // 注册监听当前登录的用户变化（登录、注销和解锁屏）事件
            Microsoft.Win32.SystemEvents.SessionSwitch += SystemEvents_SessionSwitch;
            InitImageHelper();
            InitTimers_Screen();
            InitTimers_Wallpaper();
            f_WndProc.Show();
            f_WndProc.MonitorEvent += F_WndProc_MonitorEvent;
            desktopHandle = WinApi.GetDesktopWindow();
            shellHandle = WinApi.GetShellWindow();
        }
        private void F_WndProc_MonitorEvent(MonitorEventType type)
        {
            curMonitorStatus = type;
            switch (type)
            {
                case MonitorEventType.PowerOff:
                    timerS.Stop();
                    if (screensaver != null && screensaver.Visibility == Visibility.Visible)
                    {
                        screensaver.Close();
                    }
                    break;
                case MonitorEventType.PowerOn:
                    timerS.Interval = 15000d;// 默认15秒检测一次是否达到屏保设定时间
                    timerS.Start();
                    break;
            }
        }

        private void MenuItem_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            MenuItem menuItem = sender as MenuItem;
            switch (menuItem.Header)
            {
                case "打开壁纸":
                    if (wallpaper == null || wallpaper.IsClosed)
                    {
                        wallpaper = new Wallpaper(this);
                        wallpaper.Show();
                    }
                    wallpaper.Activate();
                    wallpaper.WindowState = System.Windows.WindowState.Normal;
                    break;
                case "软件设置":
                    Setting setting = new Setting();
                    setting.ChangeConfigEvent += Setting_ChangeConfigEvent;
                    setting.ShowDialog();
                    break;
                case "立即屏保":
                    timerS.Stop();
                    screensaver = new Screensaver();
                    screensaver.ShowDialog();
                    break;
                case "调试":
                    //string url = "http://localhost:53054/Update/CheckUpdate";
                    AutoUpdate.Helper.UpdateHelper.CheckUpdateAsyn();
                    break;
            }
        }

        private void Setting_ChangeConfigEvent()
        {
            this.InitTimers_Screen();
            this.InitTimers_Wallpaper();
            this.InitImageHelper();
        }

        private void NotifyIconContextContent_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            NextWallpaper();
        }

        private void NotifyIconContextContent_MouseDoubleClick(object sender, System.Windows.RoutedEventArgs e)
        {
            if (wallpaper == null || wallpaper.IsClosed)
            { 
                wallpaper = new Wallpaper(this);
                wallpaper.Show();
            }
            wallpaper.Activate();
            wallpaper.WindowState = System.Windows.WindowState.Normal;
        }


        public void InitTimers_Screen()
        {
            timerS.Stop();
            if (ConfigManage.Screen.Open)
            {
                timerS_interval = (long)ConfigManage.Screen.OpenInterval * 60 * 1000;
                timerS.Elapsed -= new System.Timers.ElapsedEventHandler(timerS_Elapsed);
                timerS.Elapsed += new System.Timers.ElapsedEventHandler(timerS_Elapsed);
                timerS.Interval = timerS_interval;
                timerS.Start();
            }
        }
        public void InitTimers_Wallpaper()
        {
            timerW.Stop();
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
            timerW.Elapsed -= new System.Timers.ElapsedEventHandler(timerW_Elapsed);
            timerW.Elapsed += new System.Timers.ElapsedEventHandler(timerW_Elapsed);
            timerW.Start();
        }
        public void InitImageHelper()
        {
            if (ConfigManage.Base.Cache)
            {
                imgHelper = new ImageHelper(ConfigManage.Wallpaper.TypeIndexs, ConfigManage.Base.CachePath);
            }
            else
            {
                imgHelper = new ImageHelper(ConfigManage.Wallpaper.TypeIndexs);
            }
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
                if (timerS.Interval > Int32.MaxValue)
                {
                    timerS.Interval = Int32.MaxValue;
                }
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
                System.Drawing.Rectangle? rect = GetShowScreenRect();
                // 判断是否存在全屏应用
                if (rect == null)
                {
                    timerS.Interval = 15000d;
                    timerS.Start();
                    return;
                }
                this.Left = (double)rect?.Left;
                screensaver = new Screensaver((double)rect?.Left);
                screensaver.Closed += Screensaver_Closed;
                screensaver.Show();
            }));
            
        }

        private void Screensaver_Closed(object sender, EventArgs e)
        {
            // 只有当屏幕是开启的时候，才会在屏保关闭之后，继续开启屏保计时
            if (curMonitorStatus == MonitorEventType.PowerOn)
            {
                timerS.Interval = 15000d;// 默认15秒检测一次是否达到屏保设定时间
                timerS.Start();
            }
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

        private void InitDB()
        {
            if (!File.Exists(Const.dbFile))
            {
                if (!Directory.Exists(Const.dataPath))
                {
                    Directory.CreateDirectory(Const.dataPath);
                }
                File.Copy(Const.dbEmptyFile, Const.dbFile);
            }
        }

        /// <summary>
        /// 下一张壁纸
        /// </summary>
        private void NextWallpaper()
        {
            try
            {
                timerW.Stop();
                LogHelper.WriteLog("更换壁纸", EnumLogLevel.Info);
                ImgInfo imgInfo = imgHelper.GetNextImage();
                if (UserDataManage.IsDislike(imgInfo.Id)) imgInfo = imgHelper.GetNextImage();
                string imgFullName = System.IO.Path.Combine(ConfigManage.Base.CachePath, imgInfo.GetFileName());
                if (!File.Exists(imgFullName))
                {
                    // 判断下载目录中是否存在
                    imgFullName = System.IO.Path.Combine(ConfigManage.Base.DownPath, imgInfo.GetFileName());
                    if (!File.Exists(imgFullName))
                        WebHelper.DownImage(imgInfo.url, imgFullName);
                }
                if (File.Exists(imgFullName))
                {
                    Common.WinApi.SetWallpaper(imgFullName);
                    UserDataManage.AddRecord(RecordType.AutoWallpaper, imgInfo);
                    ConfigManage.Wallpaper.ReplaceLastTime = DateTime.Now;
                    ConfigManage.Save();
                }
                timerW.Start();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            
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
                    if (screensaver != null && screensaver.Visibility == Visibility.Visible)
                    {
                        screensaver.Close();
                    }
                    // 遍历所有子窗体，如果找到屏保窗体，则关闭（作废原因：上面代码可以替代）
                    //foreach (Window window in Application.Current.Windows)
                    //{
                    //    if (window.Title == "Screensaver")
                    //    {
                    //        Screensaver frm = window as Screensaver;
                    //        frm.Close();
                    //        return;
                    //    }
                    //}
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


        /// <summary>
        /// 判断前台窗口是否是全屏
        /// </summary>
        /// <returns></returns>
        public bool TopWinIsFull()
        {
            IntPtr hWnd = WinApi.GetForegroundWindow();
            //判断当前全屏的应用是否是桌面
            if (hWnd.Equals(desktopHandle) || hWnd.Equals(shellHandle))
            {
                return false;
            }
            else
            {
                WinApi.RECT rect = new WinApi.RECT();
                WinApi.GetWindowRect(hWnd, ref rect);
                int width = rect.Right - rect.Left;                        //窗口的宽度
                int height = rect.Bottom - rect.Top;                   //窗口的高度
                System.Drawing.Rectangle ScreenArea = System.Windows.Forms.Screen.AllScreens[0].Bounds;
                if (width == ScreenArea.Width && height == ScreenArea.Height
                    && rect.Left == ScreenArea.Left && rect.Bottom == ScreenArea.Bottom)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
        /// <summary>
        /// 获取屏保界面可以显示的位置
        /// </summary>
        /// <returns></returns>
        public System.Drawing.Rectangle? GetShowScreenRect()
        {
            System.Drawing.Rectangle? result = null;
            IntPtr hWnd = WinApi.GetForegroundWindow();
            //判断当前全屏的应用是否是桌面
            if (hWnd.Equals(desktopHandle) || hWnd.Equals(shellHandle))
            {
                // 当前前置窗口是桌面或者shell，不存在全屏应用，所以返回主监视器的显示范围
                result = System.Windows.Forms.Screen.PrimaryScreen.Bounds;
            }
            else
            {
                WinApi.RECT rect = new WinApi.RECT();
                WinApi.GetWindowRect(hWnd, ref rect);
                foreach (var screen in System.Windows.Forms.Screen.AllScreens)
                {
                    if (rect.Left != screen.Bounds.Left || rect.Right != screen.Bounds.Right//前台窗口在当前监视器上，且是全屏模式，则继续判断下一个监视器
                        || rect.Top != screen.Bounds.Top || rect.Bottom < screen.Bounds.Bottom - 5)
                    {
                        result = screen.Bounds;
                        break;
                    }
                }
            }
            return result;
        }
    }
}

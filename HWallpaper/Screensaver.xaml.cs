using HandyControl.Controls;
using HandyControl.Tools;
using HWallpaper.Business;
using HWallpaper.Common;
using HWallpaper.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using static HWallpaper.Common.WinApi;

namespace HWallpaper
{
    /// <summary>
    /// Screensaver.xaml 的交互逻辑
    /// </summary>
    public partial class Screensaver : System.Windows.Window
    {
        private IntPtr unRegPowerNotify = IntPtr.Zero;
        public double NegativeHeight
        {
            get
            {
                return (this.Height > 0 ? this.Height : this.ActualHeight) * -1;
            }
        }
        public double NegativeWidth
        {
            get
            {
                return (this.Width > 0 ? this.Width : this.ActualWidth) * -1;
            }
        }

        private string CachePath;
        private string DownPath;
        private ImageHelper imgHelper;
        private ImageQueue imageQueue = new ImageQueue();
        private List<Storyboard> storys = new List<Storyboard>();
        /// <summary>
        /// 图片切换用的Timer
        /// </summary>
        private System.Timers.Timer timerP = new System.Timers.Timer();
        /// <summary>
        /// Label标签（时间）文字切换用的Timer
        /// </summary>
        private System.Timers.Timer timerL = new System.Timers.Timer();
        public Screensaver()
        {
            InitializeComponent();
            this.WindowState = WindowState.Maximized;
        }
        public Screensaver(double left)
        {
            InitializeComponent();
            this.WindowStartupLocation = WindowStartupLocation.Manual;
            this.Left = left;
        }
        private void windown_Loaded(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Maximized;
            CachePath = ConfigManage.Base.CachePath;
            DownPath = ConfigManage.Base.DownPath;
            UpdateTime();
            InitTimer();
            if (ConfigManage.Base.Cache)
            {
                imgHelper = new ImageHelper(ConfigManage.Screen.TypeIndexs, ConfigManage.Base.CachePath);
            }
            else
            {
                imgHelper = new ImageHelper(ConfigManage.Screen.TypeIndexs);
            }
            imageQueue.OnComplate += ImageQueue_OnComplate;
            imageQueue.OnError += ImageQueue_OnError;
            if (ConfigManage.Base.Cache)
            {
                imageQueue.CachePath = ConfigManage.Base.CachePath;
            }
            LoadStoryboard();
            EffectPicture(null, null);
            this.Activate();
            this.Focus();
        }
        /// <summary>
        /// 初始化显示屏保后图片切换的定时器
        /// </summary>
        private void InitTimer()
        {
            timerP.Elapsed += new System.Timers.ElapsedEventHandler(EffectPicture);
            timerP.Interval = ConfigManage.Screen.ReplaceInterval * 1000;
            timerP.Enabled = true;
            timerP.Start();
            timerL.Elapsed += new System.Timers.ElapsedEventHandler(timerL_Elapsed);
            timerL.Interval = 10000;
            timerL.Enabled = true;
            timerL.Start();
        }

        private void timerL_Elapsed(object source, ElapsedEventArgs e)
        {
            UpdateTime();
        }

        private void LoadStoryboard()
        {
            foreach (System.Collections.DictionaryEntry item in base.Resources)
            {
                if (item.Value is Storyboard)
                {
                    storys.Add(item.Value as Storyboard);
                }
            }
        }
        private void UpdateTime()
        {
            labelD.Dispatcher.BeginInvoke(new Action(() =>
            {
                labelD.Text = DateTime.Now.ToString("MM-dd") + " " + Common.Utils.GetWeetString();
                labelT.Text = DateTime.Now.ToString("HH:mm");
            }));
        }
        private void EffectPicture(object source, ElapsedEventArgs e)
        {
            try
            {
                ImgInfo info = imgHelper.GetNextImage();
                if(UserDataManage.IsDislike(info.Id)) info = imgHelper.GetNextImage();
                picBox.Dispatcher.BeginInvoke(new Action<Image, ImgInfo>((image, imgInfo) =>
                {
                    timerP.Stop();
                    imageQueue.Queue(picBox,info);
                }), new Object[] { picBox, info });
                
            }
            catch (Exception ex)
            {
                Common.LogHelper.WriteLog(ex.Message, Common.EnumLogLevel.Error);
                picBox.Dispatcher.BeginInvoke(new Action<Exception>((exc) =>
                {
                    Growl.Error("壁纸切换异常：\n\t" + exc.Message);
                }), new Object[] { ex });
            }
        }
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            try
            {
                this.Hide();
                timerP.Stop();
                timerP.Dispose();
                timerP = null;
                timerL.Stop();
                timerL.Dispose();
                ConfigManage.Save();
            }
            catch { }
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            // 判断点击鼠标点击是否是在给壁纸打分（根据鼠标点击的坐标范围）
            Point point = e.GetPosition(scorePanel);
            if (point.X < 0 || point.Y < 0
                || point.X - scorePanel.ActualWidth > 0 || point.Y - scorePanel.ActualHeight > 0)
            {
                this.Close();
            }
            //EffectPicture(null,null);
        }
        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Up:
                    Btn_Click(btn_love, null); 
                    break;
                case Key.Down:
                    Btn_Click(btn_down,null);
                    break;
                case Key.Right:
                    break;
                case Key.Escape:
                    this.Close();
                    break;
            }
        }
        int index = 0;
        private void ImageQueue_OnComplate(BitmapImage b,ImgInfo imgInfo)
        {
            if (picBox.Source != null)
            {
                picBoxTemp.Source = picBox.Source;
                picBox.Source = b;
                if (base.Resources.Contains("test"))
                {
                    (base.Resources["test"] as Storyboard).Begin();
                    return;
                }
                storys[index++].Begin();
                if (index >= storys.Count) index = 0;
            }
            else
            { 
                picBox.Source = b;
                timerP.Start();
                btnPanel.Tag = imgInfo;
                //this.Visibility = Visibility.Visible;
            }
            InitBtnState(imgInfo);
        }
        private void InitBtnState(ImgInfo imgInfo)
        {
            btn_love.Foreground = Brushes.White;
            btn_dislike.Foreground = Brushes.White;
            btn_down.Foreground = Brushes.White;

            Love love = UserDataManage.GetLove(imgInfo.Id);
            if (love != null)
            {
                if (love.Type == 1)
                {
                    btn_love.Foreground = Brushes.Red;
                }
                else if (love.Type == -1)
                {
                    btn_dislike.Foreground = Brushes.Red;
                }
            }
            Download down = UserDataManage.GetDown(imgInfo.Id);
            string fullName = System.IO.Path.Combine(this.DownPath, imgInfo.GetFileName());
            if (System.IO.File.Exists(fullName))
            {
                btn_down.Foreground = Brushes.Red;
                if (down == null)
                {
                    down = new Download() { PictureId = imgInfo.Id, Time = DateTime.Now, FullName = fullName, Valid = 1 };
                }
                else if (down.Valid == 0)
                {
                    down.Valid = 1;
                }
                UserDataManage.SaveDown(down, imgInfo);
            }
        }
        private void Btn_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            if (btn == null) return;
            ImgInfo imgInfo = (btn.Parent as StackPanel).Tag as ImgInfo;
            switch (btn.Name)
            {
                case "btn_down":
                    {
                        if (btn.Foreground == Brushes.White)
                        {
                            string imgFullName = System.IO.Path.Combine(this.DownPath, imgInfo.GetFileName());
                            if (!System.IO.File.Exists(imgFullName))
                            {
                                System.Drawing.Image img = WebHelper.GetImage(imgInfo.url);
                                img.Save(imgFullName);
                                img.Dispose();
                            }
                            btn.Foreground = Brushes.Red;
                            UserDataManage.SetDown(imgFullName, imgInfo);
                            Growl.Success("下载成功。");

                            if (btn_love.Foreground == Brushes.White)
                            {
                                UserDataManage.SetLove(LoveType.Love, imgInfo);
                                btn_love.Foreground = Brushes.Red;
                                btn_dislike.Foreground = Brushes.White;
                            }
                        }
                    }
                    break;
                case "btn_wallpaper":
                    {
                        if (btn.Foreground == Brushes.White)
                        {
                            string imgFullName = System.IO.Path.Combine(this.CachePath, imgInfo.GetFileName());
                            if (!System.IO.File.Exists(imgFullName))
                            {
                                System.Drawing.Image img = WebHelper.GetImage(imgInfo.url);
                                img.Save(imgFullName);
                                img.Dispose();
                            }
                            WinApi.SetWallpaper(imgFullName);
                            btn.Foreground = Brushes.Red;
                            Growl.Success("壁纸设置成功。");
                            UserDataManage.AddRecord(RecordType.ManualWallpaper, imgInfo);
                        }
                    }
                    break;
                case "btn_love":
                    {
                        if (btn.Foreground == Brushes.White)
                        {
                            UserDataManage.SetLove(LoveType.Love, imgInfo);
                            btn.Foreground = Brushes.Red;
                            btn_dislike.Foreground = Brushes.White;
                        }
                    }
                    break;
                case "btn_dislike":
                    {
                        if (btn.Foreground == Brushes.White)
                        {
                            UserDataManage.SetLove(LoveType.Dislike, imgInfo);
                            btn.Foreground = Brushes.Red;
                            btn_love.Foreground = Brushes.White;
                            this.EffectPicture(null,null);
                        }
                    }
                    break;
            }
        }

        private void ImageQueue_OnError(Exception e)
        {
            Growl.Error(e.Message);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Storyboard_Completed(object sender, EventArgs e)
        {
            timerP?.Start();
        }

    }
}

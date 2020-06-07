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
            ImgInfo info = imgHelper.GetNextImage();
            try
            {
                picBox.Dispatcher.BeginInvoke(new Action<Image, ImgInfo>((image, imgInfo) =>
                {
                    timerP.Stop();
                    imageQueue.Queue(info.url, picBox,info.GetFileName());
                }), new Object[] { picBox, info });
                
            }
            catch (Exception ex)
            {
                Common.LogHelper.WriteLog(ex.Message, Common.EnumLogLevel.Error);
            }
        }

        private void picBox_Click(object sender, EventArgs e)
        {
            this.Close();
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
            this.Close();
            //EffectPicture(null,null);
        }
        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Up:
                case Key.Down:
                    try
                    {
                        string path = ConfigManage.Base.DownPath;
                        if (!System.IO.Directory.Exists(path))
                            System.IO.Directory.CreateDirectory(path);
                        string imgFullName = System.IO.Path.Combine(path, picBox.Tag.ToString());
                        WebImage.SaveImage((BitmapSource)picBox.Source, imgFullName);
                        Growl.Success("已保存到本地文件夹");
                    }
                    catch (Exception ex)
                    {
                        Growl.Error("保存失败：" + ex.Message);
                        Common.LogHelper.WriteLog("保存失败：" + ex.Message, Common.EnumLogLevel.Error);
                    }
                    break;
                case Key.Escape:
                    this.Close();
                    break;
            }
        }
        int index = 0;
        private void ImageQueue_OnComplate(BitmapImage b)
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
                //this.Visibility = Visibility.Visible;
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

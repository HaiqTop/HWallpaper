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
        public class ImageQ
        {
            public ImageQ(BitmapImage b,string name)
            {
                this.StoryName = name;
                this.BitmapImage = b;
            }
            public BitmapImage BitmapImage { get;set ;}
            public string StoryName { get;set ; }
        }
        private ImageHelper imgHelper;
        private ImageQueue imageQueue = new ImageQueue();
        private Queue<ImageQ> imageList= new Queue<ImageQ>();
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
            EffectPicture(null,null);
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
            timerP.Stop();
            timerP.Dispose();
            timerL.Stop();
            timerL.Dispose();
            try
            {
                ConfigManage.Save();
                //Properties.Settings.Default.screen_StartIndex = imgHelper.TotalIndex;
                //Properties.Settings.Default.Save();
            }
            catch { }
            finally
            {
                this.Hide();
                //System.Threading.Thread.Sleep(1000);
                //this.Close();
            }
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
                string key = "closeDW";
                switch (index)
                {
                    case 0: key = "close_Left"; break;
                    case 1: key = "close_Right"; break;
                    case 2: key = "close_Bottom"; break;
                    case 3: key = "close_Top"; break;
                    case 4: key = "close_Opacity"; break;
                }
                index++;
                if (index > 4) index = 0;
                picBoxTemp.Source = picBox.Source;
                //Storyboard story = (base.Resources[key] as Storyboard);
                //story.Begin();


                picBox.Source = b;

                string newName = key.Replace("close_", "show_");
                newName = "story_Top";
                var story = (base.Resources[newName] as Storyboard);
                story.Begin();

                //imageList.Enqueue(new ImageQ(b,key));

                // 切换动画效果
                //AnimationType type = AnimationType.AW_HOR_POSITIVE;
                //type = GetRandomAnimationType();
                //AnimateWindow(picBox.GetHandle(), 1500, type | AnimationType.AW_ACTIVATE);
            }
            else
            { 
                picBox.Source = b;
                //this.Visibility = Visibility.Visible;
            }
        }

        private void ImageQueue_OnError(Exception e)
        {
            Growl.Error(e.Message);
        }
        private void Storyboard_Completed(object sender, EventArgs e)
        {
            if (imageList.Count > 0)
            {
                ImageQ imageQ = imageList.Dequeue();
                picBox.Source = imageQ.BitmapImage;
                string newName = imageQ.StoryName.Replace("close_", "show_");
                var story = (base.Resources[newName] as Storyboard);
                story.Begin();
            }
        }

    }
}

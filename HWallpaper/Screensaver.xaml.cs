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
using System.Windows.Input;
using System.Windows.Media.Imaging;
using static HWallpaper.Common.WinApi;

namespace HWallpaper
{
    /// <summary>
    /// Screensaver.xaml 的交互逻辑
    /// </summary>
    public partial class Screensaver : System.Windows.Window
    {
        private ImageHelper imgHelper;
        private ImageQueue imageQueue = new ImageQueue();
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
            imageQueue.OnComplate += ImageQueue_OnComplate;
        }

        private void ImageQueue_OnComplate(System.Windows.Controls.Image i, string u, BitmapImage b)
        {
            picBoxTemp.Source = picBox.Source;
            picBox.Visibility = Visibility.Hidden;
            // 切换动画效果
            AnimationType type = AnimationType.AW_HOR_POSITIVE;
            type = GetRandomAnimationType();
            AnimateWindow(picBox.GetHandle(), 1500, type | AnimationType.AW_ACTIVATE);
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
            labelD.Text = DateTime.Now.ToString("MM-dd") + " " + Common.Utils.GetWeetString();
            labelT.Text = DateTime.Now.ToString("HH:mm");
        }
        private void EffectPicture(object source, ElapsedEventArgs e)
        {
            ImgInfo info = imgHelper.GetNextImage();
            try
            {
                string fullName = System.IO.Path.Combine(ConfigManage.Base.CachePath, info.GetFileName());
                if (System.IO.File.Exists(fullName))
                {
                    imageQueue.Queue(picBox, fullName);
                    //picBox.Source = Common.Utils.ReadImageFile(fullName);
                }
                else
                {
                    imageQueue.Queue(picBox, info.url);
                }
            }
            catch (Exception ex)
            {
                Common.LogHelper.WriteLog(ex.Message, Common.EnumLogLevel.Error);
            }
        }
        #region 切换壁纸的效果相关方法
        private Random random = new Random();
        private AnimationType[] animationTypes = null;
        private AnimationType GetRandomAnimationType()
        {
            if (animationTypes == null)
            {
                animationTypes = Enum.GetValues(typeof(AnimationType)) as AnimationType[];
            }
            return animationTypes[random.Next(0, 4)];
        }
        private int iIndex = 0;
        private AnimationType GetOrderAnimationType()
        {
            if (animationTypes == null)
            {
                animationTypes = Enum.GetValues(typeof(AnimationType)) as AnimationType[];
            }
            if (iIndex > 4) iIndex = 0;
            return animationTypes[iIndex++];
        }
        #endregion 切换壁纸的效果相关方法

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
                //Properties.Settings.Default.screen_StartIndex = imgHelper.TotalIndex;
                //Properties.Settings.Default.Save();
            }
            catch { }
            finally
            {
                this.Hide();
                System.Threading.Thread.Sleep(1000);
                this.Close();
            }
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
                        BitmapSource bitmap = (BitmapSource)picBox.Source;
                        PngBitmapEncoder PBE = new PngBitmapEncoder();
                        PBE.Frames.Add(BitmapFrame.Create(bitmap));
                        using (Stream stream = File.Create(imgFullName))
                        {
                            PBE.Save(stream);
                        }
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
    }
}

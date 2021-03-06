using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.IO;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using HWallpaper.Model;

namespace HWallpaper.Common
{
    /// <summary>
    /// 壁纸下载队列（开启一个单线程）
    /// </summary>
    public class ImageQueue
    {
        /// <summary>
        /// 队列下载需要的参数封装
        /// </summary>
        class ImageQueueInfo
        {
            public Image image { get; set; }
            public ImgInfo ImgInfo { get; set; }
            public string Url { get; set; }
            public string Name { get; set; }
        }
        /// <summary>
        ///  下载完成委托
        /// </summary>
        /// <param name="b"></param>
        /// <param name="imgInfo"></param>
        public delegate void ComplateDelegate(BitmapImage b, ImgInfo imgInfo);
        /// <summary>
        /// 下载异常委托
        /// </summary>
        /// <param name="e"></param>
        public delegate void ErrorDelegate(Exception e);
        public event ComplateDelegate OnComplate;
        public event ErrorDelegate OnError;
        private AutoResetEvent autoEvent;
        /// <summary>
        /// 队列
        /// </summary>
        private Queue<ImageQueueInfo> Stacks;

        /// <summary>
        /// 缓存路径
        /// </summary>
        public string CachePath;
        public ImageQueue()
        {
            this.Stacks = new Queue<ImageQueueInfo>();
            autoEvent = new AutoResetEvent(true);
            Thread t = new Thread(new ThreadStart(this.DownloadImage));
            t.Name = "下载图片";
            t.IsBackground = true;
            t.Start();
        }

        /// <summary>
        /// 下载方法
        /// </summary>
        private void DownloadImage()
        {
            while (true)
            {
                ImageQueueInfo t = null;
                lock (this.Stacks)
                {
                    if (this.Stacks.Count > 0)
                    {
                        t = this.Stacks.Dequeue();
                    }
                }
                if (t != null)
                {
                    Uri uri = new Uri(t.Url);
                    BitmapImage bImage = null;
                    try
                    {
                        bool local = false;
                        string fileFullName = string.Empty;
                        if (!string.IsNullOrEmpty(this.CachePath))
                        {
                            fileFullName = Path.Combine(this.CachePath, t.Name);
                            local = File.Exists(fileFullName);
                        }

                        if (!local && "http".Equals(uri.Scheme, StringComparison.CurrentCultureIgnoreCase))
                        {
                            //如果是HTTP下载文件
                            WebClient wc = new WebClient();
                            using (var ms = new MemoryStream(wc.DownloadData(uri)))
                            {
                                // 是否设置了缓存路径，有就先保存到缓存目录
                                if (!string.IsNullOrEmpty(this.CachePath))
                                {
                                    System.Drawing.Image image = System.Drawing.Image.FromStream(ms);
                                    if (image != null)
                                    {
                                        if (!File.Exists(fileFullName))
                                        { 
                                            image.Save(fileFullName);
                                        }
                                        image.Dispose();
                                        local = true;
                                    }
                                }
                                // 是否有完成后的委托，有就获取BitmapImage
                                else if (this.OnComplate != null)
                                {
                                    bImage = new BitmapImage();
                                    bImage.BeginInit();
                                    bImage.CacheOption = BitmapCacheOption.OnLoad;
                                    bImage.StreamSource = ms;
                                    bImage.EndInit();
                                }
                            }
                        }
                        // 从本地获取
                        if (local && this.OnComplate != null)
                        {
                            while (WinApi.FileIsOccupy(fileFullName))
                            {
                                Thread.Sleep(500);
                            }
                            using (var fs = new FileStream(fileFullName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                            {
                                bImage = new BitmapImage();
                                bImage.BeginInit();
                                bImage.CacheOption = BitmapCacheOption.OnLoad;
                                bImage.StreamSource = fs;
                                bImage.EndInit();
                            }
                        }
                        if (bImage != null && this.OnComplate != null && t.image != null)
                        {
                            if (bImage.CanFreeze) bImage.Freeze();
                            t.image.Dispatcher.BeginInvoke(new Action<Image,BitmapImage,string,ImgInfo>((image,bmp,name,imgInfo) =>
                            {
                                image.Tag = name;
                                this.OnComplate(bmp, imgInfo);

                            }), new Object[] { t.image,bImage, t.Name ,t.ImgInfo});
                        }
                    }
                    catch (Exception e)
                    {
                        LogHelper.WriteLog(e.Message,EnumLogLevel.Error);
                        if (this.OnError != null)
                        {
                            t.image.Dispatcher.BeginInvoke(new Action<Exception>((ex) =>
                            {
                                this.OnError(ex);

                            }), new Object[] { e });
                        }
                    }
                }
                if (this.Stacks.Count > 0) continue;
                autoEvent.WaitOne();
            }
        }

        /// <summary>
        /// 添加下载任务到队列
        /// </summary>
        /// <param name="url"></param>
        /// <param name="name"></param>
        public void Queue(string url,string name)
        {
            if (String.IsNullOrEmpty(url)) return;
            lock (this.Stacks)
            {
                this.Stacks.Enqueue(new ImageQueueInfo() { Url = url,Name = name});
                this.autoEvent.Set();
            }
        }

        /// <summary>
        /// 添加下载任务到队列
        /// </summary>
        /// <param name="url"></param>
        /// <param name="image"></param>
        /// <param name="name"></param>
        public void Queue(string url, Image image,string name)
        {
            if (String.IsNullOrEmpty(url)) return;
            lock (this.Stacks)
            {
                this.Stacks.Enqueue(new ImageQueueInfo() { Url = url, image = image ,Name = name});
                this.autoEvent.Set();
            }
        }

        /// <summary>
        /// 添加下载任务到队列
        /// </summary>
        /// <param name="image"></param>
        /// <param name="imgInfo"></param>
        public void Queue(Image image, ImgInfo imgInfo)
        {
            if (imgInfo == null) return;
            lock (this.Stacks)
            {
                this.Stacks.Enqueue(new ImageQueueInfo() { ImgInfo = imgInfo, image = image,Url = imgInfo.url, Name = imgInfo.GetFileName()});
                this.autoEvent.Set();
            }
        }
    }
}

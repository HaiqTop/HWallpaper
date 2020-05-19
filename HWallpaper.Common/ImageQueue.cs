using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.IO;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace HWallpaper.Common
{
    public class ImageQueue
    {
        class ImageQueueInfo
        {
            public Image image { get; set; }
            public string Url { get; set; }
            public string Name { get; set; }
        }
        public delegate void ComplateDelegate(BitmapImage b);
        public event ComplateDelegate OnComplate;
        private AutoResetEvent autoEvent;
        private Queue<ImageQueueInfo> Stacks;

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
                                        image.Save(fileFullName);
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
                            using (var fs = new FileStream(fileFullName, FileMode.Open))
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
                            t.image.Dispatcher.BeginInvoke(new Action<BitmapImage>((bmp) =>
                            {
                                this.OnComplate(bmp);
                                
                            }), new Object[] { bImage });
                        }
                    }
                    catch (Exception e)
                    {
                        System.Windows.MessageBox.Show(e.Message);
                        continue;
                    }
                }
                if (this.Stacks.Count > 0) continue;
                autoEvent.WaitOne();
            }
        }
        public void Queue(string url,string name)
        {
            if (String.IsNullOrEmpty(url)) return;
            lock (this.Stacks)
            {
                this.Stacks.Enqueue(new ImageQueueInfo() { Url = url,Name = name});
                this.autoEvent.Set();
            }
        }
        public void Queue(string url, Image image,string name)
        {
            if (String.IsNullOrEmpty(url)) return;
            lock (this.Stacks)
            {
                this.Stacks.Enqueue(new ImageQueueInfo() { Url = url, image = image ,Name=name});
                this.autoEvent.Set();
            }
        }

    }
}

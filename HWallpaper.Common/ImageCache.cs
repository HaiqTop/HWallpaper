using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.IO;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace HWallpaper.Common
{
    /// <summary>
    /// 壁纸缓存队列（开启一个单线程）
    /// </summary>
    public class ImageCache
    {
        class ImageQueueInfo
        {
            public string Url { get; set; }
            public string FullName { get; set; }
        }
        private AutoResetEvent autoEvent;
        private Queue<ImageQueueInfo> Stacks;

        public ImageCache()
        {
            this.Stacks = new Queue<ImageQueueInfo>();
            autoEvent = new AutoResetEvent(true);
            Thread t = new Thread(new ThreadStart(this.DownloadImage));
            t.Name = "缓存图片";
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
                    if (!File.Exists(t.FullName))
                    {
                        WebHelper.DownImage(t.Url, t.FullName);
                    }
                }
                if (this.Stacks.Count > 0) continue;
                autoEvent.WaitOne();
            }
        }
        public void Queue(string url,string fullName)
        {
            if (String.IsNullOrEmpty(url)) return;
            lock (this.Stacks)
            {
                this.Stacks.Enqueue(new ImageQueueInfo() { Url = url, FullName = fullName });
                this.autoEvent.Set();
            }
        }

    }
}

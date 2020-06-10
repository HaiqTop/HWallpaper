using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.IO;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using HWallpaper.Model;
using System.Drawing.Imaging;
using HWallpaper.Common;

namespace HWallpaper.Business
{
    public class DownQueue
    {
        enum TaskState
        { 
            None,
            Complate,
            Error
        }
        class ImageQueueInfo
        {
            public Image Image { get; set; }
            public ImgInfo ImgInfo { get; set; }
            public string Url { get; set; }
            public TaskState State { get; set; }
            public Exception Error { get; set; }
            public BitmapImage BitmapImage { get; set; }
        }
        public delegate void ComplateDelegate(Image i,BitmapImage b, ImgInfo imgInfo);
        public delegate void ErrorDelegate(Exception e);
        public event ComplateDelegate OnComplate;
        public event ErrorDelegate OnError;
        private System.Windows.FrameworkElement pElement;
        private AutoResetEvent autoEvent;
        private Queue<ImageQueueInfo> Stacks;

        string CachePath,DownPath;
        bool needBitmap;
        bool needChche;
        public DownQueue(System.Windows.FrameworkElement curElement,bool needBitmap = true)
        {
            this.pElement = curElement;
            this.needBitmap = needBitmap;
            this.CachePath = ConfigManage.Base.CachePath;
            this.DownPath = ConfigManage.Base.DownPath;
            this.needChche = ConfigManage.Base.Cache;
            this.Stacks = new Queue<ImageQueueInfo>();
            autoEvent = new AutoResetEvent(true);
            Thread t = new Thread(new ThreadStart(this.Monitor));
            t.Name = "图片下载的任务监听线程池";
            t.IsBackground = true;
            t.Start();
        }

        private void Monitor()
        {
            ThreadPool.SetMinThreads(1, 1);
            ThreadPool.SetMaxThreads(4, 4);
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
                    switch (t.State)
                    {
                        case TaskState.None:
                            ThreadPool.QueueUserWorkItem(new WaitCallback(DownloadImage), t);
                            break;
                        case TaskState.Complate:
                            if (this.OnComplate != null && t.BitmapImage != null)
                            {
                                System.Windows.FrameworkElement ele = t.Image;
                                if (ele == null) ele = this.pElement;

                                if (ele != null)
                                {
                                    ele.Dispatcher.BeginInvoke(new Action<Image, BitmapImage, ImgInfo>((image, bmp, imgInfo) =>
                                    {
                                        this.OnComplate(image, bmp, imgInfo);
                                    }), new Object[] { t.Image, t.BitmapImage, t.ImgInfo });
                                }
                            }
                            break;
                        case TaskState.Error:
                            if (this.OnError != null)
                            {
                                System.Windows.FrameworkElement ele = t.Image;
                                if (ele == null) ele = this.pElement;

                                if (ele != null)
                                {
                                    ele.Dispatcher.BeginInvoke(new Action<Image, ImgInfo, Exception>((i, info, ex) =>
                                    {
                                        this.OnError(ex);
                                    }), new Object[] { t.Image, t.ImgInfo, t.Error });
                                }
                            }
                            break;
                    }
                }
                if (this.Stacks.Count > 0) continue;
                autoEvent.WaitOne();
            }
        }
        private void DownloadImage(object obj)
        {
            if (obj != null && obj is ImageQueueInfo info)
            {
                BitmapImage bImage = null;
                try
                {
                    // 获取文件全路径（优先从下载目录中找，找不到就确定目录为缓存目录）
                    string name = info.ImgInfo.GetFileName();
                    string downFullName = Path.Combine(this.DownPath, name);
                    string cacheFullName = Path.Combine(this.CachePath, name);
                    string fileFullName = File.Exists(downFullName) ? downFullName : cacheFullName;
                    // 如果Url为空，则表示图片是自定义大小的图片，自定义的大小的图片不进行缓存
                    bool originalImage = string.IsNullOrEmpty(info.Url);

                    #region 获取图片
                    FileInfo file = new FileInfo(fileFullName);
                    if (originalImage && file.Exists && file.Length > 0)
                    {
                        // 从本地获取
                        if (this.needBitmap)
                        {
                            while (WinApi.FileIsOccupy(fileFullName))
                            {
                                Thread.Sleep(500);
                            }
                            try
                            {
                                using (var fs = new FileStream(fileFullName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                                {
                                    bImage = new BitmapImage();
                                    bImage.BeginInit();
                                    bImage.CacheOption = BitmapCacheOption.OnLoad;
                                    bImage.StreamSource = fs;
                                    bImage.EndInit();
                                }
                            }
                            catch (System.NotSupportedException)
                            {
                                file.Delete();
                                throw;
                            }
                        }
                    }
                    else
                    {
                        // 网络获取
                        WebClient wc = new WebClient();
                        string url = string.IsNullOrEmpty(info.Url) ? info.ImgInfo.url : info.Url;
                        byte[] bt;
                        try
                        {
                            bt = wc.DownloadData(url);
                        }
                        catch
                        {
                            bt = wc.DownloadData(info.ImgInfo.url);
                        }
                        using (var stream = new MemoryStream(bt))
                        {
                            if (stream != null)
                            {
                                if (this.needBitmap)
                                {
                                    bImage = new BitmapImage();
                                    bImage.BeginInit();
                                    bImage.CacheOption = BitmapCacheOption.OnLoad;
                                    bImage.StreamSource = stream;
                                    bImage.EndInit();
                                }
                                if (this.needChche && originalImage)
                                {
                                    using (System.Drawing.Image img = System.Drawing.Image.FromStream(stream))
                                    {
                                        img.Save(cacheFullName);
                                    }
                                }
                                stream.Close();
                            }
                            else
                            {
                                throw new Exception("图片下载失败");
                            }
                        }
                    }
                    #endregion
                    if (bImage.CanFreeze) bImage.Freeze();
                    // 任务完成，推送到队列
                    lock (this.Stacks)
                    {
                        info.BitmapImage = bImage;
                        info.State = TaskState.Complate;
                        this.Stacks.Enqueue(info);
                        this.autoEvent.Set();
                    }
                }
                catch (Exception e)
                {
                    LogHelper.WriteLog(e.Message, EnumLogLevel.Error);
                    lock (this.Stacks)
                    {
                        info.State = TaskState.Error;
                        info.Error = e;
                        this.Stacks.Enqueue(info);
                        this.autoEvent.Set();
                    }
                }
            }
        }
        public void Queue(Image image, ImgInfo imgInfo,string url = "")
        {
            if (imgInfo == null) return;
            lock (this.Stacks)
            {
                this.Stacks.Enqueue(new ImageQueueInfo() { ImgInfo = imgInfo, Image = image,Url = url, State = TaskState.None});
                this.autoEvent.Set();
            }
        }
    }
}

using System;
using System.Windows.Controls;
using System.IO;
using System.Net;
using System.Threading;
using System.Windows.Media.Imaging;

namespace HWallpaper.Common
{
    public class ImageDown
    {
        #region 辅助类别
        public class ImageDownInfo
        {
            public Image image { get; set; }
            public string url { get; set; }
        }
        #endregion

        public static event ComplateDelegate OnComplate;
        public delegate void ComplateDelegate(Image i, string u, BitmapImage b);
        public static event ErrorDelegate OnError;
        public delegate void ErrorDelegate(Image i, Exception ex);

        public static void DownloadImage(Image img, String url)
        {
            ImageDownInfo downInfo = new ImageDownInfo() { url = url, image = img };
            Thread t = new Thread(new ParameterizedThreadStart(ImageDown.DownloadImage));
            t.Name = "下载图片";
            t.IsBackground = true;
            t.Start(downInfo);
        }
        private static void DownloadImage(object obj)
        {
            if (obj != null && obj is ImageDownInfo)
            {
                ImageDownInfo downInfo = obj as ImageDownInfo;

                Uri uri = new Uri(downInfo.url);
                BitmapImage image = null;
                try
                {
                    if ("http".Equals(uri.Scheme, StringComparison.CurrentCultureIgnoreCase))
                    {
                        //如果是HTTP下载文件
                        WebClient wc = new WebClient();
                        using (var ms = new MemoryStream(wc.DownloadData(uri)))
                        {
                            image = new BitmapImage();
                            image.BeginInit();
                            image.CacheOption = BitmapCacheOption.OnLoad;
                            image.StreamSource = ms;
                            image.EndInit();
                        }
                    }
                    else if ("file".Equals(uri.Scheme, StringComparison.CurrentCultureIgnoreCase))
                    {
                        using (var fs = new FileStream(downInfo.url, FileMode.Open))
                        {
                            image = new BitmapImage();
                            image.BeginInit();
                            image.CacheOption = BitmapCacheOption.OnLoad;
                            image.StreamSource = fs;
                            image.EndInit();
                        }
                    }
                }
                catch (Exception ex)
                {
                    downInfo.image.Dispatcher.BeginInvoke(new Action<ImageDownInfo, BitmapImage>((i, bmp) =>
                    {
                        if (ImageDown.OnError != null)
                        {
                            ImageDown.OnError(i.image, ex);
                        }
                    }), new Object[] { downInfo, image });
                }
                try
                {

                    if (image != null)
                    {
                        if (image.CanFreeze) image.Freeze();
                        downInfo.image.Dispatcher.BeginInvoke(new Action<ImageDownInfo, BitmapImage>((i, bmp) =>
                        {
                            if (ImageDown.OnComplate != null)
                            {
                                ImageDown.OnComplate(i.image, i.url, bmp);
                            }
                        }), new Object[] { downInfo, image });
                    }
                }
                catch (Exception e)
                {
                    System.Windows.MessageBox.Show(e.Message);
                }
            }

        }
    }
}

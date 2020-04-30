using System;
using System.IO;
using System.Windows.Media.Imaging;

namespace HWallpaper.Common
{
    public class Utils
    {
        /// <summary>
        /// 获取文字星期几
        /// </summary>
        /// <returns></returns>
        public static string GetWeetString()
        {
            string[] Day = new string[] { "周日", "周一", "周二", "周三", "周四", "周五", "周六" };
            string week = Day[Convert.ToInt32(DateTime.Now.DayOfWeek.ToString("d"))].ToString();
            return week;
        }
        /// <summary>
        /// 根据下拉框选项获取秒数
        /// </summary>
        /// <param name="text"></param>
        public static long GetSecondTime(string text)
        {
            int tempIndex = text.Length - 2;
            string specs = text.Remove(0, tempIndex);
            long time = Convert.ToInt32(text.Remove(tempIndex, 2));
            switch (specs)
            {
                case "分钟": time *= 60; break;
                case "小时": time *= 3600; break;
            }
            return time;
        }


        /// <summary>
        /// 通过FileStream 来打开文件，这样就可以实现不锁定Image文件，到时可以让多用户同时访问Image文件
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static BitmapImage ReadImageFile(string path)
        {
            BitmapImage image;
            using (var fs = new FileStream(path, FileMode.Open))
            {
                image = new BitmapImage();
                image.BeginInit();
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.StreamSource = fs;
                image.EndInit();
            }
            if (image != null)
            { 
                if (image.CanFreeze) image.Freeze();
            }
            return image;
        }
    }
}

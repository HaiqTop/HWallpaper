using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HWallpaper.Model
{
    public class Config
    {
        public string TypeJson { get; set; }
        /// <summary>
        /// 开机启动
        /// </summary>
        public bool AutoOn { get; set; }
        public string DownPath { get; set; }
        /// <summary>
        /// 是否缓存壁纸
        /// </summary>
        public bool Cache { get; set; }
        /// <summary>
        /// 是否自动清理过期缓存
        /// </summary>
        public bool AutoClearCache { get; set; }
        public string CachePath { get; set; }
        public bool screen_Animation { get; set; }
        public AnimationType screen_AnimationType { get; set; }
        /// <summary>
        /// 屏保开关
        /// </summary>
        public bool screen_OnOff { get; set; }
        /// <summary>
        /// 自动替换壁纸
        /// </summary>
        public string wallpaper_AutoReplace { get; set; }
        /// <summary>
        /// 自动切换壁纸类型
        /// </summary>
        public string wallpaper_SelectedTypes { get; set; }
        /// <summary>
        /// 屏保壁纸类型
        /// </summary>
        public string screen_SelectedTypes { get; set; }

        /// <summary>
        /// 壁纸替换时间
        /// </summary>
        public int wallpaper_Time { get; set; }
        /// <summary>
        /// 壁纸替换时间单位
        /// </summary>
        public WallTimeType WallTimeType { get; set; }
    }
    public enum AnimationType
    { 
        Order,

    }
    public enum WallTimeType
    {
        Day = 0,
        Hour = 1,
        Minute = 2
    }
}

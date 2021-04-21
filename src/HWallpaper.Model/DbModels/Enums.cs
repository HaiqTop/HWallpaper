using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HWallpaper.Model
{
    /// <summary>
    /// 1：喜欢，-1：不喜欢，0：默认值
    /// </summary>
    public enum LoveType
    {
        Love = 1,
        None = 0,
        Dislike = -1
    }
    /// <summary>
    /// 使用类型（1：自动壁纸，2：手动壁纸，2：自动屏保）
    /// </summary>
    public enum RecordType
    {
        /// <summary>
        /// 自动壁纸
        /// </summary>
        AutoWallpaper = 1,
        /// <summary>
        /// 手动壁纸
        /// </summary>
        ManualWallpaper = 2,
        /// <summary>
        /// 自动屏保
        /// </summary>
        AutoScreensaver = 3
    }
}

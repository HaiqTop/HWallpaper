using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HWallpaper.Model
{
    public class Love
    {
        /// <summary>
        /// 壁纸ID
        /// </summary>
        [SqlSugar.SugarColumn(IsPrimaryKey = true)]//指定主键，当然数据库中也要设置主键和自增列才会有效
        public int PictureId { get; set; }
        /// <summary>
        /// 操作时间
        /// </summary>
        public DateTime Time { get; set; }
        /// <summary>
        /// 1：喜欢，-1：不喜欢，0：默认值
        /// </summary>
        public int Type { get; set; }
    }
}

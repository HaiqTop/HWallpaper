using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HWallpaper.Model
{
    public class Download
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
        /// 存储的文件名（包括路径）
        /// </summary>
        public string FullName { get; set; }
        /// <summary>
        /// 1：有效，0：无效
        /// </summary>
        public int Valid { get; set; }
    }
}

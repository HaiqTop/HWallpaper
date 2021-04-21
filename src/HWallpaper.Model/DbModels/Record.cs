using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HWallpaper.Model
{
    /// <summary>
    /// 壁纸使用记录表
    /// </summary>
    public class Record
    {
        /// <summary>
        /// 记录主键
        /// </summary>
        [SqlSugar.SugarColumn(IsPrimaryKey = true, IsIdentity = true)]//指定主键和自增列，当然数据库中也要设置主键和自增列才会有效
        public int Id { get; set; }
        /// <summary>
        /// 使用类型（1：自动壁纸，2：手动壁纸，2：自动屏保）
        /// </summary>
        public int Type { get; set; }
        /// <summary>
        /// 使用时间
        /// </summary>
        public DateTime Time { get; set; }
        /// <summary>
        /// 壁纸主键
        /// </summary>
        public int PictureId { get; set; }
    }
}

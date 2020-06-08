using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HWallpaper.Model
{
    /// <summary>
    /// 壁纸标签记录表
    /// </summary>
    public class TagRecord
    {
        /// <summary>
        /// 记录主键
        /// </summary>
        [SqlSugar.SugarColumn(IsPrimaryKey = true, IsIdentity = true)]//指定主键和自增列，当然数据库中也要设置主键和自增列才会有效
        public int Id { get; set; }
        /// <summary>
        /// 壁纸标签名称
        /// </summary>
        public string TagName { get; set; }
        /// <summary>
        /// 收藏总数
        /// </summary>
        public int RecordCount { get; set; }
        /// <summary>
        /// 评分总数
        /// </summary>
        public int ScoreSum { get; set; }
    }
}

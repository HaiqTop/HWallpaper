using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HWallpaper.Model
{
    public class Picture
    {
        /// <summary>
        /// 壁纸ID
        /// </summary>
        [SqlSugar.SugarColumn(IsPrimaryKey = true)]//指定主键和自增列，当然数据库中也要设置主键和自增列才会有效
        public int Id { get; set; }
        /// <summary>
        /// 壁纸分类ID
        /// </summary>
        public int Type { get; set; }
        /// <summary>
        /// 图片名称（类型id + 图片id + 文件后缀名）
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Url地址
        /// </summary>
        public string Url { get; set; }
        /// <summary>
        /// 我的评分
        /// </summary>
        public int Score { get; set; }
        /// <summary>
        /// 1：喜欢，-1：不喜欢，0：默认值
        /// </summary>
        public int Love { get; set; }
    }
}

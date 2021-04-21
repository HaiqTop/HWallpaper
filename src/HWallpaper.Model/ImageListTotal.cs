using System.Collections.Generic;

namespace HWallpaper.Model
{
    public class ImageListTotal
    {
        private int consume { get; set; }
        /// <summary>
        /// 总记录数
        /// </summary>
        public int total { get; set; }
        /// <summary>
        /// 总任务进度
        /// </summary>
        public int TotalIndex { get; set; }
        /// <summary>
        /// 当前任务进度
        /// </summary>
        public int CurIndex { get; set; }
        public string errmsg { get; set; }
        public int errno { get; set; }

        public List<ImgInfo> data { get; set; }

    }
}

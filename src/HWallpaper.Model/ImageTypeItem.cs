using System;
using System.Collections.Generic;

namespace HWallpaper.Model
{
    public class ImageTypeItem
    {
        /// <summary>
        /// 壁纸类型Code
        /// </summary>
        public string Type { get; set; }
        /// <summary>
        /// 当前类型壁纸总数
        /// </summary>
        public int Total { get; set; }
        /// <summary>
        /// 当前数据列表第一条记录在总记录中的Index数
        /// </summary>
        public int StartIndex { get; set; }
        /// <summary>
        /// 当前数据列表最后一条记录在总记录中的Index数
        /// </summary>
        public int EndIndex { 
            get
            {
                if (this.Images == null)
                {
                    throw new Exception("请先获取图片数据记录");
                }    
                return this.StartIndex + this.Images.Count - 1;
            } 
        }
        public List<ImgInfo> Images { get; set; }

        public ImageTypeItem(string Type, int Index, int total)
        {
            this.Type = Type;
            this.StartIndex = Index;
            this.Total = total;
            this.Images = new List<ImgInfo>();
        }
        public ImageTypeItem(string Type, int Index, int total, List<ImgInfo> Images)
        {
            this.Type = Type;
            this.StartIndex = Index;
            this.Total = total;
            this.Images = Images;
        }
        public ImageTypeItem(string Type, int Index, ImageListTotal total)
        {
            this.Type = Type;
            this.StartIndex = Index;
            this.Total = total.total;
            this.Images = total.data;
        }
    }
}

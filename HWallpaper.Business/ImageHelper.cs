using System;
using System.Collections.Generic;
using System.Linq;
using HWallpaper.Model;

namespace HWallpaper.Business
{
    public class ImageHelper
    {
        /// <summary>
        /// 单次请求的壁纸数量
        /// </summary>
        private const int Count = 10;
        /// <summary>
        /// MaxCount * types.Length
        /// </summary>
        public int Total { set; get; }
        /// <summary>
        /// [ 0, Total )
        /// </summary>
        public int TotalIndex { set; get; }
        /// <summary>
        /// 获取所选类别中壁纸数量最少的值
        /// </summary>
        public int MaxCount { set; get; }
        /// <summary>
        /// [ 0, MaxCount )
        /// </summary>
        public int CurIndex { set; get; }
        private List<ImageListTotal> typeList = new List<ImageListTotal>();
        private int start = 0;

        public ImageHelper(string[] types, int totalIndex = 0)
        {
            this.TotalIndex = totalIndex;

            start = (this.TotalIndex + 1) / types.Length;
            this.CurIndex = (this.TotalIndex + 1) % types.Length;
            foreach (string type in types)
            {
                ImageListTotal curImgList = WebImage.GetImageList(Convert.ToInt32(type), start, Count);
                typeList.Add(curImgList);
                // 获取壁纸最少的，作为壁纸上限
                if (this.MaxCount == 0 || this.MaxCount > curImgList.total)
                {
                    this.MaxCount = curImgList.total;
                }
            }
            this.Total = this.MaxCount * types.Length;
        }
        /// <summary>
        /// 获取下一张壁纸
        /// </summary>
        /// <returns></returns>
        public ImgInfo GetNextImage()
        {
            this.CurIndex++;
            this.TotalIndex++;
            if (!(this.TotalIndex < this.Total))
            {
                this.TotalIndex = 0;
                this.CurIndex = 0;
            }

            int imgIndex = this.CurIndex / typeList.Count;
            int typeIndex = this.CurIndex % typeList.Count;

            ImageListTotal curImgTotal = typeList[typeIndex];
            if (imgIndex < curImgTotal.data.Count)
            {
                return curImgTotal.data[imgIndex];
            }
            else
            {
                if (typeIndex == 0) this.start += Count;
                typeList[typeIndex] = WebImage.GetImageList(curImgTotal.data[0].class_id, this.start, Count);
                if (typeIndex == (typeList.Count - 1))
                {
                    this.CurIndex = typeList.Count - 1;
                }
                return curImgTotal.data[0];
            }
        }
    }
}

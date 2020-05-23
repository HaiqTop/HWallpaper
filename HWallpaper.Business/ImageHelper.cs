using System;
using System.Collections.Generic;
using System.Linq;
using HWallpaper.Common;
using HWallpaper.Model;

namespace HWallpaper.Business
{
    public class ImageHelper
    {
        private Random random = new Random();
        private int orderNum = 0;
        private ImageCache cache;
        private string CachePath;
        /// <summary>
        /// 单次请求的壁纸数量
        /// </summary>
        private int Count = 10;
        public Dictionary<int, int> TypeIndexs;
        private List<ImageListTotal> typeList = new List<ImageListTotal>();
        private Dictionary<int, ImageTypeItem> typeImageList = new Dictionary<int, ImageTypeItem>();

        public ImageHelper(Dictionary<int, int> typeIndexs,string cachePath = "")
        {
            this.TypeIndexs = typeIndexs;
            if (!string.IsNullOrEmpty(cachePath))
            {
                this.CachePath = cachePath;
                cache = new ImageCache();
            }
            LoadNextImageList();
        }

        private void LoadNextImageList()
        {
            ImageListTotal imgList;
            ImageTypeItem typeItem;
            foreach (var typeIndex in this.TypeIndexs)
            {
                // 获取总数
                if (!typeImageList.ContainsKey(typeIndex.Key))
                {
                    imgList = WebImage.GetImageList(typeIndex.Key, 0, 1);
                    typeItem = new ImageTypeItem(typeIndex.Key, typeIndex.Value, imgList.total);
                    typeImageList.Add(typeIndex.Key, typeItem);
                }
                // 判断当前
                if (typeIndex.Value < typeImageList[typeIndex.Key].Total)
                {
                    imgList = WebImage.GetImageList(typeIndex.Key, typeIndex.Value, this.Count);
                    typeImageList[typeIndex.Key].Images.AddRange(imgList.data);
                    CacheIage(imgList.data);
                }
            }
        }
        private void CacheIage(List<ImgInfo> imgList)
        {
            if (!string.IsNullOrEmpty(this.CachePath))
            {
                foreach (var item in imgList)
                {
                    cache.Queue(item.url, System.IO.Path.Combine(this.CachePath, item.GetFileName()));
                }
            }
        }
        /// <summary>
        /// 获取下一张壁纸
        /// </summary>
        /// <returns></returns>
        public ImgInfo GetNextImage()
        {
            if (orderNum >= this.TypeIndexs.Count)
            {
                orderNum = 0;
            }
            // 获取类型
            int type = this.TypeIndexs.ElementAt(orderNum++).Key;
            //int type = this.TypeIndexs.ElementAt(random.Next(0, this.TypeIndexs.Count - 1)).Key;

            this.TypeIndexs[type]++;
            int typeIndex = this.TypeIndexs[type];
            // 判断是否超过最大记录数据
            if (typeIndex >= this.typeImageList[type].Total)
            {
                this.TypeIndexs[type] = 0;
                typeIndex = 0;
            }
            // 判断是否超过当前获取到的记录
            if (typeIndex < this.typeImageList[type].StartIndex)
            {
                ImageListTotal imgList = WebImage.GetImageList(type, typeIndex, this.Count);
                CacheIage(imgList.data);
                typeImageList[type].StartIndex = typeIndex;
                typeImageList[type].Images = imgList.data;

            }
            // 判断是否超过当前获取到的记录
            if (typeIndex > this.typeImageList[type].EndIndex)
            {
                ImageListTotal imgList = WebImage.GetImageList(type, typeIndex, this.Count);
                CacheIage(imgList.data);
                typeImageList[type].Images.AddRange(imgList.data);
            }
            return typeImageList[type].Images[typeIndex - this.typeImageList[type].StartIndex];
        }
    }
}

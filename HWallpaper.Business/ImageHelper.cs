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
        public Dictionary<string, int> TypeIndexs;
        private List<ImageListTotal> typeList = new List<ImageListTotal>();
        private Dictionary<string, ImageTypeItem> typeImageList = new Dictionary<string, ImageTypeItem>();

        public ImageHelper(Dictionary<string, int> typeIndexs,string cachePath = "")
        {
            this.TypeIndexs = typeIndexs;
            if (!string.IsNullOrEmpty(cachePath))
            {
                this.CachePath = cachePath;
                cache = new ImageCache();
            }
            LoadImageList();
        }

        private void LoadImageList()
        {
            ImageListTotal imgList;
            ImageTypeItem typeItem;
            foreach (var typeIndex in this.TypeIndexs)
            {
                // 获取总数
                if (!typeImageList.ContainsKey(typeIndex.Key))
                {
                    imgList = GetImageListTotal(typeIndex.Key, 0, 1);
                    typeItem = new ImageTypeItem(typeIndex.Key, typeIndex.Value, imgList.total);
                    typeImageList.Add(typeIndex.Key, typeItem);
                }
                // 判断当前
                if (typeIndex.Value < typeImageList[typeIndex.Key].Total)
                {
                    imgList = GetImageListTotal(typeIndex.Key, typeIndex.Value, this.Count);
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
            string type = this.TypeIndexs.ElementAt(orderNum++).Key;
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
                ImageListTotal imgList = GetImageListTotal(type, typeIndex, this.Count);
                if(type != "down")
                {
                    CacheIage(imgList.data);
                }
                typeImageList[type].StartIndex = typeIndex;
                typeImageList[type].Images = imgList.data;

            }
            // 判断是否超过当前获取到的记录
            if (typeIndex > this.typeImageList[type].EndIndex)
            {
                ImageListTotal imgList = GetImageListTotal(type, typeIndex, this.Count);
                if (type != "down")
                {
                    CacheIage(imgList.data);
                }
                typeImageList[type].Images.AddRange(imgList.data);
            }
            return typeImageList[type].Images[typeIndex - this.typeImageList[type].StartIndex];
        }
        /// <summary>
        /// 根据类型获取分页的图片数据（包括：壁纸分类、收藏、下载）
        /// </summary>
        /// <param name="type"></param>
        /// <param name="start"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public static ImageListTotal GetImageListTotal(string type, int start = 0, int count = 24)
        {
            ImageListTotal total = null;
            if (type == "love")
            {
                total = UserDataManage.GetLoveList(LoveType.Love, start, count);
            }
            else if (type == "down")
            {
                total = UserDataManage.GetDownList(start, count);
            }
            else
            {
                total = WebImage.GetImageList(type, start, count);
                /*// 暂时不考虑，没有想到比较好的解决方法
                if (ConfigManage.Base.ExcludeDislike)
                {

                }*/
            }
            return total;
        }
    }
}

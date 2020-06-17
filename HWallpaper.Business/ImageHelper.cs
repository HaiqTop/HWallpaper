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
                    typeItem = new ImageTypeItem(typeIndex.Key, typeIndex.Value + 1, imgList.total);
                    typeImageList.Add(typeIndex.Key, typeItem);
                }
                // 判断当前
                if (typeIndex.Value + 1 < typeImageList[typeIndex.Key].Total)
                {
                    imgList = GetImageListTotal(typeIndex.Key, typeIndex.Value + 1, this.Count);
                    typeImageList[typeIndex.Key].Images.AddRange(imgList.data);
                    if (typeIndex.Key != "down")
                    {
                        CacheIage(imgList.data);
                    }
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
            string type = string.Empty;
            do
            {
                if (orderNum >= this.TypeIndexs.Count)
                {
                    orderNum = 0;
                }
                // 获取类型
                type = this.TypeIndexs.ElementAt(orderNum++).Key;
                //int type = this.TypeIndexs.ElementAt(random.Next(0, this.TypeIndexs.Count - 1)).Key;
            } while (this.TypeIndexs.Count > 1 && this.typeImageList[type].Total == 0);

            this.TypeIndexs[type]++;
            int typeIndex = this.TypeIndexs[type];
            // 判断是否超过最大记录数据
            if (typeIndex >= this.typeImageList[type].Total)
            {
                this.TypeIndexs[type] = 0;
                typeIndex = 0;
            }
            
            // 判断是否在获取的记录之前
            if (typeIndex < this.typeImageList[type].StartIndex
                // 如果Image列表为空，则尝试重新获取
                || typeImageList[type].Images == null || typeImageList[type].Images.Count == 0)
            {
                ImageListTotal imgList = GetImageListTotal(type, typeIndex, this.Count);
                if(type != "down")
                {
                    CacheIage(imgList.data);
                }
                typeImageList[type].StartIndex = typeIndex;
                typeImageList[type].Images = imgList.data;

            }
            if (this.typeImageList[type].Total == 0)
            {
                throw new Exception("当前分类下没有壁纸，请选择其他类型。");
            }
            // 判断是否在获取的记录之后
            if (typeIndex > this.typeImageList[type].EndIndex)
            {
                ImageListTotal imgList = GetImageListTotal(type, typeIndex, this.Count);
                if (type != "down")
                {
                    CacheIage(imgList.data);
                }
                typeImageList[type].Images.AddRange(imgList.data);
            }
            int curIndex = typeIndex - this.typeImageList[type].StartIndex;
            if (curIndex >= typeImageList[type].Images.Count)
            {
                throw new Exception($"下一张图片信息获取异常：获取到的Images集合为{typeImageList[type].Images.Count},下一张图片索引为{curIndex}");
            }
            return typeImageList[type].Images[curIndex];
        }

        #region 静态方法及变量
        private static List<ImgInfo> recommendList = new List<ImgInfo>();
        /// <summary>
        /// 当前推荐的总数（重置为0后，会重新获取推荐的列表）
        /// </summary>
        public static int recommendTotal = 0;
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
            switch (type)
            {
                case "wall":// 壁纸历史记录
                    total = UserDataManage.GetLWallList(RecordType.ManualWallpaper, start, count, true, true);
                    break;
                case "love":// 收藏的
                    total = UserDataManage.GetLoveList(LoveType.Love, start, count);
                    break;
                case "down"://下载的
                    total = UserDataManage.GetDownList(start, count);
                    break;
                case "recommend"://根据收藏推荐的
                    if (recommendTotal == 0)
                    {
                        recommendList.Clear();
                        List<TagRecord> tops = UserDataManage.GetTopTagList(3);
                        if (tops.Count > 0)
                        {
                            // 计算倍率，防止收藏数量太多造成获取的数量太多
                            decimal rate = count * 2 / tops[0].RecordCount;
                            foreach (TagRecord top in tops)
                            {
                                int tempCount = (int)(top.RecordCount * rate);
                                ImageListTotal tempTotal = WebImage.GetImageListByKW(top.TagName, 0, tempCount);
                                recommendTotal += tempTotal.total;
                                recommendList.AddRange(tempTotal.data);
                            }
                            // 去重和排序
                            List<string> ids = recommendList.GroupBy(h => h.id).Select(h => h.Key).Distinct().ToList();
                            recommendList = ids.GroupJoin(
                                recommendList,
                                h => h, h => h.id,
                                (k, v) => v.FirstOrDefault())
                                .OrderByDescending(h => h.create_time).ToList();
                        }
                    }
                    if (start < recommendList.Count)
                    {
                        total = new ImageListTotal();
                        total.total = recommendList.Count;
                        total.data = new List<ImgInfo>();
                        total.data = recommendList.Skip(start).Take(count).ToList();
                    }
                    break;
                default:
                    total = WebImage.GetImageList(type, start, count);
                    /*// 暂时不考虑，没有想到比较好的解决方法
                    if (ConfigManage.Base.ExcludeDislike)
                    {

                    }*/
                    break;
            }
            return total;
        }
       
        #endregion
    }
}

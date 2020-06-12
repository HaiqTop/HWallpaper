﻿using System;
using HWallpaper.Model;
using HWallpaper.Common;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media.Imaging;
using System.IO;

namespace HWallpaper.Business
{
    public class WebImage
    {
        /// <summary>
        /// 获取图片类别列表
        /// </summary>
        /// <returns></returns>
        public static TypeTotal GetTypeList(bool local = false)
        {
            string jsonStr = string.Empty;
            TypeTotal model = null;
            try
            {
                if (local)
                {
                    model = Common.JsonHelper.DeserializeJsonToObject<TypeTotal>(ConfigManage.Base.TypeJson);
                }
                else
                {
                    jsonStr = WebHelper.HttpGet(Const.Url_Type);
                    model = Common.JsonHelper.DeserializeJsonToObject<TypeTotal>(jsonStr);
                    // 如果正确获取到信息，则将字符串存储到本地（防止下次因为服务器接口问题导致无法正确获取数据）
                    if (model != null && model.data != null && model.data.Count > 0)
                    {
                        ConfigManage.Base.TypeJson = jsonStr;
                    }
                    else
                    {
                        throw new Exception("获取的图片类型Json数据为空");
                    }
                }

            }
            catch (Exception ex)
            {
                // 假如获取信息失败
                model = Common.JsonHelper.DeserializeJsonToObject<TypeTotal>(ConfigManage.Base.TypeJson);
                Common.LogHelper.WriteLog(ex.Message, Common.EnumLogLevel.Error);
            }
            if (model != null && model.data != null)
            {
                model.data.Insert(0, new TypeList() { id = "recommend", name = "兴趣推荐" });
                model.data.Insert(0, new TypeList() { id = "love", name = "我的收藏" });
                model.data.Insert(0, new TypeList() { id = "down", name = "我的下载" });
            }
            return model;
        }


        public static ImageListTotal GetImageList(string[] types, int start = 0, int count = 30)
        {
            if (types != null && types.Length > 0)
            {
                ImageListTotal total = new ImageListTotal();
                total.data = new List<ImgInfo>();
                int price = count / types.Length;
                for (int i = 0; i < types.Length; i++)
                {
                    if (i + 1 == types.Length)
                    {
                        price += count - (price * types.Length);
                    }
                    ImageListTotal temp = GetImageList(types[i], (start / types.Length), price);
                    //total.consume += temp.consume;
                    total.total += temp.total;
                    total.data.AddRange(temp.data);
                }
                total.data = total.data.OrderBy(o => o.id).ToList();
                return total;
            }
            return null;
        }


        /// <summary>
        /// 获取图片列表
        /// </summary>
        /// <param name="type">默认0（最新数据）</param>
        /// <param name="start">默认0</param>
        /// <param name="count">默认30</param>
        /// <returns></returns>
        public static ImageListTotal GetImageList(string type = "0", int start = 0, int count = 30)
        {
            string jsonStr = string.Empty;
            // 获取最新的
            if (type == "0")
            {
                jsonStr = WebHelper.HttpGet(string.Format(Const.Url_ListTopNew, start, count));
            }
            else//根据分类id获取
            {
                jsonStr = WebHelper.HttpGet(string.Format(Const.Url_ListByType, type, start, count));
            }
            ImageListTotal listTotal = JsonHelper.DeserializeJsonToObject<ImageListTotal>(jsonStr);
            return listTotal;
        }
        /// <summary>
        /// 根据关键字搜索
        /// </summary>
        /// <param name="keyword"></param>
        /// <param name="start"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public static ImageListTotal GetImageListByKW(string keyword, int start = 0, int count = 30)
        {
            string jsonStr = string.Empty;
            // 获取最新的
            if(string.IsNullOrEmpty(keyword))
            {
                keyword = "可爱";
            }
            jsonStr = WebHelper.HttpGet(string.Format(Const.Url_ListBySearch, keyword, start, count));
            ImageListTotal listTotal = JsonHelper.DeserializeJsonToObject<ImageListTotal>(jsonStr);
            return listTotal;
        }
        /// <summary>
        /// 保存图片到本地
        /// </summary>
        /// <param name="bitmap"></param>
        /// <param name="imgFullName"></param>
        public static void SaveImage(BitmapSource bitmap,string imgFullName)
        {
            PngBitmapEncoder PBE = new PngBitmapEncoder();
            PBE.Frames.Add(BitmapFrame.Create(bitmap));
            if (!File.Exists(imgFullName))
            {
                using (System.IO.Stream stream = System.IO.File.Create(imgFullName))
                {
                    PBE.Save(stream);
                }
            }
        }
    }
}

using System;
using HWallpaper.Model;
using HWallpaper.Common;
using System.Collections.Generic;
using System.Linq;

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
                    ImageListTotal temp = GetImageList(Convert.ToInt32(types[i]), (start / types.Length), price);
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
        public static ImageListTotal GetImageList(int type = 0, int start = 0, int count = 30)
        {
            string jsonStr = string.Empty;
            // 获取最新的
            if (type == 0)
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
    }
}

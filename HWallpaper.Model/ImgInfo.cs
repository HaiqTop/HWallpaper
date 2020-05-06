using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HWallpaper.Model
{
    public class ImgInfo
    {
        public string id { get; set; }
        public int class_id { get; set; }
        public string create_time { get; set; }
        /// <summary>
        /// 2020-02-11 15:38:04
        /// </summary>
        public string update_time { get; set; }
        /// <summary>
        /// 1920x1080
        /// </summary>
        public string resolution { get; set; }
        /// <summary>
        /// 标签（_全部_ _category_性感女神_  _category_美女模特_）
        /// </summary>
        public string tag { get; set; }
        /// <summary>
        /// (性感女神)
        /// </summary>
        public string utag { get; set; }
        public string url { get; set; }
        public string url_mid { get; set; }
        public string url_mobile { get; set; }
        public string url_thumb { get; set; }

        public int Index { get; set; }

        private string urlLeft;
        private string urlRight;
        public string UrlLeft
        {
            get
            {
                if (string.IsNullOrWhiteSpace(urlRight) && !string.IsNullOrWhiteSpace(url))
                {
                    int leftIndex = this.url.IndexOf("qhimg.com/");
                    int rightIndex = this.url.LastIndexOf("/d/");
                    if (rightIndex == -1)
                    {
                        rightIndex = this.url.LastIndexOf("/t");
                    }
                    if (rightIndex == -1)
                    {
                        urlRight = "";
                    }
                    else
                    {
                        this.urlLeft = this.url.Substring(0, leftIndex + 10);
                        this.urlRight = this.url.Substring(rightIndex);
                    }
                }
                return urlLeft;
            }
            set { urlLeft = value; }
        }
        public string UrlRight
        {
            get
            {
                if (string.IsNullOrWhiteSpace(urlRight) && !string.IsNullOrWhiteSpace(url))
                {
                    int leftIndex = this.url.IndexOf("qhimg.com/");
                    int rightIndex = this.url.LastIndexOf("/d/");
                    if (rightIndex == -1)
                    { 
                        rightIndex = this.url.LastIndexOf("/t");
                    }
                    if (rightIndex == -1)
                    {
                        urlRight = "";
                    }
                    else 
                    {
                        this.urlLeft = this.url.Substring(0, leftIndex + 10);
                        this.urlRight = this.url.Substring(rightIndex);
                    }
                }
                return urlRight;
            }
            set { urlRight = value; }
        }
        private string curUrl;
        /// <summary>
        /// 适应当前分辨率的图片url地址
        /// </summary>
        public string CurUrl
        {
            get
            {
                if (string.IsNullOrWhiteSpace(curUrl))//如果该属性为空（未处理url），则使用其他url地址
                {
                    //this.GetUrlByScreen();
                    curUrl = this.url;
                }
                return curUrl;
            }
            set { curUrl = value; }
        }
        ///// <summary>
        ///// 根据当前屏幕分辨率获取图片Url地址
        ///// </summary>
        ///// <param name="width"></param>
        ///// <param name="height"></param>
        ///// <returns></returns>
        //public string GetUrlByScreen()
        //{
        //    int width = System.Windows.Forms.SystemInformation.PrimaryMonitorSize.Width;
        //    int height = System.Windows.Forms.SystemInformation.PrimaryMonitorSize.Height;
        //    if ((width + "x" + height) == this.resolution)
        //    {
        //        this.curUrl = this.url;
        //    }
        //    else
        //    {
        //        this.curUrl = this.GetUrlBySize(width, height);
        //    }
        //    return this.curUrl;
        //}
        public string GetUrlBySize(int width, int height)
        {
            if (string.IsNullOrEmpty(this.UrlRight))
            {
                //LogHelper.WriteLog("未知Url：" + this.url,EnumLogLevel.Error);
                return this.url;
            }
            else
            { 
                return this.UrlLeft + "bdm/" + width.ToString() + "_" + height + "_85" + this.UrlRight;
            }
        }
        public bool Valid(int width, int height)
        {
            string[] temp = this.resolution.Split('x');

            if (width <= Convert.ToInt32(temp[0]) && height <= Convert.ToInt32(temp[1]))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        /// <summary>
        /// 获取图片文件名
        /// </summary>
        /// <returns></returns>
        public string GetFileName()
        {
            string outName = this.url.Split('.')[this.url.Split('.').Length - 1];
            return this.class_id + "_" + this.id + "." + outName;
        }

        public List<string> GetTagList()
        {
            List<string> tagList = new List<string>();
            string[] tags = this.tag.Split(' ');
            foreach (var item in tags)
            {
                string[] temp = item.Split('_');
                if (temp.Length == 4)
                {
                    tagList.Add(temp[2]);
                }
            }
            return tagList;
        }
    }
}

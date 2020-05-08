using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HWallpaper.Common
{
    public class Const
    {
        /// <summary>
        /// exe文件所在的目录+"\"
        /// </summary>
        public static string CurrentDirectory
        {
            get 
            {
                return AppDomain.CurrentDomain.SetupInformation.ApplicationBase;
            } 
        }
        /// <summary>
        /// 获取壁纸分类的url
        /// </summary>
        public const string Url_Type = "http://cdn.apc.360.cn/index.php?c=WallPaper&a=getAllCategoriesV2&from=360chrome";
        /// <summary>
        /// 根据壁纸分类ID获取分类下壁纸图片
        /// 参数：分类ID、从第几幅图开始(用于分页)、 每次加载的数量
        /// </summary>
        public const string Url_ListByType = "http://wallpaper.apc.360.cn/index.php?c=WallPaper&a=getAppsByCategory&cid={0}&start={1}&count={2}&from=360chrome";
        /// <summary>
        /// 获取最近更新的壁纸
        /// 参数：偏移量（从0开始）、加载张数
        /// </summary>
        public const string Url_ListTopNew = "http://wallpaper.apc.360.cn/index.php?c=WallPaper&a=getAppsByOrder&order=create_time&start={0}&count={1}&from=360chrome";
        public static string dataPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData) + "\\DeskTopTools\\";
    }
}

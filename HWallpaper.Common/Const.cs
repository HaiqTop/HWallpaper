using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HWallpaper.Common
{
    public class Const
    {
        /// <summary>
        /// 图片文件夹名称
        /// </summary>
        public static string HWallpaper = nameof(HWallpaper);

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
        public const string Url_ListByType = "http://wallpaper.apc.360.cn/index.php?c=WallPaper&a=getAppsByCategory&cid={0}&start={1}&count={2}";//&from=360chrome
        public const string Url_ListByType1 = "http://wallpaper.apc.360.cn/index.php?c=WallPaperAndroid&a=getAppsByCategory&cid={0}&start={1}&count={2}";
        /// <summary>
        /// 获取最近更新的壁纸
        /// 参数：偏移量（从0开始）、加载张数
        /// </summary>
        public const string Url_ListTopNew = "http://wallpaper.apc.360.cn/index.php?c=WallPaper&a=getAppsByOrder&order=create_time&start={0}&count={1}&from=360chrome";
        public const string Url_ListBySearch = "http://wallpaper.apc.360.cn/index.php?c=WallPaper&a=search&kw={0}&start={1}&count={2}";
        public const string DefaultTypeJson = "{\"errno\":\"0\",\"errmsg\":\"\u6b63\u5e38\",\"consume\":\"3\",\"total\":\"18\",\"data\":[{\"id\":\"36\",\"name\":\"4K\u4e13\u533a\"},{\"id\":\"6\",\"name\":\"\u7f8e\u5973\u6a21\u7279\"},{\"id\":\"30\",\"name\":\"\u7231\u60c5\u7f8e\u56fe\"},{\"id\":\"9\",\"name\":\"\u98ce\u666f\u5927\u7247\"},{\"id\":\"15\",\"name\":\"\u5c0f\u6e05\u65b0\"},{\"id\":\"26\",\"name\":\"\u52a8\u6f2b\u5361\u901a\"},{\"id\":\"11\",\"name\":\"\u660e\u661f\u98ce\u5c1a\"},{\"id\":\"14\",\"name\":\"\u840c\u5ba0\u52a8\u7269\"},{\"id\":\"5\",\"name\":\"\u6e38\u620f\u58c1\u7eb8\"},{\"id\":\"12\",\"name\":\"\u6c7d\u8f66\u5929\u4e0b\"},{\"id\":\"10\",\"name\":\"\u70ab\u9177\u65f6\u5c1a\"},{\"id\":\"29\",\"name\":\"\u6708\u5386\u58c1\u7eb8\"},{\"id\":\"7\",\"name\":\"\u5f71\u89c6\u5267\u7167\"},{\"id\":\"13\",\"name\":\"\u8282\u65e5\u7f8e\u56fe\"},{\"id\":\"22\",\"name\":\"\u519b\u4e8b\u5929\u5730\"},{\"id\":\"16\",\"name\":\"\u52b2\u7206\u4f53\u80b2\"},{\"id\":\"18\",\"name\":\"BABY\u79c0\"},{\"id\":\"35\",\"name\":\"\u6587\u5b57\u63a7\"}]}";
        public static string dataPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData) + $"\\{HWallpaper}\\";
        public static string dbFile = dataPath + "Database.db";
        public static string dbEmptyFile = AppDomain.CurrentDomain.BaseDirectory + "Database.db";
        //说明：https 有感叹号，有解决办法吗？将获取的 360 壁纸图片链接地址头由 http://p5.qhimg.com/bdm/ 改成 https://p0.ssl.qhmsg.com/dmfd/ 即可
    }
}

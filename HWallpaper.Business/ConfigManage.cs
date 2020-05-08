using HWallpaper.Common;
using HWallpaper.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace HWallpaper.Business
{
    public class ConfigManage
    {
        private static string filePath = Const.dataPath + "Config.dat";
        private static Config Data;
        public static BaseConfig Base;
        public static ScreenConfig Screen;
        public static WallpaperConfig Wallpaper;
        static ConfigManage()
        {
            LoadData();
            if (Data == null)
            {
                Data = new Config();
                Data.Base = new BaseConfig();
                Data.Screen = new ScreenConfig();
                Data.Wallpaper = new WallpaperConfig();

                #region 初始化路径
                string bPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyPictures), "ZoroWallpaper");
                Data.Base.CachePath = Path.Combine(bPath, "Cache");
                Data.Base.DownPath = Path.Combine(bPath, "Download");

                if (!Directory.Exists(Data.Base.CachePath))
                    Directory.CreateDirectory(Data.Base.CachePath);
                if (!Directory.Exists(Data.Base.DownPath))
                    Directory.CreateDirectory(Data.Base.DownPath);
                #endregion 初始化路径

                #region 初始化其他信息
                Data.Base.TypeJson = "{\"errno\":\"0\",\"errmsg\":\"\u6b63\u5e38\",\"consume\":\"3\",\"total\":\"18\",\"data\":[{\"id\":\"36\",\"name\":\"4K\u4e13\u533a\"},{\"id\":\"6\",\"name\":\"\u7f8e\u5973\u6a21\u7279\"},{\"id\":\"30\",\"name\":\"\u7231\u60c5\u7f8e\u56fe\"},{\"id\":\"9\",\"name\":\"\u98ce\u666f\u5927\u7247\"},{\"id\":\"15\",\"name\":\"\u5c0f\u6e05\u65b0\"},{\"id\":\"26\",\"name\":\"\u52a8\u6f2b\u5361\u901a\"},{\"id\":\"11\",\"name\":\"\u660e\u661f\u98ce\u5c1a\"},{\"id\":\"14\",\"name\":\"\u840c\u5ba0\u52a8\u7269\"},{\"id\":\"5\",\"name\":\"\u6e38\u620f\u58c1\u7eb8\"},{\"id\":\"12\",\"name\":\"\u6c7d\u8f66\u5929\u4e0b\"},{\"id\":\"10\",\"name\":\"\u70ab\u9177\u65f6\u5c1a\"},{\"id\":\"29\",\"name\":\"\u6708\u5386\u58c1\u7eb8\"},{\"id\":\"7\",\"name\":\"\u5f71\u89c6\u5267\u7167\"},{\"id\":\"13\",\"name\":\"\u8282\u65e5\u7f8e\u56fe\"},{\"id\":\"22\",\"name\":\"\u519b\u4e8b\u5929\u5730\"},{\"id\":\"16\",\"name\":\"\u52b2\u7206\u4f53\u80b2\"},{\"id\":\"18\",\"name\":\"BABY\u79c0\"},{\"id\":\"35\",\"name\":\"\u6587\u5b57\u63a7\"}]}";
                Data.Screen.OpenInterval = 10;
                Data.Screen.ReplaceInterval = 20;
                Data.Screen.SelectedTypes = "0";
                Data.Wallpaper.TimeInterval = 1;
                Data.Wallpaper.TimeType = TimeType.Day;
                Data.Wallpaper.SelectedTypes = "0";
                #endregion 
            }
            Base = Data.Base;
            Screen = Data.Screen;
            Wallpaper = Data.Wallpaper;
        }
        private static void LoadData()
        {
            try
            {
                if (File.Exists(filePath))
                {
                    using (FileStream fileWrite = new FileStream(filePath, FileMode.Open))
                    {
                        byte[] bytes = new byte[fileWrite.Length];
                        fileWrite.Read(bytes, 0, bytes.Length);
                        ConfigManage.Data = SerializableHelper.DeserializeObject<Config>(bytes);
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(ex.Message, EnumLogLevel.Error);
            }
        }
        /// <summary>
        /// 保存配置
        /// </summary>
        public static void Save()
        {
            try
            {
                byte[] bytes = SerializableHelper.SerializeObject(ConfigManage.Data);
                //创建文件写入对象
                using (FileStream fileWrite = new FileStream(filePath, FileMode.Create))
                {
                    fileWrite.Write(bytes, 0, bytes.Length);
                }
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(ex.Message,EnumLogLevel.Error);
                //throw;
            }
        }
    }
}

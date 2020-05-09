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
                Data.Base.TypeJson = Const.DefaultTypeJson;
                Data.Screen.OpenInterval = 10;
                Data.Screen.ReplaceInterval = 20;
                Data.Screen.SelectedTypes = "0";
                Data.Wallpaper.TimeInterval = 1;
                Data.Wallpaper.TimeType = TimeType.Day;
                Data.Wallpaper.SelectedTypes = "0";
                #endregion 
            }
            if (string.IsNullOrEmpty(Data.Base.TypeJson))
            {
                Data.Base.TypeJson = Const.DefaultTypeJson;
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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HWallpaper.Model
{
    [Serializable]
    public class Config
    {
        public BaseConfig Base { get; set; }
        public ScreenConfig Screen { get; set; }
        public WallpaperConfig Wallpaper { get; set; }

    }

    [Serializable]
    public class BaseConfig
    {
        private string typeJson;
        public string TypeJson { get { return typeJson; } set { typeJson = value; } }
        /// <summary>
        /// 开机启动
        /// </summary>
        public bool AutoOn { get; set; }
        /// <summary>
        /// 是否缓存壁纸
        /// </summary>
        public bool Cache { get; set; }
        /// <summary>
        /// 是否自动清理过期缓存
        /// </summary>
        public bool AutoClearCache { get; set; }
        public string CachePath { get; set; }
        public string DownPath { get; set; }


        #region 方法 
        public string toXml()
        {
            System.Xml.Linq.XElement element = new System.Xml.Linq.XElement("Config");
            element.SetElementValue("TypeJson", TypeJson);
            element.SetElementValue("AutoOn", AutoOn);

            //RPC.Serialize.ParamsSerializeUtil.Serialize(this);
            return element.ToString();
        }

        public void LoadByXml(string data)
        {
            System.Xml.Linq.XElement element = System.Xml.Linq.XElement.Parse(data);
            TypeJson = element.Element("TypeJson").Value;
            AutoOn = bool.Parse(element.Element("AutoOn").Value);
        }

        //public string toJson()
        //{
        //    JObject json = new JObject();
        //    //json.Add( nameof( Password ), new JValue( Password ) );
        //    json.Add(nameof(UserName), new JValue(UserName));
        //    json.Add(nameof(Age), new JValue(Age));
        //    json.Add(nameof(Phone), new JValue(Phone));
        //    json.Add(nameof(Stature), new JValue(Stature));

        //    return json.ToString();
        //}

        //public void LoadByJson(string data)
        //{
        //    JObject json = JObject.Parse(data);
        //    UserName = json[nameof(UserName)].Value<string>();
        //    //Password = json[nameof( Password )].Value<string>( );
        //    Age = json[nameof(Age)].Value<int>();
        //    Phone = json[nameof(Phone)].Value<string>();
        //    Stature = json[nameof(Stature)].Value<double>();
        //}
        #endregion
    }

    [Serializable]
    public class WallpaperConfig
    {
        /// <summary>
        /// 自动替换壁纸
        /// </summary>
        public bool AutoReplace { get; set; }

        private string selectedTypes = "0";
        /// <summary>
        /// 自动切换壁纸类型
        /// </summary>
        public string SelectedTypes { get { return selectedTypes; } set { selectedTypes = value; } }

        private int timeInterval = 1;
        /// <summary>
        /// 壁纸替换时间间隔
        /// </summary>
        public int TimeInterval { get { return timeInterval; } set { timeInterval = value; } }
        private TimeType timeType = TimeType.Day;
        /// <summary>
        /// 壁纸替换时间单位
        /// </summary>
        public TimeType TimeType { get { return timeType; } set { timeType = value; } }
        /// <summary>
        /// 最后一次更新事件
        /// </summary>
        public DateTime ReplaceLastTime { get; set; }
    }
    [Serializable]
    public class ScreenConfig
    {
        private bool open = false;
        /// <summary>
        /// 屏保是否打开
        /// </summary>
        public bool Open { get; set; }
        /// <summary>
        /// 开启屏保时间间隔（单位：分钟）
        /// </summary>
        public int OpenInterval { get; set; }
        /// <summary>
        /// 屏保壁纸切换时间间隔（单位：秒）
        /// </summary>
        public int ReplaceInterval { get; set; }

        private string selectedTypes = "0";
        /// <summary>
        /// 屏保壁纸类型
        /// </summary>
        public string SelectedTypes { get { return selectedTypes; } set { selectedTypes = value; } }
    }
    
    public enum TimeType
    {
        Day = 0,
        Hour = 1,
        Minute = 2
    }
}

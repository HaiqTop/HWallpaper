using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

namespace HWallpaper.Common
{
    public class WebHelper
    {
        
        /// <summary>
        /// 根据Url地址获取图片
        /// </summary>
        /// <param name="url">路径</param>
        public static System.Drawing.Image GetImage(string url)
        {
            WebRequest imgRequest = WebRequest.Create(url);
            HttpWebResponse res;
            try
            {
                imgRequest.Timeout = 5000;
                res = (HttpWebResponse)imgRequest.GetResponse();
            }
            catch (WebException ex)
            {
                LogHelper.WriteLog(ex.Message, EnumLogLevel.Error);
                res = (HttpWebResponse)ex.Response;
            }
            catch (Exception ex)
            { 
                LogHelper.WriteLog(ex.Message, EnumLogLevel.Error);
                return null;
            }
            if (res != null && res.StatusCode.ToString() == "OK")
            {
                System.Drawing.Image downImage = System.Drawing.Image.FromStream(imgRequest.GetResponse().GetResponseStream());
                return downImage;
            }
            return null;
        }
        /// <summary>
        /// 根据Url地址保存
        /// </summary>
        /// <param name="url">路径</param>
        public static bool DownImage(string url,string fullName)
        {
            try
            {
                if (!System.IO.File.Exists(fullName))
                {
                    if (string.IsNullOrEmpty(url))
                    {
                        throw new System.Exception("Url参数不可为空");
                    }
                    System.Drawing.Image img = GetImage(url);
                    if (img == null)
                    {
                        LogHelper.WriteLog("图片下载失败",EnumLogLevel.Warn);
                        return false;
                    } 
                    if (!File.Exists(fullName))// 需要多次判断的原因：防止在下载的过程中，其他地方已经下载好了
                    {
                        img.Save(fullName);
                    }
                    img.Dispose();
                    return true;
                }
            }
            catch (System.Exception ex)
            {
                LogHelper.WriteLog(ex.Message,EnumLogLevel.Error);
            }
            return false;
        }
        public static string HttpGet(string url, string postDataStr = "")
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url + (postDataStr == "" ? "" : "?") + postDataStr);
            request.Method = "GET";
            request.ContentType = "text/html;charset=UTF-8";
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Stream myResponseStream = response.GetResponseStream();
            StreamReader myStreamReader = new StreamReader(myResponseStream, Encoding.GetEncoding("utf-8"));
            string retString = myStreamReader.ReadToEnd();
            myStreamReader.Close();
            myResponseStream.Close();
            return retString;
        }
    }
}

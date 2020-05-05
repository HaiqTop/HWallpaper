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
                res = (HttpWebResponse)imgRequest.GetResponse();
            }
            catch (WebException ex)
            {
                res = (HttpWebResponse)ex.Response;
            }

            if (res.StatusCode.ToString() == "OK")
            {
                System.Drawing.Image downImage = System.Drawing.Image.FromStream(imgRequest.GetResponse().GetResponseStream());
                return downImage;
            }
            return null;
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

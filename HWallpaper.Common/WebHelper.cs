using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;

namespace HWallpaper.Common
{
    public class WebHelper
    {
        public static Stream GetWebStream(string url)
        {
            WebRequest imgRequest = WebRequest.Create(url);
            HttpWebResponse res = null;
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
            }
            if (res != null && res.StatusCode.ToString() == "OK")
            {
                return res.GetResponseStream();
            }
            return null;
        }

        /// <summary>
        /// 根据Url地址获取图片
        /// </summary>
        /// <param name="url">路径</param>
        public static System.Drawing.Image GetImage(string url)
        {
            using (var stream = GetWebStream(url))
            {
                if (stream != null)
                {
                    System.Drawing.Image downImage = System.Drawing.Image.FromStream(stream);
                    return downImage;
                }
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
                    using (var stream = GetWebStream(url))
                    {
                        if (stream == null)
                        {
                            LogHelper.WriteLog("图片下载失败：" + url, EnumLogLevel.Warn);
                            return false;
                        }
                        using (FileStream fs = File.Create(fullName))
                        {
                            //建立字节组，并设置它的大小是多少字节
                            byte[] bytes = new byte[102400];
                            int n = 1;
                            while (n > 0)
                            {
                                //一次从流中读多少字节，并把值赋给Ｎ，当读完后，Ｎ为０,并退出循环
                                n = stream.Read(bytes, 0, 10240);
                                fs.Write(bytes, 0, n); //将指定字节的流信息写入文件流中
                            }
                            return true;
                        }
                    }
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

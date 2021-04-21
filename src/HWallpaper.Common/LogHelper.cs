using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace HWallpaper.Common
{
    public static class LogHelper
    {
        #region https://www.cnblogs.com/jxp0202/p/12129560.html
        private static readonly Queue<Dictionary<string, string>> Que = new Queue<Dictionary<string, string>>();
        private static readonly Object Object = new Object();
        private static EnumLogLevel LogType { get; set; }
        private static string logPath = Const.dataPath + "Logs\\";
        static LogHelper()
        {
            ThreadPool.QueueUserWorkItem(s =>
            {
                while (true)
                {
                    if (Que.Count <= 0)
                    {
                        Thread.Sleep(5000);
                        continue;
                    }
                    lock (Object)
                    {
                        if (Que.Count <= 0)
                        {
                            //Thread.Sleep(5000);
                            continue;
                        }
                        try
                        {
                            if (!Directory.Exists(logPath)) { Directory.CreateDirectory(logPath); }
                            var fileName = Path.Combine(logPath, DateTime.Now.ToString("yyyy-MM-dd") + ".log");
                            using (var fs = new FileStream(fileName, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite))
                            {
                                using (var sw = new StreamWriter(fs))
                                {
                                    sw.BaseStream.Seek(0, SeekOrigin.End);
                                    var col = Que.Dequeue();
                                    foreach (KeyValuePair<string, string> m in col)
                                    {
                                        sw.WriteLine(m.Key + "：{0}", m.Value);
                                    }
                                    sw.Flush(); sw.Dispose(); sw.Close();
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            throw ex;
                        }
                        
                    }
                }
            });
        }
        public static string WriteLog(this string str, EnumLogLevel type = EnumLogLevel.Info)
        {
            var logLevel = ConfigurationManager.AppSettings["LogLevel"];
            var level = string.IsNullOrWhiteSpace(logLevel) ? EnumLogLevel.Info.ToString() : logLevel;

            if ((int)type < (int)Enum.Parse(typeof(EnumLogLevel), level))
            {
                return string.Empty;
            }
            lock (Object)
            {
                LogType = type;
                var st = new StackTrace(new StackFrame(1, true));
                StackFrame sf = st.GetFrame(0);
                var dic = new Dictionary<string, string>
                {
                    {"发生时间", DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss:ffff dddd")},
                    {"日志级别", LogType.ToString()},
                    {"日志消息", str},
                    {"当前方法", sf.GetMethod().Name},
                    {"发生位置", st.ToString()}
                };
                Que.Enqueue(dic);
            }
            return str;
        }
        #endregion

        #region https://blog.csdn.net/weixin_42800530/article/details/83546201
        public static void Write(string Msg)
        {
            string strPath;
            DateTime dt = DateTime.Now;
            try
            {
                strPath = AppDomain.CurrentDomain.BaseDirectory + "\\Log";
                if (Directory.Exists(strPath) == false)
                {
                    Directory.CreateDirectory(strPath);
                }

                DeleteLog(strPath);

                strPath = strPath + "\\" + dt.ToString("yyyyMMdd") + ".txt";
                StreamWriter FileWriter = new StreamWriter(strPath, true);
                FileWriter.WriteLine(dt.ToString("HH:mm:ss") + Msg);
                FileWriter.Close();
            }
            catch (Exception ex)
            {
                string str = ex.Message.ToString();
            }
        }
        public static void DeleteLog(string sLogPath)
        {
            //string strFolderPath = @"E:\Zhao\我的代码\f福建中医药大学附属人民医院\定制化服务\bin\Debug\logs";

            DirectoryInfo dyInfo = new DirectoryInfo(sLogPath);
            //获取文件夹下所有的文件
            foreach (FileInfo feInfo in dyInfo.GetFiles())
            {
                //判断文件日期是否小于指定日期，是则删除
                if (feInfo.CreationTime < DateTime.Now.AddDays(-15))
                    feInfo.Delete();
            }
        }
        #endregion
    }

    /// <summary>
    /// All: 表示最低的日志等级。
    /// Debug: 这个等级表示用于调试程序的正常的事件信息；
    /// Info: 默认的等级
    /// Warn: 表示可能对系统有损害的情况；
    /// Error: 表示较严重的错误等级，但是程序可以继续运行的信息；
    /// Fatal: 表示非常严重的错误等级，记录极有可能导致应用程序终止运行的致命错误信息；
    /// Off: 表示最高的等级，如果一个logger的等级标记为Off， 将不会记录任何信息；
    /// </summary>
    public enum EnumLogLevel
    {
        All,
        Debug,
        Info,
        Warn,
        Error,
        Fatal,
        Off
    }
}

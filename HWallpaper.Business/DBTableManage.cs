using HWallpaper.Common;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;

namespace HWallpaper.Business
{
    // 暂时用不到了
    public class DBTableManage
    {
        private static string createHistory = "";
        private static string existSql = "SELECT count(*) FROM sqlite_master WHERE type='table' AND name = @TableName";
        /// <summary>
        /// 检查表History是否存在
        /// </summary>
        public void NotExistCreateHistory(bool autoCreate)
        {
            SQLiteParameter[] parameters = { new SQLiteParameter("@TableName", "History") };
            object result = SQLiteHelper.ExecuteScalar(existSql, parameters);
            if (result != DBNull.Value && Convert.ToInt32(result) == 0)
            { 
                // 不存在则创建表
            }
        }
    }
}

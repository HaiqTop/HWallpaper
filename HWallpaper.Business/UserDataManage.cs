using HWallpaper.Common;
using HWallpaper.Model;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;

namespace HWallpaper.Business
{
    public static class UserDataManage
    {
        private static string connectionString = $"Data Source={Const.dbFile};Initial Catalog=sqlite;Integrated Security=True;Max Pool Size=10";
        static SqlSugarClient db = new SqlSugarClient(
            new ConnectionConfig()
            {
                ConnectionString = connectionString,
                DbType = DbType.Sqlite,//设置数据库类型
                IsAutoCloseConnection = true,//自动释放数据务，如果存在事务，在事务结束后释放
                InitKeyType = InitKeyType.Attribute //从实体特性中读取主键自增列信息
            });
        #region 公有方法
        public static bool AddRecord(int recordType, ImgInfo info)
        {
            Picture picModel = new Picture();
            picModel.Id = info.Id;
            picModel.Type = info.class_id;
            picModel.Name = info.GetFileName();
            picModel.Url = info.url;
            picModel.Score = 0;
            picModel.Love = 0;
            var oldModel = db.Queryable<Picture>().First(o => o.Id == picModel.Id);
            
            var result = db.Saveable(picModel).ExecuteReturnEntity();
            //if (db.Queryable<Picture>().Where(o => o.Id == picModel.Id).Count() > 0)
            //{
            //    db.Updateable(picModel).ExecuteCommand(); //这种方式会以主键为条件
            //    picModel.Love = 1;
            //    engine.Update(picModel);
            //    var aa = engine.GetModel<Picture>(2007791);
            //}
            //db.Saveable(picModel).ExecuteReturnEntity();
            return true;
        }
        #endregion 

    }
}

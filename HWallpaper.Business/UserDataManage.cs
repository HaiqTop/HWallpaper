using HWallpaper.Common;
using HWallpaper.Model;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Windows.Controls;

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
        /// <summary>
        /// 添加图片使用记录
        /// </summary>
        /// <param name="recordType">图片使用类型（1：自动壁纸，2：手动壁纸，2：自动屏保）</param>
        /// <param name="info"></param>
        /// <returns></returns>
        public static bool AddRecord(RecordType recordType, ImgInfo info)
        {
            try
            {
                Picture picModel = db.Queryable<Picture>().Where(o => o.Id == info.Id).First();
                if (picModel == null)
                { 
                    picModel = new Picture();
                    picModel.Id = info.Id;
                    picModel.Type = info.class_id;
                    picModel.Name = info.GetFileName();
                    picModel.Url = info.url;
                    picModel.Score = 0;
                    picModel.Love = 0;
                    db.Insertable(picModel).ExecuteCommand();
                }

                Record record = new Record();
                record.PictureId = picModel.Id;
                record.Type = (int)recordType;
                record.Time = DateTime.Now;
                db.Insertable(record).ExecuteCommand();
                return true;
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(ex.Message,EnumLogLevel.Error);
                return false;
            }
        }

        public static bool SetLove(LoveType loveType,ImgInfo info)
        {
            try
            {
                Picture picModel = db.Queryable<Picture>().Where(o => o.Id == info.Id).First();
                if (picModel == null || picModel.Love == 0) //目前仅考虑第一次收藏
                {
                    SetTagLove(loveType, info);
                }
                if (picModel == null)
                {
                    picModel = new Picture();
                    picModel.Id = info.Id;
                    picModel.Type = info.class_id;
                    picModel.Name = info.GetFileName();
                    picModel.Url = info.url;
                    picModel.Score = 0;
                    picModel.Love = (int)loveType;
                    db.Insertable(picModel).ExecuteCommand();
                }
                else
                {
                    picModel.Love = (int)loveType;
                    db.Updateable(picModel).UpdateColumns(o => new { o.Love }).ExecuteCommand();
                }
                return true;
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(ex.Message, EnumLogLevel.Error);
                return false;
            }
        }

        public static bool SetScore(int score, ImgInfo info)
        {
            try
            {
                Picture picModel = db.Queryable<Picture>().Where(o => o.Id == info.Id).First();
                if (picModel == null || picModel.Score == 0) //目前仅考虑第一次收藏
                {
                    SetTagScore(score, info);
                }
                if (picModel == null)
                {
                    picModel = new Picture();
                    picModel.Id = info.Id;
                    picModel.Type = info.class_id;
                    picModel.Name = info.GetFileName();
                    picModel.Url = info.url;
                    picModel.Score = score;
                    picModel.Love = 0;
                    db.Insertable(picModel).ExecuteCommand();
                }
                else
                {
                    picModel.Score = score;
                    db.Updateable(picModel).UpdateColumns(o => new { o.Score }).ExecuteCommand();
                }
                return true;
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(ex.Message, EnumLogLevel.Error);
                return false;
            }
        }

        public static Picture GetPicture(int id)
        {
            try
            {
                return db.Queryable<Picture>().Where(o => o.Id == id).First();
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(ex.Message, EnumLogLevel.Error);
                return null;
            }
        }
        #endregion 
        /// <summary>
        /// 目前仅考虑第一次评分或者第一次收藏
        /// </summary>
        /// <param name="loveType"></param>
        /// <param name="info"></param>
        /// <returns></returns>
        private static bool SetTagLove(LoveType loveType, ImgInfo info)
        {
            try
            {
                if (loveType == LoveType.Love)
                {
                    List<string> tags = info.GetTagList();
                    List<TagRecord> tagRecords = db.Queryable<TagRecord>().Where(o => tags.Contains(o.TagName)).ToList();
                    foreach (var tag in tags)
                    {
                        if (tag == "全部")//不考虑“全部”标签
                        {
                            continue;
                        }
                        TagRecord tagRecord = tagRecords.Find(it=>it.TagName == tag);
                        if (tagRecord == null)
                        {
                            tagRecord = new TagRecord();
                            tagRecord.TagName = tag;
                            tagRecord.RecordCount = 1;
                            tagRecord.ScoreSum = 0;
                            db.Insertable(tagRecord).ExecuteCommand();
                        }
                        else
                        {
                            tagRecord.RecordCount++;
                            db.Updateable(tagRecord).UpdateColumns(o => new { o.RecordCount }).ExecuteCommand();
                        }
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(ex.Message, EnumLogLevel.Error);
                return false;
            }
        }
        /// <summary>
        /// 目前仅考虑第一次评分或者第一次收藏
        /// </summary>
        /// <param name="score"></param>
        /// <param name="info"></param>
        /// <returns></returns>
        private static bool SetTagScore(int score, ImgInfo info)
        {
            try
            {
                if (score > 0)
                {
                    List<string> tags = info.GetTagList();
                    List<TagRecord> tagRecords = db.Queryable<TagRecord>().Where(o => tags.Contains(o.TagName)).ToList();
                    foreach (var tag in tags)
                    {
                        if (tag == "全部")//不考虑“全部”标签
                        {
                            continue;
                        }
                        TagRecord tagRecord = tagRecords.Find(it => it.TagName == tag);
                        if (tagRecord == null)
                        {
                            tagRecord = new TagRecord();
                            tagRecord.TagName = tag;
                            tagRecord.RecordCount = 0;
                            tagRecord.ScoreSum = score;
                            db.Insertable(tagRecord).ExecuteCommand();
                        }
                        else
                        {
                            tagRecord.ScoreSum += score;
                            db.Updateable(tagRecord).UpdateColumns(o => new { o.RecordCount }).ExecuteCommand();
                        }
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(ex.Message, EnumLogLevel.Error);
                return false;
            }
        }
    }
}

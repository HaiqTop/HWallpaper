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
                    picModel = ToPicture(info);
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
                Love love = db.Queryable<Love>().Where(o => o.PictureId == info.Id).First();
                if (picModel == null || love == null) //目前仅考虑第一次收藏
                {
                    SetTagLove(loveType, info);
                }
                if (picModel == null)
                {
                    picModel = ToPicture(info);
                    db.Insertable(picModel).ExecuteCommand();
                }
                love = new Love() { PictureId = info.Id,Time = DateTime.Now,Type = (int)loveType };
                db.Saveable(love).ExecuteCommand();
                return true;
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(ex.Message, EnumLogLevel.Error);
                return false;
            }
        }
        public static bool SaveDown(Download down, ImgInfo info)
        {
            try
            {
                Picture picModel = db.Queryable<Picture>().Where(o => o.Id == info.Id).First();
                if (picModel == null)
                {
                    picModel = ToPicture(info);
                    db.Insertable(picModel).ExecuteCommand();
                }
                db.Saveable(down).ExecuteCommand();
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
        public static Love GetLove(int id)
        {
            try
            {
                return db.Queryable<Love>().Where(o => o.PictureId == id).First();
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(ex.Message, EnumLogLevel.Error);
                return null;
            }
        }
        public static Download GetDown(int id)
        {
            try
            {
                return db.Queryable<Download>().Where(o => o.PictureId == id).First();
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(ex.Message, EnumLogLevel.Error);
                return null;
            }
        }
        /// <summary>
        /// 分页获取收藏（喜欢）的壁纸
        /// </summary>
        /// <param name="loveType"></param>
        /// <param name="start"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public static ImageListTotal GetLoveList(LoveType loveType, int start, int count,bool orderDesc = false)
        {
            ImageListTotal total = new ImageListTotal();
            int totalCount = 0;
            var page = db.Queryable<Picture, Love>((p, l) => new object[] { JoinType.Inner, p.Id == l.PictureId })
                .Where((p, l) => l.Type == (int)loveType)
                .OrderByIF(!orderDesc, (p, l) => l.Time, OrderByType.Asc)
                .OrderByIF(orderDesc, (p, l) => l.Time, OrderByType.Desc)
                .Select((p, l) => new ImgInfo()
                {
                    class_id = p.Type,
                    id = p.Id.ToString(),
                    url = p.Url,
                    tag = p.Tag
                });
            total.data = page.ToPageList(start, count, ref totalCount);
            total.total = totalCount;
            return total;
        }
        /// <summary>
        /// 分页获取下载的壁纸
        /// </summary>
        /// <param name="loveType"></param>
        /// <param name="start"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public static ImageListTotal GetDownList(int start, int count)
        {
            ImageListTotal total = new ImageListTotal();
            int totalCount = 0;
            total.data = db.Queryable<Picture, Download>((p, d) => new object[] { JoinType.Inner, p.Id == d.PictureId })
                .OrderBy((p, d) => d.Time)
                .Select((p, d) => new ImgInfo()
                {
                    class_id = p.Type,
                    id = p.Id.ToString(),
                    url = p.Url,
                    tag = p.Tag
                }).ToPageList(start, count, ref totalCount);
            total.total = totalCount;
            return total;
        }
        #endregion 
        private static Picture ToPicture(ImgInfo info)
        {
            Picture picModel = new Picture();
            picModel.Id = info.Id;
            picModel.Type = info.class_id;
            picModel.Name = info.GetFileName();
            picModel.Url = info.url;
            picModel.Tag = info.tag;
            return picModel;
        }
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

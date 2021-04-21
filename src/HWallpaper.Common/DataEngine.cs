using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using System.Reflection;
using System.Text;

namespace HWallpaper.Common
{
    public class DataEngine
    {
        public bool Add<T>(T t)
        {
            List<SQLiteParameter> parameters = new List<SQLiteParameter>();
            List<string> fields = new List<string>();
            PropertyInfo[] properts = t.GetType().GetProperties();
            foreach (PropertyInfo p in properts)
            {
                fields.Add(p.Name);
                var parameter = new SQLiteParameter("@" + p.Name, this.GetDbType(p));
                parameter.Value = p.GetValue(t, null);
                parameters.Add(parameter);
            }
            string sql = $"insert into {typeof(T).Name} ({string.Join(",", fields)}) values ({string.Join(",", fields.Select(o => "@" + o))})";
            int result = SQLiteHelper.ExecuteNonQuery(sql, parameters.ToArray());
            return result > 0;
        }

        public bool Update<T>(T t)
        {
            List<SQLiteParameter> parameters = new List<SQLiteParameter>();
            List<string> fields = new List<string>();
            PropertyInfo[] properts = t.GetType().GetProperties();
            string keyField = string.Empty;
            foreach (PropertyInfo p in properts)
            {
                if (p.GetCustomAttributes(typeof(KeyAttribute), false).Length > 0)
                {
                    keyField = p.Name;//获取主键名称
                }
                else
                {
                    fields.Add(p.Name + "=@" + p.Name);
                }
                var parameter = new SQLiteParameter("@" + p.Name, this.GetDbType(p));
                parameter.Value = p.GetValue(t, null);
                parameters.Add(parameter);
            }
            string sql = $"update {typeof(T).Name} set {string.Join(",", fields)} where {keyField}=@{keyField}";
            int result = SQLiteHelper.ExecuteNonQuery(sql, parameters.ToArray());
            return result > 0;
        }

        public T GetModel<T>(int key) where T : new()
        {
            List<string> fields = new List<string>();
            PropertyInfo[] properts = typeof(T).GetProperties();
            string keyField = string.Empty;
            foreach (PropertyInfo p in properts)
            {
                if (p.GetCustomAttributes(typeof(KeyAttribute), false).Length > 0)
                {
                    keyField = p.Name;//获取主键名称
                }
                fields.Add(p.Name);
            }
            string sql = $"select {string.Join(",", fields)} from {typeof(T).Name} where {keyField}=@{keyField}";
            SQLiteParameter[] parameters = {
                    new SQLiteParameter("@" + keyField, DbType.Int32) };
            parameters[0].Value = key;
            var dt = SQLiteHelper.ExecuteDataTable(sql, parameters);
            T t = new T();
            foreach (PropertyInfo p in properts)
            {
                if (p.PropertyType == typeof(System.Int32))
                {
                    p.SetValue(t, Convert.ToInt32(dt.Rows[0][p.Name]), null);
                }
                else
                { 
                    p.SetValue(t,dt.Rows[0][p.Name],null);
                }
            }
            return t;
        }

        private DbType GetDbType(PropertyInfo p)
        {
            string type = p.PropertyType.FullName;
            switch (type)
            {
                case "System.Boolean": return DbType.Boolean;
                case "System.Int16": return DbType.Int16;
                case "System.Int32": return DbType.Int32;
                case "System.Int64": return DbType.Int64;
                case "System.Double": return DbType.Double;
                case "System.Decimal": return DbType.Decimal;
                case "System.DateTime": return DbType.DateTime;/// ???????
                case "System.String": return DbType.String;
                default:return DbType.String;
            }
        }
    }
}

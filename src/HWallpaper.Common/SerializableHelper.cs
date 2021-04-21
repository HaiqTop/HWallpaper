using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace HWallpaper.Common
{
    public class SerializableHelper
    {
        public static byte[] SerializeObject(object obj)
        {
            if (obj == null)
                return null;
            //内存实例
            MemoryStream ms = new MemoryStream();
            //创建序列化的实例
            BinaryFormatter formatter = new BinaryFormatter();
            formatter.Serialize(ms, obj);//序列化对象，写入ms流中  
            byte[] bytes = ms.GetBuffer();
            return bytes;
        }
        public static T DeserializeObject<T>(byte[] bytes)
        {
            T obj = default;
            if (bytes == null)
                return obj;
            //利用传来的byte[]创建一个内存流
            MemoryStream ms = new MemoryStream(bytes);
            ms.Position = 0;
            BinaryFormatter formatter = new BinaryFormatter();
            obj = (T)formatter.Deserialize(ms);//把内存流反序列成对象  
            ms.Close();
            return obj;
        }
        /* https://www.cnblogs.com/yswenli/p/9217495.html
         * https://github.com/yswenli/SAEA
        public static byte[] Serialize(object param)
        {
            List<byte> datas = new List<byte>();
            var len = 0;
            byte[] data = null;
            if (param == null)
            {
                len = 0;
            }
            else
            {
                if (param is string)
                {
                    data = Encoding.UTF8.GetBytes((string)param);
                }
                else if (param is byte)
                {
                    data = new byte[] { (byte)param };
                }
                else if (param is bool)
                {
                    data = BitConverter.GetBytes((bool)param);
                }
                else if (param is short)
                {
                    data = BitConverter.GetBytes((short)param);
                }
                else if (param is int)
                {
                    data = BitConverter.GetBytes((int)param);
                }
                else if (param is long)
                {
                    data = BitConverter.GetBytes((long)param);
                }
                else if (param is float)
                {
                    data = BitConverter.GetBytes((float)param);
                }
                else if (param is double)
                {
                    data = BitConverter.GetBytes((double)param);
                }
                else if (param is DateTime)
                {
                    var str = "wl" + ((DateTime)param).Ticks;
                    data = Encoding.UTF8.GetBytes(str);
                }
                else if (param is Enum)
                {
                    var enumValType = Enum.GetUnderlyingType(param.GetType());

                    if (enumValType == typeof(byte))
                    {
                        data = new byte[] { (byte)param };
                    }
                    else if (enumValType == typeof(short))
                    {
                        data = BitConverter.GetBytes((Int16)param);
                    }
                    else if (enumValType == typeof(int))
                    {
                        data = BitConverter.GetBytes((Int32)param);
                    }
                    else
                    {
                        data = BitConverter.GetBytes((Int64)param);
                    }
                }
                else if (param is byte[])
                {
                    data = (byte[])param;
                }
                else
                {
                    var type = param.GetType();


                    if (type.IsGenericType || type.IsArray)
                    {
                        if (TypeHelper.DicTypeStrs.Contains(type.Name))
                            data = SerializeDic((System.Collections.IDictionary)param);
                        else if (TypeHelper.ListTypeStrs.Contains(type.Name) || type.IsArray)
                            data = SerializeList((System.Collections.IEnumerable)param);
                        else
                            data = SerializeClass(param, type);
                    }
                    else if (type.IsClass)
                    {
                        data = SerializeClass(param, type);
                    }

                }
                if (data != null)
                    len = data.Length;
            }
            datas.AddRange(BitConverter.GetBytes(len));
            if (len > 0)
            {
                datas.AddRange(data);
            }
            return datas.Count == 0 ? null : datas.ToArray();
        }

        public static object Deserialize(Type type, byte[] datas, ref int offset)
        {
            dynamic obj = null;
            var len = 0;
            byte[] data = null;
            len = BitConverter.ToInt32(datas, offset);
            offset += 4;
            if (len > 0)
            {
                data = new byte[len];
                Buffer.BlockCopy(datas, offset, data, 0, len);
                offset += len;

                if (type == typeof(string))
                {
                    obj = Encoding.UTF8.GetString(data);
                }
                else if (type == typeof(byte))
                {
                    obj = (data);
                }
                else if (type == typeof(bool))
                {
                    obj = (BitConverter.ToBoolean(data, 0));
                }
                else if (type == typeof(short))
                {
                    obj = (BitConverter.ToInt16(data, 0));
                }
                else if (type == typeof(int))
                {
                    obj = (BitConverter.ToInt32(data, 0));
                }
                else if (type == typeof(long))
                {
                    obj = (BitConverter.ToInt64(data, 0));
                }
                else if (type == typeof(float))
                {
                    obj = (BitConverter.ToSingle(data, 0));
                }
                else if (type == typeof(double))
                {
                    obj = (BitConverter.ToDouble(data, 0));
                }
                else if (type == typeof(decimal))
                {
                    obj = (BitConverter.ToDouble(data, 0));
                }
                else if (type == typeof(DateTime))
                {
                    var dstr = Encoding.UTF8.GetString(data);
                    var ticks = long.Parse(dstr.Substring(2));
                    obj = (new DateTime(ticks));
                }
                else if (type.BaseType == typeof(Enum))
                {
                    var numType = Enum.GetUnderlyingType(type);

                    if (numType == typeof(byte))
                    {
                        obj = Enum.ToObject(type, data[0]);
                    }
                    else if (numType == typeof(short))
                    {
                        obj = Enum.ToObject(type, BitConverter.ToInt16(data, 0));
                    }
                    else if (numType == typeof(int))
                    {
                        obj = Enum.ToObject(type, BitConverter.ToInt32(data, 0));
                    }
                    else
                    {
                        obj = Enum.ToObject(type, BitConverter.ToInt64(data, 0));
                    }
                }
                else if (type == typeof(byte[]))
                {
                    obj = (byte[])data;
                }
                else if (type.IsGenericType)
                {
                    if (TypeHelper.ListTypeStrs.Contains(type.Name))
                    {
                        obj = DeserializeList(type, data);
                    }
                    else if (TypeHelper.DicTypeStrs.Contains(type.Name))
                    {
                        obj = DeserializeDic(type, data);
                    }
                    else
                    {
                        obj = DeserializeClass(type, data);
                    }
                }
                else if (type.IsClass)
                {
                    obj = DeserializeClass(type, data);
                }
                else if (type.IsArray)
                {
                    obj = DeserializeArray(type, data);
                }
                else
                {
                    throw new RPCPamarsException("ParamsSerializeUtil.Deserialize 未定义的类型：" + type.ToString());
                }

            }
            return obj;
        }
        */
    }
}

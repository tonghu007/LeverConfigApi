using System;
using System.Collections.Generic;
using System.Reflection;

namespace Lever.DBUtility
{
    /// <summary>
    /// 方法扩展
    /// </summary>
    public static class DictionaryExtension
    {
        /// <summary>
        /// 取指定类型值
        /// </summary>
        /// <typeparam name="T">取值类型</typeparam>
        /// <param name="dic">字典对象</param>
        /// <param name="key">字典key</param>
        /// <returns>返回指定类型值</returns>
        public static T GetValue<T>(this IDictionary<string, object> dic, string key)
        {
            return GetValue<T>(dic, key, default(T));
        }

        /// <summary>
        /// 取指定类型值，为空返回默认值
        /// </summary>
        /// <typeparam name="T">取值类型</typeparam>
        /// <param name="dic">字典对象</param>
        /// <param name="key">字典key</param>
        /// <param name="defalultValue">默认值</param>
        /// <returns>返回指定类型值</returns>
        public static T GetValue<T>(this IDictionary<string, object> dic, string key, T defalultValue)
        {
            if (dic == null) return defalultValue;
            return ((!dic.ContainsKey(key)) || dic[key] == null) ? defalultValue : (T)Convert.ChangeType(dic[key], typeof(T));
        }

        /// <summary>
        /// 将IDictionary<string, object>转为指定实体类对象，注意字典键与实体属性名一致
        /// </summary>
        /// <typeparam name="T">实体类</typeparam>
        /// <param name="dic">数据字典</param>
        /// <returns>实体类对象</returns>
        public static T ToObject<T>(this IDictionary<string, object> dic) where T : class, new()
        {
            T result = new T();
            Type t = result.GetType();
            foreach (PropertyInfo pi in t.GetProperties())
            {
                string key = pi.Name;
                if (dic.ContainsKey(key))
                {
                    pi.SetValue(result, dic[key], null);
                }
            }
            return result;
        }

        /// <summary>
        /// 将字典列表集合转为实体类对象列表集合，注意字典键与实体属性名一致
        /// </summary>
        /// <typeparam name="T">实体类</typeparam>
        /// <param name="list">字典列表集合</param>
        /// <returns>实体类对象列表集合</returns>
        public static IList<T> ToObjectList<T>(this IList<IDictionary<string, object>> list) where T : class, new()
        {
            IList<T> result = new List<T>();
            foreach (IDictionary<string, object> dic in list)
            {
                T model = dic.ToObject<T>();
                result.Add(model);
            }
            return result;
        }

        /// <summary>
        /// 将实体类对象转为字典
        /// </summary>
        /// <typeparam name="T">实体类</typeparam>
        /// <param name="model">实体类对象</param>
        /// <returns>字典</returns>
        public static IDictionary<string, object> ToDictionary<T>(this T model) where T : class
        {
            IDictionary<string, object> result = new Dictionary<string, object>();
            Type t = result.GetType();
            foreach (PropertyInfo pi in t.GetProperties())
            {
                string key = pi.Name;
                result[key] = pi.GetValue(model, null);
            }
            return result;
        }

        /// <summary>
        /// 将实体类对象列表集合转为字典列表集合
        /// </summary>
        /// <typeparam name="T">实体类</typeparam>
        /// <param name="list">实体类对象列表集合</param>
        /// <returns>字典列表集合</returns>
        public static IList<IDictionary<string, object>> ToDictionaryList<T>(this IList<T> list) where T : class
        {
            IList<IDictionary<string, object>> result = new List<IDictionary<string, object>>();
            foreach (T model in list)
            {
                result.Add(model.ToDictionary<T>());
            }
            return result;
        }

        /// <summary>
        /// 合并两个字段
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="first">第一个字典</param>
        /// <param name="second">第二个字段</param>
        public static IDictionary<TKey, TValue> Merge<TKey, TValue>(this IDictionary<TKey, TValue> first, IDictionary<TKey, TValue> second)
        {
            if (second == null || first == null) return null;
            IDictionary<TKey, TValue> result = new Dictionary<TKey, TValue>(first);
            foreach (var item in second)
                if (!result.ContainsKey(item.Key))
                    result.Add(item.Key, item.Value);
            return result;
        }
    }
}

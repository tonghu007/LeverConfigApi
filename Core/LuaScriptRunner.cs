using Newtonsoft.Json;
using NLua;
using NLua.Exceptions;
using Lever.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Lever.Core
{
    public class LuaScriptRunner
    {
        /// <summary>
        /// 将lua代码片段放入函数中执行
        /// </summary>
        /// <param name="lua">Lua对象</param>
        /// <param name="scriptCode">代码片段</param>
        /// <param name="parameters">参数</param>
        /// <returns>返回执行结果</returns>
        public static object[] ExecuteLuaScript(Lua lua, string scriptCode, IEnumerable<KeyValuePair<string, object>> parameters, object bodyJson)
        {
            if (parameters == null)
                parameters = new Dictionary<string, object>();
            string paramsData = LuaScriptRunner.ToLuaScript(parameters);
            string bodyJsonLuaScript = LuaScriptRunner.ToLuaScript(bodyJson);
            bodyJsonLuaScript = string.IsNullOrWhiteSpace(bodyJsonLuaScript) ? "{}" : bodyJsonLuaScript;
            lua.DoString("params=" + paramsData);
            lua.DoString("bodyJson=" + bodyJsonLuaScript);
            LuaTable paramsTable = lua.GetTable("params");
            LuaTable bodyJsonTable = lua.GetTable("bodyJson");
            string script = @"
                function func(params,bodyJson)
                    " + scriptCode + @"
                end";
            return RunLuaScript(() =>
            {
                lua.DoString(script);
                var scriptFunc = lua["func"] as LuaFunction;
                var res = scriptFunc.Call(paramsTable, bodyJsonTable);
                return res;
            });
        }

        /// <summary>
        /// 将lua代码片段放入函数中执行,将key/value构建为lua的单个参数传入
        /// </summary>
        /// <param name="lua"></param>
        /// <param name="scriptCode"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static object[] ExecuteLuaScript(Lua lua, string scriptCode, IEnumerable<KeyValuePair<string, object>> parameters)
        {
            if (parameters == null)
                parameters = new Dictionary<string, object>();
            string json = LuaScriptRunner.ToLuaScript(parameters);
            json = string.IsNullOrWhiteSpace(json) ? "{}" : json;
            StringBuilder paramsBuilder = new StringBuilder();
            IList<object> paramsValues = new List<object>();
            foreach (var pair in parameters)
            {
                if (paramsBuilder.Length > 0)
                    paramsBuilder.Append(",");
                paramsBuilder.Append(pair.Key);
                paramsValues.Add(pair.Value);
            }
            string script = @"
                function func(" + paramsBuilder + @")
                    " + scriptCode + @"
                end";
            return RunLuaScript(() =>
            {
                lua.DoString(script);
                var scriptFunc = lua["func"] as LuaFunction;
                var res = scriptFunc.Call(paramsValues.ToArray());
                return res;
            });
        }

        /// <summary>
        /// 将C#数据转为Lua数据字符串
        /// </summary>
        /// <param name="obj">数据对象</param>
        /// <returns>lua数据字符串</returns>
        public static string ToLuaScript(object obj)
        {
            string json = JsonConvert.SerializeObject(obj);
            //检查是否key/value json
            MatchCollection matches = Regex.Matches(json, "\"(?<p>\\w+)\":", RegexOptions.Multiline);
            if (matches.Count() == 0) return json;
            int index = 0;
            StringBuilder builder = new StringBuilder();
            foreach (Match m in matches)
            {
                Group group = m.Groups[0];
                Group fieldGroup = m.Groups[1];
                if (group.Index > 0)
                    builder.Append(json.Substring(index, group.Index - index));
                index = group.Index;
                builder.Append(fieldGroup.Value + " = ");
                index = index + group.Length;
            }
            builder.Append(json.Substring(index));
            string result = builder.ToString();
            //处理[]号
            int count = 0;
            builder.Length = 0;
            foreach (char c in result)
            {
                if (c == '[' && count % 2 == 0)
                    builder.Append("{");
                else if (c == ']' && count % 2 == 0)
                    builder.Append("}");
                else if (c == '"')
                {
                    count = count + 1;
                    builder.Append(c);
                }
                else
                {
                    builder.Append(c);
                }
            }
            return builder.ToString();
        }

        /// <summary>
        /// lua数据转C#数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static T LuaTableToCSharpData<T>(object obj)
        {
            object result = null;
            if (obj != null)
            {
                int kind = LuaScriptRunner.CheckLuaTable(obj);
                if (kind == 3)
                    return default(T);
                if (kind == 0)
                    result = obj;
                else if (kind == 1)
                    result = LuaScriptRunner.LuaTableToDictionary((LuaTable)obj);
                else
                    result = LuaScriptRunner.LuaTableToList((LuaTable)obj);
            }
            return (T)result;
        }

        private static object[] RunLuaScript(Func<object[]> runScript)
        {
            try
            {
                return runScript();
            }
            catch (Exception e)
            {
                if (e is LuaScriptException)
                {
                    string message = ((LuaScriptException)e).Message;
                    if (!string.IsNullOrWhiteSpace(message) && !message.Contains("string \"chunk\""))
                    {
                        throw new CustomException(11, message);
                    }
                    else
                    {
                        throw;
                    }
                }
                else
                {
                    throw;
                }
            }
        }

        /// <summary>
        /// Table字典转字典
        /// </summary>
        /// <param name="table">Table字典</param>
        /// <returns></returns>
        private static IEnumerable<KeyValuePair<string, object>> LuaTableToDictionary(LuaTable table)
        {
            IDictionary<string, object> result = new Dictionary<string, object>();
            foreach (var key in table.Keys)
            {
                var value = table[key];
                var kind = LuaScriptRunner.CheckLuaTable(value);
                if (kind == 0)
                {
                    result[key.ToString()] = value;
                }
                else if (kind == 1)
                {
                    IEnumerable<KeyValuePair<string, object>> res = LuaScriptRunner.LuaTableToDictionary((LuaTable)value);
                    result[key.ToString()] = res;
                }

                else
                {
                    IList<object> res = LuaScriptRunner.LuaTableToList((LuaTable)value);
                    result[key.ToString()] = res;
                }

            }
            return result;
        }

        /// <summary>
        /// Table数组转为List
        /// </summary>
        /// <param name="table">Table数组</param>
        /// <returns></returns>
        private static IList<object> LuaTableToList(LuaTable table)
        {
            IList<object> result = new List<object>();
            foreach (var key in table.Keys)
            {
                var value = table[key];
                var kind = LuaScriptRunner.CheckLuaTable(value);
                if (kind == 0)
                {
                    result.Add(value);
                }
                else if (kind == 1)
                {
                    IEnumerable<KeyValuePair<string, object>> res = LuaScriptRunner.LuaTableToDictionary((LuaTable)value);
                    result.Add(res);
                }
                else
                {
                    IList<object> res = LuaScriptRunner.LuaTableToList((LuaTable)value);
                    result.Add(res);
                }
            }
            return result;
        }

        /// <summary>
        /// 判断对象是否LuaTable，或者数组（顺序字典不适用）
        /// </summary>
        /// <param name="obj">对象</param>
        /// <returns>返回0=非LuaTable，也非数组；1=LuaTable，2=数组</returns>
        private static int CheckLuaTable(object obj)
        {
            if (!(obj is LuaTable))
                return 0;//不是Table,也不是数组
            LuaTable table = (LuaTable)obj;
            var keys = table.Keys;
            int length = keys.Count;
            long maxKey = 0;
            if (length == 0) return 3;
            foreach (var key in keys)
            {
                if (!(key is int || key is Int64))
                    return 1;//Table（字典）
                if ((long)key > maxKey)
                    maxKey = (long)key;
            }
            if (maxKey != length) return 1;//最大索引与长度不一致，为Table（字典）
            return 2;//数组
        }
    }
}

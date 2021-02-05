using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using NLua;
using Lever.Common;
using Lever.Core;
using Lever.DBUtility;
using Lever.Plugins;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;

namespace Lever.Dal
{
    public class DynamicApiDal : DynamicApiBaseDal
    {
        private readonly ILogger<DynamicApiDal> _logger;
        private readonly static AsyncLocal<string> threadLocalDataBaseKey = new AsyncLocal<string>();
        public DynamicApiDal(MultipleDbContext dbContext, ILogger<DynamicApiDal> logger) : base(dbContext)
        {
            _logger = logger;
        }

        /// <summary>
        /// Lua 调用接口
        /// </summary>
        /// <param name="code">接口编码</param>
        /// <param name="inputParameters">可传入参数</param>
        /// <returns></returns>
        public object DynamicFetch(string code, IDictionary<string, object> inputParameters = null)
        {
            IDictionary<string, object> config = this.GetApiConfig(code);
            if (config == null)
                throw new CustomException(10, "接口不存在");
            return this.DynamicFetch(config, code, inputParameters);
        }

        /// <summary>
        /// 外部调用接口
        /// </summary>
        /// <param name="code">接口编码</param>
        /// <param name="inputParameters">可传入参数</param>
        /// <returns></returns>
        public object DynamicFetch(IDictionary<string, object> config, string code, IDictionary<string, object> inputParameters = null)
        {
            string dataBaseKey = config.GetValue<string>("DataBaseKey");
            string pluginAssemblyPath = config.GetValue<string>("PluginAssemblyPath");
            string pluginClassName = config.GetValue<string>("PluginClassName");
            threadLocalDataBaseKey.Value = dataBaseKey;
            ConfigApiPlugin dynamicApiPlugin = ReflectorHelper.GetPluginInstance<ConfigApiPlugin>(pluginAssemblyPath, pluginClassName);
            IDictionary<string, object> parameters = RequestDataHelper.GetMixParams();
            IDictionary<string, IList<IFormFile>> files = RequestDataHelper.GetAllFiles();
            IDictionary<string, object> headers = RequestDataHelper.GetHeaders();
            IDictionary<string, object> cookies = RequestDataHelper.GetCookies();
            object bodyJson = RequestDataHelper.GetBodyJsonParameters();
            if (inputParameters != null && inputParameters.Count > 0)
                parameters = RequestDataHelper.MergeDictionary(parameters, inputParameters);
            return this.AopDynamicApi<object>((apiConfig, paramsData, formFiles, reqHeaders, reqCookies, json) =>
            {
                if (dynamicApiPlugin != null)
                {
                    IDbHelper dbHelper = this._dbContext.Use(dataBaseKey);
                    //调用接口前扩展处理
                    dynamicApiPlugin.Before(dbHelper, apiConfig, paramsData, formFiles, json);
                }
            }, (apiConfig, paramsData, json) =>
            {
                string scriptCode = apiConfig.GetValue<string>("ScriptCode");
                /*
                * 1=单一结果(单个值，或者一条sql语句执行结果)
                * 2=分页
                * 3=列表结果集（多个值以List<object>返回
                * 4=字典结果集（多个值以Dictionary<string,object>返回
                * 5=主从结果集 （会查询嵌套子查询，多个值以Dictionary<string,object>返回
                * 6=返回脚本执行结果（直接返回脚本执行结果）
                */
                int codeKind = apiConfig.GetValue<int>("CodeKind");
                int apiKind = apiConfig.GetValue<int>("ApiKind");//0=公共接口；1=对内接口
                int status = apiConfig.GetValue<int>("Status");//0=禁用；1=启用
                return this.ExecuteScript(scriptCode, codeKind, paramsData, bodyJson);
            }, (apiConfig, paramsData, json, result) =>
            {
                if (dynamicApiPlugin != null)
                {
                    IDbHelper dbHelper = this._dbContext.Use(dataBaseKey);
                    //调用接口后扩展处理
                    return dynamicApiPlugin.After(dbHelper, apiConfig, paramsData, json, result);
                }
                return result;
            }, config, parameters, files, headers, cookies, bodyJson);
        }
        //lua中调用此方法执行已配置的内部或者外部接口
        public override object LuaInvokeDynamicApi(string code, LuaTable table)
        {
            try
            {
                IDictionary<string, object> parameters = LuaScriptRunner.LuaTableToCSharpData<IDictionary<string, object>>(table);
                object result = this.DynamicFetch(code, parameters);
                string luaScript = LuaScriptRunner.ToLuaScript(result);
                Lua lua = GetLua();
                var returns = lua.DoString("return " + luaScript);
                return returns[0];
            }
            catch
            {
                _logger.LogError("Lua调用接口异常");
                throw;
            }
        }

        public override object LuaExecuteSql(string sql, int codeKind, LuaTable table)
        {
            try
            {
                IDictionary<string, object> parameters = LuaScriptRunner.LuaTableToCSharpData<IDictionary<string, object>>(table);
                object result = this.ExecuteSql(sql, codeKind, parameters);
                string luaScript = LuaScriptRunner.ToLuaScript(result);
                Lua lua = GetLua();
                var returns = lua.DoString("return " + luaScript);
                return returns[0];
            }
            catch
            {
                _logger.LogError("Lua执行SQL语句异常");
                throw;
            }
        }

        public override object NewId()
        {
            return base.NewId();
        }

        public override string ObjectToLuaScriptString(object obj)
        {
            return base.ObjectToLuaScriptString(obj);
        }

        public IDictionary<string, object> GetApiConfig(string code)
        {
            IDbHelper db = this._dbContext.Use("config_database");
            db.AddInputParameter("ApiIdCode", code);
            IDictionary<string, object> config = db.SelectRow("select t.*,g.GroupName,g.SystemPramsPluginClassName,g.SystemPramsPluginAssemblyPath,g.DataBaseKey,g.Issuer,g.Audience,g.Expires,g.ClockSkew,g.SecurityKey from ApiConfig t left join ApiGroup g on t.GroupId=g.GroupId where ApiIdCode=@ApiIdCode");
            return config;
        }

        public IList<IDictionary<string, object>> GetApiParams(string code)
        {
            IDbHelper db = this._dbContext.Use("config_database");
            db.AddInputParameter("ApiIdCode", code);
            //return db.ExecuteQuery("select t.*,case t.ParamType when 0 then 'String' when 1 then 'Integer' when 2 then 'Long' when 3 then 'Boolean' when 4 then 'Date' when 5 then 'DateTime' when 6 then 'Key/Value' when 7 then 'List'  end  as ParamTypeText from ApiParams t where ApiIdCode=@ApiIdCode");
            return db.ExecuteQuery("select t.*,case t.ParamType when 0 then 'String' when 1 then 'Integer' when 2 then 'Long' when 3 then 'Double' when 4 then 'Float' when 5 then 'Decimal' when 6 then 'Boolean' when 7 then 'Date' when 8 then 'DateTime' when 9 then 'ULong' when 10 then 'Key/Value' when 11 then 'List' when 12 then 'File' end as ParamTypeText from ApiParams t where ApiIdCode=@ApiIdCode");
        }

        private T AopDynamicApi<T>(Action<IDictionary<string, object>, IDictionary<string, object>, IDictionary<string, IList<IFormFile>>, IDictionary<string, object>, IDictionary<string, object>, object> beforeFunc,
            Func<IDictionary<string, object>, IDictionary<string, object>, object, T> dynamicApiFunc,
            Func<IDictionary<string, object>, IDictionary<string, object>, object, T, T> afterFunc,
            IDictionary<string, object> config,
            IDictionary<string, object> parameters,
            IDictionary<string, IList<IFormFile>> formFiles, IDictionary<string, object> headers, IDictionary<string, object> cookies, object bodyJson)
        {
            beforeFunc?.Invoke(config, parameters, formFiles, headers, cookies, bodyJson);
            T result = dynamicApiFunc(config, parameters, bodyJson);
            if (afterFunc != null)
                result = afterFunc(config, parameters, bodyJson, result);
            return result;

        }

        private object ExecuteScript(string scriptCode, int codeKind, IDictionary<string, object> parameters, object bodyJson)
        {
            object result = null;
            if (codeKind == 4)//脚本结果
            {
                Lua lua = GetLua();
                result = LuaScriptRunner.ExecuteLuaScript(lua, scriptCode, parameters, bodyJson);
            }
            else
            {
                IList<SqlBuildModel> sqlItems = this.SqlBuilderModel(scriptCode, parameters, bodyJson);
                if (sqlItems.Count == 0)
                    throw new CustomException(11, "SQL语句配置错误");

                switch (codeKind)
                {
                    case 1://分页
                        {
                            //分页，第一条语句为分页语句，后面如果有其他语句，以字典值的形式返回结果
                            result = new Dictionary<string, object>();
                            SqlBuildModel sqlItem = null;
                            for (var i = 0; i < sqlItems.Count; i++)
                            {
                                sqlItem = sqlItems[i];
                                var key = i.ToString();
                                if (!string.IsNullOrWhiteSpace(sqlItem.Key))
                                    key = sqlItem.Key;
                                object res;
                                if (i == 0)
                                {
                                    res = this.Page(sqlItem.Sql, parameters);
                                    this.ForEachMaster(sqlItem, parameters, ((IDictionary<string, object>)res)["List"]);
                                }
                                else
                                {
                                    res = this.MasterDetail(sqlItem, parameters);
                                }
                                ((IDictionary<string, object>)result)[key] = res;
                            }
                            if (((Dictionary<string, object>)result).Count() == 1)
                                result = ((Dictionary<string, object>)result).ElementAt(0).Value;
                        }
                        break;
                    case 2://结果集 - 字典(支持主从)
                        {
                            int index = 0;
                            result = new Dictionary<string, object>();
                            foreach (var item in sqlItems)
                            {
                                var key = index.ToString();
                                if (!string.IsNullOrWhiteSpace(item.Key))
                                    key = item.Key;
                                object res = this.MasterDetail(item, parameters);
                                ((Dictionary<string, object>)result)[key] = res;
                                index++;
                            }
                            if (((Dictionary<string, object>)result).Count() == 1)
                                result = ((Dictionary<string, object>)result).ElementAt(0).Value;
                        }
                        break;
                    case 3://结果集-列表(支持主从)
                        {
                            int index = 0;
                            result = new List<object>();
                            foreach (var item in sqlItems)
                            {
                                var key = index.ToString();
                                if (!string.IsNullOrWhiteSpace(item.Key))
                                    key = item.Key;
                                object res = this.MasterDetail(item, parameters);
                                ((List<object>)result).Add(res);
                                index++;
                            }
                            if (((List<object>)result).Count() == 1)
                                result = ((List<object>)result).ElementAt(0);
                        }
                        break;
                    default:
                        throw new CustomException(11, "非有效执行结果类型");
                }
            }
            return result;
        }

        //主从查询
        private object MasterDetail(SqlBuildModel item, IDictionary<string, object> parameters)
        {
            object result = this.ExecuteSql(item.Sql, item.CodeKind, item.Parameters);
            if (item.Children != null && item.Children.Count > 0)
            {
                this.ForEachMaster(item, parameters, result);
            }
            return result;
        }

        private void ForEachMaster(SqlBuildModel item, IDictionary<string, object> parameters, object result)
        {
            if (result is IDictionary<string, object>)
            {
                this.RowMasterDetail((IDictionary<string, object>)result, item, parameters);
            }
            else if (result is IList<IDictionary<string, object>>)
            {

                foreach (var row in (IList<IDictionary<string, object>>)result)
                {
                    this.RowMasterDetail((IDictionary<string, object>)row, item, parameters);
                }
            }
        }

        private void RowMasterDetail(IDictionary<string, object> parent, SqlBuildModel item, IDictionary<string, object> parameters)
        {
            if (item.Children != null)
            {
                foreach (var child in item.Children)
                {
                    if (child.RelationKeys?.Count > 0)
                    {
                        foreach (string relationKey in child.RelationKeys)
                        {
                            string key = relationKey.Trim();
                            if (key == "") continue;
                            if (((IDictionary<string, object>)parent).ContainsKey(key))
                            {
                                object paramValue = ((IDictionary<string, object>)parent)[key];
                                child.Parameters[relationKey] = paramValue;
                            }
                            else
                            {
                                if (!parameters.ContainsKey(key))
                                {
                                    throw new CustomException(11, "主从查询关联参数有误");
                                }
                            }
                        }
                    }
                 ((IDictionary<string, object>)parent)[child.Key] = this.MasterDetail(child, parameters);
                }
            }
        }

        private object ExecuteSql(string sql, long sqlKind, IDictionary<string, object> parameters)
        {
            if (sql.Trim() == "")
                throw new CustomException(11, "执行SQL不能为空");
            object result = null;
            string dataBaseKey = this.GetCurrentDataBaseKey();
            IDbHelper db = this._dbContext.Use(dataBaseKey);
            switch (sqlKind)
            {
                case 1://列表
                    this.AnalyseSqlParameters(db, ref sql, parameters);
                    result = db.ExecuteQuery(sql);
                    break;
                case 2://单行
                    this.AnalyseSqlParameters(db, ref sql, parameters);
                    result = db.SelectRow(sql);
                    break;
                case 3://是否存在（true|false）
                    this.AnalyseSqlParameters(db, ref sql, parameters);
                    result = db.Exists(sql);
                    break;
                case 4://记录数
                    result = this.Count(db, sql, parameters);
                    break;
                case 5://单个值
                    this.AnalyseSqlParameters(db, ref sql, parameters);
                    result = db.ExecuteScalar(sql);
                    break;
                case 6://返回受影响记录数
                    this.AnalyseSqlParameters(db, ref sql, parameters);
                    result = db.ExecuteNonQuery(sql);
                    break;
                default:
                    throw new CustomException(11, "非有效数据库脚本执行类型");
            }
            return result;
        }

        private void ExecuteNonQuery(string sql, IDictionary<string, object> parameters)
        {
            string dataBaseKey = this.GetCurrentDataBaseKey();
            IDbHelper db = this._dbContext.Use(dataBaseKey);
            this.AnalyseSqlParameters(db, ref sql, parameters);
            db.ExecuteNonQuery(sql, parameters);
        }

        private IList<SqlBuildModel> SqlBuilderModel(string scriptCode, IDictionary<string, object> parameters, object bodyJson)
        {
            Lua lua = this.GetLua();
            var res = LuaScriptRunner.ExecuteLuaScript(lua, scriptCode, parameters, bodyJson);
            IList<SqlBuildModel> result = new List<SqlBuildModel>();
            for (var i = 0; i < res.Length; i++)
            {
                var item = res[i];
                if (item is LuaTable)
                {
                    var table = (LuaTable)item;
                    var extraParams = LuaScriptRunner.LuaTableToCSharpData<IDictionary<string, object>>(table["ExtraParams"]);
                    var sqlParameters = RequestDataHelper.MergeDictionary(parameters, extraParams);//合并附加参数到sql执行参数中
                    IList<object> relationKeys = LuaScriptRunner.LuaTableToCSharpData<IList<object>>(table["RelationKeys"]);
                    long codeKind = table["CodeKind"] == null ? 0 : (long)table["CodeKind"];
                    string key = (string)table["Key"];
                    var model = new SqlBuildModel { Sql = (string)table["Sql"], Parameters = sqlParameters, CodeKind = codeKind, RelationKeys = relationKeys, Key = key };
                    result.Add(model);
                    this.ChildSqlBuilderModel(parameters, model, LuaScriptRunner.LuaTableToCSharpData<object>(table["Children"]));
                }
                else
                {
                    throw new CustomException(99, "构建SQL的脚本返回有误");
                }
            }
            return result;
        }

        //递归解析主从查询的从查询
        private void ChildSqlBuilderModel(IDictionary<string, object> parameters, SqlBuildModel parentModel, object children)
        {
            if (children == null || !(children is IEnumerable<KeyValuePair<string, object>> || children is IList<object>))
                return;
            if (children is IEnumerable<KeyValuePair<string, object>>)
            {
                var child = (IDictionary<string, object>)children;
                var extraParams = !child.ContainsKey("ExtraParams") ? null : (IDictionary<string, object>)child["ExtraParams"];
                parameters = RequestDataHelper.MergeDictionary(parameters, extraParams);//合并附加参数到sql执行参数中
                IList<object> relationKeys = !child.ContainsKey("RelationKeys") ? null : (IList<object>)child["RelationKeys"];
                long codeKind = child.ContainsKey("CodeKind") ? (long)child["CodeKind"] : 0;
                string key = (string)child["Key"];
                var model = new SqlBuildModel { Sql = (string)child["Sql"], Parameters = parameters, CodeKind = codeKind, RelationKeys = relationKeys, Key = key };
                if (parentModel.Children == null)
                    parentModel.Children = new List<SqlBuildModel>();
                parentModel.Children.Add(model);
                object nextChildren = child.ContainsKey("Children") ? child["Children"] : null;
                this.ChildSqlBuilderModel(parameters, model, nextChildren);
            }
            else
            {
                foreach (var child in (IList<object>)children)
                {
                    this.ChildSqlBuilderModel(parameters, parentModel, child);
                }
            }
        }

        private IDictionary<string, object> Page(string sql, IDictionary<string, object> parameters)
        {
            string dataBaseKey = this.GetCurrentDataBaseKey();
            IDbHelper db = this._dbContext.Use(dataBaseKey);
            long count = this.Count(db, sql, parameters);
            long pageSize = long.Parse(parameters["PageSize"].ToString());//固定参数
            long pageIndex = long.Parse(parameters["PageIndex"].ToString());//固定参数
            long maxPage = (long)Math.Ceiling((decimal)count / pageSize);
            if (pageIndex <= 0)
                pageIndex = 1;
            if (pageIndex > maxPage && maxPage > 0)
                pageIndex = maxPage;
            sql = sql + " limit @Limit offset @Offset";//Limit和Offset为固定参数
            long limit = pageSize;
            long offset = (pageIndex - 1) * pageSize;
            parameters["Limit"] = limit;
            parameters["Offset"] = offset;
            this.AnalyseSqlParameters(db, ref sql, parameters);
            var list = db.ExecuteQuery(sql);
            return new Dictionary<string, object> { { "TotalRows", count }, { "TotalPage", maxPage }, { "PageSize", pageSize }, { "PageIndex", pageIndex }, { "List", list } };
        }

        private long Count(IDbHelper db, string sql, IDictionary<string, object> parameters)
        {
            //将sql重建为查询数据条数的语句
            sql = Regex.Replace(sql, @"^\s*select[\S\s]+?from\s+", "select count(1) as v_count from ", RegexOptions.Multiline | RegexOptions.IgnoreCase);
            //去掉sql语句最后的排序语句
            sql = Regex.Replace(sql, @"order\s+by[\s\w\.\,]+$", "", RegexOptions.Multiline | RegexOptions.IgnoreCase);
            this.AnalyseSqlParameters(db, ref sql, parameters);
            object res= db.ExecuteScalar(sql);
            if (res == null)
                return 0;
            else
                return (long)res;
        }

        private void AnalyseSqlParameters(IDbHelper db, ref string sql, IDictionary<string, object> parameters)
        {
            if (parameters == null) return;
            //允许mysql变量
            sql = sql.Replace("@@", "#$#");
            MatchCollection matches = Regex.Matches(sql, @"@(?<p>\w+)", RegexOptions.Multiline | RegexOptions.IgnoreCase);
            IDictionary<string, object> paramDic = new Dictionary<string, object>();
            foreach (Match m in matches)
            {
                var paramName = m.Groups["p"].Value;
                paramDic[paramName] = parameters[paramName];
            }
            db.AddInputParameter(paramDic);
            sql = sql.Replace("#$#", "@");
        }

        private string GetCurrentDataBaseKey() {
            return threadLocalDataBaseKey.Value;
        }

        internal class SqlBuildModel
        {
            public string Sql { get; set; }
            public IDictionary<string, object> Parameters { get; set; }

            public long CodeKind { get; set; }

            //执行结果key
            public string Key { get; set; }

            //关联主表的key（用于从主查询中获取参数）
            public IList<object> RelationKeys { get; set; }

            //主从查询中
            public IList<SqlBuildModel> Children { get; set; }
        }
    }
}

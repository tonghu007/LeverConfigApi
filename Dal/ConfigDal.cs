using Lever.DBUtility;
using System;
using System.Collections.Generic;
using System.Text;

namespace Lever.Dal
{
    public class ConfigDal : BaseDal
    {
        public ConfigDal(MultipleDbContext dbContext) : base(dbContext)
        { }

        public IDictionary<string, object> ApiConfigPage(int pageIndex, int pageSize, IDictionary<string, object> searchKeys)
        {
            StringBuilder whereSql = new StringBuilder();
            IDictionary<string, object> searchParams = new Dictionary<string, object>();
            foreach (var pair in searchKeys)
            {
                if (whereSql.Length > 0)
                    whereSql.Append(" and ");
                whereSql.Append(pair.Key + " like @" + pair.Key);
                searchParams[pair.Key] = "%" + pair.Value + "%";
            }
            IDbHelper db = this._dbContext.Use("config_database");
            db.AddInputParameter(searchParams);
            string sql = "select count(1) as v_count from ApiConfig where 1=1 " + (whereSql.Length > 0 ? " and " + whereSql : "");
            long count = (long)db.ExecuteScalar(sql);
            if (pageSize <= 0)
                pageSize = 10;
            int maxPage = (int)Math.Ceiling((decimal)count / pageSize);
            if (pageIndex > maxPage)
                pageIndex = maxPage;
            if (pageIndex <= 0)
                pageIndex = 1;
            db.AddInputParameter(searchParams);
            db.AddInputParameter("Offset", (pageIndex - 1) * pageSize);
            db.AddInputParameter("Limit", pageSize);
            var list = db.ExecuteQuery(@"select t.*,case when t.Status=1 then '启用' else '停用' end StatusText,case when t.ApiKind=1 then '对内接口' else '对外接口' end as ApiKindText,CASE  
    WHEN t.CodeKind = 1 THEN '分页' 
    WHEN t.CodeKind = 2 THEN '结果集-列表' 
    WHEN t.CodeKind = 3 THEN '结果集-列表' 
    WHEN t.CodeKind = 4 THEN '脚本执行结果'
    ELSE '其它' 
  END CodeKindText from ApiConfig t where 1=1 " + (whereSql.Length > 0 ? " and " + whereSql : "") + " limit @Limit offset @Offset");
            return new Dictionary<string, object> { { "TotalRows", count }, { "TotalPage", maxPage }, { "PageSize", pageSize }, { "PageIndex", pageIndex }, { "List", list } };
        }

        public IDictionary<string, object> GetApiRow(string apiIdCode)
        {
            IDbHelper db = this._dbContext.Use("config_database");
            db.AddInputParameter("ApiIdCode", apiIdCode);
            db.AddInputParameter("Path", "/api");
            return db.SelectRow(@"select t.*,CASE LOWER( t.Method ) 
    WHEN 'post' THEN CASE lower(t.PostFrom)
      WHEN 'frombody' THEN @Path || '/PostBody/' || t.ApiIdCode
      WHEN 'fromform' THEN @Path || '/PostForm/' || t.ApiIdCode
    END
    WHEN 'put' THEN CASE lower(t.PostFrom)
      WHEN 'frombody' THEN @Path || '/PutBody/' || t.ApiIdCode
      WHEN 'fromform' THEN @Path || '/PutForm/' || t.ApiIdCode
    END
    WHEN 'get' THEN @Path || '/Get/' || t.ApiIdCode
    WHEN 'delete' THEN @Path || '/Delete/' || t.ApiIdCode
  END AS ApiUrl from ApiConfig t where ApiIdCode=@ApiIdCode");
        }

        public IDictionary<string, object> InsertApi(IDictionary<string, object> parameters)
        {
            IDbHelper db = this._dbContext.Use("config_database");
            string apiIdCode = DbHelper.Decimal2Str(DbHelper.NewLongId());
            parameters["ApiIdCode"] = apiIdCode;
            db.Insert("ApiConfig", parameters);
            return parameters;
        }

        public IDictionary<string, object> UpdateApi(IDictionary<string, object> parameters)
        {
            IDbHelper db = this._dbContext.Use("config_database");
            db.Update("ApiConfig", new string[] { "ApiIdCode" }, parameters);
            return parameters;
        }

        public void DeleteApi(string apiIdCode)
        {
            IDbHelper db = this._dbContext.Use("config_database");
            db.AddInputParameter("ApiIdCode", apiIdCode);
            db.ExecuteNonQuery("Delete From ApiConfig where ApiIdCode=@ApiIdCode");
        }

        public IDictionary<string, object> ParamsPage(string apiIdCode, int pageIndex, int pageSize)
        {
            IDbHelper db = this._dbContext.Use("config_database");
            db.AddInputParameter("ApiIdCode", apiIdCode);
            string sql = "select count(1) as v_count from ApiParams where ApiIdCode=@ApiIdCode";
            long count = (long)db.ExecuteScalar(sql);
            if (pageSize <= 0)
                pageSize = 10;
            int maxPage = (int)Math.Ceiling((decimal)count / pageSize);
            if (pageIndex > maxPage)
                pageIndex = maxPage;
            if (pageIndex <= 0)
                pageIndex = 1;
            db.AddInputParameter("ApiIdCode", apiIdCode);
            db.AddInputParameter("Offset", (pageIndex - 1) * pageSize);
            db.AddInputParameter("Limit", pageSize);
            var list = db.ExecuteQuery(@"select t.*,case t.ParamType when 0 then 'String' when 1 then 'Integer' when 2 then 'Long' when 3 then 'Double' when 4 then 'Float' when 5 then 'Decimal' when 6 then 'Boolean' when 7 then 'Date' when 8 then 'DateTime' when 9 then 'ULong' when 10 then 'Key/Value' when 11 then 'List' when 12 then 'File' end as ParamTypeText from ApiParams t where ApiIdCode=@ApiIdCode limit @Limit offset @Offset");
            return new Dictionary<string, object> { { "TotalRows", count }, { "TotalPage", maxPage }, { "PageSize", pageSize }, { "PageIndex", pageIndex }, { "List", list } };
        }

        public IDictionary<string, object> GetParamRow(long paramId)
        {
            IDbHelper db = this._dbContext.Use("config_database");
            db.AddInputParameter("ParamId", paramId);
            return db.SelectRow("select * from ApiParams where ParamId=@ParamId");
        }

        public IDictionary<string, object> InsertParam(IDictionary<string, object> parameters)
        {
            IDbHelper db = this._dbContext.Use("config_database");
            long paramId = DbHelper.NewLongId();
            parameters["ParamId"] = paramId;
            db.Insert("ApiParams", parameters);
            return parameters;
        }

        public IDictionary<string, object> UpdateParam(IDictionary<string, object> parameters)
        {
            IDbHelper db = this._dbContext.Use("config_database");
            db.Update("ApiParams", new string[] { "ParamId" }, parameters);
            return parameters;
        }

        public void DeleteParam(long paramId)
        {
            IDbHelper db = this._dbContext.Use("config_database");
            db.AddInputParameter("ParamId", paramId);
            db.ExecuteNonQuery("Delete From ApiParams where ParamId=@ParamId");
        }

        public void DeleteParam(string apiIdCode,int isFixed)
        {
            IDbHelper db = this._dbContext.Use("config_database");
            db.AddInputParameter("ApiIdCode", apiIdCode);
            db.AddInputParameter("IsFixed", isFixed);
            db.ExecuteNonQuery("Delete From ApiParams where ApiIdCode=@ApiIdCode And IsFixed=@IsFixed");
        }

        public void DeleteParam(string apiIdCode)
        {
            IDbHelper db = this._dbContext.Use("config_database");
            db.AddInputParameter("ApiIdCode", apiIdCode);
            db.ExecuteNonQuery("Delete From ApiParams where ApiIdCode=@ApiIdCode");
        }

        public IDictionary<string, object> QueryApiConfigPage(int pageIndex, int pageSize, int apiKind, IDictionary<string, object> searchKeys)
        {
            StringBuilder whereSql = new StringBuilder();
            IDictionary<string, object> searchParams = new Dictionary<string, object>();
            foreach (var pair in searchKeys)
            {
                if (whereSql.Length > 0)
                    whereSql.Append(" and ");
                whereSql.Append(pair.Key + " like @" + pair.Key);
                searchParams[pair.Key] = "%" + pair.Value + "%";
            }
            IDbHelper db = this._dbContext.Use("config_database");
            db.AddInputParameter(searchParams);
            db.AddInputParameter("ApiKind", apiKind);
            string sql = "select count(1) as v_count from ApiConfig where Status=1 and ApiKind=@ApiKind " + (whereSql.Length > 0 ? " and " + whereSql : "");
            long count = (long)db.ExecuteScalar(sql);
            if (pageSize <= 0)
                pageSize = 10;
            int maxPage = (int)Math.Ceiling((decimal)count / pageSize);
            if (pageIndex > maxPage)
                pageIndex = maxPage;
            if (pageIndex <= 0)
                pageIndex = 1;
            db.AddInputParameter(searchParams);
            db.AddInputParameter("Path", "/api");
            db.AddInputParameter("ApiKind", apiKind);
            db.AddInputParameter("Offset", (pageIndex - 1) * pageSize);
            db.AddInputParameter("Limit", pageSize);
            var list = db.ExecuteQuery(@"SELECT 
  t.*, 
  CASE 
    WHEN t.ApiKind = 1 THEN '对内接口' 
    ELSE '对外接口' 
  END AS ApiKindText , 
  CASE  
    WHEN t.CodeKind = 1 THEN '分页' 
    WHEN t.CodeKind = 2 THEN '结果集-列表' 
    WHEN t.CodeKind = 3 THEN '结果集-列表' 
    WHEN t.CodeKind = 4 THEN '返回脚本执行结果'
    ELSE '其它' 
  END CodeKindText, 
  CASE LOWER( t.Method ) 
    WHEN 'post' THEN CASE lower(t.PostFrom) 
      WHEN 'frombody' THEN @Path || '/PostBody/' || t.ApiIdCode 
      WHEN 'fromform' THEN @Path || '/PostForm/' || t.ApiIdCode 
    END 
    WHEN 'put' THEN CASE lower(t.PostFrom) 
      WHEN 'frombody' THEN @Path || '/PutBody/' || t.ApiIdCode 
      WHEN 'fromform' THEN @Path || '/PutForm/' || t.ApiIdCode 
    END 
    WHEN 'get' THEN @Path || '/Get/' || t.ApiIdCode 
    WHEN 'delete' THEN @Path || '/Delete/' || t.ApiIdCode 
  END AS ApiUrl 
FROM 
  ApiConfig t 
WHERE 
  Status = 1 AND ApiKind = @ApiKind" + (whereSql.Length > 0 ? " and " + whereSql : "") + " limit @Limit offset @Offset");
            return new Dictionary<string, object> { { "TotalRows", count }, { "TotalPage", maxPage }, { "PageSize", pageSize }, { "PageIndex", pageIndex }, { "List", list } };
        }


        //ParamsKind 0=普通参数；1=系统参数；2=Id值
        public IList<IDictionary<string, object>> GetApiParaqms(string apiIdCode,params int[] paramsKind)
        {
            IDbHelper db = this._dbContext.Use("config_database");
            string inStr = this.BuildIn<int>(db,paramsKind, "ParamsKind");
            db.AddInputParameter("ApiIdCode", apiIdCode);
            IList<IDictionary<string, object>> list;
            if (inStr.Trim() != "")
            {
                list = db.ExecuteQuery($@"select t.*,case t.ParamType when 0 then 'String' when 1 then 'Integer' when 2 then 'Long' when 3 then 'Double' when 4 then 'Float' when 5 then 'Decimal' when 6 then 'Boolean' when 7 then 'Date' when 8 then 'DateTime' when 9 then 'ULong' when 10 then 'Key/Value' when 11 then 'List' when 12 then 'File' end as ParamTypeText from ApiParams t where ApiIdCode=@ApiIdCode And ParamsKind in ({inStr})");
            }
            else {
                list = db.ExecuteQuery($@"select t.*,case t.ParamType when 0 then 'String' when 1 then 'Integer' when 2 then 'Long' when 3 then 'Double' when 4 then 'Float' when 5 then 'Decimal' when 6 then 'Boolean' when 7 then 'Date' when 8 then 'DateTime' when 9 then 'ULong' when 10 then 'Key/Value' when 11 then 'List' when 12 then 'File' end as ParamTypeText from ApiParams t where ApiIdCode=@ApiIdCode");
            }
            return list;
        }

        public IDictionary<string, object> GroupPage(int pageIndex, int pageSize, IDictionary<string, object> searchKeys)
        {
            StringBuilder whereSql = new StringBuilder();
            IDictionary<string, object> searchParams = new Dictionary<string, object>();
            foreach (var pair in searchKeys)
            {
                if (whereSql.Length > 0)
                    whereSql.Append(" and ");
                whereSql.Append(pair.Key + " like @" + pair.Key);
                searchParams[pair.Key] = "%" + pair.Value + "%";
            }
            IDbHelper db = this._dbContext.Use("config_database");
            db.AddInputParameter(searchParams);
            string sql = "select count(1) as v_count from ApiGroup where 1=1 " + (whereSql.Length > 0 ? " and " + whereSql : "");
            long count = (long)db.ExecuteScalar(sql);
            if (pageSize <= 0)
                pageSize = 10;
            int maxPage = (int)Math.Ceiling((decimal)count / pageSize);
            if (pageIndex > maxPage)
                pageIndex = maxPage;
            if (pageIndex <= 0)
                pageIndex = 1;
            db.AddInputParameter(searchParams);
            db.AddInputParameter("Limit", pageSize);
            db.AddInputParameter("Offset", (pageIndex - 1) * pageSize);
            var list = db.ExecuteQuery(@"SELECT t.GroupId,t.GroupName,t.Status,t.DataBaseKey,t.SystemPramsPluginAssemblyPath,t.SystemPramsPluginClassName,t.Remark,case when t.Status=1 then '启用' else '停用' end StatusText FROM ApiGroup t WHERE 1 = 1 " + (whereSql.Length > 0 ? " and " + whereSql : "") + " limit @Limit offset @Offset");
            return new Dictionary<string, object> { { "TotalRows", count }, { "TotalPage", maxPage }, { "PageSize", pageSize }, { "PageIndex", pageIndex }, { "List", list } };
        }

        public void DeleteGroup(string groupId)
        {
            IDbHelper db = this._dbContext.Use("config_database");
            db.AddInputParameter("GroupId", groupId);
            db.ExecuteNonQuery("Delete From ApiGroup where GroupId=@GroupId");
        }

        public IDictionary<string, object> InsertGroup(IDictionary<string, object> parameters)
        {
            IDbHelper db = this._dbContext.Use("config_database");
            long paramId = DbHelper.NewLongId();
            parameters["GroupId"] = paramId;
            db.Insert("ApiGroup", parameters);
            return parameters;
        }

        public IDictionary<string, object> UpdateGroup(IDictionary<string, object> parameters)
        {
            IDbHelper db = this._dbContext.Use("config_database");
            db.Update("ApiGroup", new string[] { "GroupId" }, parameters);
            return parameters;
        }

        public IDictionary<string, object> GetGroupRow(long groupId)
        {
            IDbHelper db = this._dbContext.Use("config_database");
            db.AddInputParameter("GroupId", groupId);
            return db.SelectRow("select * from ApiGroup where GroupId=@GroupId");
        }

        public IList<IDictionary<string, object>> GetAllGroup() {
            IDbHelper db = this._dbContext.Use("config_database");
            return db.ExecuteQuery("select t.GroupId,t.GroupName,t.Status,t.DataBaseKey,t.SystemPramsPluginAssemblyPath,t.SystemPramsPluginClassName,t.Remark from ApiGroup t where t.Status=1");
        }
    }
}

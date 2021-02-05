using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace Lever.DBUtility
{
    public class DbHelper : IDbHelper
    {
        #region 静态变量
        private readonly static AsyncLocal<IDictionary<IDbHelper, Db>> threadLocalDb = new AsyncLocal<IDictionary<IDbHelper, Db>>();
        #endregion

        #region 属性
        private string ProviderName { get; set; }
        private string ConnectionString { get; set; }
        private string DataBase { get; set; }
        private string LogPath { get; set; }
        private string ParamPrefix { get; set; } = "@";
        private string SqlPrefix { get; set; } = "@";
        private bool IsLogSql { get; set; } = false;
        private bool IsTransaction { get; set; } = true;
        private IsolationLevel IsolationLevel { get; set; }
        private string NameMode { get; set; }

        private Guid Guid { get; set; }
        private Db Db
        {
            get
            {
                return this.GetCurrentDb();
            }
        }

        #endregion

        #region 构造行数

        /// <summary>
        /// 通过配置参数构造DbHelper
        /// </summary>
        /// <param name="providerName">provider名</param>
        /// <param name="connectionString">数据库连接字符串</param>
        /// <param name="dataBase">所使用的数据库，可以为sqlite、postgresql、oracle、sqlserver、mysql、db2、sybase</param>
        /// <param name="logPath">日志记录地址</param>
        /// <param name="paramPrefix">参数前缀</param>
        /// <param name="sqlPrefix">sql前缀</param>
        /// <param name="isLogSql">是否记录sql</param>
        /// <param name="isTransaction">是否开启事物</param>
        public DbHelper(string providerName, string connectionString, string dataBase, string logPath, string paramPrefix = "@", string sqlPrefix = "@", bool isLogSql = false, bool isTransaction = true, IsolationLevel isolationLevel = IsolationLevel.ReadCommitted, string nameMode = "None")
        {
            this.DataBase = dataBase;
            this.ConnectionString = connectionString;
            this.ProviderName = providerName;
            this.IsTransaction = isTransaction;
            this.LogPath = logPath;
            this.ParamPrefix = paramPrefix;
            this.SqlPrefix = sqlPrefix;
            this.IsLogSql = isLogSql;
            this.IsolationLevel = isolationLevel;
            this.NameMode = nameMode;
            this.Guid = Guid.NewGuid();
        }
        /// <summary>
        /// 通过DbSetting对象构造DbHelper
        /// </summary>
        /// <param name="dbSetting"></param>
        public DbHelper(DbSetting dbSetting)
        {
            this.DataBase = dbSetting.DataBase;
            this.ConnectionString = dbSetting.ConnectionString;
            this.ProviderName = dbSetting.ProviderName;
            this.IsTransaction = dbSetting.IsTransaction;
            this.LogPath = dbSetting.LogPath;
            this.ParamPrefix = dbSetting.ParamPrefix;
            this.SqlPrefix = dbSetting.SqlPrefix;
            this.IsLogSql = dbSetting.IsLogSql;
            this.IsolationLevel = dbSetting.IsolationLevel;
            this.NameMode = dbSetting.NameMode;
            this.Guid = Guid.NewGuid();
        }
        #endregion

        #region 静态公共属性

        /// <summary>
        /// 获得一个新的唯一Id值（可统一取得一个唯一的无符号长整数值）；赋值可以设置起始值(谨慎使用)
        /// </summary>
        /// <returns>唯一无符号长整数值</returns>
        public static long NewLongId()
        {
            return Snowflake.Instance().GetId();
        }

        /// <summary>
        /// 获得一个新的唯一字符串Id值（decimal 62进制）；赋值可以设置起始值(谨慎使用)
        /// </summary>
        /// <returns>唯一字符串值</returns>
        public static string NewStringId()
        {
            return DbHelper.Decimal2Str(Snowflake.Instance().GetId());
        }

        public void Commit()
        {
            Db db = threadLocalDb.Value[this];
            if (db != null)
            {
                db.Commit();
            }
        }

        public void Rollback()
        {
            Db db = threadLocalDb.Value[this];
            if (db != null)
            {
                db.Rollback();
            }
        }

        public void BeginTransaction(IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
        {
            Db db = threadLocalDb.Value[this];
            if (db != null)
            {
                db.IsTransaction = true;
                db.IsolationLevel = isolationLevel;
            }
        }

        #endregion

        #region 接口方法

        public void AddInputParameter(string parameterName, object value)
        {
            this.Db.AddInputParameter(parameterName, value);
        }

        public void AddInputParameter(IDictionary<string, object> dic)
        {
            this.Db.AddInputParameter(dic);
        }

        public void AddOutputParameter(string parameterName, object value)
        {
            this.Db.AddOutputParameter(parameterName, value);
        }

        public void AddOutputParameter(IDictionary<string, object> dic)
        {
            this.Db.AddOutputParameter(dic);
        }

        public object GetParameterValue(string parameterName)
        {
            return this.Db.GetParameterValue(parameterName);
        }

        public long ExecuteNonQuery(string sql, IDictionary<string, object> dic)
        {
            sql = sql.Replace("@", this.SqlPrefix);
            this.Db.AddInputParameter(dic);
            long result = this.Db.ExecuteNonQuery(sql, CommandType.Text);
            return result;
        }

        public long ExecuteNonQuery(string sql)
        {
            return this.ExecuteNonQuery(sql, null);
        }

        public IList<IDictionary<string, object>> ExecuteQuery(string sql)
        {
            return ExecuteQuery(sql, null);
        }

        public IList<IDictionary<string, object>> ExecuteQuery(string sql, IDictionary<string, object> dic)
        {
            sql = sql.Replace("@", this.SqlPrefix);
            this.Db.AddInputParameter(dic);
            return this.ExecuteToList(sql);
        }

        public DataTable ExecuteQueryDataTable(string sql, IDictionary<string, object> dic)
        {
            sql = sql.Replace("@", this.SqlPrefix);
            this.Db.AddInputParameter(dic);
            return this.ExecuteToTable(sql);
        }

        public object ExecuteScalar(string sql)
        {
            return this.ExecuteScalar(sql, null);
        }

        public object ExecuteScalar(string sql, IDictionary<string, object> dic)
        {
            sql = sql.Replace("@", this.SqlPrefix);
            this.Db.AddInputParameter(dic);
            return this.Db.ExecuteScalar(sql, CommandType.Text);
        }

        public IDictionary<string, object> SelectRow(string sql)
        {
            IList<IDictionary<string, object>> list = this.ExecuteQuery(sql);
            if (list != null && list.Count() > 0)
                return list[0];
            return null;
        }

        public IDictionary<string, object> SelectRow(string tableName, IDictionary<string, object> dic)
        {
            return this.SelectRow(tableName, null, dic);
        }

        public IDictionary<string, object> SelectRow(string tableName, string[] fields, IDictionary<string, object> dic)
        {
            return this.SelectRow(tableName, fields, dic, null, null);
        }

        public IDictionary<string, object> SelectRow(string tableName, IDictionary<string, object> dic, string[] ascFields, string[] descFields)
        {
            return this.SelectRow(tableName, null, dic, ascFields, descFields);
        }

        public IDictionary<string, object> SelectRow(string tableName, string[] fields, IDictionary<string, object> dic, string[] ascFields, string[] descFields)
        {
            IDictionary<string, object> row = null;
            this.Db.AddInputParameter(dic);
            string sql = this.BuildSql(ExecuteTypeEnum.Select, tableName, fields, ascFields, descFields, 1, 0);
            using (IDataReader reader = this.Db.ExecuteReader(sql, CommandType.Text))
            {
                if (reader != null)
                {
                    while (reader.Read())
                    {
                        row = new Dictionary<string, object>();
                        int count = reader.FieldCount;
                        for (int i = 0; i < count; i++)
                        {
                            row.Add(reader.GetName(i), reader.GetValue(i));
                        }
                        break;
                    }
                }
            }
            return row;
        }

        public bool Exists(string tableName, IDictionary<string, object> dic)
        {
            IDictionary<string, object> result = this.SelectRow(tableName, dic);
            if (result == null)
                return false;
            else
                return true;
        }

        public bool Exists(string sql)
        {
            IDictionary<string, object> result = this.SelectRow(sql);
            if (result == null)
                return false;
            else
                return true;
        }

        public DataTable SelectTable(string tableName)
        {
            return this.SelectTable(tableName, null, null);
        }

        public DataTable SelectTable(string tableName, IDictionary<string, object> dic)
        {
            return this.SelectTable(tableName, dic, null, null);
        }

        public DataTable SelectTable(string tableName, IDictionary<string, object> dic, string[] ascFields, string[] descFields)
        {
            return this.SelectTable(tableName, dic, ascFields, descFields, 0, 0);
        }

        public DataTable SelectTable(string tableName, IDictionary<string, object> dic, string[] ascFields, string[] descFields, long limit, long offset)
        {
            return this.SelectTable(tableName, null, dic, ascFields, descFields, limit, offset);
        }

        public DataTable SelectTable(string tableName, IDictionary<string, object> dic, string[] ascFields, string[] descFields, long limit)
        {
            return this.SelectTable(tableName, null, dic, ascFields, descFields, limit, 0);
        }

        public DataTable SelectTable(string tableName, string[] fields)
        {
            return this.SelectTable(tableName, fields, null);
        }

        public DataTable SelectTable(string tableName, string[] fields, IDictionary<string, object> dic)
        {
            return this.SelectTable(tableName, fields, dic, null, null);
        }

        public DataTable SelectTable(string tableName, string[] fields, IDictionary<string, object> dic, string[] ascFields, string[] descFields)
        {
            return this.SelectTable(tableName, fields, dic, ascFields, descFields, 0, 0);
        }

        public DataTable SelectTable(string tableName, string[] fields, IDictionary<string, object> dic, string[] ascFields, string[] descFields, long limit)
        {
            return this.SelectTable(tableName, fields, dic, ascFields, descFields, limit, 0);
        }

        public DataTable SelectTable(string tableName, string[] fields, IDictionary<string, object> dic, string[] ascFields, string[] descFields, long limit, long offset)
        {
            this.Db.AddInputParameter(dic);
            string sql = this.BuildSql(ExecuteTypeEnum.Select, tableName, fields, ascFields, descFields, limit, offset);
            return this.ExecuteToTable(sql);
        }

        public IList<IDictionary<string, object>> Select(string tableName)
        {
            return this.Select(tableName, null, null);
        }

        public IList<IDictionary<string, object>> Select(string tableName, IDictionary<string, object> dic)
        {
            return this.Select(tableName, dic, null, null);
        }

        public IList<IDictionary<string, object>> Select(string tableName, IDictionary<string, object> dic, string[] ascFields, string[] descFields)
        {
            return this.Select(tableName, dic, ascFields, descFields, 0, 0);
        }

        public IList<IDictionary<string, object>> Select(string tableName, IDictionary<string, object> dic, string[] ascFields, string[] descFields, long limit)
        {
            return this.Select(tableName, null, dic, ascFields, descFields, limit, 0);
        }

        public IList<IDictionary<string, object>> Select(string tableName, IDictionary<string, object> dic, string[] ascFields, string[] descFields, long limit, long offset)
        {
            return this.Select(tableName, null, dic, ascFields, descFields, limit, offset);
        }

        public IList<IDictionary<string, object>> Select(string tableName, string[] fields)
        {
            return this.Select(tableName, fields, null);
        }

        public IList<IDictionary<string, object>> Select(string tableName, string[] fields, IDictionary<string, object> dic)
        {
            return this.Select(tableName, fields, dic, null, null);
        }

        public IList<IDictionary<string, object>> Select(string tableName, string[] fields, IDictionary<string, object> dic, string[] ascFields, string[] descFields)
        {
            return this.Select(tableName, fields, dic, ascFields, descFields, 0, 0);
        }

        public IList<IDictionary<string, object>> Select(string tableName, string[] fields, IDictionary<string, object> dic, string[] ascFields, string[] descFields, long limit)
        {
            return this.Select(tableName, fields, dic, ascFields, descFields, limit, 0);
        }

        public IList<IDictionary<string, object>> Select(string tableName, string[] fields, IDictionary<string, object> dic, string[] ascFields, string[] descFields, long limit, long offset)
        {
            this.Db.AddInputParameter(dic);
            string sql = this.BuildSql(ExecuteTypeEnum.Select, tableName, fields, ascFields, descFields, limit, offset);
            return this.ExecuteToList(sql);
        }

        public long Insert(string tableName, IDictionary<string, object> dic)
        {
            this.Db.AddInputParameter(dic);
            string sql = this.BuildSql(ExecuteTypeEnum.Insert, tableName, null, null, null, 0, 0);
            return this.ExecuteNonQuery(sql);
        }


        public long InsertNotExists(string tableName, IDictionary<string, object> dic, string[] uniqueFields)
        {
            this.Db.AddInputParameter(dic);
            string sql = this.BuildSql(ExecuteTypeEnum.InsertNotExists, tableName, uniqueFields, null, null, 0, 0);
            return this.ExecuteNonQuery(sql);
        }

        public long Update(string tableName, string[] keys, IDictionary<string, object> dic)
        {
            long result = 0;
            this.Db.AddInputParameter(dic);
            string sql = this.BuildSql(ExecuteTypeEnum.Update, tableName, keys, null, null, 0, 0);
            result = this.ExecuteNonQuery(sql);
            return result;
        }

        public long Delete(string tableName, IDictionary<string, object> dic)
        {
            this.Db.AddInputParameter(dic);
            string sql = this.BuildSql(ExecuteTypeEnum.Delete, tableName, null, null, null, 0, 0);
            long result = this.ExecuteNonQuery(sql);
            return result;
        }

        public long Count(string tableName, IDictionary<string, object> dic)
        {
            long result;
            this.Db.AddInputParameter(dic);
            string sql = this.BuildSql(ExecuteTypeEnum.Count, tableName, null, null, null, 0, 0);
            long.TryParse(this.ExecuteScalar(sql).ToString(), out result);
            return result;
        }

        public long Count(string tableName)
        {
            return this.Count(tableName, null);
        }

        public void ProcedureNonQuery(string procedureName)
        {
            this.ProcedureNonQuery(procedureName, null);
        }

        public void ProcedureNonQuery(string procedureName, IDictionary<string, object> dic)
        {
            this.ProcedureNonQuery(procedureName, dic, null);
        }

        public void ProcedureNonQuery(string procedureName, IDictionary<string, object> dic, IDictionary<string, object> outputParams)
        {
            Db db = this.Db;
            db.AddInputParameter(dic);
            db.AddOutputParameter(outputParams);
            db.ExecuteNonQuery(procedureName, CommandType.StoredProcedure);
        }

        public IList<IList<IDictionary<string, object>>> ProcedureList(string procedureName)
        {
            return this.ProcedureList(procedureName, null);
        }

        public IList<IList<IDictionary<string, object>>> ProcedureList(string procedureName, IDictionary<string, object> dic)
        {
            return this.ProcedureList(procedureName, dic, null);
        }

        public IList<IList<IDictionary<string, object>>> ProcedureList(string procedureName, IDictionary<string, object> dic, IDictionary<string, object> outputParams)
        {
            Db db = this.Db;
            db.AddInputParameter(dic);
            db.AddOutputParameter(outputParams);
            IList<IList<IDictionary<string, object>>> result = null;
            using (IDataReader reader = db.ExecuteReader(procedureName, CommandType.StoredProcedure))
            {
                if (reader != null)
                {
                    result = new List<IList<IDictionary<string, object>>>();
                    do
                    {
                        IList<IDictionary<string, object>> list = this.DataReaderToList(reader);
                        result.Add(list);
                    } while (reader.NextResult());
                }
            }
            return result;
        }

        public IList<DataTable> ProcedureDataTable(string procedureName)
        {
            return this.ProcedureDataTable(procedureName, null);
        }

        public IList<DataTable> ProcedureDataTable(string procedureName, IDictionary<string, object> dic)
        {
            return this.ProcedureDataTable(procedureName, dic, null);
        }

        public IList<DataTable> ProcedureDataTable(string procedureName, IDictionary<string, object> dic, IDictionary<string, object> outputParams)
        {
            Db db = this.Db;
            db.AddInputParameter(dic);
            db.AddOutputParameter(outputParams);
            IList<DataTable> result = new List<DataTable>();
            using (IDataReader reader = db.ExecuteReader(procedureName, CommandType.StoredProcedure))
            {
                if (reader != null)
                {
                    do
                    {
                        DataTable dTable = new DataTable();
                        dTable.Load(reader);
                        this.ChangeColumnName(dTable);
                        result.Add(dTable);
                    } while (reader.NextResult());
                }
            }
            return result;
        }

        public long GetCurrentSequence(string name = "")
        {
            long result;
            object id = this.GetCurrentSequenceValue(name);
            long.TryParse(id.ToString(), out result);
            return result;
        }

        public long GetNextSequence(string name)
        {
            long result;
            object id = this.GetNextSequenceValue(name);
            long.TryParse(id.ToString(), out result);
            return result;
        }

        public void Dispose()
        {
            this.Db.Dispose();
        }

        #endregion

        #region 私有方法

        private Db GetCurrentDb()
        {
            if (threadLocalDb.Value == null)
                threadLocalDb.Value = new Dictionary<IDbHelper, Db>();
            if (!threadLocalDb.Value.ContainsKey(this) || threadLocalDb.Value[this].Disposed)
                threadLocalDb.Value[this] = this.CreateDb();
            return threadLocalDb.Value[this];
        }

        private Db CreateDb()
        {

            Db db = new Db(this.ProviderName, this.ConnectionString, this.DataBase, this.ParamPrefix, this.SqlPrefix, this.IsLogSql, this.LogPath, this.IsTransaction, this.IsolationLevel);
            threadLocalDb.Value[this] = db;
            return db;
        }

        private IList<IDictionary<string, object>> DataReaderToList(IDataReader reader)
        {
            IList<IDictionary<string, object>> list = new List<IDictionary<string, object>>();
            while (reader.Read())
            {
                IDictionary<string, object> row = new Dictionary<string, object>();
                int count = reader.FieldCount;
                for (int i = 0; i < count; i++)
                {
                    string fieldName = reader.GetName(i);
                    fieldName = this.FieldNameModeConvert(fieldName);
                    row.Add(fieldName, reader.GetValue(i));
                }
                list.Add(row);
            }
            return list;
        }

        private string FieldNameModeConvert(string fieldName)
        {
            if (this.NameMode == "hump")
                fieldName = this.LineToHumpFirstLower(fieldName);
            if (this.NameMode == "Hump")
                fieldName = this.LineToHump(fieldName);
            if (this.NameMode == "snakelike")
                fieldName = this.HumpToLine(fieldName);
            return fieldName;
        }

        private object GetNextSequenceValue(string name = "")
        {
            object id = 0;
            string sql = string.Empty;
            switch (this.DataBase.ToLower())
            {
                case "postgresql":
                    //这里sequence必须以postgresql数据库自动创建sequence的命名规则，sequence名的规则是“表名+_SERIAL字段_seq"
                    sql = "SELECT NEXTVAL('" + name + "')";
                    break;
                case "oracle":
                    //sequence名的规则是“表名+_SERIAL字段_seq"
                    sql = "SELECT " + name + ".NEXTVAL FROM DUAL";
                    break;
                case "db2":
                    //sequence名的规则是“表名+_SERIAL字段_seq"
                    sql = "SELECT NEXTVAL FOR " + name + " FROM sysibm.sysdummy1";
                    break;
                case "sybase":
                    //sequence名的规则是“表名+_SERIAL字段_seq"
                    sql = "SELECT NEXT_IDENTITY('" + name + "')";
                    break;
                case "sqlserver":
                    //sequence名的规则是“表名+_SERIAL字段_seq"
                    sql = "SELECT IDENT_CURRENT('" + name + "') + IDENT_INCR('" + name + "')";
                    break;
                case "mysql":
                    //取自增字段值
                    sql = "SELECT AUTO_INCREMENT+1 FROM information_schema.TABLES WHERE (TABLE_NAME = '" + name + "')";
                    break;
                case "sqlite":
                    //取自增字段值
                    sql = "SELECT LAST_INSERT_ROWID()+1";
                    break;
            }
            if (!string.IsNullOrWhiteSpace(sql))
                id = this.ExecuteScalar(sql);
            return id;
        }

        //取当前自增序列值
        private object GetCurrentSequenceValue(string name = "")
        {
            object id = 0;
            string sql = string.Empty;
            switch (this.DataBase.ToLower())
            {
                case "sqlite":
                    //取自增Id值
                    sql = "SELECT LAST_INSERT_ROWID()";
                    break;
                case "mysql":
                    //取自增Id值
                    //sql="SELECT LAST_INSERT_ID()";
                    sql = "SELECT AUTO_INCREMENT FROM information_schema.TABLES WHERE (TABLE_NAME = '" + name + "')";
                    break;
                case "oracle":
                    //sequence名的规则是“表名+_SERIAL字段_seq"
                    sql = "SELECT " + name + ".currval FROM DUAL";
                    break;
                case "postgresql":
                    //这里sequence必须以postgresql数据库自动创建sequence的命名规则，sequence名的规则是“表名+_SERIAL字段_seq"
                    sql = "SELECT CURRVAL('" + name + "')";
                    break;
                case "db2":
                    sql = "SELECT CURRVAL FOR " + name + " FROM sysibm.sysdummy1";
                    break;
                default://sqlserver、sybase
                    //取自增Id值
                    //sql = "SELECT @@IDENTITY";
                    sql = "SELECT IDENT_CURRENT('" + name + "')";
                    break;
            }
            if (!string.IsNullOrWhiteSpace(sql))
                id = this.ExecuteScalar(sql);
            return id;
        }

        //执行sql语句返回字典集合
        private IList<IDictionary<string, object>> ExecuteToList(string sql)
        {
            IList<IDictionary<string, object>> list = null;
            using (IDataReader reader = this.Db.ExecuteReader(sql, CommandType.Text))
            {
                if (reader != null)
                {
                    list = this.DataReaderToList(reader);
                }
            }
            return list;
        }

        //执行sql语句返回DataTable
        private DataTable ExecuteToTable(string sql)
        {
            DataTable dTable = null;
            using (IDataReader reader = this.Db.ExecuteReader(sql, CommandType.Text))
            {
                if (reader != null)
                {
                    dTable = new DataTable();
                    dTable.Load(reader);
                }
            }
            this.ChangeColumnName(dTable);
            return dTable;
        }

        /// <summary>
        /// 构建sql语句
        /// </summary>
        /// <param name="executeType">sql语句类型</param>
        /// <param name="tableName">表或视图名</param>
        /// <param name="keys">select时为查询字段，update时为修改条件字段</param>
        /// <param name="ascFields">查询正序字段</param>
        /// <param name="descFields">查询反序字段</param>
        /// <param name="limit">查询记录数</param>
        /// <param name="offset">查询取数起始记录数</param>
        /// <returns></returns>
        private string BuildSql(ExecuteTypeEnum executeType, string tableName, string[] keys, string[] ascFields, string[] descFields, long limit, long offset)
        {
            string sql = string.Empty;
            switch (executeType)
            {
                case ExecuteTypeEnum.Select:
                    sql = this.BuildSelect(tableName, keys, ascFields, descFields, limit, offset);
                    break;
                case ExecuteTypeEnum.Count:
                    sql = this.BuildCount(tableName);
                    break;
                case ExecuteTypeEnum.Insert:
                    sql = this.BuildInsert(tableName);
                    break;
                case ExecuteTypeEnum.InsertNotExists:
                    sql = this.BuildInsertNotExists(tableName, keys);
                    break;
                case ExecuteTypeEnum.Update:
                    sql = this.BuildUpdate(tableName, keys);
                    break;
                case ExecuteTypeEnum.Delete:
                    sql = this.BuildDelete(tableName);
                    break;
            }
            return sql;
        }

        private string BuildDelete(string tableName)
        {
            StringBuilder sql = new StringBuilder();
            sql.AppendFormat("DELETE FROM {0} WHERE 1=1", tableName);
            foreach (var param in this.Db.GetCommandParameters())
            {
                string fieldName = this.GetFieldName(param.Key);
                string paramName = this.SqlPrefix + fieldName;
                sql.AppendFormat(" AND {0}={1}", fieldName, paramName);
            }
            return sql.ToString();
        }

        private string BuildUpdate(string tableName, string[] keys)
        {
            StringBuilder sql = new StringBuilder();
            StringBuilder temp = new StringBuilder();
            StringBuilder fieldTemp = new StringBuilder();
            sql.AppendFormat("UPDATE {0} ", tableName);
            sql.Append("SET ");
            foreach (var param in this.Db.GetCommandParameters())
            {
                string fieldName = this.GetFieldName(param.Key);
                string paramName = this.SqlPrefix + fieldName;
                if (keys != null && keys.Contains(fieldName))
                    continue;
                if (fieldTemp.Length > 0)
                    fieldTemp.Append(",");
                fieldTemp.AppendFormat(" {0}={1}", fieldName, paramName);
            }
            sql.Append(fieldTemp);
            sql.Append(" WHERE 1=1");
            if (keys != null)
            {
                foreach (var key in keys)
                {
                    temp.AppendFormat(" AND {0}={1}{0}", key, this.SqlPrefix);
                }
            }
            sql.Append(temp);
            return sql.ToString();
        }

        private string BuildInsertNotExists(string tableName, string[] uniqueFields)
        {
            StringBuilder sql = new StringBuilder();
            StringBuilder temp = new StringBuilder();
            StringBuilder tempWhere = new StringBuilder();
            StringBuilder fieldTemp = new StringBuilder();
            sql.AppendFormat("INSERT INTO {0} ", tableName);
            sql.Append("(");
            foreach (var param in this.Db.GetCommandParameters())
            {
                String fieldName = param.Key;
                String paramName = this.SqlPrefix + fieldName;
                if (fieldTemp.Length > 0)
                {
                    fieldTemp.Append(",");
                }
                fieldTemp.AppendFormat("{0}", fieldName);
                if (temp.Length > 0)
                {
                    temp.Append(",");
                }
                temp.AppendFormat("{0}", paramName);
            }
            sql.Append(fieldTemp);
            sql.Append(")SELECT ");
            sql.Append(temp);
            if (this.DataBase.ToLower() == "oracle" || this.DataBase.ToLower() == "mysql")
                sql.Append(" FROM dual");
            sql.AppendFormat(" WHERE NOT EXISTS(SELECT 1 FROM {0} WHERE 1=1", tableName);
            if (uniqueFields != null)
            {
                foreach (string key in uniqueFields)
                {
                    tempWhere.AppendFormat(" AND {0}={1}{0}", key, this.SqlPrefix);
                }
                sql.Append(tempWhere);
            }
            sql.Append(")");
            return sql.ToString();
        }

        private string BuildInsert(string tableName)
        {
            StringBuilder sql = new StringBuilder();
            StringBuilder temp = new StringBuilder();
            StringBuilder fieldTemp = new StringBuilder();
            sql.AppendFormat("INSERT INTO {0} ", tableName);
            sql.Append("(");
            foreach (var param in this.Db.GetCommandParameters())
            {
                string fieldName = this.GetFieldName(param.Key);
                string paramName = this.SqlPrefix + fieldName;
                if (fieldTemp.Length > 0)
                    fieldTemp.Append(",");
                fieldTemp.AppendFormat("{0}", fieldName);

                if (temp.Length > 0)
                    temp.Append(",");
                temp.AppendFormat("{0}", paramName);
            }
            sql.Append(fieldTemp);
            sql.Append(")VALUES(");
            sql.Append(temp);
            sql.Append(")");
            return sql.ToString();
        }

        private string BuildCount(string tableName)
        {
            StringBuilder sql = new StringBuilder();
            sql.AppendFormat("SELECT COUNT(*) FROM {0} AA WHERE 1=1", tableName);
            foreach (var param in this.Db.GetCommandParameters())
            {
                string fieldName = this.GetFieldName(param.Key);
                string paramName = this.SqlPrefix + fieldName;
                sql.AppendFormat(" AND {0}={1}", fieldName, paramName);
            }
            return sql.ToString();
        }

        private string BuildSelect(string tableName, string[] fields, string[] ascFields, string[] descFields, long limit, long offset)
        {
            StringBuilder sql = new StringBuilder();
            StringBuilder condition = new StringBuilder();
            StringBuilder orderBy = new StringBuilder();
            string selectFields = "*";
            string ascString = string.Empty;
            string descString = string.Empty;
            if (ascFields != null && ascFields.Length > 0)
            {
                ascString = string.Join(",", ascFields);
            }
            if (descFields != null && descFields.Length > 0)
            {
                descString = string.Join(",", descFields);
            }
            if (fields != null && fields.Count() > 0)
                selectFields = string.Join(",", fields);
            foreach (var param in this.Db.GetCommandParameters())
            {
                string fieldName = this.GetFieldName(param.Key);
                string paramName = this.SqlPrefix + fieldName;
                condition.AppendFormat(" AND {0}={1}", fieldName, paramName);
            }
            if ((ascFields != null && ascFields.Length > 0) || (descFields != null && descFields.Length > 0))
            {
                orderBy.Append(" ORDER BY");
                if (ascFields != null && ascFields.Length > 0)
                {
                    orderBy.AppendFormat(" {0} ASC", ascString);
                }
                if (descFields != null && descFields.Length > 0)
                {
                    if (ascFields != null && ascFields.Length > 0)
                    {
                        orderBy.Append(",");
                    }
                    orderBy.AppendFormat(" {0} DESC", descString);
                }
            }

            if (limit > 0)
            {
                switch (this.DataBase.ToLower())
                {
                    case "oracle":
                        if (orderBy.Length == 0)
                            orderBy.AppendFormat("ORDER BY {0}", "null");
                        sql.AppendFormat("SELECT {0} FROM (SELECT AA.*,ROW_NUMBER() OVER({1}) AS ROW_INDEX_NUMBER FROM {2} AA WHERE 1=1 {3}) TT WHERE TT.ROW_INDEX_NUMBER BETWEEN {4} AND {5}", selectFields, orderBy, tableName, condition, offset, offset + limit);
                        break;
                    case "db2":
                        if (orderBy.Length == 0)
                            orderBy.Append("");//db2运行ROW_NUMBER() over()
                        sql.AppendFormat("SELECT {0} FROM (SELECT AA.*,ROW_NUMBER() OVER({1}) AS ROW_INDEX_NUMBER FROM {2} AA WHERE 1=1 {3}) TT WHERE TT.ROW_INDEX_NUMBER > {4} AND TT.ROW_INDEX_NUMBER <= {5}", selectFields, orderBy, tableName, condition, offset, offset + limit);
                        break;
                    case "sqlserver":
                        if (orderBy.Length == 0)
                            orderBy.AppendFormat("ORDER BY {0}", "newid()");
                        sql.AppendFormat("SELECT {0} FROM (SELECT AA.*,ROW_NUMBER() OVER({1}) AS ROW_INDEX_NUMBER FROM {2} AA WHERE 1=1 {3}) TT WHERE TT.ROW_INDEX_NUMBER BETWEEN {4} AND {5}", selectFields, orderBy, tableName, condition, offset, offset + limit);
                        break;
                    case "sybase":
                        if (orderBy.Length == 0)
                            orderBy.AppendFormat("ORDER BY {0}", "newid()");
                        sql.AppendFormat("SELECT {0} FROM (SELECT AA.*,RANK() OVER({1}) AS ROW_INDEX_NUMBER FROM {2} AA WHERE 1=1 {3}) TT WHERE TT.ROW_INDEX_NUMBER > {4} AND TT.ROW_INDEX_NUMBER <= {5}", selectFields, orderBy, tableName, condition, offset, offset + limit);
                        break;
                    default:
                        sql.AppendFormat("SELECT {0} FROM {1} AA WHERE 1=1 {2} {3}", selectFields, tableName, condition, orderBy);
                        sql.AppendFormat(" LIMIT {0} OFFSET {1}", limit, offset);
                        break;
                }
            }
            else
            {
                sql.AppendFormat("SELECT {0} FROM {1} AA WHERE 1=1 {2} {3}", selectFields, tableName, condition, orderBy);
            }
            return sql.ToString();
        }

        //根据使用数据库类型初始化前缀
        private void InitPrefix()
        {
            switch (this.DataBase.ToLower())
            {
                case "postgresql":
                //postgresql和oracle前缀一样
                case "oracle":
                    this.ParamPrefix = string.Empty;
                    this.SqlPrefix = ":";
                    break;
                case "mysql":
                    this.ParamPrefix = "?";
                    this.SqlPrefix = "?";
                    break;
                default:
                    this.ParamPrefix = "@";
                    this.SqlPrefix = "@";
                    break;
            }
        }

        //根据参数名取得字段名
        private string GetFieldName(string paramName)
        {
            if (!string.IsNullOrEmpty(this.ParamPrefix))
            {
                return paramName.Replace(this.ParamPrefix, "");
            }
            return paramName;
        }

        private string LineToHump(string text)
        {

            string[] strs = text.Split('_');
            StringBuilder result = new StringBuilder();
            foreach (string s in strs)
            {
                result.Append(this.FirstUpper(s));
            }
            return result.ToString();
        }

        private string LineToHumpFirstLower(string text)
        {

            string[] strs = text.Split('_');
            StringBuilder result = new StringBuilder();
            for (var i = 0; i < strs.Length; i++)
            {
                string s = strs[i];
                if (i == 0)
                    result.Append(s);
                else
                    result.Append(this.FirstUpper(s));
            }
            return result.ToString();
        }

        private string HumpToLine(string text)
        {
            if (string.IsNullOrEmpty(text)) return text;
            MatchCollection matches = Regex.Matches(text, @"(?<p>[A-Z])", RegexOptions.Multiline);
            foreach (Match m in matches)
            {
                string value = m.Groups["p"].Value;
                text = text.Replace(value, "_" + this.FirstLower(value));
            }
            if (text.StartsWith("_"))
                text = text.Substring(1);
            return text;
        }

        private string FirstUpper(string str)
        {
            if (string.IsNullOrEmpty(str)) return str;
            return str.First().ToString().ToUpper() + str.Substring(1);
        }

        private string FirstLower(string str)
        {
            if (string.IsNullOrEmpty(str)) return str;
            return str.First().ToString().ToLower() + str.Substring(1);
        }

        private DataTable ChangeColumnName(DataTable dTable)
        {
            if (this.NameMode != "none")
            {
                foreach (DataColumn col in dTable.Columns)
                {
                    string fieldName = col.ColumnName;
                    fieldName = this.FieldNameModeConvert(fieldName);
                    col.ColumnName = fieldName;
                }
            }
            return dTable;
        }
        #endregion

        #region 62进制转换

        private static String keys = "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
        private static int len = keys.Length;
        /// <summary>
        /// ulong value type to 62 string
        /// </summary>
        /// <param name="value">The max value can not more decimal.MaxValue</param>
        /// <returns>Return a specified 62 encode string</returns>
        public static string Decimal2Str(decimal value)//17223472558080896352ul
        {
            string result = string.Empty;
            do
            {
                decimal index = value % len;
                result = keys[(int)index] + result;
                value = (value - index) / len;
            }
            while (value > 0);

            return result;
        }


        /// <summary>
        /// 62 encode string to decimal
        /// </summary>
        /// <param name="value">62 encode string</param>
        /// <returns>Return a specified decimal number that decode by 62 string</returns>
        public static decimal Str2Decimal(string value)//bUI6zOLZTrj
        {
            decimal result = 0;
            for (int i = 0; i < value.Length; i++)
            {
                int x = value.Length - i - 1;
                result += keys.IndexOf(value[i]) * Pow(len, x);// Math.Pow(exponent, x);
            }
            return result;
        }

        private static decimal Pow(int exp, int x)
        {
            decimal value = 1;
            while (x > 0)
            {
                value = value * exp;
                x--;
            }
            return value;
        }
        #endregion

        #region 枚举

        internal enum ExecuteTypeEnum
        {
            Select = 0,
            Insert,
            InsertNotExists,
            Update,
            Delete,
            Count
        }

        #endregion

    }
}
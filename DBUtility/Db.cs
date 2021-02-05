using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace Lever.DBUtility
{
    internal class Db : IDisposable
    {
        private static object fileLock = new object(); //锁对象
        private bool transactionComplete = true;
        internal Db(string providerName, string connectionString, string dataBase, string paramPrefix, string sqlPrefix, bool isLogSql, string logPath, bool isTransaction, IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
        {
            this.DataBase = dataBase;
            this.ParamPrefix = paramPrefix;
            this.SqlPrefix = sqlPrefix;
            this.LogPath = logPath;
            this.IsTransaction = isTransaction;
            this.IsLogSql = isLogSql;
            this.IsolationLevel = isolationLevel;
            this.ConnectionString = connectionString;
            this.ProviderName = providerName;
            this.InitPrefix();
        }

        /// <summary>
        /// 析构函数
        /// </summary>
        ~Db()
        {
            this.Dispose(false);
        }

        public bool Disposed { get; set; } = false;
        private string DataBase { get; set; }
        private string ParamPrefix { get; set; } = "@";
        private string SqlPrefix { get; set; } = "@";

        private SortedDictionary<string, object> TempParams { get; } = new SortedDictionary<string, object>();

        private SortedDictionary<string, object> InputParams { get; } = new SortedDictionary<string, object>();

        private SortedDictionary<string, object> OutputParams { get; } = new SortedDictionary<string, object>();

        private IDbConnection Connection { get; set; }

        private string ConnectionString { get; set; }

        private string ProviderName { get; set; }

        private IDbCommand Command { get; set; }

        private IDbTransaction Transaction { get; set; }

        private bool IsLogSql { get; set; }

        private string LogPath { get; set; }

        internal bool IsTransaction { get; set; }

        internal IsolationLevel IsolationLevel { get; set; }

        internal void AddInputParameter(string parameterName, object value)
        {
            this.InputParams.Add(parameterName, value);
        }

        internal void AddInputParameter(IDictionary<string, object> dic)
        {
            if (dic == null) return;
            foreach (var o in dic)
            {
                this.AddInputParameter(o.Key, o.Value);
            }
        }

        internal void AddOutputParameter(string parameterName, object value)
        {
            this.OutputParams.Add(parameterName, value);
        }

        internal void AddOutputParameter(IDictionary<string, object> dic)
        {
            if (dic == null) return;
            foreach (var pair in dic)
            {
                this.AddOutputParameter(pair.Key, pair.Value);
            }
        }

        internal IDictionary<string, object> GetCommandParameters()
        {
            return this.InputParams.Merge<string, object>(this.OutputParams);
        }

        internal void Commit()
        {
            if (this.Transaction != null && !this.transactionComplete)
            {
                if (this.Transaction.Connection != null)
                {
                    this.Transaction?.Commit();
                    this.transactionComplete = true;
                }

            }
        }

        internal void Rollback()
        {
            if (this.Transaction != null && !this.transactionComplete)
            {
                this.Transaction?.Rollback();
                this.transactionComplete = true;
            }
        }

        internal object GetParameterValue(string parameterName)
        {
            if (this.TempParams.Count > 0)
            {
                if (!this.TempParams.ContainsKey(parameterName))
                    throw new Exception("参数“" + parameterName + "”不存在");
                return this.TempParams[parameterName];
            }
            return null;
        }

        internal long ExecuteNonQuery(string sql, CommandType commandType)
        {
            try
            {
                this.Open();
                long result;
                this.Command.CommandText = sql;
                this.Command.CommandType = commandType;
                this.Command.Prepare();
                result = this.Command.ExecuteNonQuery();
                this.WriteLogSql();//根据IsLogSql配置，true记录sql语句，false不记录
                return result;
            }
            catch (Exception e)
            {
                this.Rollback();
                this.WriteSqlErrorLog(e);
                throw;
            }
            finally
            {
                this.Clear();
            }

        }

        internal IDataReader ExecuteReader(string sql, CommandType commandType)
        {
            try
            {
                this.Open();
                this.Command.CommandText = sql;
                this.Command.CommandType = commandType;
                this.Command.Prepare();
                IDataReader result = this.Command.ExecuteReader();
                this.WriteLogSql();//根据IsLogSql配置，true记录sql语句，false不记录
                return result;
            }
            catch (Exception e)
            {
                this.Rollback();
                this.WriteSqlErrorLog(e);
                throw;
            }
            finally
            {
                this.Clear();
            }
        }

        internal object ExecuteScalar(string sql, CommandType commandType)
        {
            try
            {
                this.Open();
                this.Command.CommandText = sql;
                this.Command.CommandType = commandType;
                this.Command.Prepare();
                object result = this.Command.ExecuteScalar();
                this.WriteLogSql();//根据IsLogSql配置，true记录sql语句，false不记录
                return result;
            }
            catch (Exception e)
            {
                this.Rollback();
                this.WriteSqlErrorLog(e);
                throw;
            }
            finally
            {
                this.Clear();
            }
        }

        //打开数据库连接
        internal void Open()
        {
            this.InitConnection();
            if (this.Connection.State != ConnectionState.Open)
                this.Connection.Open();
            if (this.IsTransaction && this.Transaction == null)
                this.BeginTransaction(this.IsolationLevel);
            this.AddInputCommandParameter(this.InputParams);
            this.AddOutputCommandParameter(this.OutputParams);
        }

        /// <summary>
        /// 写日志
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="content"></param>
        /// <param name="append"></param>
        internal static void WriteLogs(string filePath, string content, bool append)
        {
            new Thread(new ThreadStart(() =>
            {
                lock (fileLock)
                {
                    string dir = Path.GetDirectoryName(filePath);
                    if (!Directory.Exists(dir))
                        Directory.CreateDirectory(dir);
                    using (StreamWriter sw = new StreamWriter(filePath, append, Encoding.Default))
                    {
                        sw.WriteLine(content);
                    }
                }
            })).Start();
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void InitConnection()
        {
            if (this.Connection == null || this.Connection.State == ConnectionState.Closed)
            {
                DbProviderFactory dbProvider = DbProviderFactories.GetFactory(this.ProviderName);
                Connection = dbProvider.CreateConnection();
                Connection.ConnectionString = this.ConnectionString;
                Command = Connection.CreateCommand();
            }
        }

        private void BeginTransaction(IsolationLevel IsolationLevel)
        {
            if (this.IsTransaction && this.Transaction != null)
            {
                this.Transaction?.Dispose();
                this.Transaction = null;
            }
            this.IsTransaction = true;
            this.Transaction = this.Connection.BeginTransaction(IsolationLevel);
            this.Command.Transaction = this.Transaction;
            this.transactionComplete = false;
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

        //清空Command参数
        private void Clear()
        {
            this.InputParams.Clear();
            this.OutputParams.Clear();
            this.TempParams.Clear();//清楚上次执行的参数值
            if (this.Command.Parameters.Count > 0)
            {
                //清空参数前缓存输出参数
                foreach (DbParameter p in this.Command.Parameters)
                {
                    string fieldName = this.GetFieldName(p.ParameterName);
                    this.TempParams[fieldName] = p.Value;
                }
                //清空参数
                this.Command.Parameters.Clear();
            }
        }

        private string GetExecuteSQL(string sql = null)
        {
            sql = sql ?? this.Command.CommandText;
            if (string.IsNullOrWhiteSpace(sql)) return "";
            MatchCollection matches = Regex.Matches(sql, @"(?<p>" + this.SqlPrefix.Replace("?", "\\?") + @"\w+)", RegexOptions.Multiline);
            StringBuilder result = new StringBuilder();
            int offset = 0;
            IDataParameterCollection parameters = this.Command.Parameters;
            foreach (Match m in matches)
            {
                int index = m.Groups["p"].Index;
                int len = m.Groups["p"].Length;
                string paramName = m.Groups["p"].Value;
                paramName = Regex.Replace(paramName, @"^" + this.SqlPrefix.Replace("?", "\\?"), this.ParamPrefix);
                object paramValue = paramName;
                if (parameters.Contains(paramName)&&(IDataParameter)parameters[paramName] != null)
                    paramValue = ((IDataParameter)parameters[paramName]).Value;
                result.Append(sql.Substring(offset, index - offset));
                result.Append(this.GetSqlValue(paramValue));
                offset = index + len;
            }
            result.Append(sql.Substring(offset, sql.Length - offset));
            return result.ToString();
        }

        private string GetSqlValue(object val)
        {
            if (val is string)
            {
                return "'" + val + "'";
            }
            else if (val is DBNull)
            {
                return "null";
            }
            else if (val is DateTime)
            {
                string date = ((DateTime)val).ToString("yyyy-MM-dd HH:mm:ss");
                switch (this.DataBase)
                {
                    case "db2"://db2字符串转日期与oracle一样
                    case "oracle":
                        return "to_date('" + date + "','yyyy-mm-dd,hh24:mi:ss')";
                    case "postgresql":
                        return "to_timestamp('" + date + "','yyyy-mm-dd hh24:mi:ss')";
                    case "mysql":
                        return "str_to_date('" + date + "','%Y-%m-%d %H:%i:%s')";
                    case "sqlite":
                        return "datetime('" + date + "')";
                    case "sqlserver":
                        return "convert(datetime,'" + date + "',120)";
                    case "sybase":
                        return "convert(datetime,'" + date + "')";
                    default:
                        return "'" + val + "'";
                }
            }
            else
            {
                return val.ToString();
            }
        }

        /// <summary>
        /// 根据IsLogSql配置，true记录sql语句，false不记录
        /// </summary>
        private void WriteLogSql()
        {
            if (this.IsLogSql)
            {
                string sql = this.GetExecuteSQL();//构建执行的sql语句
                string path = this.LogPath + "/log_sql/" + DateTime.Now.ToString("yyyyMMdd") + ".txt";
                if (!string.IsNullOrWhiteSpace(sql))
                    WriteLogs(path, string.Format("{0:G}", DateTime.Now) + "\r\n执行的SQL语句：" + sql + "\r\n", true);
            }
        }

        /// <summary>
        /// 写SQL执行异常信息
        /// </summary>
        /// <param name="e"></param>
        private void WriteSqlErrorLog(Exception e)
        {
            string sql = this.GetExecuteSQL();//构建执行的sql语句
            string path = this.LogPath + "/error_sql/" + DateTime.Now.ToString("yyyyMMdd") + ".txt";
            if (string.IsNullOrWhiteSpace(sql))
                WriteLogs(path, string.Format("{0:G}", DateTime.Now) + "\r\n异常信息：" + e.Message + "\r\n堆栈信息：" + e.StackTrace + "\r\n", true);
            else
                WriteLogs(path, string.Format("{0:G}", DateTime.Now) + "\r\n异常信息：" + e.Message + "\r\n堆栈信息：" + e.StackTrace + "\r\n执行的SQL语句：" + sql + "\r\n", true);
        }

        private void Dispose(bool disposing)
        {
            if (!this.Disposed)
            {
                if (disposing)
                {
                    this.Commit();
                    this.Command?.Dispose();
                    this.Transaction?.Dispose();
                    this.Connection?.Close();
                    this.Connection?.Dispose();
                }
                this.Disposed = true;
            }
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
                    this.ParamPrefix = "@";
                    this.SqlPrefix = "@";
                    break;
                default:
                    this.ParamPrefix = "@";
                    this.SqlPrefix = "@";
                    break;
            }
        }

        private void AddInputCommandParameter(string parameterName, object value)
        {
            IDbDataParameter param = this.Command.CreateParameter();
            param.ParameterName = this.ParamPrefix + parameterName;
            param.Direction = ParameterDirection.Input;
            if (value == null)
                value = DBNull.Value;
            this.SetDbType(param, value);
            param.Value = value;
            this.Command.Parameters.Add(param);
        }

        private void AddInputCommandParameter(IDictionary<string, object> dic)
        {
            if (dic == null) return;
            foreach (var o in dic)
            {
                this.AddInputCommandParameter(o.Key, o.Value);
            }
        }

        private void AddOutputCommandParameter(string parameterName, object value)
        {
            IDbDataParameter param = this.Command.CreateParameter();
            param.ParameterName = this.ParamPrefix + parameterName;
            this.SetDbType(param, value);
            param.Direction = ParameterDirection.Output;
            this.Command.Parameters.Add(param);
        }

        private void AddOutputCommandParameter(IDictionary<string, object> dic)
        {
            if (dic == null) return;
            foreach (var pair in dic)
            {
                this.AddOutputCommandParameter(pair.Key, pair.Value);
            }
        }

        private void SetDbType(IDbDataParameter param, object value)
        {
            Type type = value.GetType();
            System.ComponentModel.TypeConverter converter;
            converter = System.ComponentModel.TypeDescriptor.GetConverter(param.DbType);
            try
            {
                if (type != typeof(DBNull))
                {
                    param.DbType = (DbType)converter.ConvertFrom(type.Name);
                    param.Size = Int32.MaxValue;
                }
            }
            catch (Exception e)
            {
                this.WriteSqlErrorLog(e);
                throw;
            }
        }

    }
}

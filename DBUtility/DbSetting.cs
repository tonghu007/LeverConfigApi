using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace Lever.DBUtility
{
    public class DbSetting
    {
        public DbSetting(){}

        public DbSetting(string providerName, string connectionString, string dataBase, string logPath, string paramPrefix = "@", string sqlPrefix = "@", bool isLogSql = false, bool isTransaction = true, IsolationLevel isolationLevel = IsolationLevel.ReadCommitted,string nameMode="None") {
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
        }

        public string ProviderName { get; set; }
        public string ConnectionString { get; set; }
        public string DataBase { get; set; }
        public string LogPath { get; set; }
        public string ParamPrefix { get; set; } = "@";
        public string SqlPrefix { get; set; } = "@";
        public bool IsLogSql { get; set; } = false;
        public bool IsTransaction { get; set; } = true;
        public IsolationLevel IsolationLevel { get; set; } = IsolationLevel.ReadCommitted;
        public string NameMode { get; set; }
    }
}

using System.Data;

namespace Lever.DBUtility
{
    public class MultipleDbSetting : DbSetting
    {
        public MultipleDbSetting() : base() { }

        public MultipleDbSetting(string providerName, string connectionString, string dataBase, string logPath, string paramPrefix = "@", string sqlPrefix = "@", bool isLogSql = false, bool isTransaction = true, IsolationLevel isolationLevel = IsolationLevel.ReadCommitted, bool isDefault = false, string dbName = "DataBase")
            : base(providerName, connectionString, dataBase, logPath, paramPrefix, sqlPrefix, isLogSql, isTransaction, isolationLevel)
        {
            this.IsDefault = isDefault;
            this.DbName = dbName;
        }

        public bool IsDefault { get; set; } = false;

        public string DbName { get; set; } = "DataBase";
    }
}

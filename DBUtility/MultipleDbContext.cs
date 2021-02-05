using System;
using System.Collections.Generic;
using System.Linq;

namespace Lever.DBUtility
{
    public class MultipleDbContext : IDisposable
    {
        private static IDictionary<string, IDbHelper> _dbHelpers = new Dictionary<string, IDbHelper>();
        private List<MultipleDbSetting> _multipleDbSettings;
        public MultipleDbContext(List<MultipleDbSetting> multipleDbSettings)
        {
            this._multipleDbSettings = multipleDbSettings;
        }

        public IDbHelper Use(string dbName)
        {
            KeyValuePair<string, IDbHelper> dic = _dbHelpers.FirstOrDefault((t) => (t.Key.StartsWith("default#") ? t.Key.Substring(t.Key.IndexOf("#") + 1) : t.Key) == dbName);
            if (dic.Value==null)
                return this.CreateDbHelper(dbName);
            return dic.Value;
        }

        public IDbHelper Use()
        {
            KeyValuePair<string, IDbHelper> dic = _dbHelpers.FirstOrDefault((t) => t.Key.StartsWith("default#"));
            if (dic.Value == null)
                return this.CreateDefaultDbHelper();
            return dic.Value;
        }

        public void Dispose()
        {
            foreach (KeyValuePair<string, IDbHelper> pair in _dbHelpers)
            {
                pair.Value?.Dispose();
            }
        }

        private IDbHelper CreateDbHelper(string dbName)
        {
            var multipleDbSetting = this._multipleDbSettings.FirstOrDefault<MultipleDbSetting>(t => t.DbName == dbName);
            if (multipleDbSetting == null)
                throw new Exception($"数据库对象{dbName}不存在");
            IDbHelper db = new DbHelper(multipleDbSetting);
            _dbHelpers[multipleDbSetting.DbName] = db;
            return db;
        }

        private IDbHelper CreateDefaultDbHelper()
        {
            var multipleDbSetting = this._multipleDbSettings.FirstOrDefault<MultipleDbSetting>(t => t.IsDefault == true);
            if (multipleDbSetting == null)
                throw new Exception("默认数据库对象不存在");
            IDbHelper db = new DbHelper(multipleDbSetting);
            _dbHelpers["default#" + multipleDbSetting.DbName] = db;
            return db;
        }
    }
}

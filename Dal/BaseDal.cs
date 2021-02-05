using Lever.DBUtility;
using System;
using System.Text;

namespace Lever.Dal
{
    public class BaseDal : IDisposable
    {
        protected readonly MultipleDbContext _dbContext;
        public BaseDal(MultipleDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public string BuildIn<T>(IDbHelper db, T[] paramData, string fieldName)
        {
            StringBuilder builder = new StringBuilder();
            int index = 0;
            foreach (T param in paramData)
            {
                if (builder.Length > 0)
                    builder.Append(",");
                string paramName = fieldName + index;
                builder.Append("@" + paramName);
                db.AddInputParameter(paramName, param);
                index++;
            }
            return builder.ToString();
        }

        public void Dispose()
        {
            _dbContext.Dispose();
        }
    }
}

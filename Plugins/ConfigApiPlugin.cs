using Microsoft.AspNetCore.Http;
using Lever.DBUtility;
using System.Collections.Generic;

namespace Lever.Plugins
{
    public abstract class ConfigApiPlugin
    {
        public virtual void Before(IDbHelper db, IDictionary<string, object> config, IEnumerable<KeyValuePair<string, object>> parameters, IDictionary<string, IList<IFormFile>> files, object bodyJson)
        {

        }

        public virtual object After(IDbHelper db, IDictionary<string, object> config, IEnumerable<KeyValuePair<string, object>> parameters, object bodyJson, object result)
        {
            return result;
        }
    }
}

using System.Collections.Generic;
using System.Threading;

namespace Lever.Plugins
{
    public abstract class ParamsPlugin
    {
        private readonly static AsyncLocal<IDictionary<string, object>> threadParams = new AsyncLocal<IDictionary<string, object>>();

        public virtual void InitParams() {

        }

        public static void Set(string key, object value)
        {
            ParamsPlugin.Params[key] = value;
        }

        public static object Get(string key)
        {
            return ParamsPlugin.Params[key];
        }

        public static bool ContainsKey(string key)
        {
            return ParamsPlugin.Params.ContainsKey(key);
        }

        private static IDictionary<string, object> Params
        {
            get
            {
                if (threadParams.Value == null)
                    threadParams.Value = new Dictionary<string, object>();
                return threadParams.Value;
            }
        }
    }
}

using System.Collections.Generic;

namespace Lever.IBLL
{
    public interface IDynamicApiBll
    {
        object DynamicFetch(string code);

        object DynamicFetch(string code, IDictionary<string, object> inputParameters);
    }
}

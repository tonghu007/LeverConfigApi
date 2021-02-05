using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace Lever.IBLL
{
    public interface IConfigBll
    {
        IDictionary<string, object> ApiConfigPage(int pageIndex, int pageSize, IDictionary<string, object> searchParams);

        IDictionary<string, object> GetApiRow(string apiIdCode);

        IDictionary<string, object> SaveApi(IDictionary<string, object> parameters);

        void DeleteApi(string apiIdCode);

        void BatchDeleteApi(JArray jArray);

        IDictionary<string, object> ParamsPage(string apiIdCode, int pageIndex, int pageSize);

        IDictionary<string, object> GetParamRow(long paramId);

        IDictionary<string, object> SaveParam(IDictionary<string, object> parameters);

        void DeleteParam(long paramId);

        void BatchDeleteParam(JArray jArray);

        IDictionary<string, object> QueryApiConfigPage(int pageIndex, int pageSize, int apiKind, IDictionary<string, object> searhParams);

        IList<IDictionary<string, object>> GetApiParaqms(string apiIdCode, params int[] paramsKind);

        IDictionary<string, object> GroupPage(int pageIndex, int pageSize, IDictionary<string, object> searhParams);

        void DeleteGroup(string groupId);

        void BatchDeleteGroup(JArray jArray);

        IDictionary<string, object> SaveGroup(IDictionary<string, object> parameters);

        IDictionary<string, object> GetGroupRow(long groupId);

        IList<IDictionary<string, object>> GetAllGroup();
    }
}

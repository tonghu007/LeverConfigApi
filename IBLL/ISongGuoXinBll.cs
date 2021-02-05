using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace Lever.IBLL
{
    public interface ILeverXinBll
    {
        IDictionary<string, object> GetEnterprisePage(int pageIndex, int pageSize, IDictionary<string, object> searhParams);

        void DeleteEnterprise(ulong enterpriseId);

        void BatchDeleteEnterprise(JArray jArray);

        IDictionary<string, object> SaveEnterprise(IDictionary<string, object> parameters);

        IDictionary<string, object> GetEnterprise(ulong enterpriseId);

        IDictionary<string, object> GetProjectPage(int pageIndex, int pageSize, IDictionary<string, object> searhParams);

        void DeleteProject(ulong projectId);

        void BatchDeleteProject(JArray jArray);

        IDictionary<string, object> SaveProject(IDictionary<string, object> parameters);

        IDictionary<string, object> GetProject(ulong projectId);

        IDictionary<string, object> GetPracticeModePage(ulong parentId, int pageIndex, int pageSize, IDictionary<string, object> searhParams);

        void DeletePracticeMode(ulong practiceModeId);

        void BatchDeletePracticeMode(JArray jArray);

        IDictionary<string, object> SavePracticeMode(IDictionary<string, object> parameters);

        IDictionary<string, object> GetPracticeMode(ulong practiceModeId);

        IList<IDictionary<string, object>> GetAllPracticeMode();

        IDictionary<string, object> GetGuideMusicPage(int pageIndex, int pageSize, IDictionary<string, object> searhParams);

        void DeleteGuideMusic(ulong guideMusicId);

        void BatchDeleteGuideMusic(JArray jArray);

        IDictionary<string, object> SaveGuideMusic(IDictionary<string, object> parameters);

        IDictionary<string, object> GetGuideMusic(ulong guideMusicId);

        IDictionary<string, object> SaveProjectEnterprise(IDictionary<string, object> parameters);

        IDictionary<string, object> GetProjectEnterprise(ulong projectEnterpriseId);

        IList<IDictionary<string, object>> GetAllEnterprise(ulong projectId,ulong projectEnterpriseId);

        IDictionary<string, object> GetProjectEnterprisePage(ulong projectId, int pageIndex, int pageSize, IDictionary<string, object> searhParams);

        void DeleteProjectEnterprise(ulong projectEnterpriseId);

        void BatchDeleteProjectEnterprise(JArray jArray);

    }
}

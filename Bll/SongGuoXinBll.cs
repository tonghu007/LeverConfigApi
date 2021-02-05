using Newtonsoft.Json.Linq;
using Lever.Common;
using Lever.Dal;
using Lever.IBLL;
using System;
using System.Collections.Generic;
using System.Text;

namespace Lever.Bll
{
    public class LeverXinBll : ILeverXinBll
    {
        private readonly LeverXinDal _dal;
        public LeverXinBll(LeverXinDal dal)
        {
            _dal = dal;
        }

        public IDictionary<string, object> GetEnterprisePage(int pageIndex, int pageSize, IDictionary<string, object> searhParams)
        {
            using (_dal)
            {
                return _dal.GetEnterprisePage(pageIndex, pageSize, searhParams);
            }
        }

        public void BatchDeleteEnterprise(JArray jArray)
        {
            using (_dal)
            {
                foreach (var json in jArray)
                {
                    JObject jObject = json.ToObject<JObject>();
                    _dal.DeleteEnterprise(jObject.Value<ulong>("id"));
                }
            }
        }

        public void DeleteEnterprise(ulong enterpriseId)
        {
            using (_dal)
            {
                _dal.DeleteEnterprise(enterpriseId);
            }
        }

        public IDictionary<string, object> SaveEnterprise(IDictionary<string, object> parameters)
        {
            using (_dal)
            {
                ulong id = ulong.Parse(parameters["id"].ToString());
                if (id == 0)
                {
                    return _dal.InsertEnterprise(parameters);
                }
                else
                {
                    IDictionary<string, object> enterprise = _dal.GetEnterprise(id);
                    return _dal.UpdateEnterprise(parameters);
                }
            }
        }

        public IDictionary<string, object> GetEnterprise(ulong enterpriseId)
        {
            using (_dal)
            {
                return _dal.GetEnterprise(enterpriseId);
            }
        }


        public IDictionary<string, object> GetProjectPage(int pageIndex, int pageSize, IDictionary<string, object> searhParams)
        {
            using (_dal)
            {
                return _dal.GetProjectPage(pageIndex, pageSize, searhParams);
            }
        }

        public void BatchDeleteProject(JArray jArray)
        {
            using (_dal)
            {
                foreach (var json in jArray)
                {
                    JObject jObject = json.ToObject<JObject>();
                    _dal.DeleteProject(jObject.Value<ulong>("id"));
                }
            }
        }

        public void DeleteProject(ulong projectId)
        {
            using (_dal)
            {
                _dal.DeleteProject(projectId);
            }
        }

        public IDictionary<string, object> SaveProject(IDictionary<string, object> parameters)
        {
            using (_dal)
            {
                ulong id = ulong.Parse(parameters["id"].ToString());
                int status = int.Parse(parameters["status"].ToString());
                //如果当前操作的项目是激活状态，将其他项目都改为未激活状态
                if (status == 1)
                    _dal.UpdateProjectStatus(0);
                if (id == 0)
                {
                    return _dal.InsertProject(parameters);
                }
                else
                {
                    IDictionary<string, object> project = _dal.GetProject(id);
                    if (project != null)
                    {
                        decimal dailyLimit = 0;
                        decimal.TryParse(parameters["daily_limit"].ToString(), out dailyLimit);
                        decimal sameDayAmount = 0;
                        decimal.TryParse(project["same_day_amount"].ToString(), out sameDayAmount);
                        if (dailyLimit < sameDayAmount)
                            throw new CustomException(11, "每日限额不能小于今日获赠金额");
                    }
                    return _dal.UpdateProject(parameters);
                }
            }
        }

        public IDictionary<string, object> GetProject(ulong projectId)
        {
            using (_dal)
            {
                return _dal.GetProject(projectId);
            }
        }

        public IDictionary<string, object> GetPracticeModePage(ulong parentId, int pageIndex, int pageSize, IDictionary<string, object> searhParams)
        {
            using (_dal)
            {
                return _dal.GetPracticeModePage(parentId, pageIndex, pageSize, searhParams);
            }
        }

        public void BatchDeletePracticeMode(JArray jArray)
        {
            using (_dal)
            {
                foreach (var json in jArray)
                {
                    JObject jObject = json.ToObject<JObject>();
                    _dal.DeletePracticeMode(jObject.Value<ulong>("id"));
                }
            }
        }

        public void DeletePracticeMode(ulong practiceModeId)
        {
            using (_dal)
            {
                _dal.DeletePracticeMode(practiceModeId);
            }
        }

        public IDictionary<string, object> SavePracticeMode(IDictionary<string, object> parameters)
        {
            using (_dal)
            {
                long id = long.Parse(parameters["id"].ToString());
                if (id == 0)
                {
                    return _dal.InsertPracticeMode(parameters);
                }
                else
                {
                    return _dal.UpdatePracticeMode(parameters);
                }
            }
        }

        public IDictionary<string, object> GetPracticeMode(ulong practiceModeId)
        {
            using (_dal)
            {
                return _dal.GetPracticeMode(practiceModeId);
            }
        }


        public IList<IDictionary<string, object>> GetAllPracticeMode()
        {
            using (_dal)
            {
                IList<IDictionary<string, object>> list = _dal.GetChildrenPracticeModeList(0);
                foreach (var dic in list)
                {
                    var id = ulong.Parse(dic["id"].ToString());
                    var children = _dal.GetChildrenPracticeModeList(id);
                    if (children.Count > 0)
                    {
                        dic["children"] = children;
                    }
                }
                return list;
            }
        }


        public IDictionary<string, object> GetGuideMusicPage(int pageIndex, int pageSize, IDictionary<string, object> searhParams)
        {
            using (_dal)
            {
                return _dal.GetGuideMusicPage(pageIndex, pageSize, searhParams);
            }
        }

        public void BatchDeleteGuideMusic(JArray jArray)
        {
            using (_dal)
            {
                foreach (var json in jArray)
                {
                    JObject jObject = json.ToObject<JObject>();
                    _dal.DeleteGuideMusic(jObject.Value<ulong>("id"));
                }
            }
        }

        public void DeleteGuideMusic(ulong guideMusicId)
        {
            using (_dal)
            {
                _dal.DeleteGuideMusic(guideMusicId);
            }
        }

        public IDictionary<string, object> SaveGuideMusic(IDictionary<string, object> parameters)
        {
            using (_dal)
            {
                IDictionary<string, object> result;
                ulong id = ulong.Parse(parameters["id"].ToString());
                string practiceModeIdStr = parameters["practice_mode_id"].ToString();
                parameters.Remove("practice_mode_id");

                if (id == 0)
                {
                    result = _dal.InsertGuideMusic(parameters);
                }
                else
                {
                    result = _dal.UpdateGuideMusic(parameters);
                }
                id = ulong.Parse(parameters["id"].ToString());
                _dal.DeletePracticeMusic(id);
                if(!string.IsNullOrWhiteSpace(practiceModeIdStr))
                {
                    string[] practiceModeIds = practiceModeIdStr.Split(",");
                    foreach(var practiceModeId in practiceModeIds)
                    {
                        if (string.IsNullOrWhiteSpace(practiceModeId)) continue;
                        IDictionary<string, object> dic = new Dictionary<string, object>();
                        dic["music_id"] = id;
                        dic["practice_mode_id"] = ulong.Parse(practiceModeId);
                        _dal.InsertPracticeMusic(dic);
                    }
                }
                return result;
            }
        }

        public IDictionary<string, object> GetGuideMusic(ulong guideMusicId)
        {
            using (_dal)
            {
                return _dal.GetGuideMusic(guideMusicId);
            }
        }

        public IDictionary<string, object> SaveProjectEnterprise(IDictionary<string, object> parameters)
        {
            using (_dal)
            {
                long id = long.Parse(parameters["id"].ToString());
                if (id == 0)
                {
                    return _dal.InsertProjectEnterprise(parameters);
                }
                else
                {
                    return _dal.UpdateProjectEnterprise(parameters);
                }
            }
        }

        public IDictionary<string, object> GetProjectEnterprise(ulong projectEnterpriseId)
        {
            using (_dal)
            {
                return _dal.GetProjectEnterprise(projectEnterpriseId);
            }
        }

        public IList<IDictionary<string, object>> GetAllEnterprise(ulong projectId, ulong projectEnterpriseId)
        {
            using (_dal)
            {
                return _dal.GetAllEnterprise(projectId, projectEnterpriseId);
            }
        }

        public IDictionary<string, object> GetProjectEnterprisePage(ulong projectId, int pageIndex, int pageSize, IDictionary<string, object> searhParams)
        {
            using (_dal)
            {
                return _dal.GetProjectEnterprisePage(projectId, pageIndex, pageSize, searhParams);
            }
        }

        public void DeleteProjectEnterprise(ulong projectEnterpriseId)
        {
            using (_dal)
            {
                _dal.DeleteProjectEnterprise(projectEnterpriseId);
            }
        }

        public void BatchDeleteProjectEnterprise(JArray jArray)
        {
            using (_dal)
            {
                foreach (var json in jArray)
                {
                    JObject jObject = json.ToObject<JObject>();
                    _dal.DeleteProjectEnterprise(jObject.Value<ulong>("id"));
                }
            }
        }
    }
}

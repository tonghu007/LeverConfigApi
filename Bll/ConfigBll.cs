using System;
using Newtonsoft.Json.Linq;
using Lever.Common;
using Lever.Dal;
using Lever.IBLL;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Configuration;

namespace Lever.Bll
{
    public class ConfigBll : IConfigBll
    {
        private readonly ConfigDal _dal;
        public ConfigBll(ConfigDal dal)
        {
            _dal = dal;
        }


        public IDictionary<string, object> ApiConfigPage(int pageIndex, int pageSize, IDictionary<string, object> searchParams)
        {
            using (_dal)
            {
                return _dal.ApiConfigPage(pageIndex, pageSize, searchParams);
            }
        }

        public IDictionary<string, object> GetApiRow(string apiIdCode)
        {
            using (_dal)
            {
                return _dal.GetApiRow(apiIdCode);
            }
        }

        public IDictionary<string, object> SaveApi(IDictionary<string, object> parameters)
        {
            using (_dal)
            {
                string apiIdCode = parameters["ApiIdCode"].ToString();
                IDictionary<string, object> result;
                if (string.IsNullOrEmpty(apiIdCode))
                {
                    result = _dal.InsertApi(parameters);
                    int codeKind = int.Parse(parameters["CodeKind"].ToString());
                    if (codeKind == 1)
                    {
                        //分页添加默认参数（PageIndex，PageSize）
                        apiIdCode = result["ApiIdCode"].ToString();
                        this.AddPageFixedParams(apiIdCode);
                    }
                }
                else
                {
                    var apiConfig = _dal.GetApiRow(apiIdCode);
                    int oldCodeKind = int.Parse(apiConfig["CodeKind"].ToString());
                    int newCodeKind = int.Parse(parameters["CodeKind"].ToString());
                    if (oldCodeKind != 1 && newCodeKind == 1)
                    {
                        this.AddPageFixedParams(apiIdCode);
                    }
                    else if (newCodeKind != 1 && oldCodeKind == 1)
                    {
                        _dal.DeleteParam(apiIdCode, 1);
                    }
                    result = _dal.UpdateApi(parameters);
                }
                return result;
            }
        }

        private void AddPageFixedParams(string apiIdCode)
        {
            IDictionary<string, object> param = new Dictionary<string, object>();
            param["IsFixed"] = 1;
            param["ApiIdCode"] = apiIdCode;
            param["ParamCode"] = "PageSize";
            param["ParamName"] = "每页数据条数";
            param["ParamType"] = 1;
            param["Description"] = "固定参数";
            _dal.InsertParam(param);
            param.Clear();
            param["IsFixed"] = 1;
            param["ApiIdCode"] = apiIdCode;
            param["ParamCode"] = "PageIndex";
            param["ParamName"] = "页码";
            param["ParamType"] = 1;
            param["Description"] = "固定参数";
            _dal.InsertParam(param);
        }

        public void DeleteApi(string apiIdCode)
        {
            using (_dal)
            {
                _dal.DeleteApi(apiIdCode);
            }
        }

        public void BatchDeleteApi(JArray jArray)
        {
            using (_dal)
            {
                foreach (var json in jArray)
                {
                    JObject jObject = json.ToObject<JObject>();
                    _dal.DeleteApi(jObject.Value<string>("ApiIdCode"));
                }
            }
        }

        public IDictionary<string, object> ParamsPage(string apiIdCode, int pageIndex, int pageSize)
        {
            using (_dal)
            {
                return _dal.ParamsPage(apiIdCode, pageIndex, pageSize);
            }
        }

        public IDictionary<string, object> GetParamRow(long paramId)
        {
            using (_dal)
            {
                return _dal.GetParamRow(paramId);
            }
        }

        public IDictionary<string, object> SaveParam(IDictionary<string, object> parameters)
        {
            using (_dal)
            {
                long paramId = long.Parse(parameters["ParamId"].ToString());
                if (paramId == 0)
                    return _dal.InsertParam(parameters);
                else
                    return _dal.UpdateParam(parameters);
            }
        }

        public void DeleteParam(long paramId)
        {
            using (_dal)
            {
                _dal.DeleteParam(paramId);
            }
        }

        public void BatchDeleteParam(JArray jArray)
        {
            using (_dal)
            {
                foreach (var json in jArray)
                {
                    JObject jObject = json.ToObject<JObject>();
                    _dal.DeleteParam(jObject.Value<long>("ParamId"));
                }
            }
        }

        public IDictionary<string, object> QueryApiConfigPage(int pageIndex, int pageSize, int apiKind, IDictionary<string, object> searhParams)
        {
            using (_dal)
            {
                return _dal.QueryApiConfigPage(pageIndex, pageSize, apiKind, searhParams);
            }
        }

        public IList<IDictionary<string, object>> GetApiParaqms(string apiIdCode, params int[] paramsKind)
        {
            using (_dal)
            {
                return _dal.GetApiParaqms(apiIdCode, paramsKind);
            }
        }

        public IDictionary<string, object> GroupPage(int pageIndex, int pageSize, IDictionary<string, object> searhParams)
        {
            using (_dal)
            {
                return _dal.GroupPage(pageIndex, pageSize, searhParams);
            }
        }

        public void DeleteGroup(string groupId)
        {
            using (_dal)
            {
                _dal.DeleteGroup(groupId);
            }
        }

        public void BatchDeleteGroup(JArray jArray)
        {
            using (_dal)
            {
                foreach (var json in jArray)
                {
                    JObject jObject = json.ToObject<JObject>();
                    _dal.DeleteGroup(jObject.Value<string>("GroupId"));
                }
            }
        }

        public IDictionary<string, object> SaveGroup(IDictionary<string, object> parameters)
        {
            using (_dal)
            {
                long groupId = long.Parse(parameters["GroupId"].ToString());
                string isModify = parameters["IsModify"].ToString();
                
                if (isModify == "1")
                {
                    string aesKey = parameters["AesKey"].ToString();
                    string aesIv = parameters["AesIv"].ToString();
                    string securityKey = parameters["SecurityKey"].ToString();
                    aesKey = RsaCryptoUtils.Decrypt(AesCryptoUtils.base64UrlDecode(aesKey), RsaCryptoUtils.GetPublicKey(), RsaCryptoUtils.GetPrivateKey(), 1024);
                    aesIv = RsaCryptoUtils.Decrypt(AesCryptoUtils.base64UrlDecode(aesIv), RsaCryptoUtils.GetPublicKey(), RsaCryptoUtils.GetPrivateKey(), 1024);
                    byte[] aesKeyByte = Encoding.UTF8.GetBytes(aesKey);
                    byte[] aesIvByte = Encoding.UTF8.GetBytes(aesIv);
                    securityKey = AesCryptoUtils.Decrypt(securityKey, aesKeyByte, aesIvByte);
                    aesKeyByte = Encoding.UTF8.GetBytes(AppConfigurtaionHelper.Configuration.GetValue<string>("AesCrypto:Key"));
                    aesIvByte = Encoding.UTF8.GetBytes(AppConfigurtaionHelper.Configuration.GetValue<string>("AesCrypto:Iv"));
                    securityKey = AesCryptoUtils.Encrypt(securityKey, aesKeyByte, aesIvByte);
                    parameters["SecurityKey"] = securityKey;
                }
                parameters.Remove("IsModify");
                parameters.Remove("AesIv");
                parameters.Remove("AesKey");
                if (groupId == 0)
                    return _dal.InsertGroup(parameters);
                else
                    return _dal.UpdateGroup(parameters);
            }
        }

        public IDictionary<string, object> GetGroupRow(long groupId)
        {
            using (_dal)
            {
                return _dal.GetGroupRow(groupId);
            }
        }

        public IList<IDictionary<string, object>> GetAllGroup()
        {
            using (_dal)
            {
                return _dal.GetAllGroup();
            }
        }
    }

}

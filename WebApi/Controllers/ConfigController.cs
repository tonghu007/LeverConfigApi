using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Lever.Bll;
using Lever.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using WebApi.Extensions;
using Lever.IBLL;

namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiLoginActionFilter]
    [ApiController]
    public class ConfigController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<ConfigController> _logger;
        private readonly IConfigBll _configBll;

        public ConfigController(ILogger<ConfigController> logger, IConfigBll configBll, IConfiguration configuration)
        {
            _logger = logger;
            _configBll = configBll;
            _configuration = configuration;
        }

        [HttpGet]
        public IDictionary<string, object> Get(int pageIndex, int pageSize, string searchKey = "{}")
        {
            var searhParams = JsonConvert.DeserializeObject<IDictionary<string, object>>(searchKey);
            IDictionary<string, object> page = _configBll.ApiConfigPage(pageIndex, pageSize, searhParams);
            return page;
        }

        [HttpGet("{apiIdCode}")]
        public IDictionary<string, object> GetApiRow(string apiIdCode = "")
        {
            IDictionary<string, object> row = _configBll.GetApiRow(apiIdCode);
            return row;
        }

        [HttpPost]
        [Route("SaveApi")]
        public void SaveApi([FromBody] dynamic model)
        {
            IDictionary<string, object> parameters = RequestDataHelper.GetMixParams();
            if (parameters.Count > 0)
            {
                _configBll.SaveApi(parameters);
            }
        }

        [HttpGet("Delete/{apiIdCode}")]
        public void Delete(string apiIdCode)
        {
            _configBll.DeleteApi(apiIdCode);
        }

        [HttpPost]
        [Route("ApiBatchDelete")]
        public void BatchDelete([FromBody] dynamic model)
        {
            JArray parameters = (JArray)RequestDataHelper.GetBodyJsonParameters();
            _configBll.BatchDeleteApi(parameters);
        }

        [HttpGet("Params/{apiIdCode}")]
        public IDictionary<string, object> GetParams(int pageIndex, int pageSize, string apiIdCode = "")
        {
            IDictionary<string, object> page = _configBll.ParamsPage(apiIdCode, pageIndex, pageSize);
            return page;
        }

        [HttpGet("ParamRow/{paramId}")]
        public IDictionary<string, object> GetParamRow(long paramId = 0)
        {
            IDictionary<string, object> row = _configBll.GetParamRow(paramId);
            return row;
        }

        [HttpPost]
        [Route("SaveParam")]
        public void SaveParam([FromBody] dynamic model)
        {
            IDictionary<string, object> parameters = RequestDataHelper.GetMixParams();
            if (parameters.Count > 0)
            {
                _configBll.SaveParam(parameters);
            }
        }

        [HttpGet("ParamDelete/{apiIdCode}")]
        public void ParamDelete(long paramId)
        {
            _configBll.DeleteParam(paramId);
        }

        [HttpPost]
        [Route("ParamBatchDelete")]
        public void ParamBatchDelete([FromBody] dynamic model)
        {
            JArray parameters = (JArray)RequestDataHelper.GetBodyJsonParameters();
            _configBll.BatchDeleteParam(parameters);
        }

        [HttpGet]
        [Route("Query")]
        public IDictionary<string, object> ApiQuery(int pageIndex, int pageSize, string searchKey = "{}")
        {
            var searhParams = JsonConvert.DeserializeObject<IDictionary<string, object>>(searchKey);
            IDictionary<string, object> page = _configBll.QueryApiConfigPage(pageIndex, pageSize, 0, searhParams);
            return page;
        }

        [HttpGet]
        [Route("Group")]
        public IDictionary<string, object> GetGroups(int pageIndex, int pageSize, string searchKey = "{}")
        {
            var searhParams = JsonConvert.DeserializeObject<IDictionary<string, object>>(searchKey);
            IDictionary<string, object> page = _configBll.GroupPage(pageIndex, pageSize, searhParams);
            return page;
        }

        [HttpGet("GroupDelete/{groupId}")]
        public void GroupDelete(string groupId)
        {
            _configBll.DeleteGroup(groupId);
        }

        [HttpPost]
        [Route("ApiGroupBatchDelete")]
        public void ApiGroupBatchDelete([FromBody] dynamic model)
        {
            JArray parameters = (JArray)RequestDataHelper.GetBodyJsonParameters();
            _configBll.BatchDeleteGroup(parameters);
        }

        [HttpPost]
        [Route("SaveGroup")]
        public void SaveGroup([FromBody] dynamic model)
        {
            try
            {
                IDictionary<string, object> parameters = RequestDataHelper.GetMixParams();
                if (parameters.Count > 0)
                {
                    _configBll.SaveGroup(parameters);
                }
            }
            catch (Exception e) {
                _logger.LogError(e,"接口分组配置异常");
                throw;
            }
        }

        [HttpGet("Group/{groupId}")]
        public IDictionary<string, object> GetGroupRow(long groupId = 0)
        {
            IDictionary<string, object> row = _configBll.GetGroupRow(groupId);
            return row;
        }

        [HttpGet]
        [Route("AllGroup")]
        public IList<IDictionary<string, object>> GetAllGroup()
        {
            return _configBll.GetAllGroup();
        }

        [HttpPost]
        [Route("Login")]
        public object Login([FromBody] dynamic model)
        {
            IDictionary<string, object> parameters = RequestDataHelper.GetMixParams();
            string username = parameters["username"].ToString();
            string password = parameters["password"].ToString();
            if (_configuration.GetValue<string>("AdminAccount:Account") == username && _configuration.GetValue<string>("AdminAccount:Password") == password)
            {
                HttpContext.Session.SetString("User", JsonConvert.SerializeObject(parameters));
                //跳转到系统首页
                return RsaCryptoUtils.GetPublicKey();
            }
            else {
                throw new CustomException(11, "用户名或密码错误");
            }
        }

        [HttpPost]
        [Route("Test")]
        public object Test([FromForm] dynamic form)
        {
            IDictionary<string, object> parameters = RequestDataHelper.GetMixParams();
            var files = RequestDataHelper.GetAllFiles();
            return null;
        }
    }
}
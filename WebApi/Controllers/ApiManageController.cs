using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Lever.Bll;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Lever.IBLL;

namespace WebApi.Controllers
{
    public class ApiManageController : BaseController
    {
        private readonly ILogger<ApiManageController> _logger;
        private readonly IConfigBll _configBll;

        public ApiManageController(ILogger<ApiManageController> logger, IConfigBll configBll)
        {
            _logger = logger;
            _configBll = configBll;
        }

        /// <summary>
        /// 首页
        /// </summary>
        /// <returns></returns>
        public IActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// API接口列表
        /// </summary>
        /// <returns></returns>
        public IActionResult ApiList()
        {
            return View();
        }

        public IActionResult ApiEdit(string ApiIdCode = "")
        {
            ViewData["ApiIdCode"] = ApiIdCode;
            return View();
        }

        public IActionResult EditParameter(string ApiIdCode, long ParamId)
        {
            ViewData["ApiIdCode"] = ApiIdCode;
            ViewData["ParamId"] = ParamId;
            return View();
        }

        /// <summary>
        /// API接口查询页
        /// </summary>
        /// <returns></returns>
        public IActionResult ApiQuery()
        {
            return View();
        }

        /// <summary>
        /// 查看Api文档
        /// </summary>
        /// <returns></returns>
        public IActionResult ApiDocument(string ApiIdCode)
        {
            IDictionary<string, object> apiConfig = _configBll.GetApiRow(ApiIdCode);
            ViewData["apiConfig"] = apiConfig;
            IList<IDictionary<string, object>> apiParams = _configBll.GetApiParaqms(ApiIdCode);
            ViewData["apiParams"] = apiParams;
            return View();
        }

        /// <summary>
        /// Api接口测试
        /// </summary>
        /// <returns></returns>
        public IActionResult ApiTest(string ApiIdCode)
        {
            IDictionary<string, object> apiConfig = _configBll.GetApiRow(ApiIdCode);
            ViewData["apiConfig"] = apiConfig;
            IList<IDictionary<string, object>> apiParams = _configBll.GetApiParaqms(ApiIdCode,0);
            IDictionary<string, object> paramsDic = new Dictionary<string, object>();
            object resultJson;
            foreach (var param in apiParams)
            {
                string paramCode = (string)param["ParamCode"];
                if (string.IsNullOrWhiteSpace(paramCode))
                {
                    resultJson = new List<object>();
                    continue;
                }
                else
                {
                    int paramType = (int)param["ParamType"];
                    paramsDic[paramCode] = this.SetDefaultValue(paramType);
                }
            }
            int codeKind = (int)apiConfig["CodeKind"];
            if (codeKind == 1)
            {
                paramsDic["PageSize"] = 10;
                paramsDic["PageIndex"] = 1;
            }
            resultJson = paramsDic;
            ViewData["json"] = this.FormatSerializeJson(resultJson);
            return View();
        }

        public string FormatSerializeJson(object obj)
        {
            JsonSerializer serializer = new JsonSerializer();
            using (StringWriter textWriter = new StringWriter())
            {
                JsonTextWriter jsonWriter = new JsonTextWriter(textWriter)
                {
                    Formatting = Formatting.Indented,
                    Indentation = 4,
                    IndentChar = ' '
                };
                serializer.Serialize(jsonWriter, obj);
                return textWriter.ToString();
            }
        }

        //0 = String,1 = Integer,2 = Long,3 = Double,4 = Float,5 = Decimal,6 = Boolean,7 = Date,8 = DateTime,9 = Key/Value,10 = List,11 = File
        private object SetDefaultValue(int paramType)
        {
            switch (paramType)
            {
                case 0:
                    return "";
                case 1:
                    return 0;
                case 2:
                    return 0;
                case 3:
                    return 0;
                case 4:
                    return 0;
                case 5:
                    return 0;
                case 6:
                    return false;
                case 7:
                    return new DateTime().Date;
                case 8:
                    return new DateTime();
                case 9:
                    return 0;
                case 10:
                    return new Dictionary<string, object>();
                case 11:
                    return new List<string>();
                case 12:
                    return null;
                default:
                    return null;
            }
        }

        public IActionResult ApiGroup()
        {
            return View();
        }

        public IActionResult ApiGroupEdit(long GroupId=0)
        {
            ViewData["GroupId"] = GroupId;
            return View();
        }

    }
}
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Lever.Bll;
using Lever.Common;
using Lever.DBUtility;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using WebApi.Extensions;
using Microsoft.AspNetCore.Authentication;
using Lever.IBLL;

namespace WebApi.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class ApiController : ControllerBase
    {

        private readonly ILogger<ApiController> _logger;
        private readonly IDynamicApiBll _dynamicApiBll;

        public ApiController(ILogger<ApiController> logger, IDynamicApiBll dynamicApiBll)
        {
            _logger = logger;
            _dynamicApiBll = dynamicApiBll;

        }

        [HttpPost("PostForm/{code}")]
        public object PostFromForm(string code, [FromForm]dynamic model) {
            return _dynamicApiBll.DynamicFetch(code);
        }

        [HttpPost("PostBody/{code}")]
        public object PostFromBody(string code,[FromBody]dynamic model)
        {
            return _dynamicApiBll.DynamicFetch(code);
        }

        [HttpPut("PutForm/{code}")]
        public object PutFromForm(string code, [FromForm]dynamic model)
        {
            return _dynamicApiBll.DynamicFetch(code);
        }

        [HttpPut("PutBody/{code}")]
        public object PutFromBody(string code, [FromBody]dynamic model)
        {
            return _dynamicApiBll.DynamicFetch(code);
        }

        [HttpGet("Get/{code}")]
        public object Get(string code)
        {
            return _dynamicApiBll.DynamicFetch(code);
        }

        [HttpDelete("Delete/{code}")]
        public object Delete(string code)
        {
            return _dynamicApiBll.DynamicFetch(code);
        }
    }
}

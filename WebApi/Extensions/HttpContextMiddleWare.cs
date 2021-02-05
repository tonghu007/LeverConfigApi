using Lever.Common;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApi.Extensions
{

    /// <summary>
    /// 可用来处理参数，用Filter实现了，此中间件没有使用
    /// </summary>
    public class HttpContextMiddleWare
    {
        public RequestDelegate _next;

        public HttpContextMiddleWare(RequestDelegate next) {
            this._next = next;
        }

        public async Task Invoke(HttpContext context) {
            RequestDataHelper.InitParams(context);
            await this._next(context);
        }
    }
}

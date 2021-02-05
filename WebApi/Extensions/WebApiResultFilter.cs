using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApi.Extensions
{
    public class WebApiResultFilter : ActionFilterAttribute
    {
        public override void OnResultExecuting(ResultExecutingContext context)
        {
            //根据实际需求进行具体实现
            if (context.Result is ObjectResult)
            {
                var objectResult = context.Result as ObjectResult;
                context.Result = new ObjectResult(new { code = 0, message = "", data = objectResult.Value });
            }
            else if (context.Result is EmptyResult)
            {
                context.Result = new ObjectResult(new { code = 0, message = "", data = "" });
            }
            else if (context.Result is ContentResult)
            {
                context.Result = new ObjectResult(new { code = 0, message = "", data = (context.Result as ContentResult).Content });
            }
            else if (context.Result is StatusCodeResult)
            {
                context.Result = new ObjectResult(new { code = (context.Result as StatusCodeResult).StatusCode, message = "" });
            }
        }
    }
}

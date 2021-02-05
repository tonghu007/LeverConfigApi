using Lever.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace WebApi.Extensions
{
    public class CustomExceptionFilter : IExceptionFilter
    {
        private readonly ILogger<CustomExceptionFilter> _logger;
        public CustomExceptionFilter(ILogger<CustomExceptionFilter> logger)
        {
            _logger = logger;
        }
        public void OnException(ExceptionContext context)
        {
            if (context.ExceptionHandled)
                return;
            Exception exception = context.Exception;
            if (exception is CustomException)
            {
                CustomException customException = (CustomException)exception;
                context.ExceptionHandled = true;
                context.Result = new ObjectResult(new { code = customException.Code, message = customException.Message, data = "" });
            }
            else if(exception is Exception)
            {
                _logger.LogError(exception, "未处理异常");
                context.ExceptionHandled = true;
                context.Result = new ObjectResult(new { code = 99, message = "咦，好像出现了点问题哦", data = "" });
            }
        }
    }
}

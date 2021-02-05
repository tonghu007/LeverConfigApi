using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace WebApi.Controllers
{
    public class BaseController : Controller
    {
        /// <summary>
        /// 请求过滤处理
        /// </summary>
        /// <param name="filterContext"></param>
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var session = filterContext.HttpContext.Session;
            string result=session.GetString("User");
            if (result == null)
            {
                filterContext.Result = new RedirectResult("/Login/SignIn");
                return;
            }
            base.OnActionExecuting(filterContext);
        }
    }

}
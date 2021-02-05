using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Lever.Bll;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Lever.IBLL;
using Lever.Common;

namespace WebApi.Controllers
{
    public class HomeController : BaseController
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IDynamicApiBll _dynamicApiBll;

        public HomeController(ILogger<HomeController> logger, IDynamicApiBll dynamicApiBll)
        {
            _logger = logger;
            _dynamicApiBll = dynamicApiBll;
        }

        public IActionResult Index()
        {
            return View();
        }
    }
}
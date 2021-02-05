using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers
{
    public class PartialController : Controller
    {
        public IActionResult _Header()
        {
            return PartialView("_Header");
        }
    }
}
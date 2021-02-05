using Lever.Bll;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApi.Components
{
    public class LayoutHeaderViewComponent : ViewComponent
    {
        private readonly ILogger<LayoutHeaderViewComponent> _logger;
        private readonly ComponentBll _componentBll;

        public LayoutHeaderViewComponent(ILogger<LayoutHeaderViewComponent> logger, ComponentBll componentBll)
        {
            _logger = logger;
            _componentBll = componentBll;
        }

        public async Task<IViewComponentResult> InvokeAsync(int count)
        {
            return View();
        }
    }
}

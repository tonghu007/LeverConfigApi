using Lever.Bll;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApi.Components
{
    public class LayoutFooterViewComponent : ViewComponent
    {
        private readonly ILogger<LayoutFooterViewComponent> _logger;
        private readonly ComponentBll _componentBll;

        public LayoutFooterViewComponent(ILogger<LayoutFooterViewComponent> logger, ComponentBll componentBll)
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

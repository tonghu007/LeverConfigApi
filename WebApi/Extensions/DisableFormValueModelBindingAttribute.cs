using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApi.Extensions
{
    /// <summary>
    /// 用于指定某个action或者controller不自动绑定表单模型（无效，没有使用）
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class DisableFormValueModelBindingAttribute : Attribute, IResourceFilter
    {
        public void OnResourceExecuting(ResourceExecutingContext context)
        {
            var factories = context.ValueProviderFactories;
            factories.RemoveType<FormValueProviderFactory>();
            factories.RemoveType<JQueryFormValueProviderFactory>();
            factories.RemoveType<JQueryQueryStringValueProviderFactory>();

            //factories.RemoveType<FormFileValueProviderFactory>();//.netcore3.0
        }

        public void OnResourceExecuted(ResourceExecutedContext context)
        {

        }
    }
}

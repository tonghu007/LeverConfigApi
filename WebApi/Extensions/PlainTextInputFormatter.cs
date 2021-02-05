using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Formatters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebApi.Extensions
{
    public class PlainTextInputFormatter: TextInputFormatter
    {
        public PlainTextInputFormatter()
        {
            SupportedMediaTypes.Add("text/plain");
            SupportedEncodings.Add(Encoding.UTF8);
        }

        public override async Task<InputFormatterResult> ReadRequestBodyAsync(InputFormatterContext context, Encoding encoding)
        {
            string content;
            context.HttpContext.Request.EnableBuffering();
            using (var reader = context.ReaderFactory(context.HttpContext.Request.Body, encoding))
            {
                content = await reader.ReadToEndAsync();
            }
            return await InputFormatterResult.SuccessAsync(content);
        }
    }
}

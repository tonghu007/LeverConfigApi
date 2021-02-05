using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;

namespace Lever.Common
{
    public class RequestDataHelper
    {
        private readonly static AsyncLocal<ReqestParams> threadLocalParams = new AsyncLocal<ReqestParams>();

        public static void InitParams(HttpContext httpContext)
        {
            if (threadLocalParams.Value == null)
            {
                IDictionary<string, object> queryParams = RequestDataHelper.GetQueryParameters(httpContext.Request);
                IDictionary<string, object> formData = RequestDataHelper.GetFormParameters(httpContext.Request);
                object body = RequestDataHelper.GetBodyJsonParameters(httpContext.Request, httpContext.Response);
                IDictionary<string, IList<IFormFile>> files = RequestDataHelper.GetFormFiles(httpContext.Request);
                IDictionary<string, object> headers = RequestDataHelper.GetRequestHeaders(httpContext.Request);
                IDictionary<string, object> cookies = RequestDataHelper.GetRequestCookies(httpContext.Request);
                var mixParams = RequestDataHelper.MixParameters(queryParams, formData, body);
                threadLocalParams.Value = new ReqestParams(httpContext, queryParams, files, headers, cookies, formData, mixParams, body);
            }
        }

        public static HttpContext GetHttpContext()
        {
            if (threadLocalParams.Value != null)
            {
                return threadLocalParams.Value.Context;
            }
            return null;
        }

        public static IDictionary<string, object> GetMixParams()
        {
            if (threadLocalParams.Value != null)
            {
                return threadLocalParams.Value.MixPrams;
            }
            return null;
        }

        public static IDictionary<string, object> GetQueryParameters()
        {
            if (threadLocalParams.Value != null)
            {
                return threadLocalParams.Value.QueryPrams;
            }
            return null;
        }

        public static IDictionary<string, object> GetFormParameters()
        {
            if (threadLocalParams.Value != null)
            {
                return threadLocalParams.Value.FormData;
            }
            return null;
        }

        public static object GetBodyJsonParameters()
        {
            if (threadLocalParams.Value != null)
            {
                return threadLocalParams.Value.Body;
            }
            return null;
        }

        public static IDictionary<string, IList<IFormFile>> GetAllFiles()
        {
            if (threadLocalParams.Value != null)
            {
                return threadLocalParams.Value.Files;
            }
            return null;
        }

        public static IDictionary<string, object> GetHeaders()
        {
            if (threadLocalParams.Value != null)
            {
                return threadLocalParams.Value.Headers;
            }
            return null;
        }

        public static IDictionary<string, object> GetCookies()
        {
            if (threadLocalParams.Value != null)
            {
                return threadLocalParams.Value.Cookies;
            }
            return null;
        }

        private static IDictionary<string, object> MixParameters(IDictionary<string, object> query, IDictionary<string, object> formData, object bodyJson)
        {
            var parameters = RequestDataHelper.MergeDictionary(query, formData);
            if (bodyJson is IDictionary<string, object>)
            {
                IDictionary<string, object> bodyParams = (IDictionary<string, object>)bodyJson;
                parameters = RequestDataHelper.MergeDictionary(parameters, bodyParams);
            }
            else
            {
                parameters["__BodyJson"] = bodyJson;
            }
            return parameters;
        }

        public static IDictionary<string, object> MergeDictionary(params IDictionary<string, object>[] sources)
        {
            IDictionary<string, object> result = new Dictionary<string, object>();
            RequestDataHelper.MergeDictionary(ref result, sources);
            return result;
        }

        public static void MergeDictionary(ref IDictionary<string, object> result, params IDictionary<string, object>[] sources)
        {
            if (result == null)
                result = new Dictionary<string, object>();
            if (sources == null || sources.Length == 0) return;
            foreach (IDictionary<string, object> source in sources)
            {
                if (source == null) continue;
                foreach (var pair in source)
                {
                    result[pair.Key] = pair.Value;
                }
            }
        }

        private static IDictionary<string, object> GetQueryParameters(HttpRequest request)
        {
            IQueryCollection query = request.Query;
            if (query.Count > 0)
            {
                IDictionary<string, object> result = new Dictionary<string, object>();
                foreach (var pair in query)
                {
                    if (!pair.Key.StartsWith("__"))
                    {
                        result[pair.Key] = pair.Value.Count > 0 ? pair.Value[0] : null;
                    }
                }
                return result;
            }
            return null;
        }

        private static IDictionary<string, object> GetFormParameters(HttpRequest request)
        {
            if (request.ContentLength != null && request.ContentLength > 0 && request.HasFormContentType)
            {
                IDictionary<string, object> result = new Dictionary<string, object>();
                ICollection<string> keys = request.Form.Keys;
                if (keys != null && keys.Count > 0)
                {
                    foreach (var key in keys)
                    {
                        if (!key.StartsWith("__"))
                        {
                            result[key] = request.Form[key];
                        }
                    }
                }
                return result;
            }
            return null;
        }

        private static IDictionary<string, IList<IFormFile>> GetFormFiles(HttpRequest request)
        {
            if (request.ContentLength != null && request.ContentLength > 0 && request.HasFormContentType)
            {
                IFormFileCollection files = request.Form.Files;
                if (files != null && files.Count > 0)
                {
                    IDictionary<string, IList<IFormFile>> result = new Dictionary<string, IList<IFormFile>>();
                    foreach (var file in files)
                    {
                        string key = file.Name;
                        if (!result.ContainsKey(key))
                            result.Add(key, new List<IFormFile>());
                        result[key].Add(file);
                    }
                    return result;
                }
            }
            return null;
        }

        private static object GetBodyJsonParameters(HttpRequest request, HttpResponse response)
        {
            if (request.ContentType == "application/json")
            {
                response.ContentType = "application/json";
                request.EnableRewind();
                request.Body.Position = 0;
                var body = request.Body;
                if (body.Length > 0)
                {
                    using (var reader = new StreamReader(body))
                    {
                        var content = reader.ReadToEnd();
                        request.Body.Position = 0;
                        var result = JsonConvert.DeserializeObject(content);
                        if (result is JObject)
                            return ((JObject)result).ToObject<IDictionary<string, object>>();
                        else
                            return result;
                    }
                }
            }
            else if (request.ContentType == "text/plain")
            {
                request.EnableRewind();
                request.Body.Position = 0;
                var body = request.Body;
                if (body.Length > 0)
                {
                    using (var reader = new StreamReader(body))
                    {
                        var content = reader.ReadToEnd();
                        request.Body.Position = 0;
                        return RequestDataHelper.BuildPlainTextParameters(content);
                    }
                }
            }
            return null;
        }

        private static object BuildPlainTextParameters(string content)
        {
            if (string.IsNullOrWhiteSpace(content)) return null;
            IDictionary<string, object> result = new Dictionary<string, object>();
            string[] parameters = Regex.Split(content, "\r\n");
            foreach (var p in parameters)
            {
                if (string.IsNullOrWhiteSpace(p)) continue;
                if (p.StartsWith("__")) continue;
                if (p.IndexOf("=") > 0)
                {
                    string key = p.Substring(0, p.IndexOf("="));
                    result[key] = p.Substring(p.IndexOf("=") + 1);
                }
                else
                {
                    return content;//数据不是键值形式，直接返回数据内容
                }
            }
            return result;
        }

        private static IDictionary<string, object> GetRequestCookies(HttpRequest request)
        {
            var cookies = request.Cookies;
            if (cookies == null || cookies.Count == 0) return null;
            IDictionary<string, object> result = new Dictionary<string, object>();
            foreach (var cookie in cookies)
            {
                result[cookie.Key] = cookie.Value;
            }
            return result;
        }
        private static IDictionary<string, object> GetRequestHeaders(HttpRequest request)
        {
            var headers = request.Headers;
            if (headers == null || headers.Count == 0) return null;
            IDictionary<string, object> result = new Dictionary<string, object>();
            foreach (var header in headers)
            {
                result[header.Key] = header.Value;
            }
            return result;
        }
        private class ReqestParams
        {
            public ReqestParams(HttpContext context, IDictionary<string, object> queryPrams, IDictionary<string, IList<IFormFile>> files, IDictionary<string, object> headers, IDictionary<string, object> cookies, IDictionary<string, object> formData, IDictionary<string, object> mixParams, object body)
            {
                this.Context = context;
                this.QueryPrams = queryPrams;
                this.FormData = formData;
                this.Files = files;
                this.Headers = headers;
                this.Cookies = cookies;
                this.Body = body;
                this.MixPrams = mixParams;
            }
            public HttpContext Context { get; set; }
            public IDictionary<string, object> MixPrams { get; set; }
            public IDictionary<string, object> QueryPrams { get; set; }

            public IDictionary<string, object> Headers { get; set; }

            public IDictionary<string, object> Cookies { get; set; }

            public IDictionary<string, IList<IFormFile>> Files { get; set; }

            public object Body { get; set; }

            public IDictionary<string, object> FormData { get; set; }
        }
    }
}

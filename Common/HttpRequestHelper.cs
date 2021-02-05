using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;

namespace Lever.Common
{
    public class HttpRequestHelper
    {
        public static string Request(string url, string method, string contentType, IDictionary<string, object> data)
        {
            method = string.IsNullOrEmpty(method) ? "GET" : method.ToUpper();
            if (method == "GET")
            {
                if (data != null)
                {
                    StringBuilder builder = new StringBuilder();
                    foreach (KeyValuePair<string, object> pair in data)
                    {
                        if (builder.Length > 0)
                            builder.Append("&");
                        builder.Append($"{pair.Key}={pair.Value}");
                    }
                    if (url.IndexOf("?") > 0)
                    {
                        url = url + "&" + builder;
                    }
                    else
                    {
                        url = url + "?" + builder;
                    }
                }
            }
            var request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = method;
            request.Timeout = 5000; //是进行后续同步请求时使用 GetResponse 方法等待响应以及 GetRequestStream 方法等待流所允许的毫秒数
            if (method == "POST")
            {
                var byteData = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(data));
                var length = byteData.Length;
                contentType = string.IsNullOrEmpty(contentType) ? "application/json;charset=UTF-8" : contentType;
                request.ContentType = contentType;
                request.ContentLength = length;
                request.ServicePoint.Expect100Continue = false;
                using (var writer = request.GetRequestStream())
                {
                    writer.Write(byteData, 0, length);
                }
            }

            //接收响应内容
            var response = (HttpWebResponse)request.GetResponse();
            var responseString = new StreamReader(response.GetResponseStream(), Encoding.GetEncoding("utf-8")).ReadToEnd();
            return responseString.ToString();
        }
    }
}

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApi.Extensions
{
    public class JsonNumberConverter : JsonConverter
    {
        //是否开启自定义反序列化，值为true时，反序列化时会走ReadJson方法，值为false时，不走ReadJson方法，而是默认的反序列化
        public override bool CanRead => true;
        //是否开启自定义序列化，值为true时，序列化时会走WriteJson方法，值为false时，不走WriteJson方法，而是默认的序列化
        public override bool CanWrite => true;

        public override bool CanConvert(Type objectType)
        {

            return objectType == typeof(long) || objectType == typeof(decimal) || objectType == typeof(ulong) || objectType == typeof(double);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            string value = reader.Value as string;
            if (objectType == typeof(long))
                return Convert.ToInt64(value);
            if (objectType == typeof(ulong))
                return Convert.ToUInt64(value);
            if (objectType == typeof(decimal))
                return Convert.ToDecimal(value);
            if (objectType == typeof(double))
                return Convert.ToDouble(value);
            return 0;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            writer.WriteValue(value.ToString());
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Lever.Common
{
    public class ReflectorHelper
    {
        /// <summary>
        /// 创建插件对象
        /// </summary>
        /// <param name="pluginAssemblyPath">程序集名及路径</param>
        /// <param name="PluginClassName">类名<param>
        /// <returns></returns>
        public static T GetPluginInstance<T>(string pluginAssemblyPath, string pluginClassName)
        {
            if (string.IsNullOrEmpty(pluginAssemblyPath) || string.IsNullOrEmpty(pluginClassName)) return default(T);
            Assembly assembly = Assembly.Load(pluginAssemblyPath);
            Type type = assembly.GetType(pluginClassName);
            IList<object> paramObjects = new List<object>();
            //扩展插件支持注入IServiceCollection中对象
            ParameterInfo[] parameters = type.GetConstructors()[0].GetParameters();
            foreach (ParameterInfo info in parameters)
            {
                Type paramType = info.ParameterType;
                object param = ServicesHelper.GetService(paramType);
                paramObjects.Add(param);
            }
            T instance = (T)Activator.CreateInstance(type, paramObjects.ToArray());
            return instance;
        }
    }
}

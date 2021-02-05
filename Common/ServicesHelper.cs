using Microsoft.Extensions.DependencyInjection;
using System;

namespace Lever.Common
{
    public class ServicesHelper
    {
        private static ServiceProvider _serviceProvider;
        public static void SetServiceProvider(IServiceCollection services)
        {
            _serviceProvider = services.BuildServiceProvider();
        }

        public static T GetService<T>()
        {
            return (T)_serviceProvider.GetService(typeof(T));
        }

        public static object GetService(Type type)
        {
            return _serviceProvider.GetService(type);
        }
    }
}

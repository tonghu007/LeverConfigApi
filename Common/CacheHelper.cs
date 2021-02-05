using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using System;

namespace Lever.Common
{
    public class CacheHelper
    {
        private static MemoryCache memoryCache;
        static CacheHelper()
        {
            memoryCache = new MemoryCache(new MemoryCacheOptions()
            {
                SizeLimit = AppConfigurtaionHelper.Configuration.GetValue<int>("CacheSetting:SizeLimit"),
                //缓存满了时，压缩20%（即删除20份优先级低的缓存项）
                CompactionPercentage = AppConfigurtaionHelper.Configuration.GetValue<float>("CacheSetting:CompactionPercentage"),
                //两秒钟查找一次过期项
                ExpirationScanFrequency = TimeSpan.FromSeconds(AppConfigurtaionHelper.Configuration.GetValue<int>("CacheSetting:ExpirationScanFrequency"))
            });
        }

        public T Set<T>(string key, T value)
        {
            return memoryCache.Set<T>(key, value);
        }

        public T Set<T>(string key, T value, TimeSpan timeSpan, bool isSliding)
        {
            if (isSliding)
                return memoryCache.Set<T>(key, value, new MemoryCacheEntryOptions
                {
                    SlidingExpiration = timeSpan
                });
            else
                return memoryCache.Set<T>(key, value, timeSpan);
        }

        public T Get<T>(string key)
        {
            return memoryCache.Get<T>(key);
        }
    }
}

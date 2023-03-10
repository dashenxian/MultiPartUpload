using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Web;

namespace ServerApi
{
    public static class CacheHelper
    {
        public static readonly MemoryCache Cache = MemoryCache.Default;

        /// <summary>
        /// 获取缓存并转为string类型
        /// </summary>
        /// <param name="cache"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static string GetString(this MemoryCache cache, string key)
        {
            return cache.Get(key)?.ToString();
        }
        /// <summary>
        /// 获取指定类型的缓存数据，如果缓存的数据不是T类型或其兼容类型，可能转换会报错
        /// </summary>
        /// <typeparam name="T">类型</typeparam>
        /// <param name="cache"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static T Get<T>(this MemoryCache cache, string key)
        {
            return (T)cache.Get(key);
        }
        /// <summary>
        /// 设置缓存,注意如果直接设置对象会造成缓存直接持有对象，可能出现内存泄露
        /// </summary>
        /// <typeparam name="T">类型</typeparam>
        /// <param name="cache"></param>
        /// <param name="key"></param>
        /// <param name="value">缓存数据</param>
        /// <param name="slidingTimeSpan">滑动过期时间</param>
        public static void Set<T>(this MemoryCache cache, string key, T value, TimeSpan slidingTimeSpan)
        {
            cache.Set(key, value, new CacheItemPolicy() { SlidingExpiration = slidingTimeSpan });
        }
    }
}
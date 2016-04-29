//<TunynetCopyright>
//--------------------------------------------------------------
//<version>V0.5</verion>
//<createdate>2011-10-28</createdate>
//<author>mazq</author>
//<email>mazq@tunynet.com</email>
//<log date="2011-10-28" version="0.5">创建</log>
//--------------------------------------------------------------
//</TunynetCopyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tunynet.Caching;
using System.Runtime.Caching;

namespace Tunynet.Caching
{
    /// <summary>
    /// 使用System.Runtime.Caching.MemoryCache实现的本机缓存
    /// </summary>
    /// <remarks>
    /// 仅能在.net framework4.0及以上版本使用
    /// </remarks>
    public class RuntimeMemoryCache : ICache
    {
        private readonly MemoryCache _cache = MemoryCache.Default; 


        #region ICacheService 成员


        /// <summary>
        /// 加入缓存项
        /// </summary>
        /// <param name="key">缓存项标识</param>
        /// <param name="value">缓存项</param>
        /// <param name="timeSpan">缓存失效时间</param>
        public void Add(string key, object value, TimeSpan timeSpan)
        {
            if (string.IsNullOrEmpty(key) || value == null)
                return;

            _cache.Add(key, value, DateTimeOffset.Now.Add(timeSpan), null);
        }

        /// <summary>
        /// 加入依赖物理文件的缓存项
        /// </summary>
        /// <remarks>主要应用于配置文件或配置项</remarks>
        /// <param name="key">缓存项标识</param>
        /// <param name="value">缓存项</param>
        /// <param name="fullFileNameOfFileDependency">依赖的文件全路径</param>
        public void AddWithFileDependency(string key, object value, string fullFileNameOfFileDependency)
        {
            if (string.IsNullOrEmpty(key) || value == null)
                return;

            CacheItemPolicy policy = new CacheItemPolicy();
            policy.AbsoluteExpiration = DateTimeOffset.Now.AddMonths(1);
            policy.ChangeMonitors.Add(new HostFileChangeMonitor(new List<string>() { fullFileNameOfFileDependency }));

            _cache.Add(key, value, policy, null);
        }

        /// <summary>
        /// 如果不存在缓存项则添加，否则更新
        /// </summary>
        /// <param name="key">缓存项标识</param>
        /// <param name="value">缓存项</param>
        /// <param name="timeSpan">缓存失效时间</param>
        public void Set(string key, object value, TimeSpan timeSpan)
        {
            _cache.Set(key, value, DateTimeOffset.Now.Add(timeSpan));
        }

        /// <summary>
        /// 标识删除
        /// </summary>
        /// <remarks>
        /// 在本机缓存情况下直接删除
        /// </remarks>
        /// <param name="key">缓存项标识</param>
        /// <param name="value">缓存项</param>
        /// <param name="timeSpan">缓存失效时间间隔</param>
        public void MarkDeletion(string key, object value, TimeSpan timeSpan)
        {
            Remove(key);
        }

        /// <summary>
        /// 获取缓存项
        /// </summary>
        /// <param name="cacheKey">缓存项标识</param>
        /// <returns>缓存项</returns>
        public object Get(string cacheKey)
        {
            return _cache[cacheKey];
        }

        /// <summary>
        /// 移除指定的缓存项
        /// </summary>
        /// <param name="cacheKey">要移除的缓存项标识</param>
        public void Remove(string cacheKey)
        {
            _cache.Remove(cacheKey);
        }

        /// <summary>
        /// 从缓存中清除所有缓存项
        /// </summary>
        public void Clear()
        {
            Dictionary<string, KeyValuePair<string, object>> allCacheItems = _cache.AsParallel().ToDictionary(a => { return a.Key; });
            foreach (var key in allCacheItems.Keys)
            {
                _cache.Remove(key);
            }
        }

        ///// <summary>
        ///// 获取缓存服务器统计信息
        ///// </summary>
        ///// <returns></returns>
        //public Dictionary<string, Dictionary<string, string>> GetStatistics()
        //{
        //    throw new NotImplementedException();
        //}

        #endregion

    }


}

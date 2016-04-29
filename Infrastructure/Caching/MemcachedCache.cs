//<TunynetCopyright>
//--------------------------------------------------------------
//<version>V0.5</verion>
//<createdate>2010-3-22</createdate>
//<author>chenq</author>
//<email>chenq@tunynet.com</email>
//<log date="2010-3-22">创建</log>
//<log date="2011-10-28" version="0.6" author="mazq">适应新架构重新调整</log>
//--------------------------------------------------------------
//</TunynetCopyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using Enyim.Caching;
using Enyim.Caching.Memcached;
using System.Net;
using Enyim.Caching.Configuration;

namespace Tunynet.Caching
{
    /// <summary>
    /// 用于连接Memcached的分布式缓存
    /// </summary>
    public class MemcachedCache : ICache
    {
        private MemcachedClient cache = new MemcachedClient();

        #region ICacheService 成员

        /// <summary>
        /// 加入缓存项
        /// </summary>
        /// <param name="key">缓存项标识</param>
        /// <param name="value">缓存项</param>
        /// <param name="timeSpan">缓存失效时间</param>
        public void Add(string key, object value, TimeSpan timeSpan)
        {
            key = key.ToLower();
            cache.Store(StoreMode.Set, key, value, DateTime.Now.Add(timeSpan));
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
            //分布式缓存无法对文件进行监控
            Add(key, value, new TimeSpan(30, 0, 0, 0));
        }

        /// <summary>
        /// 如果不存在缓存项则添加，否则更新
        /// </summary>
        /// <param name="key">缓存项标识</param>
        /// <param name="value">缓存项</param>
        /// <param name="timeSpan">缓存失效时间</param>
        public void Set(string key, object value, TimeSpan timeSpan)
        {
            Add(key, value, timeSpan);
        }

        /// <summary>
        /// 标识删除
        /// </summary>
        /// <remarks>
        /// 由于DB读写分离导致只读DB会有延迟，为保证缓存中的数据时时更新，需要在缓存中设置实体缓存为已删除状态
        /// </remarks>
        /// <param name="key">缓存项标识</param>
        /// <param name="value">缓存项</param>
        /// <param name="timeSpan">缓存失效时间间隔</param>
        public void MarkDeletion(string key, object value, TimeSpan timeSpan)
        {
            //CodePreview@Zhengw::Memcache不需要标识实体已经被删除？
            Set(key, value, timeSpan);
        }

        /// <summary>
        /// 获取缓存项
        /// </summary>
        /// <param name="cacheKey">cacheKey</param>
        public object Get(string cacheKey)
        {
            cacheKey = cacheKey.ToLower();

            System.Web.HttpContext httpContext = System.Web.HttpContext.Current;
            if (httpContext != null && httpContext.Items.Contains(cacheKey))
                return httpContext.Items[cacheKey];

            object obj = cache.Get(cacheKey);

            if (httpContext != null && obj != null)
                httpContext.Items[cacheKey] = obj;

            return obj;
        }

        /// <summary>
        /// 移除缓存项
        /// </summary>
        /// <param name="cacheKey">cacheKey</param>
        public void Remove(string cacheKey)
        {
            cacheKey = cacheKey.ToLower();
            cache.Remove(cacheKey);
        }

        /// <summary>
        /// 清空缓存
        /// </summary>
        public void Clear()
        {
            cache.FlushAll();
        }

        /// <summary>
        /// 获取缓存服务器统计信息
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, Dictionary<string, string>> GetStatistics()
        {
            //ServerStats stats = cache.Stats();
            //return Convert.ToInt32(stats.GetValue(ServerStats.All, StatItem.TotalItems));
            throw new NotImplementedException();
        }

        #endregion

    }
}

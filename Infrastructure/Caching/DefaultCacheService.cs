//<TunynetCopyright>
//--------------------------------------------------------------
//<version>V0.5</verion>
//<createdate>2010-11-01</createdate>
//<author>mazq</author>
//<email>mazq@tunynet.com</email>
//<log date="2010-11-01" version="0.5">创建</log>
//--------------------------------------------------------------
//</TunynetCopyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tunynet.Caching
{
    /// <summary>
    /// 默认提供的缓存服务
    /// </summary>
    [Serializable]
    public class DefaultCacheService : ICacheService
    {
        // 本机缓存
        ICache localCache;

        // 在启用分布式缓存情况下指分布式缓存，否则指本机缓存
        ICache cache;

        private readonly Dictionary<CachingExpirationType, TimeSpan> cachingExpirationDictionary;

        /// <summary>
        /// 构造函数(仅本机缓存)
        /// </summary>
        /// <param name="cache">本机缓存</param>
        /// <param name="cacheExpirationFactor">缓存过期时间因子</param>
        public DefaultCacheService(ICache cache, float cacheExpirationFactor)
            : this(cache, cache, cacheExpirationFactor, false)
        {
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="cache">缓存</param>
        /// <param name="localCache">本机缓存</param>
        /// <param name="cacheExpirationFactor">缓存过期时间因子</param>
        /// <param name="enableDistributedCache">是否启用分布式缓存</param>
        public DefaultCacheService(ICache cache, ICache localCache, float cacheExpirationFactor, bool enableDistributedCache)
        {
            this.cache = cache;
            this.localCache = localCache;
            this.enableDistributedCache = enableDistributedCache;

            cachingExpirationDictionary = new Dictionary<CachingExpirationType, TimeSpan>();
            cachingExpirationDictionary.Add(CachingExpirationType.Invariable, new TimeSpan(0, 0, (int)(24 * 60 * 60 * cacheExpirationFactor)));
            cachingExpirationDictionary.Add(CachingExpirationType.Stable, new TimeSpan(0, 0, (int)(8 * 60 * 60 * cacheExpirationFactor)));
            cachingExpirationDictionary.Add(CachingExpirationType.RelativelyStable, new TimeSpan(0, 0, (int)(2 * 60 * 60 * cacheExpirationFactor)));
            cachingExpirationDictionary.Add(CachingExpirationType.UsualSingleObject, new TimeSpan(0, 0, (int)(10 * 60 * cacheExpirationFactor)));
            cachingExpirationDictionary.Add(CachingExpirationType.UsualObjectCollection, new TimeSpan(0, 0, (int)(5 * 60 * cacheExpirationFactor)));
            cachingExpirationDictionary.Add(CachingExpirationType.SingleObject, new TimeSpan(0, 0, (int)(3 * 60 * cacheExpirationFactor)));
            cachingExpirationDictionary.Add(CachingExpirationType.ObjectCollection, new TimeSpan(0, 0, (int)(3 * 60 * cacheExpirationFactor)));
        }

        bool enableDistributedCache = false;
        /// <summary>
        /// 是否启用分布式缓存
        /// </summary>
        public bool EnableDistributedCache
        {
            get { return enableDistributedCache; }
        }

        /// <summary>
        /// 添加到缓存
        /// </summary>
        /// <param name="cacheKey">缓存项标识</param>
        /// <param name="value">缓存项</param>
        /// <param name="cachingExpirationType">缓存期限类型</param>
        public void Add(string cacheKey, object value, CachingExpirationType cachingExpirationType)
        {
            Add(cacheKey, value, cachingExpirationDictionary[cachingExpirationType]);
        }

        /// <summary>
        /// 添加到缓存
        /// </summary>
        /// <param name="cacheKey">缓存项标识</param>
        /// <param name="value">缓存项</param>
        /// <param name="timeSpan">缓存失效时间间隔</param>
        public void Add(string cacheKey, object value, TimeSpan timeSpan)
        {
            cache.Add(cacheKey, value, timeSpan);
        }

        /// <summary>
        /// 添加或更新缓存
        /// </summary>
        /// <param name="cacheKey">缓存项标识</param>
        /// <param name="value">缓存项</param>
        /// <param name="cachingExpirationType">缓存期限类型</param>
        public void Set(string cacheKey, object value, CachingExpirationType cachingExpirationType)
        {
            Set(cacheKey, value, cachingExpirationDictionary[cachingExpirationType]);
        }

        /// <summary>
        /// 添加或更新缓存
        /// </summary>
        /// <param name="cacheKey">缓存项标识</param>
        /// <param name="value">缓存项</param>
        /// <param name="timeSpan">缓存失效时间间隔</param>
        public void Set(string cacheKey, object value, TimeSpan timeSpan)
        {
            cache.Set(cacheKey, value, timeSpan);
        }

        /// <summary>
        /// 移除缓存
        /// </summary>
        /// <param name="cacheKey">缓存项标识</param>
        public void Remove(string cacheKey)
        {
            cache.Remove(cacheKey);
        }

        /// <summary>
        /// 标识为删除
        /// </summary>
        /// <param name="cacheKey">缓存项标识</param>
        /// <param name="entity">缓存的实体</param>
        /// <param name="cachingExpirationType">缓存期限类型</param>
        public void MarkDeletion(string cacheKey, IEntity entity, CachingExpirationType cachingExpirationType)
        {
            entity.IsDeletedInDatabase = true;
            cache.MarkDeletion(cacheKey, entity, cachingExpirationDictionary[cachingExpirationType]);
        }

        /// <summary>
        /// 清空缓存
        /// </summary>
        public void Clear()
        {
            cache.Clear();
        }

        /// <summary>
        /// 从缓存获取
        /// </summary>
        /// <param name="cacheKey">缓存项标识</param>
        /// <returns>返回cacheKey对应的缓存项，如果不存在则返回null</returns>
        public object Get(string cacheKey)
        {
            object obj = null;
            if (enableDistributedCache)
                obj = localCache.Get(cacheKey);

            if (obj == null)
            {
                obj = cache.Get(cacheKey);
                if (enableDistributedCache)
                    localCache.Add(cacheKey, obj, cachingExpirationDictionary[CachingExpirationType.SingleObject]);
            }

            return obj;
        }

        /// <summary>
        ///////////// 从缓存获取(缓存项必须是引用类型)
        /// </summary>
        /// <typeparam name="T">缓存项类型</typeparam>
        /// <param name="cacheKey">缓存项标识</param>
        /// <returns>返回cacheKey对应的缓存项，如果不存在则返回null</returns>
        public T Get<T>(string cacheKey) where T : class
        {
            object obj = Get(cacheKey);
            if (obj != null)
            {
                T t = obj as T;
                return t;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// 从一层缓存获取缓存项
        /// </summary>
        /// <remarks>
        /// 在启用分布式缓存的情况下，指穿透二级缓存从一层缓存（分布式缓存）读取
        /// </remarks>
        /// <param name="cacheKey">缓存项标识</param>
        /// <returns>返回cacheKey对应的缓存项，如果不存在则返回null</returns>
        public object GetFromFirstLevel(string cacheKey)
        {
            return cache.Get(cacheKey);
        }

        /// <summary>
        /// 从一层缓存获取缓存项
        /// </summary>
        /// <remarks>
        /// 在启用分布式缓存的情况下，指穿透二级缓存从一层缓存（分布式缓存）读取
        /// </remarks>
        /// <typeparam name="T">缓存项类型</typeparam>
        /// <param name="cacheKey">缓存项标识</param>
        /// <returns>返回cacheKey对应的缓存项，如果不存在则返回null</returns>
        public T GetFromFirstLevel<T>(string cacheKey) where T : class
        {
            object obj = GetFromFirstLevel(cacheKey);
            if (obj != null)
            {
                T t = obj as T;
                return t;
            }
            else
            {
                return null;
            }
        }
    }
}

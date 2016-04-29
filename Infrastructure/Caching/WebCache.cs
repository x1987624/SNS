////<TunynetCopyright>
////--------------------------------------------------------------
////<version>V0.5</verion>
////<createdate>2011-10-9</createdate>
////<author>mazq</author>
////<email>mazq@tunynet.com</email>
////<log date="2011-10-9" version="0.5">创建</log>
////--------------------------------------------------------------
////</TunynetCopyright>

//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Web;
//using System.Web.Caching;
//using System.Collections;

//namespace Tunynet.Caching
//{
//    /// <summary>
//    /// 采用System.Web.Caching.Cache实现的ICache（在.net framework4.0以下版本的本机缓存中使用）
//    /// </summary>
//    public class WebCache : ICache
//    {
//        private readonly Dictionary<CachingExpirationTypes, TimeSpan> CachingExpirationDictionary;
//        private readonly Cache _cache;

//        /// <summary>
//        /// WebCache构造函数
//        /// </summary>
//        public WebCache(double cacheExpirationFactor)
//        {
//            HttpContext context = HttpContext.Current;
//            if (context != null)
//                _cache = context.Cache;
//            else
//                _cache = HttpRuntime.Cache;

//            CachingExpirationDictionary = new Dictionary<CachingExpirationTypes, TimeSpan>();
//            CachingExpirationDictionary.Add(CachingExpirationTypes.Invariable, new TimeSpan(0, 0, (int)(24 * 60 * 60 * cacheExpirationFactor)));
//            CachingExpirationDictionary.Add(CachingExpirationTypes.Stable, new TimeSpan(0, 0, (int)(8 * 60 * 60 * cacheExpirationFactor)));
//            CachingExpirationDictionary.Add(CachingExpirationTypes.RelativelyStable, new TimeSpan(0, 0, (int)(2 * 60 * 60 * cacheExpirationFactor)));
//            CachingExpirationDictionary.Add(CachingExpirationTypes.UsualSingleObject, new TimeSpan(0, 0, (int)(10 * 60 * cacheExpirationFactor)));
//            CachingExpirationDictionary.Add(CachingExpirationTypes.UsualObjectCollection, new TimeSpan(0, 0, (int)(5 * 60 * cacheExpirationFactor)));
//            CachingExpirationDictionary.Add(CachingExpirationTypes.SingleObject, new TimeSpan(0, 0, (int)(3 * 60 * cacheExpirationFactor)));
//            CachingExpirationDictionary.Add(CachingExpirationTypes.ObjectCollection, new TimeSpan(0, 0, (int)(3 * 60 * cacheExpirationFactor)));
//        }


//        #region ICacheService 成员


//        /// <summary>
//        /// 加入缓存项
//        /// </summary>
//        /// <param name="key">缓存项标识</param>
//        /// <param name="value">缓存项</param>
//        /// <param name="cachingExpirationType">缓存期限类型</param>
//        public void Add(string key, object value, CachingExpirationTypes cachingExpirationType)
//        {
//            if (key == null || value == null)
//                return;

//            Add(key, value, CachingExpirationDictionary[cachingExpirationType]);
//        }

//        /// <summary>
//        /// 加入缓存项
//        /// </summary>
//        /// <param name="key">缓存项标识</param>
//        /// <param name="value">缓存项</param>
//        /// <param name="timeSpan">缓存失效时间</param>
//        public void Add(string key, object value, TimeSpan timeSpan)
//        {
//            if (string.IsNullOrEmpty(key) || value == null)
//                return;

//            _cache.Add(key, value, null, DateTime.Now.Add(timeSpan), TimeSpan.Zero, CacheItemPriority.Normal, null);
//        }

//        /// <summary>
//        /// 加入依赖物理文件的缓存项
//        /// </summary>
//        /// <remarks>主要应用于配置文件或配置项</remarks>
//        /// <param name="key">缓存项标识</param>
//        /// <param name="value">缓存项</param>
//        /// <param name="fullFileNameOfFileDependency">依赖的文件全路径</param>
//        public void AddWithFileDependency(string key, object value, string fullFileNameOfFileDependency)
//        {
//            if (string.IsNullOrEmpty(key) || value == null)
//                return;

//            CacheDependency cacheDependency = new CacheDependency(fullFileNameOfFileDependency);
//            _cache.Add(key, value, cacheDependency, DateTime.Now.Add(CachingExpirationDictionary[CachingExpirationTypes.Stable]), TimeSpan.Zero, CacheItemPriority.Normal, null);
//        }

//        /// <summary>
//        /// 如果不存在缓存项则添加，否则更新
//        /// </summary>
//        /// <param name="key">缓存项标识</param>
//        /// <param name="value">缓存项</param>
//        /// <param name="cachingExpirationType">缓存期限类型</param>
//        public void Set(string key, object value, CachingExpirationTypes cachingExpirationType)
//        {
//            Remove(key);
//            Add(key, value, cachingExpirationType);
//        }

//        /// <summary>
//        /// 如果不存在缓存项则添加，否则更新
//        /// </summary>
//        /// <param name="key">缓存项标识</param>
//        /// <param name="value">缓存项</param>
//        /// <param name="timeSpan">缓存失效时间</param>
//        public void Set(string key, object value, TimeSpan timeSpan)
//        {
//            Remove(key);
//            Add(key, value, timeSpan);
//        }

//        /// <summary>
//        /// 标识删除
//        /// </summary>
//        /// <remarks>
//        /// 由于DB读写分离导致只读DB会有延迟，为保证缓存中的数据时时更新，需要在缓存中设置ID缓存为已删除状态
//        /// </remarks>
//        /// <param name="key">缓存项标识</param>
//        /// <param name="value">缓存项</param>
//        /// <param name="cachingExpirationType">缓存期限类型</param>
//        public void MarkDeletion(string key, object value, CachingExpirationTypes cachingExpirationType)
//        {
//            Remove(key);
//        }

//        /// <summary>
//        /// 获取缓存项
//        /// </summary>
//        /// <param name="key">缓存项标识</param>
//        /// <returns>缓存项</returns>
//        public object Get(string cacheKey)
//        {
//            return _cache[cacheKey];
//        }

//        /// <summary>
//        /// 移除指定的缓存项
//        /// </summary>
//        /// <param name="key">要移除的缓存项标识</param>
//        public void Remove(string cacheKey)
//        {
//            _cache.Remove(cacheKey);
//        }

//        /// <summary>
//        /// 从缓存中清除所有缓存项
//        /// </summary>
//        public void Clear()
//        {
//            IDictionaryEnumerator cacheEnum = _cache.GetEnumerator();
//            List<string> keys = new List<string>();
//            while (cacheEnum.MoveNext())
//            {
//                keys.Add(cacheEnum.Key.ToString());
//            }

//            foreach (string key in keys)
//            {
//                _cache.Remove(key);
//            }
//        }

//        /// <summary>
//        /// 获取缓存服务器统计信息
//        /// </summary>
//        /// <returns></returns>
//        public Dictionary<string, Dictionary<string, string>> GetStatistics()
//        {
//            throw new NotImplementedException();
//        }

//        #endregion

//    }


//}

//<TunynetCopyright>
//--------------------------------------------------------------
//<version>V0.5</verion>
//<createdate>2011-10-9</createdate>
//<author>mazq</author>
//<email>mazq@tunynet.com</email>
//<log date="2011-10-9" version="0.5">创建</log>
//<log date="2011-10-27" version="0.6" author="libsh">走查</log>
//<log date="2012-02-07" version="0.7" author="mazq">走查</log>
//--------------------------------------------------------------
//</TunynetCopyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Collections.Concurrent;

namespace Tunynet.Caching
{
    /// <summary>
    /// 实时性缓存助手
    /// </summary>
    /// <remarks>
    /// 主要有两个作用：递增缓存版本号、获取缓存CacheKey
    /// </remarks>
    [Serializable]
    public class RealTimeCacheHelper
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="enableCache">是否启用缓存</param>
        /// <param name="typeHashID">类型名称哈希值</param>
        public RealTimeCacheHelper(bool enableCache, string typeHashID)
        {
            EnableCache = enableCache;
            TypeHashID = typeHashID;
        }

        /// <summary>
        /// 是否使用缓存
        /// </summary>
        public bool EnableCache { get; private set; }

        /// <summary>
        /// 缓存过期类型
        /// </summary>
        public CachingExpirationType CachingExpirationType { get; set; }

        /// <summary>
        /// 缓存分区的属性
        /// </summary>
        public IEnumerable<PropertyInfo> PropertiesOfArea { get; set; }

        /// <summary>
        /// 实体正文缓存对应的属性名称（如果不需单独存储实体正文缓存，则不要设置该属性）
        /// </summary>
        public PropertyInfo PropertyNameOfBody { get; set; }

        /// <summary>
        /// 完整名称md5-16
        /// </summary>
        public string TypeHashID { get; private set; }


        #region Cache Version

        int globalVersion = 0;
        /// <summary>
        /// 列表缓存全局version
        /// </summary>
        public int GetGlobalVersion()
        {
            return globalVersion;
        }

        ConcurrentDictionary<object, int> entityVersionDictionary = new ConcurrentDictionary<object, int>();
        /// <summary>
        /// 获取Entity的缓存版本
        /// </summary>
        /// <param name="primaryKey">实体主键</param>
        /// <returns>实体的缓存版本（从0开始）</returns>
        public int GetEntityVersion(object primaryKey)
        {
            int version = 0;
            entityVersionDictionary.TryGetValue(primaryKey, out version);
            return version;
        }

        ConcurrentDictionary<string, ConcurrentDictionary<int, int>> areaVersionDictionary = new ConcurrentDictionary<string, ConcurrentDictionary<int, int>>();
        /// <summary>
        /// 获取列表缓存区域version
        /// </summary>
        /// <param name="propertyName">分区属性名称</param>
        /// <param name="propertyValue">分区属性值</param>
        /// <returns>分区属性的缓存版本（从0开始）</returns>
        public int GetAreaVersion(string propertyName, object propertyValue)
        {
            int version = 0;
            if (string.IsNullOrEmpty(propertyName))
                return version;

            //全部切换成小写
            propertyName = propertyName.ToLower();

            ConcurrentDictionary<int, int> areaVersion;
            if (areaVersionDictionary.TryGetValue(propertyName, out areaVersion))
            {
                areaVersion.TryGetValue(propertyValue.GetHashCode(), out version);
            }
            return version;
        }

        /// <summary>
        /// 递增列表缓存全局版本
        /// </summary>
        public void IncreaseGlobalVersion()
        {
            this.globalVersion++;
        }

        /// <summary>
        /// 递增实体缓存（仅更新实体时需要递增）
        /// </summary>
        /// <param name="entityId">实体Id</param>
        public void IncreaseEntityCacheVersion(object entityId)
        {
            ICacheService cacheService = DIContainer.Resolve<ICacheService>();
            if (cacheService.EnableDistributedCache)
            {
                int entityVersion;
                if (entityVersionDictionary.TryGetValue(entityId, out entityVersion))
                    entityVersion++;
                else
                    entityVersion = 1;

                entityVersionDictionary[entityId] = entityVersion;

                OnChanged();
            }
        }

        /// <summary>
        /// 递增列表缓存version（增加、更改、删除实体时需要递增）
        /// </summary>
        /// <param name="entity">实体</param>
        public void IncreaseListCacheVersion(IEntity entity)
        {
            if (PropertiesOfArea != null)
            {
                foreach (var pi in PropertiesOfArea)
                {
                    object propertyValue = pi.GetValue(entity, null);
                    if (propertyValue != null)
                    {
                        //全部切换成小写
                        IncreaseAreaVersion(pi.Name.ToLower(), new object[] { propertyValue }, false);
                    }
                }
            }
            IncreaseGlobalVersion();

            OnChanged();
        }

        /// <summary>
        /// 递增列表缓存区域version
        /// </summary>
        /// <param name="propertyName">分区属性名称</param>
        /// <param name="propertyValue">分区属性值</param>
        public void IncreaseAreaVersion(string propertyName, object propertyValue)
        {
            if (propertyValue != null)
                IncreaseAreaVersion(propertyName, new object[] { propertyValue }, true);
        }

        /// <summary>
        /// 递增列表缓存区域version
        /// </summary>
        /// <param name="propertyName">分区属性名称</param>
        /// <param name="propertyValues">多个分区属性值</param>
        public void IncreaseAreaVersion(string propertyName, IEnumerable<object> propertyValues)
        {
            IncreaseAreaVersion(propertyName, propertyValues, true);
        }

        /// <summary>
        /// 递增列表缓存区域version
        /// </summary>
        /// <param name="propertyName">分区属性名称</param>
        /// <param name="propertyValues">多个分区属性值</param>
        /// <param name="raiseChangeEvent">是否触发Change事件</param>
        private void IncreaseAreaVersion(string propertyName, IEnumerable<object> propertyValues, bool raiseChangeEvent)
        {
            if (string.IsNullOrEmpty(propertyName))
                return;

            propertyName = propertyName.ToLower();

            int version = 0;
            ConcurrentDictionary<int, int> areaVersion;

            if (!areaVersionDictionary.TryGetValue(propertyName, out areaVersion))
            {
                areaVersionDictionary[propertyName] = new ConcurrentDictionary<int, int>();
                areaVersion = areaVersionDictionary[propertyName];
            }

            foreach (var propertyValue in propertyValues)
            {
                int propertyValueHashCode = propertyValue.GetHashCode();

                if (areaVersion.TryGetValue(propertyValueHashCode, out version))
                    version++;
                else
                    version = 1;

                areaVersion[propertyValueHashCode] = version;
            }

            if (raiseChangeEvent)
                OnChanged();
        }

        /// <summary>
        /// 标识为已删除
        /// </summary>
        public void MarkDeletion(IEntity entity)
        {
            ICacheService cacheService = DIContainer.Resolve<ICacheService>();
            if (EnableCache)
            {
                //更新列表缓存版本
                cacheService.MarkDeletion(GetCacheKeyOfEntity(entity.EntityId), entity, CachingExpirationType.SingleObject);
            }
        }

        #endregion



        #region GetCacheKey

        /// <summary>
        /// 获取实体缓存的cacheKey
        /// </summary>
        /// <param name="primaryKey">主键</param>
        /// <returns>实体的CacheKey</returns>
        public string GetCacheKeyOfEntity(object primaryKey)
        {
            ICacheService cacheService = DIContainer.Resolve<ICacheService>();
            if (cacheService.EnableDistributedCache)
                return TypeHashID + ":" + primaryKey + ":" + GetEntityVersion(primaryKey);
            else
                return TypeHashID + ":" + primaryKey;
        }

        /// <summary>
        /// 获取实体正文缓存的cacheKey
        /// </summary>
        /// <param name="primaryKey">主键</param>
        /// <returns>实体正文缓存的cacheKey</returns>
        public string GetCacheKeyOfEntityBody(object primaryKey)
        {
            ICacheService cacheService = DIContainer.Resolve<ICacheService>();
            if (cacheService.EnableDistributedCache)
                return TypeHashID + ":B-" + primaryKey + ":" + GetEntityVersion(primaryKey);
            else
                return TypeHashID + ":B-" + primaryKey;
        }

        /// <summary>
        /// 获取列表缓存CacheKey的前缀（例如：abe3ds2sa90:8:）
        /// </summary>
        /// <param name="cacheVersionSetting">列表缓存设置</param>
        /// <returns>列表缓存CacheKey的前缀</returns>
        public string GetListCacheKeyPrefix(IListCacheSetting cacheVersionSetting)
        {
            StringBuilder cacheKeyPrefix = new StringBuilder(TypeHashID);
            cacheKeyPrefix.Append("-L:");
            switch (cacheVersionSetting.CacheVersionType)
            {
                case CacheVersionType.GlobalVersion:
                    cacheKeyPrefix.AppendFormat("{0}:", GetGlobalVersion());
                    break;
                case CacheVersionType.AreaVersion:
                    cacheKeyPrefix.AppendFormat("{0}-{1}-{2}:", cacheVersionSetting.AreaCachePropertyName, cacheVersionSetting.AreaCachePropertyValue.ToString(), GetAreaVersion(cacheVersionSetting.AreaCachePropertyName, cacheVersionSetting.AreaCachePropertyValue));
                    break;
            }
            return cacheKeyPrefix.ToString();
        }

        /// <summary>
        /// 获取列表缓存CacheKey的前缀（例如：abe3ds2sa90:8:）
        /// </summary>
        /// <param name="cacheVersionType">列表缓存版本设置</param>
        /// <returns>列表缓存CacheKey的前缀</returns>
        public string GetListCacheKeyPrefix(CacheVersionType cacheVersionType)
        {
            return GetListCacheKeyPrefix(cacheVersionType, null, null);
        }

        /// <summary>
        /// 获取列表缓存CacheKey的前缀（例如：abe3ds2sa90:8:）
        /// </summary>
        /// <param name="cacheVersionType"></param>
        /// <param name="areaCachePropertyName">缓存分区名称</param>
        /// <param name="areaCachePropertyValue">缓存分区值</param>
        /// <returns></returns>
        public string GetListCacheKeyPrefix(CacheVersionType cacheVersionType, string areaCachePropertyName, object areaCachePropertyValue)
        {
            StringBuilder cacheKeyPrefix = new StringBuilder(TypeHashID);
            cacheKeyPrefix.Append("-L:");
            switch (cacheVersionType)
            {
                case CacheVersionType.GlobalVersion:
                    cacheKeyPrefix.AppendFormat("{0}:", GetGlobalVersion());
                    break;
                case CacheVersionType.AreaVersion:
                    cacheKeyPrefix.AppendFormat("{0}-{1}-{2}:", areaCachePropertyName, areaCachePropertyValue, GetAreaVersion(areaCachePropertyName, areaCachePropertyValue));
                    break;
            }
            return cacheKeyPrefix.ToString();
        }

        /// <summary>
        /// 获取CacheTimelinessHelper的CacheKey
        /// </summary>
        /// <returns>typeHashID对应类型的缓存设置CacheKey</returns>
        internal static string GetCacheKeyOfTimelinessHelper(string typeHashID)
        {
            return "CacheTimelinessHelper:" + typeHashID;
        }

        #endregion


        /// <summary>
        /// 对象变更时回调
        /// </summary>
        /// <remarks>
        /// 在分布式缓存情况，需要把缓存设置存储到缓存中
        /// </remarks>
        private void OnChanged()
        {
            ICacheService cacheService = DIContainer.Resolve<ICacheService>();
            if (cacheService.EnableDistributedCache)
            {
                //更新到分布式缓存
                cacheService.Set(GetCacheKeyOfTimelinessHelper(this.TypeHashID), this, CachingExpirationType.Invariable);
            }
        }
    }



}

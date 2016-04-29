//<TunynetCopyright>
//--------------------------------------------------------------
//<version>V0.5</verion>
//<createdate>2011-10-9</createdate>
//<author>mazq</author>
//<email>mazq@tunynet.com</email>
//<log date="2011-10-9" version="0.5">创建</log>
//<log date="2011-10-27" version="0.6" author="libsh">走查</log>
//--------------------------------------------------------------
//</TunynetCopyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Collections.Concurrent;
using Tunynet.Utilities;
using Tunynet.Caching;

namespace Tunynet
{
    /// <summary>
    /// 实体元数据
    /// </summary>
    [Serializable]
    public class EntityData
    {
        static ConcurrentDictionary<Type, EntityData> entityDatas = new ConcurrentDictionary<Type, EntityData>();

        /// <summary>
        /// 实体类型
        /// </summary>
        public Type Type { get; private set; }

        /// <summary>
        /// 类型的哈希值（16位md5）
        /// </summary>
        public string TypeHashID { get; private set; }

        private RealTimeCacheHelper realTimeCacheHelper = null;
        /// <summary>
        /// 实体缓存设置
        /// </summary>
        public RealTimeCacheHelper RealTimeCacheHelper
        {
            get
            {
                ICacheService cacheService = DIContainer.Resolve<ICacheService>();

                if (!cacheService.EnableDistributedCache)
                    return realTimeCacheHelper;
                else
                {
                    string cacheKey = RealTimeCacheHelper.GetCacheKeyOfTimelinessHelper(TypeHashID);
                    RealTimeCacheHelper rch = cacheService.GetFromFirstLevel<RealTimeCacheHelper>(cacheKey);
                    if (rch == null)
                    {
                        rch = ParseCacheTimelinessHelper(this.Type);
                        cacheService.Set(cacheKey, rch, CachingExpirationType.Invariable);
                    }
                    return rch;
                }
            }
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="t">实体类型</param>
        public EntityData(Type t)
        {
            Type = t;
            TypeHashID = EncryptionUtility.MD5_16(t.FullName);

            ICacheService cacheService = DIContainer.Resolve<ICacheService>();
            RealTimeCacheHelper rch = ParseCacheTimelinessHelper(t);
            if (cacheService.EnableDistributedCache)
                cacheService.Set(RealTimeCacheHelper.GetCacheKeyOfTimelinessHelper(TypeHashID), rch, CachingExpirationType.Invariable);
            else
                realTimeCacheHelper = rch;
        }

        /// <summary>
        /// 解析Type的CacheTimelinessHelper
        /// </summary>
        /// <param name="t">实体类型</param>
        /// <returns>实体缓存设置</returns>
        private RealTimeCacheHelper ParseCacheTimelinessHelper(Type t)
        {
            RealTimeCacheHelper rch = null;

            // Get CacheTimelinessHelper
            var a = t.GetCustomAttributes(typeof(CacheSettingAttribute), true);
            if (a.Length > 0)
            {
                CacheSettingAttribute csa = a[0] as CacheSettingAttribute;
                if (csa != null)
                {
                    rch = new RealTimeCacheHelper(csa.EnableCache, TypeHashID);
                    if (csa.EnableCache)
                    {
                        switch (csa.ExpirationPolicy)
                        {
                            case EntityCacheExpirationPolicies.Stable:
                                rch.CachingExpirationType = CachingExpirationType.Stable;
                                break;
                            case EntityCacheExpirationPolicies.Usual:
                                rch.CachingExpirationType = CachingExpirationType.UsualSingleObject;
                                break;
                            case EntityCacheExpirationPolicies.Normal:
                            default:
                                rch.CachingExpirationType = CachingExpirationType.SingleObject;
                                break;
                        }

                        List<PropertyInfo> propertyInfoOfArea = new List<PropertyInfo>();
                        if (!string.IsNullOrEmpty(csa.PropertyNamesOfArea))
                        {
                            string[] propertyNamesOfAreaArray = csa.PropertyNamesOfArea.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                            foreach (var pn in propertyNamesOfAreaArray)
                            {
                                var pi = t.GetProperty(pn);
                                if (pi != null)
                                    propertyInfoOfArea.Add(pi);
                            }
                        }
                        rch.PropertiesOfArea = propertyInfoOfArea;

                        if (!string.IsNullOrEmpty(csa.PropertyNameOfBody))
                        {
                            var pi = t.GetProperty(csa.PropertyNameOfBody);
                            rch.PropertyNameOfBody = pi;
                        }
                    }
                }
            }
            if (rch == null)
            {
                rch = new RealTimeCacheHelper(true, TypeHashID);
            }

            return rch;
        }


        private static readonly object lockObject = new object();

        /// <summary>
        /// 根据实体类型获取实体元数据
        /// </summary>
        /// <param name="t">实体类型</param>
        /// <returns>实体元数据</returns>
        public static EntityData ForType(Type t)
        {
            EntityData ed;

            if (!entityDatas.TryGetValue(t, out ed))
            {
                if (ed == null)
                {
                    //if (System.Threading.Monitor.TryEnter(lockObject, 10000))
                    //{
                    //    if (ed == null)
                    //    {
                    ed = new EntityData(t);
                    entityDatas[t] = ed;
                    //    }

                    //    System.Threading.Monitor.Exit(lockObject);
                    //}
                }
            }
            return ed;
        }

    }

}

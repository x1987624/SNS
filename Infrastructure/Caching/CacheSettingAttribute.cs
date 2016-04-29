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

namespace Tunynet.Caching
{
    /// <summary>
    /// 实体缓存设置属性
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class CacheSettingAttribute : Attribute
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="enableCache">是否使用缓存</param>
        public CacheSettingAttribute(bool enableCache)
        {
            EnableCache = enableCache;
        }

        /// <summary>
        /// 是否使用缓存
        /// </summary>
        public bool EnableCache { get; private set; }


        private EntityCacheExpirationPolicies expirationPolicy = EntityCacheExpirationPolicies.Normal;
        /// <summary>
        /// 缓存过期策略
        /// </summary>
        public EntityCacheExpirationPolicies ExpirationPolicy
        {
            get { return expirationPolicy; }
            set { expirationPolicy = value; }
        }

        /// <summary>
        /// 实体正文缓存对应的属性名称（如果不需单独存储实体正文缓存，则不要设置该属性）
        /// </summary>
        public string PropertyNameOfBody { get; set; }

        /// <summary>
        /// 缓存分区的属性名称（可以设置多个，用逗号分隔）
        /// </summary>
        /// <remarks>
        /// 必须是实体包含的属性，自动维护维护这些分区属性的版本号
        /// </remarks>
        public string PropertyNamesOfArea { get; set; }

    }


    /// <summary>
    /// 实体缓存期限类型
    /// </summary>
    public enum EntityCacheExpirationPolicies
    {
        /// <summary>
        /// 稳定数据      
        /// </summary>
        /// <remarks>
        /// 例如： Area/School
        /// </remarks>
        Stable = 1,

        /// <summary>
        /// 常用的单个实体
        /// </summary>
        /// <remarks>
        /// 例如： 用户、圈子
        /// </remarks>
        Usual = 3,

        /// <summary>
        /// 单个实体
        /// </summary>
        /// <remarks>
        /// 例如： 博文、帖子
        /// </remarks>
        Normal = 5,
    }


}

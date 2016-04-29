//<TunynetCopyright>
//--------------------------------------------------------------
//<version>V0.5</verion>
//<createdate>2011-10-9</createdate>
//<author>mazq</author>
//<email>mazq@tunynet.com</email>
//<log date="2011-10-9" version="0.5">创建</log>
//--------------------------------------------------------------
//</TunynetCopyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tunynet.Caching
{
    /// <summary>
    /// 缓存期限类型
    /// </summary>
    public enum CachingExpirationType
    {
        /// <summary>
        /// 永久不变的
        /// </summary>
        Invariable = 0,

        /// <summary>
        /// 稳定数据      
        /// </summary>
        /// <remarks>
        /// 例如： Resources.xml/Area/School
        /// </remarks>
        Stable = 1,

        /// <summary>
        /// 相对稳定
        /// </summary>
        /// <remarks>
        /// 例如：权限配置、审核配置
        /// </remarks>
        RelativelyStable = 2,

        /// <summary>
        /// 常用的单个对象
        /// </summary>
        /// <remarks>
        /// 例如： 用户、圈子
        /// </remarks>
        UsualSingleObject = 3,

        /// <summary>
        /// 常用的对象集合
        /// </summary>
        /// <remarks>
        ///  例如： 用户的朋友
        /// </remarks>
        UsualObjectCollection = 4,

        /// <summary>
        /// 单个对象
        /// </summary>
        /// <remarks>
        /// 例如： 博文、帖子
        /// </remarks>
        SingleObject = 5,

        /// <summary>
        /// 对象集合
        /// </summary>
        /// <remarks>
        /// 例如： 用于分页的私信数据
        /// </remarks>
        ObjectCollection = 6

    }
    
    /// <summary>
    /// 列表缓存版本设置
    /// </summary>
    public enum CacheVersionType
    {
        /// <summary>
        /// 不使用缓存版本
        /// </summary>
        None,

        /// <summary>
        /// 使用全局缓存版本
        /// </summary>
        GlobalVersion,

        /// <summary>
        /// 使用分区缓存版本
        /// </summary>
        AreaVersion
    }

}

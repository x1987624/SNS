//<TunynetCopyright>
//--------------------------------------------------------------
//<version>V0.5</verion>
//<createdate>2011-10-10</createdate>
//<author>mazq</author>
//<email>mazq@tunynet.com</email>
//<log date="2011-10-10" version="0.5">创建</log>
//--------------------------------------------------------------
//</TunynetCopyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tunynet.Caching
{
    /// <summary>
    /// 用于列表缓存设置接口
    /// </summary>
    /// <remarks>用于在查询对象中设置缓存策略</remarks>
    /// <include file='CacheExample.xml' path='doc/members/member[@name="T:Tunynet.Caching.IListCacheSetting"]/example'/>
    public interface IListCacheSetting
    {
        /// <summary>
        /// 列表缓存版本设置
        /// </summary>
        CacheVersionType CacheVersionType { get; }

        /// <summary>
        /// 缓存分区字段名称
        /// </summary>
        string AreaCachePropertyName { get; set; }

        /// <summary>
        /// 缓存分区字段值
        /// </summary>
        object AreaCachePropertyValue { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Spacebuilder.Wiki
{
    /// <summary>
    /// 词条排序依据
    /// </summary>
    public enum SortBy_WikiPage
    {
        /// <summary>
        /// 发布时间倒序
        /// </summary>
        DateCreated_Desc,

        /// <summary>
        /// 阶段浏览数
        /// </summary>
        StageHitTimes
    }
}
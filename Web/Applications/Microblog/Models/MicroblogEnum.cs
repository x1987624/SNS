//<TunynetCopyright>
//--------------------------------------------------------------
//<version>V0.5</verion>
//<createdate></createdate>
//<author></author>
//<email>@tunynet.com</email>
//<log date="" version="0.5">创建</log>
//--------------------------------------------------------------
//</TunynetCopyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tunynet.Repositories;
using Tunynet;

namespace Spacebuilder.Microblog
{
    /// <summary>
    /// 
    /// </summary>
    public enum SortBy_Microblog
    {
        /// <summary>
        /// 最新
        /// </summary>
        DateCreated,

        /// <summary>
        /// 热门转发
        /// </summary>
        ForwardedCount,

        /// <summary>
        /// 热门评论
        /// </summary>
        ReplyCount
    }


    /// <summary>
    /// 微博待审核、需再审核
    /// </summary>
    public enum MicroblogManageableCountType
    {
        /// <summary>
        /// 待审核
        /// </summary>
        Pending = 1,

        /// <summary>
        /// 需再审核
        /// </summary>
        Again = 2,

        /// <summary>
        /// 总微博数
        /// </summary>
        IsAll = 3,

        /// <summary>
        /// 24小时新增数
        /// </summary>
        IsLast24 = 4
    }
}

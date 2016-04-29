//<TunynetCopyright>
//--------------------------------------------------------------
//<version>V0.5</verion>
//<createdate></createdate>
//<author>libsh</author>
//<email>libsh@tunynet.com</email>
//<log date="2012-08-09" version="0.5">新建</log>
//--------------------------------------------------------------
//</TunynetCopyright>

using System;
using PetaPoco;
using Tunynet;
using Tunynet.Caching;
using Tunynet.Common;

namespace Spacebuilder.Microblog
{

    /// <summary>
    /// 封装后台管理用户时用于查询用户的条件
    /// </summary>
    public class MicroblogQuery
    {
        /// <summary>
        /// 微博内容关键字
        /// </summary>
        public string Keyword { get; set; }

        /// <summary>
        /// 用户Id
        /// </summary>
        public long? UserId { get; set; }

        /// <summary>
        ///租户类型Id
        /// </summary>
        public string TenantTypeId { get; set; }

        /// <summary>
        /// 拥有者Id
        /// </summary>
        public long? OwnerId { get; set; }

        /// <summary>
        /// 开始日期（用于注册时间条件）
        /// </summary>
        public DateTime? StartDate { get; set; }

        /// <summary>
        /// 结束日期（用于注册时间条件）
        /// </summary>
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// 附件媒体类型
        /// </summary>
        public MediaType? MediaType { get; set; }

        /// <summary>
        /// 是否为原创
        /// </summary>
        public bool? isOriginal { get; set; }
        
        //reply:已修改
        /// <summary>
        /// 审核状态
        /// </summary>
        public AuditStatus? AuditStatus { get; set; }


    }
}

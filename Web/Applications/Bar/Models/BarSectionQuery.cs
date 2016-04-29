//<TunynetCopyright>
//--------------------------------------------------------------
//<copyright>拓宇网络科技有限公司 2005-2012</copyright>
//<version>V0.5</verion>
//<createdate>2012-09-06</createdate>
//<author>zhengw</author>
//<email>zhengw@tunynet.com</email>
//<log date="2012-09-06" version="0.5">创建</log>
//--------------------------------------------------------------
//</TunynetCopyright>

using System;
using PetaPoco;
using Tunynet;
using Tunynet.Caching;
using Tunynet.Common;

namespace Spacebuilder.Bar
{

    /// <summary>
    /// 封装管理帖吧时用于查询帖吧的条件
    /// </summary>
    public class BarSectionQuery
    {
        /// <summary>
        /// 帖吧关键字
        /// </summary>
        public string NameKeyword { get; set; }

        /// <summary>
        /// 帖吧类别Id（包含后代子类别）
        /// </summary>
        public long? CategoryId { get; set; }

        /// <summary>
        /// 吧主Id
        /// </summary>
        public long? UserId { get; set; }

        /// <summary>
        /// 是否启用
        /// </summary>
        public bool? IsEnabled { get; set; }
        
        /// <summary>
        /// 审核状态
        /// </summary>
        public AuditStatus? AuditStatus { get; set; }
    }
}

//<TunynetCopyright>
//--------------------------------------------------------------
//<copyright>拓宇网络科技有限公司 2005-2012</copyright>
//<version>V0.5</verion>
//<createdate>2012-08-23</createdate>
//<author>zhengw</author>
//<email>zhengw@tunynet.com</email>
//<log date="2012-08-23" version="0.5">创建</log>
//--------------------------------------------------------------
//</TunynetCopyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Web.Routing;
using Tunynet.Common;
using Spacebuilder.Common;

namespace Spacebuilder.Bar
{
    /// <summary>
    /// 帖吧审核项
    /// </summary>
    public static class AuditItemKeysExtension
    {
        /// <summary>
        /// 申请帖吧审核项
        /// </summary>
        public static string Bar_Section(this AuditItemKeys auditItemKeys)
        {
            return "Bar_Section";
        }

        /// <summary>
        /// 发布帖子审核项
        /// </summary>
        public static string Bar_Thread(this AuditItemKeys auditItemKeys)
        {
            return "Bar_Thread";
        }

        /// <summary>
        /// 发布回帖审核项
        /// </summary>
        public static string Bar_Post(this AuditItemKeys auditItemKeys)
        {
            return "Bar_Post";
        }
    }

}

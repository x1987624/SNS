//<TunynetCopyright>
//--------------------------------------------------------------
//<version>V0.5</verion>
//<createdate>2012-8-17</createdate>
//<author>libsh</author>
//<email>libsh@tunynet.com</email>
//<log date="2012-8-17" version="0.5">创建</log>
//--------------------------------------------------------------
//</TunynetCopyright>

using Tunynet.Common;

namespace Spacebuilder.Microblog
{
    /// <summary>
    /// 微博审核项
    /// </summary>
    public static class AuditItemKeysExtension
    {
        /// <summary>
        /// 发布微博
        /// </summary>
        public static string CreateMicroblog(this AuditItemKeys auditItemKeys)
        {
            return "Microblog_Create";
        }

        /// <summary>
        /// 发表评论·
        /// </summary>
        public static string CreateComment(this AuditItemKeys auditItemKeys)
        {
            return "Microblog_Comment";
        }
    }

}

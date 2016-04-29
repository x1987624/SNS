//<TunynetCopyright>
//--------------------------------------------------------------
//<copyright>拓宇网络科技有限公司 2005-2012</copyright>
//<version>V0.5</verion>
//<createdate>2012-09-21</createdate>
//<author>mazq</author>
//<email>mazq@tunynet.com</email>
//<log date="2012-09-21" version="0.5">创建</log>
//--------------------------------------------------------------
//</TunynetCopyright>

using Tunynet.Common;

namespace Spacebuilder.Blog
{
    /// <summary>
    /// 日志审核项
    /// </summary>
    public static class AuditItemKeysExtension
    {
        /// <summary>
        /// 日志审核项
        /// </summary>
        public static string Blog_Thread(this AuditItemKeys auditItemKeys)
        {
            return "Blog_Thread";
        }

    }

}

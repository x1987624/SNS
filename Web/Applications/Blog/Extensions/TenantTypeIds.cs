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
    /// 日志定义的租户类型
    /// </summary>
    public static class TenantTypeIdsExtension
    {

        /// <summary>
        /// 日志应用
        /// </summary>
        public static string Blog(this TenantTypeIds TenantTypeIds)
        {
            return "100200";
        }

        /// <summary>
        /// 日志
        /// </summary>
        public static string BlogThread(this TenantTypeIds TenantTypeIds)
        {
            return "100201";
        }

    }
}
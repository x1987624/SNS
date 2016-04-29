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
    public static class TenantTypeIdsExtension
    {
        /// <summary>
        /// 微博
        /// </summary>
        /// <param name="TenantTypeIds">被扩展对象</param>
        public static string Microblog(this TenantTypeIds TenantTypeIds)
        {
            return "100101";
        }
    }

}

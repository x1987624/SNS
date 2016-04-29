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
    /// 微博动态项
    /// </summary>
    public static class ActivityItemKeysExtension
    {
        /// <summary>
        /// 发布微博动态项
        /// </summary>
        public static string CreateMicroblog(this ActivityItemKeys activityItemKeys)
        {
            return "CreateMicroblog";
        }
    }

}

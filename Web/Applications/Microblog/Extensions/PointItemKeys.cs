//<TunynetCopyright>
//--------------------------------------------------------------
//<copyright>拓宇网络科技有限公司 2005-2012</copyright>
//<version>V0.5</verion>
//<createdate>2013-03-27</createdate>
//<author>zhaok</author>
//<email>zhaok@tunynet.com</email>
//<log date="2013-03-27" version="0.5">创建</log>
//--------------------------------------------------------------
//</TunynetCopyright>

using Tunynet.Common;

namespace Spacebuilder.Microblog
{
    /// <summary>
    /// 积分项
    /// </summary>
    public static class PointItemKeysExtension
    {
        /// <summary>
        /// 创建微博
        /// </summary>
        public static string Microblog_CreateMicroblog(this PointItemKeys pointItemKeys)
        {
            return "Microblog_CreateMicroblog";
        }

        /// <summary>
        /// 删除微博
        /// </summary>
        public static string Microblog_DeleteMicroblog(this PointItemKeys pointItemKeys)
        {
            return "Microblog_DeleteMicroblog";
        }
    }
}

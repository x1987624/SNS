//<TunynetCopyright>
//--------------------------------------------------------------
//<copyright>拓宇网络科技有限公司 2005-2012</copyright>
//<version>V0.5</verion>
//<createdate>2012-10-16</createdate>
//<author>jiangshl</author>
//<email>jiangshl@tunynet.com</email>
//<log date="2012-10-16" version="0.5">创建</log>
//--------------------------------------------------------------
//</TunynetCopyright>

using Tunynet.Common;

namespace Spacebuilder.Blog
{
    /// <summary>
    /// 用户计数类型扩展类
    /// </summary>
    public static class OwnerDataKeysExtension
    {
        /// <summary>
        /// 日志数
        /// </summary>
        public static string ThreadCount(this OwnerDataKeys ownerDataKeys)
        {
            return ApplicationKeys.Instance().Blog() + "-ThreadCount";
        }

    }
}
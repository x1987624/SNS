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
    /// 日志计数类型扩展类
    /// </summary>
    public static class CountTypesExtension
    { 
        /// <summary>
        /// 被转载次数
        /// </summary>
        public static string ReproduceCount(this CountTypes countTypes)
        {
            return "ReproduceCount";
        }
    }
}
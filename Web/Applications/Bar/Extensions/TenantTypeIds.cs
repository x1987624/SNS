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

using Tunynet.Common;

namespace Spacebuilder.Bar
{
    public static class TenantTypeIdsExtension
    {

        /// <summary>
        /// 帖吧应用
        /// </summary>
        /// <param name="TenantTypeIds">被扩展对象</param>
        public static string Bar(this TenantTypeIds TenantTypeIds)
        {
            
            
            return "101200";
        }

        /// <summary>
        /// 帖吧
        /// </summary>
        /// <param name="TenantTypeIds">被扩展对象</param>
        public static string BarSection(this TenantTypeIds TenantTypeIds)
        {
            return "101201";
        }

        /// <summary>
        /// 帖子
        /// </summary>
        /// <param name="TenantTypeIds">被扩展对象</param>
        public static string BarThread(this TenantTypeIds TenantTypeIds)
        {
            return "101202";
        }

        /// <summary>
        /// 回帖
        /// </summary>
        /// <param name="TenantTypeIds">被扩展对象</param>
        public static string BarPost(this TenantTypeIds TenantTypeIds)
        {
            return "101203";
        }

        /// <summary>
        /// 评分
        /// </summary>
        /// <param name="TenantTypeIds"></param>
        /// <returns></returns>
        public static string BarRating(this TenantTypeIds TenantTypeIds)
        {
            return "101204";
        }
    }

}

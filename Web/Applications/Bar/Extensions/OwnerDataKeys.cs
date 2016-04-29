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

namespace Spacebuilder.Bar
{
    /// <summary>
    /// 拥有者计数类型扩展类
    /// </summary>
    public static class OwnerDataKeysExtension
    {
        /// <summary>
        /// 主题帖数
        /// </summary>
        public static string ThreadCount(this OwnerDataKeys ownerDataKeys)
        {
            return ApplicationKeys.Instance().Bar() + "-ThreadCount";
        }

        /// <summary>
        /// 回帖总数
        /// </summary>
        public static string PostCount(this OwnerDataKeys ownerDataKeys)
        {
            return ApplicationKeys.Instance().Bar() + "-PostCount";
        }
        /// <summary>
        /// 关注的帖吧数
        /// </summary>
        public static string FollowSectionCount(this OwnerDataKeys ownerDataKeys)
        {
            return ApplicationKeys.Instance().Bar() + "-FollowSectionCount";
        }
    }
}
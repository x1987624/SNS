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
    /// 计数类型扩展类
    /// </summary>
    public static class CountTypesExtension
    {
        /// <summary>
        /// 主题帖数
        /// </summary>
        public static string ThreadCount(this CountTypes countTypes)
        {
            return "ThreadCount";
        }

        /// <summary>
        /// 主题帖和回帖总数
        /// </summary>
        public static string ThreadAndPostCount(this CountTypes countTypes)
        {
            return "ThreadAndPostCount";
        }
        /// <summary>
        /// 被关注数
        /// </summary>
        public static string FollowedCount(this CountTypes countTypes)
        {
            return  "FollowedCount";
        }
    }

}

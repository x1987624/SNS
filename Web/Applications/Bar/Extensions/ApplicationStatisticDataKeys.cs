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
    /// 用户计数类型扩展类
    /// </summary>
    public static class ApplicationStatisticDataKeysExtension
    {
        /// <summary>
        /// 帖吧待审核数
        /// </summary>
        public static string SectionPendingCount(this ApplicationStatisticDataKeys applicationStatisticDataKeys)
        {
            return "SectionPendingCount";
        }

        /// <summary>
        /// 帖吧需再审核数
        /// </summary>
        public static string SectionAgainCount(this ApplicationStatisticDataKeys applicationStatisticDataKeys)
        {
            return "SectionAgainCount";
        }

        /// <summary>
        /// 回帖待审核数
        /// </summary>
        public static string PostPendingCount(this ApplicationStatisticDataKeys applicationStatisticDataKeys)
        {
            return "PostPendingCount";
        }

        /// <summary>
        /// 贴吧需再审核数
        /// </summary>
        public static string PostAgainCount(this ApplicationStatisticDataKeys applicationStatisticDataKeys)
        {
            return "PostAgainCount";
        }
    }
}
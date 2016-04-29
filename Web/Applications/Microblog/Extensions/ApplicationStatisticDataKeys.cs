//<TunynetCopyright>
//--------------------------------------------------------------
//<copyright>拓宇网络科技有限公司 2005-2012</copyright>
//<version>V0.5</verion>
//<createdate>2012-11-26</createdate>
//<author>cuiz</author>
//<email>cuiz@tunynet.com</email>
//<log date="2012-11-26" version="0.5">创建</log>
//--------------------------------------------------------------
//</TunynetCopyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Web.Routing;
using Tunynet.Common;

namespace Spacebuilder.Microblog
{
    /// <summary>
    /// 应用计数类型扩展类
    /// </summary>
    public static class ApplicationStatisticDataKeysExtension
    {
        /// <summary>
        /// 微博待审核数
        /// </summary>
        public static string MicroblogPendingCount(this ApplicationStatisticDataKeys applicationStatisticDataKeys)
        {
            return "MicroblogPendingCount";
        }

        /// <summary>
        /// 微博需再审核数
        /// </summary>
        public static string MicroblogAgainCount(this ApplicationStatisticDataKeys applicationStatisticDataKeys)
        {
            return "MicroblogAgainCount";
        }

    }
}
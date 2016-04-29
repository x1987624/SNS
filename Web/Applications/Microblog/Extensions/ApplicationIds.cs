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

using Tunynet.Common;
using Spacebuilder.Common;

namespace Spacebuilder.Microblog
{

    /// <summary>
    /// 应用Id
    /// </summary>
    public static class ApplicationIdsExtension
    {
        /// <summary>
        /// 
        /// </summary>
        public static int Microblog(this ApplicationIds applicationIds)
        {
            return 1001;
        }
    }

}

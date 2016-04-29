//<TunynetCopyright>
//--------------------------------------------------------------
//<copyright>拓宇网络科技有限公司 2005-2013</copyright>
//<version>V0.5</verion>
//<createdate>2013-05-10</createdate>
//<author>zhengw</author>
//<email>zhengw@tunynet.com</email>
//<log date="2013-05-10" version="0.5">创建</log>
//--------------------------------------------------------------
//</TunynetCopyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Tunynet.Common;
using Spacebuilder.Common;

namespace Spacebuilder.Blog
{

    /// <summary>
    /// 应用Id
    /// </summary>
    public static class ApplicationIdsExtension
    {
        /// <summary>
        /// 帖吧应用Id
        /// </summary>
        public static int Blog(this ApplicationIds applicationIds)
        {
            return 1002;
        }
    }

}

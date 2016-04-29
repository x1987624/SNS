//<TunynetCopyright>
//--------------------------------------------------------------
//<version>V0.5</verion>
//<createdate>2012-08-23</createdate>
//<author>libsh</author>
//<email>libsh@tunynet.com</email>
//<log date="2012-08-23" version="0.5">创建</log>
//--------------------------------------------------------------
//</TunynetCopyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Tunynet.Common;

namespace Spacebuilder.Microblog
{
    /// <summary>
    /// 拥有者计数类型扩展类
    /// </summary>
    public static class OwnerDataKeysExtension
    {
        /// <summary>
        /// 微博数
        /// </summary>
        public static string ThreadCount(this OwnerDataKeys ownerDataKeys)
        {
            return ApplicationKeys.Instance().Microblog() + "-ThreadCount";
        }
    }

}

//<TunynetCopyright>
//--------------------------------------------------------------
//<version>V0.5</verion>
//<createdate>2010-11-01</createdate>
//<author>mazq</author>
//<email>mazq@tunynet.com</email>
//<log date="2010-11-01" version="0.5">创建</log>
//--------------------------------------------------------------
//</TunynetCopyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tunynet
{
    /// <summary>
    /// 实体操作类型
    /// </summary>
    public enum EntityOperationType
    {
        /// <summary>
        /// 创建
        /// </summary>
        Create = 0,

        /// <summary>
        /// 更新
        /// </summary>
        Update = 1,

        /// <summary>
        /// 删除
        /// </summary>
        Delete = 2,

        /// <summary>
        /// 未知
        /// </summary>
        Unknown = 99

    }
}

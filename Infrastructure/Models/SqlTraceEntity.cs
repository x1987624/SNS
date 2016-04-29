//<TunynetCopyright>
//--------------------------------------------------------------
//<version>V0.5</verion>
//<createdate>2011-12-6</createdate>
//<author>libsh</author>
//<email>libsh@tunynet.com</email>
//<log date="2011-12-6" version="0.5">创建</log>
//--------------------------------------------------------------
//</TunynetCopyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Collections.Concurrent;
using Tunynet.Utilities;
using Tunynet.Caching;


namespace Tunynet
{
    /// <summary>
    /// 用于Sql语句跟踪的实体
    /// </summary>
    public class SqlTraceEntity
    {
        /// <summary>
        /// 跟踪到的Sql语句
        /// </summary>
        public string Sql { get; set; }

        /// <summary>
        /// 执行时间（毫秒）
        /// </summary>
        public long ElapsedMilliseconds { get; set; }

    }
}

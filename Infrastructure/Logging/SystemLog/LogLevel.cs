//<TunynetCopyright>
//--------------------------------------------------------------
//<version>V0.5</verion>
//<createdate>2012-02-28</createdate>
//<author>mazq</author>
//<email>mazq@tunynet.com</email>
//<log date="2012-02-28" version="0.5">创建</log>
//--------------------------------------------------------------
//</TunynetCopyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tunynet.Logging
{
    /// <summary>
    /// 日常级别
    /// </summary>
    public enum LogLevel
    {
        /// <summary>
        /// Debug
        /// </summary>
        Debug,

        /// <summary>
        /// Information
        /// </summary>
        Information,

        /// <summary>
        /// Warning
        /// </summary>
        Warning,

        /// <summary>
        /// Error
        /// </summary>
        Error,

        /// <summary>
        /// Fatal
        /// </summary>
        Fatal
    }
}

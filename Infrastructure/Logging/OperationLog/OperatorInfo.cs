//<TunynetCopyright>
//--------------------------------------------------------------
//<version>V0.5</verion>
//<createdate>2012-02-16</createdate>
//<author>mazq</author>
//<email>mazq@tunynet.com</email>
//<log date="2012-02-16" version="0.5">创建</log>
//<log date="2012-02-18" version="0.6" author="zhengw">走查</log>
//--------------------------------------------------------------
//</TunynetCopyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tunynet.Logging
{
    /// <summary>
    /// 操作者信息
    /// </summary>
    public class OperatorInfo
    {

        /// <summary>
        ///操作者UserId
        /// </summary>
        public long OperatorUserId { get; set; }

        /// <summary>
        ///操作者名称
        /// </summary>
        public string Operator { get; set; }

        /// <summary>
        ///操作者IP
        /// </summary>
        public string OperatorIP { get; set; }

        /// <summary>
        ///操作访问的url
        /// </summary>
        public string AccessUrl { get; set; }
    }
}
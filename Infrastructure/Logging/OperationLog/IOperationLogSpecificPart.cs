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
    /// 具体的操作日志信息接口
    /// </summary>
    public interface IOperationLogSpecificPart
    {
        /// <summary>
        ///应用Id
        /// </summary>
        int ApplicationId { get; set; }

        /// <summary>
        ///日志来源，一般为应用模块名称
        /// </summary>
        string Source { get; set; }

        /// <summary>
        ///操作类型标识
        /// </summary>
        string OperationType { get; set; }

        /// <summary>
        ///操作对象名称
        /// </summary>
        string OperationObjectName { get; set; }

        /// <summary>
        ///OperationObjectId
        /// </summary>
        long OperationObjectId { get; set; }

        /// <summary>
        ///操作描述
        /// </summary>
        string Description { get; set; }

    }
}

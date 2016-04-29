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
    /// OperationLog查询对象
    /// </summary>
    public class OperationLogQuery
    {

        /// <summary>
        /// 操作人（可以模糊搜索）
        /// </summary>
        public string Operator;

        /// <summary>
        /// 关键字
        /// </summary>
        /// <remarks>搜索操作对象</remarks>
        public string Keyword;

        /// <summary>
        ///操作者UserId
        /// </summary>
        public long? OperatorUserId { get; set; }

        /// <summary>
        /// 应用Id
        /// </summary>
        public int? ApplicationId;

        /// <summary>
        /// 操作类型
        /// </summary>
        public string OperationType;

        /// <summary>
        /// 开始时间
        /// </summary>
        public DateTime? StartDateTime;

        /// <summary>
        /// 截止时间
        /// </summary>
        public DateTime? EndDateTime;

        /// <summary>
        ///日志来源，一般为应用模块名称
        /// </summary>
        public string Source { get; set; }

    }
}

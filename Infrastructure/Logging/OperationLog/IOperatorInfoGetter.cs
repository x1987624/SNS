//<TunynetCopyright>
//--------------------------------------------------------------
//<version>V0.5</verion>
//<createdate>2012-02-17</createdate>
//<author>mazq</author>
//<email>mazq@tunynet.com</email>
//<log date="2012-02-17" version="0.5">创建</log>
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
    /// 当前操作者信息获取器
    /// </summary>
    public interface IOperatorInfoGetter
    {
        /// <summary>
        /// 获取当前操作者信息
        /// </summary>
        /// <returns>操作日志实体</returns>
        OperatorInfo GetOperatorInfo();
    }
}
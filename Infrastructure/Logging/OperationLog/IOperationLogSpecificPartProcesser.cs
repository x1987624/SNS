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
    /// 具体的操作日志信息转换接口
    /// </summary>
    /// <typeparam name="TEntity">实体类型</typeparam>
    public interface IOperationLogSpecificPartProcesser<TEntity>
    {

        /// <summary>
        /// 处理操作日志具体信息部分（把entity、eventOperationType转化成ISpecificOperationLogInformation）
        /// </summary>
        /// <param name="entity">日志操作对象</param>
        /// <param name="eventOperationType">操作类型</param>
        /// <param name="operationLogSpecificPart">具体的操作日志信息接口</param>
        void Process(TEntity entity, string eventOperationType, IOperationLogSpecificPart operationLogSpecificPart);

        /// <summary>
        /// 处理操作日志具体信息部分（把entity、eventOperationType、historyData转化成ISpecificOperationLogInformation）
        /// </summary>
        /// <param name="entity">日志操作对象</param>
        /// <param name="eventOperationType">操作类型</param>
        /// <param name="historyData">历史数据</param>
        /// <param name="operationLogSpecificPart">具体的操作日志信息接口</param>
        void Process(TEntity entity, string eventOperationType, TEntity historyData, IOperationLogSpecificPart operationLogSpecificPart);
    }
}

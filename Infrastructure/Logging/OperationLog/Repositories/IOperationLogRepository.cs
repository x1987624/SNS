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
using Tunynet.Repositories;

namespace Tunynet.Logging.Repositories
{
    /// <summary>
    /// OperationLog仓储接口
    /// </summary>
    public interface IOperationLogRepository : IRepository<OperationLogEntry>
    {
        /// <summary>
        /// 删除指定时间段内的日志列表
        /// </summary>
        /// <param name="startDate">开始日期</param>
        /// <param name="endDate">结束日期</param>
        int Clean(DateTime? startDate, DateTime? endDate);


        /// <summary>
        /// 根据DiscussQuestionQuery查询获取可分页的数据集合
        /// </summary>
        /// <param name="query">OperationLog查询对象</param>
        /// <param name="pageSize">每页记录数</param>
        /// <param name="pageIndex">当前页码(从1开始)</param>
        PagingDataSet<OperationLogEntry> GetLogs(OperationLogQuery query, int pageSize, int pageIndex);


    }
}

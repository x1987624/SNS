//<TunynetCopyright>
//--------------------------------------------------------------
//<version>V0.5</verion>
//<createdate>2012-02-16</createdate>
//<author>libsh</author>
//<email>libsh@tunynet.com</email>
//<log date="2012-02-16" version="0.5">创建</log>
//<log date="2012-02-18" version="0.51" author="mazq">走查</log>
//<log date="2012-02-18" version="0.6" author="zhengw">走查</log>
//--------------------------------------------------------------
//</TunynetCopyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tunynet.Repositories;
using PetaPoco;

namespace Tunynet.Logging.Repositories
{
    /// <summary>
    /// OperationLog仓储接口
    /// </summary>
    public class OperationLogRepository : Repository<OperationLogEntry>, IOperationLogRepository
    {
        /// <summary>
        /// 删除指定时间段内的日志列表
        /// </summary>
        /// <param name="startDate">开始日期</param>
        /// <param name="endDate">结束日期</param>
        public int Clean(DateTime? startDate, DateTime? endDate)
        {
            var sql = PetaPoco.Sql.Builder;
            sql.Append("delete from tn_OperationLogs");

            if (startDate.HasValue)
                sql.Where("DateCreated >= @0", startDate.Value);
            if (endDate.HasValue)
                sql.Where("DateCreated <= @0", endDate.Value);

            int result = CreateDAO().Execute(sql);

            return result;
        }

        /// <summary>
        /// 根据DiscussQuestionQuery查询获取可分页的数据集合
        /// </summary>
        /// <param name="query">OperationLog查询对象</param>
        /// <param name="pageSize">每页记录数</param>
        /// <param name="pageIndex">当前页码(从1开始)</param>
        public PagingDataSet<OperationLogEntry> GetLogs(OperationLogQuery query, int pageSize, int pageIndex)
        {
            var sql = PetaPoco.Sql.Builder;

            if (query.ApplicationId.HasValue)
                sql.Where("ApplicationId = @0", query.ApplicationId);
            if (!string.IsNullOrEmpty(query.Keyword))
                sql.Where("OperationObjectName like @0 or Description like @0", '%' + query.Keyword + '%');
            if (!string.IsNullOrEmpty(query.OperationType))
                sql.Where("OperationType = @0", query.OperationType);
            if (!string.IsNullOrEmpty(query.Operator))
                sql.Where("Operator like @0", "%" + query.Operator + "%");
            if (query.StartDateTime.HasValue)
                sql.Where("DateCreated >= @0", query.StartDateTime.Value);
            if (query.EndDateTime.HasValue)
                sql.Where("DateCreated <= @0", query.EndDateTime.Value);
            if (query.OperatorUserId.HasValue)
                sql.Where("OperatorUserId = @0", query.OperatorUserId.Value);
            if (!string.IsNullOrEmpty(query.Source))
                sql.Where("Source like @0", "%" + query.Source + "%");

            sql.OrderBy("Id desc");

            PagingEntityIdCollection peic = CreateDAO().FetchPagingPrimaryKeys<OperationLogEntry>(PrimaryMaxRecords, pageSize, pageIndex, sql);

            IEnumerable<OperationLogEntry> entitiesOfPage = PopulateEntitiesByEntityIds(peic.GetPagingEntityIds(pageSize, pageIndex));
            PagingDataSet<OperationLogEntry> pagingEntities = new PagingDataSet<OperationLogEntry>(entitiesOfPage)
            {
                PageIndex = pageIndex,
                PageSize = pageSize,
                TotalRecords = peic.TotalRecords
            };
            return pagingEntities;
        }
    }
}

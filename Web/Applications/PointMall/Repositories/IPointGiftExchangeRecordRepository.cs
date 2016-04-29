//------------------------------------------------------------------------------
// <copyright company="Tunynet">
//     Copyright (c) Tunynet Inc.  All rights reserved.
// </copyright> 
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Tunynet.Repositories;
using Tunynet;
using Tunynet.Common;
using Spacebuilder.Common;

namespace Spacebuilder.PointMall
{
    /// <summary>
    /// 积分商城应用的兑换记录仓储接口
    /// </summary>
    public interface IPointGiftExchangeRecordRepository : IRepository<PointGiftExchangeRecord>
    {

        PagingDataSet<PointGiftExchangeRecord> GetRecords(long? giftId, int pageSize, int pageIndex, ApproveStatus? approveStatus);

        PagingDataSet<PointGiftExchangeRecord> GetRecordsForAdmin(long? userId, ApproveStatus? approveStatus, DateTime? beginDate, DateTime endDate, int pageSize, int pageIndex);

        Dictionary<string, long> GetRecordApplicationStatisticData(string tenantTypeId);

        Dictionary<string, long> GetRecordManageableData(string tenantTypeId);

        PagingDataSet<PointGiftExchangeRecord> GetRecordsOfUser(long userId, DateTime beginDate, DateTime endDate, ApproveStatus? approveStatus, int pageSize, int pageIndex);

        void TakeOver(long userId, User takeOverUser);

        PagingDataSet<PointGiftExchangeRecord> GetRecordsCount(long giftId, ApproveStatus? approveStatus = null);
    }
}
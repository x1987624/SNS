//------------------------------------------------------------------------------
// <copyright company="Tunynet">
//     Copyright (c) Tunynet Inc.  All rights reserved.
// </copyright> 
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Text;
using PetaPoco;
using Spacebuilder.Common;
using Tunynet;
using Tunynet.Caching;
using Tunynet.Common;
using Tunynet.Repositories;

namespace Spacebuilder.PointMall
{
    /// <summary>
    /// 积分商城应用的兑换记录仓储实现
    /// </summary>
    public class PointGiftExchangeRecordRepository : Repository<PointGiftExchangeRecord>, IPointGiftExchangeRecordRepository
    {
        /// <summary>
        /// 获取申请统计数据
        /// </summary>
        /// <param name="tenantTypeId">租户类型</param>
        /// <returns>返回应用统计数据</returns>
        public Dictionary<string, long> GetRecordApplicationStatisticData(string tenantTypeId)
        {
            string cacheKey = string.Format("GetRecordApplicationStatisticData-{0}", tenantTypeId);
            Dictionary<string, long> statisticData = cacheService.Get<Dictionary<string, long>>(cacheKey);

            if (statisticData != null)
                return statisticData;

            statisticData = new Dictionary<string, long>();
            
            //总申请数
            Sql sql = Sql.Builder
                .Select("count(*)")
                .From("spb_PointGiftExchangeRecords");

            var dao = CreateDAO();
            dao.OpenSharedConnection();
            statisticData.Add(ApplicationStatisticDataKeys.Instance().TotalCount(), dao.FirstOrDefault<long>(sql));

            //24小时新增申请
            sql = Sql.Builder
                .Select("count(*)")
                .From("spb_PointGiftExchangeRecords")
                .Where("DateCreated > @0", DateTime.UtcNow.AddDays(-1));

            dao = CreateDAO();
            dao.OpenSharedConnection();
            statisticData.Add(ApplicationStatisticDataKeys.Instance().Last24HCount(), dao.FirstOrDefault<long>(sql));

            //获取总商品数
            sql = Sql.Builder
                .Select("count(*)")
                .From("spb_PointGifts");

            dao = CreateDAO();
            dao.OpenSharedConnection();
            statisticData.Add("TotalCountGifts", dao.FirstOrDefault<long>(sql));

            dao.CloseSharedConnection();

            cacheService.Set(cacheKey, statisticData, CachingExpirationType.SingleObject);

            return statisticData;
        }

        /// <summary>
        /// 获取申请管理数据
        /// </summary>
        /// <param name="tenantTypeId">租户类型</param>
        /// <returns>返回申请管理数据</returns>
        public Dictionary<string, long> GetRecordManageableData(string tenantTypeId)
        {
            Dictionary<string, long> dic = new Dictionary<string, long>();

            Sql sql = Sql.Builder
                .Select("count(*)")
                .From("spb_PointGiftExchangeRecords");

            sql.Where("Status = @0", ApproveStatus.Pending);

            var dao = CreateDAO();
            dao.OpenSharedConnection();
            dic.Add(ApplicationStatisticDataKeys.Instance().PendingCount(), dao.FirstOrDefault<long>(sql));

            dao.CloseSharedConnection();

            return dic;
        }

        #region 获取SQL
        /// <summary>
        /// 获取记录的Sql
        /// </summary>
        /// <param name="giftId">商品ID</param>
        /// <param name="userId">兑换人ID</param>
        /// <param name="approveStatus">申请状态</param>
        /// <param name="beginDate">开始日期</param>
        /// <param name="endDate">结束日期</param>
        /// <returns></returns>
        private Sql GetSql_GetRecords(long? giftId, long? userId, ApproveStatus? approveStatus,  DateTime? beginDate, DateTime? endDate)
        {
            Sql sql = Sql.Builder;
            Sql sql_Where = Sql.Builder;
            Sql sql_Orderby = Sql.Builder;

            sql.Select("spb_PointGiftExchangeRecords.*")
               .From("spb_PointGiftExchangeRecords");

            if (giftId.HasValue)
                sql_Where.Where("spb_PointGiftExchangeRecords.GiftId = @0", giftId);

            if (userId.HasValue&&userId!=0)
                sql_Where.Where("spb_PointGiftExchangeRecords.PayerUserId = @0", userId);

            if (approveStatus.HasValue)
                sql_Where.Where("spb_PointGiftExchangeRecords.Status = @0", approveStatus);

            if (beginDate.HasValue)
                sql_Where.Where("spb_PointGiftExchangeRecords.DateCreated>= @0 and spb_PointGiftExchangeRecords.DateCreated<=@1 ", beginDate, endDate);

            sql_Orderby.OrderBy("spb_PointGiftExchangeRecords.RecordId desc");

            sql.Append(sql_Where)
               .Append(sql_Orderby);
            return sql;
        }
        #endregion

        /// <summary>
        /// 获取评价（用于频道）
        /// </summary>
        /// <param name="giftId">商品ID</param>
        /// <param name="pageSize">分页大小</param>
        /// <param name="pageIndex">页码</param>
        /// <param name="approveStatus">审核状态</param>
        /// <returns></returns>
        public PagingDataSet<PointGiftExchangeRecord> GetRecords(long? giftId, int pageSize, int pageIndex, ApproveStatus? approveStatus=null)
        {
            

            return GetPagingEntities(pageSize, pageIndex, CachingExpirationType.UsualObjectCollection, () =>
            {
                //return string.Format("GetRecords::GiftId-{0}-approveStatus-{1}", giftId,approveStatus);
                StringBuilder cacheKey = new StringBuilder(RealTimeCacheHelper.GetListCacheKeyPrefix(CacheVersionType.AreaVersion, "GiftId", giftId));
                cacheKey.AppendFormat("GiftRecords::giftId-{0}:approveStatus-{1}", giftId,approveStatus);
                return cacheKey.ToString();
            }, () =>
            {   
                Sql sql = GetSql_GetRecords(giftId,null, approveStatus, null,null);
                return sql;
            });
        }

        /// <summary>
        /// 管理员获取兑换记录
        /// </summary>
        /// <param name="userId">用户Id</param>
        /// <param name="approveStatus">记录状态</param>
        /// <param name="beginDate">开始时间</param>
        /// <param name="endDate">结束时间</param>
        /// <param name="pageSize">分页大小</param>
        /// <param name="pageIndex">页码</param>
        /// <returns></returns>
        public PagingDataSet<PointGiftExchangeRecord> GetRecordsForAdmin(long? userId, ApproveStatus? approveStatus, DateTime? beginDate, DateTime endDate, int pageSize, int pageIndex)
        {
            Sql sql = GetSql_GetRecords(null,userId, approveStatus,beginDate,endDate);

            return GetPagingEntities(pageSize, pageIndex, sql);
        }

        /// <summary>
        /// 获取兑换记录(用于空间)
        /// </summary>
        /// <param name="userId">兑换人ID</param>
        /// <param name="beginDate">开始时间</param>
        /// <param name="endDate">结束时间</param>
        /// <param name="approveStatus">审核状态</param>
        /// <param name="pageSize">分页大小</param>
        /// <param name="pageIndex">页码</param>
        /// <returns></returns>
        public PagingDataSet<PointGiftExchangeRecord> GetRecordsOfUser(long userId, DateTime beginDate, DateTime endDate, ApproveStatus? approveStatus, int pageSize, int pageIndex)
        {
            Sql sql = GetSql_GetRecords(null, userId, approveStatus, beginDate, endDate);
            return GetPagingEntities(pageSize, pageIndex, CachingExpirationType.ObjectCollection, () =>
            {
                string userIdCacheKey = RealTimeCacheHelper.GetListCacheKeyPrefix(CacheVersionType.AreaVersion, "PayerUserId", userId);
                return string.Format("GetUserRecords::PayerUserId-{0};ApproveStatus-{1},BeginDate-{2},EndDate-{3}", userIdCacheKey, approveStatus, beginDate, endDate);
            }, () =>
            {
                return sql;
            });
        }

        /// <summary>
        /// 删除用户时处理其兑换申请
        /// </summary>
        /// <remarks>
        /// 1.如果需要接管，直接执行Repository的方法更新数据库记录
        /// 2.如果不需要接管，删除申请记录
        /// </remarks>
        /// <param name="userId">用户ID</param>
        /// <param name="takeOverUser">指定接管用户的用户名</param>
        public void TakeOver(long userId, User takeOverUser)
        {
            Sql sql = new Sql();
            sql.Append("update spb_PointGiftExchangeRecords set PayerUserId = @0,Payer = @1 where PayerUserId = @2", takeOverUser.UserId, takeOverUser.DisplayName, userId);
            CreateDAO().Execute(sql);
          
        }

        public PagingDataSet<PointGiftExchangeRecord> GetRecordsCount(long giftId, ApproveStatus? approveStatus = null)
        {
            Sql sql = Sql.Builder;
            sql.Select("RecordId")
               .From("spb_PointGiftExchangeRecords")
               .Where("GiftId=@0",giftId);
            if (approveStatus.HasValue)
            {
                sql.Where("Status=@0",approveStatus);
            }
            
          return GetPagingEntities(20,1,sql);
        }
    }
}
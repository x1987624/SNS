//------------------------------------------------------------------------------
// <copyright company="Tunynet">
//     Copyright (c) Tunynet Inc.  All rights reserved.
// </copyright> 
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PetaPoco;
using Tunynet;
using Tunynet.Caching;
using Tunynet.Common;
using Tunynet.Repositories;
using Tunynet.Utilities;
using Spacebuilder.Common;

namespace Spacebuilder.Photo
{
    /// <summary>
    /// 相册应用的相册仓储实现
    /// </summary>
    public class AlbumRepository : Repository<Album>, IAlbumRepository
    {
        /// <summary>
        /// 构造器
        /// </summary>
        public AlbumRepository()
        {

        }

        /// <summary>
        /// 构造器
        /// </summary>
        /// <param name="publiclyAuditStatus"></param>
        public AlbumRepository(PubliclyAuditStatus publiclyAuditStatus)
        {
            this.publiclyAuditStatus = publiclyAuditStatus;
        }


        /// <summary>
        /// 删除用户时，指定用户接管当前用户的相册以及相册下照片
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="takeOverUser"></param>
        public void TakeOver(long userId, User takeOverUser)
        {
            List<Sql> sqls = new List<Sql>();
            //todo:bianchx by jiangshl,sql里没有!=，应该是<>

            sqls.Add(Sql.Builder.Append("update spb_Albums set UserId = @0,Author = @1,OwnerId=@2 where OwnerId = @3 and TenantTypeId = @4", takeOverUser.UserId, takeOverUser.DisplayName, takeOverUser.UserId, userId, TenantTypeIds.Instance().User()));
            sqls.Add(Sql.Builder.Append("update spb_Photos set UserId = @0,Author = @1,OwnerId=@2 where OwnerId = @3 and TenantTypeId = @4", takeOverUser.UserId, takeOverUser.DisplayName, takeOverUser.UserId, userId, TenantTypeIds.Instance().User()));
            sqls.Add(Sql.Builder.Append("update spb_Albums set UserId = @0,Author = @1 where UserId = @2 and TenantTypeId <> @3", takeOverUser.UserId, takeOverUser.DisplayName, userId, TenantTypeIds.Instance().User()));
            sqls.Add(Sql.Builder.Append("update spb_Photos set UserId = @0,Author = @1 where UserId = @2 and TenantTypeId <> @3", takeOverUser.UserId, takeOverUser.DisplayName, userId, TenantTypeIds.Instance().User()));
            CreateDAO().Execute(sqls);
        }

        /// <summary>
        /// 获取相册列表（用于频道）
        /// </summary>
        /// <remarks>
        /// 缓存周期：常用对象集合，不用维护即时性
        /// </remarks>
        /// <param name="tenantTypeId">租户类型</param>
        /// <param name="ownerId">所有者ID</param>
        /// <param name="sortBy">排序依据</param>
        /// <param name="pageSize">分页大小</param>
        /// <param name="pageIndex">页码</param>
        /// <returns>返回相册分页列表</returns>
        public PagingDataSet<Album> GetAlbums(string tenantTypeId, long? ownerId, SortBy_Album sortBy, int pageSize, int pageIndex)
        {
            return GetPagingEntities(pageSize, pageIndex, CachingExpirationType.UsualObjectCollection, () =>
            {
                return string.Format("GetAlbums::TenantTypeId-{0};OwnerId-{1};SortBy-{2}", tenantTypeId, ownerId ?? 0, sortBy);
            }, () =>
            {
                return GetSql_GetAlbums(tenantTypeId, ownerId, null, false, null, null, sortBy);
            });
        }

        /// <summary>
        /// 获取相册列表（用于用户空间）
        /// </summary>
        /// <remarks>
        /// 缓存周期：对象集合，使用分区缓存
        /// </remarks>
        /// <param name="tenantTypeId">租户类型</param>
        /// <param name="userId">用户ID</param>
        /// <param name="ignoreAuditAndPrivacy">是否忽略审核状态和隐私，相册主人查看时为true</param>
        /// <param name="sortBy">排序依据</param>
        /// <param name="pageSize">分页大小</param>
        /// <param name="pageIndex">页码</param>
        /// <returns>返回相册分页列表</returns>
        public PagingDataSet<Album> GetUserAlbums(string tenantTypeId, long userId, bool ignoreAuditAndPrivacy, SortBy_Album sortBy, int pageSize, int pageIndex)
        {
            return GetPagingEntities(pageSize, pageIndex, CachingExpirationType.ObjectCollection, () =>
            {
                return string.Format("GetUserAlbums::TenantTypeId-{0};UserId-{1};IgnoreAuditAndPrivacy-{2};SortBy-{3}", tenantTypeId, RealTimeCacheHelper.GetListCacheKeyPrefix(CacheVersionType.AreaVersion, "UserId", userId), ignoreAuditAndPrivacy, sortBy);
            }, () =>
            {
                return GetSql_GetAlbums(tenantTypeId, null, userId, ignoreAuditAndPrivacy, null, null, sortBy);
            });
        }

        /// <summary>
        /// 获取相册列表（用于群组）
        /// </summary>
        /// <remarks>
        /// 缓存周期：对象集合，使用分区缓存
        /// </remarks>
        /// <param name="tenantTypeId">租户类型</param>
        /// <param name="ownerId">所有者ID</param>
        /// <param name="ignoreAuditAndPrivacy">是否忽略审核状态和隐私，相册主人查看时为true</param>
        /// <param name="sortBy">排序依据</param>
        /// <param name="pageSize">分页大小</param>
        /// <param name="pageIndex">页码</param>
        /// <returns>返回相册分页列表</returns>
        public PagingDataSet<Album> GetOwnerAlbums(string tenantTypeId, long ownerId, bool ignoreAuditAndPrivacy, SortBy_Album sortBy, int pageSize, int pageIndex)
        {
            return GetPagingEntities(pageSize, pageIndex, CachingExpirationType.ObjectCollection, () =>
            {
                return string.Format("GetOwnerAlbums::TenantTypeId-{0};OwnerId-{1};IgnoreAuditAndPrivacy-{2};SortBy_Album-{3}", tenantTypeId, RealTimeCacheHelper.GetListCacheKeyPrefix(CacheVersionType.AreaVersion, "OwnerId", ownerId), ignoreAuditAndPrivacy, sortBy);
            }, () =>
            {
                return GetSql_GetAlbums(tenantTypeId, ownerId, null, ignoreAuditAndPrivacy, null, null, sortBy);
            });
        }

        /// <summary>
        /// 管理员获取相册数据
        /// </summary>
        /// <remarks>无需缓存</remarks>
        /// <param name="tenantTypeId">租户类型</param>
        /// <param name="nameKeyword">相册名称</param>
        /// <param name="userId">作者ID</param>
        /// <param name="ownerId">所有者ID</param>
        /// <param name="auditStatus">审核状态</param>
        /// <param name="pageSize">分页大小</param>
        /// <param name="pageIndex">页码</param>
        /// <returns></returns>
        public PagingDataSet<Album> GetAlbumsForAdmin(string tenantTypeId, string nameKeyword, long? userId, long? ownerId, AuditStatus? auditStatus, int pageSize, int pageIndex)
        {
            Sql sql = GetSql_GetAlbums(tenantTypeId, ownerId, userId, null, nameKeyword, auditStatus, SortBy_Album.DateCreated_Desc);
            return GetPagingEntities(pageSize, pageIndex, sql);
        }

        /// <summary>
        /// 获取相册统计数据
        /// </summary>
        /// <param name="tenantTypeId">租户类型（可以获取该应用下针对某种租户类型的统计计数，默认不进行筛选）</param>
        /// <returns>返回相册统计数据</returns>
        public Dictionary<string, long> GetAlbumApplicationStatisticData(string tenantTypeId)
        {
            string cacheKey = string.Format("GetAlbumApplicationStatisticData::TenantTypeId-{0}", tenantTypeId);

            Dictionary<string, long> statisticData = cacheService.Get<Dictionary<string, long>>(cacheKey);

            if (statisticData != null)
                return statisticData;

            statisticData = new Dictionary<string, long>();

            var dao = CreateDAO();


            Sql sql_Select = Sql.Builder;
            sql_Select.Select("Count(*)")
                .From("spb_Albums");
            if (!string.IsNullOrEmpty(tenantTypeId))
                sql_Select.Where("TenantTypeId = @0", tenantTypeId);


            dao.OpenSharedConnection();

            statisticData.Add(ApplicationStatisticDataKeys.Instance().TotalCount(), dao.FirstOrDefault<long>(sql_Select));


            sql_Select = Sql.Builder;
            sql_Select.Select("Count(*)")
                .From("spb_Albums");
            if (!string.IsNullOrEmpty(tenantTypeId))
                sql_Select.Where("TenantTypeId = @0", tenantTypeId);
            sql_Select.Where("DateCreated > @0", DateTime.UtcNow.AddDays(-1).ToString());

            statisticData.Add(ApplicationStatisticDataKeys.Instance().Last24HCount(), dao.FirstOrDefault<long>(sql_Select));

            dao.CloseSharedConnection();

            cacheService.Set(cacheKey, statisticData, CachingExpirationType.SingleObject);

            return statisticData;
        }

        /// <summary>
        /// 获取相册管理数据
        /// </summary>
        /// <param name="tenantTypeId">租户类型（可以获取该应用下针对某种租户类型的统计计数，默认不进行筛选）</param>
        /// <returns>返回相册管理数据</returns>
        public Dictionary<string, long> GetAlbumManageableData(string tenantTypeId)
        {
            Dictionary<string, long> manageableData = new Dictionary<string, long>();
            string cacheKey = string.Format("GetAlbumManageableData::TenantTypeId-{0}", tenantTypeId);

            manageableData = cacheService.Get<Dictionary<string, long>>(cacheKey);
            if (manageableData != null)
                return manageableData;

            var dao = CreateDAO();

            manageableData = new Dictionary<string, long>();


            Sql sql = Sql.Builder
               .Select("Count(*)")
               .From("spb_Albums");

            if (!string.IsNullOrEmpty(tenantTypeId))
                sql.Where("TenantTypeId = @0", tenantTypeId);

            sql.Where("AuditStatus = @0", AuditStatus.Pending);
            dao.OpenSharedConnection();
            manageableData.Add(ApplicationStatisticDataKeys.Instance().PendingCount(), dao.FirstOrDefault<long>(sql));

            sql = Sql.Builder
                .Select("Count(*)")
               .From("spb_Albums");

            if (!string.IsNullOrEmpty(tenantTypeId))
                sql.Where("TenantTypeId = @0", tenantTypeId);
            sql.Where("AuditStatus = @0", AuditStatus.Again);
            manageableData.Add(ApplicationStatisticDataKeys.Instance().AgainCount(), dao.FirstOrDefault<long>(sql));
            dao.CloseSharedConnection();

            cacheService.Set(cacheKey, manageableData, CachingExpirationType.SingleObject);

            return manageableData;
        }


        #region private items

        /// <summary>
        /// 获取Sql
        /// </summary>
        /// <param name="tenantTypeId"></param>
        /// <param name="ownerId"></param>
        /// <param name="ignoreAuditAndPrivacy"></param>
        /// <param name="userId"></param>
        /// <param name="nameKeyword"></param>
        /// <param name="auditStatus"></param>
        /// <param name="sortBy"></param>
        /// <returns></returns>
        private Sql GetSql_GetAlbums(string tenantTypeId, long? ownerId, long? userId, bool? ignoreAuditAndPrivacy, string nameKeyword, AuditStatus? auditStatus, SortBy_Album sortBy = SortBy_Album.DateCreated_Desc)
        {
            Sql sql = Sql.Builder
                .Select("*")
                .From("spb_Albums");
            Sql sql_Where = Sql.Builder;
            Sql sql_OrderBy = Sql.Builder;

            if (!string.IsNullOrEmpty(tenantTypeId))
                sql_Where.Where("TenantTypeId = @0", tenantTypeId);

            if (ownerId.HasValue)
                sql_Where.Where("OwnerId = @0", ownerId);

            if (ignoreAuditAndPrivacy.HasValue && !ignoreAuditAndPrivacy.Value)
            {
                sql_Where.Where("PrivacyStatus <> @0", PrivacyStatus.Private);
                switch (this.PubliclyAuditStatus)
                {
                    case PubliclyAuditStatus.Again:
                    case PubliclyAuditStatus.Fail:
                    case PubliclyAuditStatus.Pending:
                    case PubliclyAuditStatus.Success:
                        sql_Where.Where("AuditStatus = @0", this.PubliclyAuditStatus);
                        break;
                    case PubliclyAuditStatus.Pending_GreaterThanOrEqual:
                    case PubliclyAuditStatus.Again_GreaterThanOrEqual:
                        sql_Where.Where("AuditStatus > @0", this.PubliclyAuditStatus);
                        break;
                }
            }

            if (userId.HasValue)
                sql_Where.Where("UserId = @0", userId);

            if (!string.IsNullOrEmpty(nameKeyword))
                sql_Where.Where("AlbumName like @0", "%" + StringUtility.StripSQLInjection(nameKeyword) + "%");

            if (auditStatus.HasValue)
                sql_Where.Where("AuditStatus = @0", auditStatus);

            switch (sortBy)
            {
                case SortBy_Album.DateCreated_Desc:
                    sql_OrderBy.OrderBy("AlbumId desc");
                    break;
                case SortBy_Album.DateCreated_Asc:
                    sql_OrderBy.OrderBy("AlbumId asc");
                    break;
                case SortBy_Album.LastUploadDate_Desc:
                    sql_OrderBy.OrderBy("LastUploadDate desc");
                    break;
                case SortBy_Album.LastUploadDate_Asc:
                    sql_OrderBy.OrderBy("LastUploadDate asc");
                    break;
                case SortBy_Album.DisplayOrder:
                    sql_OrderBy.OrderBy("DisplayOrder");
                    break;
            }

            return sql.Append(sql_Where).Append(sql_OrderBy);
        }

        /// <summary>
        /// 可对外显示的审核状态
        /// </summary>
        private PubliclyAuditStatus? publiclyAuditStatus;
        /// <summary>
        /// 可对外显示的审核状态
        /// </summary>
        protected PubliclyAuditStatus PubliclyAuditStatus
        {
            get
            {
                if (publiclyAuditStatus == null)
                {
                    AuditService auditService = new AuditService();
                    publiclyAuditStatus = auditService.GetPubliclyAuditStatus(PhotoConfig.Instance().ApplicationId);
                }
                return publiclyAuditStatus.Value;
            }
            set
            {
                this.publiclyAuditStatus = value;
            }
        }

        #endregion
    }
}
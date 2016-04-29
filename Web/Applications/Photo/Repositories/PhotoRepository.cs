//------------------------------------------------------------------------------
// <copyright company="Tunynet">
//     Copyright (c) Tunynet Inc.  All rights reserved.
// </copyright> 
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Text;
using PetaPoco;
using Tunynet;
using Tunynet.Caching;
using Tunynet.Common;
using Tunynet.Repositories;
using Tunynet.Utilities;
using Spacebuilder.Common;
using System.Linq;

namespace Spacebuilder.Photo
{
    /// <summary>
    /// 相册应用的照片仓储实现
    /// </summary>
    public class PhotoRepository : Repository<Photo>, IPhotoRepository
    {
        /// <summary>
        /// 构造器
        /// </summary>
        public PhotoRepository()
        {

        }

        /// <summary>
        /// 构造器
        /// </summary>
        /// <param name="publiclyAuditStatus"></param>
        public PhotoRepository(PubliclyAuditStatus publiclyAuditStatus)
        {
            this.publiclyAuditStatus = publiclyAuditStatus;
        }


        /// <summary>
        /// 移动相片
        /// </summary>
        /// <param name="photo">被移动的照片</param>
        /// <param name="targetAlbum">接收的相册</param>
        /// <returns>是否移动成功</returns>
        public bool MovePhoto(Photo photo, Album targetAlbum)
        {
            AlbumRepository albumRepository = new AlbumRepository();

            if (photo.UserId != targetAlbum.UserId || photo.AlbumId == targetAlbum.AlbumId)
            {
                return false;
            }

            photo.Album.PhotoCount--;

            //更新原相册数据
            if (photo.Album.CoverId == photo.PhotoId)
            {
                photo.Album.CoverId = 0;
            }

            albumRepository.Update(photo.Album);

            RealTimeCacheHelper.IncreaseAreaVersion("AlbumId", photo.AlbumId);

            //更新照片所属相册以及继承属性
            photo.AlbumId = targetAlbum.AlbumId;
            photo.TenantTypeId = targetAlbum.TenantTypeId;
            photo.OwnerId = targetAlbum.OwnerId;
            photo.PrivacyStatus = targetAlbum.PrivacyStatus;
            Update(photo);


            //更新目标相册
            targetAlbum.PhotoCount++;
            albumRepository.Update(targetAlbum);
            return true;
        }

        /// <summary>
        /// 相册隐私状态更改，同步更改相册下隐私状态
        /// </summary>
        /// <param name="albumId"></param>
        public void UpdatePrivacyStatus(long albumId)
        {
            var dao = CreateDAO();

            dao.OpenSharedConnection();

            Album album = new PhotoService().GetAlbum(albumId);
            if (album != null)
            {
                Sql sql = Sql.Builder
                    .Append("update spb_Photos set PrivacyStatus = @0 where spb_Photos.AlbumId = @1", album.PrivacyStatus, albumId);
                dao.Execute(sql);
            }

            Sql sql_Select = Sql.Builder
                .Select("PhotoId")
                .From("spb_Photos");

            IEnumerable<object> photoIds = dao.FetchTopPrimaryKeys<Photo>(int.MaxValue, sql_Select);

            dao.CloseSharedConnection();

            foreach (var item in photoIds)
            {
                long id = 0;
                if (long.TryParse(item.ToString(), out id))
                {
                    RealTimeCacheHelper.IncreaseEntityCacheVersion(id);

                    //处理掉本机缓存
                    string cacheKeyOfEntity = RealTimeCacheHelper.GetCacheKeyOfEntity(id);
                    Photo photo = cacheService.Get<Photo>(cacheKeyOfEntity);
                    if (photo != null)
                        photo.IsEssential = true;
                }
            }

        }

        /// <summary>
        /// 设置精华
        /// </summary>
        /// <param name="photoId">照片对象</param>
        /// <param name="isEssential">是否精华</param>
        public void SetEssential(long photoId, bool isEssential)
        {
            Sql sql = Sql.Builder
                .Append("update spb_Photos set IsEssential = @0", isEssential)
                .Where("PhotoId = @0", photoId);
            CreateDAO().Execute(sql);

            Photo photo = Get(photoId);
            if (photo != null)
            {
                photo.IsEssential = isEssential;
                base.OnUpdated(photo);
            }
        }

        /// <summary>
        /// 获取照片列表（用于频道页面）
        /// </summary>
        /// <remarks>
        /// 缓存周期：常用对象集合，不用维护即时性
        /// </remarks>
        /// <param name="tenantTypeId">租户类型</param>
        /// <param name="tagName">标签</param>
        /// <param name="isEssential">是否精华</param>
        /// <param name="sortBy">排序依据</param>
        /// <param name="createDatetime">动态所需时间</param>
        /// <param name="pageSize">分页大小</param>
        /// <param name="pageIndex">页码</param>
        /// <returns>返回照片分页列表</returns>
        public PagingDataSet<Photo> GetPhotos(string tenantTypeId, string tagName, bool? isEssential, SortBy_Photo sortBy, DateTime? createDateTime, int pageSize, int pageIndex)
        {
            Sql sql = GetSql_GetPhotos(tenantTypeId, string.Empty, null, null, isEssential, createDateTime, null, tagName, false, null, sortBy);

            return GetPagingEntities(pageSize, pageIndex, CachingExpirationType.UsualObjectCollection, () =>
            {
                return string.Format("GetPhotos::TenantTypeId-{0};TagName-{1};SortBy_Photo-{2};isEssential-{3}", tenantTypeId, tagName, sortBy, isEssential);
            }, () =>
            {
                return sql;
            });
        }

        /// <summary>
        /// 获取用户照片列表（用于用户空间）
        /// </summary>
        /// <remarks>
        /// 缓存周期：对象集合，使用分区缓存
        /// </remarks>
        /// <param name="tenantTypeId">租户类型</param>
        /// <param name="userId">作者ID</param>
        /// <param name="ignoreAuditAndPrivacy">是否忽略审核状态和隐私，相册主人查看时为true</param>
        /// <param name="tagName">标签</param>
        /// <param name="albumId">相册ID</param>
        /// <param name="isEssential">是否精华</param>
        /// <param name="sortBy">排序依据</param>
        /// <param name="pageSize">分页大小</param>
        /// <param name="pageIndex">页码</param>
        /// <returns>返回照片分页列表</returns>
        public PagingDataSet<Photo> GetUserPhotos(string tenantTypeId, long? userId, bool ignoreAuditAndPrivacy, string tagName, long? albumId, bool? isEssential, SortBy_Photo sortBy, DateTime? createDateTime, int pageSize, int pageIndex)
        {
            return GetPagingEntities(pageSize, pageIndex, CachingExpirationType.ObjectCollection, () =>
            {
                string userIdCacheKey = userId.HasValue ? RealTimeCacheHelper.GetListCacheKeyPrefix(CacheVersionType.AreaVersion, "UserId", userId) : "null";
                string albumIdCacheKey = albumId.HasValue ? RealTimeCacheHelper.GetListCacheKeyPrefix(CacheVersionType.AreaVersion, "AlbumId", albumId.Value) : "null";
                return string.Format("GetUserPhotos::TenantTypeId-{0};UserId-{1};IgnoreAuditAndPrivacy-{2};TagName-{3};AlbumId-{4};IsEssential-{5};SortBy_Photo-{6}", tenantTypeId, userIdCacheKey, ignoreAuditAndPrivacy, tagName, albumIdCacheKey, isEssential, sortBy);
            }, () =>
            {
                return GetSql_GetPhotos(tenantTypeId, string.Empty, userId, null, isEssential, null, null, tagName, ignoreAuditAndPrivacy, albumId, sortBy);
            });
        }

        /// <summary>
        /// 获取所有者照片列表（用于群组）
        /// </summary>
        /// <remarks>
        /// 缓存周期：对象集合，使用分区缓存
        /// </remarks>
        /// <param name="tenantTypeId">租户类型</param>
        /// <param name="ownerId">拥有者ID</param>
        /// <param name="ignoreAuditAndPrivacy">是否忽略审核状态和隐私，相册主人查看时为true</param>
        /// <param name="tagName">标签</param>
        /// <param name="albumId">相册ID</param>
        /// <param name="isEssential">是否精华</param>
        /// <param name="sortBy">排序依据</param>
        /// <param name="pageSize">分页大小</param>
        /// <param name="pageIndex">页码</param>
        /// <returns>返回照片分页列表</returns>
        public PagingDataSet<Photo> GetOwnerPhotos(string tenantTypeId, long ownerId, bool ignoreAuditAndPrivacy, string tagName, long? albumId, bool? isEssential, SortBy_Photo sortBy, int pageSize, int pageIndex)
        {
            return GetPagingEntities(pageSize, pageIndex, CachingExpirationType.ObjectCollection, () =>
            {
                string ownerIdCacheKey = RealTimeCacheHelper.GetListCacheKeyPrefix(CacheVersionType.AreaVersion, "OwnerId", ownerId);
                return string.Format("GetUserPhotos::TenantTypeId-{0};IgnoreAuditAndPrivacy-{1};TagName-{2};AlbumId-{3};IsEssential-{4};SortBy_Photo-{5};OwnerId-{6}", tenantTypeId, ignoreAuditAndPrivacy, tagName, albumId, isEssential, sortBy, ownerIdCacheKey);
            }, () =>
            {
                return GetSql_GetPhotos(tenantTypeId, string.Empty, null, ownerId, isEssential, null, null, tagName, ignoreAuditAndPrivacy, albumId, sortBy);
            });
        }

        /// <summary>
        /// 管理员获取照片数据
        /// </summary>
        /// <param name="tenantTypeId">租户类型ID</param>
        /// <param name="descriptionKeyword">照片描述</param>
        /// <param name="userId">作者ID</param>
        /// <param name="ownerId">所有者ID</param>
        /// <param name="isEssential">是否精华</param>
        /// <param name="auditStatus">审核状态</param>
        /// <param name="pageSize">分页大小</param>
        /// <param name="pageIndex">页码</param>
        /// <returns>返回管理员分页照片数据</returns>
        public PagingDataSet<Photo> GetPhotosForAdmin(string tenantTypeId, string descriptionKeyword, long? userId, long? ownerId, bool? isEssential, AuditStatus? auditStatus, int pageSize, int pageIndex)
        {
            Sql sql = GetSql_GetPhotos(tenantTypeId, descriptionKeyword, userId, ownerId, isEssential, null, auditStatus, string.Empty, null, null, SortBy_Photo.DateCreated_Desc);

            return GetPagingEntities(pageSize, pageIndex, sql);
        }

        /// <summary>
        /// 获取用户关注照片
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <param name="pageSize">分页大小</param>
        /// <param name="pageIndex">页码</param>
        /// <returns></returns>
        public PagingDataSet<Photo> GetPhotosOfFollowedUsers(long userId, int pageSize, int pageIndex)
        {
            return GetPagingEntities(pageSize, pageIndex, CachingExpirationType.ObjectCollection, () =>
            {
                return string.Format("GetFollowerPhotos-{0}", RealTimeCacheHelper.GetListCacheKeyPrefix(CacheVersionType.AreaVersion, "UserId", userId));

            }, () =>
            {
                Sql sql = Sql.Builder;
                sql.Select("spb_Photos.*")
                .From("spb_Photos");
                var whereSql = Sql.Builder;

                sql.InnerJoin("tn_Follows")
                    .On("spb_Photos.UserId = tn_Follows.FollowedUserId");
                whereSql.Where("tn_Follows.UserId=@0", userId);

                //添加权限控制
                switch (this.PubliclyAuditStatus)
                {
                    case PubliclyAuditStatus.Again:
                    case PubliclyAuditStatus.Fail:
                    case PubliclyAuditStatus.Pending:
                    case PubliclyAuditStatus.Success:
                        whereSql.Where("(spb_Photos.AuditStatus = @0 and spb_Photos.PrivacyStatus <> @1) or spb_Photos.UserId = @2", this.PubliclyAuditStatus, PrivacyStatus.Private, userId);
                        break;
                    case PubliclyAuditStatus.Pending_GreaterThanOrEqual:
                    case PubliclyAuditStatus.Again_GreaterThanOrEqual:
                        whereSql.Where("(spb_Photos.AuditStatus > @0 and spb_Photos.PrivacyStatus <> @1) or spb_Photos.UserId = @2", this.PubliclyAuditStatus, PrivacyStatus.Private, userId);
                        break;
                }
                sql.Append(whereSql).OrderBy("PhotoId desc");
                return sql;
            });
        }

        /// <summary>
        /// 照片统计数据
        /// </summary>
        /// <pram name="tenantTypeId">租户类型</param>
        /// <returns>返回照片统计数据</returns>
        public Dictionary<string, long> GetPhotoApplicationStatisticData(string tenantTypeId)
        {
            string cacheKey = string.Format("GetPhotoApplicationStatisticData-{0}", tenantTypeId);
            Dictionary<string, long> statisticData = cacheService.Get<Dictionary<string, long>>(cacheKey);

            if (statisticData != null)
                return statisticData;

            statisticData = new Dictionary<string, long>();

            Sql sql = Sql.Builder
                .Select("count(*)")
                .From("spb_Photos");
            if (!string.IsNullOrEmpty(tenantTypeId))
                sql.Where("TenantTypeId = @0", tenantTypeId);

            var dao = CreateDAO();
            dao.OpenSharedConnection();
            statisticData.Add(ApplicationStatisticDataKeys.Instance().TotalCount(), dao.FirstOrDefault<long>(sql));

            sql = Sql.Builder
                .Select("count(*)")
                .From("spb_Photos");
            if (!string.IsNullOrEmpty(tenantTypeId))
                sql.Where("TenantTypeId = @0", tenantTypeId);
            sql.Where("DateCreated > @0", DateTime.UtcNow.AddDays(-1));
            statisticData.Add(ApplicationStatisticDataKeys.Instance().Last24HCount(), dao.FirstOrDefault<long>(sql));
            dao.CloseSharedConnection();

            cacheService.Set(cacheKey, statisticData, CachingExpirationType.SingleObject);

            return statisticData;
        }

        /// <summary>
        /// 照片管理数据
        /// </summary>
        /// <param name="tenantTypeId">租户类型</param>
        /// <returns>返回照片管理数据</returns>
        public Dictionary<string, long> GetPhotoManageableData(string tenantTypeId)
        {
            string cacheKey = string.Format("GetPhotoManageableData::tenantTypeId-{0}", tenantTypeId);
            Dictionary<string, long> dic = cacheService.Get<Dictionary<string, long>>(cacheKey);
            if (dic != null)
                return dic;

            dic = new Dictionary<string, long>();

            Sql sql = Sql.Builder
                .Select("count(*)")
                .From("spb_Photos");

            if (!string.IsNullOrEmpty(tenantTypeId))
                sql.Where("TenantTypeId = @0", tenantTypeId);
            sql.Where("AuditStatus = @0", AuditStatus.Pending);

            var dao = CreateDAO();
            dao.OpenSharedConnection();
            dic.Add(ApplicationStatisticDataKeys.Instance().PendingCount(), dao.FirstOrDefault<long>(sql));

            sql = Sql.Builder
                .Select("count(*)")
                .From("spb_Photos");

            if (!string.IsNullOrEmpty(tenantTypeId))
                sql.Where("TenantTypeId = @0", tenantTypeId);

            sql.Where("AuditStatus = @0", AuditStatus.Again);

            dic.Add(ApplicationStatisticDataKeys.Instance().AgainCount(), dao.FirstOrDefault<long>(sql));
            dao.CloseSharedConnection();

            cacheService.Set(cacheKey, dic, CachingExpirationType.SingleObject);

            return dic;
        }


        #region private items

        /// <summary>
        /// 通用的获取SQL的方法
        /// </summary>
        /// <param name="tenantTypeId">租户类型id</param>
        /// <param name="descriptionKeyword">描述性字段</param>
        /// <param name="userId">用户id</param>
        /// <param name="ownerId">拥有者id</param>
        /// <param name="isEssential">是否精华</param>
        /// <param name="auditStatus">审核状态</param>
        /// <param name="tagName">标签名</param>
        /// <param name="ignoreAuditAndPrivacy">是否忽略审核和隐私状态</param>
        /// <param name="albumId">相册id</param>
        /// <param name="sortBy">排序方式</param>
        /// <returns>SQL</returns>
        private Sql GetSql_GetPhotos(string tenantTypeId, string descriptionKeyword, long? userId, long? ownerId, bool? isEssential, DateTime? createDateTime
            , AuditStatus? auditStatus, string tagName, bool? ignoreAuditAndPrivacy, long? albumId, SortBy_Photo sortBy)
        {
            Sql sql = Sql.Builder;
            Sql sql_Where = Sql.Builder;
            Sql sql_Orderby = Sql.Builder;

            sql.Select("spb_Photos.*")
                .From("spb_Photos");

            if (!string.IsNullOrEmpty(tenantTypeId))
                sql_Where.Where("spb_Photos.TenantTypeId = @0", tenantTypeId);

            if (!string.IsNullOrEmpty(descriptionKeyword))
                sql_Where.Where("spb_Photos.Description like @0", "%" + StringUtility.StripSQLInjection(descriptionKeyword) + "%");

            if (userId.HasValue)
                sql_Where.Where("spb_Photos.UserId = @0", userId.Value);

            if (ownerId.HasValue)
                sql_Where.Where("spb_Photos.OwnerId = @0", ownerId.Value);

            if (auditStatus.HasValue)
                sql_Where.Where("spb_Photos.AuditStatus = @0", auditStatus.Value);

            if (isEssential.HasValue)
                sql_Where.Where("spb_Photos.IsEssential = @0", isEssential);

            if (createDateTime.HasValue)
                sql_Where.Where("spb_Photos.DateCreated > @0", createDateTime);

            if (!string.IsNullOrEmpty(tagName))
            {
                sql.InnerJoin("tn_ItemsInTags")
                    .On("spb_Photos.PhotoId = tn_ItemsInTags.ItemId");

                sql_Where.Where("tn_ItemsInTags.TagName=@0", tagName)
                    .Where("tn_ItemsInTags.TenantTypeId = @0", TenantTypeIds.Instance().Photo());
            }

            if (ignoreAuditAndPrivacy.HasValue && !ignoreAuditAndPrivacy.Value)
            {
                switch (this.PubliclyAuditStatus)
                {
                    case PubliclyAuditStatus.Again:
                    case PubliclyAuditStatus.Fail:
                    case PubliclyAuditStatus.Pending:
                    case PubliclyAuditStatus.Success:
                        sql_Where.Where("spb_Photos.AuditStatus = @0", this.PubliclyAuditStatus);
                        break;
                    case PubliclyAuditStatus.Pending_GreaterThanOrEqual:
                    case PubliclyAuditStatus.Again_GreaterThanOrEqual:
                        sql_Where.Where("spb_Photos.AuditStatus > @0", this.PubliclyAuditStatus);
                        break;
                }

                sql_Where.Where("spb_Photos.PrivacyStatus <> @0", PrivacyStatus.Private);
            }

            if (albumId.HasValue)
                sql_Where.Where("spb_Photos.AlbumId = @0", albumId);

            CountService countService = new CountService(TenantTypeIds.Instance().Photo());
            string countTableName = countService.GetTableName_Counts();

            switch (sortBy)
            {
                case SortBy_Photo.DateCreated_Desc:
                    sql_Orderby.OrderBy("spb_Photos.PhotoId desc");
                    break;
                case SortBy_Photo.HitTimes_Desc:
                    sql.LeftJoin(countTableName).On("spb_Photos.PhotoId = " + countTableName + ".ObjectId");
                    sql_Where.Where(countTableName + ".CountType = @0 or " + countTableName + ".CountType is null", CountTypes.Instance().HitTimes());
                    sql_Orderby.OrderBy(countTableName + ".StatisticsCount desc");
                    break;
                case SortBy_Photo.CommentCount_Desc:
                    sql.LeftJoin(countTableName).On("spb_Photos.PhotoId = " + countTableName + ".ObjectId");
                    sql_Where.Where(countTableName + ".CountType = @0 or " + countTableName + ".CountType is null", CountTypes.Instance().CommentCount());
                    sql_Orderby.OrderBy(countTableName + ".StatisticsCount desc");
                    break;
                case SortBy_Photo.SupportCount_Desc:
                    sql.LeftJoin("tn_Attitudes").On("spb_Photos.PhotoId = tn_Attitudes.ObjectId");
                    sql_Where.Where("tn_Attitudes.TenantTypeId = @0", TenantTypeIds.Instance().Photo());
                    sql_Orderby.OrderBy("tn_Attitudes.SupportCount desc");
                    break;
            }

            sql.Append(sql_Where)
               .Append(sql_Orderby);

            return sql;
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
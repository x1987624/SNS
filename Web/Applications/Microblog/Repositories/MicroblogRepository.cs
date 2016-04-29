//<TunynetCopyright>
//--------------------------------------------------------------
//<version>V0.5</verion>
//<createdate>2012-08-09</createdate>
//<author>libsh</author>
//<email>libsh@tunynet.com</email>
//<log date="2012-08-09" version="0.5">创建</log>
//<log date="2012-08-10" version="0.6" author="yangmj">走查</log>
//--------------------------------------------------------------
//</TunynetCopyright>

using System.Collections.Generic;
using System.Text;
using System.Linq;
using PetaPoco;
using Tunynet;
using Tunynet.Caching;
using Tunynet.Common;
using Tunynet.Repositories;
using System;
using Spacebuilder.Common;
using Tunynet.Utilities;

namespace Spacebuilder.Microblog
{
    /// <summary>
    ///微博Repository
    /// </summary>
    public class MicroblogRepository : Repository<MicroblogEntity>, IMicroblogRepository
    {
        /// <summary>
        /// 每页显示条数（需要从配置文件中读取）
        /// </summary>
        private int pageSize = 30;

        /// <summary>
        /// 微博应用可对外显示的审核状态
        /// </summary>
        protected PubliclyAuditStatus PubliclyAuditStatus
        {
            get
            {
                AuditService auditService = new AuditService();
                return auditService.GetPubliclyAuditStatus(ApplicationIds.Instance().Microblog());
            }
        }

        #region 维护

        public override object Insert(MicroblogEntity entity)
        {

            object oId = base.Insert(entity);

            long id = 0;
            long.TryParse(oId.ToString(), out id);

            if (id > 0)
            {
                Sql sql = Sql.Builder;
                int affect = CreateDAO().Execute(sql.Append("update spb_Microblogs set ForwardedCount = ForwardedCount + 1 where MicroblogId = @0", entity.ForwardedMicroblogId));

                if (affect > 0)
                {
                    string cacheKey = RealTimeCacheHelper.GetCacheKeyOfEntity(entity.ForwardedMicroblogId);
                    MicroblogEntity microblogEntity = cacheService.Get<MicroblogEntity>(cacheKey);
                    if (microblogEntity != null)
                    {
                        microblogEntity.ForwardedCount++;
                        cacheService.Set(cacheKey, microblogEntity, CachingExpirationType.SingleObject);
                    }
                }
            }

            return oId;
        }

        public override int Delete(MicroblogEntity entity)
        {
            int affect = base.Delete(entity);

            if (affect > 0)
            {
                Sql sql = Sql.Builder;
                sql.Append("update spb_Microblogs set ForwardedCount = ForwardedCount - 1 where MicroblogId = @0 and ForwardedCount > 0", entity.ForwardedMicroblogId);

                if (CreateDAO().Execute(sql) > 0)
                {
                    string cacheKey = RealTimeCacheHelper.GetCacheKeyOfEntity(entity.ForwardedMicroblogId);
                    MicroblogEntity microblogEntity = cacheService.Get<MicroblogEntity>(cacheKey);
                    if (microblogEntity != null)
                    {
                        if (microblogEntity.ForwardedCount > 0)
                            microblogEntity.ForwardedCount--;
                        cacheService.Set(cacheKey, microblogEntity, CachingExpirationType.SingleObject);
                    }
                }
            }

            return affect;
        }

        /// <summary>
        /// 删除用户记录（删除用户时使用）
        /// </summary>
        /// <param name="userId">被删除用户</param>
        /// <param name="takeOver">接管用户</param>
        /// <param name="takeOverAll">是否接管被删除用户的所有内容</param>
        public void DeleteUser(long userId, User takeOver, bool takeOverAll)
        {
            List<Sql> sqls = new List<Sql>();
   
            if (takeOverAll && takeOver != null)
            {
                sqls.Add(Sql.Builder.Append("update spb_Microblogs set UserId = @0 where UserId = @1", takeOver.UserId, userId));
            }
           
            CreateDAO().Execute(sqls);
        }

        #endregion

        #region 获取

        /// <summary>
        /// 根据用户获取可排序的微博集合
        /// </summary>
        /// <param name="userId">用户Id</param>
        /// <param name="mediaType"><see cref="MediaType"/></param>
        /// <param name="isOriginal">是否为原创</param>
        /// <param name="pageIndex">页码</param>
        ///<param name="tenantTypeId">租户类型Id</param>
        /// <returns></returns>
        public PagingDataSet<long> GetPagingIds(long userId, MediaType? mediaType, bool? isOriginal, int pageIndex, string tenantTypeId = "")
        {
            Sql sql = Sql.Builder;
            sql.Select("MicroblogId")
               .From("spb_Microblogs")
               .Where("UserId = @0 and OwnerId = @0", userId);

            if (!string.IsNullOrEmpty(tenantTypeId))
            {
                sql.Where("TenantTypeId = @0", tenantTypeId);
            }

            if (isOriginal.HasValue && isOriginal.Value)
            {
                sql.Where("OriginalMicroblogId = 0");
            }
            else if (mediaType.HasValue)
            {
                switch (mediaType)
                {
                    case MediaType.Image:
                        sql.Where("HasPhoto = 1");
                        break;
                    case MediaType.Video:
                        sql.Where("HasVideo = 1");
                        break;
                    case MediaType.Audio:
                        sql.Where("HasMusic = 1");
                        break;
                }

            }

            switch (this.PubliclyAuditStatus)
            {
                case PubliclyAuditStatus.Again:
                case PubliclyAuditStatus.Fail:
                case PubliclyAuditStatus.Pending:
                case PubliclyAuditStatus.Success:
                    sql.Where("AuditStatus = @0", this.PubliclyAuditStatus);
                    break;
                case PubliclyAuditStatus.Again_GreaterThanOrEqual:
                case PubliclyAuditStatus.Pending_GreaterThanOrEqual:
                    sql.Where("AuditStatus > @0", this.PubliclyAuditStatus);
                    break;
                default:
                    break;
            }

            sql.OrderBy("MicroblogId desc");

            PagingEntityIdCollection peic = null;
            if (pageIndex < CacheablePageCount)
            {

                StringBuilder cacheKey = new StringBuilder(RealTimeCacheHelper.GetListCacheKeyPrefix(CacheVersionType.AreaVersion, "UserId", userId));
                cacheKey.AppendFormat("PagingsByUser::Type-{0}:isOriginal-{1}:TenantTypeId-{2}", mediaType, isOriginal, tenantTypeId);

                peic = cacheService.Get<PagingEntityIdCollection>(cacheKey.ToString());
                if (peic == null)
                {
                    peic = CreateDAO().FetchPagingPrimaryKeys<MicroblogEntity>(PrimaryMaxRecords, pageSize * CacheablePageCount, 1, sql);
                    peic.IsContainsMultiplePages = true;
                    cacheService.Add(cacheKey.ToString(), peic, CachingExpirationType.ObjectCollection);
                }
            }
            else
            {
                peic = CreateDAO().FetchPagingPrimaryKeys<MicroblogEntity>(PrimaryMaxRecords, pageSize, pageIndex, sql);
            }

            IEnumerable<long> ids = peic.GetPagingEntityIds(pageSize, pageIndex).Cast<long>();
            PagingDataSet<long> pds = new PagingDataSet<long>(ids);
            pds.PageSize = pageSize;
            pds.PageIndex = pageIndex;
            pds.TotalRecords = peic.TotalRecords;

            return pds;
        }

        /// <summary>
        /// 获取微博分页数据
        /// </summary>
        ///<param name="pageIndex">待获取条数</param>
        ///<param name="tenantTypeId">租户类型Id</param>
        /// <param name="mediaType"><see cref="MediaType"/></param>
        /// <param name="isOriginal">是否为原创</param>
        ///<param name="sortBy">排序</param>
        /// <returns></returns>
        public PagingDataSet<MicroblogEntity> GetPagings(int pageIndex, string tenantTypeId = "", MediaType? mediaType = null, bool? isOriginal = null, SortBy_Microblog sortBy = SortBy_Microblog.DateCreated)
        {
            return GetPagingEntities(pageSize, pageIndex, CachingExpirationType.ObjectCollection,
                   () =>
                   {
                       return GetCackeKey_PagingMicroblogs(tenantTypeId, mediaType, isOriginal, sortBy);
                   },
                   () =>
                   {
                       return GenerateSql_Paging_Top(null, tenantTypeId, mediaType, isOriginal, sortBy);
                   });
        }

        /// <summary>
        /// 根据用户获取可排序的微博集合
        /// </summary>
        /// <param name="userId">用户Id</param>
        /// <param name="mediaType"><see cref="MediaType"/></param>
        /// <param name="isOriginal">是否为原创</param>
        /// <param name="pageIndex">页码</param>
        ///<param name="tenantTypeId">租户类型Id</param>
        /// <returns></returns>
        public PagingDataSet<MicroblogEntity> GetPagings(long userId, MediaType? mediaType, bool? isOriginal, int pageIndex, string tenantTypeId = "")
        {
            return GetPagingEntities(pageSize, pageIndex, CachingExpirationType.ObjectCollection,
            () =>
            {
                StringBuilder cacheKey = new StringBuilder(RealTimeCacheHelper.GetListCacheKeyPrefix(CacheVersionType.AreaVersion, "UserId", userId));
                cacheKey.AppendFormat("PagingsByUser::Type-{0}:isOriginal-{1}:TenantTypeId-{2}", mediaType, isOriginal, tenantTypeId);

                return cacheKey.ToString();
            },
            () =>
            {
                Sql sql = Sql.Builder;
                sql.Select("MicroblogId")
                   .From("spb_Microblogs")
                   .Where("UserId = @0", userId);

                if (!string.IsNullOrEmpty(tenantTypeId))
                {
                    sql.Where("TenantTypeId = @0", tenantTypeId);
                }
                
                //reply:是的就是一个效果
                if (isOriginal.HasValue && isOriginal.Value)
                {
                    sql.Where("OriginalMicroblogId = 0");
                }
                else if (mediaType.HasValue)
                {
                    switch (mediaType)
                    {
                        case MediaType.Image:
                            sql.Where("HasPhoto = 1");
                            break;
                        case MediaType.Video:
                            sql.Where("HasVideo = 1");
                            break;
                        case MediaType.Audio:
                            sql.Where("HasMusic = 1");
                            break;
                    }

                }

                switch (this.PubliclyAuditStatus)
                {
                    case PubliclyAuditStatus.Again:
                    case PubliclyAuditStatus.Fail:
                    case PubliclyAuditStatus.Pending:
                    case PubliclyAuditStatus.Success:
                        sql.Where("AuditStatus = @0", this.PubliclyAuditStatus);
                        break;
                    case PubliclyAuditStatus.Again_GreaterThanOrEqual:
                    case PubliclyAuditStatus.Pending_GreaterThanOrEqual:
                        sql.Where("AuditStatus > @0", this.PubliclyAuditStatus);
                        break;
                    default:
                        break;
                }

                sql.OrderBy("MicroblogId desc");

                return sql;
            });
        }

        /// <summary>
        /// 根据拥有者获取微博分页集合
        /// </summary>
        /// <param name="ownerId">用户Id</param>
        /// <param name="userId">微博作者Id</param>
        ///<param name="mediaType"><see cref="MediaType"/></param>
        ///<param name="isOriginal">是否为原创</param>
        /// <param name="pageIndex">页码</param>
        ///<param name="tenantTypeId">租户类型Id</param>
        /// <returns></returns>
        public PagingDataSet<MicroblogEntity> GetPagings(long ownerId, long? userId, MediaType? mediaType, bool? isOriginal, int pageIndex, string tenantTypeId = "")
        {
            return GetPagingEntities(pageSize, pageIndex, CachingExpirationType.ObjectCollection,
            () =>
            {
                StringBuilder cacheKey = new StringBuilder(RealTimeCacheHelper.GetListCacheKeyPrefix(CacheVersionType.AreaVersion, "OwnerId", ownerId));
                cacheKey.AppendFormat("PagingsByOwner::userId-{0}:type-{1}:isOriginal-{2}:TenantTypeId-{3}", userId, mediaType, isOriginal, tenantTypeId);

                return cacheKey.ToString();
            },
            () =>
            {
                Sql sql = Sql.Builder;
                sql.Where("OwnerId = @0", ownerId);

                if (userId.HasValue)
                {
                    sql.Where("UserId = @0", userId);
                }

                if (!string.IsNullOrEmpty(tenantTypeId))
                {
                    sql.Where("TenantTypeId = @0", tenantTypeId);
                }

                if (isOriginal.HasValue && isOriginal.Value)
                {
                    sql.Where("OriginalMicroblogId = 0");
                }
                else if (mediaType.HasValue)
                {
                    switch (mediaType)
                    {
                        case MediaType.Image:
                            sql.Where("HasPhoto = 1");
                            break;
                        case MediaType.Video:
                            sql.Where("HasVideo = 1");
                            break;
                        case MediaType.Audio:
                            sql.Where("HasMusic = 1");
                            break;
                    }
                }



                sql.OrderBy("MicroblogId desc");

                return sql;
            });
        }

        /// <summary>
        /// 获取指定条数的微博
        /// </summary>
        ///<param name="topNumber">待获取条数</param>
        ///<param name="tenantTypeId">租户类型Id</param>
        ///<param name="mediaType"><see cref="MediaType"/></param>
        ///<param name="isOriginal">是否为原创</param>
        ///<param name="sortBy">排序</param>
        /// <returns></returns>
        public IEnumerable<MicroblogEntity> GetTops(int topNumber, string tenantTypeId = "", MediaType? mediaType = null, bool? isOriginal = null, SortBy_Microblog sortBy = SortBy_Microblog.DateCreated)
        {
            return GetTopEntities(topNumber, CachingExpirationType.ObjectCollection,
            () =>
            {
                StringBuilder cacheKey = new StringBuilder(RealTimeCacheHelper.GetListCacheKeyPrefix(CacheVersionType.GlobalVersion));
                cacheKey.AppendFormat("TopMicroblogs::TenantTypeId-{0}:sortBy-{1}:type-{2},isOriginal-{3}", tenantTypeId, ((int)sortBy).ToString(), mediaType, isOriginal);

                return cacheKey.ToString();
            },
            () =>
            {
                return GenerateSql_Paging_Top(0, tenantTypeId, mediaType, isOriginal, sortBy);
            });
        }

        /// <summary>
        /// 根据用户获取指定条数的微博
        /// </summary>
        /// <param name="userId">用户Id</param>
        /// <param name="mediaType"><see cref="MediaType"/></param>
        /// <param name="isOriginal">是否为原创</param>
        ///<param name="topNumber">待获取条数</param>
        ///<param name="tenantTypeId">租户类型Id</param>
        ///<param name="sortBy">排序</param>
        /// <returns></returns>
        public IEnumerable<MicroblogEntity> GetTops(long userId, MediaType? mediaType, bool? isOriginal, int topNumber, string tenantTypeId = "", SortBy_Microblog sortBy = SortBy_Microblog.DateCreated)
        {
            return GetTopEntities(topNumber, CachingExpirationType.ObjectCollection,
            () =>
            {
                StringBuilder cacheKey = new StringBuilder(RealTimeCacheHelper.GetListCacheKeyPrefix(CacheVersionType.AreaVersion, "UserId", userId));
                cacheKey.AppendFormat("TopMicroblogsByUser::TenantTypeId-{0}:sortBy-{1}:type-{2}:isOriginal-{3}", tenantTypeId, ((int)sortBy).ToString(), mediaType, isOriginal);

                return cacheKey.ToString();
            },
            () =>
            {
                Sql sql = Sql.Builder;
                sql.Where("UserId = @0", userId);

                if (!string.IsNullOrEmpty(tenantTypeId))
                {
                    sql.Where("TenantTypeId = @0", tenantTypeId);
                }

                if (isOriginal.HasValue && isOriginal.Value)
                {
                    sql.Where("OriginalMicroblogId = 0");
                }
                else if (mediaType.HasValue)
                {
                    switch (mediaType)
                    {
                        case MediaType.Image:
                            sql.Where("HasPhoto = 1");
                            break;
                        case MediaType.Video:
                            sql.Where("HasVideo = 1");
                            break;
                        case MediaType.Audio:
                            sql.Where("HasMusic = 1");
                            break;
                    }
                }

                switch (this.PubliclyAuditStatus)
                {
                    case PubliclyAuditStatus.Again:
                    case PubliclyAuditStatus.Fail:
                    case PubliclyAuditStatus.Pending:
                    case PubliclyAuditStatus.Success:
                        sql.Where("AuditStatus = @0", this.PubliclyAuditStatus);
                        break;
                    case PubliclyAuditStatus.Again_GreaterThanOrEqual:
                    case PubliclyAuditStatus.Pending_GreaterThanOrEqual:
                        sql.Where("AuditStatus > @0", this.PubliclyAuditStatus);
                        break;
                    default:
                        break;
                }

                switch (sortBy)
                {
                    case SortBy_Microblog.DateCreated:
                        sql.OrderBy("MicroblogId desc");
                        break;
                    case SortBy_Microblog.ForwardedCount:
                        sql.Where("ForwardedCount desc");
                        break;
                    case SortBy_Microblog.ReplyCount:
                        sql.Where("ReplyCount Desc");
                        break;
                }

                return sql;
            });

        }

        /// <summary>
        /// 根据拥有者获取指定条数的微博
        /// </summary>
        ///<param name="ownerId">微博拥有者Id</param>
        ///<param name="topNumber">待获取条数</param>
        ///<param name="tenantTypeId">租户类型Id</param>
        ///<param name="mediaType"><see cref="MediaType"/></param>
        ///<param name="isOriginal">是否为原创</param>
        ///<param name="sortBy">排序</param>
        /// <returns></returns>
        public IEnumerable<MicroblogEntity> GetTops(long ownerId, int topNumber, string tenantTypeId, MediaType? mediaType = null, bool? isOriginal = null, SortBy_Microblog sortBy = SortBy_Microblog.DateCreated)
        {
            return GetTopEntities(topNumber, CachingExpirationType.ObjectCollection,
            () =>
            {
                StringBuilder cacheKey = new StringBuilder(RealTimeCacheHelper.GetListCacheKeyPrefix(CacheVersionType.AreaVersion, "OwnerId", ownerId));
                cacheKey.AppendFormat("TopMicroblogsByOwner::TenantTypeId-{0}:sortBy-{1}:type-{2}:isOriginal-{3}", tenantTypeId, ((int)sortBy).ToString(), mediaType, isOriginal);

                return cacheKey.ToString();
            },
            () =>
            {
                return GenerateSql_Paging_Top(ownerId, tenantTypeId, mediaType, isOriginal, sortBy);
            });
        }

        /// <summary>
        /// 管理员后台获取列表方法
        /// </summary>
        /// <param name="query">查询条件</param>
        ///<param name="pageSize">每页显示内容数</param>
        ///<param name="pageIndex">页码</param>
        /// <returns></returns>
        public PagingDataSet<MicroblogEntity> GetMicroblogs(MicroblogQuery query, int pageSize, int pageIndex)
        {
            Sql sql = Sql.Builder;
            
            //reply:已修改
            sql.Select("*")
               .From("spb_Microblogs");
            BuildSqlWhere_MicroblogQuery(query, ref sql);
            return GetPagingEntities(pageSize, pageIndex, sql);
        }

        /// <summary>
        /// 获取某个用户的所有微博（用于删除用户）
        /// </summary>
        /// <param name="userId">用户Id</param>
        /// <returns></returns>
        public IEnumerable<MicroblogEntity> GetAllMicroblogsOfUser(long userId)
        {
            
            //reply:已修改，这里是其他人加的
            var sql = Sql.Builder;
            sql.Select("MicroblogId")
               .From("spb_Microblogs")
               .Where("UserId = @0", userId);
            IEnumerable<object> Ids = CreateDAO().FetchFirstColumn(sql);
            return PopulateEntitiesByEntityIds(Ids);
        }

        /// <summary>
        /// 获取解析后的内容
        /// </summary>
        /// <param name="microblogId">微博Id</param>
        /// <returns></returns>
        public string GetResolvedBody(long microblogId)
        {
            MicroblogEntity microblog = Get(microblogId);
            if (microblog == null)
                return string.Empty;

            string cacheKey = string.Format("ResolvedBody{0}::{1}", RealTimeCacheHelper.GetEntityVersion(microblogId), microblogId);
            string resolveBody = cacheService.Get<string>(cacheKey);
            if (string.IsNullOrEmpty(resolveBody))
            {
                resolveBody = microblog.Body;
                IMicroblogBodyProcessor microblogBodyProcessor = DIContainer.Resolve<IMicroblogBodyProcessor>();
                resolveBody = microblogBodyProcessor.Process(resolveBody, microblog.TenantTypeId, microblog.MicroblogId, microblog.UserId, microblog.OwnerId);
                cacheService.Set(cacheKey, resolveBody, CachingExpirationType.UsualSingleObject);
            }
            return resolveBody;
        }

        /// <summary>
        /// 获取最新微博
        /// </summary>
        ///<param name="lastMicroblogId">用户当前浏览的最新一条微博Id</param>
        ///<param name="tenantTypeId">租户类型Id</param>
        /// <returns></returns>
        public int GetNewerCount(long lastMicroblogId, string tenantTypeId)
        {
            ICacheService cacheService = DIContainer.Resolve<ICacheService>();

            string cacheKey = GetCackeKey_PagingMicroblogs(tenantTypeId, null, null, SortBy_Microblog.DateCreated);

            PagingEntityIdCollection peic = cacheService.Get<PagingEntityIdCollection>(cacheKey);
            if (peic == null)
            {
                Sql sql = GenerateSql_Paging_Top(0, tenantTypeId, null, null, SortBy_Microblog.DateCreated);
                peic = CreateDAO().FetchPagingPrimaryKeys<MicroblogEntity>(PrimaryMaxRecords, pageSize * CacheablePageCount, 1, sql);
                peic.IsContainsMultiplePages = true;
                cacheService.Add(cacheKey, peic, Tunynet.Caching.CachingExpirationType.ObjectCollection);
            }

            IEnumerable<long> ids = peic.GetTopEntityIds(1000).Cast<long>();
            int count = 0;

            if (ids != null && ids.Count() > 0 && ids.Contains(lastMicroblogId))
            {
                count = ids.ToList().IndexOf(lastMicroblogId);
            }

            return count;
        }

        /// <summary>
        /// 获取最新微博
        /// </summary>
        ///<param name="lastMicroblogId">用户当前浏览的最新一条微博Id</param>
        ///<param name="tenantTypeId">租户类型Id</param>
        /// <returns></returns>
        public IEnumerable<MicroblogEntity> GetNewerMicroblogs(long lastMicroblogId, string tenantTypeId)
        {
            ICacheService cacheService = DIContainer.Resolve<ICacheService>();

            string cacheKey = GetCackeKey_PagingMicroblogs(tenantTypeId, null, null, SortBy_Microblog.DateCreated);

            PagingEntityIdCollection peic = cacheService.Get<PagingEntityIdCollection>(cacheKey);
            if (peic == null)
            {
                Sql sql = GenerateSql_Paging_Top(0, tenantTypeId, null, null, SortBy_Microblog.DateCreated);
                peic = CreateDAO().FetchPagingPrimaryKeys<MicroblogEntity>(PrimaryMaxRecords, pageSize * CacheablePageCount, 1, sql);
                peic.IsContainsMultiplePages = true;
                cacheService.Add(cacheKey, peic, Tunynet.Caching.CachingExpirationType.ObjectCollection);
            }

            IEnumerable<long> ids = peic.GetTopEntityIds(1000).Cast<long>();
            IEnumerable<long> getIds = null;
            int count = 0;

            if (ids != null && ids.Count() > 0 && ids.Contains(lastMicroblogId))
            {
                count = ids.ToList().IndexOf(lastMicroblogId);

                if (count == ids.Count())
                {
                    getIds = ids;
                }
                else
                {
                    getIds = ids.Take(count);
                }
            }

            return PopulateEntitiesByEntityIds(getIds);
        }

        /// <summary>
        /// 获取微博管理数据
        /// </summary>
        /// <param name="tenantTypeId">租户类型Id（可以获取该应用下针对某种租户类型的统计计数，默认不进行筛选）</param>
        /// <returns></returns>
        public Dictionary<string, long> GetManageableDatas(string tenantTypeId = null)
        {
            var dao = CreateDAO();
            Dictionary<string, long> manageableDatas = new Dictionary<string, long>();
            Sql sql = Sql.Builder;
            sql.Select("count(*)")
                .From("spb_Microblogs")
                .Where("AuditStatus=@0", AuditStatus.Pending);
            if (!string.IsNullOrEmpty(tenantTypeId))
            {
                sql.Where("TenantTypeId=@0", tenantTypeId);
            }

            manageableDatas.Add(ApplicationStatisticDataKeys.Instance().MicroblogPendingCount(), dao.FirstOrDefault<long>(sql));
            sql = Sql.Builder;
            sql.Select("count(*)")
                .From("spb_Microblogs")
                .Where("AuditStatus=@0", AuditStatus.Again);
            if (!string.IsNullOrEmpty(tenantTypeId))
            {
                sql.Where("TenantTypeId=@0", tenantTypeId);
            }

            manageableDatas.Add(ApplicationStatisticDataKeys.Instance().MicroblogAgainCount(), dao.FirstOrDefault<long>(sql));

            return manageableDatas;
        }

        /// <summary>
        /// 获取微博统计数据
        /// </summary>
        /// <param name="tenantTypeId">租户类型Id（可以获取该应用下针对某种租户类型的统计计数，默认不进行筛选）</param>
        /// <returns></returns>
        public Dictionary<string, long> GetStatisticDatas(string tenantTypeId = null)
        {
            var dao = CreateDAO();
            string cacheKey = "MicroBlogStatisticData";
            Dictionary<string, long> statisticDatas = cacheService.Get<Dictionary<string, long>>(cacheKey);
            if (statisticDatas == null)
            {
                statisticDatas = new Dictionary<string, long>();
                Sql sql = Sql.Builder;
                sql.Select("count(*)")
                    .From("spb_Microblogs");
                if (!string.IsNullOrEmpty(tenantTypeId))
                {
                    sql.Where("TenantTypeId=@0", tenantTypeId);
                }
                statisticDatas.Add(ApplicationStatisticDataKeys.Instance().TotalCount(), dao.FirstOrDefault<long>(sql));

                sql = Sql.Builder;
                sql.Select("count(*)")
                    .From("spb_Microblogs")
                    .Where("DateCreated > @0", DateTime.UtcNow.AddDays(-1));
                if (!string.IsNullOrEmpty(tenantTypeId))
                {
                    sql.Where("TenantTypeId=@0", tenantTypeId);
                }

                statisticDatas.Add(ApplicationStatisticDataKeys.Instance().Last24HCount(), dao.FirstOrDefault<long>(sql));
                cacheService.Add(cacheKey, statisticDatas, CachingExpirationType.UsualSingleObject);
            }

            return statisticDatas;
        }
        #endregion

        #region Private Item

        /// <summary>
        /// 从MicroblogQuery构建PetaPoco.Sql的where条件
        /// </summary>
        /// <param name="query">MicroblogQuery查询条件</param>
        /// <param name="sql">PetaPoco.Sql对象</param>
        private void BuildSqlWhere_MicroblogQuery(MicroblogQuery query, ref PetaPoco.Sql sql)
        {
            if (sql == null)
            {
                sql = PetaPoco.Sql.Builder;
            }

            if (query.UserId.HasValue)
                sql.Where("UserId = @0", query.UserId);

            if (!string.IsNullOrEmpty(query.TenantTypeId))
            {
                sql.Where("TenantTypeId = @0", query.TenantTypeId);
            }

            if (query.OwnerId.HasValue)
            {
                sql.Where("OwnerId = @0", query.OwnerId);
            }

            if (query.isOriginal.HasValue && query.isOriginal.Value)
            {
                sql.Where("OriginalMicroblogId = 0");
            }
            else if (query.MediaType.HasValue)
            {
                switch (query.MediaType)
                {
                    case MediaType.Image:
                        sql.Where("HasPhoto = 1");
                        break;
                    case MediaType.Video:
                        sql.Where("HasVideo = 1");
                        break;
                    case MediaType.Audio:
                        sql.Where("HasMusic = 1");
                        break;
                }
            }
            
            if (!string.IsNullOrEmpty(query.Keyword))
                sql.Where("Body like @0", "%" + StringUtility.StripSQLInjection(query.Keyword) + "%");

            if (query.StartDate.HasValue)
                sql.Where("DateCreated >= @0", query.StartDate);
            
            //reply:已修改
            if (query.EndDate.HasValue)
                sql.Where("DateCreated < @0", query.EndDate.Value.AddDays(1));

            if (query.AuditStatus.HasValue)
            {
                sql.Where("AuditStatus  = @0", (int)query.AuditStatus);
            }

            sql.OrderBy("MicroblogId desc");
        }

        /// <summary>
        /// 获取微博分页列表的CacheKey
        /// </summary>
        /// <param name="tenantTypeId">租户类型Id</param>
        /// <param name="mediaType">多媒体类型</param>
        /// <param name="isOriginal">是否为原创</param>
        /// <param name="sortBy">排序字段</param>
        /// <returns></returns>
        private string GetCackeKey_PagingMicroblogs(string tenantTypeId, MediaType? mediaType, bool? isOriginal, SortBy_Microblog sortBy)
        {
            StringBuilder cacheKey = new StringBuilder(RealTimeCacheHelper.GetListCacheKeyPrefix(CacheVersionType.GlobalVersion));
            cacheKey.AppendFormat("PagingsMicroblogs::TenantTypeId-{0}:sortBy-{1}:type-{2},isOriginal-{3}", tenantTypeId, ((int)sortBy).ToString(), mediaType, isOriginal);
            return cacheKey.ToString();
        }

        /// <summary>
        /// 生成Sql语句用于微博分页及TopN条数据获取
        /// </summary>
        /// <param name="ownerId">拥有者Id</param>
        ///<param name="tenantTypeId">租户类型Id</param>
        /// <param name="mediaType"><see cref="MediaType"/></param>
        /// <param name="isOriginal">是否为原创</param>
        ///<param name="sortBy">排序</param>
        /// <returns></returns>
        private Sql GenerateSql_Paging_Top(long? ownerId, string tenantTypeId, MediaType? mediaType, bool? isOriginal, SortBy_Microblog sortBy = SortBy_Microblog.DateCreated)
        {
            Sql sql = Sql.Builder;
            if (ownerId.HasValue)
                sql.Where("OwnerId = @0", ownerId.Value);

            if (!string.IsNullOrEmpty(tenantTypeId))
            {
                sql.Where("TenantTypeId = @0", tenantTypeId);
            }

            if (isOriginal.HasValue && isOriginal.Value)
            {
                sql.Where("OriginalMicroblogId = 0");
            }
            else if (mediaType.HasValue)
            {
                switch (mediaType)
                {
                    case MediaType.Image:
                        sql.Where("HasPhoto = 1");
                        break;
                    case MediaType.Video:
                        sql.Where("HasVideo = 1");
                        break;
                    case MediaType.Audio:
                        sql.Where("HasMusic = 1");
                        break;
                }
            }

            switch (this.PubliclyAuditStatus)
            {
                case PubliclyAuditStatus.Again:
                case PubliclyAuditStatus.Fail:
                case PubliclyAuditStatus.Pending:
                case PubliclyAuditStatus.Success:
                    sql.Where("AuditStatus = @0", this.PubliclyAuditStatus);
                    break;
                case PubliclyAuditStatus.Again_GreaterThanOrEqual:
                case PubliclyAuditStatus.Pending_GreaterThanOrEqual:
                    sql.Where("AuditStatus > @0", this.PubliclyAuditStatus);
                    break;
                default:
                    break;
            }

            switch (sortBy)
            {
                case SortBy_Microblog.DateCreated:
                    sql.OrderBy("MicroblogId desc");
                    break;
                case SortBy_Microblog.ForwardedCount:
                    sql.OrderBy("ForwardedCount desc");
                    break;
                case SortBy_Microblog.ReplyCount:
                    sql.OrderBy("ReplyCount Desc");
                    break;
            }

            return sql;
        }

        #endregion

        /// <summary>
        /// 组装实体列表
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entityIds"></param>
        /// <returns></returns>
        public override IEnumerable<MicroblogEntity> PopulateEntitiesByEntityIds<T>(IEnumerable<T> entityIds)
        {
            var microblogs = base.PopulateEntitiesByEntityIds<T>(entityIds);
            var userService = DIContainer.Resolve<IUserService>();
            IEnumerable<IUser> listUsers = userService.GetUsers(microblogs.Select(n => n.UserId));            
            return microblogs;
        }


    }
}

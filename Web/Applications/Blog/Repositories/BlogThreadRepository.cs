//<TunynetCopyright>
//--------------------------------------------------------------
//<copyright>拓宇网络科技有限公司 2005-2012</copyright>
//<version>V0.5</verion>
//<createdate>2012-10-16</createdate>
//<author>jiangshl</author>
//<email>jiangshl@tunynet.com</email>
//<log date="2012-10-16" version="0.5">创建</log>
//--------------------------------------------------------------
//</TunynetCopyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PetaPoco;
using Spacebuilder.Common;
using Tunynet;
using Tunynet.Caching;
using Tunynet.Common;
using Tunynet.Repositories;
using Tunynet.Utilities;

namespace Spacebuilder.Blog
{
    /// <summary>
    /// 日志仓储
    /// </summary>
    public class BlogThreadRepository : Repository<BlogThread>, IBlogThreadRepository
    {
        IBodyProcessor blogBodyProcessor = DIContainer.ResolveNamed<IBodyProcessor>(TenantTypeIds.Instance().Blog());

        public BlogThreadRepository() { }

        public BlogThreadRepository(PubliclyAuditStatus publiclyAuditStatus)
        {
            this.publiclyAuditStatus = publiclyAuditStatus;
        }

        /// <summary>
        /// 更新日志
        /// </summary>
        /// <param name="blogThread">日志实体</param>
        public override void Update(BlogThread blogThread)
        {
            base.Update(blogThread);

            //更新解析正文缓存
            string cacheKey = GetCacheKeyOfResolvedBody(blogThread.ThreadId);
            string resolveBody = cacheService.Get<string>(cacheKey);
            if (resolveBody != null)
            {
                resolveBody = GetBody(blogThread.ThreadId);

                AtUserService atUserService = new AtUserService(TenantTypeIds.Instance().BlogThread());
                atUserService.ResolveBodyForEdit(resolveBody, blogThread.UserId, blogThread.ThreadId);

                resolveBody = blogBodyProcessor.Process(resolveBody, TenantTypeIds.Instance().BlogThread(), blogThread.ThreadId, blogThread.UserId);
                cacheService.Set(cacheKey, resolveBody, CachingExpirationType.SingleObject);
            }
        }

        /// <summary>
        /// 获取解析的正文
        /// </summary>
        /// <param name="threadId">日志id</param>
        /// <returns>解析后的日志正文</returns>
        public string GetResolvedBody(long threadId)
        {
            string cacheKey = GetCacheKeyOfResolvedBody(threadId);
            string resolveBody = cacheService.Get<string>(cacheKey);

            if (resolveBody == null)
            {
                resolveBody = GetBody(threadId);
                if (!string.IsNullOrEmpty(resolveBody))
                {
                    BlogThread blogThread = Get(threadId);
                    resolveBody = blogBodyProcessor.Process(resolveBody, TenantTypeIds.Instance().BlogThread(), threadId, blogThread.UserId);
                    cacheService.Set(cacheKey, resolveBody, CachingExpirationType.SingleObject);
                }
            }

            return resolveBody;
        }

        /// <summary>
        /// 获取日志正文内容
        /// </summary>
        /// <param name="threadId">日志id</param>
        /// <returns>日志正文内容</returns>
        public string GetBody(long threadId)
        {
            string cacheKey = RealTimeCacheHelper.GetCacheKeyOfEntityBody(threadId);
            string body = cacheService.Get<string>(cacheKey);

            if (body == null)
            {
                BlogThread blogThread = CreateDAO().SingleOrDefault<BlogThread>(threadId);
                if (blogThread == null)
                {
                    return string.Empty;
                }

                body = blogThread.Body;
                cacheService.Add(cacheKey, body, CachingExpirationType.SingleObject);
            }

            return body;
        }

        /// <summary>
        /// 删除某用户时指定其他用户接管数据
        /// </summary>
        /// <param name="userId">要删日志的用户Id</param>
        /// <param name="takeOverUser">要接管日志的用户</param>
        public void TakeOver(long userId, User takeOverUser)
        {
            List<Sql> sqls = new List<Sql>();

            //更新所属为用户的日志（UserId=OwnerId）
            sqls.Add(Sql.Builder.Append("update spb_BlogThreads set UserId = @0,OwnerId=@1,Author=@2 where UserId=OwnerId and UserId=@3", takeOverUser.UserId, takeOverUser.UserId, takeOverUser.DisplayName, userId));

            //更新所属为群组的日志（UserId<>OwnerId）,OwnerId不更新
            sqls.Add(Sql.Builder.Append("update spb_BlogThreads set UserId = @0,Author=@1 where UserId<>OwnerId and UserId=@2", takeOverUser.UserId, takeOverUser.DisplayName, userId));

            //更新被转载用户Id
            sqls.Add(Sql.Builder.Append("update spb_BlogThreads set OriginalAuthorId=@0 where OriginalAuthorId=@1", takeOverUser.UserId, userId));

            CreateDAO().Execute(sqls);




        }

        /// <summary>
        /// 加精/取消精华
        /// </summary>
        /// <param name="threadId">日志id</param>
        /// <param name="isEssential">是否精华</param>
        public void SetEssential(long threadId, bool isEssential)
        {
            var sql = Sql.Builder.Append("update spb_BlogThreads set IsEssential=@0 where ThreadId=@1", isEssential, threadId);

            BlogThread blogThread = Get(threadId);
            blogThread.IsEssential = isEssential;
            base.OnUpdated(blogThread);

            CreateDAO().Execute(sql);
        }

        /// <summary>
        /// 获取上一篇
        /// </summary>
        /// <param name="blogThread">当前日志</param>
        /// <returns>上一篇日志id</returns>
        public long GetPrevThreadId(BlogThread blogThread)
        {
            string cacheKey = RealTimeCacheHelper.GetCacheKeyOfEntity(blogThread.ThreadId) + "-PrevThreadId";
            long? prevThreadId = cacheService.Get(cacheKey) as long?;

            if (!prevThreadId.HasValue)
            {
                prevThreadId = 0;

                var sql = Sql.Builder.Select("ThreadId").From("spb_BlogThreads");
                var whereSql = Sql.Builder.Where("ThreadId < @0", blogThread.ThreadId)
                                          .Where("TenantTypeId = @0", blogThread.TenantTypeId)
                                          .Where("OwnerId = @0", blogThread.OwnerId)
                                          .Where("PrivacyStatus<>@0", PrivacyStatus.Private)
                                          .Where("spb_BlogThreads.IsDraft=0");
                switch (this.PubliclyAuditStatus)
                {
                    case PubliclyAuditStatus.Again:
                    case PubliclyAuditStatus.Fail:
                    case PubliclyAuditStatus.Pending:
                    case PubliclyAuditStatus.Success:
                        whereSql.Where("AuditStatus=@0", this.PubliclyAuditStatus);
                        break;
                    case PubliclyAuditStatus.Again_GreaterThanOrEqual:
                    case PubliclyAuditStatus.Pending_GreaterThanOrEqual:
                        whereSql.Where("AuditStatus>@0", this.PubliclyAuditStatus);
                        break;
                    default:
                        break;
                }
                var orderSql = Sql.Builder.OrderBy("ThreadId DESC");

                sql.Append(whereSql).Append(orderSql);

                var ids_object = CreateDAO().FetchTopPrimaryKeys<BlogThread>(1, sql);
                if (ids_object.Count() > 0)
                {
                    prevThreadId = ids_object.Cast<long>().First();
                }

                cacheService.Add(cacheKey, prevThreadId, CachingExpirationType.SingleObject);
            }
            return prevThreadId.Value;
        }


        /// <summary>
        /// 获取下一篇
        /// </summary>
        /// <param name="blogThread">当前日志</param>
        /// <returns>下一篇日志id</returns>
        public long GetNextThreadId(BlogThread blogThread)
        {
            string cacheKey = RealTimeCacheHelper.GetCacheKeyOfEntity(blogThread.ThreadId) + "-NextThreadId";
            long? nextThreadId = cacheService.Get(cacheKey) as long?;

            if (!nextThreadId.HasValue)
            {
                nextThreadId = 0;

                var sql = Sql.Builder.Select("ThreadId").From("spb_BlogThreads");
                var whereSql = Sql.Builder.Where("ThreadId > @0", blogThread.ThreadId)
                                          .Where("TenantTypeId = @0", blogThread.TenantTypeId)
                                          .Where("OwnerId = @0", blogThread.OwnerId)
                                          .Where("PrivacyStatus<>@0", PrivacyStatus.Private)
                                          .Where("spb_BlogThreads.IsDraft=0");
                switch (this.PubliclyAuditStatus)
                {
                    case PubliclyAuditStatus.Again:
                    case PubliclyAuditStatus.Fail:
                    case PubliclyAuditStatus.Pending:
                    case PubliclyAuditStatus.Success:
                        whereSql.Where("AuditStatus=@0", this.PubliclyAuditStatus);
                        break;
                    case PubliclyAuditStatus.Again_GreaterThanOrEqual:
                    case PubliclyAuditStatus.Pending_GreaterThanOrEqual:
                        whereSql.Where("AuditStatus>@0", this.PubliclyAuditStatus);
                        break;
                    default:
                        break;
                }





                var orderSql = Sql.Builder.OrderBy("ThreadId ASC");

                sql.Append(whereSql).Append(orderSql);

                var ids_object = CreateDAO().FetchTopPrimaryKeys<BlogThread>(1, sql);
                if (ids_object.Count() > 0)
                {
                    nextThreadId = ids_object.Cast<long>().First();
                }

                cacheService.Add(cacheKey, nextThreadId, CachingExpirationType.SingleObject);
            }
            return nextThreadId.Value;
        }

        /// <summary>
        /// 获取Owner的日志
        /// </summary>
        /// <param name="tenantTypeId">租户类型Id</param>
        /// <param name="ownerId">拥有者Id</param>
        /// <param name="ignoreAudit">是否忽略审核状态（作者或管理员查看时忽略审核状态）</param>
        /// <param name="ignorePrivate">是否忽略私有</param>
        /// <param name="categoryId">类别Id</param>
        /// <param name="tagName">标签名称</param>
        /// <param name="isSticky">是否置顶的排在前面</param>
        /// <param name="pageSize">分页大小</param> 
        /// <param name="pageIndex">页码</param>        
        /// <returns>日志分页列表</returns>
        public PagingDataSet<BlogThread> GetOwnerThreads(string tenantTypeId, long ownerId, bool ignoreAudit, bool ignorePrivate, long? categoryId, string tagName, bool isSticky, int pageSize = 20, int pageIndex = 1)
        {
            return GetPagingEntities(pageSize, pageIndex, CachingExpirationType.ObjectCollection,
                       () =>
                       {
                           StringBuilder cacheKey = new StringBuilder();
                           cacheKey.Append(RealTimeCacheHelper.GetListCacheKeyPrefix(CacheVersionType.AreaVersion, "OwnerId", ownerId));
                           cacheKey.AppendFormat("BlogOwner::TenantTypeId-{0}:IgnoreAudit-{1}:IgnorePrivate-{2}:CategoryId-{3}:TagName-{4}:isSticky-{5}", tenantTypeId, ignoreAudit, ignorePrivate, categoryId, tagName, isSticky);
                           return cacheKey.ToString();
                       },
                       () =>
                       {
                           var sql = Sql.Builder.Select("spb_BlogThreads.*").From("spb_BlogThreads");




                           //排除草稿
                           var whereSql = Sql.Builder.Where("spb_BlogThreads.OwnerId=@0", ownerId)
                                                     .Where("spb_BlogThreads.TenantTypeId=@0", tenantTypeId)
                                                     .Where("spb_BlogThreads.IsDraft=0");

                           //置顶日志靠前显示
                           var orderBy = Sql.Builder;
                           if (isSticky)
                           {
                               orderBy.OrderBy("spb_BlogThreads.IsSticky desc")
                                      .OrderBy("spb_BlogThreads.ThreadId desc");
                           }
                           else
                           {
                               orderBy.OrderBy("spb_BlogThreads.ThreadId desc");
                           }

                           if (categoryId.HasValue && categoryId > 0)
                           {
                               sql.InnerJoin("tn_ItemsInCategories").On("spb_BlogThreads.ThreadId=tn_ItemsInCategories.ItemId");


                               whereSql.Where("tn_ItemsInCategories.CategoryId=@0", categoryId.Value);
                           }

                           if (!string.IsNullOrEmpty(tagName))
                           {
                               sql.InnerJoin("tn_ItemsInTags").On("spb_BlogThreads.ThreadId = tn_ItemsInTags.ItemId");


                               whereSql.Where("tn_ItemsInTags.TenantTypeId = @0", TenantTypeIds.Instance().BlogThread())
                                       .Where("tn_ItemsInTags.TagName=@0", tagName);
                           }

                           if (!ignoreAudit)
                           {
                               switch (this.PubliclyAuditStatus)
                               {
                                   case PubliclyAuditStatus.Again:
                                   case PubliclyAuditStatus.Fail:
                                   case PubliclyAuditStatus.Pending:
                                   case PubliclyAuditStatus.Success:
                                       whereSql.Where("AuditStatus=@0", this.PubliclyAuditStatus);
                                       break;
                                   case PubliclyAuditStatus.Again_GreaterThanOrEqual:
                                   case PubliclyAuditStatus.Pending_GreaterThanOrEqual:
                                       whereSql.Where("AuditStatus>@0", this.PubliclyAuditStatus);
                                       break;
                                   default:
                                       break;
                               }
                           }

                           if (ignorePrivate)
                           {
                               whereSql.Where("PrivacyStatus<>@0", PrivacyStatus.Private);
                           }

                           return sql.Append(whereSql).Append(orderBy);
                       });
        }

        /// <summary>
        /// 获取Owner的草稿
        /// </summary>
        /// <param name="tenantTypeId">租户类型id</param>
        /// <param name="ownerId">所有者id</param>
        /// <returns>日志列表</returns>
        public IEnumerable<BlogThread> GetDraftThreads(string tenantTypeId, long ownerId)
        {
            var sql = Sql.Builder.Select("ThreadId").From("spb_BlogThreads")
                                 .Where("TenantTypeId=@0", tenantTypeId)
                                 .Where("OwnerId=@0", ownerId)
                                 .Where("IsDraft=1")
                                 .OrderBy("LastModified DESC");

            return PopulateEntitiesByEntityIds(CreateDAO().FetchFirstColumn(sql));
        }

        /// <summary>
        /// 获取归档项目
        /// </summary>
        /// <param name="tenantTypeId">租户类型Id</param>
        /// <param name="ownerId">所有者id</param>
        /// <param name="ignoreAudit">是否忽略审核状态（作者或管理员查看时忽略审核状态）</param>
        /// <param name="ignorePrivate">是否忽略私有</param>
        /// <returns>归档项目id</returns>
        public IEnumerable<ArchiveItem> GetArchiveItems(string tenantTypeId, long ownerId, bool ignoreAudit, bool ignorePrivate)
        {
            //构建cacheKey
            StringBuilder cacheKey = new StringBuilder();
            cacheKey.AppendFormat("BlogArchiveItems::TenantTypeId-{0}:IgnoreAudit-{1}:IgnorePrivate-{2}:OwnerId-{3}", tenantTypeId, ignoreAudit, ignorePrivate, ownerId);

            //先从缓存里取归档项目列表，如果缓存里没有就去数据库取
            IList<ArchiveItem> archiveItems = cacheService.Get<List<ArchiveItem>>(cacheKey.ToString());

            if (archiveItems == null)
            {
                archiveItems = new List<ArchiveItem>();

                Database dao = CreateDAO();
                try
                {
                    dao.OpenSharedConnection();

                    //取出最早的日志创建日期
                    var sql_first = Sql.Builder.Select("DateCreated")
                                                .From("spb_BlogThreads")
                                                .Where("TenantTypeId=@0", tenantTypeId)
                                                .Where("OwnerId=@0", ownerId)
                                                .Where("IsDraft=0");
                    if (!ignoreAudit)
                    {
                        switch (this.PubliclyAuditStatus)
                        {
                            case PubliclyAuditStatus.Again:
                            case PubliclyAuditStatus.Fail:
                            case PubliclyAuditStatus.Pending:
                            case PubliclyAuditStatus.Success:
                                sql_first.Where("AuditStatus=@0", this.PubliclyAuditStatus);
                                break;
                            case PubliclyAuditStatus.Again_GreaterThanOrEqual:
                            case PubliclyAuditStatus.Pending_GreaterThanOrEqual:
                                sql_first.Where("AuditStatus>@0", this.PubliclyAuditStatus);
                                break;
                            default:
                                break;
                        }
                    }

                    if (ignorePrivate)
                    {
                        sql_first.Where("PrivacyStatus<>@0", PrivacyStatus.Private);
                    }
                    sql_first.OrderBy("ThreadId ASC");



                    DateTime firstDate = dao.ExecuteScalar<DateTime?>(sql_first) ?? DateTime.MinValue;


                    if (firstDate == DateTime.MinValue)
                    {
                        return archiveItems;
                    }




                    int yearFirst = firstDate.ConvertToUserDate().Year;

                    //取出最新的日志创建日期
                    var sql_latest = Sql.Builder.Select("DateCreated")
                                                .From("spb_BlogThreads")
                                                .Where("TenantTypeId=@0", tenantTypeId)
                                                .Where("OwnerId=@0", ownerId)
                                                .Where("IsDraft=0");
                    if (!ignoreAudit)
                    {
                        switch (this.PubliclyAuditStatus)
                        {
                            case PubliclyAuditStatus.Again:
                            case PubliclyAuditStatus.Fail:
                            case PubliclyAuditStatus.Pending:
                            case PubliclyAuditStatus.Success:
                                sql_latest.Where("AuditStatus=@0", this.PubliclyAuditStatus);
                                break;
                            case PubliclyAuditStatus.Again_GreaterThanOrEqual:
                            case PubliclyAuditStatus.Pending_GreaterThanOrEqual:
                                sql_latest.Where("AuditStatus>@0", this.PubliclyAuditStatus);
                                break;
                            default:
                                break;
                        }
                    }

                    if (ignorePrivate)
                    {
                        sql_latest.Where("PrivacyStatus<>@0", PrivacyStatus.Private);
                    }
                    sql_latest.OrderBy("ThreadId DESC");

                    DateTime latestDate = dao.ExecuteScalar<DateTime>(sql_latest);
                    int yearLatest = latestDate.ConvertToUserDate().Year;
                    int monthLatest = latestDate.ConvertToUserDate().Month;

                    //当前年份
                    int yearNow = DateTime.Now.Year;

                    //如果今年有数据
                    if (yearLatest == yearNow)
                    {
                        for (int month = monthLatest; month >= 1; month--)
                        {
                            DateTime beginDate = new DateTime(yearNow, month, 1).ToUniversalTime();
                            DateTime endDate = beginDate.AddMonths(1);

                            var sql_count = Sql.Builder.Select("count(*)")
                                               .From("spb_BlogThreads")
                                               .Where("TenantTypeId=@0", tenantTypeId)
                                               .Where("OwnerId=@0", ownerId)
                                               .Where("IsDraft=0")
                                               .Where("DateCreated >=@0 and DateCreated<@1", beginDate, endDate);
                            if (!ignoreAudit)
                            {
                                switch (this.PubliclyAuditStatus)
                                {
                                    case PubliclyAuditStatus.Again:
                                    case PubliclyAuditStatus.Fail:
                                    case PubliclyAuditStatus.Pending:
                                    case PubliclyAuditStatus.Success:
                                        sql_count.Where("AuditStatus=@0", this.PubliclyAuditStatus);
                                        break;
                                    case PubliclyAuditStatus.Again_GreaterThanOrEqual:
                                    case PubliclyAuditStatus.Pending_GreaterThanOrEqual:
                                        sql_count.Where("AuditStatus>@0", this.PubliclyAuditStatus);
                                        break;
                                    default:
                                        break;
                                }
                            }

                            if (ignorePrivate)
                            {
                                sql_count.Where("PrivacyStatus<>@0", PrivacyStatus.Private);
                            }

                            long count = dao.ExecuteScalar<long>(sql_count);
                            if (count > 0)
                            {


                                ArchiveItem item = new ArchiveItem()
                                {
                                    Year = yearNow,
                                    Month = month,
                                    Count = count
                                };

                                archiveItems.Add(item);
                            }
                        }
                    }

                    //统计往年数据
                    for (int year = yearLatest; year >= yearFirst; year--)
                    {
                        if (year == yearNow)
                        {
                            continue;
                        }

                        DateTime beginDate = new DateTime(year, 1, 1).ToUniversalTime();
                        DateTime endDate = new DateTime(year + 1, 1, 1).ToUniversalTime();

                        var sql_count = Sql.Builder.Select("count(*)")
                                           .From("spb_BlogThreads")
                                           .Where("TenantTypeId=@0", tenantTypeId)
                                           .Where("OwnerId=@0", ownerId)
                                           .Where("IsDraft=0")
                                           .Where("DateCreated >=@0 and DateCreated<@1", beginDate, endDate);
                        if (!ignoreAudit)
                        {
                            switch (this.PubliclyAuditStatus)
                            {
                                case PubliclyAuditStatus.Again:
                                case PubliclyAuditStatus.Fail:
                                case PubliclyAuditStatus.Pending:
                                case PubliclyAuditStatus.Success:
                                    sql_count.Where("AuditStatus=@0", this.PubliclyAuditStatus);
                                    break;
                                case PubliclyAuditStatus.Again_GreaterThanOrEqual:
                                case PubliclyAuditStatus.Pending_GreaterThanOrEqual:
                                    sql_count.Where("AuditStatus>@0", this.PubliclyAuditStatus);
                                    break;
                                default:
                                    break;
                            }
                        }

                        if (ignorePrivate)
                        {
                            sql_count.Where("PrivacyStatus<>@0", PrivacyStatus.Private);
                        }

                        long count = dao.ExecuteScalar<long>(sql_count);
                        if (count > 0)
                        {


                            ArchiveItem item = new ArchiveItem()
                            {
                                Year = year,
                                Count = count
                            };

                            archiveItems.Add(item);
                        }
                    }
                }
                finally
                {
                    dao.CloseSharedConnection();
                }

                //加入缓存
                cacheService.Add(cacheKey.ToString(), archiveItems, CachingExpirationType.UsualObjectCollection);
            }

            return archiveItems;
        }

        /// <summary>
        /// 获取存档的日志分页集合
        /// </summary>
        /// <param name="tenantTypeId">租户类型Id</param>
        /// <param name="ownerId">所有者id</param>
        /// <param name="ignoreAudit">是否忽略审核状态（作者或管理员查看时忽略审核状态）</param>
        /// <param name="ignorePrivate">是否忽略私有</param>
        /// <param name="archivePeriod">归档阶段</param>
        /// <param name="archiveItem">归档日期标识</param>
        /// <param name="pageSize">分页大小</param>
        /// <param name="pageIndex">页码</param>
        /// <returns>日志分页列表</returns>
        public PagingDataSet<BlogThread> GetsForArchive(string tenantTypeId, long ownerId, bool ignoreAudit, bool ignorePrivate, ArchivePeriod archivePeriod, ArchiveItem archiveItem, int pageSize = 20, int pageIndex = 1)
        {
            return GetPagingEntities(pageSize, pageIndex, CachingExpirationType.ObjectCollection,
                    () =>
                    {
                        StringBuilder cacheKey = new StringBuilder();
                        cacheKey.Append(RealTimeCacheHelper.GetListCacheKeyPrefix(CacheVersionType.AreaVersion, "OwnerId", ownerId));
                        cacheKey.AppendFormat("BlogArchives::TenantTypeId-{0}:ArchivePeriod-{1}:ArchiveItem-{2}:IgnoreAudit-{3}:IgnorePrivate-{4}", tenantTypeId, archivePeriod, archiveItem, ignoreAudit, ignorePrivate);

                        return cacheKey.ToString();
                    },
                    () =>
                    {
                        var sql = Sql.Builder.Select("spb_BlogThreads.*")
                                             .From("spb_BlogThreads")
                                             .Where("TenantTypeId=@0", tenantTypeId)
                                             .Where("OwnerId=@0", ownerId)
                                             .Where("IsDraft=0");
                        if (!ignoreAudit)
                        {
                            switch (this.PubliclyAuditStatus)
                            {
                                case PubliclyAuditStatus.Again:
                                case PubliclyAuditStatus.Fail:
                                case PubliclyAuditStatus.Pending:
                                case PubliclyAuditStatus.Success:
                                    sql.Where("AuditStatus=@0", this.PubliclyAuditStatus);
                                    break;
                                case PubliclyAuditStatus.Again_GreaterThanOrEqual:
                                case PubliclyAuditStatus.Pending_GreaterThanOrEqual:
                                    sql.Where("AuditStatus>@0", this.PubliclyAuditStatus);
                                    break;
                                default:
                                    break;
                            }
                        }

                        if (ignorePrivate)
                        {
                            sql.Where("PrivacyStatus<>@0", PrivacyStatus.Private);
                        }

                        DateTime beginDate = new DateTime(1900, 1, 1);
                        DateTime endDate = DateTime.Now;
                        switch (archivePeriod)
                        {
                            case ArchivePeriod.Year:
                                beginDate = new DateTime(archiveItem.Year, 1, 1).ToUniversalTime();
                                endDate = new DateTime(archiveItem.Year + 1, 1, 1).ToUniversalTime();
                                break;
                            case ArchivePeriod.Month:
                                beginDate = new DateTime(archiveItem.Year, archiveItem.Month, 1).ToUniversalTime();
                                endDate = beginDate.AddMonths(1);
                                break;
                            case ArchivePeriod.Day:
                                beginDate = new DateTime(archiveItem.Year, archiveItem.Month, archiveItem.Day).ToUniversalTime();
                                endDate = new DateTime(archiveItem.Year, archiveItem.Month, archiveItem.Day + 1).ToUniversalTime();
                                break;
                            default:
                                break;
                        }
                        sql.Where("DateCreated >= @0 and DateCreated < @1", beginDate, endDate).OrderBy("ThreadId desc");
                        return sql;
                    });
        }

        /// <summary>
        /// 获取日志的排行数据
        /// </summary>
        /// <param name="tenantTypeId">租户类型Id</param>
        /// <param name="topNumber">前多少条</param>
        /// <param name="categoryId">类别Id</param>
        /// <param name="isEssential">是否精华</param>
        /// <param name="sortBy">日志排序依据</param>
        /// <returns>日志列表</returns>
        public IEnumerable<BlogThread> GetTops(string tenantTypeId, int topNumber, long? categoryId, bool? isEssential, SortBy_BlogThread sortBy)
        {
            return GetTopEntities(topNumber, CachingExpirationType.UsualObjectCollection,
                    () =>
                    {
                        StringBuilder cacheKey = new StringBuilder();
                        cacheKey.AppendFormat("BlogTops::TenantTypeId-{0}:CategoryId-{1}:IsEssential-{2}:SortBy-{3}", tenantTypeId, categoryId, isEssential, sortBy);
                        return cacheKey.ToString();
                    },
                    () =>
                    {
                        var sql = Sql.Builder.Select("spb_BlogThreads.*")
                                             .From("spb_BlogThreads");

                        var whereSql = Sql.Builder.Where("spb_BlogThreads.PrivacyStatus<>@0", PrivacyStatus.Private)
                                                  .Where("spb_BlogThreads.IsDraft=0");

                        if (!string.IsNullOrEmpty(tenantTypeId))
                        {
                            whereSql.Where("spb_BlogThreads.TenantTypeId=@0", tenantTypeId);
                        }

                        if (categoryId.HasValue && categoryId > 0)
                        {
                            sql.InnerJoin("tn_ItemsInCategories").On("spb_BlogThreads.ThreadId=tn_ItemsInCategories.ItemId");
                            whereSql.Where("tn_ItemsInCategories.CategoryId=@0", categoryId.Value);
                        }

                        switch (this.PubliclyAuditStatus)
                        {
                            case PubliclyAuditStatus.Again:
                            case PubliclyAuditStatus.Fail:
                            case PubliclyAuditStatus.Pending:
                            case PubliclyAuditStatus.Success:
                                whereSql.Where("spb_BlogThreads.AuditStatus=@0", this.PubliclyAuditStatus);
                                break;
                            case PubliclyAuditStatus.Again_GreaterThanOrEqual:
                            case PubliclyAuditStatus.Pending_GreaterThanOrEqual:
                                whereSql.Where("spb_BlogThreads.AuditStatus>@0", this.PubliclyAuditStatus);
                                break;
                            default:
                                break;
                        }
                        if (isEssential.HasValue)
                        {
                            whereSql.Where("spb_BlogThreads.IsEssential=@0", isEssential);
                        }


                        var orderSql = Sql.Builder;

                        CountService countService = new CountService(TenantTypeIds.Instance().BlogThread());
                        string countTableName = countService.GetTableName_Counts();
                        switch (sortBy)
                        {
                            case SortBy_BlogThread.DateCreated_Desc:
                                orderSql.OrderBy("spb_BlogThreads.ThreadId desc");
                                break;
                            case SortBy_BlogThread.CommentCount:
                                sql.LeftJoin(string.Format("(select * from {0} WHERE ({0}.CountType = '{1}')) c", countTableName, CountTypes.Instance().CommentCount()))
                                .On("ThreadId = c.ObjectId");
                                orderSql.OrderBy("c.StatisticsCount desc");
                                break;
                            case SortBy_BlogThread.StageHitTimes:
                                StageCountTypeManager stageCountTypeManager = StageCountTypeManager.Instance(TenantTypeIds.Instance().BlogThread());
                                int stageCountDays = stageCountTypeManager.GetMaxDayCount(CountTypes.Instance().HitTimes());
                                string stageCountType = stageCountTypeManager.GetStageCountType(CountTypes.Instance().HitTimes(), stageCountDays);
                                sql.LeftJoin(string.Format("(select * from {0} WHERE ({0}.CountType = '{1}')) c", countTableName, stageCountType))
                                .On("ThreadId = c.ObjectId");
                                orderSql.OrderBy("c.StatisticsCount desc");
                                break;
                            default:


                                orderSql.OrderBy("spb_BlogThreads.ThreadId desc");
                                break;
                        }
                        return sql.Append(whereSql).Append(orderSql);
                    });
        }

        /// <summary>
        /// 获取日志排行分页集合
        /// </summary>
        /// <remarks>rss订阅也使用方法</remarks>
        /// <param name="tenantTypeId">租户类型Id</param>
        /// <param name="ownerId">所有者id</param>
        /// <param name="categoryId">类别Id</param>
        /// <param name="tagName">标签名称</param>
        /// <param name="isEssential">是否精华</param>
        /// <param name="sortBy">帖子排序依据</param>
        /// <param name="pageSize">分页大小</param>
        /// <param name="pageIndex">页码</param>
        /// <returns>日志分页列表</returns>
        public PagingDataSet<BlogThread> Gets(string tenantTypeId, long? ownerId, bool ignoreAudit, bool ignorePrivate, long? categoryId, string tagName, bool? isEssential, SortBy_BlogThread sortBy = SortBy_BlogThread.DateCreated_Desc, int pageSize = 20, int pageIndex = 1)
        {
            return GetPagingEntities(pageSize, pageIndex, CachingExpirationType.UsualObjectCollection,
                      () =>
                      {
                          StringBuilder cacheKey = new StringBuilder();
                          if (ownerId.HasValue)
                          {
                              cacheKey.Append(RealTimeCacheHelper.GetListCacheKeyPrefix(CacheVersionType.AreaVersion, "OwnerId", ownerId.Value));
                          }
                          cacheKey.AppendFormat("GetBlogs::TenantTypeId-{0}:CategoryId-{1}:TagName-{2}:IsEssential-{3}:SortBy-{4}", tenantTypeId, categoryId, tagName, isEssential, sortBy);
                          return cacheKey.ToString();
                      },
                      () =>
                      {
                          var sql = Sql.Builder.Select("spb_BlogThreads.*").From("spb_BlogThreads");
                          var whereSql = Sql.Builder.Where("spb_BlogThreads.IsDraft=0");
                          var orderSql = Sql.Builder;

                          if (!string.IsNullOrEmpty(tenantTypeId))
                          {
                              whereSql.Where("spb_BlogThreads.TenantTypeId=@0", tenantTypeId);
                          }

                          if (ownerId.HasValue)
                          {
                              whereSql.Where("spb_BlogThreads.OwnerId=@0", ownerId.Value);
                          }

                          if (!ignoreAudit)
                          {
                              switch (this.PubliclyAuditStatus)
                              {
                                  case PubliclyAuditStatus.Again:
                                  case PubliclyAuditStatus.Fail:
                                  case PubliclyAuditStatus.Pending:
                                  case PubliclyAuditStatus.Success:
                                      whereSql.Where("spb_BlogThreads.AuditStatus=@0", this.PubliclyAuditStatus);
                                      break;
                                  case PubliclyAuditStatus.Again_GreaterThanOrEqual:
                                  case PubliclyAuditStatus.Pending_GreaterThanOrEqual:
                                      whereSql.Where("spb_BlogThreads.AuditStatus>@0", this.PubliclyAuditStatus);
                                      break;
                                  default:
                                      break;
                              }
                          }

                          if (ignorePrivate)
                          {
                              whereSql.Where("spb_BlogThreads.PrivacyStatus<>@0", PrivacyStatus.Private);
                          }

                          if (categoryId.HasValue && categoryId > 0)
                          {
                              sql.InnerJoin("tn_ItemsInCategories").On("spb_BlogThreads.ThreadId=tn_ItemsInCategories.ItemId");


                              whereSql.Where("tn_ItemsInCategories.CategoryId=@0", categoryId.Value);
                          }

                          if (!string.IsNullOrEmpty(tagName))
                          {
                              sql.InnerJoin("tn_ItemsInTags").On("spb_BlogThreads.ThreadId = tn_ItemsInTags.ItemId");


                              whereSql.Where("tn_ItemsInTags.TenantTypeId = @0", TenantTypeIds.Instance().BlogThread())
                                      .Where("tn_ItemsInTags.TagName=@0", tagName);
                          }

                          if (isEssential.HasValue)
                          {
                              whereSql.Where("spb_BlogThreads.IsEssential=@0", isEssential.Value);
                          }

                          CountService countService = new CountService(TenantTypeIds.Instance().BlogThread());
                          string countTableName = countService.GetTableName_Counts();
                          switch (sortBy)
                          {
                              case SortBy_BlogThread.DateCreated_Desc:
                                  orderSql.OrderBy("spb_BlogThreads.ThreadId desc");
                                  break;
                              case SortBy_BlogThread.CommentCount:
                                  sql.LeftJoin(string.Format("(select * from {0} WHERE ({0}.CountType = '{1}')) c", countTableName, CountTypes.Instance().CommentCount()))
                                  .On("ThreadId = c.ObjectId");
                                  orderSql.OrderBy("c.StatisticsCount desc");
                                  break;
                              case SortBy_BlogThread.StageHitTimes:
                                  StageCountTypeManager stageCountTypeManager = StageCountTypeManager.Instance(TenantTypeIds.Instance().BlogThread());
                                  int stageCountDays = stageCountTypeManager.GetMaxDayCount(CountTypes.Instance().HitTimes());
                                  string stageCountType = stageCountTypeManager.GetStageCountType(CountTypes.Instance().HitTimes(), stageCountDays);
                                  sql.LeftJoin(string.Format("(select * from {0} WHERE ({0}.CountType = '{1}')) c", countTableName, stageCountType))
                                  .On("ThreadId = c.ObjectId");
                                  orderSql.OrderBy("c.StatisticsCount desc");
                                  break;
                              default:
                                  orderSql.OrderBy("spb_BlogThreads.ThreadId desc");
                                  break;
                          }
                          sql.Append(whereSql).Append(orderSql);
                          return sql;
                      });
        }

        /// <summary>
        /// 获取日志排行分页集合
        /// </summary>
        /// <param name="tenantTypeId">租户类型Id</param>
        /// <param name="auditStatus">审核状态</param>
        /// <param name="categoryId">站点类别Id</param>
        /// <param name="isEssential">是否精华</param>
        /// <param name="ownerId">OwnerId</param>
        /// <param name="subjectKeywords">标题关键字</param>        
        /// <param name="pageSize">分页大小</param>
        /// <param name="pageIndex">页码</param>
        /// <returns>日志分页列表</returns>
        public PagingDataSet<BlogThread> GetsForAdmin(string tenantTypeId, AuditStatus? auditStatus, long? categoryId, bool? isEssential, long? ownerId, string subjectKeywords, int pageSize = 20, int pageIndex = 1)
        {
            var sql = Sql.Builder.Select("spb_BlogThreads.*").From("spb_BlogThreads");
            var whereSql = Sql.Builder.Where("spb_BlogThreads.IsDraft=0");

            if (!string.IsNullOrEmpty(tenantTypeId))
            {
                whereSql.Where("spb_BlogThreads.TenantTypeId=@0", tenantTypeId);
            }

            if (auditStatus.HasValue)
            {
                whereSql.Where("spb_BlogThreads.AuditStatus=@0", auditStatus.Value);
            }
            if (categoryId.HasValue && categoryId > 0)
            {
                sql.InnerJoin("tn_ItemsInCategories").On("spb_BlogThreads.ThreadId=tn_ItemsInCategories.ItemId");


                whereSql.Where("tn_ItemsInCategories.CategoryId=@0", categoryId.Value);
            }
            if (isEssential.HasValue)
            {
                whereSql.Where("spb_BlogThreads.IsEssential=@0", isEssential);
            }
            if (ownerId.HasValue)
            {
                whereSql.Where("spb_BlogThreads.OwnerId=@0", ownerId);
            }
            if (!string.IsNullOrEmpty(subjectKeywords))
            {
                whereSql.Where("spb_BlogThreads.Subject like @0", "%" + StringUtility.StripSQLInjection(subjectKeywords) + "%");
            }

            sql.Append(whereSql).OrderBy("spb_BlogThreads.ThreadId desc");

            return GetPagingEntities(pageSize, pageIndex, sql);
        }

        /// <summary>
        /// 获取日志管理数据
        /// </summary>
        /// <param name="tenantTypeId">租户类型Id（可以获取该应用下针对某种租户类型的统计计数，默认不进行筛选）</param>
        /// <returns>日志管理数据</returns>
        public Dictionary<string, long> GetManageableDatas(string tenantTypeId = null)
        {
            Dictionary<string, long> manageableDatas = new Dictionary<string, long>();

            //查询待审核数
            Sql sql = Sql.Builder.Select("count(*)")
                                 .From("spb_BlogThreads")
                                 .Where("IsDraft=0")
                                 .Where("AuditStatus=@0", AuditStatus.Pending);
            if (!string.IsNullOrEmpty(tenantTypeId))
            {
                sql.Where("TenantTypeId=@0", tenantTypeId);
            }
            manageableDatas.Add(ApplicationStatisticDataKeys.Instance().PendingCount(), CreateDAO().FirstOrDefault<long>(sql));

            //查询需再审核数
            sql = Sql.Builder.Select("count(*)")
                             .From("spb_BlogThreads")
                             .Where("IsDraft=0")
                             .Where("AuditStatus=@0", AuditStatus.Again);
            if (!string.IsNullOrEmpty(tenantTypeId))
            {
                sql.Where("TenantTypeId=@0", tenantTypeId);
            }
            manageableDatas.Add(ApplicationStatisticDataKeys.Instance().AgainCount(), CreateDAO().FirstOrDefault<long>(sql));

            return manageableDatas;
        }

        /// <summary>
        /// 获取日志统计数据
        /// </summary>
        /// <param name="tenantTypeId">租户类型Id（可以获取该应用下针对某种租户类型的统计计数，默认不进行筛选）</param>
        /// <returns>日志统计数据</returns>
        public Dictionary<string, long> GetStatisticDatas(string tenantTypeId = null)
        {
            string cacheKey = "BlogThreadStatisticData";
            Dictionary<string, long> statisticDatas = cacheService.Get<Dictionary<string, long>>(cacheKey);
            if (statisticDatas == null)
            {
                statisticDatas = new Dictionary<string, long>();

                //查询总数
                Sql sql = Sql.Builder.Select("count(*)")
                                     .From("spb_BlogThreads")
                                     .Where("IsDraft=0");
                if (!string.IsNullOrEmpty(tenantTypeId))
                {
                    sql.Where("TenantTypeId=@0", tenantTypeId);
                }
                statisticDatas.Add(ApplicationStatisticDataKeys.Instance().TotalCount(), CreateDAO().FirstOrDefault<long>(sql));

                //查询24小时新增数
                sql = Sql.Builder.Select("count(*)")
                                 .From("spb_BlogThreads")
                                 .Where("IsDraft=0")
                                 .Where("DateCreated > @0", DateTime.UtcNow.AddDays(-1));
                if (!string.IsNullOrEmpty(tenantTypeId))
                {
                    sql.Where("TenantTypeId=@0", tenantTypeId);
                }
                statisticDatas.Add(ApplicationStatisticDataKeys.Instance().Last24HCount(), CreateDAO().FirstOrDefault<long>(sql));

                cacheService.Add(cacheKey, statisticDatas, CachingExpirationType.UsualSingleObject);
            }

            return statisticDatas;
        }

        /// <summary>
        /// 获取解析正文缓存Key
        /// </summary>
        /// <param name="threadId">日志id</param>
        /// <returns>正文缓存key</returns>
        private string GetCacheKeyOfResolvedBody(long threadId)
        {
            return "BlogThreadResolvedBody" + threadId;
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
                    publiclyAuditStatus = auditService.GetPubliclyAuditStatus(ApplicationIds.Instance().Blog());
                }
                return publiclyAuditStatus.Value;
            }
            set
            {
                this.publiclyAuditStatus = value;
            }
        }

    }
}
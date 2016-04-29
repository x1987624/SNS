//<TunynetCopyright>
//--------------------------------------------------------------
//<copyright>拓宇网络科技有限公司 2005-2012</copyright>
//<version>V0.5</verion>
//<createdate>2012-08-28</createdate>
//<author>zhengw</author>
//<email>zhengw@tunynet.com</email>
//<log date="2012-08-28" version="0.5">创建</log>
//--------------------------------------------------------------
//</TunynetCopyright>

using System;
using System.Collections.Generic;
using System.Text;
using PetaPoco;
using Tunynet;
using Tunynet.Caching;
using Tunynet.Repositories;
using Tunynet.Common;
using System.Linq;
using Tunynet.Utilities;
using Spacebuilder.Common;

namespace Spacebuilder.Bar
{
    /// <summary>
    ///回帖Repository
    /// </summary>
    public class BarPostRepository : Repository<BarPost>, IBarPostRepository
    {
        private int pageSize = 20;
        private int childrenPageSize = 5;
        IBodyProcessor barBodyProcessor = DIContainer.ResolveNamed<IBodyProcessor>(TenantTypeIds.Instance().Bar());

        /// <summary>
        /// 构造器
        /// </summary>
        public BarPostRepository()
        {

        }

        /// <summary>
        /// 构造器
        /// </summary>
        /// <param name="publiclyAuditStatus"></param>
        public BarPostRepository(PubliclyAuditStatus publiclyAuditStatus)
        {
            this.publiclyAuditStatus = publiclyAuditStatus;
        }

        /// <summary>
        /// 插入回复贴
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public override object Insert(BarPost entity)
        {
            Database dao = CreateDAO();
            dao.OpenSharedConnection();

            object id_object = base.Insert(entity);
            long id = 0;
            long.TryParse(id_object.ToString(), out id);
            if (id > 0)
            {
                //需同时更新主题帖的最后更新时间
                Sql sql = Sql.Builder;
                sql.Append("Update spb_BarThreads Set PostCount=PostCount+1,LastModified=@0", DateTime.UtcNow)
                .Where("ThreadId=@0", entity.ThreadId);
                dao.Execute(sql);
                //更新父回帖的子回复数
                if (entity.ParentId > 0)
                {
                    sql = Sql.Builder.Append("Update spb_BarPosts Set ChildPostCount=ChildPostCount+1")
                       .Where("PostId=@0", entity.ParentId);
                    dao.Execute(sql);
                }
                //更新主题帖最新回复的缓存
                string cacheKey = GetCacheKey_NewestPost(entity.ThreadId);
                cacheService.Set(cacheKey, entity, CachingExpirationType.UsualSingleObject);
            }
            dao.CloseSharedConnection();
            return id_object;
        }

        /// <summary>
        /// 更新帖子
        /// </summary>
        /// <param name="entity"></param>
        public override void Update(BarPost entity)
        {
            base.Update(entity);
            //更新解析正文缓存
            string cacheKey = GetCacheKeyOfResolvedBody(entity.PostId);
            string resolveBody = cacheService.Get<string>(cacheKey);
            if (resolveBody != null)
            {
                resolveBody = entity.GetBody();
                resolveBody = barBodyProcessor.Process(resolveBody, TenantTypeIds.Instance().BarPost(), entity.PostId, entity.UserId);
                cacheService.Set(cacheKey, resolveBody, CachingExpirationType.SingleObject);
            }
        }

        /// <summary>
        /// 删除回复贴
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public override int Delete(BarPost entity)
        {
            Database dao = CreateDAO();
            dao.OpenSharedConnection();

            //删除父回帖时，同时将其子回帖删除
            var sql = Sql.Builder.Append("delete from spb_BarPosts where ParentId = @0", entity.PostId);


            dao.Execute(sql);

            int affectCount = base.Delete(entity);
            if (affectCount > 0)
            {
                //更新主题帖的回帖数
                sql = Sql.Builder;
                sql.Append("Update spb_BarThreads Set PostCount=PostCount-@0", entity.ChildPostCount + 1)
                .Where("ThreadId=@0", entity.ThreadId);
                dao.Execute(sql);
                //更新父回帖的子回复数
                if (entity.ParentId > 0)
                {
                    sql = Sql.Builder.Append("Update spb_BarPosts Set ChildPostCount=ChildPostCount-1")
                       .Where("PostId=@0", entity.ParentId);
                    dao.Execute(sql);
                }
            }


            dao.CloseSharedConnection();
            return affectCount;
        }

        /// <summary>
        /// 获取某个帖子下的所有回帖（用于删除帖子）
        /// </summary>
        /// <param name="threadId"></param>
        /// <returns></returns>
        public IEnumerable<BarPost> GetAllPostsOfThread(long threadId)
        {



            var sql = Sql.Builder;
            sql.Select("*")
            .From("spb_BarPosts")
            .Where("ThreadId=@0", threadId);
            return CreateDAO().Fetch<BarPost>(sql);
        }


        /// <summary>
        /// 获取某个用户的所有回帖（用于删除用户）
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public IEnumerable<BarPost> GetAllPostsOfUser(long userId)
        {
            var sql = Sql.Builder;
            sql.Select("*")
            .From("spb_BarPosts")
            .Where("UserId=@0", userId);
            return CreateDAO().Fetch<BarPost>(sql);
        }

        /// <summary>
        /// 获取解析的正文
        /// </summary>
        /// <param name="postId"></param>
        /// <returns></returns>
        public string GetResolvedBody(long postId)
        {
            BarPost barPost = Get(postId);
            if (barPost == null)
                return string.Empty;

            string cacheKey = GetCacheKeyOfResolvedBody(postId);
            string resolveBody = cacheService.Get<string>(cacheKey);
            if (resolveBody == null)
            {
                resolveBody = barPost.GetBody();
                resolveBody = barBodyProcessor.Process(resolveBody, TenantTypeIds.Instance().BarPost(), barPost.PostId, barPost.UserId);
                cacheService.Set(cacheKey, resolveBody, CachingExpirationType.SingleObject);
            }
            return resolveBody;
        }

        /// <summary>
        /// 获取BarPost内容
        /// </summary>
        /// <param name="postId"></param>
        /// <returns></returns>
        public string GetBody(long postId)
        {
            string cacheKey = RealTimeCacheHelper.GetCacheKeyOfEntityBody(postId);
            string body = cacheService.Get<string>(cacheKey);
            if (body == null)
            {
                BarPost BarPost = CreateDAO().SingleOrDefault<BarPost>(postId);
                body = BarPost != null ? BarPost.Body : string.Empty;
                cacheService.Add(cacheKey, body, CachingExpirationType.SingleObject);
            }
            return body;
        }

        /// <summary>
        /// 用户某一段时间内对哪些帖子回过帖
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="beforeDays"></param>
        /// <returns></returns>
        public IEnumerable<long> GetThreadIdsByUser(long userId, int beforeDays)
        {
            string cacheKey = RealTimeCacheHelper.GetListCacheKeyPrefix(CacheVersionType.AreaVersion, "UserId", userId) + "BarPostThreadIdsOfUser";
            List<long> threadIds = cacheService.Get<List<long>>(cacheKey);
            if (threadIds == null)
            {
                var sql = Sql.Builder;
                sql.Select("ThreadId")
                .From("spb_BarPosts")
                .Where("UserId = @0", userId)
                .Where("DateCreated > @0", DateTime.UtcNow.AddDays(-beforeDays));
                sql.OrderBy("PostId desc");
                IEnumerable<object> threadIds_object = CreateDAO().FetchFirstColumn(sql);
                threadIds = threadIds_object.Cast<long>().ToList();
                cacheService.Add(cacheKey, threadIds, CachingExpirationType.UsualObjectCollection);
            }
            return threadIds;
        }


        /// <summary>
        /// 获取主题帖最新的一条回复
        /// </summary>
        /// <param name="threadId"></param>
        /// <returns>回复贴（若没有，则返回null）</returns>
        public BarPost GetNewestPost(long threadId)
        {

            string cacheKey = GetCacheKey_NewestPost(threadId);
            BarPost post = cacheService.Get<BarPost>(cacheKey);
            if (post == null)
            {
                var sql = PetaPoco.Sql.Builder;
                sql.Select("*")
                   .From("spb_BarPosts")
                   .Where("ThreadId=@0", threadId);
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
                sql.OrderBy("PostId desc");


                var ids_object = CreateDAO().FetchTopPrimaryKeys<BarPost>(1, sql);
                if (ids_object.Count() > 0)
                {
                    var postId = ids_object.First();
                    post = Get(postId);
                }

                if (post != null)
                    cacheService.Add(cacheKey, post, CachingExpirationType.UsualSingleObject);
            }
            return post;
        }

        /// <summary>
        /// 获取我的回复贴分页集合
        /// </summary>
        /// <param name="userId">回复贴作者Id</param>
        /// <param name="tenantTypeId">租户类型Id</param>
        /// <param name="pageIndex">页码</param>
        /// <returns>回复贴列表</returns>
        public PagingDataSet<BarPost> GetMyPosts(long userId, string tenantTypeId = null, int pageIndex = 1)
        {
            //不必筛选审核状态
            //缓存周期：对象集合，需要维护即时性
            //排序：发布时间（倒序）

            return GetPagingEntities(pageSize, pageIndex, CachingExpirationType.ObjectCollection,
                    () =>
                    {
                        StringBuilder cacheKey = new StringBuilder();
                        cacheKey.Append(RealTimeCacheHelper.GetListCacheKeyPrefix(CacheVersionType.AreaVersion, "UserId", userId));
                        cacheKey.AppendFormat("MyBarPosts::TenantTypeId-{0}", tenantTypeId);
                        return cacheKey.ToString();
                    },
                    () =>
                    {
                        var sql = Sql.Builder;
                        sql.Select("*")
                        .From("spb_BarPosts")
                        .Where("UserId = @0", userId);
                        if (!string.IsNullOrEmpty(tenantTypeId))
                            sql.Where("TenantTypeId = @0", tenantTypeId);
                        //if (ownerId.HasValue && ownerId.Value > 0)
                        //    sql.Where("OwnerId = @0", ownerId.Value);
                        sql.OrderBy("PostId desc");
                        return sql;
                    });
        }

        /// <summary>
        /// 获取主题帖的回帖排行分页集合
        /// </summary>
        /// <param name="threadId">主题帖Id</param>
        /// <param name="sortBy">回帖排序依据（默认为按创建时间正序排序）</param>
        /// <param name="onlyStarter">仅看楼主</param>
        /// <param name="starterId">楼主Id</param>
        /// <param name="pageIndex">页码</param>
        /// <returns>回复贴列表</returns>
        public PagingDataSet<BarPost> Gets(long threadId, bool onlyStarter, long starterId, SortBy_BarPost sortBy, int pageIndex)
        {
            //只获取可对外显示审核状态的回复贴
            //缓存周期：对象集合，需要维护即时性
            //排序：发布时间（倒序）
            return GetPagingEntities(pageSize, pageIndex, CachingExpirationType.ObjectCollection,
                    () =>
                    {
                        return GetCacheKey_PostIdsOfThread(threadId, onlyStarter, sortBy);
                    },
                    () =>
                    {
                        return GetSql_Posts(threadId, onlyStarter, starterId, sortBy);
                    });
        }



        /// <summary>
        /// 获取回帖的管理列表
        /// </summary>
        /// <param name="tenantTypeId">租户类型Id</param>
        /// <param name="query">帖子查询条件</param>
        /// <param name="pageSize">每页多少条数据</param>
        /// <param name="pageIndex">页码</param>
        /// <returns>帖子分页集合</returns>
        public PagingDataSet<BarPost> Gets(string tenantTypeId, BarPostQuery query, int pageSize, int pageIndex)
        {
            //当SubjectKeyword、StartDate、EndDate为null时，进行缓存
            //当SectionId不为null时，使用分区版本，分区名为：SectionId，否则使用全局版本
            //缓存周期：对象集合，需要维护即时性
            //使用用户选择器设置query.UserId参数
            //排序：发布时间（倒序）

            var sql = Sql.Builder;
            sql.Select("*")
            .From("spb_BarPosts")
            .Where("TenantTypeId = @0", tenantTypeId);

            if (query.UserId != null && query.UserId > 0)
                sql.Where("UserId = @0", query.UserId);

            if (query.SectionId != null && query.SectionId > 0)
                sql.Where("SectionId = @0", query.SectionId);

            if (query.AuditStatus != null)
                sql.Where("AuditStatus = @0", query.AuditStatus);

            if (!string.IsNullOrEmpty(query.PostKeyword))
                sql.Where("Body like @0", "%" + StringUtility.StripSQLInjection(query.PostKeyword) + "%");

            if (query.StartDate != null)
                sql.Where("DateCreated >= @0", query.StartDate);
            if (query.EndDate != null)
                sql.Where("DateCreated < @0", query.EndDate.Value.AddDays(1));
            sql.OrderBy("PostId desc");

            return GetPagingEntities(pageSize, pageIndex, sql);
        }

        /// <summary>
        /// 获取子级回帖列表
        /// </summary>
        /// <param name="parentId">父回帖Id</param>
        /// <param name="pageIndex">当前页码</param>
        /// <param name="sortBy">排序字段</param> 
        /// <returns></returns>
        public PagingDataSet<BarPost> GetChildren(long parentId, int pageIndex, SortBy_BarPost sortBy)
        {

            return GetPagingEntities(childrenPageSize, pageIndex, CachingExpirationType.ObjectCollection,
                    () =>
                    {
                        return GetCacheKey_GetChildren(sortBy, parentId);
                    },
                    () =>
                    {
                        var sql = Sql.Builder;
                        sql.Select("*")
                        .From("spb_BarPosts")
                        .Where("ParentId = @0", parentId);

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
                        if (sortBy == SortBy_BarPost.DateCreated_Desc)
                            sql.OrderBy("PostId desc");
                        return sql;
                    });
        }

        #region CacheKey

        private string GetCacheKey_GetChildren(SortBy_BarPost sortBy, long parentId)
        {
            StringBuilder cacheKey = new StringBuilder();
            cacheKey.Append(RealTimeCacheHelper.GetListCacheKeyPrefix(CacheVersionType.AreaVersion, "ParentId", parentId));
            cacheKey.AppendFormat("ChildBarPosts::SortBy-{0}:ParentId-{1}", (int)sortBy, parentId);
            return cacheKey.ToString();
        }

        /// <summary>
        /// 获取主题帖最新的一条回复的CacheKey
        /// </summary>
        /// <returns></returns>
        private string GetCacheKey_NewestPost(long threadId)
        {
            return string.Format("NewestPost::V-{0}:ThreadId-{1}", RealTimeCacheHelper.GetAreaVersion("ThreadId", threadId), threadId);
        }

        /// <summary>
        /// 获取解析正文缓存Key
        /// </summary>
        /// <param name="postId"></param>
        /// <returns></returns>
        private string GetCacheKeyOfResolvedBody(long postId)
        {
            return "BarPostResolvedBody" + postId;
        }

        /// <summary>
        /// 获取主题帖下回帖列表缓存
        /// </summary>
        /// <param name="threadId"></param>
        /// <param name="onlyStarter"></param>
        /// <param name="sortBy"></param>
        /// <returns></returns>
        private string GetCacheKey_PostIdsOfThread(long threadId, bool onlyStarter, SortBy_BarPost sortBy)
        {
            StringBuilder cacheKey = new StringBuilder();
            cacheKey.Append(RealTimeCacheHelper.GetListCacheKeyPrefix(CacheVersionType.AreaVersion, "ThreadId", threadId));
            cacheKey.AppendFormat("BarPostsOfThread::SortBy-{0}:onlyLandlord-{1}", (int)sortBy, onlyStarter);
            return cacheKey.ToString();
        }


        private PubliclyAuditStatus? publiclyAuditStatus;
        /// <summary>
        /// 帖吧应用可对外显示的审核状态
        /// </summary>
        protected PubliclyAuditStatus PubliclyAuditStatus
        {
            get
            {
                if (publiclyAuditStatus == null)
                {
                    AuditService auditService = new AuditService();
                    publiclyAuditStatus = auditService.GetPubliclyAuditStatus(ApplicationIds.Instance().Bar());
                }
                return publiclyAuditStatus.Value;
            }
            set
            {
                this.publiclyAuditStatus = value;
            }
        }
        #endregion

        #region Sql
        /// <summary>
        /// 获取主题帖的回帖列表的sql语句
        /// </summary>
        /// <param name="threadId">主题帖Id</param>
        /// <param name="sortBy">回帖排序依据（默认为按创建时间正序排序）</param>
        /// <param name="onlyStarter">仅看楼主</param>
        /// <param name="starterId">楼主Id</param>
        /// <returns></returns>
        private Sql GetSql_Posts(long threadId, bool onlyStarter, long starterId, SortBy_BarPost sortBy)
        {
            var sql = Sql.Builder;
            sql.Select("*")
            .From("spb_BarPosts")
            .Where("ParentId = 0")
            .Where("ThreadId = @0", threadId);

            if (onlyStarter)
                sql.Where("UserId = @0", starterId);

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
            if (sortBy == SortBy_BarPost.DateCreated_Desc)
                sql.OrderBy("PostId desc");
            return sql;

        }
        #endregion

        /// <summary>
        /// 计算回帖在帖子的哪一页
        /// </summary>
        /// <param name="threadId"></param>
        /// <param name="postId"></param>
        /// <returns></returns>
        public int GetPageIndexForPostInThread(long threadId, long postId)
        {
            int pageIndex = 1;
            string cacheKey = GetCacheKey_PostIdsOfThread(threadId, false, SortBy_BarPost.DateCreated);
            PagingEntityIdCollection peic = cacheService.Get<PagingEntityIdCollection>(cacheKey);
            if (peic == null)
            {
                peic = CreateDAO().FetchPagingPrimaryKeys<BarPost>(PrimaryMaxRecords, pageSize * CacheablePageCount, 1, GetSql_Posts(threadId, false, 0, SortBy_BarPost.DateCreated));
                peic.IsContainsMultiplePages = true;
                cacheService.Add(cacheKey, peic, CachingExpirationType.ObjectCollection);
            }

            if (peic != null)
            {
                IList<long> postIds = peic.GetTopEntityIds(peic.Count).Cast<long>().ToList();
                int postIndex = postIds.IndexOf(postId);
                if (postIndex > 0)
                {
                    pageIndex = postIndex / pageSize + 1;
                }
                else
                {
                    Sql sql = Sql.Builder
                        .Select("count(PostId)")
                        .From("spb_BarPosts")
                        .Where("ParentId=0")
                        .Where("ThreadId=@0", threadId)
                        .Where("PostId<@0", postId);
                    postIndex = CreateDAO().FirstOrDefault<int>(sql);

                    if (postIndex > 0)
                        pageIndex = postIndex / pageSize + 1;
                }
            }
            return pageIndex;
        }

        /// <summary>
        /// 计算子级回帖在子回帖的第几页
        /// </summary>
        /// <param name="parentId"></param>
        /// <param name="postId"></param>
        /// <returns></returns>
        public int GetPageIndexForChildrenPost(long parentId, long postId)
        {
            int pageIndex = 1;

            //和实际获取的时候使用一个缓存
            string cacheKey = GetCacheKey_GetChildren(SortBy_BarPost.DateCreated_Desc, parentId);

            PagingEntityIdCollection peic = cacheService.Get<PagingEntityIdCollection>(cacheKey);

            if (peic == null)
            {
                var sql = Sql.Builder;
                sql.Select("PostId")
                .From("spb_BarPosts")
                .Where("ParentId = @0", parentId)
                .OrderBy("PostId desc");
                peic = CreateDAO().FetchPagingPrimaryKeys<BarPost>(PrimaryMaxRecords, childrenPageSize * CacheablePageCount, 1, sql);
                peic.IsContainsMultiplePages = true;
                cacheService.Set(cacheKey, peic, CachingExpirationType.ObjectCollection);
            }

            if (peic != null)
            {
                IList<long> postIds = peic.GetTopEntityIds(peic.Count).Cast<long>().ToList();
                int postIndex = postIds.IndexOf(postId);
                if (postIndex > 0)
                {
                    pageIndex = postIndex / childrenPageSize + 1;
                }
                else
                {
                    Sql sql = Sql.Builder;
                    sql.Select("count(PostId)")
                        .From("spb_BarPosts")
                        .Where("ParentId = @0", parentId)
                        .Where("PostId>@0", postId);
                    postIndex = CreateDAO().FirstOrDefault<int>(sql);
                    if (postIndex > 0)
                        pageIndex = postIndex / childrenPageSize + 1;
                }
            }
            return pageIndex;
        }

        /// <summary>
        /// 获取统计数据
        /// </summary>
        /// <param name="tenantTypeId">租户类型id</param>
        /// <returns></returns>
        public Dictionary<string, long> GetManageableDatas(string tenantTypeId)
        {
            var dao = CreateDAO();
            Dictionary<string, long> manageableDatas = new Dictionary<string, long>();
            dao.OpenSharedConnection();
            Sql sql = Sql.Builder;
            sql.Select("count(*)")
                .From("spb_BarPosts")
                .Where("AuditStatus=@0", AuditStatus.Pending);
            if (!string.IsNullOrEmpty(tenantTypeId))
                sql.Where("TenantTypeId=@0", tenantTypeId);
            manageableDatas.Add(ApplicationStatisticDataKeys.Instance().PostPendingCount(), dao.FirstOrDefault<long>(sql));
            sql = Sql.Builder;
            sql.Select("count(*)")
                .From("spb_BarPosts")
                .Where("AuditStatus=@0", AuditStatus.Again);
            if (!string.IsNullOrEmpty(tenantTypeId))
                sql.Where("TenantTypeId=@0", tenantTypeId);

            manageableDatas.Add(ApplicationStatisticDataKeys.Instance().PostAgainCount(), dao.FirstOrDefault<long>(sql));
            dao.CloseSharedConnection();
            return manageableDatas;
        }
    }
}
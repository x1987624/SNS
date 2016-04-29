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
    /// 帖子仓储
    /// </summary>
    public class BarThreadRepository : Repository<BarThread>, IBarThreadRepository
    {
        IBodyProcessor barBodyProcessor = DIContainer.ResolveNamed<IBodyProcessor>(TenantTypeIds.Instance().Bar());
        private int pageSize = 20;

        /// <summary>
        /// 构造器
        /// </summary>
        public BarThreadRepository()
        {

        }
        
        /// <summary>
        /// 构造器
        /// </summary>
        /// <param name="publiclyAuditStatus"></param>
        public BarThreadRepository(PubliclyAuditStatus publiclyAuditStatus)
        {
            this.publiclyAuditStatus = publiclyAuditStatus;
        }

        /// <summary>
        /// 更新帖子
        /// </summary>
        /// <param name="entity"></param>
        public override void Update(BarThread entity)
        {
            base.Update(entity);
            //更新解析正文缓存
            string cacheKey = GetCacheKeyOfResolvedBody(entity.ThreadId);
            string resolveBody = cacheService.Get<string>(cacheKey);
            if (resolveBody != null)
            {
                resolveBody = entity.GetBody();

                AtUserService atUserService = new AtUserService(TenantTypeIds.Instance().BarThread());
                atUserService.ResolveBodyForEdit(resolveBody, entity.UserId, entity.ThreadId);

                resolveBody = barBodyProcessor.Process(resolveBody, TenantTypeIds.Instance().BarThread(), entity.ThreadId, entity.UserId);
                cacheService.Set(cacheKey, resolveBody, CachingExpirationType.SingleObject);
            }
        }

        /// <summary>
        /// 移动帖子
        /// </summary>
        /// <param name="threadId"></param>
        /// <param name="moveToSectionId"></param>
        public void MoveThread(long threadId, long moveToSectionId)
        {
            BarThread thread = Get(threadId);
            if (thread == null)
                return;
            if (thread.SectionId == moveToSectionId)
                return;
            long oldSectionId = thread.SectionId;
            thread.SectionId = moveToSectionId;

            var sql = Sql.Builder.Append("update spb_BarThreads set SectionId = @0 where ThreadId = @1", moveToSectionId, threadId);
            CreateDAO().Execute(sql);

            RealTimeCacheHelper.IncreaseAreaVersion("SectionId", oldSectionId);
            RealTimeCacheHelper.IncreaseAreaVersion("SectionId", moveToSectionId);
        }

        /// <summary>
        /// 更新置顶到期的帖子
        /// </summary>
        public void ExpireStickyThreads()
        {
            var sql = Sql.Builder.Append("update spb_BarThreads set IsSticky = 0 where StickyDate < @0", DateTime.UtcNow);
            CreateDAO().Execute(sql);
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
            if (takeOver != null)
            {
                sqls.Add(Sql.Builder.Append("update spb_BarSections set UserId = @0 where UserId = @1", takeOver.UserId, userId));
                if (takeOverAll)
                {
                    sqls.Add(Sql.Builder.Append("update spb_BarThreads set UserId = @0,Author=@1 where UserId = @2", takeOver.UserId, takeOver.DisplayName, userId));
                    sqls.Add(Sql.Builder.Append("update spb_BarPosts set UserId = @0, where UserId = @1", takeOver.UserId, userId));
                }
            }
            sqls.Add(Sql.Builder.Append("delete from spb_BarRatings where UserId = @0", userId));
            sqls.Add(Sql.Builder.Append("delete from spb_BarSectionManagers where UserId = @0", userId));
            CreateDAO().Execute(sqls);
        }

        /// <summary>
        /// 删除帖子
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public override int Delete(BarThread entity)
        {
            //同时删除帖子、回帖、评分记录
            List<Sql> sqls = new List<Sql>();



            sqls.Add(Sql.Builder.Append("delete from spb_BarRatings where ThreadId = @0", entity.ThreadId));

            CreateDAO().Execute(sqls);
            int affectCount = base.Delete(entity);

            return affectCount;
        }

        /// <summary>
        /// 获取上一篇
        /// </summary>
        /// <param name="barThread"></param>
        /// <returns></returns>
        public long GetPrevThreadId(BarThread barThread)
        {
            Database dao = CreateDAO();
            string cacheKey = string.Format("BarPrevThreadId-{0}", barThread.ThreadId);
            long? prevThreadId = cacheService.Get(cacheKey) as long?;
            if (prevThreadId == null)
            {
                prevThreadId = 0;
                var sql = Sql.Builder;
                sql.Select("ThreadId")
                .From("spb_BarThreads")
                .Where("LastModified > @0", barThread.LastModified)
                .Where("TenantTypeId = @0", barThread.TenantTypeId)
                .Where("SectionId = @0", barThread.SectionId);

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

                sql.OrderBy("LastModified");
                var ids_object = dao.FetchTopPrimaryKeys<BarThread>(1, sql);
                if (ids_object.Count() > 0)
                    prevThreadId = ids_object.Cast<long>().First();
                cacheService.Add(cacheKey, prevThreadId, CachingExpirationType.SingleObject);
            }
            return prevThreadId ?? 0;
        }


        /// <summary>
        /// 获取下一篇
        /// </summary>
        /// <param name="barThread"></param>
        /// <returns></returns>
        public long GetNextThreadId(BarThread barThread)
        {
            Database dao = CreateDAO();
            string cacheKey = string.Format("BarNextThreadId-{0}", barThread.ThreadId);
            long? nextThreadId = cacheService.Get(cacheKey) as long?;
            if (nextThreadId == null)
            {
                nextThreadId = 0;
                var sql = Sql.Builder;
                sql.Select("ThreadId")
                .From("spb_BarThreads")
                .Where("LastModified < @0", barThread.LastModified)
                .Where("TenantTypeId = @0", barThread.TenantTypeId)
                .Where("SectionId = @0", barThread.SectionId);
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
                sql.OrderBy("LastModified desc");
                var ids_object = dao.FetchTopPrimaryKeys<BarThread>(1, sql);
                if (ids_object.Count() > 0)
                    nextThreadId = ids_object.Cast<long>().First();
                cacheService.Add(cacheKey, nextThreadId, CachingExpirationType.SingleObject);
            }
            return nextThreadId ?? 0;
        }

        /// <summary>
        /// 获取某个帖吧下的所有帖子（用于删除帖子）
        /// </summary>
        /// <param name="sectionId"></param>
        /// <returns></returns>
        public IEnumerable<BarThread> GetAllThreadsOfSection(long sectionId)
        {
            var sql = Sql.Builder;
            sql.Select("*")
            .From("spb_BarThreads")
            .Where("SectionId=@0", sectionId);
            IEnumerable<BarThread> threads = CreateDAO().Fetch<BarThread>(sql);
            return threads;
        }

        /// <summary>
        /// 获取某个用户的所有帖子（用于删除用户）
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public IEnumerable<BarThread> GetAllThreadsOfUser(long userId)
        {
            var sql = Sql.Builder;
            sql.Select("*")
            .From("spb_BarThreads")
            .Where("UserId=@0", userId);
            IEnumerable<BarThread> threads = CreateDAO().Fetch<BarThread>(sql);
            return threads;
        }

        /// <summary>
        /// 获取解析的正文
        /// </summary>
        /// <param name="threadId"></param>
        /// <returns></returns>
        public string GetResolvedBody(long threadId)
        {
            BarThread barThread = Get(threadId);
            if (barThread == null)
                return string.Empty;

            string cacheKey = GetCacheKeyOfResolvedBody(threadId);
            string resolveBody = cacheService.Get<string>(cacheKey);
            if (resolveBody == null)
            {
                resolveBody = barThread.GetBody();
                resolveBody = barBodyProcessor.Process(resolveBody, TenantTypeIds.Instance().BarThread(), barThread.ThreadId, barThread.UserId);
                cacheService.Set(cacheKey, resolveBody, CachingExpirationType.SingleObject);
            }
            return resolveBody;
        }

        /// <summary>
        /// 获取BarThread内容
        /// </summary>
        /// <param name="threadId"></param>
        /// <returns></returns>
        public string GetBody(long threadId)
        {
            string cacheKey = RealTimeCacheHelper.GetCacheKeyOfEntityBody(threadId);
            string body = cacheService.Get<string>(cacheKey);
            if (body == null)
            {
                BarThread barThread = CreateDAO().SingleOrDefault<BarThread>(threadId);
                body = barThread != null ? barThread.Body : string.Empty;
                cacheService.Add(cacheKey, body, CachingExpirationType.SingleObject);
            }
            return body;
        }

        /// <summary>
        /// 获取用户的主题帖分页集合
        /// </summary>
        /// <param name="userId">用户Id</param>
        /// <param name="ignoreAudit">是否忽略审核状态（作者或管理员查看时忽略审核状态）</param>
        /// <param name="isPosted">是否是取我回复过的</param>
        /// <param name="pageIndex">页码</param>
        /// <param name="tenantTypeId">租户类型Id</param>
        /// <returns>主题帖列表</returns>
        public PagingDataSet<BarThread> GetUserThreads(string tenantTypeId, long userId, bool ignoreAudit, bool isPosted, int pageIndex, long? sectionId)
        {
            //不必筛选审核状态
            //缓存周期：对象集合，需要维护即时性
            //排序：发布时间（倒序）
            return GetPagingEntities(pageSize, pageIndex, CachingExpirationType.ObjectCollection,
                              () =>
                              {
                                  StringBuilder cacheKey = new StringBuilder();
                                  cacheKey.Append(RealTimeCacheHelper.GetListCacheKeyPrefix(CacheVersionType.AreaVersion, "UserId", userId));
                                  cacheKey.AppendFormat("MyBarThreads::TenantTypeId-{0}:IsPosted-{1}:IgnoreAudit-{2}:SectionId-{3}", tenantTypeId, isPosted, ignoreAudit, sectionId ?? null);
                                  return cacheKey.ToString();
                              },
                              () =>
                              {
                                  var sql = Sql.Builder;
                                  if (isPosted)
                                  {
                                      sql.Select("distinct ThreadId")
                                          .From("spb_BarPosts")
                                          .Where("UserId = @0", userId)
                                          .Where("TenantTypeId = @0", tenantTypeId);
                                      if (sectionId.HasValue)
                                          sql.Where("SectionId=@0", sectionId.Value);
                                      sql.OrderBy("ThreadId desc");
                                  }
                                  else
                                  {
                                      sql.Select("ThreadId")
                                      .From("spb_BarThreads")
                                      .Where("UserId = @0", userId)
                                      .Where("TenantTypeId = @0", tenantTypeId);
                                      if (sectionId.HasValue)
                                          sql.Where("SectionId=@0", sectionId.Value);
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
                                      sql.OrderBy("ThreadId desc");
                                  }
                                  return sql;
                              });
        }

        /// <summary>
        /// 获取主题帖的排行数据
        /// </summary>
        /// <param name="tenantTypeId">租户类型Id</param>
        /// <param name="topNumber">前多少条</param>
        /// <param name="sortBy">主题帖排序依据</param>
        /// <returns></returns>
        public IEnumerable<BarThread> GetTops(string tenantTypeId, int topNumber, bool? isEssential, SortBy_BarThread sortBy)
        {
            //只获取可对外显示审核状态的主题帖
            //缓存周期：常用对象集合，不用维护即时性
            return GetTopEntities(topNumber, CachingExpirationType.UsualObjectCollection, () =>
            {
                StringBuilder cacheKey = new StringBuilder();
                cacheKey.AppendFormat("BarThreadRanks::TenantTypeId-{0}:SortBy-{1}:isEssential-{2}", tenantTypeId, sortBy, isEssential);
                return cacheKey.ToString();
            }
            , () =>
            {
                var sql = Sql.Builder;
                var whereSql = Sql.Builder;
                var orderSql = Sql.Builder;
                sql.Select("spb_BarThreads.*")
                .From("spb_BarThreads");
                whereSql.Where("TenantTypeId = @0", tenantTypeId);

                if (isEssential.HasValue)
                    whereSql.Where("IsEssential = @0", isEssential.Value);

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
                CountService countService = new CountService(TenantTypeIds.Instance().BarThread());
                string countTableName = countService.GetTableName_Counts();
                switch (sortBy)
                {
                    case SortBy_BarThread.DateCreated_Desc:
                        orderSql.OrderBy("ThreadId desc");
                        break;
                    case SortBy_BarThread.LastModified_Desc:
                        orderSql.OrderBy("LastModified desc");
                        break;
                    case SortBy_BarThread.HitTimes:
                        sql.LeftJoin(string.Format("(select * from {0} WHERE ({0}.CountType = '{1}')) c", countTableName, CountTypes.Instance().HitTimes()))
                        .On("ThreadId = c.ObjectId");
                        orderSql.OrderBy("c.StatisticsCount desc");
                        break;
                    case SortBy_BarThread.StageHitTimes:
                        StageCountTypeManager stageCountTypeManager = StageCountTypeManager.Instance(TenantTypeIds.Instance().BarThread());
                        int stageCountDays = stageCountTypeManager.GetMaxDayCount(CountTypes.Instance().HitTimes());
                        string stageCountType = stageCountTypeManager.GetStageCountType(CountTypes.Instance().HitTimes(), stageCountDays);
                        sql.LeftJoin(string.Format("(select * from {0} WHERE ({0}.CountType = '{1}')) c", countTableName, stageCountType))
                        .On("ThreadId = c.ObjectId");
                        orderSql.OrderBy("c.StatisticsCount desc");
                        break;
                    case SortBy_BarThread.PostCount:
                        orderSql.OrderBy("PostCount desc");
                        break;
                    default:
                        orderSql.OrderBy("ThreadId desc");
                        break;
                }
                sql.Append(whereSql).Append(orderSql);
                return sql;
            });
        }

        /// <summary>
        /// 获取群组贴吧的排行数据
        /// </summary>
        /// <param name="tenantTypeId">租户类型Id</param>
        /// <param name="topNumber">前多少条</param>
        /// <param name="sortBy">主题帖排序依据</param>
        /// <returns></returns>
        public IEnumerable<BarThread> GetTopsThreadOfGroup(string tenantTypeId, int topNumber, bool? isEssential, SortBy_BarThread sortBy)
        {

            //只获取可对外显示审核状态的主题帖
            //缓存周期：常用对象集合，不用维护即时性
            return GetTopEntities(topNumber, CachingExpirationType.UsualObjectCollection, () =>
            {
                StringBuilder cacheKey = new StringBuilder();
                cacheKey.AppendFormat("BarThreadRanks::TenantTypeId-{0}:SortBy-{1}:isEssential-{2}:isPublic-{3}", tenantTypeId, sortBy, isEssential, 1);
                return cacheKey.ToString();
            }
            , () =>
            {
                var sql = Sql.Builder;
                var whereSql = Sql.Builder;
                var orderSql = Sql.Builder;
                sql.Select("spb_BarThreads.*")
                .From("spb_BarThreads")
                .InnerJoin("spb_Groups")
                .On("spb_BarThreads.SectionId=spb_Groups.GroupId");
                whereSql.Where("spb_BarThreads.TenantTypeId = @0", tenantTypeId);
                whereSql.Where("spb_Groups.IsPublic=@0", 1);

                if (isEssential.HasValue)
                    whereSql.Where("spb_BarThreads.IsEssential = @0", isEssential.Value);

                switch (this.PubliclyAuditStatus)
                {
                    case PubliclyAuditStatus.Again:
                    case PubliclyAuditStatus.Fail:
                    case PubliclyAuditStatus.Pending:
                    case PubliclyAuditStatus.Success:
                        whereSql.Where("spb_BarThreads.AuditStatus=@0", this.PubliclyAuditStatus);
                        break;
                    case PubliclyAuditStatus.Again_GreaterThanOrEqual:
                    case PubliclyAuditStatus.Pending_GreaterThanOrEqual:
                        whereSql.Where("spb_BarThreads.AuditStatus>@0", this.PubliclyAuditStatus);
                        break;
                    default:
                        break;
                }
                CountService countService = new CountService(TenantTypeIds.Instance().BarThread());
                string countTableName = countService.GetTableName_Counts();
                switch (sortBy)
                {
                    case SortBy_BarThread.DateCreated_Desc:
                        orderSql.OrderBy("spb_BarThreads.ThreadId desc");
                        break;
                    case SortBy_BarThread.LastModified_Desc:
                        orderSql.OrderBy("spb_BarThreads.LastModified desc");
                        break;

                    case SortBy_BarThread.HitTimes:
                        sql.LeftJoin(string.Format("(select * from {0} WHERE ({0}.CountType = '{1}')) c", countTableName, CountTypes.Instance().HitTimes()))
                        .On("spb_BarThreads.ThreadId = c.ObjectId");
                        orderSql.OrderBy("c.StatisticsCount desc");
                        break;
                    case SortBy_BarThread.StageHitTimes:
                        StageCountTypeManager stageCountTypeManager = StageCountTypeManager.Instance(TenantTypeIds.Instance().BarThread());
                        int stageCountDays = stageCountTypeManager.GetMaxDayCount(CountTypes.Instance().HitTimes());
                        string stageCountType = stageCountTypeManager.GetStageCountType(CountTypes.Instance().HitTimes(), stageCountDays);
                        sql.LeftJoin(string.Format("(select * from {0} WHERE ({0}.CountType = '{1}')) c", countTableName, stageCountType))
                        .On("spb_BarThreads.ThreadId = c.ObjectId");
                        orderSql.OrderBy("c.StatisticsCount desc");
                        break;
                    case SortBy_BarThread.PostCount:
                        orderSql.OrderBy("spb_BarThreads.PostCount desc");
                        break;
                    default:
                        orderSql.OrderBy("spb_BarThreads.ThreadId desc");
                        break;
                }
                sql.Append(whereSql).Append(orderSql);
                return sql;
            });
        }
        /// <summary>
        /// 根据标签名获取主题帖排行分页集合
        /// </summary>
        /// <param name="tenantTypeId">租户类型Id</param>
        /// <param name="tageName">标签名</param>
        /// <param name="isEssential">是否为精华帖</param>
        /// <param name="sortBy">帖子排序依据</param>
        /// <param name="pageIndex">页码</param>
        /// <returns>主题帖列表</returns>
        public PagingDataSet<BarThread> Gets(string tenantTypeId, string tageName, bool? isEssential, SortBy_BarThread sortBy, int pageIndex)
        {
            //只获取可对外显示审核状态的主题帖
            //缓存周期：对象集合，不用维护即时性
            return GetPagingEntities(pageSize, pageIndex, CachingExpirationType.ObjectCollection, () =>
            {
                StringBuilder cacheKey = new StringBuilder();
                cacheKey.AppendFormat("BarThreadsOfTag::TenantTypeId-{0}:TagName-{1}:IsEssential-{2}:SortBy-{3}", tenantTypeId, tageName, isEssential, sortBy);
                return cacheKey.ToString();
            }
            , () =>
            {
                var sql = Sql.Builder;
                sql.Select("spb_BarThreads.*")
                .From("spb_BarThreads");
                var whereSql = Sql.Builder;
                var orderSql = Sql.Builder;

                whereSql.Where("spb_BarThreads.TenantTypeId = @0", tenantTypeId);
                if (!string.IsNullOrEmpty(tageName))
                {
                    sql.InnerJoin("tn_ItemsInTags")
                    .On("spb_BarThreads.ThreadId = tn_ItemsInTags.ItemId");

                    whereSql.Where("tn_ItemsInTags.TagName=@0", tageName)
                    .Where("tn_ItemsInTags.TenantTypeId = @0", TenantTypeIds.Instance().BarThread());
                }

                if (isEssential.HasValue)
                    whereSql.Where("IsEssential = @0", isEssential.Value);

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
                CountService countService = new CountService(TenantTypeIds.Instance().BarThread());
                string countTableName = countService.GetTableName_Counts();
                switch (sortBy)
                {
                    case SortBy_BarThread.DateCreated_Desc:
                        orderSql.OrderBy("ThreadId desc");
                        break;
                    case SortBy_BarThread.LastModified_Desc:
                        orderSql.OrderBy("LastModified desc");
                        break;


                    case SortBy_BarThread.HitTimes:
                        sql.LeftJoin(string.Format("(select * from {0} WHERE ({0}.CountType = '{1}')) c", countTableName, CountTypes.Instance().HitTimes()))
                                  .On("ThreadId = c.ObjectId");
                        orderSql.OrderBy("c.StatisticsCount desc");
                        break;
                    case SortBy_BarThread.StageHitTimes:
                        StageCountTypeManager stageCountTypeManager = StageCountTypeManager.Instance(TenantTypeIds.Instance().BarThread());
                        int stageCountDays = stageCountTypeManager.GetMaxDayCount(CountTypes.Instance().HitTimes());
                        string stageCountType = stageCountTypeManager.GetStageCountType(CountTypes.Instance().HitTimes(), stageCountDays);
                        sql.LeftJoin(string.Format("(select * from {0} WHERE ({0}.CountType = '{1}')) c", countTableName, stageCountType))
                        .On("ThreadId = c.ObjectId");
                        orderSql.OrderBy("c.StatisticsCount desc");
                        break;
                    case SortBy_BarThread.PostCount:
                        orderSql.OrderBy("PostCount desc");
                        break;
                    default:
                        orderSql.OrderBy("ThreadId desc");
                        break;
                }
                sql.Append(whereSql).Append(orderSql);
                return sql;
            });
        }


        /// <summary>
        /// 根据帖吧获取主题帖分页集合
        /// </summary>
        /// <param name="tenantTypeId">租户类型Id</param>
        /// <param name="sectionId">帖吧Id</param>
        /// <param name="categoryId">帖吧分类Id</param>
        /// <param name="isEssential">是否为精华帖</param>
        /// <param name="sortBy">帖子排序依据</param>
        /// <param name="pageIndex">页码</param>
        /// <returns>主题帖列表</returns>
        public PagingDataSet<BarThread> Gets(long sectionId, long? categoryId, bool? isEssential, SortBy_BarThread sortBy, int pageIndex)
        {
            //只获取可对外显示审核状态的主题帖
            //缓存周期：对象集合，需要维护即时性
            return GetPagingEntities(pageSize, pageIndex, CachingExpirationType.ObjectCollection, () =>
            {
                StringBuilder cacheKey = new StringBuilder();
                cacheKey.Append(RealTimeCacheHelper.GetListCacheKeyPrefix(CacheVersionType.AreaVersion, "SectionId", sectionId));
                cacheKey.AppendFormat("BarThreadsOfSectionId::CategoryId-{0}:IsEssential-{1}:SortBy-{2}", categoryId, isEssential, sortBy);
                return cacheKey.ToString();
            }
            , () =>
            {
                var sql = Sql.Builder;
                var whereSql = Sql.Builder;
                var orderSql = Sql.Builder;
                sql.Select("spb_BarThreads.*")
                .From("spb_BarThreads");
                whereSql.Where("SectionId = @0", sectionId);

                if (categoryId.HasValue && categoryId.Value > 0)
                {
                    sql.InnerJoin("tn_ItemsInCategories")
                    .On("ThreadId = tn_ItemsInCategories.ItemId");
                    whereSql.Where("tn_ItemsInCategories.CategoryId=@0", categoryId.Value);
                }
                if (isEssential.HasValue)
                    whereSql.Where("IsEssential = @0", isEssential.Value);

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
                CountService countService = new CountService(TenantTypeIds.Instance().BarThread());
                string countTableName = countService.GetTableName_Counts();
                switch (sortBy)
                {
                    case SortBy_BarThread.DateCreated_Desc:
                        orderSql.OrderBy("ThreadId desc");
                        break;
                    case SortBy_BarThread.LastModified_Desc:
                        orderSql.OrderBy("IsSticky desc,LastModified desc");
                        break;
                    case SortBy_BarThread.HitTimes:
                        sql.LeftJoin(string.Format("(select * from {0} WHERE ({0}.CountType = '{1}')) c", countTableName, CountTypes.Instance().HitTimes()))
                                  .On("ThreadId = c.ObjectId");
                        orderSql.OrderBy("c.StatisticsCount desc");
                        break;
                    case SortBy_BarThread.StageHitTimes:
                        StageCountTypeManager stageCountTypeManager = StageCountTypeManager.Instance(TenantTypeIds.Instance().BarThread());
                        int stageCountDays = stageCountTypeManager.GetMaxDayCount(CountTypes.Instance().HitTimes());
                        string stageCountType = stageCountTypeManager.GetStageCountType(CountTypes.Instance().HitTimes(), stageCountDays);
                        sql.LeftJoin(string.Format("(select * from {0} WHERE ({0}.CountType = '{1}')) c", countTableName, stageCountType))
                        .On("ThreadId = c.ObjectId");
                        orderSql.OrderBy("c.StatisticsCount desc");
                        break;
                    case SortBy_BarThread.PostCount:
                        orderSql.OrderBy("PostCount desc");
                        break;
                    default:
                        orderSql.OrderBy("ThreadId desc");
                        break;
                }
                sql.Append(whereSql).Append(orderSql);
                return sql;
            });
        }

        /// <summary>
        /// 帖子管理时查询帖子分页集合
        /// </summary>
        /// <param name="tenantTypeId">租户类型Id</param>
        /// <param name="query">帖子查询条件</param>
        /// <param name="pageSize">每页多少条数据</param>
        /// <param name="pageIndex">页码</param>
        /// <returns>帖子分页集合</returns>
        public PagingDataSet<BarThread> Gets(string tenantTypeId, BarThreadQuery query, int pageSize, int pageIndex)
        {
            var sql = Sql.Builder;
            sql.Select("*")
            .From("spb_BarThreads");

            if (query.CategoryId.HasValue && query.CategoryId.Value > 0)
            {
                sql.InnerJoin("tn_ItemsInCategories")
                .On("ThreadId = tn_ItemsInCategories.ItemId");
                sql.Where("tn_ItemsInCategories.CategoryId=@0", query.CategoryId.Value);
            }

            if (!string.IsNullOrEmpty(tenantTypeId))
                sql.Where("TenantTypeId = @0", tenantTypeId);

            if (query.UserId != null && query.UserId > 0)
                sql.Where("UserId = @0", query.UserId);

            if (query.SectionId != null && query.SectionId > 0)
                sql.Where("SectionId = @0", query.SectionId);

            if (query.AuditStatus != null)
                sql.Where("AuditStatus = @0", query.AuditStatus);

            if (!string.IsNullOrEmpty(query.SubjectKeyword))
                sql.Where("Subject like @0", StringUtility.StripSQLInjection(query.SubjectKeyword) + "%");

            if (query.IsEssential.HasValue)
                sql.Where("IsEssential = @0", query.IsEssential.Value);

            if (query.IsSticky.HasValue)
                sql.Where("IsSticky = @0", query.IsSticky.Value);

            if (query.StartDate != null)
                sql.Where("DateCreated >= @0", query.StartDate);
            if (query.EndDate != null)
                sql.Where("DateCreated < @0", query.EndDate.Value.AddDays(1));
            sql.OrderBy("ThreadId desc");

            return GetPagingEntities(pageSize, pageIndex, sql);
        }


        /// <summary>
        /// 获取帖子管理数据
        /// </summary>
        /// <param name="tenantTypeId">租户类型Id（可以获取该应用下针对某种租户类型的统计计数，默认不进行筛选）</param>
        /// <returns></returns>
        public Dictionary<string, long> GetManageableDatas(string tenantTypeId = null)
        {
            var dao = CreateDAO();
            Dictionary<string, long> manageableDatas = new Dictionary<string, long>();
            dao.OpenSharedConnection();
            Sql sql = Sql.Builder;
            sql.Select("count(*)")
                .From("spb_BarThreads")
                .Where("AuditStatus=@0", AuditStatus.Pending);
            if (!string.IsNullOrEmpty(tenantTypeId))
                sql.Where("TenantTypeId=@0", tenantTypeId);
            manageableDatas.Add(ApplicationStatisticDataKeys.Instance().PendingCount(), dao.FirstOrDefault<long>(sql));
            sql = Sql.Builder;
            sql.Select("count(*)")
                .From("spb_BarThreads")
                .Where("AuditStatus=@0", AuditStatus.Again);
            if (!string.IsNullOrEmpty(tenantTypeId))
                sql.Where("TenantTypeId=@0", tenantTypeId);

            manageableDatas.Add(ApplicationStatisticDataKeys.Instance().AgainCount(), dao.FirstOrDefault<long>(sql));
            dao.CloseSharedConnection();
            return manageableDatas;
        }

        /// <summary>
        /// 获取帖子统计数据
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, long> GetStatisticDatas(string tenantTypeId = null)
        {
            var dao = CreateDAO();
            string cacheKey = "BarThreadStatisticData";
            Dictionary<string, long> statisticDatas = cacheService.Get<Dictionary<string, long>>(cacheKey);
            if (statisticDatas == null)
            {
                statisticDatas = new Dictionary<string, long>();
                Sql sql = Sql.Builder;
                sql.Select("count(*)")
                    .From("spb_BarThreads");
                if (!string.IsNullOrEmpty(tenantTypeId))
                    sql.Where("TenantTypeId=@0", tenantTypeId);
                statisticDatas.Add(ApplicationStatisticDataKeys.Instance().TotalCount(), dao.FirstOrDefault<long>(sql));

                sql = Sql.Builder;
                sql.Select("count(*)")
                    .From("spb_BarThreads")
                    .Where("DateCreated > @0", DateTime.UtcNow.AddDays(-1));
                if (!string.IsNullOrEmpty(tenantTypeId))
                    sql.Where("TenantTypeId=@0", tenantTypeId);
                statisticDatas.Add(ApplicationStatisticDataKeys.Instance().Last24HCount(), dao.FirstOrDefault<long>(sql));
                cacheService.Add(cacheKey, statisticDatas, CachingExpirationType.UsualSingleObject);
            }

            return statisticDatas;
        }

        /// <summary>
        /// 帖吧应用可对外显示的审核状态
        /// </summary>
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

        /// <summary>
        /// 获取解析正文缓存Key
        /// </summary>
        /// <param name="threadId"></param>
        /// <returns></returns>
        private string GetCacheKeyOfResolvedBody(long threadId)
        {
            return "BarThreadResolvedBody" + threadId;
        }

    }
}
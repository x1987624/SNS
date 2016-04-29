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

namespace Spacebuilder.Ask
{
    /// <summary>
    /// 问答应用的问题仓储实现
    /// </summary>
    public class AskQuestionRepository : Repository<AskQuestion>, IAskQuestionRepository
    {
        IBodyProcessor bodyProcessor = DIContainer.ResolveNamed<IBodyProcessor>(TenantTypeIds.Instance().AskQuestion());
        /// <summary>
        /// 构造器
        /// </summary>
        public AskQuestionRepository()
        {

        }

        /// <summary>
        /// 构造器
        /// </summary>
        /// <param name="publiclyAuditStatus"></param>
        public AskQuestionRepository(PubliclyAuditStatus publiclyAuditStatus)
        {
            this.publiclyAuditStatus = publiclyAuditStatus;
        }


        /// <summary>
        /// 更新问题
        /// </summary>
        /// <param name="entity"></param>
        public override void Update(AskQuestion entity)
        {
            base.Update(entity);

            //更新解析正文缓存
            string cacheKey = GetCacheKeyOfResolvedBody(entity.QuestionId);
            string resolveBody = bodyProcessor.Process(entity.Body, TenantTypeIds.Instance().AskQuestion(), entity.QuestionId, entity.UserId);
            if (!string.IsNullOrEmpty(resolveBody))
            {
                cacheService.Set(cacheKey, resolveBody, CachingExpirationType.SingleObject);
            }
        }

        /// <summary>
        /// 设置/取消精华问题（管理员操作）
        /// </summary>
        /// <param name="question">问题实体</param>
        /// <param name="isEssential">设置/取消精华</param>
        public void SetEssential(AskQuestion question, bool isEssential)
        {
            //更新数据库
            var sql = Sql.Builder.Append("update spb_AskQuestions set IsEssential=@0 where QuestionId=@1", isEssential, question.QuestionId);
            CreateDAO().Execute(sql);

            //更新实体缓存
            question.IsEssential = isEssential;
            base.OnUpdated(question);
        }

        /// <summary>
        /// 指定用户接管当前用户的问题
        /// </summary>
        /// <param name="userId">当前用户id</param>
        /// <param name="takeOverUser">指定用户</param>
        public void TakeOver(long userId, Common.User takeOverUser)
        {
            List<Sql> sqls = new List<Sql>();

            //更新问题的作者
            sqls.Add(Sql.Builder.Append("update spb_AskQuestions set UserId=@0,Author=@1,LastModifier=@2 where UserId=@3", takeOverUser.UserId, takeOverUser.DisplayName, takeOverUser.DisplayName, userId));

            //更新最后回答用户
            sqls.Add(Sql.Builder.Append("update spb_AskQuestions set LastAnswerUserId=@0,LastAnswerAuthor=@1 where LastAnswerUserId=@2", takeOverUser.UserId, takeOverUser.DisplayName, userId));

            CreateDAO().Execute(sqls);
        }

        /// <summary>
        /// 获取感兴趣的问题
        /// </summary>
        /// <param name="tenantTypeId">租户类型id</param>
        /// <param name="userId">用户id</param>
        /// <param name="topNumber">查询条数</param>
        /// <returns>问题列表</returns>
        public IEnumerable<AskQuestion> GetTopInterestedQuestions(string tenantTypeId, long userId, int topNumber)
        {
            StringBuilder cacheKey = new StringBuilder();
            cacheKey.Append(RealTimeCacheHelper.GetListCacheKeyPrefix(CacheVersionType.AreaVersion, "UserId", userId));
            cacheKey.AppendFormat("GetTopInterestedQuestions::tenantTypeId-{0}", tenantTypeId);

            //从缓存里取列表，如果缓存里没有就去数据库取
            List<AskQuestion> questions = cacheService.Get<List<AskQuestion>>(cacheKey.ToString());
            if (questions != null && questions.Count() > 0)
            {
                return questions.Take(topNumber);
            }

            IEnumerable<object> questionIds = null;

            //先查询关注标签下的问题
            //查询用户关注的标签
            SubscribeService subscribeService = new SubscribeService(TenantTypeIds.Instance().AskTag());
            IEnumerable<long> tagIds = subscribeService.GetAllObjectIds(userId);
            if (tagIds != null && tagIds.Count() > 0)
            {
                Sql sql;
                Sql whereSql;
                Sql orderSql;
                BuildSqlForGetTopInterestedQuestions(tenantTypeId, out sql, out whereSql, out orderSql);
                sql.InnerJoin("tn_ItemsInTags").On("spb_AskQuestions.QuestionId = tn_ItemsInTags.ItemId")
                   .Append(whereSql)
                   .Where("tn_ItemsInTags.TenantTypeId = @0", TenantTypeIds.Instance().AskQuestion())
                   .Where("tn_ItemsInTags.Id in (@tagIds)", new { tagIds = tagIds })
                   .Append(orderSql);
                questionIds = CreateDAO().FetchTopPrimaryKeys<AskQuestion>(100, sql);
                questions = this.PopulateEntitiesByEntityIds(questionIds).ToList();
                if (questions != null && questions.Count() >= topNumber)
                {
                    //加入缓存
                    cacheService.Add(cacheKey.ToString(), questions, CachingExpirationType.UsualObjectCollection);
                    return questions.Take(topNumber);
                }
            }





            //如果查询结果不够topNumber，从关注用户的问题中查找
            //查询用户关注的用户
            FollowService followService = new FollowService();
            IEnumerable<long> followedUserIds = followService.GetTopFollowedUserIds(userId, 100, null, Follow_SortBy.FollowerCount_Desc);
            if (followedUserIds != null && followedUserIds.Count() > 0)
            {
                Sql sql;
                Sql whereSql;
                Sql orderSql;
                BuildSqlForGetTopInterestedQuestions(tenantTypeId, out sql, out whereSql, out orderSql);
                sql.Append(whereSql)
                   .Where("spb_AskQuestions.UserId in (@followedUserIds)", new { followedUserIds = followedUserIds })
                   .Append(orderSql);
                questionIds = CreateDAO().FetchTopPrimaryKeys<AskQuestion>(100, sql);
                IEnumerable<AskQuestion> questionsFollow = this.PopulateEntitiesByEntityIds(questionIds);
                if (questionsFollow != null && questionsFollow.Count() > 0)
                {
                    if (questions == null)
                    {
                        questions = new List<AskQuestion>();
                    }
                    questions.AddRange(questionsFollow);
                    questions = questions.Distinct<AskQuestion>().ToList();
                }
                if (questions != null && questions.Count() >= topNumber)
                {
                    //加入缓存
                    cacheService.Add(cacheKey.ToString(), questions, CachingExpirationType.UsualObjectCollection);
                    return questions.Take(topNumber);
                }
            }

            //如果查询结果还不够topNumber，从最新问题中查找
            Sql sqlNew;
            Sql whereSqlNew;
            Sql orderSqlNew;
            BuildSqlForGetTopInterestedQuestions(tenantTypeId, out sqlNew, out whereSqlNew, out orderSqlNew);
            sqlNew.Append(whereSqlNew).Append(orderSqlNew);
            questionIds = CreateDAO().FetchTopPrimaryKeys<AskQuestion>(100, sqlNew);
            IEnumerable<AskQuestion> questionsNew = this.PopulateEntitiesByEntityIds(questionIds);
            if (questionsNew != null && questionsNew.Count() > 0)
            {
                if (questions == null)
                {
                    questions = new List<AskQuestion>();
                }
                questions.AddRange(questionsNew);
                questions = questions.Distinct().ToList();
            }
            if (questions != null)
            {
                //加入缓存
                cacheService.Add(cacheKey.ToString(), questions, CachingExpirationType.UsualObjectCollection);
                return questions.Take(topNumber);
            }

            return questions;
        }

        /// <summary>
        /// 构建用于GetTopInterestedQuestions方法的Sql
        /// </summary>
        /// <param name="tenantTypeId">租户类型id</param>
        /// <param name="sql"></param>
        /// <param name="whereSql"></param>
        /// <param name="orderSql"></param>
        private void BuildSqlForGetTopInterestedQuestions(string tenantTypeId, out Sql sql, out Sql whereSql, out Sql orderSql)
        {
            sql = Sql.Builder.Select("spb_AskQuestions.*")
                             .From("spb_AskQuestions");

            whereSql = Sql.Builder.Where("spb_AskQuestions.Status<>@0", QuestionStatus.Canceled);

            if (!string.IsNullOrEmpty(tenantTypeId))
            {
                whereSql.Where("spb_AskQuestions.TenantTypeId=@0", tenantTypeId);
            }

            //过滤审核状态
            switch (this.PubliclyAuditStatus)
            {
                case PubliclyAuditStatus.Again:
                case PubliclyAuditStatus.Fail:
                case PubliclyAuditStatus.Pending:
                case PubliclyAuditStatus.Success:
                    whereSql.Where("spb_AskQuestions.AuditStatus=@0", this.PubliclyAuditStatus);
                    break;
                case PubliclyAuditStatus.Again_GreaterThanOrEqual:
                case PubliclyAuditStatus.Pending_GreaterThanOrEqual:
                    whereSql.Where("spb_AskQuestions.AuditStatus>@0", this.PubliclyAuditStatus);
                    break;
                default:
                    break;
            }

            //优先获取待解决问题（时间倒序）
            orderSql = Sql.Builder.OrderBy("spb_AskQuestions.QuestionId desc");

            //其次是热门问题（关注数倒序）
            CountService countService = new CountService(TenantTypeIds.Instance().AskQuestion());
            string countTableName = countService.GetTableName_Counts();
            sql.LeftJoin(countTableName).On("spb_AskQuestions.QuestionId = " + countTableName + ".ObjectId");
            whereSql.Where(countTableName + ".CountType = @0 or " + countTableName + ".CountType is null", CountTypes.Instance().QuestionFollowerCount());
            orderSql.OrderBy(countTableName + ".StatisticsCount desc");
        }

        /// <summary>
        /// 根据不同条件获取前n条的问题列表（用于前台）
        /// </summary>
        /// <param name="tenantTypeId">租户类型id</param>
        /// <param name="topNumber">前多少条</param>
        /// <param name="status">问题状态</param>
        /// <param name="isEssential">是否精华</param>
        /// <param name="sortBy">排序条件</param>
        /// <returns>问题列表</returns>
        public IEnumerable<AskQuestion> GetTopQuestions(string tenantTypeId, int topNumber, QuestionStatus? status, bool? isEssential, SortBy_AskQuestion sortBy)
        {
            return GetTopEntities(topNumber, CachingExpirationType.UsualObjectCollection,
                    () =>
                    {
                        StringBuilder cacheKey = new StringBuilder();


                        cacheKey.AppendFormat("GetTopQuestions::tenantTypeId-{0}:status-{1}:isEssential-{2}:sortBy-{3}", tenantTypeId, status, isEssential, sortBy);
                        return cacheKey.ToString();
                    },
                    () =>
                    {
                        var sql = Sql.Builder.Select("spb_AskQuestions.*")
                                             .From("spb_AskQuestions");

                        var whereSql = Sql.Builder.Where("spb_AskQuestions.Status<>@0", QuestionStatus.Canceled);

                        //过滤审核状态
                        switch (this.PubliclyAuditStatus)
                        {
                            case PubliclyAuditStatus.Again:
                            case PubliclyAuditStatus.Fail:
                            case PubliclyAuditStatus.Pending:
                            case PubliclyAuditStatus.Success:
                                whereSql.Where("spb_AskQuestions.AuditStatus=@0", this.PubliclyAuditStatus);
                                break;
                            case PubliclyAuditStatus.Again_GreaterThanOrEqual:
                            case PubliclyAuditStatus.Pending_GreaterThanOrEqual:
                                whereSql.Where("spb_AskQuestions.AuditStatus>@0", this.PubliclyAuditStatus);
                                break;
                            default:
                                break;
                        }

                        if (!string.IsNullOrEmpty(tenantTypeId))
                        {
                            whereSql.Where("spb_AskQuestions.TenantTypeId=@0", tenantTypeId);
                        }

                        if (status.HasValue)
                        {
                            whereSql.Where("spb_AskQuestions.Status=@0", status.Value);
                        }

                        if (isEssential.HasValue)
                        {
                            whereSql.Where("spb_AskQuestions.IsEssential=@0", isEssential.Value);
                        }

                        var orderSql = Sql.Builder;
                        switch (sortBy)
                        {
                            case SortBy_AskQuestion.AnswerCount:
                                orderSql.OrderBy("spb_AskQuestions.AnswerCount desc");
                                break;
                            case SortBy_AskQuestion.Reward:
                                orderSql.OrderBy("spb_AskQuestions.Reward desc");
                                break;
                            case SortBy_AskQuestion.StageHitTimes:
                                CountService countService = new CountService(TenantTypeIds.Instance().AskQuestion());
                                string countTableName = countService.GetTableName_Counts();
                                StageCountTypeManager stageCountTypeManager = StageCountTypeManager.Instance(TenantTypeIds.Instance().AskQuestion());
                                int stageCountDays = stageCountTypeManager.GetMaxDayCount(CountTypes.Instance().HitTimes());
                                string stageCountType = stageCountTypeManager.GetStageCountType(CountTypes.Instance().HitTimes(), stageCountDays);
                                sql.LeftJoin(string.Format("(select * from {0} WHERE ({0}.CountType = '{1}')) c", countTableName, stageCountType))
                                .On("QuestionId = c.ObjectId");
                                orderSql.OrderBy("c.StatisticsCount desc");
                                break;
                            default:
                                orderSql.OrderBy("spb_AskQuestions.QuestionId desc");
                                break;
                        }

                        return sql.Append(whereSql).Append(orderSql);
                    });
        }

        /// <summary>
        /// 根据不同条件分页获取问题列表（用于前台）
        /// </summary>
        /// <param name="tenantTypeId">租户类型id</param>
        /// <param name="tagName">标签</param>
        /// <param name="status">问题状态</param>
        /// <param name="isEssential">是否精华</param>
        /// <param name="sortBy">排序条件</param>
        /// <param name="pageSize">分页大小</param>
        /// <param name="pageIndex">页码</param>
        /// <returns>问题分页列表</returns>
        public Tunynet.PagingDataSet<AskQuestion> GetQuestions(string tenantTypeId, string tagName, QuestionStatus? status, bool? isEssential, SortBy_AskQuestion sortBy, int pageSize, int pageIndex)
        {
            return GetPagingEntities(pageSize, pageIndex, CachingExpirationType.UsualObjectCollection,
                      () =>
                      {
                          StringBuilder cacheKey = new StringBuilder();
                          cacheKey.AppendFormat("GetQuestions::tenantTypeId-{0}:tagName-{1}:status-{2}:isEssential-{3}:sortBy-{4}", tenantTypeId, tagName, status, isEssential, sortBy);
                          return cacheKey.ToString();
                      },
                      () =>
                      {
                          var sql = Sql.Builder.Select("spb_AskQuestions.*").From("spb_AskQuestions");
                          var whereSql = Sql.Builder.Where("spb_AskQuestions.Status<>@0", QuestionStatus.Canceled);
                          var orderSql = Sql.Builder;

                          //过滤审核条件
                          switch (this.PubliclyAuditStatus)
                          {
                              case PubliclyAuditStatus.Again:
                              case PubliclyAuditStatus.Fail:
                              case PubliclyAuditStatus.Pending:
                              case PubliclyAuditStatus.Success:
                                  whereSql.Where("spb_AskQuestions.AuditStatus=@0", this.PubliclyAuditStatus);
                                  break;
                              case PubliclyAuditStatus.Again_GreaterThanOrEqual:
                              case PubliclyAuditStatus.Pending_GreaterThanOrEqual:
                                  whereSql.Where("spb_AskQuestions.AuditStatus>@0", this.PubliclyAuditStatus);
                                  break;
                              default:
                                  break;
                          }

                          if (!string.IsNullOrEmpty(tenantTypeId))
                          {
                              whereSql.Where("spb_AskQuestions.TenantTypeId=@0", tenantTypeId);
                          }

                          if (status.HasValue)
                          {
                              whereSql.Where("spb_AskQuestions.Status=@0", status.Value);
                          }

                          if (isEssential.HasValue)
                          {
                              whereSql.Where("spb_AskQuestions.IsEssential=@0", isEssential.Value);
                          }

                          if (!string.IsNullOrEmpty(tagName))
                          {
                              sql.InnerJoin("tn_ItemsInTags").On("spb_AskQuestions.QuestionId = tn_ItemsInTags.ItemId");
                              whereSql.Where("tn_ItemsInTags.TenantTypeId = @0", TenantTypeIds.Instance().AskQuestion())
                                      .Where("tn_ItemsInTags.TagName=@0", tagName);
                          }

                          switch (sortBy)
                          {
                              case SortBy_AskQuestion.AnswerCount:
                                  orderSql.OrderBy("spb_AskQuestions.AnswerCount desc");
                                  break;
                              case SortBy_AskQuestion.StageHitTimes:
                                  CountService countService = new CountService(TenantTypeIds.Instance().AskQuestion());
                                  string countTableName = countService.GetTableName_Counts();
                                  StageCountTypeManager stageCountTypeManager = StageCountTypeManager.Instance(TenantTypeIds.Instance().AskQuestion());
                                  int stageCountDays = stageCountTypeManager.GetMaxDayCount(CountTypes.Instance().HitTimes());
                                  string stageCountType = stageCountTypeManager.GetStageCountType(CountTypes.Instance().HitTimes(), stageCountDays);
                                  sql.LeftJoin(string.Format("(select * from {0} WHERE ({0}.CountType = '{1}')) c", countTableName, stageCountType))
                                  .On("QuestionId = c.ObjectId");
                                  orderSql.OrderBy("c.StatisticsCount desc");
                                  break;
                              case SortBy_AskQuestion.Reward:
                                  whereSql.Where("spb_AskQuestions.Reward > 0");
                                  orderSql.OrderBy("spb_AskQuestions.Reward desc");
                                  break;
                              default:
                                  orderSql.OrderBy("spb_AskQuestions.QuestionId desc");
                                  break;
                          }

                          sql.Append(whereSql).Append(orderSql);
                          return sql;
                      });
        }

        /// <summary>
        /// 分页获取拥有者的问题列表
        /// </summary>
        /// <remarks>
        /// 缓存周期：对象集合
        /// </remarks>
        /// <param name="tenantTypeId">租户类型id</param>
        /// <param name="ownerId">所有者id</param>
        /// <param name="ignoreAudit">是否忽略审核状态（作者或管理员查看时忽略审核状态）</param>
        /// <param name="pageSize">分页大小</param>
        /// <param name="pageIndex">页码</param>
        /// <returns>问题分页列表</returns>
        public Tunynet.PagingDataSet<AskQuestion> GetOwnerQuestions(string tenantTypeId, long ownerId, bool ignoreAudit, int pageSize, int pageIndex)
        {
            return GetPagingEntities(pageSize, pageIndex, CachingExpirationType.ObjectCollection,
                () =>
                {
                    StringBuilder cacheKey = new StringBuilder();
                    cacheKey.Append(RealTimeCacheHelper.GetListCacheKeyPrefix(CacheVersionType.AreaVersion, "OwnerId", ownerId));
                    cacheKey.AppendFormat("GetOwnerQuestions::tenantTypeId-{0}:IgnoreAudit-{1}", tenantTypeId, ignoreAudit);
                    return cacheKey.ToString();
                },
                () =>
                {
                    var sql = Sql.Builder.Select("spb_AskQuestions.*").From("spb_AskQuestions");
                    var whereSql = Sql.Builder.Where("spb_AskQuestions.OwnerId=@0", ownerId)
                                                .Where("spb_AskQuestions.Status<>@0", QuestionStatus.Canceled);
                    var orderBy = Sql.Builder.OrderBy("spb_AskQuestions.QuestionId desc");

                    if (!string.IsNullOrEmpty(tenantTypeId))
                    {
                        whereSql.Where("spb_AskQuestions.TenantTypeId=@0", tenantTypeId);
                    }

                    //过滤审核状态
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

                    return sql.Append(whereSql).Append(orderBy);
                });
        }

        /// <summary>
        /// 分页获取用户的问题列表
        /// </summary>
        /// <remarks>
        /// 缓存周期：对象集合
        /// </remarks>
        /// <param name="tenantTypeId">租户类型id</param>
        /// <param name="userId">问题的作者用户id</param>
        /// <param name="ignoreAudit">是否忽略审核状态（作者或管理员查看时忽略审核状态）</param>
        /// <param name="pageSize">分页大小</param>
        /// <param name="pageIndex">页码</param>
        /// <returns>问题分页列表</returns>
        public Tunynet.PagingDataSet<AskQuestion> GetUserQuestions(string tenantTypeId, long userId, bool ignoreAudit, int pageSize, int pageIndex)
        {
            return GetPagingEntities(pageSize, pageIndex, CachingExpirationType.ObjectCollection,
                      () =>
                      {
                          StringBuilder cacheKey = new StringBuilder();
                          cacheKey.Append(RealTimeCacheHelper.GetListCacheKeyPrefix(CacheVersionType.AreaVersion, "UserId", userId));
                          cacheKey.AppendFormat("GetUserQuestions::tenantTypeId-{0}:IgnoreAudit-{1}", tenantTypeId, ignoreAudit);
                          return cacheKey.ToString();
                      },
                      () =>
                      {
                          var sql = Sql.Builder.Select("spb_AskQuestions.*").From("spb_AskQuestions");
                          var whereSql = Sql.Builder.Where("spb_AskQuestions.UserId=@0", userId)
                                                    .Where("spb_AskQuestions.Status<>@0", QuestionStatus.Canceled);
                          var orderBy = Sql.Builder.OrderBy("spb_AskQuestions.QuestionId desc");

                          if (!string.IsNullOrEmpty(tenantTypeId))
                          {
                              whereSql.Where("spb_AskQuestions.TenantTypeId=@0", tenantTypeId);
                          }

                          //过滤审核状态
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

                          return sql.Append(whereSql).Append(orderBy);
                      });
        }

        /// <summary>
        /// 获取零回答的问题（用于前台）
        /// </summary>
        /// <remarks>
        /// 缓存周期：常用对象集合，不用维护即时性
        /// </remarks>
        /// <param name="tenantTypeId">租户类型id</param>
        /// <param name="tagName">标签</param>
        /// <param name="pageSize">分页大小</param>
        /// <param name="pageIndex">页码</param>
        /// <returns>问题分页列表</returns>
        public Tunynet.PagingDataSet<AskQuestion> GetNoAnswerQuestions(string tenantTypeId, string tagName, int pageSize, int pageIndex)
        {
            return GetPagingEntities(pageSize, pageIndex, CachingExpirationType.ObjectCollection,
                      () =>
                      {
                          StringBuilder cacheKey = new StringBuilder();
                          cacheKey.AppendFormat("GetNoAnswerQuestions::tenantTypeId-{0}", tenantTypeId);
                          return cacheKey.ToString();
                      },
                      () =>
                      {
                          var sql = Sql.Builder.Select("spb_AskQuestions.*").From("spb_AskQuestions");
                          var whereSql = Sql.Builder.Where("spb_AskQuestions.AnswerCount=0")
                                                    .Where("spb_AskQuestions.Status<>@0", QuestionStatus.Canceled);
                          var orderBy = Sql.Builder.OrderBy("spb_AskQuestions.QuestionId desc");

                          if (!string.IsNullOrEmpty(tenantTypeId))
                          {
                              whereSql.Where("spb_AskQuestions.TenantTypeId=@0", tenantTypeId);
                          }

                          if (!string.IsNullOrEmpty(tagName))
                          {
                              sql.InnerJoin("tn_ItemsInTags").On("spb_AskQuestions.QuestionId = tn_ItemsInTags.ItemId");
                              whereSql.Where("tn_ItemsInTags.TenantTypeId = @0", TenantTypeIds.Instance().AskQuestion())
                                      .Where("tn_ItemsInTags.TagName=@0", tagName);
                          }

                          //过滤审核状态
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

                          return sql.Append(whereSql).Append(orderBy);
                      });
        }



        /// <summary>
        /// 管理员获取问题分页集合
        /// </summary>
        /// <remarks>
        /// 无需缓存
        /// </remarks>
        /// <param name="tenantTypeId">租户类型id</param>
        /// <param name="auditStatus">审核状态</param>
        /// <param name="questionStatus">问题状态</param>
        /// <param name="ownerId">所属id</param>
        /// <param name="userId">用户id</param>
        /// <param name="keyword">查询关键字</param>
        /// <param name="tagName">标签</param>
        /// <param name="pageSize">分页大小</param>
        /// <param name="pageIndex">页码</param>
        /// <returns>问题分页列表</returns>
        public Tunynet.PagingDataSet<AskQuestion> GetQuestionsForAdmin(string tenantTypeId, AuditStatus? auditStatus, QuestionStatus? questionStatus, long? ownerId, long? userId, string keyword, string tagName, int pageSize, int pageIndex)
        {
            var sql = Sql.Builder.Select("spb_AskQuestions.*").From("spb_AskQuestions");



            var whereSql = Sql.Builder;

            if (!string.IsNullOrEmpty(tenantTypeId))
            {
                whereSql.Where("spb_AskQuestions.TenantTypeId=@0", tenantTypeId);
            }

            if (auditStatus.HasValue)
            {
                whereSql.Where("spb_AskQuestions.AuditStatus=@0", auditStatus.Value);
            }

            if (questionStatus.HasValue)
            {
                whereSql.Where("spb_AskQuestions.Status=@0", questionStatus.Value);
            }

            if (ownerId.HasValue)
            {
                whereSql.Where("spb_AskQuestions.OwnerId=@0", ownerId.Value);
            }

            if (userId.HasValue)
            {
                whereSql.Where("spb_AskQuestions.UserId=@0", userId.Value);
            }

            //todo:jiangshl,by zhengw:是全模糊还是半模糊，应根据站点设置确定，可提供这样一个工具方法；


            if (!string.IsNullOrEmpty(keyword))
            {
                whereSql.Where("spb_AskQuestions.Subject like @0", "%" + StringUtility.StripSQLInjection(keyword) + "%");
            }

            if (!string.IsNullOrEmpty(tagName))
            {
                sql.InnerJoin("tn_ItemsInTags").On("spb_AskQuestions.QuestionId = tn_ItemsInTags.ItemId");
                whereSql.Where("tn_ItemsInTags.TenantTypeId = @0", TenantTypeIds.Instance().AskQuestion())
                        .Where("tn_ItemsInTags.TagName=@0", tagName);
            }
            sql.Append(whereSql).OrderBy("spb_AskQuestions.QuestionId desc");

            return GetPagingEntities(pageSize, pageIndex, sql);
        }

        /// <summary>
        /// 获取问题管理数据
        /// </summary>
        /// <remarks>
        /// 无需缓存
        /// </remarks>
        /// <param name="tenantTypeId">租户类型Id（可以获取该应用下针对某种租户类型的统计计数，默认不进行筛选）</param>
        /// <returns>问题管理数据</returns>
        public Dictionary<string, long> GetManageableData(string tenantTypeId = null)
        {
            Dictionary<string, long> manageableData = new Dictionary<string, long>();

            //查询待审核数
            Sql sql = Sql.Builder.Select("count(*)")
                                 .From("spb_AskQuestions")
                                 .Where("Status<>@0", QuestionStatus.Canceled)
                                 .Where("AuditStatus=@0", AuditStatus.Pending);
            if (!string.IsNullOrEmpty(tenantTypeId))
            {
                sql.Where("TenantTypeId=@0", tenantTypeId);
            }
            manageableData.Add(ApplicationStatisticDataKeys.Instance().PendingCount(), CreateDAO().FirstOrDefault<long>(sql));

            //查询需再审核数
            sql = Sql.Builder.Select("count(*)")
                             .From("spb_AskQuestions")
                             .Where("Status<>@0", QuestionStatus.Canceled)
                             .Where("AuditStatus=@0", AuditStatus.Again);
            if (!string.IsNullOrEmpty(tenantTypeId))
            {
                sql.Where("TenantTypeId=@0", tenantTypeId);
            }
            manageableData.Add(ApplicationStatisticDataKeys.Instance().AgainCount(), CreateDAO().FirstOrDefault<long>(sql));

            return manageableData;
        }

        /// <summary>
        /// 获取问题应用统计数据
        /// </summary>
        /// <param name="tenantTypeId">租户类型Id（可以获取该应用下针对某种租户类型的统计计数，默认不进行筛选）</param>
        /// <returns>问题统计数据</returns>
        public Dictionary<string, long> GetApplicationStatisticData(string tenantTypeId = null)
        {
            StringBuilder cacheKey = new StringBuilder();
            cacheKey.AppendFormat("GetApplicationStatisticData::tenantTypeId-{0}:table-AskQuestion", tenantTypeId);
            Dictionary<string, long> statisticData = cacheService.Get<Dictionary<string, long>>(cacheKey.ToString());
            if (statisticData == null)
            {
                statisticData = new Dictionary<string, long>();

                //查询总数
                Sql sql = Sql.Builder.Select("count(*)")
                                     .From("spb_AskQuestions")
                                     .Where("Status<>@0", QuestionStatus.Canceled);
                if (!string.IsNullOrEmpty(tenantTypeId))
                {
                    sql.Where("TenantTypeId=@0", tenantTypeId);
                }
                statisticData.Add(ApplicationStatisticDataKeys.Instance().TotalCount(), CreateDAO().FirstOrDefault<long>(sql));

                //查询24小时新增数
                sql = Sql.Builder.Select("count(*)")
                                 .From("spb_AskQuestions")
                                 .Where("Status<>@0", QuestionStatus.Canceled)
                                 .Where("DateCreated > @0", DateTime.UtcNow.AddDays(-1));
                if (!string.IsNullOrEmpty(tenantTypeId))
                {
                    sql.Where("TenantTypeId=@0", tenantTypeId);
                }
                statisticData.Add(ApplicationStatisticDataKeys.Instance().Last24HCount(), CreateDAO().FirstOrDefault<long>(sql));

                cacheService.Add(cacheKey.ToString(), statisticData, CachingExpirationType.UsualSingleObject);
            }

            return statisticData;
        }

        /// <summary>
        /// 获取解析的正文
        /// </summary>
        /// <param name="threadId">问题id</param>
        /// <returns>解析后的问题正文</returns>
        public string GetResolvedBody(long threadId)
        {
            string cacheKey = GetCacheKeyOfResolvedBody(threadId);
            string resolveBody = cacheService.Get<string>(cacheKey);

            if (resolveBody == null)
            {
                resolveBody = GetBody(threadId);
                AskQuestion askQuestion = Get(threadId);
                if (askQuestion == null)
                    return null;
                resolveBody = bodyProcessor.Process(resolveBody, TenantTypeIds.Instance().AskQuestion(), threadId, askQuestion.UserId);
                cacheService.Set(cacheKey, resolveBody, CachingExpirationType.SingleObject);
            }

            return resolveBody;
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
                    publiclyAuditStatus = auditService.GetPubliclyAuditStatus(AskConfig.Instance().ApplicationId);
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
        /// <param name="threadId">问题id</param>
        /// <returns>正文缓存key</returns>
        private string GetCacheKeyOfResolvedBody(long threadId)
        {
            return "AskQuestionResolvedBody" + threadId;
        }

        /// <summary>
        /// 获取问题内容
        /// </summary>
        /// <param name="threadId">问题id</param>
        /// <returns>问题正文内容</returns>
        public string GetBody(long threadId)
        {
            string cacheKey = RealTimeCacheHelper.GetCacheKeyOfEntityBody(threadId);
            string body = cacheService.Get<string>(cacheKey);

            if (body == null)
            {
                AskQuestion askQuestion = CreateDAO().SingleOrDefault<AskQuestion>(threadId);
                body = askQuestion != null ? askQuestion.Body : string.Empty;
                body = body ?? string.Empty;
                cacheService.Add(cacheKey, body, CachingExpirationType.SingleObject);
            }

            return body;
        }

        /// <summary>
        /// 通过问题Id获取该问题下所有回答的顶踩数
        /// </summary>
        /// <param name="questionId">问题Id</param>
        /// <param name="tenantTypeId">租户类型Id</param>
        /// <returns></returns>
        public int? GetAnswersAttitudesCount(long questionId, string tenantTypeId)
        {
            StringBuilder cacheKey = new StringBuilder();
            int? attitudeCount = 0;
            cacheKey.AppendFormat("GetApplicationStatisticData::question-{0}:table-AskQuestions", questionId);
            object cacheItem = cacheService.Get(cacheKey.ToString());
            if (cacheItem != null)
            {
                attitudeCount = (int)cacheItem;
                return attitudeCount;
            }
            Sql attitudeSql = Sql.Builder.Append("select sum(SupportCount) from tn_Attitudes ,spb_AskAnswers where tn_Attitudes.objectId = spb_AskAnswers.AnswerId  and questionId=@0 and TenantTypeId=@1", questionId, tenantTypeId);
            attitudeCount = CreateDAO().FirstOrDefault<int?>(attitudeSql);
            cacheService.Add(cacheKey.ToString(), attitudeCount, CachingExpirationType.SingleObject);
            return attitudeCount;
        }
    }
}
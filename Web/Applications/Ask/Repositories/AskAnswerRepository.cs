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

namespace Spacebuilder.Ask
{
    /// <summary>
    /// 问答应用的回答仓储实现
    /// </summary>
    public class AskAnswerRepository : Repository<AskAnswer>, IAskAnswerRepository
    {
        IBodyProcessor bodyProcessor = DIContainer.ResolveNamed<IBodyProcessor>(TenantTypeIds.Instance().AskAnswer());

        /// <summary>
        /// 构造器
        /// </summary>
        public AskAnswerRepository()
        {

        }

        /// <summary>
        /// 构造器
        /// </summary>
        /// <param name="publiclyAuditStatus"></param>
        public AskAnswerRepository(PubliclyAuditStatus publiclyAuditStatus)
        {
            this.publiclyAuditStatus = publiclyAuditStatus;
        }

        /// <summary>
        /// 更新回答
        /// </summary>
        /// <param name="entity"></param>
        public override void Update(AskAnswer entity)
        {
            base.Update(entity);
            //更新解析正文缓存
            string cacheKey = GetCacheKeyOfResolvedBody(entity.AnswerId);
            string resolveBody = bodyProcessor.Process(entity.Body, TenantTypeIds.Instance().AskAnswer(), entity.AnswerId, entity.UserId);
            if (!string.IsNullOrEmpty(resolveBody))
            {
                cacheService.Set(cacheKey, resolveBody, CachingExpirationType.SingleObject);
            }
        }

        /// <summary>
        /// 指定用户接管当前用户的回答
        /// </summary>
        /// <param name="userId">当前用户id</param>
        /// <param name="takeOverUser">指定用户</param>
        public void TakeOver(long userId, Common.User takeOverUser)
        {
            Sql sql = Sql.Builder.Append("update spb_AskAnswers set UserId=@0,Author=@1 where UserId=@2", takeOverUser.UserId, takeOverUser.DisplayName, userId);
            CreateDAO().Execute(sql);
        }

        /// <summary>
        /// 分页获取用户的回答列表
        /// </summary>
        /// <remarks>
        /// 缓存周期：对象集合
        /// </remarks>
        /// <param name="userId">回答的作者用户id</param>
        /// <param name="ignoreAudit">是否忽略审核状态（作者或管理员查看时忽略审核状态）</param>
        /// <param name="pageSize">分页大小</param>
        /// <param name="pageIndex">页码</param>
        /// <returns>回答分页列表</returns>
        public Tunynet.PagingDataSet<AskAnswer> GetUserAnswers(long userId, bool ignoreAudit, int pageSize, int pageIndex)
        {
            return GetPagingEntities(pageSize, pageIndex, CachingExpirationType.ObjectCollection,
                () =>
                {
                    StringBuilder cacheKey = new StringBuilder();
                    cacheKey.Append(RealTimeCacheHelper.GetListCacheKeyPrefix(CacheVersionType.AreaVersion, "UserId", userId));
                    cacheKey.AppendFormat("GetUserAnswers::IgnoreAudit-{0}", ignoreAudit);
                    return cacheKey.ToString();
                },
                () =>
                {


                    var sql = Sql.Builder.Select("*").From("spb_AskAnswers");
                    var whereSql = Sql.Builder.Where("UserId=@0", userId);
                    var orderSql = Sql.Builder.OrderBy("AnswerId desc");

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

                    return sql.Append(whereSql).Append(orderSql);
                });
        }

        /// <summary>
        /// 根据问题id获取回答实体列表
        /// </summary>
        /// <remarks>
        /// 缓存周期：对象集合
        /// </remarks>
        /// <param name="questionId">问题id</param>
        /// <param name="sortBy">排序条件</param>
        /// <param name="pageSize">分页大小</param>
        /// <param name="pageIndex">页码</param>
        /// <returns>回答分页列表</returns>
        public Tunynet.PagingDataSet<AskAnswer> GetAnswersByQuestionId(long questionId, SortBy_AskAnswer sortBy, int pageSize, int pageIndex)
        {
            return GetPagingEntities(pageSize, pageIndex, CachingExpirationType.ObjectCollection,
                () =>
                {
                    StringBuilder cacheKey = new StringBuilder();
                    cacheKey.Append(RealTimeCacheHelper.GetListCacheKeyPrefix(CacheVersionType.AreaVersion, "QuestionId", questionId));
                    cacheKey.AppendFormat("GetAnswersByQuestionId::sortBy-{0}", sortBy);
                    return cacheKey.ToString();
                },
                () =>
                {
                    var sql = Sql.Builder.Select("spb_AskAnswers.*").From("spb_AskAnswers");
                    var whereSql = Sql.Builder.Where("spb_AskAnswers.QuestionId=@0", questionId);
                    var orderSql = Sql.Builder;

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

                    switch (sortBy)
                    {
                        case SortBy_AskAnswer.SupportCount:
                            sql.LeftJoin("tn_Attitudes").On("spb_AskAnswers.AnswerId = tn_Attitudes.ObjectId");
                            whereSql.Where("(tn_Attitudes.TenantTypeId=@0 or tn_Attitudes.TenantTypeId is null)", TenantTypeIds.Instance().AskAnswer());
                            orderSql.OrderBy("spb_AskAnswers.IsBest desc")
                                    .OrderBy("tn_Attitudes.SupportCount desc")
                                    .OrderBy("spb_AskAnswers.AnswerId desc");
                            break;
                        default:
                            orderSql.OrderBy("spb_AskAnswers.IsBest desc")
                                    .OrderBy("spb_AskAnswers.AnswerId desc");
                            break;
                    }
                    return sql.Append(whereSql).Append(orderSql);
                });
        }

        /// <summary>
        /// 管理员获取回答分页集合
        /// </summary>
        /// <remarks>
        /// 无需缓存
        /// </remarks>
        /// <param name="auditStatus">审核状态</param>
        /// <param name="userId">用户id</param>
        /// <param name="keyword">查询关键字</param>
        /// <param name="pageSize">分页大小</param>
        /// <param name="pageIndex">页码</param>
        /// <returns>回答分页列表</returns>
        public Tunynet.PagingDataSet<AskAnswer> GetAnswersForAdmin(Tunynet.Common.AuditStatus? auditStatus, long? userId, string keyword, int pageSize, int pageIndex)
        {
            var sql = Sql.Builder.Select("spb_AskAnswers.*").From("spb_AskAnswers");
            var whereSql = Sql.Builder;

            if (auditStatus.HasValue)
            {
                whereSql.Where("spb_AskAnswers.AuditStatus=@0", auditStatus.Value);
            }


            if (userId.HasValue && userId.Value > 0)
            {
                whereSql.Where("spb_AskAnswers.UserId=@0", userId.Value);
            }

            if (!string.IsNullOrEmpty(keyword))
            {
                whereSql.Where("spb_AskAnswers.Body like @0", "%" + StringUtility.StripSQLInjection(keyword) + "%");
            }

            sql.Append(whereSql).OrderBy("spb_AskAnswers.AnswerId desc");

            return GetPagingEntities(pageSize, pageIndex, sql);
        }

        /// <summary>
        /// 根据用户id和问题id获取回答实体
        /// </summary>
        /// <param name="userId">用户id</param>
        /// <param name="questionId">问题id</param>
        /// <returns>问题实体</returns>
        public AskAnswer GetUserAnswerByQuestionId(long userId, long questionId)
        {
            var sql = Sql.Builder.Select("*")
                                 .From("spb_AskAnswers")
                                 .Where("UserId=@0", userId)
                                 .Where("QuestionId=@0", questionId);

            return CreateDAO().FirstOrDefault<AskAnswer>(sql);
        }

        /// <summary>
        /// 获取回答管理数据
        /// </summary>
        /// <remarks>
        /// 无需缓存
        /// </remarks>
        /// <returns>回答管理数据</returns>
        public Dictionary<string, long> GetManageableData()
        {
            Dictionary<string, long> manageableData = new Dictionary<string, long>();

            //查询待审核数
            Sql sql = Sql.Builder.Select("count(*)")
                                 .From("spb_AskAnswers")
                                 .Where("AuditStatus=@0", AuditStatus.Pending);
            manageableData.Add(ApplicationStatisticDataKeys.Instance().PendingCount(), CreateDAO().FirstOrDefault<long>(sql));

            //查询需再审核数
            sql = Sql.Builder.Select("count(*)")
                             .From("spb_AskAnswers")
                             .Where("AuditStatus=@0", AuditStatus.Again);
            manageableData.Add(ApplicationStatisticDataKeys.Instance().AgainCount(), CreateDAO().FirstOrDefault<long>(sql));

            return manageableData;
        }

        /// <summary>
        /// 获取回答应用统计数据
        /// </summary>
        /// <returns>回答统计数据</returns>
        public Dictionary<string, long> GetApplicationStatisticData()
        {
            StringBuilder cacheKey = new StringBuilder();
            cacheKey.Append("GetApplicationStatisticData::table-AskAnswer");
            ICacheService cacheService = DIContainer.Resolve<ICacheService>();
            Dictionary<string, long> statisticData = cacheService.Get<Dictionary<string, long>>(cacheKey.ToString());
            if (statisticData == null)
            {
                statisticData = new Dictionary<string, long>();

                //查询总数
                Sql sql = Sql.Builder.Select("count(*)")
                                     .From("spb_AskAnswers");
                statisticData.Add(ApplicationStatisticDataKeys.Instance().TotalCount(), CreateDAO().FirstOrDefault<long>(sql));

                //查询24小时新增数
                sql = Sql.Builder.Select("count(*)")
                                 .From("spb_AskAnswers")
                                 .Where("DateCreated > @0", DateTime.UtcNow.AddDays(-1));
                statisticData.Add(ApplicationStatisticDataKeys.Instance().Last24HCount(), CreateDAO().FirstOrDefault<long>(sql));

                cacheService.Add(cacheKey.ToString(), statisticData, CachingExpirationType.UsualSingleObject);
            }

            return statisticData;
        }

        /// <summary>
        /// 获取解析的正文
        /// </summary>
        /// <param name="threadId">回答id</param>
        /// <returns>解析后的回答正文</returns>
        public string GetResolvedBody(long threadId)
        {
            string cacheKey = GetCacheKeyOfResolvedBody(threadId);
            string resolveBody = cacheService.Get<string>(cacheKey);

            if (resolveBody == null)
            {
                resolveBody = GetBody(threadId);
                if (!string.IsNullOrEmpty(resolveBody))
                {
                    AskAnswer askAnswer = Get(threadId);
                    resolveBody = bodyProcessor.Process(resolveBody, TenantTypeIds.Instance().AskAnswer(), threadId, askAnswer.UserId);
                    cacheService.Set(cacheKey, resolveBody, CachingExpirationType.SingleObject);
                }
            }

            return resolveBody;
        }

        /// <summary>
        /// 获取解析正文缓存Key
        /// </summary>
        /// <param name="threadId">回答id</param>
        /// <returns>正文缓存key</returns>
        private string GetCacheKeyOfResolvedBody(long threadId)
        {
            return "AskAnswerResolvedBody" + threadId;
        }

        /// <summary>
        /// 获取回答内容
        /// </summary>
        /// <param name="threadId">回答id</param>
        /// <returns>回答正文内容</returns>
        public string GetBody(long threadId)
        {
            string cacheKey = RealTimeCacheHelper.GetCacheKeyOfEntityBody(threadId);
            string body = cacheService.Get<string>(cacheKey);

            if (body == null)
            {
                AskAnswer askAnswer = CreateDAO().SingleOrDefault<AskAnswer>(threadId);
                if (askAnswer == null)
                {
                    return string.Empty;
                }

                body = askAnswer.Body;
                cacheService.Add(cacheKey, body, CachingExpirationType.SingleObject);
            }

            return body;
        }

        /// <summary>
        /// 统计某个标签的回答数
        /// </summary>
        /// <param name="tagName">标签名</param>
        /// <returns>回答数</returns>
        public long GetAnswerCountOfTag(string tagName)
        {
            StringBuilder cacheKey = new StringBuilder();
            cacheKey.AppendFormat("GetAnswerCountOfTag::tagName-{0}", tagName);
            string count = cacheService.Get<string>(cacheKey.ToString());
            if (string.IsNullOrEmpty(count))
            {
                Sql sql = Sql.Builder.Select("count(*)")
                                     .From("spb_AskAnswers")
                                     .InnerJoin("tn_ItemsInTags").On("spb_AskAnswers.QuestionId = tn_ItemsInTags.ItemId")
                                     .Where("tn_ItemsInTags.TenantTypeId = @0", TenantTypeIds.Instance().AskQuestion())
                                     .Where("tn_ItemsInTags.TagName=@0", tagName);

                count = CreateDAO().FirstOrDefault<long>(sql).ToString();
                cacheService.Add(cacheKey.ToString(), count, CachingExpirationType.UsualSingleObject);
            }

            return long.Parse(count);
        }

        /// <summary>
        /// 获取某个回答所在的页码
        /// </summary>
        public long GetPageIndexOfCurrentAnswer(AskAnswer askAnswer, long pageSize)
        {
            StringBuilder cacheKey = new StringBuilder();
            cacheKey.AppendFormat("GetPageIndexOfCurrentAnswer::answerId-{0}:questionId-{1}:pageSize-{2}", askAnswer.AnswerId, askAnswer.QuestionId, pageSize);
            string pageIndex = cacheService.Get<string>(cacheKey.ToString());
            if (string.IsNullOrEmpty(pageIndex))
            {
                Sql sql = Sql.Builder.Select("count(*)").From("spb_AskAnswers");
                Sql whereSql = Sql.Builder.Where("AnswerId>=@0", askAnswer.AnswerId)
                                          .Where("QuestionId=@0", askAnswer.QuestionId);
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
                sql.Append(whereSql);
                long count = CreateDAO().FirstOrDefault<long>(sql);
                long mod = count % pageSize;
                pageIndex = (count / pageSize).ToString();
                if (mod > 0)
                {
                    long index = long.Parse(pageIndex);
                    index += 1;
                    pageIndex = index.ToString();
                }
                cacheService.Add(cacheKey.ToString(), pageIndex, CachingExpirationType.UsualSingleObject);
            }
            return long.Parse(pageIndex);
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
                    int askApplicationId = AskConfig.Instance().ApplicationId;
                    publiclyAuditStatus = auditService.GetPubliclyAuditStatus(askApplicationId);
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
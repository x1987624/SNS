//<TunynetCopyright>
//--------------------------------------------------------------
//<copyright>拓宇网络科技有限公司 2005-2013</copyright>
//<version>V0.5</verion>
//<createdate>2013-06-22</createdate>
//<author>zhengw</author>
//<email>zhengw@tunynet.com</email>
//<log date="2013-06-22" version="0.5">创建</log>
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

namespace Spacebuilder.Wiki
{
    /// <summary>
    /// 词条仓储
    /// </summary>
    public class WikiPageRepository : Repository<WikiPage>, IWikiPageRepository
    {
        public WikiPageRepository() { }

        public WikiPageRepository(PubliclyAuditStatus publiclyAuditStatus)
        {
            this.publiclyAuditStatus = publiclyAuditStatus;
        }

        /// <summary>
        /// 删除词条时，同时删除版本
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public override int Delete(WikiPage entity)
        {
            int result = base.Delete(entity);
            CreateDAO().Execute(Sql.Builder.Append("delete from spb_WikiPageVersions where PageId=@0", entity.PageId));
            return result;
        }

        /// <summary>
        /// 删除某用户时指定其他用户接管数据
        /// </summary>
        /// <param name="userId">要删词条的用户Id</param>
        /// <param name="takeOverUser">要接管词条的用户</param>
        public void TakeOver(long userId, User takeOverUser)
        {
            List<Sql> sqls = new List<Sql>();

            //更新所属为用户的词条（UserId=OwnerId）
            sqls.Add(Sql.Builder.Append("update spb_WikiPages set UserId = @0,OwnerId=@1,Author=@2 where UserId=OwnerId and UserId=@3", takeOverUser.UserId, takeOverUser.UserId, takeOverUser.DisplayName, userId));

            //更新所属为群组的词条（UserId<>OwnerId）,OwnerId不更新
            sqls.Add(Sql.Builder.Append("update spb_WikiPages set UserId = @0,Author=@1 where UserId<>OwnerId and UserId=@2", takeOverUser.UserId, takeOverUser.DisplayName, userId));

            //更新词条版本
            sqls.Add(Sql.Builder.Append("update spb_WikiPageVersions set UserId = @0,Author=@1 where UserId=@2", takeOverUser.UserId, takeOverUser.DisplayName, userId));

            CreateDAO().Execute(sqls);
        }

        /// <summary>
        /// 获取Owner的词条
        /// </summary>
        /// <param name="tenantTypeId">租户类型Id</param>
        /// <param name="ownerId">拥有者Id</param>
        /// <param name="ignoreAudit">是否忽略审核状态（作者或管理员查看时忽略审核状态）</param>
        /// <param name="tagName">标签名称</param>
        /// <param name="pageSize">分页大小</param> 
        /// <param name="pageIndex">页码</param>        
        /// <returns>词条分页列表</returns>
        public PagingDataSet<WikiPage> GetOwnerPages(string tenantTypeId, long ownerId, bool ignoreAudit, string tagName, int pageSize = 20, int pageIndex = 1)
        {
            return GetPagingEntities(pageSize, pageIndex, CachingExpirationType.ObjectCollection,
                       () =>
                       {
                           StringBuilder cacheKey = new StringBuilder();
                           cacheKey.Append(RealTimeCacheHelper.GetListCacheKeyPrefix(CacheVersionType.AreaVersion, "OwnerId", ownerId));
                           cacheKey.AppendFormat("WikiOwner::TenantTypeId-{0}:IgnoreAudit-{1}:TagName-{2}", tenantTypeId, ignoreAudit, tagName);
                           return cacheKey.ToString();
                       },
                       () =>
                       {
                           var sql = Sql.Builder.Select("spb_WikiPages.*").From("spb_WikiPages");

                           //排除逻辑删除的词条
                           var whereSql = Sql.Builder.Where("spb_WikiPages.OwnerId=@0", ownerId)
                                                     .Where("spb_WikiPages.TenantTypeId=@0", tenantTypeId)
                                                     .Where("spb_WikiPages.IsLogicalDelete=0");

                           //置顶词条靠前显示
                           var orderBy = Sql.Builder.OrderBy("spb_WikiPages.PageId desc");

                           if (!string.IsNullOrEmpty(tagName))
                           {
                               sql.InnerJoin("tn_ItemsInTags").On("spb_WikiPages.PageId = tn_ItemsInTags.ItemId");
                               whereSql.Where("tn_ItemsInTags.TenantTypeId = @0", TenantTypeIds.Instance().WikiPage())
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
                                       whereSql.Where("spb_WikiPages.AuditStatus=@0", this.PubliclyAuditStatus);
                                       break;
                                   case PubliclyAuditStatus.Again_GreaterThanOrEqual:
                                   case PubliclyAuditStatus.Pending_GreaterThanOrEqual:
                                       whereSql.Where("spb_WikiPages.AuditStatus>@0", this.PubliclyAuditStatus);
                                       break;
                                   default:
                                       break;
                               }
                           }

                           return sql.Append(whereSql).Append(orderBy);
                       });
        }

        /// <summary>
        /// 获取词条的排行数据
        /// </summary>
        /// <param name="tenantTypeId">租户类型Id</param>
        /// <param name="topNumber">前多少条</param>
        /// <param name="categoryId">类别Id</param>
        /// <param name="isEssential">是否精华</param>
        /// <param name="sortBy">词条排序依据</param>
        /// <returns>词条列表</returns>
        public IEnumerable<WikiPage> GetTops(string tenantTypeId, int topNumber, long? categoryId, bool? isEssential, SortBy_WikiPage sortBy)
        {
            return GetTopEntities(topNumber, CachingExpirationType.UsualObjectCollection,
                    () =>
                    {
                        StringBuilder cacheKey = new StringBuilder();
                        cacheKey.AppendFormat("WikiTops::TenantTypeId-{0}:CategoryId-{1}:IsEssential-{2}:SortBy-{3}", tenantTypeId, categoryId, isEssential, sortBy);
                        return cacheKey.ToString();
                    },
                    () =>
                    {
                        var sql = Sql.Builder.Select("spb_WikiPages.*")
                                             .From("spb_WikiPages");

                        var whereSql = Sql.Builder.Where("spb_WikiPages.IsLogicalDelete=0");

                        if (!string.IsNullOrEmpty(tenantTypeId))
                        {
                            whereSql.Where("spb_WikiPages.TenantTypeId=@0", tenantTypeId);
                        }

                        if (categoryId.HasValue && categoryId > 0)
                        {
                            sql.InnerJoin("tn_ItemsInCategories").On("spb_WikiPages.PageId=tn_ItemsInCategories.ItemId");
                            whereSql.Where("tn_ItemsInCategories.CategoryId=@0", categoryId.Value);
                        }

                        switch (this.PubliclyAuditStatus)
                        {
                            case PubliclyAuditStatus.Again:
                            case PubliclyAuditStatus.Fail:
                            case PubliclyAuditStatus.Pending:
                            case PubliclyAuditStatus.Success:
                                whereSql.Where("spb_WikiPages.AuditStatus=@0", this.PubliclyAuditStatus);
                                break;
                            case PubliclyAuditStatus.Again_GreaterThanOrEqual:
                            case PubliclyAuditStatus.Pending_GreaterThanOrEqual:
                                whereSql.Where("spb_WikiPages.AuditStatus>@0", this.PubliclyAuditStatus);
                                break;
                            default:
                                break;
                        }
                        if (isEssential.HasValue)
                        {
                            whereSql.Where("spb_WikiPages.IsEssential=@0", isEssential);
                        }

                        var orderSql = Sql.Builder;

                        CountService countService = new CountService(TenantTypeIds.Instance().WikiPage());
                        string countTableName = countService.GetTableName_Counts();
                        switch (sortBy)
                        {
                            case SortBy_WikiPage.DateCreated_Desc:
                                orderSql.OrderBy("spb_WikiPages.PageId desc");
                                break;
                            case SortBy_WikiPage.StageHitTimes:
                                StageCountTypeManager stageCountTypeManager = StageCountTypeManager.Instance(TenantTypeIds.Instance().WikiPage());
                                int stageCountDays = stageCountTypeManager.GetMaxDayCount(CountTypes.Instance().HitTimes());
                                string stageCountType = stageCountTypeManager.GetStageCountType(CountTypes.Instance().HitTimes(), stageCountDays);
                                sql.LeftJoin(string.Format("(select * from {0} WHERE ({0}.CountType = '{1}')) c", countTableName, stageCountType))
                                .On("PageId = c.ObjectId");
                                orderSql.OrderBy("c.StatisticsCount desc");
                                break;
                            default:
                                orderSql.OrderBy("spb_WikiPages.PageId desc");
                                break;
                        }
                        return sql.Append(whereSql).Append(orderSql);
                    });
        }

        /// <summary>
        /// 获取词条排行分页集合
        /// </summary>
        /// <param name="tenantTypeId">租户类型Id</param>
        /// <param name="ignoreAudit">是否忽略审核状态</param>
        /// <param name="ownerId">所有者id</param>
        /// <param name="categoryId">类别Id</param>
        /// <param name="tagName">标签名称</param>
        /// <param name="isEssential">是否精华</param>
        /// <param name="sortBy">帖子排序依据</param>
        /// <param name="pageSize">分页大小</param>
        /// <param name="pageIndex">页码</param>
        /// <returns>词条分页列表</returns>
        public PagingDataSet<WikiPage> Gets(string tenantTypeId, long? ownerId, bool ignoreAudit, long? categoryId, string tagName, bool? isEssential, SortBy_WikiPage sortBy = SortBy_WikiPage.DateCreated_Desc, int pageSize = 20, int pageIndex = 1)
        {
            var sql = Sql.Builder.Select("spb_WikiPages.*").From("spb_WikiPages");
            var whereSql = Sql.Builder.Where("spb_WikiPages.IsLogicalDelete=0 and AuditStatus=40");
            var orderSql = Sql.Builder;


            if (!string.IsNullOrEmpty(tenantTypeId))
            {
                whereSql.Where("spb_WikiPages.TenantTypeId=@0", tenantTypeId);
            }

            if (ownerId.HasValue)
            {
                whereSql.Where("spb_WikiPages.OwnerId=@0", ownerId.Value);
            }

            if (categoryId.HasValue && categoryId > 0)
            {
                sql.InnerJoin("tn_ItemsInCategories").On("spb_WikiPages.PageId=tn_ItemsInCategories.ItemId");
                whereSql.Where("tn_ItemsInCategories.CategoryId=@0", categoryId.Value);
            }

            if (!string.IsNullOrEmpty(tagName))
            {
                whereSql.Where("spb_WikiPages.PageId in ( select ItemId from tn_ItemsInTags where tn_ItemsInTags.TenantTypeId = @0 and tn_ItemsInTags.TagName=@1 )", TenantTypeIds.Instance().WikiPage(), tagName);
            }

            if (!ignoreAudit)
            {
                switch (this.PubliclyAuditStatus)
                {
                    case PubliclyAuditStatus.Again:
                    case PubliclyAuditStatus.Fail:
                    case PubliclyAuditStatus.Pending:
                    case PubliclyAuditStatus.Success:
                        whereSql.Where("spb_WikiPages.AuditStatus=@0", this.PubliclyAuditStatus);
                        break;
                    case PubliclyAuditStatus.Again_GreaterThanOrEqual:
                    case PubliclyAuditStatus.Pending_GreaterThanOrEqual:
                        whereSql.Where("spb_WikiPages.AuditStatus>@0", this.PubliclyAuditStatus);
                        break;
                    default:
                        break;
                }
            }
            if (isEssential.HasValue)
            {
                whereSql.Where("spb_WikiPages.IsEssential=@0", isEssential.Value);
            }

            CountService countService = new CountService(TenantTypeIds.Instance().WikiPage());
            string countTableName = countService.GetTableName_Counts();
            switch (sortBy)
            {
                case SortBy_WikiPage.DateCreated_Desc:
                    orderSql.OrderBy("spb_WikiPages.LastModified desc");
                    break;
                case SortBy_WikiPage.StageHitTimes:
                    StageCountTypeManager stageCountTypeManager = StageCountTypeManager.Instance(TenantTypeIds.Instance().WikiPage());
                    int stageCountDays = stageCountTypeManager.GetMaxDayCount(CountTypes.Instance().HitTimes());
                    string stageCountType = stageCountTypeManager.GetStageCountType(CountTypes.Instance().HitTimes(), stageCountDays);
                    sql.LeftJoin(string.Format("(select * from {0} WHERE ({0}.CountType = '{1}')) c", countTableName, stageCountType))
                    .On("PageId = c.ObjectId");
                    orderSql.OrderBy("c.StatisticsCount desc");
                    break;
                default:
                    orderSql.OrderBy("spb_WikiPages.PageId desc");
                    break;
            }

            sql.Append(whereSql).Append(orderSql);
            return GetPagingEntities(pageSize, pageIndex, sql);
        }

        /// <summary>
        /// 获取词条排行分页集合
        /// </summary>
        /// <param name="tenantTypeId">租户类型Id</param>
        /// <param name="auditStatus">审核状态</param>
        /// <param name="categoryId">站点类别Id</param>
        /// <param name="isEssential">是否精华</param>
        /// <param name="ownerId">OwnerId</param>
        /// <param name="titleKeywords">标题关键字</param>        
        /// <param name="pageSize">分页大小</param>
        /// <param name="pageIndex">页码</param>
        /// <returns>词条分页列表</returns>
        public PagingDataSet<WikiPage> GetsForAdmin(string tenantTypeId, AuditStatus? auditStatus, long? categoryId, bool? isEssential, long? ownerId, string titleKeywords, int pageSize = 20, int pageIndex = 1)
        {
            var sql = Sql.Builder.Select("spb_WikiPages.*").From("spb_WikiPages");
            var whereSql = Sql.Builder.Where("spb_WikiPages.IsLogicalDelete=0");

            if (!string.IsNullOrEmpty(tenantTypeId))
            {
                whereSql.Where("spb_WikiPages.TenantTypeId=@0", tenantTypeId);
            }

            if (auditStatus.HasValue)
            {
                whereSql.Where("spb_WikiPages.AuditStatus=@0", auditStatus.Value);
            }

            if (categoryId.HasValue && categoryId > 0)
            {
                sql.InnerJoin("tn_ItemsInCategories").On("spb_WikiPages.PageId=tn_ItemsInCategories.ItemId");
                whereSql.Where("tn_ItemsInCategories.CategoryId=@0", categoryId.Value);
            }

            if (isEssential.HasValue)
            {
                whereSql.Where("spb_WikiPages.IsEssential=@0", isEssential);
            }

            if (ownerId.HasValue)
            {
                whereSql.Where("spb_WikiPages.OwnerId=@0", ownerId);
            }

            if (!string.IsNullOrEmpty(titleKeywords))
            {
                whereSql.Where("spb_WikiPages.Title like @0", "%" + StringUtility.StripSQLInjection(titleKeywords) + "%");
            }

            sql.Append(whereSql).OrderBy("spb_WikiPages.PageId desc");

            return GetPagingEntities(pageSize, pageIndex, sql);
        }

        /// <summary>
        /// 根据词条名获取词条Id
        /// </summary>
        /// <param name="title"></param>
        /// <returns></returns>
        public long GetPageIdByTitle(string title)
        {
            var sql = Sql.Builder.Select("PageId").From("spb_WikiPages").Where("Title = @0", title);
            return CreateDAO().FirstOrDefault<long>(sql);
        }

        /// <summary>
        /// 获取词条管理数据
        /// </summary>
        /// <param name="tenantTypeId">租户类型Id（可以获取该应用下针对某种租户类型的统计计数，默认不进行筛选）</param>
        /// <returns>词条管理数据</returns>
        public Dictionary<string, long> GetManageableDatas(string tenantTypeId = null)
        {
            Dictionary<string, long> manageableDatas = new Dictionary<string, long>();

            //查询待审核数
            Sql sql = Sql.Builder.Select("count(*)")
                                 .From("spb_WikiPages")
                                 .Where("IsLogicalDelete=0")
                                 .Where("AuditStatus=@0", AuditStatus.Pending);
            if (!string.IsNullOrEmpty(tenantTypeId))
            {
                sql.Where("TenantTypeId=@0", tenantTypeId);
            }
            manageableDatas.Add(ApplicationStatisticDataKeys.Instance().PendingCount(), CreateDAO().FirstOrDefault<long>(sql));

            //查询需再审核数
            sql = Sql.Builder.Select("count(*)")
                             .From("spb_WikiPages")
                             .Where("IsLogicalDelete=0")
                             .Where("AuditStatus=@0", AuditStatus.Again);
            if (!string.IsNullOrEmpty(tenantTypeId))
            {
                sql.Where("TenantTypeId=@0", tenantTypeId);
            }
            manageableDatas.Add(ApplicationStatisticDataKeys.Instance().AgainCount(), CreateDAO().FirstOrDefault<long>(sql));

            return manageableDatas;
        }

        /// <summary>
        /// 获取词条统计数据
        /// </summary>
        /// <param name="tenantTypeId">租户类型Id（可以获取该应用下针对某种租户类型的统计计数，默认不进行筛选）</param>
        /// <returns>词条统计数据</returns>
        public Dictionary<string, long> GetStatisticDatas(string tenantTypeId = null)
        {
            string cacheKey = "WikiPageStatisticData";
            Dictionary<string, long> statisticDatas = cacheService.Get<Dictionary<string, long>>(cacheKey);
            if (statisticDatas == null)
            {
                statisticDatas = new Dictionary<string, long>();

                //查询总数
                Sql sql = Sql.Builder.Select("count(*)")
                                     .From("spb_WikiPages")
                                     .Where("IsLogicalDelete=0");
                if (!string.IsNullOrEmpty(tenantTypeId))
                {
                    sql.Where("TenantTypeId=@0", tenantTypeId);
                }
                statisticDatas.Add(ApplicationStatisticDataKeys.Instance().TotalCount(), CreateDAO().FirstOrDefault<long>(sql));

                //查询24小时新增数
                sql = Sql.Builder.Select("count(*)")
                                 .From("spb_WikiPages")
                                 .Where("IsLogicalDelete=0")
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

        private PubliclyAuditStatus publiclyAuditStatus;
        /// <summary>
        /// 百科应用可对外显示的审核状态
        /// </summary>
        protected PubliclyAuditStatus PubliclyAuditStatus
        {
            get
            {
                AuditService auditService = new AuditService();
                publiclyAuditStatus = auditService.GetPubliclyAuditStatus(WikiConfig.Instance().ApplicationId);
                return publiclyAuditStatus;
            }
            set
            {
                this.publiclyAuditStatus = value;
            }
        }

        /// <summary>
        /// 获取问答的标题和内容
        /// </summary>
        /// <param name="questionId"></param>
        /// <param name="title"></param>
        /// <param name="body"></param>
        public void GetAskTitleAndBody(long questionId, out string title, out string body)
        {
            IBodyProcessor bodyProcessor = DIContainer.ResolveNamed<IBodyProcessor>("101300");
            var dao = CreateDAO();
            string questionBody = string.Empty;
            dynamic question = dao.FirstOrDefault<dynamic>(Sql.Builder.Select("*").From("spb_AskQuestions").Where("QuestionId=@0", questionId));
            title = question.Subject;
            questionBody = question.Body;
            bodyProcessor.Process(questionBody, "101301", questionId, question.UserId);

            dynamic answer = dao.FirstOrDefault<dynamic>(Sql.Builder.Select("*").From("spb_AskAnswers").Where("QuestionId=@0 and IsBest=1 ", questionId));
            string answerBody = "";
            if (answer != null)
            {
                answerBody = answer.Body;
                bodyProcessor.Process(answerBody, "101302", answer.AnswerId, answer.UserId);
            }
            body = questionBody + answerBody;
        }


        /// <summary>
        /// 获取用户编辑过的词条
        /// </summary>
        /// <param name="tenantTypeId"></param>
        /// <param name="userId"></param>
        /// <param name="ignoreAudit"></param>
        /// <param name="pageSize"></param>
        /// <param name="pageIndex"></param>
        /// <returns></returns>
        public PagingDataSet<WikiPage> GetUserEditedPages(string tenantTypeId, long userId, bool ignoreAudit, int pageSize, int pageIndex)
        {
            var sql = Sql.Builder.Select("distinct PageId").From("spb_WikiPageVersions");

            var whereSql = Sql.Builder.Where("TenantTypeId=@0", tenantTypeId)
            .Where("UserId=@0", userId)
            .Where("VersionNum = 0");

            var orderBy = Sql.Builder.OrderBy("PageId desc");

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

            sql.Append(whereSql).Append(orderBy);

            return GetPagingEntities(pageSize, pageIndex, sql);
        }

        /// <summary>
        /// 获取需要用户完善的词条
        /// </summary>
        /// <param name="tenantTypeId"></param>
        /// <param name="userId"></param>
        /// <param name="pageSize"></param>
        /// <param name="pageIndex"></param>
        /// <returns></returns>
        public PagingDataSet<WikiPage> GetPerfectPages(string tenantTypeId, long userId, int pageSize, int pageIndex)
        {
            IEnumerable<string> tagNames = null;
            TagService tagService = new TagService(TenantTypeIds.Instance().User());
            tagNames = tagService.GetItemInTagsOfItem(userId).Select(n => n.TagName);
            if (tagNames == null || tagNames.Count() == 0)
                return new PagingDataSet<WikiPage>(new List<WikiPage>());

            var sql = Sql.Builder.Select("distinct PageId").From("spb_WikiPages");

            //排除逻辑删除的词条
            var whereSql = Sql.Builder.Where("spb_WikiPages.TenantTypeId=@0", tenantTypeId)
                                      .Where("spb_WikiPages.IsLogicalDelete=0");

            var orderBy = Sql.Builder.OrderBy("PageId desc");

            sql.InnerJoin("tn_ItemsInTags").On("spb_WikiPages.PageId = tn_ItemsInTags.ItemId");
            whereSql.Where("tn_ItemsInTags.TenantTypeId = @0", TenantTypeIds.Instance().WikiPage())
                    .Where("tn_ItemsInTags.TagName in (@tagNames)", new { tagNames = tagNames });

            switch (this.PubliclyAuditStatus)
            {
                case PubliclyAuditStatus.Again:
                case PubliclyAuditStatus.Fail:
                case PubliclyAuditStatus.Pending:
                case PubliclyAuditStatus.Success:
                    whereSql.Where("spb_WikiPages.AuditStatus=@0", this.PubliclyAuditStatus);
                    break;
                case PubliclyAuditStatus.Again_GreaterThanOrEqual:
                case PubliclyAuditStatus.Pending_GreaterThanOrEqual:
                    whereSql.Where("spb_WikiPages.AuditStatus>@0", this.PubliclyAuditStatus);
                    break;
                default:
                    break;
            }

            sql.Append(whereSql).Append(orderBy);

            return GetPagingEntities(pageSize, pageIndex, sql);
        }


        /// <summary>
        /// 获取词条排行分页集合
        /// </summary>
        /// <param name="tenantTypeId">租户类型Id</param>
        /// <param name="ignoreAudit">是否忽略审核状态</param>
        /// <param name="ownerId">所有者id</param>
        /// <param name="categoryId">类别Id</param>
        /// <param name="tagName">标签名称</param>
        /// <param name="isEssential">是否精华</param>
        /// <param name="sortBy">帖子排序依据</param>
        /// <param name="pageSize">分页大小</param>
        /// <param name="pageIndex">页码</param>
        /// <returns>词条分页列表</returns>
        public PagingDataSet<WikiPage> Gets(string keyWord, string tenantTypeId, long? ownerId, bool ignoreAudit, long? categoryId, string tagName, bool? isEssential, SortBy_WikiPage sortBy = SortBy_WikiPage.DateCreated_Desc, int pageSize = 20, int pageIndex = 1)
        {
            var sql = Sql.Builder.Select("spb_WikiPages.*").From("spb_WikiPages");
            var whereSql = Sql.Builder.Where("spb_WikiPages.IsLogicalDelete=0");
            var orderSql = Sql.Builder;

            if (!string.IsNullOrEmpty(tenantTypeId))
            {
                whereSql.Where("spb_WikiPages.TenantTypeId=@0", tenantTypeId);
            }
            if (!string.IsNullOrEmpty(keyWord))
            {
                whereSql.Where("spb_WikiPages.Title like @0", "%" + keyWord + "%");
            }
            if (ownerId.HasValue)
            {
                whereSql.Where("spb_WikiPages.OwnerId=@0", ownerId.Value);
            }

            if (categoryId.HasValue && categoryId > 0)
            {
                sql.InnerJoin("tn_ItemsInCategories").On("spb_WikiPages.PageId=tn_ItemsInCategories.ItemId");
                whereSql.Where("tn_ItemsInCategories.CategoryId=@0", categoryId.Value);
            }

            if (!string.IsNullOrEmpty(tagName))
            {
                whereSql.Where("spb_WikiPages.PageId in ( select ItemId from tn_ItemsInTags where tn_ItemsInTags.TenantTypeId = @0 and tn_ItemsInTags.TagName=@1 )", TenantTypeIds.Instance().WikiPage(), tagName);
            }

            if (!ignoreAudit)
            {
                switch (this.PubliclyAuditStatus)
                {
                    case PubliclyAuditStatus.Again:
                    case PubliclyAuditStatus.Fail:
                    case PubliclyAuditStatus.Pending:
                    case PubliclyAuditStatus.Success:
                        whereSql.Where("spb_WikiPages.AuditStatus=@0", this.PubliclyAuditStatus);
                        break;
                    case PubliclyAuditStatus.Again_GreaterThanOrEqual:
                    case PubliclyAuditStatus.Pending_GreaterThanOrEqual:
                        whereSql.Where("spb_WikiPages.AuditStatus>@0", this.PubliclyAuditStatus);
                        break;
                    default:
                        break;
                }
            }
            if (isEssential.HasValue)
            {
                whereSql.Where("spb_WikiPages.IsEssential=@0", isEssential.Value);
            }

            CountService countService = new CountService(TenantTypeIds.Instance().WikiPage());
            string countTableName = countService.GetTableName_Counts();
            switch (sortBy)
            {
                case SortBy_WikiPage.DateCreated_Desc:
                    orderSql.OrderBy("spb_WikiPages.PageId desc");
                    break;
                case SortBy_WikiPage.StageHitTimes:
                    StageCountTypeManager stageCountTypeManager = StageCountTypeManager.Instance(TenantTypeIds.Instance().WikiPage());
                    int stageCountDays = stageCountTypeManager.GetMaxDayCount(CountTypes.Instance().HitTimes());
                    string stageCountType = stageCountTypeManager.GetStageCountType(CountTypes.Instance().HitTimes(), stageCountDays);
                    sql.LeftJoin(string.Format("(select * from {0} WHERE ({0}.CountType = '{1}')) c", countTableName, stageCountType))
                    .On("PageId = c.ObjectId");
                    orderSql.OrderBy("c.StatisticsCount desc");
                    break;
                default:
                    orderSql.OrderBy("spb_WikiPages.PageId desc");
                    break;
            }

            sql.Append(whereSql).Append(orderSql);
            return GetPagingEntities(pageSize, pageIndex, sql);
        }

    }
}